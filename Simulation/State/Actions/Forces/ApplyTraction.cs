using Photon.Deterministic;
using System;
using HnSF.core.state;
using HnSF.core.state.actions;
using Quantum;
using UnityEngine;

namespace HnSF.core.state.actions
{
    [Serializable]
    [AddTypeMenu(menuName: "Fighter/Movement/Apply Traction")]
    public unsafe partial class ApplyTraction : HNSFStateAction
    {
        public enum TractionType
        {
            Movement,
            FallSpeed,
            Both
        }

        public enum ModifierType
        {
            Add,
            Multiply
        }

        public enum CurveTimeType
        {
            StateTime,
            ActionRange
        }

        public TractionType tractionType;
        public ModifierType modifierType;
        public HNSFParamFP traction;
        public HNSFParamFP multiplier = FP._1;
        public bool useCurve;
        [DrawIf(nameof(useCurve), true)]
        public HNSFParamAssetRef tractionMultiplierCurve;
        [DrawIf(nameof(useCurve), true)]
        public CurveTimeType tractionCurveTimeType = CurveTimeType.StateTime;

        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            if (!frame.Unsafe.TryGetPointer<BattleActorPhysics>(entity, out var physics)) return false;

            FP t = traction.Resolve(frame, entity, ref stateContext) * multiplier.Resolve(frame, entity, ref stateContext);

            if (useCurve &&
                frame.TryFindAsset<AnimationCurveAsset>(
                    tractionMultiplierCurve.Resolve(frame, entity, ref stateContext), out var ac))
            {
                switch (tractionCurveTimeType)
                {
                    case CurveTimeType.StateTime:
                        var sTotalFrames = frame.FindAsset(stateContext.workingState).totalFrames;
                        t *= ac.animationCurve.Evaluate((FP)stateContext.stateFrame / (FP)sTotalFrames);
                        break;
                    case CurveTimeType.ActionRange:
                        t *= ac.animationCurve.Evaluate(rangePercent);
                        break;
                }
            }

            if (t == FP._0) return false;
            
            switch (tractionType)
            {
                case TractionType.Movement:
                    switch (modifierType)
                    {
                        case ModifierType.Add:
                            physics->force.X = MoveTowards(physics->force.X, 0, t * frame.DeltaTime);
                            //kcc->SetKinematicVelocity(FPVector3.MoveTowards(kcc->Data.KinematicVelocity, FPVector3.Zero, t * frame.DeltaTime));
                            break;
                        case ModifierType.Multiply:
                            physics->force.X *= t;
                            //kcc->SetKinematicVelocity(kcc->Data.KinematicVelocity * t);
                            break;
                    }
                    break;
                case TractionType.FallSpeed:
                    physics->force.Y = MoveTowards(physics->force.Y, 0, t * frame.DeltaTime);
                    /*
                    var dynamicVelo = kcc->Data.DynamicVelocity;
                    dynamicVelo.Y = MoveTowards(dynamicVelo.Y, 0, t * frame.DeltaTime);
                    kcc->Data.DynamicVelocity = dynamicVelo;*/
                    break;
                case TractionType.Both:
                    //movement = FPVector3.MoveTowards(movement, FPVector3.Zero, t * frame.DeltaTime);
                    break;
            }
            return false;
        }
        
        private FP MoveTowards(FP current, FP target, FP maxDelta)
        {
            if (FPMath.Abs(target - current) <= maxDelta) return target;
            return current + FPMath.Sign(target - current) * maxDelta;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new ApplyTraction());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as ApplyTraction;
            t.tractionType = tractionType;
            t.modifierType = modifierType;
            t.traction = traction.Clone() as HNSFParamFP;
            t.multiplier = multiplier.Clone() as HNSFParamFP;
            return base.CopyTo(target);
        }
    }
}