using System;
using System.Collections.Generic;
using System.IO.Binary;
using System.Linq;
using System.Reflection;
using System.Text;

namespace VSMEdit.Vectorworks
{

    public partial class Plugin
    {
        public abstract class FieldData
        {
            public enum ByteOrdering
            {
                LittleEndian = 73, // I (Intel)
                BigEndian = 77     // M (Motorola)
            }

            public enum PluginType
            {
                Invalid = -1,
                Menu = 0,
                Tool = 1,
                Object = 2,
                TextFile = 3
            }

            public enum FileVersion
            {
                Unknown = -1,
                Min = 1,
                UnicodeSupport = 12,  // UTF-8 strings are saved in file version 12 and later. Vectorworks 23.x.
                Current = 13,         // Current "known" file format version as 08-2021
            }

            public static string GetFileVersionInfo(byte version)
            {
                // TODO: more info?
                switch (version)
                {
                    case (byte)FileVersion.Current: return "VectorWorks 23.x and later (unicode support) [current]";

                    case 1: return "VectorWorks 8.0 - 8.5.2";   // Version 1 is for VectorWorks 8.0 - 8.5.2
                    case 2: return "VectorWorks 9.0";           // Version 2 is for VectorWorks 9.0    
                    case 3: return "VectorWorks 10.0";          // Version 3 is for VectorWorks 10.0.  Adds Miscellaneous strings.
                    case 4: return "VectorWorks 10.0";          // Version 4 is for VectorWorks 10.0.  Adds alt plug-in name, and alt names for pop-up values.                   
                    case 5: return "VectorWorks 11.x";          // Version 5 is for VectorWorks 11.x.  Adds extended properties bit
                    case 6: return "VectorWorks 12.x";          // Version 6 is for VectorWorks 12.x.  Adds metric defaults
                    case 7: return "VectorWorks 13.x";          // Version 7 is for VectorWorks 13.x.  Adds PNG icons and Contextual Help Identifiers
                    case 8: return "VectorWorks 14.x";          // Version 8 is for VectorWorks 14.x.  Adds Version Information to record what version:
                                                                //                                        1. The plug-in was created for
                                                                //                                        2. The plug-in was significantly modified for
                                                                //                                        3. The plug-in was retired (considered legacy)
                    case 9: return "VectorWorks 15.x";          // Version 9 is for VectorWorks 15.x   Adds Insert Into Walls option									
                    case 10: return "VectorWorks 19.x";         // Version 10 is for VectorWorks 19.x  Adds PNG Retina icons
                    case 11: return "VectorWorks 19.x - 22.x";  // Version 11 is for VectorWorks 19.x  This version does not add a new information. 
                                                                //                                        This version was added, because there were broken
                                                                //                                        binary files by version 10 and we had to recognize them.
                                                                //                                        bug VB-109151
                }

                return "";
            }
        }

        public abstract class Field
        {
            public static readonly DataField<Byte> PluginType = DataField.Byte(4, readOnly: true);
            public static readonly DataField<Byte> ByteOrdering = DataField.Byte(5, readOnly: true);

            public static readonly DataField<string> UniversalName = DataField.String(6, 63);
            public static readonly DataField<string> Category = DataField.String(70, 63);

            public static readonly DataField<Byte> FileVersion = DataField.Byte(134, readOnly: true);

            public static readonly DataField<string> LocalizedName = DataField.String(162, 63);

            public static (string Name, DataField Field) Get(string name)
            {
                var list = Fields().Where(f => f.Name.ToLower().Contains(name.ToLower())).ToList();
                if (list.Count <= 0) throw new FieldNotFoundException(name);
                if (list.Count > 1) throw new FieldAmbiguityException(name);
                return list.First();
            }

            public static IEnumerable<(string Name, DataField Field)> Fields()
            {
                var list = typeof(Field).GetFields(BindingFlags.Public | BindingFlags.Static);
                return list.Select(f => (f.Name, f.GetValue(null) as DataField));
            }
        }

        public class FieldNotFoundException : Exception
        {
            public FieldNotFoundException(string name) : base($"Field '{name}' does not exist.")
            {
            }
        }
        public class FieldAmbiguityException : Exception
        {
            public FieldAmbiguityException(string name) : base($"Too many matching fields for '{name}'.")
            {
            }
        }
        public class ReadOnlyFieldException : Exception
        {
            public ReadOnlyFieldException(string name) : base($"Field with name '{name}' is read only and can't be changed.")
            {
            }
        }
    }

}
