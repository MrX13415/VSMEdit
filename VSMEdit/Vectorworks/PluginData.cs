using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VSMEdit.Vectorworks
{

    public partial class Plugin
    {

        public void Get()
        {

        }


        public void Set()
        {

        }

        public Stream Open(bool write = false)
        {
            return File.Open(FileMode.Open, write ? FileAccess.Write : FileAccess.Read);
        }

        public bool Load()
        {
            using (BinaryReader reader = new BinaryReader(Open()))
            {
               
                
            }
            return true;
        }

        public bool Save()
        {
            return true;
        }
    }
}
