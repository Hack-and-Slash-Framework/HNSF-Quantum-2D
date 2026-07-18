using Photon.Deterministic;

namespace Quantum
{
    public unsafe partial struct FacingDirection
    {
        public FP GetFacingMulti()
        {
            return isFacingRight ? 1 : -1;
        }

        public FPVector2 TransformDirection(FPVector2 vector)
        {
            return new FPVector2(vector.X * GetFacingMulti(), vector.Y);
        }
        
        public FPVector3 TransformDirection(FPVector3 vector)
        {
            return new FPVector3(vector.X * GetFacingMulti(), vector.Y, vector.Z);
        }
    }
}