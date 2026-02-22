using Photon.Deterministic;
using System;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    [AddTypeMenu(menuName: "Fighter/Movement/Force Unground")]
    public unsafe partial class ForceUnground : HNSFStateAction
    {
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            if (frame.Unsafe.TryGetPointer<KCC2D>(entity, out var kcc))
            {
                // TODO
            }
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new ForceUnground());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as ForceUnground;
            return base.CopyTo(target);
        }
    }
}