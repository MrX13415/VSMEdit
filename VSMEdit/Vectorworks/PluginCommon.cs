using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VSMEdit.Vectorworks
{
    public enum PluginType
    {
        Invalid,
        Menu,
        Tool,
        Object
    }


    public partial class Plugin
    {
  
        public FileInfo File { get; private set; }
        
        public PluginType Type => GetType(File.Extension);


        public Plugin(string filePath)
        {
            this.File = new FileInfo(filePath);
            if (!File.Exists) throw new FileNotFoundException();
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
                case PluginType.Invalid:
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
                default:
                    return PluginType.Invalid;
            }
        }
    }
}
