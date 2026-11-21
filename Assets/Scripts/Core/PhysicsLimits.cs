namespace MotionCommotion.Core
{
    public static class PhysicsLimits
    {
        public const double MinimumMass = .0001;
        public const double MaximumMass = 1000000;
        public const double MaximumKickVelocityBudget = 10000;
        public const double MaximumGravity = 10000;
        public const double MaximumProgramSeconds = 3600;
        public const double MaximumStartingPosition = 1000000;
    }
}
