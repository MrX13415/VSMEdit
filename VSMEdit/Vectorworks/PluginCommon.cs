using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static VSMEdit.Vectorworks.Plugin.FieldData;

namespace VSMEdit.Vectorworks
{
    public partial class Plugin
    {
  
        public FileInfo PluginFile { get; private set; }
        
        public PluginType Type => GetType(PluginFile.Extension);


        public Plugin(string filePath)
        {
            this.PluginFile = new FileInfo(filePath);
            if (!PluginFile.Exists) throw new FileNotFoundException();
        }
      

        public static string GetFileExtention(PluginType type)
        {
            switch (type)
            {
                case PluginType.Menu:
                    return ".vsm";                    
                case PluginType.Tool:
                    return ".vst";               
                case PluginType.Object:
                    return ".vso";               
                case PluginType.TextFile:
                    return ".txt"; // TODO: txt file extension??
                default:
                    return null;               
            }
        }

        public static PluginType GetType(string extention)
        {
            extention = extention.Trim();
            if (!extention.StartsWith(".")) extention = $".{extention}";
            switch (extention)
            {
                case ".vsm":
                    return PluginType.Menu;
                case ".vst":
                    return PluginType.Tool;
                case ".vso":
                    return PluginType.Object;
                    // TODO: txt file extension??
                default:
                    return PluginType.Invalid;
            }
        }
    }
}
