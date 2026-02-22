using Photon.Deterministic;
using System;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    [AddTypeMenu(menuName: "Position/Set Position")]
    public unsafe partial class SetPosition : HNSFStateAction
    {
        public FPVector2 offset;
        public int throweeId;

        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            if (!frame.Unsafe.TryGetPointer<FacingDirection>(entity, out var facingDir) ||
                !frame.Unsafe.TryGetPointer<Transform2D>(entity, out var originPointTransform)) return false;

            var targetEntityRef = GetActionTargetEntityRef(frame, entity);
            if (targetEntityRef == EntityRef.None) return false;

            if (!frame.Unsafe.TryGetPointer<Transform2D>(targetEntityRef, out var throweeTransform)) return false;
            throweeTransform->Position = originPointTransform->Position + facingDir->TransformDirection(offset);
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new SetPosition());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as SetPosition;
            t.offset = offset;
            t.throweeId = throweeId;
            return base.CopyTo(target);
        }
    }
}