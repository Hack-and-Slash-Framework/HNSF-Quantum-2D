using Photon.Deterministic;
using System;
using System.Linq;
using HnSF.core.state;
using HnSF.core.state.actions;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    [AddTypeMenu(menuName: "Fighter/Movement/Apply Movement")]
    public unsafe partial class ApplyMovement : HNSFStateAction
    {
        public enum InputSourceType
        {
            none,
            stick,
            rotation,
            slope,
            custom
        }

        public InputSourceType[] inputSources;
        public HNSFParamFP acceleration;
        public HNSFParamFP deceleration;
        public HNSFParamFP decelerationOverMax;
        public HNSFParamFP minSpeed;
        public HNSFParamFP maxSpeed;
        public AssetRef curveRef;
        public HNSFParamFPVector2 customInput;

        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            var facingDir = frame.Unsafe.GetPointer<FacingDirection>(entity);
            BattleActorPhysics* physics = frame.Unsafe.GetPointer<BattleActorPhysics>(entity);
            var inputs = frame.Unsafe.GetPointer<ActorInputBufferMovement>(entity);
            AnimationCurveAsset curve = frame.FindAsset<AnimationCurveAsset>(curveRef.Id);

            FPVector2 input = FPVector2.Zero;

            for (int i = 0; i < inputSources.Length; i++)
            {
                switch (inputSources[i])
                {
                    case InputSourceType.stick:
                        input = inputs->GetMovement(0);
                        input.Y = 0;
                        break;
                    case InputSourceType.rotation:
                        input.X = facingDir->GetFacingMulti();
                        break;
                    case InputSourceType.custom:
                        input = customInput.Resolve(frame, entity, ref stateContext);
                        break;
                }

                if (input != FPVector2.Zero) break;
            }
            
            HandleMovement(frame, entity, physics, frame.DeltaTime, input, 
                acceleration.Resolve(frame, entity, ref stateContext),
                deceleration.Resolve(frame, entity, ref stateContext), 
                decelerationOverMax.Resolve(frame, entity, ref stateContext), 
                minSpeed.Resolve(frame, entity, ref stateContext), 
                maxSpeed.Resolve(frame, entity, ref stateContext),
                curve.animationCurve);
            return false;
        }

        private void HandleMovement(Frame frame, EntityRef entity, BattleActorPhysics* physics, FP deltaTime, FPVector2 movement, 
            FP acceleration, FP deceleration, FP decelerationOverMax, FP minSpeed, FP maxSpeed, FPAnimationCurve accelFromDot)
        {
            movement.Y.RawValue = FP.RAW_ZERO;
            
            if (movement.Magnitude > 1)
            {
                movement = movement.Normalized;
            }
            
            FP realAcceleration = (movement.Magnitude * acceleration);

            // Calculated our wanted movement force.
            FP accel = 0;

            if (FPMath.Abs(physics->force.X) > maxSpeed)
            {
                accel = decelerationOverMax;
            }
            else
            {
                accel = movement == FPVector2.Zero ? deceleration : realAcceleration * accelFromDot.Evaluate(FPVector2.Dot(movement.Normalized, physics->force)); // TODO: Fix dot product to ignore Y.
            }

            FPVector2 goalVelocity = movement.Normalized * FPMath.Lerp(minSpeed, maxSpeed, movement.Magnitude / (FP)1 );

            physics->force.X = MoveTowards(physics->force.X, goalVelocity.X, accel * deltaTime);
        }
        
        FP MoveTowards(FP current, FP target, FP maxDelta)
        {
            if (FPMath.Abs(target - current) <= maxDelta)
                return target;
            return current + FPMath.Sign(target - current) * maxDelta;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new ApplyMovement());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as ApplyMovement;
            t.inputSources = inputSources.ToArray();
            t.acceleration = acceleration.Clone() as HNSFParamFP;
            t.deceleration = deceleration.Clone() as HNSFParamFP;
            t.decelerationOverMax = decelerationOverMax.Clone() as HNSFParamFP;
            t.minSpeed = minSpeed.Clone() as HNSFParamFP;
            t.maxSpeed = maxSpeed.Clone() as HNSFParamFP;
            t.curveRef = curveRef;
            t.customInput = customInput.Clone() as HNSFParamFPVector2;
            return base.CopyTo(target);
        }
    }
}