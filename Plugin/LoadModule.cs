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

        public override IEnumerator OnLoadCoroutine(Level level)
        {
            EventManager.onCreatureSpawn += EventManager_onCreatureSpawn;
            return base.OnLoadCoroutine(level);
        }

        private void EventManager_onCreatureSpawn(Creature creature)
        {
            if (!creature.gameObject.TryGetComponent<UndyingRagdoll>(out UndyingRagdoll ur))
            {
                creature.gameObject.AddComponent<UndyingRagdoll>();
            }
        }
    }
}
