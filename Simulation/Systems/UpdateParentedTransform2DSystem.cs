using Photon.Deterministic;
using Quantum;

namespace HnSF.core.systems
{
    public unsafe class UpdateParentedTransform2DSystem : SystemMainThreadFilter<UpdateParentedTransform2DSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public Parented2D* Parented;
            public Transform2D* Transform;
        }

        public override void Update(Frame f, ref Filter filter)
        {
            if (!f.Unsafe.TryGetPointer<Transform2D>(filter.Parented->parent, out var parentTransform)) return;
            
            filter.Transform->Position = parentTransform->Position + parentTransform->TransformDirection(filter.Parented->localOffset);
            filter.Transform->Rotation = parentTransform->Rotation + (filter.Parented->localRotation * FP.Deg2Rad);
        }
    }
}