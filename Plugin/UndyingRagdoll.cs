using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using UnityEngine;
using ThunderRoad;


namespace NonSnappingLimbs
{
    public class UndyingRagdoll : MonoBehaviour
    {
        public bool dieOnHeadChop = true;
        public class PartNode
        {
            public RagdollPart part;
            public PartNode parent;
            public List<PartNode> children;
            public bool sliced_off = false;
            public bool slice_root = false;
        }

        public class PartTree
        {
            Dictionary<RagdollPart, PartNode> part_map = new Dictionary<RagdollPart, PartNode>();
            
            public PartTree(RagdollPart _root)
            {
                register(_root);
                root = part_map[_root];
            }

            public PartNode root;

            public void arrange_tree()
            {
                foreach (KeyValuePair<RagdollPart, PartNode> entry in part_map)
                {
                    PartNode node = entry.Value;
                    RagdollPart rp = node.part;

                    node.parent = null;
                    if (rp.parentPart && part_map.ContainsKey(rp.parentPart))
                    {
                        node.parent = part_map[rp.parentPart];
                        if (!node.parent.children.Contains(node))
                            node.parent.children.Add(node);
                    }
                }
            }

            public void register(RagdollPart rp)
            {
                PartNode node = new PartNode();
                node.part = rp;
                node.children = new List<PartNode>();
                if (!part_map.ContainsKey(rp)) part_map[rp] = node;
            }

            public void reset_slice_status()
            {
                root.sliced_off = false;
                root.slice_root = false;
                foreach(var e in getSubNodes(root.part))
                {
                    e.sliced_off = false;
                    e.slice_root = false;
                }
            }

            public PartNode getNode(RagdollPart p)
            {
                if(p && part_map.ContainsKey(p)) return part_map[p];
                return null;
            }

            public List<PartNode> getSubNodes(RagdollPart p)
            {
                if (!p) return null;
                if (!part_map.ContainsKey(p)) return null;

                List<PartNode> ret = new List<PartNode>();
                PartNode node = part_map[p];
                getSubNodes_impl(node, ret);
                return ret;
            }
            private void getSubNodes_impl(PartNode n, List<PartNode> parts)
            {
                foreach(var child in n.children)
                {
                    parts.Add(child);
                    getSubNodes_impl(child, parts);
                }
            }
        }

        PartTree part_tree;
        Dictionary<RagdollPart, Rigidbody> original_connected_bodies = new Dictionary<RagdollPart, Rigidbody>();
        Dictionary<RagdollPart, Transform> original_animation_parent = new Dictionary<RagdollPart, Transform>();
        float original_max_stabilization_velocity;
        private void Awake()
        {
            Ragdoll ragdoll = gameObject.GetComponentInChildren<Ragdoll>();
            Logger.Detailed(String.Format("Applying changes to {0} ({1}, {2})", ragdoll.creature.name, ragdoll.creature.creatureId, ragdoll.creature.GetInstanceID()));

            part_tree = new PartTree(ragdoll.rootPart);
            foreach (RagdollPart rp in ragdoll.parts)
            {
                part_tree.register(rp);
                rp.data.sliceForceKill = false;
                original_connected_bodies[rp] = rp.bone.animationJoint.connectedBody;
                original_animation_parent[rp] = rp.bone.animation.parent;
            }

            original_max_stabilization_velocity = ragdoll.creature.groundStabilizationMaxVelocity;

            part_tree.arrange_tree();

            ragdoll.OnSliceEvent += Ragdoll_OnSliceEvent;
            ragdoll.OnStateChange += Ragdoll_OnStateChange;
            ragdoll.creature.OnDespawnEvent += Creature_OnDespawnEvent;
        }


        private void revert_changes()
        {
            Ragdoll ragdoll = gameObject.GetComponentInChildren<Ragdoll>();
            Logger.Detailed(String.Format("Reverting changes for {0} ({1}, {2})", ragdoll.creature.name, ragdoll.creature.creatureId, ragdoll.creature.GetInstanceID()));

            ragdoll.creature.groundStabilizationMaxVelocity = original_max_stabilization_velocity;
            ragdoll.creature.stepEnabled = true;
            ragdoll.RemovePhysicToggleModifier(this);

            foreach (RagdollPart part in ragdoll.parts)
            {
                PartNode node = part_tree.getNode(part);
                if (node == null) continue;

                if (node.sliced_off)
                {
                    part.bone.animationJoint.connectedBody = original_connected_bodies[part];
                    part.bone.animation.SetParent(original_animation_parent[part]);
                    part.bone.animationJoint.gameObject.SetActive(true);
                    part.characterJointLocked = false;
                }
            }
            part_tree.reset_slice_status();
        }
        private void Creature_OnDespawnEvent(EventTime eventTime)
        {
            if (eventTime == EventTime.OnStart)
            {
                Creature creature = gameObject.GetComponentInChildren<Creature>();
                Logger.Detailed(String.Format("Creature despawned for {0} ({1}, {2})", creature.name, creature.creatureId, creature.GetInstanceID()));
                revert_changes();
            }
        }

        private void onDestroy()
        {
            Creature creature = gameObject.GetComponentInChildren<Creature>();
            Logger.Basic(String.Format("Component destroyed for {0} ({1}, {2})", creature.name, creature.creatureId, creature.GetInstanceID()));
            revert_changes();
        }

