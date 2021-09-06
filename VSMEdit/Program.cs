using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Binary;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;
using VSMEdit.Vectorworks;
using VWLmergeR.Extensions;
using static VSMEdit.Vectorworks.Plugin.FieldData;

namespace VWLmergeR
{
    class Program : ProgramBase
    {
        
        static void Main(string[] args)
        {
            Program program = new Program(args);
            program.Main();
        }

        public enum FieldMode
        {
            None,
            Get,
            Set
        }

        public FieldMode Mode { get; set; } = FieldMode.None;
        public string FilePath { get; set; }
        public string FieldName { get; set; }
        public string FieldValue { get; set; }

        public Program(string[] args) : base(args)
        {
            HeaderDisplay = HeaderDisplay.Help | HeaderDisplay.Errors;
        }

        public override void OnArgument(string arg, int argIndex)
        {
            switch (argIndex)
            {
                case 0: FilePath = arg; break;
                default: break;
            }
        }

        public override void OnFlagArgument(ref FlagEvent flag, string arg, int argIndex)
        {
            switch (argIndex)
            {
                case 0:
                    FieldName = arg; break;
                case 1:
                    FieldValue = arg; break;
                default: break;
            }
        }

        public override bool OnFlag(ref FlagEvent flag)
        {
            switch (flag.Flag)
            {
                case "?":
                case "h":
                case "help":
                    flag.Operation = FlagOperation.DisplayHelp;
                    break;
                case "a":
                case "attr":
                case "attrs":
                case "attribut":
                case "attribute":
                case "attributes":
                    flag.Operation = FlagOperation.Quit;
                    flag.ExitCode = 103;
                    Console.WriteLine(GetAttributeList());
                    break;
                case "g":
                case "r":
                case "get":
                case "read":
                    Mode = FieldMode.Get;
                    flag.ArgumentCount = 1;
                    break;
                case "s":
                case "w":
                case "set":
                case "write":
                    Mode = FieldMode.Set;
                    flag.ArgumentCount = 2;
                    break;
                default:
                    return false;                    
            }

            return true;
        }

        public override int Run()
        {
            if (!File.Exists(FilePath))
            {
                Console.WriteLine($"Error: File not found: '{FilePath}'");
                return 2;
            }

            Plugin plugin = new Plugin(FilePath);

            if (!plugin.IsValid())
            {
                Console.WriteLine($"Invalid or not a Vectorworks script plugin.");
                return 1;
            }

            FileVersion version = (FileVersion) plugin.Read(Plugin.Field.FileVersion);
            if (version < Plugin.FieldData.FileVersion.Min) 
            {
                Console.WriteLine($"Invalid or Unsupported plugin format version: {(byte)version}");
                Console.WriteLine($"Only plugins from Vectorworks 2018 (23.x) and later are supported!");
                return 1;
            }
            if (version > Plugin.FieldData.FileVersion.Current) //
            {
                Console.WriteLine($"Warning: Unknown plugin format version: {(byte)version}");
                Console.WriteLine($"         The version of the specifed plugin is newer then the supported version.");
                Console.WriteLine($"         Getting or setting any attribute may or may not work.");
            }

            try
            {
                var field = Plugin.Field.Get(FieldName);
                int code = 0;

                if (Mode == FieldMode.Set)
                {
                    if (field.Field.ReadOnly) throw new Plugin.ReadOnlyFieldException(field.Name);
                    var f = SetFieldValue(plugin, field.Field, FieldValue);

                    code = f.ErrorCode;
                    if (!f.Ok)
                    {
                        Console.WriteLine($"Unable to set attribute '{field.Name}'! Unsupported?");
                        return code;
                    }
                }

                // Mode == FieldMode.Get
                var v = GetFieldValue(plugin, field.Field);
                if (v.Custom) Console.WriteLine($"{v.Text}");
                else Console.WriteLine($"{field.Name}: '{v.Text}'");

                if (code == 0) code = v.ErrorCode;
                return code;
            }
            catch (Plugin.FieldNotFoundException)
            {
                Console.WriteLine($"Attribute with name '{FieldName}' could not be found!");
                return 3;
            }
            catch (Plugin.FieldAmbiguityException)
            {
                Console.WriteLine($"Too many possiblities for '{FieldName}':");
                Plugin.Field.Fields().ToList()
                    .ForEach(f => Console.WriteLine($"   {f.Name}"));
                return 4;
            }
            catch (Plugin.ReadOnlyFieldException)
            {
                Console.WriteLine($"Attribute with name '{FieldName}' is read only and can not be changed!");
                return 6;
            }
        }

