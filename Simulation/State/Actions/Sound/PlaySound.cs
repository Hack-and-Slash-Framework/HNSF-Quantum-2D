using System;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    public unsafe partial class PlaySound : HNSFStateAction
    {
        public PlaySoundRequestParam playSoundRequestParam;
        public bool offsetFromECBCenter;

        public override void OnValidate()
        {
            base.OnValidate();
            playSoundRequestParam.OnValidate();
        }

        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            var facingDir = frame.Unsafe.GetPointer<FacingDirection>(entity);
            var transform = frame.Unsafe.GetPointer<Transform2D>(entity);

            var soundRequest = playSoundRequestParam.Resolve(frame);
            var sound = soundRequest.GetRngSound(frame.RNG);
            if (!sound.soundRef.IsValid) return false;

            var position = transform->Position;

            if (offsetFromECBCenter && frame.Unsafe.TryGetPointer<ECB>(entity, out var ecb))
            {
                position += ecb->offset;
            }
            
            position += facingDir->TransformDirection(soundRequest.positionOffset.XY);
            SoundEffectHelper.PlaySound(frame, soundRequest, sound, entity, position.XYO);
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new PlaySound());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as PlaySound;
            t.playSoundRequestParam = playSoundRequestParam;
            return base.CopyTo(target);
        }
    }
}