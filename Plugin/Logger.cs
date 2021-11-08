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
        public static string modname = "fksmod";
        public static void Msg(object msg)
        {

            Debug.Log(String.Format("{0} | {1}", modname, msg));
        }
    }
}
