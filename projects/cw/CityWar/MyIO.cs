//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.IO;

//namespace CityWar
//{

//      class MyWriter
//    {
//          static int GetBits(int max)
//        {
//            return (int)Math.Ceiling(Math.Log(max + 1, 2));
//        }

//        BinaryWriter w;

//          MyWriter(Stream s)
//        {
//            w = new BinaryWriter(s);
//        }

//          void WriteSingle(float value)
//        {
//            w.Write(value);
//        }

//          void WriteString(string value)
//        {
//            w.Write(value);
//        }

//          void WriteBoolean(bool value)
//        {
//            w.Write(value);
//        }

//        readonly int[] writeables = new int[] { 64, 32, 16, 8, 1 };
//          void Write(int bits, long value)
//        {
//            if (bits > 64 || bits < 1)
//                throw new ArgumentOutOfRangeException();

//            while (bits > 0)
//            {
//                for (int i = 0; i < 5; ++i)
//                    if (bits == writeables[i])
//                    {
//                        writePart(bits, value);
//                        return;
//                    }

//                for (int i = 1; i < 5; ++i)
//                    if (bits > writeables[i])
//                    {
//                        int extra = bits - writeables[i];
//                        writePart(writeables[i], value >> extra);
//                        Write(extra, value);
//                        return;
//                    }
//            }
//        }

//        private void writePart(int bits, long value)
//        {
//            if (bits == 1)
//                w.Write((value & 1) == 1);
//            else if (bits == 8)
//                w.Write((byte)value);
//            else if (bits == 16)
//                w.Write((short)value);
//            else if (bits == 32)
//                w.Write((int)value);
//            else if (bits == 64)
//                w.Write(value);
//        }
//    }

//      class MyReader
//    {
//        BinaryReader r;

//          MyReader(Stream s)
//        {
//            r = new BinaryReader(s);
//        }

//          float ReadSingle()
//        {
//            return r.ReadSingle();
//        }

//          string ReadString()
//        {
//            return r.ReadString();
//        }

//          bool ReadBoolean()
//        {
//            return r.ReadBoolean();
//        }

//          long Read(int bits)
//        {
//        }
//    }
//}
