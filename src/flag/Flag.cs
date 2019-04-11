using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;

namespace flag
{
    public abstract class Flag
    {
        private static readonly List<string> _args = new List<string>(10);
        private static readonly List<Flag> _flags = new List<Flag>(10);
        private static readonly Flag<bool> _helpFlag = new Flag<bool>("-?|--help", false, "Show help information");
        private static string[] _remaining;
        private static Stream _output;

        public string Template { get; protected set; }
        public string Usage { get; protected set; }

        public static StringComparison StringComparison { get; set; } = StringComparison.CurrentCultureIgnoreCase;

        protected abstract bool IsMatch(string name, StringComparison stringComparison);
        protected abstract void ParseValue(string value);

        #region Static methods

        public static void Parse()
        {
            var args = Environment.GetCommandLineArgs();

            if (args.Length == 1)
                ShowHelp();

            for (var i = 1; i < args.Length; i++)
            {
                if (args[i][0] != '-')
                {
                    _args.Add(args[i]);
                    continue;
                }

                if (args[i] == "--")
                {
                    _remaining = new string[args.Length - i];

                    for (var j = i; j < args.Length; j++)
                        _remaining[j - i] = args[j];

                    break;
                }

                var split = args[i].Split('=', ':');
                var name = split[0];
                string value = default;

                if (split.Length == 1 && args.Length - 1 > i)
                {
                    var next = args[i + 1];
                    if (next[0] != '-')
                    {
                        value = next;
                        i++;
                    }
                }
                else
                    value = split[1];


                if (_helpFlag.IsMatch(name, StringComparison))
                {
                    ShowHelp();
                    break;
                }

                GetFlag(name).ParseValue(value);
            }
        }

        public static string Arg(int i)
        {
            return _args.Count - 1 < i ? null : _args[i];
        }

        public static string[] Remaining()
        {
            return _remaining;
        }

        public static ref bool Bool(string name, bool defaultValue, string usage)
        {
            return ref AddFlag(name, defaultValue, usage);
        }

        public static ref byte Byte(string name, byte defaultValue, string usage)
        {
            return ref AddFlag(name, defaultValue, usage);
        }

        public static ref int Int(string name, int defaultValue, string usage)
        {
            return ref AddFlag(name, defaultValue, usage);
        }

        public static ref uint UInt(string name, uint defaultValue, string usage)
        {
            return ref AddFlag(name, defaultValue, usage);
        }

        public static ref short Short(string name, short defaultValue, string usage)
        {
            return ref AddFlag(name, defaultValue, usage);
        }

        public static ref ushort UShort(string name, ushort defaultValue, string usage)
        {
            return ref AddFlag(name, defaultValue, usage);
        }

        public static ref long Long(string name, long defaultValue, string usage)
        {
            return ref AddFlag(name, defaultValue, usage);
        }

        public static ref ulong ULong(string name, ulong defaultValue, string usage)
        {
            return ref AddFlag(name, defaultValue, usage);
        }

        public static ref float Float(string name, float defaultValue, string usage)
        {
            return ref AddFlag(name, defaultValue, usage);
        }

        public static ref double Double(string name, double defaultValue, string usage)
        {
            return ref AddFlag(name, defaultValue, usage);
        }

        public static ref decimal Decimal(string name, decimal defaultValue, string usage)
        {
            return ref AddFlag(name, defaultValue, usage);
        }

        public static ref string String(string name, string defaultValue, string usage)
        {
            return ref AddFlag(name, defaultValue, usage);
        }

        public static ref DateTime DateTime(string name, DateTime defaultValue, string usage)
        {
            return ref AddFlag(name, defaultValue, usage);
        }

        public static ref TimeSpan TimeSpan(string name, TimeSpan defaultValue, string usage)
        {
            return ref AddFlag(name, defaultValue, usage);
        }

        public static ref Version Version(string name, Version defaultValue, string usage)
        {
            return ref AddFlag(name, defaultValue, usage);
        }

        public static ref IPAddress IPAddress(string name, IPAddress defaultValue, string usage)
        {
            return ref AddFlag(name, defaultValue, usage);
        }

        public static ref FileInfo File(string name, FileInfo defaultValue, string usage)
        {
            return ref AddFlag(name, defaultValue, usage);
        }

        public static ref DirectoryInfo Directory(string name, DirectoryInfo defaultValue, string usage)
        {
            return ref AddFlag(name, defaultValue, usage);
        }

