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
   
    }
}
