using Photon.Deterministic;
using System;
using HnSF.core.state;
using HnSF.core.state.actions;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    [AddTypeMenu(menuName: "Fighter/Movement/Clamp Forces")]
    public unsafe partial class ClampForces : HNSFStateAction
    {
        public enum ForceGroupType
        {
            Movement,
            Gravity,
            Both
        }

        public ForceGroupType forceToClamp;
        public bool useMaxAsMin;
        public bool makeMinNegative;
        public HNSFParamFP minMagnitude;
        public HNSFParamFP maxMagnitude;

        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            BattleActorPhysics* physics = frame.Unsafe.GetPointer<BattleActorPhysics>(entity);
            
            var clampMagnitude = maxMagnitude.Resolve(frame, entity, ref stateContext);
            var minClamp = clampMagnitude;
            if(!useMaxAsMin) minClamp = minMagnitude.Resolve(frame, entity, ref stateContext);;
            if (makeMinNegative) minClamp *= -1;

            switch (forceToClamp)
            {
                case ForceGroupType.Movement:
                    physics->force.X = FPMath.Clamp(physics->force.X, minClamp, clampMagnitude);
                    break;
                case ForceGroupType.Gravity:
                    physics->force.Y = FPMath.Clamp(physics->force.Y, minClamp, clampMagnitude);
                    break;
                case ForceGroupType.Both:
                    physics->force = FPVector2.ClampMagnitude(physics->force, clampMagnitude);
                    break;
            }
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new ClampForces());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as ClampForces;
            t.forceToClamp = forceToClamp;
            t.useMaxAsMin = useMaxAsMin;
            t.makeMinNegative = makeMinNegative;
            t.minMagnitude = minMagnitude.Clone() as HNSFParamFP;
            t.maxMagnitude = maxMagnitude.Clone() as HNSFParamFP;
            return base.CopyTo(target);
        }
    }
}