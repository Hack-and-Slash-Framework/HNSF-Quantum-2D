using Photon.Deterministic;

namespace Quantum
{
    public unsafe partial struct BattleActorPhysics
    {
        public void SetExternalImpulse(Frame f, EntityRef entity, FPVector2 impulse)
        {
            externalCollisionImpulse = impulse;
        }
        
        public FPVector2 GetOverallVelocity(Frame f, EntityRef entity)
        {
            return force + externalCollisionImpulse;
        }

        /*
        public FPVector3 GetKinematicVelocity(Frame f, EntityRef entity)
        {
            if (!f.Unsafe.TryGetPointer<KCC>(entity, out var kcc)) return FPVector3.Zero;
            return kcc->Data.KinematicVelocity;
        }

        public FPVector3 GetDynamicVelocity(Frame f, EntityRef entity)
        {
            if (!f.Unsafe.TryGetPointer<KCC>(entity, out var kcc)) return FPVector3.Zero;
            return kcc->Data.DynamicVelocity;
        }

        public void SetOverallVelocity(Frame f, EntityRef entity, FPVector3 velocity)
        {
            if (f.Unsafe.TryGetPointer<KCC>(entity, out var kcc))
            {
                //kcc->SetKinematicVelocity();
            }
        }

        public void SetKinematicVelocity(Frame f, EntityRef entity, FPVector3 velocity)
        {
            if (f.Unsafe.TryGetPointer<KCC>(entity, out var kcc))
            {
                kcc->SetKinematicVelocity(velocity);
            }
        }

        public void SetDynamicVelocty(Frame f, EntityRef entity, FPVector3 velocity)
        {
            if (f.Unsafe.TryGetPointer<KCC>(entity, out var kcc))
            {
                kcc->SetDynamicVelocity(velocity);
            }
        }*/
    }
}