using System.Collections.Generic;
using UnityEngine;
using ArkhamIsland.AgentFramework;

namespace ArkhamIsland.Validators
{
    public class PerformanceBudgetValidator : IDeterministicValidator
    {
        public string Name => "PerformanceBudgetValidator";
        public string TargetScope => "rendering";

        public IEnumerable<ConstraintViolation> Validate(ExperienceContext context)
        {
            var violations = new List<ConstraintViolation>();
            if (context?.Performance == null) return violations;

            var perf = context.Performance;

            // Frame time budget check
            if (perf.consumedBudgetMs > perf.frameBudgetMs)
            {
                violations.Add(new ConstraintViolation
                {
                    sourceAgentName = Name,
                    constraintName = "FrameTimeBudget",
                    severity = ConstraintSeverity.Blocking,
                    description = $"Frame budget overrun: consumed {perf.consumedBudgetMs:F2}ms exceeds target {perf.frameBudgetMs:F2}ms.",
                    suggestedFix = "Reduce draw calls, lower particle count, or enable aggressive foveated rendering."
                });
            }

            // Draw call budget check
            if (perf.maxDrawCalls > 150 && context.TargetPlatform == "quest3")
            {
                violations.Add(new ConstraintViolation
                {
                    sourceAgentName = Name,
                    constraintName = "DrawCallBudget",
                    severity = ConstraintSeverity.Warning,
                    description = $"Draw call budget ({perf.maxDrawCalls}) may cause frame drops on target platform '{context.TargetPlatform}'.",
                    suggestedFix = "Combine meshes or use instanced rendering."
                });
            }

            return violations;
        }
    }
}
