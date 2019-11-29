using System;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;
using VSMEdit.Vectorworks;
using VWLmergeR.Extensions;

namespace VWLmergeR
{

    public enum FlagOperation
    {
        Continue,
        DisplayHelp,
        Quit
    }

    public class FlagEvent
    {
        public string Flag { get; private set; }
        public int ExitCode { get; set; }
        public FlagOperation Operation { get; set; }
        public int ArgumentCount { get; set; }

        public FlagEvent(string flag)
        {
            this.Flag = flag;
        }
    }

    public abstract class ProgramBase
    {
        public static string Name => typeof(ProgramBase).Assembly.GetCustomAttribute<AssemblyTitleAttribute>().Title;
        public static string Copyright => typeof(ProgramBase).Assembly.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright;
        public static string Description => typeof(ProgramBase).Assembly.GetCustomAttribute<AssemblyDescriptionAttribute>().Description;
        public static Version Version => new Version(typeof(ProgramBase).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion);
        public static Version FileVersion => new Version(typeof(ProgramBase).Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version);

        public static string Framework => typeof(ProgramBase).Assembly.GetCustomAttribute<TargetFrameworkAttribute>().FrameworkName.Replace("App,Version=", " ");

        /// <summary>
        /// The application console header, containing the name, the version and copyright information.
        /// </summary>
        /// <returns>The header as string.</returns>
        public static string Header => $"{Name} -- v{Version.ToString(2)} -- {Copyright} | {Framework}";

        public string[] Arguments { get; private set; }

        public ProgramBase(string[] args)
        {
            Arguments = args;
        }

        public void Main()
        { 
            Console.OutputEncoding = Encoding.UTF8;

            Console.WriteLine(Header);
            Console.WriteLine();

            int exitcode = 0;
            if (exitcode == 0) exitcode = HandleArgs(Arguments);
            if (exitcode == 0) exitcode = Run();

            // Also works for osx/linux ...
            Environment.Exit(exitcode);
        }

        public int HandleArgs(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Error.WriteLine("No arguments. " + HelpAdvice);
                Thread.Sleep(1500);
                return 101;
            }

            int argIndex = 0;            
            FlagEvent currentFlag = null;
            int flagArgIndex = 0;

            foreach (var arg in args)
            {
                var flag = arg.ToLower();
                flag = flag.Replace("/", "-");
                flag = flag.Replace("\\", "-");
                bool isFlag = flag.StartsWith("-");

                if (arg.Equals("?")) isFlag = true;
                if (!isFlag)
                {
                    if (currentFlag != null && currentFlag.ArgumentCount > 0)
                    {
                        OnFlagArgument(currentFlag, arg, flagArgIndex);
                        flagArgIndex++;
                    }
                    OnArgument(arg, argIndex);

                    argIndex++;
                }

                if (!isFlag) continue;
                flag = flag.Replace("-", "");

                FlagEvent flagEvent = new FlagEvent(flag);
                bool validFlag = OnFlag(ref flagEvent);
                if (!validFlag)
                {
                    Console.Error.WriteLine("Invalid argument. " + HelpAdvice);
                    return 102;
                }

                switch (flagEvent.Operation)
                {
                    case FlagOperation.DisplayHelp:
                        Console.WriteLine(HelpText);
                        return 100; // Help Message displayed
                    case FlagOperation.Quit:
                        int code = flagEvent.ExitCode;
                        return code == 0 ? 1 : code;
                    case FlagOperation.Continue:
                    default: break;
                }

                if (currentFlag.ArgumentCount > 0)
                {
                    currentFlag = flagEvent;
                }
            }

            return 0;
        }

        public abstract void OnArgument(string arg, int argIndex);
        public abstract void OnFlagArgument(ref FlagEvent flag, string arg, int argIndex);
        public abstract bool OnFlag(ref FlagEvent flag);

        public abstract int Run();

        public string HelpAdvice => "Try '--help' for more information.";

        public string HelpText => String.Join('\n', new string[]
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

        public void WriteError(Exception ex)
        {
#if DEBUG
            Console.Error.WriteLine(ex);
#else
            Console.Error.WriteLine("Error: " + ex.Message);
#endif
        }
    }
}