        private void Ragdoll_OnStateChange(Ragdoll.State previousState, Ragdoll.State newState, Ragdoll.PhysicStateChange physicStateChange, EventTime eventTime)
        {
            Ragdoll ragdoll = gameObject.GetComponentInChildren<Ragdoll>();

            foreach (RagdollPart rp in ragdoll.parts)
            {
                PartNode node = part_tree.getNode(rp);
                if (node == null) continue;
                if (node.sliced_off)
                {
                    rp.bone.animation.SetParent(rp.transform);
                    rp.bone.animationJoint.gameObject.SetActive(false);
                    rp.bone.animation.localPosition = UnityEngine.Vector3.zero;
                    rp.bone.animation.localRotation = Quaternion.identity;

                    if (!node.slice_root)
                    {
                        rp.bone.mesh.SetParent(rp.transform);
                        rp.bone.mesh.localPosition = UnityEngine.Vector3.zero;
                        rp.bone.mesh.localRotation = Quaternion.identity;
                        rp.bone.mesh.localScale = UnityEngine.Vector3.one;
                    }

                    rp.bone.animation.localScale = UnityEngine.Vector3.one;
                }
            }

        }

        private void Ragdoll_OnSliceEvent(RagdollPart ragdollPart, EventTime eventTime)
        {
            if (eventTime == EventTime.OnEnd)
            {
                ragdollPart.ragdoll.AddPhysicToggleModifier(this);
                PartNode node = part_tree.getNode(ragdollPart);
                if (node != null)
                {
                    node.sliced_off = true;
                    node.slice_root = true;
                }

                foreach (var child in part_tree.getSubNodes(ragdollPart))
                {
                    child.sliced_off = true;
                }
            }
        }

        private void check_destabilize(Ragdoll ragdoll)
        {
            if (ragdoll.state == Ragdoll.State.Destabilized) return;

            bool leftLegGone = false;
            bool rightLegGone = false;
            bool headGone = false;

            foreach (RagdollPart rp in ragdoll.parts)
            {
                if (rp.type == RagdollPart.Type.LeftFoot && part_tree.getNode(rp).sliced_off) leftLegGone = true;
                if (rp.type == RagdollPart.Type.RightFoot && part_tree.getNode(rp).sliced_off) rightLegGone = true;
                if (rp.type == RagdollPart.Type.Head && part_tree.getNode(rp).sliced_off) headGone = true;
            }

            if((leftLegGone && rightLegGone) || headGone)
            {
                Logger.Detailed("Destabilizing creature: {0} ({1}, {2})", ragdoll.creature.name, ragdoll.creature.creatureId, ragdoll.creature.GetInstanceID());
                ragdoll.SetState(Ragdoll.State.Destabilized);
                ragdoll.creature.groundStabilizationMaxVelocity = 0.0f;
            }

            if (headGone) ragdoll.creature.stepEnabled = false;
        }

        private void check_handle_release()
        {
            Ragdoll ragdoll = gameObject.GetComponentInChildren<Ragdoll>();

            var leftHand = ragdoll.GetPart(RagdollPart.Type.LeftHand);
            var rightHand = ragdoll.GetPart(RagdollPart.Type.RightHand);
            if (leftHand && part_tree.getNode(leftHand).sliced_off) ragdoll.creature.handLeft.TryRelease();
            if (rightHand && part_tree.getNode(rightHand).sliced_off) ragdoll.creature.handRight.TryRelease();
        }

        private void check_head_kill()
        {
            if (dieOnHeadChop)
            {
                Ragdoll ragdoll = gameObject.GetComponentInChildren<Ragdoll>();
                if (ragdoll.headPart && part_tree.getNode(ragdoll.headPart).sliced_off)
                {
                    if (ragdoll.creature.maxHealth != float.MaxValue)
                    {
                        Logger.Detailed("Killing creature (head chop): {0} ({1}, {2})", ragdoll.creature.name, ragdoll.creature.creatureId, ragdoll.creature.GetInstanceID());
                        CollisionInstance collision = new CollisionInstance(new DamageStruct(DamageType.Energy, float.MaxValue));
                        ragdoll.creature.Damage(collision);
                    }
                }
            }
        }

        private void Update()
        {
            Ragdoll ragdoll = gameObject.GetComponentInChildren<Ragdoll>();

            if (ragdoll.creature.isKilled) return;

            check_destabilize(ragdoll);
            check_handle_release();
            check_head_kill();

            foreach (RagdollPart rp in ragdoll.parts)
            {
                PartNode node = part_tree.getNode(rp);
                if (node != null)
                {
                    if (node.sliced_off)
                    {
                        rp.rb.isKinematic = false;
                        if (node.slice_root)
                        {
                            rp.DestroyCharJoint();
                            rp.characterJointLocked = true;
                        }

                        rp.transform.SetParent(null);
                        rp.bone.animationJoint.connectedBody = null;

                        rp.transform.localScale = UnityEngine.Vector3.one;

                        if (rp.bone.fixedJoint)
                            Destroy(rp.bone.fixedJoint);

                        rp.collisionHandler.RemovePhysicModifier((object)ragdoll);
                    }
                }
            }
        }
    }
}
