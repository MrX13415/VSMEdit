using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Binary;
using System.Text;

namespace VSMEdit.Vectorworks
{

    public partial class Plugin
    {

        public FileStream Open(bool write = false)
        {
            return PluginFile.Open(FileMode.Open, write ? FileAccess.Write : FileAccess.Read);
        }

        public T Read<T>(DataField<T> data)
        {
            using (FileStream reader = Open())
            {
               return data.GetValue(reader);
            }
        }

        public void Write<T>(DataField<T> data, T value)
        {
            using (FileStream writer = Open(write: true))
            {
                data.SetValue(writer, value);
            }
        }
   
        public bool IsValid()
        {
            if (PluginFile.Length <= 4) return false;
            using (BinaryReader reader = new BinaryReader(Open()))
            {
                char m1 = reader.ReadChar();
                char m2 = reader.ReadChar();
                char m3 = reader.ReadChar();
                char m4 = reader.ReadChar();

                if (m1 == 'M' && m2 == 'C' && m3 == 'V' && m4 == 'S') return true;
            }

            return false;
        }
    }
}
