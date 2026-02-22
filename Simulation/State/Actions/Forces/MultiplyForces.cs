using Photon.Deterministic;
using System;
using HnSF.core.state;
using HnSF.core.state.actions;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    [AddTypeMenu(menuName: "Fighter/Movement/Multiply Forces")]
    public unsafe partial class MultiplyForces : HNSFStateAction
    {
        public enum ForceGroupType
        {
            Movement,
            Gravity,
            Both
        }
        
        public ForceGroupType forceToMultiply;
        public HNSFParamFP value;

        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            BattleActorPhysics* physics = frame.Unsafe.GetPointer<BattleActorPhysics>(entity);

            var multiplyValue = value.Resolve(frame, entity, ref stateContext);

            switch (forceToMultiply)
            {
                case ForceGroupType.Movement:
                    physics->force.X *= multiplyValue;
                    break;
                case ForceGroupType.Gravity:
                    physics->force.Y *= multiplyValue;
                    break;
                case ForceGroupType.Both:
                    physics->force *= multiplyValue;
                    break;
            }
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new MultiplyForces());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as MultiplyForces;
            t.forceToMultiply = forceToMultiply;
            t.value = value.Clone() as HNSFParamFP;
            return base.CopyTo(target);
        }
    }
}