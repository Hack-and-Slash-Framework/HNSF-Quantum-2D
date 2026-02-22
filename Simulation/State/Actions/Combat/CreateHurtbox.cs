using Photon.Deterministic;
using System;
using System.Linq;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    public unsafe partial class CreateHurtbox : HNSFStateAction
    {
        public int hurtboxIdentifier;
        public int priority;

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
            
            var hitboxList = frame.ResolveList(boxCombatant->hurtboxList);

            if (boxCombatant->HurtboxExistWithId(frame, hurtboxIdentifier))
            {
                Log.Debug($"Hurtbox of id {hurtboxIdentifier} already exist on entity {entity.ToString()}. Error came from state {frame.FindAsset<HNSFState>(stateContext.workingState).Label}");
                return false;
            }

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
            
            var hitboxEntity = frame.Create();
            
            var boxPhysicsCollider = new PhysicsCollider2D
            {
                Layer = frame.Layers.GetLayerIndex("Hurtboxbox"),
                IsTrigger = true,
                Shape = shape
            };

            frame.Add(hitboxEntity, new Hurtbox() { active = true, owner = entity, id = hurtboxIdentifier, hurtboxInfoRef = hurtboxInfoReference, priority = priority});
            frame.Add(hitboxEntity, new Transform2D(){ Position = transform->Position + faceDir->TransformDirection(realOffset), Rotation = transform->Rotation + (realRotation * FP.Deg2Rad)});
            frame.Add(hitboxEntity, new Parented2D() { parent = entity, localOffset = faceDir->TransformDirection(realOffset), localRotation = realRotation });
            frame.Add(hitboxEntity, boxPhysicsCollider);
            
            hitboxList.Add(hitboxEntity);
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new CreateCollisionbox());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as CreateHurtbox;
            t.hurtboxIdentifier = hurtboxIdentifier;
            t.priority = priority;
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
