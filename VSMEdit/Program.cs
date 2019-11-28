using System;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading;
using VWLmergeR.Extensions;

namespace VWLmergeR
{
    class Program
    {
        public enum Mode
        {
            None,
            Get,
            Set
        }

        private Mode mode = Mode.None;
        private int VersionYear = -1;
        private bool test = false;

        public static string Name => typeof(Program).Assembly.GetCustomAttribute<AssemblyTitleAttribute>().Title;
        public static string Copyright => typeof(Program).Assembly.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright;
        public static string Description => typeof(Program).Assembly.GetCustomAttribute<AssemblyDescriptionAttribute>().Description;
        public static Version Version => new Version(typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion);
        public static Version FileVersion => new Version(typeof(Program).Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version);

        public static string Framework => typeof(Program).Assembly.GetCustomAttribute<TargetFrameworkAttribute>().FrameworkName.Replace("App,Version=", " ");

        /// <summary>
        /// The application console header, containing the name, the version and copyright information.
        /// </summary>
        /// <returns>The header as string.</returns>
        public static string Header
         => $"{Name} -- v{Version.ToString(2)} -- {Copyright} | {Framework}";

        static int Main(string[] args)
        {
            Console.WriteLine(Header);
            Console.WriteLine();

            int exitcode = 0;
            Program program = new Program();
            if (exitcode == 0) exitcode = program.HandleArgs(args);
            if (exitcode == 0) exitcode = program.Run();

            // Also works for osx/linux ...
            Environment.Exit(exitcode);
            return exitcode;
        }

        public int HandleArgs(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Error.WriteLine("No arguments. " + HelpAdvice);
                Thread.Sleep(1500);
                return 101;
            }

            int reqFlagArgs = 0;
            foreach (var arg in args)
            {
                var flag = arg.ToLower();
                flag = flag.Replace("/", "-");
                flag = flag.Replace("\\", "-");
                bool isFlag = flag.StartsWith("-");

                if (!isFlag)
                {
                    if (arg.Equals("?")) isFlag = true;


                }

                if (!isFlag) continue;
                flag = flag.Replace("-", "");

                switch (flag)
                {
                    case "?":
                    case "h":
                    case "help":
                        Console.WriteLine(HelpText);
                        return 100; // Help Message displayed
                    case "a":
                    case "attr":
                    case "attrs":
                    case "attribut":
                    case "attribute":
                    case "attributes":
                        Console.WriteLine(HelpText);
                        return 100; // Help Message displayed
                    case "g":
                    case "r":
                    case "get":
                    case "read":
                        mode = Mode.Get;
                        reqFlagArgs = 1;
                        break;
                    case "s":
                    case "w":
                    case "set":
                    case "write":
                        mode = Mode.Set;
                        reqFlagArgs = 2;
                        break;
                    default:
                        if (!TryParseYear(arg))
                        {
                            Console.Error.WriteLine("Invalid argument. " + HelpAdvice);
                            return 102;
                        }
                        break;
                }
            }

            return 0;
        }

        public int Run()
        {
            if (!test && !ValidateYear()) return 1;
            
            Config config = null;
            switch (serverCfg)
            {
                case ServerConfig.Test: config = Config.Test(VersionYear); break;
                case ServerConfig.Live: config = Config.Live(VersionYear); break;
                case ServerConfig.None:
                default:
                    Console.Error.WriteLine("No Server selected! " + HelpAdvice);
                    return 2;
            }

            Console.Write($"Connecting to {config.Name} ... ");

            using(var server = new Server(config))
            {
                if (!server.Connect())
                {
                    Console.WriteLine("Failed");
                    WriteError(server.LastError);
                    return 3;
                }
                Console.WriteLine("OK");
                Console.WriteLine();

                if (test)
                {
                    Console.WriteLine("Testing Connectivity:");
                    Console.WriteLine();
                    if (server.Execute_Test()) return 10;
                    WriteError(server.LastError);
                    return 11;
                }
                else
                {
                    if (server.Execute_Merge()) return 0;
                    WriteError(server.LastError);
                    return 4;
                }
            }
        }

        public bool ValidateYear()
        {
            int T = DateTime.Today.Year;

            (int year, string message)[] list = {
                (   0, $"Invalid year: '{VersionYear}'"),
                (   1, $"Jesus hasn't even arrived yet!"),
                (1937, $"Computers don't exist yet..."),
                (1985, $"Vectorworks does not exist yet..."),
                (1999, $"MiniCAD is not supported!"),
                (2001, "VectorWorks 8 is not supported!"),
                (2002, "VectorWorks 9 is not supported!"),
                (2004, "VectorWorks 10 is not supported!"),
                (2005, "VectorWorks 11 is not supported!" ),
                (2008, "VectorWorks 12 is not supported!"),
                (2016, $"Vectorworks {VersionYear} is not supported!"),                
                (9999, $"Are you sure, you entered a valid year?"),
                (T+400, $"Forecasting the future is not supported yet."),
                (T+50, $"Vectorworks does not support quantum computers yet."),
                (T+1, $"Vectorworks {VersionYear} must be developed first.")
            };
 
            foreach (var item in list)
            {
                bool history = item.year < T && VersionYear < item.year;
                bool future = item.year > T && VersionYear > item.year;
                if (history || future)
                {
                    Console.Error.WriteLine(item.message);
                    Console.Error.WriteLine("Only Vectorworks 2016 or later is supported. " + HelpAdvice);
                    return false;
                }
            }

            return true;
        }

        public bool TryParseYear(string arg)
        {
            VersionYear = arg.ToInteger(-1);
            return VersionYear >= 0 && VersionYear <= short.MaxValue;
        }

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
