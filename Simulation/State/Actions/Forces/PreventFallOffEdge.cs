using Photon.Deterministic;
using System;
using HnSF.core.state;
using HnSF.core.state.actions;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    [AddTypeMenu(menuName: "Movement/Prevent Fall Off Edge")]
    public unsafe partial class PreventFallOffEdge : HNSFStateAction
    {
        public FP multi = 1;
        public LayerMask layerMask;
        public FP distanceModi = FP._0_10;
        
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            if (!frame.Unsafe.TryGetPointer<BattleActorPhysics>(entity, out var bap)
                || !frame.Unsafe.TryGetPointer<ECB>(entity, out var ecb)
                || !frame.Unsafe.TryGetPointer<Transform2D>(entity, out var entityTransform)) return false;

            var force = FPMath.Abs(bap->force.X * frame.DeltaTime);
            
            if (force <= FP.SmallestNonZero) return false;

            var forceDir = bap->force.X > 0 ? 1 : -1;

            var h1 = frame.Physics2D.Raycast(
                entityTransform->Position + ecb->offset,
                FPVector2.Right * forceDir,
                force, 
                layerMask);
            if (h1.HasValue) return false;
            
            var h2 = frame.Physics2D.Raycast(
                entityTransform->Position + ecb->offset + (FPVector2.Right * forceDir * force * multi),
                FPVector2.Down,
                (ecb->height / FP._2) + distanceModi,
                layerMask);
            if (h2.HasValue) return false;

            bap->force.X = 0;
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new PreventFallOffEdge());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as PreventFallOffEdge;
            t.multi = multi;
            t.layerMask = layerMask;
            return base.CopyTo(target);
        }
    }
}