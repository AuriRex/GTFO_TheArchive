using System;

namespace TheArchive.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class BuildConstraint : Attribute
    {
        public int BuildNumber { get; private set; }
        public string BuildNumberString => BuildNumber.ToString();

        public MatchMode Mode { get; private set; }

        public BuildConstraint(int build, MatchMode mode = MatchMode.Exact)
        {
            BuildNumber = build;
            Mode = mode;
        }

        public bool Matches(int buildNumber)
        {
            switch (Mode)
            {
                default:
                case MatchMode.Exact:
                    return buildNumber == BuildNumber;
                case MatchMode.Greater:
                    return buildNumber > BuildNumber;
                case MatchMode.GreaterOrEqual:
                    return buildNumber >= BuildNumber;
                case MatchMode.Lower:
                    return buildNumber < BuildNumber;
                case MatchMode.LowerOrEqual:
                    return buildNumber <= BuildNumber;
                case MatchMode.Exclude:
                    return buildNumber != BuildNumber;
            }
        }

        public enum MatchMode
        {
            Exact,
            Lower,
            LowerOrEqual,
            Greater,
            GreaterOrEqual,
            Exclude
        }
    }
}
