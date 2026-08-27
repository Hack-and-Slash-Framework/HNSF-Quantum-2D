using Photon.Deterministic;
using System;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    [AddTypeMenu(menuName: "Position/Set Position")]
    public unsafe partial class SetPosition : HNSFStateAction
    {
        public HNSFParamFPVector2 positionParam;
        public int throweeId;

        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            var targetEntityRef = GetActionTargetEntityRef(frame, entity);
            if (targetEntityRef == EntityRef.None) return false;

            HNSFStateContext ctx = new HNSFStateContext(frame, targetEntityRef);
            
            if (!frame.Unsafe.TryGetPointer<Transform2D>(targetEntityRef, out var targetTransform)) return false;
            
            var position = positionParam.Resolve(frame, entity, ref ctx);
            
            targetTransform->Position = position;
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new SetPosition());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as SetPosition;
            t.positionParam = positionParam.Clone() as HNSFParamFPVector2;
            t.throweeId = throweeId;
            return base.CopyTo(target);
        }
    }
}