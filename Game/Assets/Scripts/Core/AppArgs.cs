using System;
using System.Collections.Generic;

namespace TwelfthStamp
{
    /// <summary>
    /// Our own command-line arguments (Unity does not know these):
    ///   --mode=vr | --mode=desktop      choose the player rig (default desktop)
    ///   --demo                          boot straight into the judges' demo
    ///   --autotest=&lt;name&gt;           run an in-build automated check and quit (writes a report)
    ///   --report-dir=&lt;path&gt;          where autotest reports/screenshots go
    ///   --start=&lt;CellId&gt;            developer: start the campaign in a given cell (no save writes)
    /// </summary>
    public static class AppArgs
    {
        public static GameMode? Mode { get; private set; }
        public static bool ModeArgumentPresent { get; private set; }
        public static string InvalidModeValue { get; private set; }
        public static bool Demo { get; private set; }
        public static string AutoTest { get; private set; }
        public static string ReportDir { get; private set; }
        public static string StartCell { get; private set; }

        static bool s_Parsed;

        public static void Parse() => Parse(Environment.GetCommandLineArgs());

        public static void Parse(IList<string> args)
        {
            Mode = null; ModeArgumentPresent = false; InvalidModeValue = null; Demo = false; AutoTest = null; ReportDir = null; StartCell = null;
            foreach (var raw in args)
            {
                if (string.IsNullOrEmpty(raw)) continue;
                var a = raw.Trim();
                if (a.StartsWith("--mode=", StringComparison.OrdinalIgnoreCase))
                {
                    ModeArgumentPresent = true;
                    var v = a.Substring(7).Trim().ToLowerInvariant();
                    if (v == "vr" || v == "quest" || v == "xr") Mode = GameMode.VR;
                    else if (v == "desktop" || v == "flat" || v == "pc") Mode = GameMode.Desktop;
                    else InvalidModeValue = v;
                }
                else if (a.Equals("--demo", StringComparison.OrdinalIgnoreCase)) Demo = true;
                else if (a.StartsWith("--autotest=", StringComparison.OrdinalIgnoreCase)) AutoTest = a.Substring(11);
                else if (a.StartsWith("--report-dir=", StringComparison.OrdinalIgnoreCase)) ReportDir = a.Substring(13).Trim('"');
                else if (a.StartsWith("--start=", StringComparison.OrdinalIgnoreCase)) StartCell = a.Substring(8);
            }
            s_Parsed = true;
        }

        public static void EnsureParsed()
        {
            if (!s_Parsed) Parse();
        }
    }
}
