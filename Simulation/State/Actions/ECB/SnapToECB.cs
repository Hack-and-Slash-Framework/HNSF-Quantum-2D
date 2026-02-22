using Photon.Deterministic;
using System;
using HnSF.core.state;
using HnSF.core.state.actions;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    [AddTypeMenu(menuName: "Physics/ECB/Snap to ECB")]
    public unsafe partial class SnapToECB : HNSFStateAction
    {
        public bool asTeleport = true;
        
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            if (!frame.Unsafe.TryGetPointer<ECB>(entity, out var ecb)
                || !frame.Unsafe.TryGetPointer<Transform2D>(entity, out var transform)) return false;

            var vTarget = new FPVector2(ecb->offset.X, (ecb->offset.Y) - (ecb->height / FP._2) );
            
            if(asTeleport) transform->Teleport(frame, transform->Position + vTarget);
            else transform->Position += vTarget;
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new SnapToECB());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as SnapToECB;
            t.asTeleport = asTeleport;
            return base.CopyTo(target);
        }
    }
}