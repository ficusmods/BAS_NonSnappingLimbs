using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace NonSnappingLimbs
{
    class Logger
    {
        public static string modname = "UnnamedMod";
        public static string mod_version = "0.0";
        public static void Msg(object msg)
        {

            Debug.Log(String.Format("{0} v{1} | {2}", modname, mod_version, msg));
        }
    }
}
