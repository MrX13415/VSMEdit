using System;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;
using VSMEdit.Vectorworks;
using VWLmergeR.Extensions;

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

        private FieldMode Mode { get; set; } = FieldMode.None;
        private string FilePath { get; set; }

        public Program(string[] args) : base(args)
        {
        }

        public int HandleArgss(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Error.WriteLine("No arguments. " + HelpAdvice);
                Thread.Sleep(1500);
                return 101;
            }

            int reqFlagArgs = 0;
            int paramIndex = 0;
            foreach (var arg in args)
            {
                var flag = arg.ToLower();
                flag = flag.Replace("/", "-");
                flag = flag.Replace("\\", "-");
                bool isFlag = flag.StartsWith("-");

                if (!isFlag)
                {
                    if (arg.Equals("?")) isFlag = true;


                    paramIndex++;
                }

                if (!isFlag) continue;
                flag = flag.Replace("-", "");

            }

            return 0;
        }

        public override void OnArgument(string arg, int argIndex)
        {
            switch (argIndex)
            {
                case 0: FilePath = arg; break;
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
                    flag.Operation = FlagOperation.DisplayHelp;
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
            Plugin vsm = new Plugin(FilePath);

            switch(Mode)
            {
                case FieldMode.Get:
                    
                    break;
            }

            return 0;
        }

        public new string HelpText => String.Join('\n', new string[]
        {
            //2345678901234567890123456789012345678901234567890123456789012345678901234567890
            "<desc>",
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
            "  i.e. VSMEdit -n=\"My Tool\" MyToolCW.vsm",
            "",
            "  Error codes:",
            "      0   Operation succeeded.",
            "      1   Invalid or not supported year.",
            "      2   No Server selected.",
            "      3   Connection failed.",
            "      4   Merging failed.",
            //"     10   Connectivity test succeeded.",
            //"     11   Connectivity test failed.",
            "    100   Help message displayed.",
            "    101   No arguments.",
            "    102   Invalid arguments.",
        });

    }
}
