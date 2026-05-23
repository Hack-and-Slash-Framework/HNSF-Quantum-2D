using Photon.Deterministic;
using System;
using System.Linq;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    public unsafe partial class CreateAndDestroyHurtbox : HNSFStateAction
    {
        public bool autoDelete = true;
        public int hurtboxIdentifier;
        public int priority;
        public bool ignoreScale;
        public AssetRef<HurtboxInfo> hurtboxInfoReference;
        
        public bool useExternalShapeConfig;
        [DrawIf(nameof(useExternalShapeConfig), true)]
        public AssetRef<Shape2DConfigOffsetRotation> externalShape2DConfigReference;
        [DrawIf(nameof(useExternalShapeConfig), false)]
        public FPVector2 offset;
        [DrawIf(nameof(useExternalShapeConfig), false)]
        public FP rotation;
        [DrawIf(nameof(useExternalShapeConfig), false)]
        public Shape2DConfig shapeConfig = new();
        
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            if (!frame.Unsafe.TryGetPointer<BoxCombatant>(entity, out var boxCombatant)
                || !frame.Unsafe.TryGetPointer<Transform2D>(entity, out var transform)
                || !frame.Unsafe.TryGetPointer<FacingDirection>(entity, out var faceDir)) return false;

            if (autoDelete && rangePercent >= 1)
            {
                boxCombatant->DeleteHurtboxByID(frame, hurtboxIdentifier);
                return false;
            }
            if (boxCombatant->HurtboxExistWithId(frame, hurtboxIdentifier)) return false;
            
            
            var hurtboxList = frame.ResolveList(boxCombatant->hurtboxList);
            Shape2D shape;
            FPVector2 realOffset;
            FP realRotation;
            if (useExternalShapeConfig && frame.TryFindAsset(externalShape2DConfigReference, out var externalShape2DConfig))
            {
                shape = externalShape2DConfig.shape.CreateShape(frame);
                realOffset = externalShape2DConfig.offset;
                realRotation = externalShape2DConfig.rotation;
            }
            else
            {
                shape = shapeConfig.CreateShape(frame);
                realOffset = offset;
                realRotation = rotation;
            }
            
            if (!ignoreScale && frame.Unsafe.TryGetPointer<Scale2D>(entity, out var scale))
            {
                switch (shape.Type)
                {
                    case Shape2DType.Box:
                        shape.Box.Extents = new FPVector2(shape.Box.Extents.X * scale->value.X, shape.Box.Extents.Y * scale->value.Y);
                        break;
                    case Shape2DType.Circle:
                        shape.Circle.Radius *= scale->value.X;
                        break;
                }

                realOffset = new FPVector2(realOffset.X * scale->value.X, realOffset.Y * scale->value.Y);
            }
                
            var boxPhysicsCollider = new PhysicsCollider2D
            {
                Layer = frame.Layers.GetLayerIndex("Hurtbox"),
                IsTrigger = true,
                Shape = shape
            };
                
            var boxEntity = frame.Create();
            frame.Add(boxEntity, new Hurtbox() { active = true, priority = priority, owner = entity, hurtboxInfoRef = hurtboxInfoReference, id = hurtboxIdentifier});
            frame.Add(boxEntity, new Transform2D(){ Position = transform->Position + faceDir->TransformDirection(realOffset), Rotation = transform->Rotation + (realRotation * FP.Deg2Rad)});
            frame.Add(boxEntity, new Parented2D() { parent = entity, localOffset = faceDir->TransformDirection(realOffset), localRotation = realRotation });
            frame.Add(boxEntity, boxPhysicsCollider);
            hurtboxList.Add(boxEntity);
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new CreateAndDestroyHurtbox());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as CreateAndDestroyHurtbox;
            t.autoDelete = autoDelete;
            t.hurtboxIdentifier = hurtboxIdentifier;
            t.priority = priority;
            t.ignoreScale = ignoreScale;
            t.hurtboxInfoReference = hurtboxInfoReference;
            t.useExternalShapeConfig = useExternalShapeConfig;
            t.externalShape2DConfigReference = externalShape2DConfigReference;
            t.offset = offset;
            t.rotation = rotation;
            t.shapeConfig = new Shape2DConfig()
            {
                BoxExtents = shapeConfig.BoxExtents,
                CapsuleSize = shapeConfig.CapsuleSize,
                CircleRadius = shapeConfig.CircleRadius,
                CompoundShapes = shapeConfig.CompoundShapes.ToArray(),
                EdgeExtent = shapeConfig.EdgeExtent,
                IsPersistent = shapeConfig.IsPersistent,
                PolygonCollider = shapeConfig.PolygonCollider,
                PositionOffset = shapeConfig.PositionOffset,
                RotationOffset = shapeConfig.RotationOffset,
                ShapeType = shapeConfig.ShapeType,
                UserTag = shapeConfig.UserTag
            };
            return base.CopyTo(target);
        }
    }
}
