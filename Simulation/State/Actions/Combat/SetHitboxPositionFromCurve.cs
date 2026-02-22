using Photon.Deterministic;
using System;
using HnSF.core.state;
using HnSF.core.state.actions;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    [AddTypeMenu(menuName: "Hitbox/Set Hitbox Position From Curves")]
    public unsafe partial class SetHitboxPositionFromCurve : HNSFStateAction
    {
        public int hitboxIdentifier;
        public FP multi = 1;
        public FPAnimationCurve xPosition;
        public FPAnimationCurve yPosition;

        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            var boxCombatant = frame.Get<BoxCombatant>(entity);
            var faceDir = frame.Unsafe.GetPointer<FacingDirection>(entity);

            if (!boxCombatant.TryGetHitbox(frame, hitboxIdentifier, out var hitboxIndex)) return false;
            
            var xPos = xPosition.Evaluate(rangePercent) * multi;
            var yPos = yPosition.Evaluate(rangePercent) * multi;
            
            var hitboxList = frame.ResolveList(boxCombatant.hitboxList);
            var hitboxParented = frame.Unsafe.GetPointer<Parented2D>(hitboxList[hitboxIndex]);
            hitboxParented->localOffset = new FPVector2(xPos * faceDir->GetFacingMulti(), yPos);
            return false;
        }
    }
}