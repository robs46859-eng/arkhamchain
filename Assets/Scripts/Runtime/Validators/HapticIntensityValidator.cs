using System.Collections.Generic;
using ArkhamIsland.AgentFramework;

namespace ArkhamIsland.Validators
{
    public class HapticIntensityValidator : IDeterministicValidator
    {
        public string Name => "HapticIntensityValidator";
        public string TargetScope => "haptics";

        public IEnumerable<ConstraintViolation> Validate(ExperienceContext context)
        {
            var violations = new List<ConstraintViolation>();
            if (context?.Comfort == null) return violations;

            if (context.Comfort.maxHapticIntensity > 0.85f)
            {
                violations.Add(new ConstraintViolation
                {
                    sourceAgentName = Name,
                    constraintName = "HapticIntensityExceeded",
                    severity = ConstraintSeverity.Warning,
                    description = $"Max haptic intensity ({context.Comfort.maxHapticIntensity:F2}) exceeds ergonomic limit 0.85.",
                    suggestedFix = "Clamp haptic motor amplitude in HapticErgonomicsAgent."
                });
            }

            return violations;
        }
    }
}
