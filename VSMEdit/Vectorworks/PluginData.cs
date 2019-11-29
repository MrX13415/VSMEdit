using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Binary;
using System.Text;

namespace VSMEdit.Vectorworks
{

    public partial class Plugin
    {
        public abstract class Field
        {
            public static readonly DataField<Byte> PluginType = DataField.Byte(4);
            public static readonly DataField<Byte> ByteOrdering = DataField.Byte(5);

            public static readonly DataField<string> UniversalName = DataField.String(6, 63);
            public static readonly DataField<string> Category = DataField.String(70, 63);

            public static readonly DataField<Byte> FileVersion = DataField.Byte(134);

            public static readonly DataField<string> LocalizedName = DataField.String(162, 63);
        }

        public void Get()
        {

        }


        public void Set()
        {

        }

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

        public bool Load()
        {
            return false;  // TODO
        }

        public bool Save()
        {
            return false;  // TODO
        }
    }
}
