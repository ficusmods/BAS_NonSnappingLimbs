using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace NonSnappingLimbs
{
    public class Logger
    {
        public enum Level
        {
            None = 0,
            Basic = 1,
            Detailed = 2
        }

        private static string modname = "UnnamedMod";
        private static string mod_version = "0.0";
        private static Level level = Level.Basic;

        public static void init(string name, string version, string level)
        {
            Logger.modname = name;
            Logger.mod_version = version;
            Logger.level = (Logger.Level)Enum.Parse(typeof(Logger.Level), level);
        }

        public static void Basic(object msg)
        {
            if(level >= Level.Basic)
                Debug.Log(String.Format("{0} v{1} (Basic) | {2}", modname, mod_version, msg));
        }
        public static void Detailed(object msg)
        {
            if (level >= Level.Detailed)
                Debug.Log(String.Format("{0} v{1} (Detailed) | {2}", modname, mod_version, msg));
        }
    }
}
