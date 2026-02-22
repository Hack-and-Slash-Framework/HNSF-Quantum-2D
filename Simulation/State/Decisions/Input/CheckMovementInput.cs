using System;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.decisions
{
    [Serializable]
    public unsafe partial class CheckMovementInput : HNSFStateDecision
    {
        public enum CheckType
        {
            IsMoving,
            IsNotMoving,
            Values
        }

        public CheckType checkType;
        public FP minValue;
        public FP maxValue;
        public bool alsoCheckLastInput;
        public int lastInputBufferOffset = 0;
        
        public override bool Decide(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            var inputs = frame.Unsafe.GetPointer<ActorInputBufferMovement>(entity);
            if (inputs->disableReadMovement) return false;

            var moveInput = inputs->GetMovement(0);
            var lastMoveInput = inputs->GetMovement(1 + lastInputBufferOffset);
            var movementMagnitude = FPMath.Abs(moveInput.X);
            
            switch (checkType)
            {
                case CheckType.IsMoving:
                    var lastIs = !alsoCheckLastInput || FPMath.Abs(lastMoveInput.X) >= minValue;
                    return movementMagnitude >= minValue && lastIs;
                case CheckType.IsNotMoving:
                    var lastNot = !alsoCheckLastInput || FPMath.Abs(lastMoveInput.X) <= minValue;
                    return movementMagnitude <= minValue && lastNot;
                case CheckType.Values:
                    if (movementMagnitude < (minValue) ||
                        movementMagnitude >= (maxValue)) return false;
                    return true;
            }
            return false;
        }

        public override HNSFStateDecision Copy()
        {
            return CopyTo(new CheckMovementInput());
        }

        public override HNSFStateDecision CopyTo(HNSFStateDecision target)
        {
            var t = target as CheckMovementInput;
            t.checkType = checkType;
            t.minValue = minValue;
            t.maxValue = maxValue;
            t.alsoCheckLastInput = alsoCheckLastInput;
            t.lastInputBufferOffset = lastInputBufferOffset;
            return base.CopyTo(target);
        }
    }
}