namespace MotionCommotion.Core
{
    public static class PhysicsLimits
    {
        public const double MinimumMass = .001;
        public const double MaximumMass = 1000000;
        public const double MaximumAssignedVelocity = NumericRules.MaximumResult;
        public const double MaximumCommandedAcceleration = NumericRules.MaximumResult;
        public const double MaximumGravity = NumericRules.MaximumResult;
        public const double MaximumProgramSeconds = 3600;
        public const double MaximumStartingPosition = 1000000;
    }
}
