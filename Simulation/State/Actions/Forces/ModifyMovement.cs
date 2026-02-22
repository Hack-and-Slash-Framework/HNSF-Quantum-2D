using Photon.Deterministic;
using System;
using System.Linq;
using HnSF.core.state;
using HnSF.core.state.actions;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    [AddTypeMenu(menuName: "Fighter/Movement/Modify Movement")]
    public unsafe partial class ModifyMovement : HNSFStateAction
    {
        public enum InputSourceType
        {
            none,
            stick,
            lookDirection,
            slope,
            custom,
            StickBuffered,
            wall
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
        public int stickBufferedBuffer = 3;
        public bool zeroInputY = true;
        
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            BattleActorPhysics* kcc = frame.Unsafe.GetPointer<BattleActorPhysics>(entity);
            var faceDir = frame.Unsafe.GetPointer<FacingDirection>(entity);
            var inputs = frame.Unsafe.GetPointer<ActorInputBufferMovement>(entity);
            FPVector2 input = FPVector2.Zero;

            var speed = speedParam.Resolve(frame, entity, ref stateContext);
            
            for (int i = 0; i < inputSources.Length; i++)
            {
                switch (inputSources[i])
                {
                    case InputSourceType.slope:
                        break;
                    case InputSourceType.stick:
                        input = inputs->GetMovement(0);
                        break;
                    case InputSourceType.lookDirection:
                        input = new FPVector2(faceDir->GetFacingMulti(), 0);
                        break;
                    case InputSourceType.custom:
                        input = customInput.Resolve(frame, entity, ref stateContext);
                        break;
                    case InputSourceType.StickBuffered:
                        input = inputs->GetMovement(stickBufferedBuffer);
                        break;
                    case InputSourceType.wall:
                        if (frame.Unsafe.TryGetPointer<GotWallInfo>(entity, out var gwi))
                        {
                            input = gwi->wallNormal;
                        }
                        break;
                }
                
                if (input != FPVector2.Zero) break;
            }

            if(zeroInputY) input.Y = 0;
            if (normalizeInput && input != FPVector2.Zero) input = input.Normalized;

            if (modifyType == ModifyType.SET)
            {
                kcc->force.X = input.X * speed;
            }
            else
            {
                kcc->force.X += input.X * speed;
            }
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new ModifyMovement());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as ModifyMovement;
            t.inputSources = inputSources.ToArray();
            t.speedParam = speedParam.Clone() as HNSFParamFP;
            t.normalizeInput = normalizeInput;
            t.customInput = customInput.Clone() as HNSFParamFPVector2;
            t.stickBufferedBuffer = stickBufferedBuffer;
            return base.CopyTo(target);
        }
    }
}