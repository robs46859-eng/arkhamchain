using System.Collections.Generic;
using ArkhamIsland.AgentFramework;

namespace ArkhamIsland.Validators
{
    public class QuestReachabilityValidator : IDeterministicValidator
    {
        public string Name => "QuestReachabilityValidator";
        public string TargetScope => "narrative";

        public IEnumerable<ConstraintViolation> Validate(ExperienceContext context)
        {
            var violations = new List<ConstraintViolation>();
            if (context?.Narrative == null) return violations;

            // Check if active quest items are defined and reachable
            if (!string.IsNullOrEmpty(context.Narrative.currentQuestId) && context.Narrative.currentQuestId != "none")
            {
                if (context.Narrative.criticalInteractableIds == null || context.Narrative.criticalInteractableIds.Length == 0)
                {
                    violations.Add(new ConstraintViolation
                    {
                        sourceAgentName = Name,
                        constraintName = "QuestInteractableMissing",
                        severity = ConstraintSeverity.Warning,
                        description = $"Active quest '{context.Narrative.currentQuestId}' has no critical interactables declared.",
                        suggestedFix = "Declare critical quest interactables in NarrativeState to ensure accessibility."
                    });
                }
            }

            return violations;
        }
    }
}
