using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ThunderRoad;
using ThunderRoad.AI;
using UnityEngine;
using HarmonyLib;

namespace NonSnappingLimbs
{
    public class LoadModule : ThunderScript
    {

        public string mod_version = "1.10";
        public string mod_name = "NonSnappingLimbs";
        public string logger_level = "Basic";

        public override void ScriptEnable()
        {
            base.ScriptEnable();
            Logger.init(mod_name, mod_version, logger_level);

            Logger.Basic("Loading " + mod_name);
            ChangeUnarmedTree();
            Harmony harmony = new Harmony("cafe.ficus.nslpatch");
            harmony.PatchAll();
            EventManager.onCreatureSpawn += EventManager_onCreatureSpawn;
        }

        public override void ScriptDisable()
        {
            base.ScriptDisable();
            EventManager.onCreatureSpawn-= EventManager_onCreatureSpawn;
        }

        private void ChangeUnarmedTree()
        {
            BehaviorTreeData unarmedTree = Catalog.GetData<BehaviorTreeData>("HumanUnarmed");
            if (unarmedTree == null) return;
            try
            {
                ThunderRoad.AI.Control.Sequence sequence =
                    (ThunderRoad.AI.Control.Sequence)(((ThunderRoad.AI.Control.Selector)((ThunderRoad.AI.Decorator.IfCondition)unarmedTree.rootNode).child).childs[0]);
                ThunderRoad.AI.Get.GetItem getItem = sequence.childs[3] as ThunderRoad.AI.Get.GetItem;
                ThunderRoad.AI.Decorator.IfCondition isCutOff = new ThunderRoad.AI.Decorator.IfCondition(sequence);
                isCutOff.ifNotConditions.Add(new AI_IsLimbCutOff());
                isCutOff.child = getItem;
                sequence.childs[3] = isCutOff;
            }
            catch(Exception e)
            {
                Logger.Basic("Failed to change the HumanUnarmed tree. Probably a conflict with some other mod or the game has been updated.");
            }
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
