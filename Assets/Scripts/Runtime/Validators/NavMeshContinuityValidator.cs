using System.Collections.Generic;
using ArkhamIsland.AgentFramework;

namespace ArkhamIsland.Validators
{
    public class NavMeshContinuityValidator : IDeterministicValidator
    {
        public string Name => "NavMeshContinuityValidator";
        public string TargetScope => "world";

        public IEnumerable<ConstraintViolation> Validate(ExperienceContext context)
        {
            var violations = new List<ConstraintViolation>();
            if (context?.World == null) return violations;

            // Check if World specification has disconnected segments or invalid surfaces
            if (context.World.segmentCount > 0 && context.World.navMeshSurfaceCount == 0)
            {
                violations.Add(new ConstraintViolation
                {
                    sourceAgentName = Name,
                    constraintName = "NavMeshSurfaceMissing",
                    severity = ConstraintSeverity.Blocking,
                    description = $"World has {context.World.segmentCount} segments but 0 baked NavMesh surfaces.",
                    suggestedFix = "Run NavMeshBuilder to generate locomotion surfaces across island segments."
                });
            }

            return violations;
        }
    }
}
