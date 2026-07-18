using Photon.Deterministic;
using System;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    [AddTypeMenu(menuName: "Position/Set Other Position from Baked Clip")]
    public unsafe partial class SetOtherPositionFromBakedClip : HNSFStateAction
    {
        // SELF
        public AssetRef<Tag> self_BoneTag;
        public AssetRef<AnimationEntryBakedData> self_clipBakedData;
        public int self_FrameOffset;

        // Other
        public AssetRef<Tag> other_AnimTargetTag;
        public AssetRef<Tag> other_BoneTag;
        public int other_FrameOffset;
        public FPVector3 offset;
        public bool flipRot;

        public bool useZPosition = true;
        public bool zPositionFlipping;
        
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            if (!frame.Unsafe.TryGetPointer<Transform3D>(entity, out var originPointTransform)) return false;

            var otherTarget = GetActionTargetEntityRef(frame, entity);

            if (frame.Exists(otherTarget)) DoAction(frame, entity, rangePercent, otherTarget, ref stateContext);
            return false;
        }

        private void DoAction(Frame frame, EntityRef entity, FP rangePercent, EntityRef targetEntityRef,
            ref HNSFStateContext stateContext)
        {
            if (!frame.Unsafe.TryGetPointer<BattleActorAnimator>(targetEntityRef, out var target_ActorAnimation))
                return;
            
            var selfRealFrame = FPMath.Clamp(target_ActorAnimation->state.layers[0].frame + self_FrameOffset, 0, FP.UseableMax);
            var otherRealFrame = FPMath.Clamp(target_ActorAnimation->state.layers[0].frame + other_FrameOffset, 0, FP.UseableMax);

            if (!frame.Unsafe.TryGetPointer<Transform2D>(targetEntityRef, out var defenderTransform)
                || !frame.Unsafe.TryGetPointer<FacingDirection>(targetEntityRef, out var defenderFacingDirection)
                || !frame.TryFindAsset<AnimationEntry>(target_ActorAnimation->state.layers[0].animationEntry,
                    out var target_AnimationEntry)
                || !frame.TryFindAsset(target_AnimationEntry.GetAnimTargetBakedClipData(other_AnimTargetTag),
                    out var target_BakedClipData)
                || !target_BakedClipData.TryGetClosestFrame(otherRealFrame / (FP)60, other_BoneTag,
                    out var other_AnimationFrame))
                return;

            if (!frame.Unsafe.TryGetPointer<Transform2D>(entity, out var selfTransform)
                || !frame.Unsafe.TryGetPointer<FacingDirection>(entity, out var selfFacingDirection)
                || !frame.TryFindAsset(self_clipBakedData, out var self_BakedClipData)
                || !self_BakedClipData.TryGetClosestFrame(selfRealFrame / (FP)60, self_BoneTag,
                    out var self_AnimationFrame))
                return;

            var selfPos = selfTransform->Position;
            var correctedPos = selfPos.XYO + selfFacingDirection->TransformDirection(self_AnimationFrame.Position);
            var defenderFinalPos = defenderTransform->Position.XYO +
                                   defenderFacingDirection->TransformDirection(other_AnimationFrame.Position);

            Draw.Circle(selfTransform->Position, FP._0_20, ColorRGBA.Black, true);
            Draw.Circle(correctedPos.XY, FP._0_20, ColorRGBA.Red, true);
            Draw.Circle(defenderFinalPos.XY, FP._0_20, ColorRGBA.Green, true);

            var targetBonePosition = selfTransform->Position.XYO +
                                     selfFacingDirection->TransformDirection(self_AnimationFrame.Position);

            defenderFacingDirection->isFacingRight =
                flipRot ? selfFacingDirection->isFacingRight : !selfFacingDirection->isFacingRight;

            // Move the target root so the selected target bone lands on the selected self bone.
            var finalPos = (targetBonePosition
                            - defenderFacingDirection->TransformDirection(other_AnimationFrame.Position)
                            - defenderFacingDirection->TransformDirection(offset));
            defenderTransform->Position = finalPos.XY;

            // Z Positioning
            if (useZPosition &&
                frame.Unsafe.TryGetPointer<Transform2DVertical>(targetEntityRef,
                    out var defenderVerticalTransform))
            {
                var zPos = useZPosition ? finalPos.Z : 0;
                if (zPositionFlipping) zPos *= defenderFacingDirection->GetFacingMulti();
                        
                defenderVerticalTransform->Position = zPos;
            }
            
            Draw.Circle(
                (defenderTransform->Position.XYO +
                 defenderFacingDirection->TransformDirection(other_AnimationFrame.Position)).XY,
                FP._0_20, ColorRGBA.Magenta, true);
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new SetOtherPositionFromBakedClip());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as SetOtherPositionFromBakedClip;
            t.self_BoneTag = self_BoneTag;
            t.self_clipBakedData = self_clipBakedData;
            t.self_FrameOffset = self_FrameOffset;
            t.other_AnimTargetTag = other_AnimTargetTag;
            t.other_BoneTag = other_BoneTag;
            t.other_FrameOffset = other_FrameOffset;
            t.offset = offset;
            t.useZPosition = useZPosition;
            t.zPositionFlipping = zPositionFlipping;
            return base.CopyTo(target);
        }
    }
}