namespace Quantum
{
    public partial class VisualEffectEntry
    {
        public enum FlipType
        {
            None,
            Rotation,
            Scale
        }
        public FlipType flipType = FlipType.Rotation;
    }
}