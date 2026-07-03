using System;
using Photon.Deterministic;

namespace HnSF
{
    [Serializable]
    public struct AnimationFrameListWithParam : IEquatable<AnimationFrameListWithParam>
    {
        public FPVector2 param;
        public AnimationFrame[] Frames;


        public bool Equals(AnimationFrameListWithParam other)
        {
            return param.Equals(other.param) && Frames == other.Frames;
        }

        public override bool Equals(object obj)
        {
            return obj is AnimationFrameListWithParam other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(param, Frames);
        }
    }
}