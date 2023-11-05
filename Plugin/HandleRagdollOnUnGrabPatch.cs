using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;
using ThunderRoad;
using UnityEngine;

namespace NonSnappingLimbs
{
    [HarmonyPatch(typeof(Handle), "OnUnGrab")]
    class OnUnGrabBasePatch
    {
        [HarmonyReversePatch]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void OnUnGrab(Handle instance, RagdollHand ragdollHand, bool throwing)
        {
            Logger.Basic("Handle OnUnGrab");
        }
    }
    [HarmonyPatch(typeof(HandleRagdoll))]
    [HarmonyPatch("OnUnGrab")]
    [HarmonyPatch(new Type[] { typeof(RagdollHand), typeof(bool) })]
    public class HandleRagdollOnUnGrabPatch
    {
        public static bool Prefix(HandleRagdoll __instance, RagdollHand ragdollHand, bool throwing)
        {
            if(__instance.ragdollPart.isSliced)
            {
                OnUnGrabBasePatch.OnUnGrab(__instance, ragdollHand, throwing);
                return false;
            }
            return true;
        }
    }
}
