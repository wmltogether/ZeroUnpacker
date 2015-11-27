using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using System.Threading;

namespace ZeroUnpacker
{
    public static class Config
    {
        public static bool bUseExternalDeLess;
        public static bool bUseLIMGPatch;

    }
    struct fileInfo
    {
        public string baseName;
        public long baseSize;
        public int baseNums;
        public long BLOCK0_OFFSET;
        public long BLOCK1_OFFSET;
        public long BLOCK2_OFFSET;
        public long BLOCK3_OFFSET;
        public long BLOCK4_OFFSET;
    }
    struct FHDInfo
    {
        public string FolderName;
        public string FileName;
        public int FHD_ID;
        public long LBA_OFFSET;
        public bool Compress_Mark;
        public long CompressedSize;
        public long DecompressedSize;

    }
    class FatalFrame
    {
        public static void FHD2INI(string FHD_name)
        {

            fileInfo m_fileInfo;
            FHDInfo m_FHDInfo;
            string directoryName = Path.GetDirectoryName(FHD_name);
            FileStream fStream = new FileStream(FHD_name, FileMode.Open, FileAccess.Read);//打开FHD文件流
            BinaryReader fp = new BinaryReader(fStream);

            string IMG_NAME = String.Format("{0}\\IMG_BD.bin", directoryName);//打开IMG_BD文件流
            long img_size = new FileInfo(IMG_NAME).Length;

            FileStream ini = new FileStream(String.Format("{0}\\zero.ini", directoryName), FileMode.OpenOrCreate, FileAccess.Write);
            ini.Seek(0, SeekOrigin.Begin);
            ini.SetLength(0);
            ini.Seek(0, SeekOrigin.Begin);
            StreamWriter inifile = new StreamWriter(ini);

            fp.BaseStream.Seek(0, SeekOrigin.Begin);
            uint sig = fp.ReadUInt32();
            if (sig != 0x46484400)
            {
                MessageBox.Show("Error: Not a FHD file");
                fp.Close();
                fStream.Close();
                return;

            }
            else
            {
                fp.BaseStream.Seek(8, SeekOrigin.Begin);
                m_fileInfo.baseName = FHD_name;
                m_fileInfo.baseSize = fp.ReadUInt32();
                m_fileInfo.baseNums = fp.ReadInt32();
                m_fileInfo.BLOCK0_OFFSET = fp.ReadUInt32();// Decompressed BLOCK
                m_fileInfo.BLOCK1_OFFSET = fp.ReadUInt32();// TYPE BLOCK
                m_fileInfo.BLOCK2_OFFSET = fp.ReadUInt32();// SIZE BLOCK
                m_fileInfo.BLOCK3_OFFSET = fp.ReadUInt32();// NAME BLOCK
                m_fileInfo.BLOCK4_OFFSET = fp.ReadUInt32();// LBA BLOCK
                fp.BaseStream.Seek(m_fileInfo.BLOCK0_OFFSET, SeekOrigin.Begin);// goto Block 0 
                for (int i = 0; i < m_fileInfo.baseNums; i++)
                {

                    m_FHDInfo.FHD_ID = i;
                    long LBA_POS = m_fileInfo.BLOCK4_OFFSET + i * 4;
                    long SIZE_POS = m_fileInfo.BLOCK2_OFFSET + i * 4;
                    long NAME_POS = m_fileInfo.BLOCK3_OFFSET + i * 8;
                    long DEC_POS = m_fileInfo.BLOCK0_OFFSET + i * 4;
                    fp.BaseStream.Seek(NAME_POS, SeekOrigin.Begin);
                    long destfolderNameOffset = fp.ReadUInt32();//获得文件夹名称地址
                    long destfileNameOffset = fp.ReadUInt32();//获得子文件名称地址
                    fp.BaseStream.Seek(destfolderNameOffset, SeekOrigin.Begin);
                    string destfolderName = Encoding.ASCII.GetString(fp.ReadBytes(64)).Split('\0')[0];//获得文件夹名称
                    fp.BaseStream.Seek(destfileNameOffset, SeekOrigin.Begin);
                    string destfileName = Encoding.ASCII.GetString(fp.ReadBytes(64)).Split('\0')[0];//获得文件名

                    fp.BaseStream.Seek(LBA_POS, SeekOrigin.Begin);
                    m_FHDInfo.LBA_OFFSET = fp.ReadUInt32() * 0x800;//获取LBA地址
                    fp.BaseStream.Seek(DEC_POS, SeekOrigin.Begin);
                    m_FHDInfo.DecompressedSize = fp.ReadUInt32();

                    fp.BaseStream.Seek(SIZE_POS, SeekOrigin.Begin);
                    m_FHDInfo.CompressedSize = fp.ReadUInt32();
                    m_FHDInfo.Compress_Mark = false;
                    if ((m_FHDInfo.CompressedSize & 1) == 1)
                    {
                        //File was Compressed
                        m_FHDInfo.Compress_Mark = true; //压缩标志
                    }
                    m_FHDInfo.CompressedSize = m_FHDInfo.CompressedSize >> 1;//获取压缩文件长度

                    /*Console.WriteLine(String.Format("{0} , {1},{2},{3} , {4},{5}", Convert.ToString(m_FHDInfo.LBA_OFFSET, 16),
                                                                           Convert.ToString(m_FHDInfo.CompressedSize, 16),
                                                                           Convert.ToString(m_FHDInfo.DecompressedSize, 16),
                                                                           Convert.ToString(m_FHDInfo.Compress_Mark),
                                                                           destfolderName,
                                                                           destfileName));
                    */
                    if (0 <= m_FHDInfo.LBA_OFFSET && m_FHDInfo.LBA_OFFSET < img_size)
                    {
                        inifile.WriteLine(String.Format("{6}\t{0}\t{1}\t{2}\t{3}\t{4}\t{5}", Convert.ToString(m_FHDInfo.LBA_OFFSET, 16),
                                                                           Convert.ToString(m_FHDInfo.CompressedSize, 16),
                                                                           Convert.ToString(m_FHDInfo.DecompressedSize, 16),
                                                                           Convert.ToString(m_FHDInfo.Compress_Mark),
                                                                           destfolderName,
                                                                           destfileName,
                                                                           Convert.ToString(m_FHDInfo.FHD_ID, 16)));

                    }
                }
            }



            fp.Close();
            fStream.Close();
            inifile.Close();
            ini.Close();
        }
        public static void GetFHDInfo(string FHD_name)

