using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace System.IO.Binary
{

    public class DataField
    {
        public long Offset { get; internal set; }
        public ulong Length { get; internal set; }

        public Type Type { get; internal set; }


        public DataField(long offset)
        {
            this.Offset = offset;
            this.Length = 1;
            Type type = typeof(byte);
        }

        public static DataField<T> Instance<T>(long offset) where T : struct
        {
            return new DataField<T>(offset)
            {
                Length = GetByteSize<T>(),
                Type = typeof(T)
            };
        }

        public static DataField<T> Instance<T>(long offset, ulong length) where T : class
        {
            return new DataField<T>(offset)
            {
                Length = length,
                Type = typeof(T)
            };
        }

        public static DataField<Byte> Byte(long offset) => Instance<Byte>(offset);
        public static DataField<Int32> Int32(long offset) => Instance<Int32>(offset);
        public static DataField<string> String(long offset, uint strLength) => Instance<string>(offset, strLength + 1);

        public byte[] GetData(Stream stream)
        {
            var buffer = new byte[Length];
            stream.Position = Offset;
            int r = stream.Read(buffer, 0, buffer.Length);
            if (r >= 0 && (ulong)r != Length) Array.Resize(ref buffer, r);
            return buffer;
        }

        public static uint GetByteSize<T>()
        {
            T var = default(T);
            switch (var)
            {
                case Byte s:
                    return 1;
                case UInt16 u:
                case Int16 s:
                    return 2;
                case UInt32 u:  // uint
                case Int32 s:   // int
                    return 4;
                case UInt64 u:  // ulong
                case Int64 s:   // long
                    return 8;
                default:
                    return 0;
            }
        }
    }


    public class DataField<T> : DataField
    {

        public DataField(long offset) : base(offset)
        {
        }

        public T GetValue(Stream stream)
        {
            return GetValue(GetData(stream));
        }

        public T GetValue(byte[] data, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;

            if (data.Length == 0) return default(T);

            if (typeof(T) == typeof(string))
                return (T)Convert.ChangeType(GetString(data, encoding), typeof(T));
            
            return default(T);
        }

        public string GetString(byte[] data, Encoding encoding = null)
        {
            // binary format of string:
            // <byte length><string text> 

            encoding = encoding ?? Encoding.UTF8;

            byte length = data[0];

            if (data.Length <= 1) return "";
            if (length > data.Length - 1) length = (byte)(data.Length - 1);
            if (length <= 0) return "";

            Index end = new Index(length + 1);
            return encoding.GetString(data[1..end]);
        }
    }

  
}