        public static ref DayOfWeek DayOfWeek(string name, DayOfWeek defaultValue, string usage)
        {
            return ref Enum(name, defaultValue, usage);
        }

        public static ref T Enum<T>(string name, T defaultValue, string usage) where T : Enum
        {
            return ref AddFlag(name, defaultValue, usage);
        }

        public static void SetOutput(Stream output)
        {
            _output = output;
        }

        private static void ShowHelp()
        {
            var output = new StreamWriter(_output ?? Console.OpenStandardError());
            var thisApp = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);

            output.WriteLine(thisApp.ProductVersion);
            output.WriteLine($"\r\n{thisApp.ProductName} - {thisApp.Comments}");
            output.WriteLine($"Usage: {thisApp.OriginalFilename} [options] [arguments]\r\n");
            output.WriteLine("OPTIONS\r\n");

            foreach (var flag in _flags)
                output.WriteLine($"  {flag.Template,-20}\t{flag.Usage}");

            output.WriteLine($"  {_helpFlag.Template,-20}\t{_helpFlag.Usage}");

            output.Flush();

            Environment.Exit(-1);
        }

        private static ref T AddFlag<T>(string name, T defaultValue, string usage)
        {
            var flag = new Flag<T>(name, defaultValue, usage);
            _flags.Add(flag);

            return ref flag.Value();
        }

        private static Flag GetFlag(string name)
        {
            return _flags.FirstOrDefault(f => f.IsMatch(name, StringComparison)) ??
                   throw new ArgumentException("Unexpected argument: " + name);
        }

        #endregion
    }

    public class Flag<T> : Flag
    {
        private T _value;


        public Flag(string name, T defaultValue, string usage)
        {
            _value = defaultValue;
            Template = name;
            Usage = usage;
        }

        public ref T Value()
        {
            return ref _value;
        }

        protected override bool IsMatch(string s, StringComparison stringComparison)
        {
            return Template.Split('|').Any(t => t.Equals(s, stringComparison));
        }

        protected override void ParseValue(string value)
        {
            if (typeof(T) == typeof(bool))
                _value = (T) (object) bool.Parse(value);
            else if (typeof(T) == typeof(byte))
                _value = (T) (object) byte.Parse(value);
            else if (typeof(T) == typeof(int))
                _value = (T) (object) int.Parse(value);
            else if (typeof(T) == typeof(uint))
                _value = (T) (object) uint.Parse(value);
            else if (typeof(T) == typeof(short))
                _value = (T) (object) short.Parse(value);
            else if (typeof(T) == typeof(ushort))
                _value = (T) (object) ushort.Parse(value);
            else if (typeof(T) == typeof(long))
                _value = (T) (object) long.Parse(value);
            else if (typeof(T) == typeof(ulong))
                _value = (T) (object) ulong.Parse(value);
            else if (typeof(T) == typeof(float))
                _value = (T) (object) float.Parse(value);
            else if (typeof(T) == typeof(double))
                _value = (T) (object) double.Parse(value);
            else if (typeof(T) == typeof(decimal))
                _value = (T) (object) decimal.Parse(value);
            else if (typeof(T) == typeof(string) || typeof(T) == typeof(object))
                _value = (T) (object) value;
            else if (typeof(T).IsEnum)
                _value = (T) System.Enum.Parse(typeof(T), value,
                    StringComparison == StringComparison.CurrentCultureIgnoreCase ||
                    StringComparison == StringComparison.InvariantCultureIgnoreCase ||
                    StringComparison == StringComparison.OrdinalIgnoreCase);
            else if (typeof(T) == typeof(DateTime))
                _value = (T) (object) System.DateTime.Parse(value);
            else if (typeof(T) == typeof(TimeSpan))
                _value = (T) (object) System.TimeSpan.Parse(value);
            else if (typeof(T) == typeof(Version))
                _value = (T) (object) System.Version.Parse(value);
            else if (typeof(T) == typeof(IPAddress))
                _value = (T) (object) System.Net.IPAddress.Parse(value);
            else if (typeof(T) == typeof(FileInfo))
                _value = (T) (object) new FileInfo(value);
            else if (typeof(T) == typeof(DirectoryInfo))
                _value = (T) (object) new DirectoryInfo(value);
            else
                throw new InvalidOperationException("Unable to convert value " + value + " to type " + typeof(T));
        }
    }
}
