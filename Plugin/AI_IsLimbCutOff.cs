using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ThunderRoad;
using ThunderRoad.AI;
using UnityEngine;

namespace NonSnappingLimbs
{
    class AI_IsLimbCutOff : ConditionNode
    {
        protected Side side = Side.Right;
        public override void Init(Creature p_creature, Blackboard p_blackboard)
        {
            base.Init(p_creature, p_blackboard);
            this.creature = p_creature;
        }

        public override bool Evaluate()
        {
            bool ret = false;
            if(side == Side.Right)
            {
                ret = creature.handRight.isSliced;
            }
            else
            {
                ret = creature.handLeft.isSliced;
            }
            return ret;
        }
    }
}
