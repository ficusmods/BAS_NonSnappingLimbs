using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ThunderRoad;
using UnityEngine;

namespace NonSnappingLimbs
{
    public class LoadModule : LevelModule
    {

        public string mod_version = "0.0";
        public string mod_name = "UnnamedMod";
        public override IEnumerator OnLoadCoroutine()
        {
            Logger.mod_version = mod_version;
            Logger.modname = mod_name;
            Logger.Msg("Loading " + Logger.modname);
            EventManager.onCreatureSpawn += EventManager_onCreatureSpawn;
            return base.OnLoadCoroutine();
        }

        private void EventManager_onCreatureSpawn(Creature creature)
        {
            if (!creature.gameObject.TryGetComponent<UndyingRagdoll>(out UndyingRagdoll ur))
            {
                Logger.Msg(String.Format("Adding component to {0} ({1}, {2})", creature.name, creature.creatureId, creature.GetInstanceID()));
                creature.gameObject.AddComponent<UndyingRagdoll>();
            }
        }
    }
}
