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
    [HarmonyPatch(typeof(Handle), "OnGrab")]
    class OnGrabBasePatch
    {
        [HarmonyReversePatch]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void OnGrab(Handle instance, RagdollHand ragdollHand, float axisPosition, HandlePose handlePose, bool teleportToHand)
        {
            Logger.Basic("Handle OnGrab");
        }
    }
    [HarmonyPatch(typeof(HandleRagdoll))]
    [HarmonyPatch("OnGrab")]
    [HarmonyPatch(new Type[] { typeof(RagdollHand), typeof(float), typeof(HandlePose), typeof(bool) })]
    public class HandleRagdollOnGrabPatch
    {
        public static bool Prefix(HandleRagdoll __instance, RagdollHand ragdollHand, float axisPosition, HandlePose orientation, bool teleportToHand)
        {
            if(__instance.ragdollPart.isSliced)
            {
                OnGrabBasePatch.OnGrab(__instance, ragdollHand, axisPosition, orientation, teleportToHand);
                return false;
            }
            return true;
        }
    }
}
