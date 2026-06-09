using Photon.Deterministic;
using System;
using System.Linq;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    public unsafe partial class CreateAndDestroyWarningbox : HNSFStateAction
    {
        public bool autoDelete = true;
        public int warningboxIdentifier;
        public bool ignoreScale;
        public AssetRef damageSourceInfo;
        public bool isThrow;
        
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
                boxCombatant->DeleteWarningboxByID(frame, warningboxIdentifier);
                return false;
            }
            if (boxCombatant->WarningboxExistWithId(frame, warningboxIdentifier)) return false;
            
            var warningboxList = frame.ResolveList(boxCombatant->warningboxList);
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
                
            var warningboxPhysicsCollider = new PhysicsCollider2D
            {
                Layer = frame.Layers.GetLayerIndex("Warningbox"),
                IsTrigger = true,
                Shape = shape
            };
                
            var warningboxEntity = frame.Create();
            frame.Add(warningboxEntity, new Warningbox() { active = true, owner = entity, damageSourceInfoRef = new AssetRef(damageSourceInfo.Id), isThrow = isThrow, id = warningboxIdentifier });
            frame.Add(warningboxEntity, new Transform2D(){ Position = transform->Position + faceDir->TransformDirection(realOffset), Rotation = transform->Rotation + (realRotation * FP.Deg2Rad)});
            frame.Add(warningboxEntity, new Parented2D() { parent = entity, localOffset = faceDir->TransformDirection(realOffset), localRotation = realRotation });
            frame.Add(warningboxEntity, warningboxPhysicsCollider);
            warningboxList.Add(warningboxEntity);
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new CreateAndDestroyWarningbox());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as CreateAndDestroyWarningbox;
            t.autoDelete = autoDelete;
            t.warningboxIdentifier = warningboxIdentifier;
            t.ignoreScale = ignoreScale;
            t.damageSourceInfo = damageSourceInfo;
            t.isThrow = isThrow;
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
