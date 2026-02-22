using Photon.Deterministic;
using System;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    [AddTypeMenu(menuName: "Wall/Snap To Wall")]
    public unsafe partial class SnapToWall : HNSFStateAction
    {
        public enum ModifyType
        {
            SET,
            MoveTowards
        }
        
        public ModifyType modifyType;
        public FP moveSpeed = 1;
        public FP fudging = FP._0_10;
        
        public bool setRotation;
        
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            if (!frame.Unsafe.TryGetPointer<Transform2D>(entity, out var transform)
                || !frame.Unsafe.TryGetPointer<KCC2D>(entity, out var kcc)
                || !frame.Unsafe.TryGetPointer<GotWallInfo>(entity, out var gwi)
                || !frame.Unsafe.TryGetPointer<ECB>(entity, out var ecb)
                || !frame.TryFindAsset(kcc->Config, out var kccSettings)) return false;
            
            var adjustedNormal = gwi->wallNormal;

            //if (setRotation) transform->Rotation = FPQuaternion.LookRotation(-gwi->wallNormal, FPVector3.ProjectOnPlane(transform->Up, gwi->wallNormal));
            
            FPVector2 midPoint = transform->TransformDirection(ecb->offset + new FPVector2(0, (ecb->height / FP._2)));
            
            FPVector2 newPosition =
                gwi->wallPoint
                + (adjustedNormal * ecb->radius)
                - midPoint;
            
            if (FPVector2.DistanceSquared(transform->Position, newPosition) <= fudging)
            {
                return false;
            }
            
            switch (modifyType)
            {
                case ModifyType.SET:
                    transform->Position = newPosition;
                    break;
                case ModifyType.MoveTowards:
                    transform->Position = FPVector2.MoveTowards(transform->Position, newPosition, moveSpeed * frame.DeltaTime);
                    break;
            }
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