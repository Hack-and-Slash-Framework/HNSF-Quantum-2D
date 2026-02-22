using Photon.Deterministic;
using System;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    [AddTypeMenu(menuName: "Fighter/Movement/Apply Gravity")]
    public unsafe partial class ApplyGravity : HNSFStateAction
    {

        public HNSFParamFP gravity;
        public HNSFParamFP maxFallSpeed;
        public bool applyCurve;
        public AssetRef<AnimationCurveAsset> gravityCurve;

        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            BattleActorPhysics* physics = frame.Unsafe.GetPointer<BattleActorPhysics>(entity);
            
            physics->force.Y = MoveTowards(
                physics->force.Y, 
                -maxFallSpeed.Resolve(frame,entity, ref stateContext),
                gravity.Resolve(frame, entity, ref stateContext) * frame.DeltaTime);
            return false;
        }
        
        private FP MoveTowards(FP current, FP target, FP maxDelta)
        {
            if (FPMath.Abs(target - current) <= maxDelta) return target;
            return current + FPMath.Sign(target - current) * maxDelta;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new ApplyGravity());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as ApplyGravity;
            t.gravity = gravity.Clone() as HNSFParamFP;
            t.maxFallSpeed = maxFallSpeed.Clone() as HNSFParamFP;
            t.applyCurve = applyCurve;
            t.gravityCurve = gravityCurve;
            return base.CopyTo(target);
        }
    }
}