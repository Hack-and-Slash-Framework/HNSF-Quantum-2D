using System;
using Quantum;

namespace HnSF.core.state.decisions
{
    [Serializable]
    public unsafe partial class IsPhysicsGrounded : HNSFStateDecision
    {
        public enum CheckType
        {
            IsGrounded,
            IsAerial
        }

        public CheckType checkType;
        public bool checkLastFrame;

        public override bool Decide(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            if (frame.Unsafe.TryGetPointer<KCC2D>(entity, out var cc2d))
            {
                switch (checkType)
                {
                    case CheckType.IsGrounded:
                        return cc2d->State == KCCState.GROUNDED;
                    case CheckType.IsAerial:
                        return cc2d->State != KCCState.GROUNDED;
                }
            }
            return false;
        }

        public override HNSFStateDecision Copy()
        {
            return CopyTo(new IsPhysicsGrounded());
        }

        public override HNSFStateDecision CopyTo(HNSFStateDecision target)
        {
            var t = target as IsPhysicsGrounded;
            t.checkType = checkType;
            t.checkLastFrame = checkLastFrame;
            return base.CopyTo(target);
        }
    }
}