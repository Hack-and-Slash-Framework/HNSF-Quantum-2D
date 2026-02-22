using Photon.Deterministic;
using System;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    public unsafe partial class PlayVisualEffect : HNSFStateAction
    {
        public PlayVisualEffectRequestParam visualEffectRequestParam;
        public bool atClosestBodyPosition;

        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            var facingDir = frame.Unsafe.GetPointer<FacingDirection>(entity);
            var transform = frame.Unsafe.GetPointer<Transform2D>(entity);

            var request = visualEffectRequestParam.Resolve(frame);
            var vfx = request.GetRngVFX(frame.RNG);

            VisualEffectHelper.PlayVisualEffect(frame, request, vfx, entity, transform->Position.XYO,
                facingDir->isFacingRight, atClosestBodyPosition);
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new PlayVisualEffect());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as PlayVisualEffect;
            t.visualEffectRequestParam = visualEffectRequestParam;
            return base.CopyTo(target);
        }
    }
}