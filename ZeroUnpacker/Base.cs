using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ZeroUnpacker
{
    class Base
    {
        public static void WriteLogging(string message)
        {
            CodeNode.Logging.Logger.Instance.Info(message);
        }

        public static byte[] FillByteArray(int count)
        {
            byte[] c = new byte[count];
            for (int i=0; i < count; i++)
            {
                c[i] = 0;
            }
            return c;
        }
        public static void WriteBigFile(BinaryReader inputBuffer , BinaryWriter outputBuffer ,long currentPosition)
        {
            long input_size = inputBuffer.BaseStream.Length;

        }

    }
}
