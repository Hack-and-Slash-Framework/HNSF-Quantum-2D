using System;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.decisions
{
    [Serializable]
    public unsafe partial class PhysicsShapeOverlap : HNSFStateDecision
    {
        public LayerMaskParam layerMaskParam;
        public AssetRef<Shape2DConfigOffsetRotation> shapeConfigRef;
        public QueryOptions queryOptions = QueryOptions.HitAll;
        
        public override bool Decide(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            var transform = frame.Unsafe.GetPointer<Transform2D>(entity);

            var layerMask = layerMaskParam.Get(frame);
            var shapeConfig = frame.FindAsset(shapeConfigRef);
            
            var realOffset = shapeConfig.offset;

            if (frame.Unsafe.TryGetPointer<FacingDirection>(entity, out var facingDirection))
            {
                realOffset = facingDirection->TransformDirection(shapeConfig.offset);
            }

            Shape2D s = shapeConfig.shape.CreateShape(frame);

            var hits = frame.Physics2D.OverlapShape(transform->Position + realOffset, transform->Rotation + shapeConfig.rotation, s, layerMask, queryOptions);

            for (int i = 0; i < hits.Count; i++)
            {
                var h = hits[i];

                if (EntityIsSelfOrOwnedBySelf(frame, entity, h.Entity)) continue;
                return true;
            }
            return false;
        }

        private bool EntityIsSelfOrOwnedBySelf(Frame frame, EntityRef entity, EntityRef hEntity)
        {
            if (entity == hEntity) return true;

            // TODO: Support more than 1 layer deep parents.
            if (frame.Unsafe.TryGetPointer<Parented2D>(hEntity, out var par))
            {
                if (par->parent == entity) return true;
            }
            
            return false;
        }

        public override HNSFStateDecision Copy()
        {
            return CopyTo(new PhysicsShapeOverlap());
        }

        public override HNSFStateDecision CopyTo(HNSFStateDecision target)
        {
            var t = target as PhysicsShapeOverlap;
            t.layerMaskParam = layerMaskParam.Clone();
            t.shapeConfigRef = shapeConfigRef;
            t.queryOptions = queryOptions;
            return base.CopyTo(target);
        }
    }
}