        public (bool Custom, string Text, int ErrorCode) GetFieldValue(Plugin plugin, DataField field)
        {
            if (field.Type == typeof(byte))
            {
                DataField<byte> bField = field as DataField<byte>;
                byte value = plugin.Read(bField);


                if (field == Plugin.Field.PluginType)
                {
                    switch ((PluginType)value)
                    {
                        case PluginType.Menu: return (true, "Plugin type: Menu command", 0);
                        case PluginType.Tool: return (true, "Plugin type: Tool command", 0);
                        case PluginType.Object: return (true, "Plugin type: Parametric object", 0);
                        case PluginType.TextFile: return (true, "Plugin type: Text file", 0);
                        case PluginType.Invalid: return (true, "Invalid or unknown plugin type", 1);
                    }
                }


                if (field == Plugin.Field.ByteOrdering)
                {
                    switch ((ByteOrdering)value)
                    {
                        case ByteOrdering.LittleEndian: return (true, "Byte order: Little Endian (Intel)", 0);  // I
                        case ByteOrdering.BigEndian: return (true, "Byte order: Big Endian (Motorola)", 0);     // M
                    }
                }

                if (field == Plugin.Field.FileVersion)
                {
                    var info = Plugin.FieldData.GetFileVersionInfo(value);
                    return (true, $"Plugin format version: {value} {(info.Length > 0 ? "-" : "")} {info}", 0);
                }

                return (false, value.ToString(), 0);
            }


            if (field.Type == typeof(string))
            {
                DataField<string> strField = field as DataField<string>;
                return (false, plugin.Read(strField), 0);
            }

            return (true, "", 1);
        }

        public (bool Ok, int ErrorCode) SetFieldValue(Plugin plugin, DataField field, string value)
        {
            if (field.ReadOnly) return (false, 4);

            if (field.Type == typeof(byte))
            {
                // NOT supported as of yet ...
                return (false, 1);
            }

            if (field.Type == typeof(string))
            {
                DataField<string> strField = field as DataField<string>;
                int code = 0;
                if (value.Length > strField.StringMaxLength)
                {
                    Console.WriteLine($"Warning: The text to be set exceeds the allowed maximum length ({strField.StringMaxLength} characters) of the specified attribute.");
                    Console.WriteLine($"The text has been truncate to: '{value.Substring(0, strField.StringMaxLength)}'");
                    code = 6;
                }
                plugin.Write(strField, value);
                return (true, code);
            }

            return (false, 1);
        }

        public string GetAttributeList()
        {
            var fields = Plugin.Field.Fields().ToList();
            string txt = $"Supported attributes: ({fields.Count})\n";
            int padding = 0;
            fields.ForEach(f => padding = (f.Name.Length > padding) ? f.Name.Length : padding);

            foreach (var f in fields)
            {
                var field = f.Field;
                var name = String.Format($"{{0,-{padding}}}", f.Name);

                List<string> infos = new List<string>();
                if (field.Type == typeof(string))
                    infos.Add($"Text: max {(field.Length - 1)} characters");
                if (field.ReadOnly)
                    infos.Add("Read Only");

                txt += $"   {name}   ({(String.Join("; ", infos))})\n";
            }

            return txt;
        }

        public override string[] GetHelpTextLines()
        {
            return new string[]
            {
                //2345678901234567890123456789012345678901234567890123456789012345678901234567890
                "A command line utility for changing various attributes of script plugins from Vectorworks.",
                "",
                "  Usage: VSMEdit <options>",
                "",
                "    -h, --help                 Prints this help message.",
                "    -a, --attributes           Lists the supported attributes.",
                "",
                "",
                "  Usage: VSMEdit [options] <plugin>",
                "",
                "    -g, --get <attr>           Returns the value of an attribute.",
                "    -s, --set <attr> <value>   Sets the value of an attribute.",
                "",
                "    plugin                     A Vectorworks Script plugin file.",
                "                               (.vsm, .vso, .vst)",
                "",
                "",
                "  i.e. VSMEdit --get UniversalName MyToolCW.vsm",
                "",
                "  Error codes:",
                "      0   Operation succeeded.",
                "      1   Unexpected error.",
                "      2   File not found or readable/writeable.",
                "      3   Attribute not found.",
                "      4   Ambiguous attribute.",
                "      5   Attribute is read only.",
                "      6   Input value truncate.",
                "    100   Help message displayed.",
                "    101   No arguments.",
                "    102   Invalid arguments.",
                "    103   Attributes listed.",
            };
        }
    }
}
