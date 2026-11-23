using Photon.Deterministic;
using System;
using System.Linq;
using Quantum;
using UnityEngine.Serialization;
#if QUANTUM_UNITY
using UnityEngine.Scripting.APIUpdating;
#endif

namespace HnSF.core.state.actions
{
    [Serializable]
#if QUANTUM_UNITY
    [MovedFrom(autoUpdateAPI: false, sourceClassName: "CreateWarningbox")]
#endif
    public unsafe partial class CreateTelegraphbox : HNSFStateAction
    {
        [FormerlySerializedAs("warningboxIdentifier")] public int boxIdentifier;
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
            
            var boxList = frame.ResolveList(boxCombatant->telegraphboxList);

            if (boxCombatant->TelegraphboxExistWithId(frame, boxIdentifier))
            {
                Log.Debug($"Telegraphbox of id {boxIdentifier} already exist on entity {entity.ToString()}. Error came from state {frame.FindAsset<HNSFState>(stateContext.workingState).Label}");
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
            
            var boxEntity = frame.Create();
            
            var boxPhysicsCollider = new PhysicsCollider2D
            {
                Layer = frame.SimulationConfig.layerTelegraphbox,
                IsTrigger = true,
                Shape = shape
            };

            frame.Add(boxEntity, new Telegraphbox() { active = true, owner = entity, damageSourceInfoRef = new AssetRef(damageSourceInfo.Id), isThrow = isThrow, id = boxIdentifier });
            frame.Add(boxEntity, new Transform2D(){ Position = transform->Position + faceDir->TransformDirection(realOffset), Rotation = transform->Rotation + (realRotation * FP.Deg2Rad)});
            frame.Add(boxEntity, new Parented2D() { parent = entity, localOffset = faceDir->TransformDirection(realOffset), localRotation = realRotation });
            frame.Add(boxEntity, boxPhysicsCollider);
            
            boxList.Add(boxEntity);
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new CreateTelegraphbox());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as CreateTelegraphbox;
            t.boxIdentifier = boxIdentifier;
            t.damageSourceInfo = damageSourceInfo;
            t.isThrow = isThrow;
            t.ignoreScale = ignoreScale;
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
