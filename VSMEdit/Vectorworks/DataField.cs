using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VSMEdit.Vectorworks
{

    public class DataField<T>
    {
        public string Name { get; private set; }

        public ulong Position { get; private set; }
        public ulong MaxLength { get; private set; }


        public DataField(string name, ulong position, ulong maxLength)
        {
            this.Name = name;
            this.Position = position;
            this.MaxLength = maxLength;
        }

        public static DataField<bool> Boolean(string name) => new DataField<bool>(name, 1, 1);


        public T Read(ref BinaryReader stream)
        {
            switch (this)
            {
                case DataField<bool> b:

                default:
                    break;
            }

            return default(T);
        }

    }
}
