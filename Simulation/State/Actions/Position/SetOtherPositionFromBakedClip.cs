using Photon.Deterministic;
using System;
using HnSF;
using HnSF.core.state;
using HnSF.core.state.actions;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    [AddTypeMenu(menuName: "Position/Set Other Position from Baked Clip")]
    public unsafe partial class SetOtherPositionFromBakedClip : HNSFStateAction
    {
        public int throweeId;
        
        // SELF
        public AssetRef<Tag> self_BoneTag;
        public AssetRef<AnimationClipBakedData> self_clipBakedData;
        public int self_FrameOffset;
        
        // Other
        public AssetRef<Tag> other_AnimTargetTag;
        public AssetRef<Tag> other_BoneTag;
        public int other_FrameOffset;
        public FPVector2 offset;
        
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            if (!frame.Unsafe.TryGetPointer<Transform2D>(entity, out var originPointTransform)) return false;

            var targetEntityRef = GetActionTargetEntityRef(frame, entity);
            if (targetEntityRef == EntityRef.None) return false;
            
            DoAction(frame, entity, rangePercent, targetEntityRef, ref stateContext);
            return false;
        }

        private void DoAction(Frame frame, EntityRef entity, FP rangePercent, EntityRef targetEntityRef, ref HNSFStateContext stateContext)
        {
            var realFrame = FPMath.Clamp(stateContext.stateFrame + self_FrameOffset, 0, FP.UseableMax);
            
            if (!frame.Unsafe.TryGetPointer<Transform2D>(targetEntityRef, out var throweeTransform)
                || !frame.Unsafe.TryGetPointer<FacingDirection>(targetEntityRef, out var throweeFacingDirection)
                || !frame.Unsafe.TryGetPointer<BattleActorAnimator>(targetEntityRef, out var target_ActorAnimation)
                || !frame.TryFindAsset<AnimationEntry>(target_ActorAnimation->state.layers[0].animationEntry,
                    out var target_AnimationEntry)
                || !frame.TryFindAsset(target_AnimationEntry.GetAnimTargetBakedClipData(other_AnimTargetTag),
                    out var target_BakedClipData)
                || !target_BakedClipData.TryGetClosestFrame(realFrame / (FP)60, other_BoneTag, out var other_AnimationFrame))
                return;

            if (!frame.Unsafe.TryGetPointer<Transform2D>(entity, out var selfTransform)
                || !frame.Unsafe.TryGetPointer<FacingDirection>(entity, out var selfFacingDirection)
                || !frame.TryFindAsset(self_clipBakedData, out var self_BakedClipData)
                || !self_BakedClipData.TryGetClosestFrame(realFrame / (FP)60, self_BoneTag, out var self_AnimationFrame))
                return;

            // Target origin as our origin
            throweeTransform->Position = selfTransform->Position;
            throweeTransform->Rotation = -selfTransform->Rotation;
            throweeFacingDirection->isFacingRight = !selfFacingDirection->isFacingRight;
            
            // Offset target origin to our bone
            throweeTransform->Position += selfFacingDirection->TransformDirection(self_AnimationFrame.Position.XY);

            other_AnimationFrame.Position.Z *= -1;
            // Offset target origin to their bone
            throweeTransform->Position -= throweeFacingDirection->TransformDirection(other_AnimationFrame.Position.XY)
                                            + throweeFacingDirection->TransformDirection(offset);
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new SetOtherPositionFromBakedClip());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as SetOtherPositionFromBakedClip;
            t.throweeId = throweeId;
            t.self_BoneTag = self_BoneTag;
            t.self_clipBakedData = self_clipBakedData;
            t.self_FrameOffset = self_FrameOffset;
            t.other_AnimTargetTag = other_AnimTargetTag;
            t.other_BoneTag = other_BoneTag;
            t.other_FrameOffset = other_FrameOffset;
            t.offset = offset;
            return base.CopyTo(target);
        }
    }
}