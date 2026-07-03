using Photon.Deterministic;
using System;
using HnSF;
using HnSF.core.state;
using HnSF.core.state.actions;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    [AddTypeMenu(menuName: "Hitbox/Set Hitbox Position From Baked Clip")]
    public unsafe partial class SetHitboxPositionFromBakedClip : HNSFStateAction
    {
        public int hitboxIdentifier;

        public AssetRef<AnimationEntryBakedData> bakedClipAssetRef;
        public AssetRef<Tag> boneTag;
        public FP multi = 1;

        public int startOffset = 0;

        public bool useSubframe = true;
        public bool useRotation = true;
        
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            if (!frame.TryFindAsset(bakedClipAssetRef, out var bakedClipAsset)
                || !frame.Unsafe.TryGetPointer<BoxCombatant>(entity, out var boxCombatant)
                || !boxCombatant->TryGetHitbox(frame, hitboxIdentifier, out var hitboxIndex)) return false;
            
            var faceDir = frame.Unsafe.GetPointer<FacingDirection>(entity);

            var hitboxList = frame.ResolveList(boxCombatant->hitboxList);
            var hitboxParented = frame.Unsafe.GetPointer<Parented2D>(hitboxList[hitboxIndex]);
            
            if (useSubframe)
            {
                if (!bakedClipAsset.TryGetFrameAtTime(((FP)(stateContext.stateFrame - startOffset) /
                                                      (FP)bakedClipAsset.FrameRate) * multi, boneTag, out var animationFrame))
                    return false;

                hitboxParented->localOffset = new FPVector2(animationFrame.Position.X * faceDir->GetFacingMulti(), animationFrame.Position.Y);
                if(useRotation) hitboxParented->localRotation = animationFrame.RotationY+90;
            }
            else
            {
                if (!bakedClipAsset.TryGetClosestFrame(((FP)(stateContext.stateFrame - startOffset) /
                                                      (FP)bakedClipAsset.FrameRate) * multi, boneTag, out var animationFrame))
                    return false;
                
                hitboxParented->localOffset = new FPVector2(animationFrame.Position.X * faceDir->GetFacingMulti(), animationFrame.Position.Y);
                if(useRotation) hitboxParented->localRotation = animationFrame.RotationY+90;
            }
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new SetHitboxPositionFromBakedClip());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as SetHitboxPositionFromBakedClip;
            t.hitboxIdentifier = hitboxIdentifier;
            t.bakedClipAssetRef = bakedClipAssetRef;
            t.boneTag = boneTag;
            t.multi = multi;
            t.startOffset = startOffset;
            t.useSubframe = useSubframe;
            t.useRotation = useRotation;
            return base.CopyTo(target);
        }
    }
}