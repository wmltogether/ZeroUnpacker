using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Collections;

namespace ZeroUnpacker
{

    class Patcher
    {
        public static void GetIndexInfo(string ini_name, List<FHDInfo> list)
        {
            FHDInfo m_FHDInfo;
            FileStream ini = new FileStream(ini_name, FileMode.Open, FileAccess.Read);
            StreamReader inifile = new StreamReader(ini);
            string[] lines = inifile.ReadToEnd().Split(new string[] { "\r\n" }, StringSplitOptions.None);
            //List<FHDInfo> list = new List<FHDInfo>();
            Base.WriteLogging(String.Format("GetIndexInfo:{0}", ini_name));
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (line.Length > 8)
                {
                    m_FHDInfo.FHD_ID = Convert.ToInt32(line.Split(new string[] { "\t" }, StringSplitOptions.None)[0], 16);
                    m_FHDInfo.LBA_OFFSET = Convert.ToUInt32(line.Split(new string[] { "\t" }, StringSplitOptions.None)[1], 16);
                    m_FHDInfo.CompressedSize = Convert.ToUInt32(line.Split(new string[] { "\t" }, StringSplitOptions.None)[2], 16);
                    m_FHDInfo.DecompressedSize = Convert.ToUInt32(line.Split(new string[] { "\t" }, StringSplitOptions.None)[3], 16);
                    m_FHDInfo.Compress_Mark = Convert.ToBoolean(line.Split(new string[] { "\t" }, StringSplitOptions.None)[4]);
                    m_FHDInfo.FolderName = line.Split(new string[] { "\t" }, StringSplitOptions.None)[5];
                    m_FHDInfo.FileName = line.Split(new string[] { "\t" }, StringSplitOptions.None)[6];
                    string pfName = String.Format("{0}\\Patch\\{1}{2}", Path.GetDirectoryName(ini_name),
                                                                    m_FHDInfo.FolderName.Replace("..", ""),
                                                                    m_FHDInfo.FileName);
                    if (File.Exists(pfName))
                    {
                        Base.WriteLogging(String.Format("File Found in patch folder:{0}", pfName));
                        // File Found in patch folder
                        FileInfo pfInfo = new FileInfo(pfName);
                        int pfSize = Convert.ToInt32(pfInfo.Length);
                        if (pfSize > m_FHDInfo.CompressedSize)
                        {
                            m_FHDInfo.CompressedSize = pfSize;
                            m_FHDInfo.DecompressedSize = pfSize;
                            m_FHDInfo.LBA_OFFSET = -1;// Seek to end 
                            list.Add(m_FHDInfo);
                        }
                        else
                        {
                            m_FHDInfo.CompressedSize = pfSize;
                            m_FHDInfo.DecompressedSize = pfSize;
                            list.Add(m_FHDInfo);
                        }
                    }

                }

            }
            inifile.Close();
            ini.Close();
        }

