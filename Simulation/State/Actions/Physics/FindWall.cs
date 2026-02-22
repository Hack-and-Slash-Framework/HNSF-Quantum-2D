using Photon.Deterministic;
using System;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    [AddTypeMenu(menuName: "Wall/Find Wall")]
    public unsafe partial class FindWall : HNSFStateAction
    {
        public enum DirType
        {
            FacingDirection,
            MovingInput,
            MovementDirection,
        }
        
        public bool clearWallInfo;
        public LayerMask raycastMask;
        public FPVector2 offset;
        public FP raycastDistance = 1;
        public DirType direction;
        public FP validWallAngle = 20;
        
        public FP distanceFudge = 0;
        
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            if (!frame.Unsafe.TryGetPointer<Transform2D>(entity, out var transform2D)
                || !frame.Unsafe.TryGetPointer<ECB>(entity, out var ecb)) return false;
            var hadWallInfo = frame.Unsafe.TryGetPointer<GotWallInfo>(entity, out var gwi);
            
            var inputDir = FPVector2.Zero;

            switch (direction)
            {
                case DirType.FacingDirection:
                    if (frame.Unsafe.TryGetPointer<FacingDirection>(entity, out var fd))
                    {
                        inputDir.X = fd->GetFacingMulti();
                    }
                    break;
                case DirType.MovingInput:
                    if (frame.Unsafe.TryGetPointer<ActorInputBufferMovement>(entity, out var ci))
                    {
                        inputDir.X = ci->GetMovement(0).X;
                    }
                    break;
                case DirType.MovementDirection:
                    if (frame.Unsafe.TryGetPointer<BattleActorPhysics>(entity, out var ccc2d))
                    {
                        inputDir.X = ccc2d->GetOverallVelocity(frame, entity).X;
                    }
                    break;
            }

            if (inputDir == FPVector2.Zero)
            {
                if (clearWallInfo) frame.Remove<GotWallInfo>(entity);
                return false;
            }
            inputDir = inputDir.Normalized;
            
            var raycastHit = frame.Physics2D.Raycast(
                transform2D->Position + ecb->offset,
                inputDir,
                raycastDistance,
                raycastMask,
                options: QueryOptions.HitStatics | QueryOptions.ComputeDetailedInfo
                );

            if (raycastHit.HasValue)
            {
                if (FPVector2.Angle(-inputDir, raycastHit.Value.Normal) <= validWallAngle)
                {
                    if (hadWallInfo && FPVector2.DistanceSquared(raycastHit.Value.Point, gwi->wallPoint) <=
                        (distanceFudge * distanceFudge))
                    {
                        return false;
                    }
                    
                    frame.AddOrGet<GotWallInfo>(entity, out gwi);
                    gwi->wallPoint = raycastHit.Value.Point;
                    gwi->wallNormal = raycastHit.Value.Normal;
                    return false;
                }
            }

            if (clearWallInfo) frame.Remove<GotWallInfo>(entity);
            
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new FindWall());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as FindWall;
            
            return base.CopyTo(target);
        }
    }
}