using Photon.Deterministic;

namespace Quantum
{
    public static unsafe partial class VisualEffectHelper
    {
        public static bool PlayVisualEffect(Frame frame, PlayVisualEffectRequest request, EntityRef entity,
            FPVector3 closestBodyPosition, bool facingRight = true, bool atClosestBodyPosition = false)
        {
            var vfx = request.GetRngVFX(frame.RNG);
            if (!vfx.vfxReference.IsValid) return false;
            PlayVisualEffect(frame, request, vfx, entity, closestBodyPosition, facingRight, atClosestBodyPosition);
            return true;
        }

        public static void PlayVisualEffect(Frame frame, PlayVisualEffectRequest request,
            PlayVisualEffectRequest.VFXReference vfx, EntityRef entity,
            FPVector3 closestBodyPosition, bool facingRight = true, bool atClosestBodyPosition = false)
        {
            frame.Events.PlayVisualEffectAtLocation2D(
                visualEffectRef: vfx.vfxReference,
                parented: request.parentedToSelf,
                parent: entity,
                positionAsOffset: request.positionAsOffset,
                position: request.positionOffset.XY,
                rotationAsOffset: request.rotationAsOffset,
                rotation: request.rotationOffset.X,
                atClosestBodyPosition: atClosestBodyPosition,
                sourcePosition: closestBodyPosition.XY,
                setRotationToForceDir: request.rotateToMoveForce,
                parentBoneTag: request.parentBoneTag,
                forwardOffset: request.forwardOffset,
                flipped: !facingRight
            );
        }

        public static bool PlayVisualEffect(Frame frame, PlayVisualEffectRequest request, EntityRef entity,
            bool positionAsOffset, FPVector3 position, bool rotationAsOffset, FP rotation,
            FPVector3 closestBodyPosition, bool facingRight = true, bool atClosestBodyPosition = false)
        {
            var vfx = request.GetRngVFX(frame.RNG);
            if (!vfx.vfxReference.IsValid) return false;
            PlayVisualEffect(frame, request, vfx, entity, positionAsOffset, position, rotationAsOffset, rotation,
                closestBodyPosition, facingRight, atClosestBodyPosition);
            return true;
        }

        public static void PlayVisualEffect(Frame frame, PlayVisualEffectRequest request,
            PlayVisualEffectRequest.VFXReference vfx, EntityRef entity,
            bool positionAsOffset, FPVector3 position, bool rotationAsOffset, FP rotation,
            FPVector3 closestBodyPosition, bool facingRight = true, bool atClosestBodyPosition = false)
        {
            frame.Events.PlayVisualEffectAtLocation2D(
                visualEffectRef: vfx.vfxReference,
                parented: request.parentedToSelf,
                parent: entity,
                positionAsOffset: positionAsOffset,
                position: position.XY,
                rotationAsOffset: rotationAsOffset,
                rotation: rotation,
                atClosestBodyPosition: atClosestBodyPosition,
                sourcePosition: closestBodyPosition.XY,
                setRotationToForceDir: request.rotateToMoveForce,
                parentBoneTag: request.parentBoneTag,
                forwardOffset: request.forwardOffset,
                flipped: !facingRight
            );
        }
    }
}