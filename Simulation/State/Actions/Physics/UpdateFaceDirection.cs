using Photon.Deterministic;
using System;
using HnSF.core.state;
using HnSF.core.state.actions;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    public unsafe partial class UpdateFaceDirection : HNSFStateAction
    {
        public enum InputSourceType
        {
            Stick,
            OppositeFaceDirection,
            Custom,
            OppositeOfSelf,
            OppositeOfTargetB
        }

        public InputSourceType inputSource = InputSourceType.Stick;
        public HNSFParamFPVector2 customInput;
        public StateActionTargetContext targetBContext;

        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            HNSFStateContext targetStateContext = stateContext;
            var targetEntityRef = GetActionTargetEntityRef(frame, entity, ref targetStateContext);
            if (targetEntityRef == EntityRef.None) return false;
            DoAction(frame, targetEntityRef, ref targetStateContext);
            return false;
        }

        private void DoAction(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            var charaPhysics = frame.Unsafe.GetPointer<FacingDirection>(entity);
            var inputs = frame.Unsafe.GetPointer<ActorInputBufferMovement>(entity);

            var inp = FPVector2.Zero;

            switch (inputSource)
            {
                case InputSourceType.Stick:
                    inp = inputs->GetMovement(0);
                    break;
                case InputSourceType.OppositeFaceDirection:
                    inp = charaPhysics->TransformDirection(FPVector2.Left);
                    break;
                case InputSourceType.Custom:
                    inp = customInput.Resolve(frame, entity, ref stateContext);
                    break;
                case InputSourceType.OppositeOfSelf:
                    if (frame.Unsafe.TryGetPointer<FacingDirection>(entity, out var fd))
                        inp = fd->TransformDirection(FPVector2.Left);
                    break;
                case InputSourceType.OppositeOfTargetB:
                    var targetBEntityRef = HNSFStateHelper.GetStateTargetEntity(frame, ref targetBContext);
                    if (targetBEntityRef != EntityRef.None
                        && frame.Unsafe.TryGetPointer<FacingDirection>(targetBEntityRef, out var targetBfd))
                    {
                        inp = targetBfd->TransformDirection(FPVector2.Left);
                    }
                    break;
            }

            if (FPMath.Abs(inp.X) <= FP.SmallestNonZero) return;
            charaPhysics->isFacingRight = inp.X > 0 ? true : false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new UpdateFaceDirection());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as UpdateFaceDirection;
            t.inputSource = inputSource;
            t.customInput = customInput.Clone() as HNSFParamFPVector2;
            t.targetBContext = targetBContext;
            return base.CopyTo(target);
        }
    }
}