        public static bool PatchIMG(string FHD_name)
        {
            string directoryName = Path.GetDirectoryName(FHD_name);//获取FHD文件目录
            string patchFolderName = String.Format("{0}\\Patch", directoryName);

            if (!File.Exists(patchFolderName))
            //如果Patch文件夹不存在，创建Patch文件夹
            {
                Directory.CreateDirectory(String.Format("{0}\\Patch", directoryName));
            }
            FatalFrame.FHD2INI(FHD_name);//从当前选择的FHD文件导出ini表
            List<FHDInfo> list = new List<FHDInfo>();
            GetIndexInfo(String.Format("{0}\\zero.ini", directoryName), list);//获取struct 列表

            fileInfo m_fileInfo;
            FileStream fStream = new FileStream(FHD_name, FileMode.Open, FileAccess.ReadWrite);//打开FHD文件流
            BinaryReader fp = new BinaryReader(fStream);
            BinaryWriter fhd_writer = new BinaryWriter(fStream);
            fp.BaseStream.Seek(0, SeekOrigin.Begin);
            fhd_writer.BaseStream.Seek(0, SeekOrigin.Begin);

            string IMG_NAME = String.Format("{0}\\IMG_BD.bin", directoryName);//打开IMG_BD文件流
            FileStream IMG_Stream = new FileStream(IMG_NAME, FileMode.Open, FileAccess.ReadWrite);
            BinaryWriter img = new BinaryWriter(IMG_Stream);


            uint sig = fp.ReadUInt32();
            if (sig != 0x46484400)
            {
                Base.WriteLogging(String.Format("Error: Not a FHD file:{0}", FHD_name));
                MessageBox.Show("Error: Not a FHD file");
                fp.Close();
                fStream.Close();
                return false;

            }
            else
            {
                fp.BaseStream.Seek(8, SeekOrigin.Begin);
                m_fileInfo.baseName = FHD_name;
                m_fileInfo.baseSize = fp.ReadInt32();
                m_fileInfo.baseNums = fp.ReadInt32();
                m_fileInfo.BLOCK0_OFFSET = fp.ReadInt32();// Decompressed BLOCK
                m_fileInfo.BLOCK1_OFFSET = fp.ReadInt32();// TYPE BLOCK
                m_fileInfo.BLOCK2_OFFSET = fp.ReadInt32();// SIZE BLOCK
                m_fileInfo.BLOCK3_OFFSET = fp.ReadInt32();// NAME BLOCK
                m_fileInfo.BLOCK4_OFFSET = fp.ReadInt32();// LBA BLOCK
                fp.BaseStream.Seek(m_fileInfo.BLOCK0_OFFSET, SeekOrigin.Begin);// goto Block 0 
                Base.WriteLogging(String.Format("{0} files need to patch", list.Count));
                for (int i = 0; i < list.Count; i++)
                {
                    FHDInfo m_FHDInfo = list[i];
                    //根据当前ID获取FHD的指针地址
                    long LBA_POS = m_fileInfo.BLOCK4_OFFSET + m_FHDInfo.FHD_ID * 4;
                    long SIZE_POS = m_fileInfo.BLOCK2_OFFSET + m_FHDInfo.FHD_ID * 4;
                    long NAME_POS = m_fileInfo.BLOCK3_OFFSET + m_FHDInfo.FHD_ID * 8;
                    long DEC_POS = m_fileInfo.BLOCK0_OFFSET + m_FHDInfo.FHD_ID * 4;
                    string pfName = String.Format("{0}\\Patch\\{1}{2}", directoryName,
                                                                    m_FHDInfo.FolderName.Replace("..", ""),
                                                                    m_FHDInfo.FileName);
                    FileStream pfStream = new FileStream(pfName, FileMode.Open, FileAccess.ReadWrite);//打开patch file文件流
                    BinaryReader pfReader = new BinaryReader(pfStream);
                    pfReader.BaseStream.Seek(0, SeekOrigin.Begin);
                    if (m_FHDInfo.LBA_OFFSET == -1)//文件追加到IMG_BD末尾
                    {

                        img.BaseStream.Seek(0, SeekOrigin.End);
                        long currentPosition = img.BaseStream.Position;
                        long tmp_offset = Convert.ToUInt32(currentPosition);
                        m_FHDInfo.LBA_OFFSET = tmp_offset;
                        Base.WriteLogging(String.Format("Writing {0} to the end of img --->{1}", pfName, m_FHDInfo.LBA_OFFSET));
                        byte[] data = pfReader.ReadBytes(Convert.ToInt32(pfStream.Length));
                        img.Write(data);
                        if (Convert.ToInt32(pfStream.Length) % 0x800 != 0)
                        {
                            img.Write(Base.FillByteArray(0x800 - Convert.ToInt32(pfStream.Length) % 0x800));

                        }
                        img.Flush();

                    }
                    else
                    {
                        long tmp_offset = m_FHDInfo.LBA_OFFSET;
                        img.BaseStream.Seek(tmp_offset, SeekOrigin.Begin);
                        
                        byte[] data = pfReader.ReadBytes(Convert.ToInt32(pfStream.Length));
                        img.Write(data);
                        img.Flush();
                    }
                    fStream.Position = LBA_POS;
                    fStream.Write(System.BitConverter.GetBytes(m_FHDInfo.LBA_OFFSET / 0x800), 0, 4);
                    fStream.Position = SIZE_POS;
                    fStream.Write(System.BitConverter.GetBytes(m_FHDInfo.CompressedSize << 1), 0, 4);
                    fStream.Position = DEC_POS;
                    fStream.Write(System.BitConverter.GetBytes(m_FHDInfo.DecompressedSize), 0, 4);
                    pfReader.Close();
                    pfStream.Close();





                }
                fp.Close();
                fStream.Close();
                IMG_Stream.Close();
                img.Close();




                return true;
            }
        }
        public static bool PatchISO(string iso_name)
        {
            ArrayList arrayListLBA = null;
            arrayListLBA = new ArrayList();
            arrayListLBA = ISO9660.LBAReader(iso_name);
            return true;
        }

    }

}
