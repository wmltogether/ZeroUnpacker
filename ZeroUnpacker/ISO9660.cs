using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ZeroUnpacker
{
    class ISO9660
    {
        /*
        ISO 9660 + UDF
        [需要修改]7086L long (LE) ISO大小 = value*0x800 + 0x800
        7886L long (LE) ISO大小 = value*0x800 + 0x800,重复一次
        [需要修改]8000L 01 43 44 30 30 31 01 00 .CD001. PLAYSTATION起始标志
        [需要修改]8050L long (LE + BE 共8字节) ISO大小= value * 0x800
        809EL long (LE + BE 共8字节)文件LBA索引区域地址 index_offset = value * 0x800

        [需要修改]110C0L  ISO大小 = value*0x800 + 0x83000
        [需要修改]190C0L  ISO大小 = value*0x800 + 0x83000
        [需要修改]20054L  ISO大小 = value*0x800 + 0x83000
        20078L long 文件数
        Seek(index_offset,0)//Path Tables
        {   
            文件数
            {
                    ???
            }
            
        }



        */
        public static ArrayList LBAReader(string path)
        {
            ArrayList arrayListLBA = null;
            FileStream fs = null;
            fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            arrayListLBA = new ArrayList();
            //int blockSize = 0;
            //string str = "";
            Dictionary<int, string> dNumDir = new Dictionary<int, string>();
            
            arrayListLBA.Sort();
            return arrayListLBA;

        }
    }
}