        {
            
            fileInfo m_fileInfo;
            FHDInfo m_FHDInfo;
            string directoryName = Path.GetDirectoryName(FHD_name);
            FileStream fStream = new FileStream(FHD_name, FileMode.Open, FileAccess.Read);//打开FHD文件流
            BinaryReader fp = new BinaryReader(fStream);

            string IMG_NAME = String.Format("{0}\\IMG_BD.bin", directoryName);//打开IMG_BD文件流
            FileStream IMG_Stream = new FileStream(IMG_NAME, FileMode.Open, FileAccess.Read);
            BinaryReader img = new BinaryReader(IMG_Stream);
            long img_size = new FileInfo(IMG_NAME).Length;

            FileStream ini = new FileStream(String.Format("{0}\\zero.ini", directoryName), FileMode.Create, FileAccess.Write);
            StreamWriter inifile = new StreamWriter(ini);


            fp.BaseStream.Seek(0, SeekOrigin.Begin);
            uint sig = fp.ReadUInt32();
            if (sig != 0x46484400)
            {
                MessageBox.Show("Error: Not a FHD file");
                fp.Close();
                fStream.Close();
                return;

            }
            else
            {
                fp.BaseStream.Seek(8, SeekOrigin.Begin);
                m_fileInfo.baseName = FHD_name;
                m_fileInfo.baseSize = fp.ReadUInt32();
                m_fileInfo.baseNums = fp.ReadInt32();
                m_fileInfo.BLOCK0_OFFSET = fp.ReadUInt32();// Decompressed BLOCK
                m_fileInfo.BLOCK1_OFFSET = fp.ReadUInt32();// TYPE BLOCK
                m_fileInfo.BLOCK2_OFFSET = fp.ReadUInt32();// SIZE BLOCK
                m_fileInfo.BLOCK3_OFFSET = fp.ReadUInt32();// NAME BLOCK
                m_fileInfo.BLOCK4_OFFSET = fp.ReadUInt32();// LBA BLOCK
                fp.BaseStream.Seek(m_fileInfo.BLOCK0_OFFSET, SeekOrigin.Begin);// goto Block 0 
                for (int i = 0; i < m_fileInfo.baseNums; i++)
                {
                    
                    m_FHDInfo.FHD_ID = i;
                    long LBA_POS = m_fileInfo.BLOCK4_OFFSET + i * 4;
                    long SIZE_POS = m_fileInfo.BLOCK2_OFFSET + i * 4;
                    long NAME_POS = m_fileInfo.BLOCK3_OFFSET + i * 8;
                    long DEC_POS = m_fileInfo.BLOCK0_OFFSET + i * 4;
                    fp.BaseStream.Seek(NAME_POS, SeekOrigin.Begin);
                    long destfolderNameOffset = fp.ReadUInt32();//获得文件夹名称地址
                    long destfileNameOffset = fp.ReadUInt32();//获得子文件名称地址
                    fp.BaseStream.Seek(destfolderNameOffset, SeekOrigin.Begin);
                    string destfolderName = Encoding.ASCII.GetString(fp.ReadBytes(64)).Split('\0')[0];//获得文件夹名称
                    fp.BaseStream.Seek(destfileNameOffset, SeekOrigin.Begin);
                    string destfileName = Encoding.ASCII.GetString(fp.ReadBytes(64)).Split('\0')[0];//获得文件名
                    
                    fp.BaseStream.Seek(LBA_POS, SeekOrigin.Begin);
                    m_FHDInfo.LBA_OFFSET = fp.ReadUInt32() * 0x800;//获取LBA地址
                    fp.BaseStream.Seek(DEC_POS, SeekOrigin.Begin);
                    m_FHDInfo.DecompressedSize = fp.ReadUInt32();

                    fp.BaseStream.Seek(SIZE_POS, SeekOrigin.Begin);
                    m_FHDInfo.CompressedSize = fp.ReadUInt32();           
                    m_FHDInfo.Compress_Mark = false;
                    if ((m_FHDInfo.CompressedSize & 1) == 1) {
                        //File was Compressed
                        m_FHDInfo.Compress_Mark = true; //压缩标志
                    }
                    m_FHDInfo.CompressedSize = m_FHDInfo.CompressedSize >> 1;//获取压缩文件长度

                    /*Console.WriteLine(String.Format("{0} , {1},{2},{3} , {4},{5}", Convert.ToString(m_FHDInfo.LBA_OFFSET, 16),
                                                                           Convert.ToString(m_FHDInfo.CompressedSize, 16),
                                                                           Convert.ToString(m_FHDInfo.DecompressedSize, 16),
                                                                           Convert.ToString(m_FHDInfo.Compress_Mark),
                                                                           destfolderName,
                                                                           destfileName));
                    */
                    if (0 <= m_FHDInfo.LBA_OFFSET && m_FHDInfo.LBA_OFFSET < img_size)
                    {
                        inifile.WriteLine(String.Format("{6}\t{0}\t{1}\t{2}\t{3}\t{4}\t{5}", Convert.ToString(m_FHDInfo.LBA_OFFSET, 16),
                                                                           Convert.ToString(m_FHDInfo.CompressedSize, 16),
                                                                           Convert.ToString(m_FHDInfo.DecompressedSize, 16),
                                                                           Convert.ToString(m_FHDInfo.Compress_Mark),
                                                                           destfolderName,
                                                                           destfileName,
                                                                           Convert.ToString(m_FHDInfo.FHD_ID, 16)));
                        img.BaseStream.Seek(m_FHDInfo.LBA_OFFSET, SeekOrigin.Begin);
                        byte[] imgData = img.ReadBytes(Convert.ToInt32(m_FHDInfo.CompressedSize));

                        string destName = String.Format("{0}\\{1}{2}", directoryName, destfolderName.Replace("..", ""), destfileName);
                        if (!File.Exists(destName))
                        {
                            Directory.CreateDirectory(String.Format("{0}\\{1}", directoryName, destfolderName.Replace("..", "")));
                        }

                        FileStream dest = new FileStream(destName, FileMode.Create, FileAccess.Write);
                        BinaryWriter bw = new BinaryWriter(dest);
                        bw.Write(imgData);
                        bw.Close();
                        dest.Close();
                        if (m_FHDInfo.Compress_Mark == true)
                        {
                            if (Config.bUseExternalDeLess == true)
                            {
                                string _command = String.Format(" {0}\\{1}{2}", directoryName, destfolderName.Replace("..", ""), destfileName);
                                string path = string.Format(@"{0}", System.IO.Directory.GetCurrentDirectory());
                                System.Diagnostics.Process process = new System.Diagnostics.Process();
                                process.StartInfo.FileName = string.Format("{0}\\DeLESS.exe", path);
                                process.StartInfo.WorkingDirectory = path;
                                process.StartInfo.UseShellExecute = false;
                                process.StartInfo.CreateNoWindow = true;
                                process.StartInfo.Arguments = _command;
                                process.Start();
                                process.WaitForExit();
                                
                                string dname = String.Format(" {0}\\{1}{2}", directoryName, destfolderName.Replace("..", ""), destfileName);
                                string tmpname = String.Format(" {0}\\{1}{2}.LED", directoryName, destfolderName.Replace("..", ""), destfileName);
                                File.Delete(dname);
                                File.Move(tmpname, dname);

                            }
                        }

                    }
                }
            }


            inifile.Close();
            ini.Close();
            img.Close();
            IMG_Stream.Close();
            fp.Close();
            fStream.Close();
        }

    }
}
