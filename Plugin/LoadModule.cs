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
        public string logger_level = "Basic";

        public bool dieOnHeadChop
        {
            get => Config.dieOnHeadChop;
            set => Config.dieOnHeadChop = value;
        }

        public bool destabilizeOneLeg
        {
            get => Config.destabilizeOneLeg;
            set => Config.destabilizeOneLeg = value;
        }


        public override IEnumerator OnLoadCoroutine()
        {
            Logger.init(mod_name, mod_version, logger_level);

            Logger.Basic("Loading " + mod_name);
            EventManager.onCreatureSpawn += EventManager_onCreatureSpawn;
            return base.OnLoadCoroutine();
        }

        private void EventManager_onCreatureSpawn(Creature creature)
        {
            if (!creature.isPlayer)
            {
                if (!creature.gameObject.TryGetComponent<UndyingRagdoll>(out UndyingRagdoll ur))
                {
                    Logger.Detailed(String.Format("Adding component to {0} ({1}, {2})", creature.name, creature.creatureId, creature.GetInstanceID()));
                    var obj = creature.gameObject.AddComponent<UndyingRagdoll>();
                }
            }
        }
    }
}
