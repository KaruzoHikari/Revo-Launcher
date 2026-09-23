using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Misc.SFO
{
    
    // From https://github.com/LiEnby/Sfo.NET
    // Thanks!
    class DataUtils
    {
        public static void CopyString(byte[] str, String Text, int Index)
        {
            byte[] TextBytes = Encoding.UTF8.GetBytes(Text);
            Array.ConstrainedCopy(TextBytes, 0, str, Index, TextBytes.Length);
        }

        public static void CopyInt32(byte[] str, Int32 Value, int Index)
        {
            byte[] ValueBytes = BitConverter.GetBytes(Value);
            Array.ConstrainedCopy(ValueBytes, 0, str, Index, ValueBytes.Length);
        }

        public static void CopyInt32BE(byte[] str, Int32 Value, int Index)
        {
            byte[] ValueBytes = BitConverter.GetBytes(Value);
            byte[] ValueBytesBE = ValueBytes.Reverse().ToArray();
            Array.ConstrainedCopy(ValueBytesBE, 0, str, Index, ValueBytesBE.Length);
        }

        // Read From Streams
        public static UInt32 ReadUInt32(Stream Str)
        {
            byte[] IntBytes = new byte[0x4];
            Str.Read(IntBytes, 0x00, IntBytes.Length);
            return BitConverter.ToUInt32(IntBytes, 0x00);
        }

        public static UInt32 ReadInt32(Stream Str)
        {
            byte[] IntBytes = new byte[0x4];
            Str.Read(IntBytes, 0x00, IntBytes.Length);
            return BitConverter.ToUInt32(IntBytes, 0x00);
        }

        public static UInt64 ReadUInt64(Stream Str)
        {
            byte[] IntBytes = new byte[0x8];
            Str.Read(IntBytes, 0x00, IntBytes.Length);
            return BitConverter.ToUInt64(IntBytes, 0x00);
        }

        public static Int64 ReadInt64(Stream Str)
        {
            byte[] IntBytes = new byte[0x8];
            Str.Read(IntBytes, 0x00, IntBytes.Length);
            return BitConverter.ToInt64(IntBytes, 0x00);
        }

        public static UInt16 ReadUInt16(Stream Str)
        {
            byte[] IntBytes = new byte[0x2];
            Str.Read(IntBytes, 0x00, IntBytes.Length);
            return BitConverter.ToUInt16(IntBytes, 0x00);
        }

        public static Int16 ReadInt16(Stream Str)
        {
            byte[] IntBytes = new byte[0x2];
            Str.Read(IntBytes, 0x00, IntBytes.Length);
            return BitConverter.ToInt16(IntBytes, 0x00);
        }

        public static UInt32 ReadUint32At(Stream Str, int location)
        {
            long oldPos = Str.Position;
            Str.Seek(location, SeekOrigin.Begin);
            UInt32 outp = ReadUInt32(Str);
            Str.Seek(oldPos, SeekOrigin.Begin);
            return outp;
        }

        public static byte[] ReadBytesAt(Stream Str, int location, int length)
        {
            long oldPos = Str.Position;
            Str.Seek(location, SeekOrigin.Begin);
            byte[] work_buf = new byte[length];
            Str.Read(work_buf, 0x0, work_buf.Length);
            Str.Seek(oldPos, SeekOrigin.Begin);
            return work_buf;
        }

        public static string ReadStringAt(Stream Str, int location)
        {
            long oldPos = Str.Position;
            Str.Seek(location, SeekOrigin.Begin);
            string outp = ReadString(Str);
            Str.Seek(oldPos, SeekOrigin.Begin);
            return outp;
        }

        public static string ReadString(Stream Str)
        {
            MemoryStream ms = new MemoryStream();

            while (true)
            {
                byte c = (byte)Str.ReadByte();
                if (c == 0)
                    break;
                ms.WriteByte(c);
            }

            ms.Seek(0x00, SeekOrigin.Begin);
            string outp = Encoding.UTF8.GetString(ms.ToArray());
            ms.Dispose();
            return outp;
        }

        // Write To Streams

        public static void WriteUInt32(Stream Str, UInt32 Numb)
        {
            byte[] IntBytes = BitConverter.GetBytes(Numb);
            Str.Write(IntBytes, 0x00, IntBytes.Length);
        }

        public static void WriteInt32(Stream Str, Int32 Numb)
        {
            byte[] IntBytes = BitConverter.GetBytes(Numb);
            Str.Write(IntBytes, 0x00, IntBytes.Length);
        }

        public static void WriteUInt64(Stream dst, UInt64 value)
        {
            byte[] ValueBytes = BitConverter.GetBytes(value);
            dst.Write(ValueBytes, 0x00, 0x8);
        }

        public static void WriteInt64(Stream dst, Int64 value)
        {
            byte[] ValueBytes = BitConverter.GetBytes(value);
            dst.Write(ValueBytes, 0x00, 0x8);
        }

        public static void WriteUInt16(Stream dst, UInt16 value)
        {
            byte[] ValueBytes = BitConverter.GetBytes(value);
            dst.Write(ValueBytes, 0x00, 0x2);
        }

        public static void WriteInt16(Stream dst, Int16 value)
        {
            byte[] ValueBytes = BitConverter.GetBytes(value);
            dst.Write(ValueBytes, 0x00, 0x2);
        }

        public static void WriteInt32BE(Stream Str, Int32 Numb)
        {
            byte[] IntBytes = BitConverter.GetBytes(Numb);
            byte[] IntBytesBE = IntBytes.Reverse().ToArray();
            Str.Write(IntBytesBE, 0x00, IntBytesBE.Length);
        }

        public static void WriteString(Stream Str, String Text, int len = -1)
        {
            if (len < 0)
            {
                len = Text.Length;
            }

            byte[] TextBytes = Encoding.UTF8.GetBytes(Text);
            Str.Write(TextBytes, 0x00, TextBytes.Length);
        }

    }
}