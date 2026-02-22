using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.functions
{
    [System.Serializable]
    public unsafe partial class GetKCC2DForces : StateFunctionFPVector2
    {
        public enum ForceType
        {
            All,
            KinematicVelocity,
            DynamicVelocity
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
            if (frame.Unsafe.TryGetPointer<KCC2D>(entity, out var kcc2d))
            {
                var returnVal = FPVector2.Zero;
                switch (forceType)
                {
                    case ForceType.All:
                        returnVal = kcc2d->CombinedVelocity;
                        break;
                    case ForceType.KinematicVelocity:
                        returnVal = kcc2d->_kinematicVelocity;
                        break;
                    case ForceType.DynamicVelocity:
                        returnVal = kcc2d->_dynamicVelocity;
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