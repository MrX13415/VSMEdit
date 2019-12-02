using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Binary;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace System.IO.Binary
{
    public class DataField
    {
        public long Offset { get; internal set; }
        public ulong Length { get; internal set; }

        public Type Type { get; internal set; }
        public bool ReadOnly { get; internal set; }

        public DataField(long offset, bool readOnly = false)
        {
            this.Offset = offset;
            this.Length = 1;
            Type type = typeof(byte);
            ReadOnly = readOnly;
        }

        public static DataField<T> Instance<T>(long offset, bool readOnly = false) where T : struct
        {
            return new DataField<T>(offset)
            {
                Length = GetByteSize<T>(),
                Type = typeof(T),
                ReadOnly = readOnly
            };
        }

        public static DataField<T> Instance<T>(long offset, ulong length, bool readOnly = false) where T : class
        {
            return new DataField<T>(offset)
            {
                Length = length,
                Type = typeof(T),
                ReadOnly = readOnly
            };
        }

        public static DataField<Byte> Byte(long offset, bool readOnly = false)
            => Instance<Byte>(offset, readOnly);
        public static DataField<Int32> Int32(long offset, bool readOnly = false)
            => Instance<Int32>(offset, readOnly);
        public static DataField<string> String(long offset, uint strLength, bool readOnly = false)
            => Instance<string>(offset, strLength + 1, readOnly);

        public byte[] GetData(Stream stream)
        {
            var buffer = new byte[Length];
            stream.Position = Offset;
            int r = stream.Read(buffer, 0, buffer.Length);
            if (r >= 0 && (ulong)r != Length) Array.Resize(ref buffer, r);
            return buffer;
        }

        public void SetData(Stream stream, byte[] data)
        {
            stream.Position = Offset;
            stream.Write(data, 0, data.Length);
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
                case Single f:  // float
                    return 4;
                case UInt64 u:  // ulong
                case Int64 s:   // long
                case Double d:  // double
                    return 8;
                default:
                    return 0;
            }
        }
    }


    public class DataField<T> : DataField
    {

        public DataField(long offset, bool readOnly = false) : base(offset, readOnly)
        {
        }

        public byte StringMaxLength
        {
            get
            {
                var l = (byte)Length - 1;
                return (byte)l;
            }
        }


        public T GetValue(Stream stream)
        {
            return GetValue(GetData(stream));
        }

        public void SetValue(Stream stream, T value)
        {
            byte[] data = GetBytes(value, StringMaxLength);
            SetData(stream, data);
        }

        private static T GetValue(byte[] data, Encoding encoding = null)
        {
            if (data.Length == 0) return default(T);

            object value = null;

            if (typeof(T) == typeof(byte)) value = data[0];

            if (typeof(T) == typeof(UInt16)) value = BitConverter.ToUInt16(data);
            if (typeof(T) == typeof(Int16)) value = BitConverter.ToInt16(data);

            if (typeof(T) == typeof(UInt32)) value = BitConverter.ToUInt32(data);
            if (typeof(T) == typeof(Int32)) value = BitConverter.ToInt32(data);

            if (typeof(T) == typeof(UInt64)) value = BitConverter.ToUInt64(data);
            if (typeof(T) == typeof(Int64)) value = BitConverter.ToInt64(data);

            if (typeof(T) == typeof(Single)) value = BitConverter.ToSingle(data);
            if (typeof(T) == typeof(Double)) value = BitConverter.ToDouble(data);

            if (typeof(T) == typeof(string)) value = GetString(data, encoding);
           
            if (value != null)
                return (T)Convert.ChangeType(value, typeof(T));

            return default(T);
        }

        public static byte[] GetBytes(T value, byte strMaxLength = byte.MaxValue, Encoding encoding = null)
        {
            byte[] data = null;
            
            if (typeof(T) == typeof(byte)) data = BitConverter.GetBytes(Convert.ToByte(value));

            if (typeof(T) == typeof(UInt16)) data = BitConverter.GetBytes(Convert.ToUInt16(value));
            if (typeof(T) == typeof(Int16)) data = BitConverter.GetBytes(Convert.ToInt16(value));
           
            if (typeof(T) == typeof(UInt32)) data = BitConverter.GetBytes(Convert.ToUInt32(value));
            if (typeof(T) == typeof(Int32)) data = BitConverter.GetBytes(Convert.ToInt32(value));
            
            if (typeof(T) == typeof(UInt64)) data = BitConverter.GetBytes(Convert.ToUInt64(value));
            if (typeof(T) == typeof(Int64)) data = BitConverter.GetBytes(Convert.ToInt64(value));

            if (typeof(T) == typeof(Single)) data = BitConverter.GetBytes(Convert.ToSingle(value));
            if (typeof(T) == typeof(Double)) data = BitConverter.GetBytes(Convert.ToDouble(value));

            if (typeof(T) == typeof(string)) data = GetBytes(Convert.ToString(value), strMaxLength, encoding);

            return data;
        }

        public static string GetString(byte[] data, Encoding encoding = null)
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

        public static byte[] GetBytes(string text, byte maxLength = byte.MaxValue, Encoding encoding = null)
        {
            // binary format of string:
            // <byte length><string text> 

            encoding = encoding ?? Encoding.UTF8;

            if (text.Length > maxLength) text = text.Substring(0, maxLength);
            var data = new byte[] { (byte)text.Length };
            return data.Concat(encoding.GetBytes(text)).ToArray();
        }
    }

  
}
