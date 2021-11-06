using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using UnityEngine;
using ThunderRoad;
using RainyReignGames.MeshDismemberment;


namespace NonSnappingLimbs
{
    public class UndyingRagdoll : MonoBehaviour
    {
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
            //ragdoll.creature.OnKillEvent += Creature_OnKillEvent;
            ragdoll.creature.OnDespawnEvent += Creature_OnDespawnEvent;
        }


        private void revert_changes()
        {
            Ragdoll ragdoll = gameObject.GetComponentInChildren<Ragdoll>();

            ragdoll.creature.groundStabilizationMaxVelocity = original_max_stabilization_velocity;
            ragdoll.creature.stepEnabled = true;

            foreach (RagdollPart part in ragdoll.parts)
            {
                PartNode node = part_tree.getNode(part);
                if (node == null) continue;

                if (node.sliced_off)
                {
                    part.bone.animationJoint.connectedBody = original_connected_bodies[part];
                    part.bone.animation.SetParent(original_animation_parent[part]);
                    part.characterJointLocked = false;
                }
            }
            part_tree.reset_slice_status();
        }
        private void Creature_OnDespawnEvent(EventTime eventTime)
        {
            if (eventTime == EventTime.OnStart)
            {
                revert_changes();
            }
        }

        private void onDestroy()
        {
            revert_changes();
        }

        private void Creature_OnKillEvent(CollisionInstance collisionInstance, EventTime eventTime)
        {
            if (eventTime == EventTime.OnEnd)
            {
                revert_changes();
            }
        }

        Dictionary<RagdollPart, Transform> previous_bone_anim_position = new Dictionary<RagdollPart, Transform>();
        private void Ragdoll_OnStateChange(Ragdoll.State previousState, Ragdoll.State newState, Ragdoll.PhysicStateChange physicStateChange, EventTime eventTime)
        {            
            Ragdoll ragdoll = gameObject.GetComponentInChildren<Ragdoll>();

            foreach (RagdollPart rp in ragdoll.parts)
            {
                PartNode node = part_tree.getNode(rp);
                if (node == null) continue;
                if (node.sliced_off)
                {
                    if (node.slice_root)
                    {
                        rp.bone.animation.SetParent(rp.transform);
                        rp.bone.animation.localPosition = UnityEngine.Vector3.zero;
                        rp.bone.animation.localRotation = Quaternion.identity;
                    }
                    else
                    {
                        rp.bone.animation.SetParent(rp.bone.parent.part.transform);
                        rp.bone.mesh.SetParent(rp.transform, true);
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
                ragdoll.SetState(Ragdoll.State.Destabilized);
                ragdoll.creature.groundStabilizationMaxVelocity = 0.0f;
            }

            if (headGone) ragdoll.creature.stepEnabled = false;
        }

        private void check_handle_release()
        {
            Ragdoll ragdoll = gameObject.GetComponentInChildren<Ragdoll>();

            var leftArm = ragdoll.GetPart(RagdollPart.Type.LeftArm);
            var rightArm = ragdoll.GetPart(RagdollPart.Type.RightArm);
            if (part_tree.getNode(leftArm).sliced_off) ragdoll.creature.handLeft.TryRelease();
            if (part_tree.getNode(rightArm).sliced_off) ragdoll.creature.handRight.TryRelease();
        }

        private void Update()
        {
            Ragdoll ragdoll = gameObject.GetComponentInChildren<Ragdoll>();

            if (ragdoll.creature.isKilled) return;

            check_destabilize(ragdoll);
            check_handle_release();

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
