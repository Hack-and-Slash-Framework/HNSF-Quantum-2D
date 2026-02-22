using Photon.Deterministic;
using System;
using HnSF.core.state;
using HnSF.core.state.actions;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    [AddTypeMenu(menuName: "Position/Modify Z Position")]
    public unsafe partial class ModifyZPosition : HNSFStateAction
    {
        public int throweeId;
        public FP pos;

        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            var targetEntityRef = GetActionTargetEntityRef(frame, entity);
            if (targetEntityRef == EntityRef.None) return false;
            
            if (!frame.Unsafe.TryGetPointer<FacingDirection>(targetEntityRef, out var facingDir) ||
                !frame.Unsafe.TryGetPointer<Transform2DVertical>(targetEntityRef, out var targetTransform)) return false;

            targetTransform->Position = pos;
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new ModifyZPosition());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as ModifyZPosition;
            t.throweeId = throweeId;
            t.pos = pos;
            return base.CopyTo(target);
        }
    }
}