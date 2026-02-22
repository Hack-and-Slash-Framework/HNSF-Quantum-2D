using Photon.Deterministic;
using System;
using HnSF.core.state;
using HnSF.core.state.actions;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    [AddTypeMenu(menuName: "Fighter/Movement/Modify Gravity")]
    public unsafe partial class ModifyGravity : HNSFStateAction
    {
        public enum ModifyType
        {
            SET,
            ADD
        }

        public ModifyType modifyType;
        public HNSFParamFP value;
        public bool asJumpForce;
        public FP multiplier = 1;

        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            if (!frame.Unsafe.TryGetPointer<BattleActorPhysics>(entity, out var kcc)) return false;
            
            switch (modifyType)
            {
                case ModifyType.SET:
                    kcc->force.Y = value.Resolve(frame, entity, ref stateContext) * multiplier;
                    /*
                    if(asJumpForce) kcc->Jump(new FPVector3(0, value.Resolve(frame, entity, ref stateContext), 0));
                    else kcc->SetDynamicVelocity(new FPVector3(0, value.Resolve(frame, entity, ref stateContext), 0));*/
                    break;
                case ModifyType.ADD:
                    kcc->force.Y += value.Resolve(frame, entity, ref stateContext) * multiplier;
                    break;
            }
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new ModifyGravity());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as ModifyGravity;
            t.modifyType = modifyType;
            t.value = value.Clone() as HNSFParamFP;
            t.asJumpForce = asJumpForce;
            t.multiplier = multiplier;
            return base.CopyTo(target);
        }
    }
}