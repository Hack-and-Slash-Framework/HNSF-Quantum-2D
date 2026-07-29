using System;
using HnSF.core.GroupControl.Actions;
using HnSF.core.GroupControl.Functions;
using HnSF.Nodes;
using Photon.Deterministic;
using Quantum;
#if QUANTUM_UNITY
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
#endif
#if UNITY_EDITOR
using HnSF.core.GroupControl.Nodes;
using Unity.GraphToolkit.Editor;
#endif

namespace HnSF.core.GroupControl.Functions
{
    [Serializable]
    public unsafe partial class GetEntityRefPositionFPVector2 : GroupControlFunctionFPVector2
    {
#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public GroupControlFunctionEntityRef functionEntityRef;
        public bool useFacingForOffset;
        public BattleScriptingParamFPVector2 paramOffset;
        
        public override FPVector2 Execute(Frame frame, EntityRef infoEntityRef, ref BattleScriptContext context)
        {
            var entityRef = functionEntityRef.Execute(frame, infoEntityRef, ref context);
            if (!frame.Exists(entityRef)
                || !frame.Unsafe.TryGetPointer<Transform2D>(entityRef, out var t2d)) return FPVector2.Zero;
            switch (useFacingForOffset)
            {
                case true:
                    if (!frame.Unsafe.TryGetPointer<FacingDirection>(entityRef, out var facingDirection))
                        return t2d->Position;
                    return t2d->Position + facingDirection->TransformDirection(paramOffset.Resolve(frame, infoEntityRef, ref context));
                    break;
                case false:
                    return t2d->Position;
                    break;
            }
        }
    }
}

# if UNITY_EDITOR
namespace HnSF.core.GroupControl.Nodes
{
    [Serializable]
    [UseWithGraph(typeof(ActorGroupScriptGraph))]
    internal class GetEntityRefPositionFPVector2 : FunctionNodeBase
    {
        public const string inEntityRefFunction = "EntityRefFunction";
        public const string inUseFacingForOffset = "UseFacingForOffset";
        public const string inOffset = "Offset";

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);
        }

        protected override void OnDefinePorts(Node.IPortDefinitionContext context)
        {
            AddInputOutputExecutionPorts(context);
            
            context.AddInputPort(inEntityRefFunction)
                .WithDisplayName("Entity Ref Function")
                .Build();
            
            context.AddInputPort<FPVector2>(inOffset)
                .WithDisplayName("Offset")
                .Build();

            context.AddInputPort<bool>(inUseFacingForOffset)
                .WithDisplayName("Use Facing For Offset")
                .WithDefaultValue(true)
                .Build();
        }

        public override GroupControlFunction Convert()
        {
            return new HnSF.core.GroupControl.Functions.GetEntityRefPositionFPVector2()
            {
                functionEntityRef = ConvertFunctionNode<GroupControlFunctionEntityRef>(GetInputPortByName(inEntityRefFunction)),
                useFacingForOffset = NodeHelper.GetInputPortValue<bool>(GetInputPortByName(inUseFacingForOffset)),
                paramOffset = GetInputPortParam<BattleScriptingParamFPVector2, FPVector2>(GetInputPortByName(inOffset))
            };
        }
    }
}
#endif