using System.Collections.Generic;
using ArkhamIsland.AgentFramework;

namespace ArkhamIsland.Validators
{
    public class BiometricFeedbackLoopValidator : IDeterministicValidator
    {
        public string Name => "BiometricFeedbackLoopValidator";
        public string TargetScope => "biometrics";

        public IEnumerable<ConstraintViolation> Validate(ExperienceContext context)
        {
            var violations = new List<ConstraintViolation>();
            if (context?.Biometrics == null || context.Sanity == null) return violations;

            // Detect positive feedback loop risk (high stress + low sanity = instability)
            if (context.Biometrics.stressLevel > 0.85f && context.Sanity.currentSanity < 0.25f)
            {
                violations.Add(new ConstraintViolation
                {
                    sourceAgentName = Name,
                    constraintName = "UnstableBiometricFeedbackLoop",
                    severity = ConstraintSeverity.Blocking,
                    description = "Unstable feedback loop detected: high stress (>0.85) + critical low sanity (<0.25) causing runaway trigger cascades.",
                    suggestedFix = "Enforce cooldowns and noise filtering in PlayerStateEffectsCoordinator."
                });
            }

            return violations;
        }
    }
}
