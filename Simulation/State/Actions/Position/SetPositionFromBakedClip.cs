using Photon.Deterministic;
using System;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    [AddTypeMenu(menuName: "Fighter/Set Position From Baked Clip")]
    public unsafe partial class SetPositionFromBakedClip : HNSFStateAction
    {
        public int throweeId;

        public AssetRef<Tag> tag;
        public AssetRef<AnimationClipBakedData> curveAssetReference;
        
        public FPVector2 offset;
        
        public int frameOffset;
        //public FP frameModifier = 1;
        
        public StateActionTargetContext targetContext;
        
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            if (!frame.Unsafe.TryGetPointer<Transform2D>(entity, out var originPointTransform)
                || !frame.Unsafe.TryGetPointer<FacingDirection>(entity, out var facingDirection)) return false;
            
            targetContext.callingEntity = entity;
            var targetEntityRef = HNSFStateHelper.GetStateTargetEntity(frame, ref targetContext);
            if (targetEntityRef == EntityRef.None) return false;
            
            if (!frame.Unsafe.TryGetPointer<Transform2D>(targetEntityRef, out var targetTransform)
                || !frame.TryFindAsset(curveAssetReference, out var curveAsset)) return false;
            
            var realFrame = FPMath.Clamp(stateContext.stateFrame + frameOffset, 0, FP.UseableMax);

            var fr = curveAsset.GetFrameAtTime((FP)realFrame / (FP)curveAsset.FrameRate, tag);
            targetTransform->Position = originPointTransform->Position +
                                         facingDirection->TransformDirection(fr.Position.XY);
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new SetPositionFromBakedClip());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as SetPositionFromBakedClip;
            t.throweeId = throweeId;
            t.tag = tag;
            t.curveAssetReference = curveAssetReference;
            t.offset = offset;
            t.frameOffset = frameOffset;
            t.targetContext = targetContext;
            return base.CopyTo(target);
        }
    }
}