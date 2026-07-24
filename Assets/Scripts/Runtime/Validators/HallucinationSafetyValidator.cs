using System.Collections.Generic;
using ArkhamIsland.AgentFramework;

namespace ArkhamIsland.Validators
{
    public class HallucinationSafetyValidator : IDeterministicValidator
    {
        public string Name => "HallucinationSafetyValidator";
        public string TargetScope => "narrative";

        public IEnumerable<ConstraintViolation> Validate(ExperienceContext context)
        {
            var violations = new List<ConstraintViolation>();
            if (context == null) return violations;

            // Check if hallucinations occur during cutscenes or dialogue
            if (context.Narrative != null && (context.Narrative.isInCutscene || context.Narrative.isInDialogue))
            {
                if (context.Sanity != null && context.Sanity.hallucinationsTriggedThisSession > 0)
                {
                    violations.Add(new ConstraintViolation
                    {
                        sourceAgentName = Name,
                        constraintName = "HallucinationInCutscene",
                        severity = ConstraintSeverity.Blocking,
                        description = "Hallucinations triggered during cutscene or dialogue state.",
                        suggestedFix = "Defer hallucination triggers until dialogue/cutscene completes."
                    });
                }
            }

            return violations;
        }
    }
}
