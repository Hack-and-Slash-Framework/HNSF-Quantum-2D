using Photon.Deterministic;
using System;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    [AddTypeMenu(menuName: "Physics/ECB/Set ECB")]
    public unsafe partial class SetECB : HNSFStateAction
    {
        public HNSFParamFP radiusParam;
        public HNSFParamFP heightParam;
        public HNSFParamFPVector2 offsetParam;
        public bool ignoreScale;
        
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            if (!frame.Unsafe.TryGetPointer<ECB>(entity, out var ecb)) return false;

            var radius = radiusParam.Resolve(frame, entity, ref stateContext);
            var height = heightParam.Resolve(frame, entity, ref stateContext);
            var offset = offsetParam.Resolve(frame, entity, ref stateContext);

            if (!ignoreScale && frame.Unsafe.TryGetPointer<Scale2D>(entity, out var scale2D))
            {
                radius *= scale2D->value.X;
                height *= scale2D->value.Y;
                offset = new FPVector2(offset.X * scale2D->value.X, offset.Y * scale2D->value.Y);
            }
            
            ecb->radius = radius;
            ecb->height = height;
            ecb->offset = offset;
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new SetECB());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as SetECB;
            t.radiusParam = radiusParam.Clone() as HNSFParamFP;
            t.heightParam = heightParam.Clone() as HNSFParamFP;
            t.offsetParam = offsetParam.Clone() as HNSFParamFPVector2;
            return base.CopyTo(target);
        }
    }
}