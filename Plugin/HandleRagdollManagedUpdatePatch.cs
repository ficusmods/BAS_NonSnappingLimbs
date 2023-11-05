using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;
using ThunderRoad;
using UnityEngine;

namespace NonSnappingLimbs
{
    [HarmonyPatch(typeof(HandleRagdoll))]
    [HarmonyPatch("ManagedUpdate")]
    public class HandleRagdollManagedUpdatePatch
    {
        public static bool Prefix(HandleRagdoll __instance)
        {
            if (!__instance.ragdollPart.initialized)
            {
                return true;
            }

            if(__instance.ragdollPart.isSliced)
            {
                return false;
            }
            return true;
        }
    }
}
