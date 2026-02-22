using Photon.Deterministic;
using System;
using System.Linq;
using HnSF.core.state;
using HnSF.core.state.actions;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    [AddTypeMenu(menuName: "Fighter/Movement/Modify Forces")]
    public unsafe partial class ModifyForces : HNSFStateAction
    {
        public enum InputSourceType
        {
            none,
            stick,
            rotation,
            slope,
            custom
        }
        
        public enum ModifyType
        {
            SET,
            ADD
        }

        public ModifyType modifyType;
        public InputSourceType[] inputSources;
        public HNSFParamFP speedParam;
        public bool normalizeInput;
        public HNSFParamFPVector2 customInput;
        public int throweeId;
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            HNSFStateContext targetStateContext = stateContext;
            var targetEntityRef = GetActionTargetEntityRef(frame, entity, ref targetStateContext);
            if (targetEntityRef == EntityRef.None) return false;
            DoAction(frame, targetEntityRef, ref targetStateContext);
            return false;
        }

        private void DoAction(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            var kcc = frame.Unsafe.GetPointer<BattleActorPhysics>(entity);

            var speed = speedParam.Resolve(frame, entity, ref stateContext);
            
            FPVector2 input = FPVector2.Zero;
            for (int i = 0; i < inputSources.Length; i++)
            {
                switch (inputSources[i])
                {
                    case InputSourceType.slope:
                        break;
                    case InputSourceType.stick:
                        var inputs = frame.Unsafe.GetPointer<ActorInputBufferMovement>(entity);
                        input = inputs->GetMovement(0);
                        break;
                    case InputSourceType.rotation:
                        var facDir = frame.Unsafe.GetPointer<FacingDirection>(entity);
                        input = facDir->TransformDirection(FPVector2.Right);
                        break;
                    case InputSourceType.custom:
                        input = customInput.Resolve(frame, entity, ref stateContext);
                        break;
                }
                
                if (input != FPVector2.Zero) break;
            }

            if (normalizeInput && input != FPVector2.Zero) input = input.Normalized;

            input *= speed;
            
            switch (modifyType)
            {
                case ModifyType.SET:
                    kcc->force = input;
                    break;
                case ModifyType.ADD:
                    kcc->force += input;
                    break;
            }
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new ModifyForces());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as ModifyForces;
            t.modifyType = modifyType;
            t.inputSources = inputSources.ToArray();
            t.speedParam = speedParam.Clone() as HNSFParamFP;
            t.normalizeInput = normalizeInput;
            t.customInput = customInput.Clone() as HNSFParamFPVector2;
            t.throweeId = throweeId;
            return base.CopyTo(target);
        }
    }
}