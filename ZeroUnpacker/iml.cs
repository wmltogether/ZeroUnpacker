using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using sio = System.IO;

namespace iml2iso
{
    public class imlClass
    {
        private class imlItem
        {
            public long start, length;
            public string file;

            public imlItem(string str)
            {
                int i;
                string org = str;

                while ((i = str.LastIndexOf('\t') + 1) > 0)
                    str = str.Replace('\t', ' ');
                while ((i = str.LastIndexOf("  ") + 1) > 0)
                    str = str.Replace("  ", " ");

                i = str.LastIndexOf(' ') + 1;
                file = str.Substring(i, str.Length - i);
                if (file.IndexOf('\"') > -1)
                    file = file.Substring(1, file.Length - 2);

                i = str.IndexOf(' ') + 1;
                if (i > 0)
                    start = Convert.ToInt64(str.Substring(0, i)) << 0xb;
                else
                    throw new ArgumentException(string.Format("Error parsing line '{0}'", org));
                length = str.IndexOf(' ', i) + 1;
                if (i > 0)
                    length = ((Convert.ToInt64(str.Substring(i, (int)length - i)) + 1) << 0xb) - start;
                else
                    throw new ArgumentException(string.Format("Error parsing line '{0}'", org));
            }
        }

        private List<imlItem> items;
        string imlPath;
        string isoPath;

        public imlClass(string imlPath, string isoPath)
        {
            sio.FileInfo fi;

            fi = new sio.FileInfo(imlPath);
            if (!fi.Exists)
                throw new sio.FileNotFoundException(string.Format("File '{0}' not found", fi.FullName));

            this.imlPath = imlPath;
            this.isoPath = isoPath;
            LoadIml();
        }

        private void LoadIml()
        {
            sio.StreamReader sr;
            string str;

            items = new List<imlItem>();
            sr = new sio.StreamReader(imlPath);

            while (sr.ReadLine().ToLower() != "[loc]") ;

            str = sr.ReadLine().ToLower();
            while (str != "[/loc]")
            {
                if (str.Length > 0)
                    if (!str.StartsWith("#"))
                        items.Add(new imlItem(str));
                str = sr.ReadLine().ToLower();
            }

            sr.Close();

            imlPath = new sio.FileInfo(imlPath).Directory.FullName + sio.Path.DirectorySeparatorChar;
        }

        public void Rebuild()
        {
            sio.FileStream fs;
            sio.FileInfo fi;

            foreach (var item in items)
            {
                fi = new sio.FileInfo(imlPath + item.file);
                if (!fi.Exists)
                    throw new sio.FileNotFoundException(string.Format("File '{0}' not found", fi.FullName));
                if (fi.Length > item.length)
                    throw new ArgumentException(string.Format("File '{0}' is bigger than defined in iml", fi.FullName));
            }

            fs = new sio.FileStream(isoPath, sio.FileMode.Create, sio.FileAccess.Write);
            Console.WriteLine("Rebuilding image '{0}'", isoPath);
            foreach (var item in items)
            {
                fs.Position = item.start;
                Console.WriteLine("Importing file '{0}'", item.file);
                fs.Insert(imlPath + item.file, 0);
            }
            fs.Close();

            Console.WriteLine("Done");
        }
    }

    public static class ext
    {
        public static void Insert(this sio.FileStream fs, string path, int align)
        {
            const int bufLen = 0x1000;
            byte[] buf;
            int bufCnt;
            long remSize;
            sio.FileStream str;

            str = new sio.FileStream(path, sio.FileMode.Open, sio.FileAccess.Read);
            buf = new byte[bufLen];
            bufCnt = bufLen;
            remSize = str.Length;

            while (remSize > 0)
            {
                if (remSize < bufLen)
                    bufCnt = (int)remSize;
                str.Read(buf, 0, bufCnt);
                fs.Write(buf, 0, bufCnt);
                remSize -= bufCnt;
            }

            if (align > 0)
            {
                remSize = fs.Position & (align - 1);
                if (remSize > 0)
                    remSize = align - remSize;
                fs.Write(new byte[remSize], 0, (int)remSize);
            }
            str.Close();
        }
    }
}
