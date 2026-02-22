using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.functions
{
    [System.Serializable]
    public unsafe partial class GetBattleActorPhysicsForces : StateFunctionFPVector2
    {
        public enum ForceType
        {
            All,
            Force,
            ExternalForce
        }

        public enum SplitType
        {
            All,
            XOnly,
            YOnly
        }

        public ForceType forceType;
        public SplitType splitType;
        public bool normalize;
        
        public override FPVector2 Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            if (frame.Unsafe.TryGetPointer<BattleActorPhysics>(entity, out var bap))
            {
                var returnVal = FPVector2.Zero;
                switch (forceType)
                {
                    case ForceType.All: 
                        returnVal = bap->GetOverallVelocity(frame, entity);
                        break;
                    case ForceType.Force:
                        returnVal = bap->force;
                        break;
                    case ForceType.ExternalForce:
                        returnVal = bap->externalCollisionImpulse;
                        break;
                }

                switch (splitType)
                {
                    case SplitType.All:
                        break;
                    case SplitType.XOnly:
                        returnVal.Y = 0;
                        break;
                    case SplitType.YOnly:
                        returnVal.X = 0;
                        break;
                }

                if (normalize && returnVal != FPVector2.Zero) returnVal = returnVal.Normalized;
                return returnVal;
            }
            return FPVector2.Zero;
        }

        public override HNSFStateFunction Copy()
        {
            return CopyTo(new GetBattleActorPhysicsForces());
        }

        public override HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            var t = target as GetBattleActorPhysicsForces;
            return base.CopyTo(target);
        }
    }
}