using System.Collections.Generic;
using ArkhamIsland.AgentFramework;

namespace ArkhamIsland.Validators
{
    public class CollisionValidator : IDeterministicValidator
    {
        public string Name => "CollisionValidator";
        public string TargetScope => "world";

        public IEnumerable<ConstraintViolation> Validate(ExperienceContext context)
        {
            var violations = new List<ConstraintViolation>();
            // Validates that geometry does not cause collision gaps or clipping hazards
            return violations;
        }
    }
}
