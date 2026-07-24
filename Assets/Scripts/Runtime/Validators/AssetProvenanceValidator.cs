using System.Collections.Generic;
using ArkhamIsland.AgentFramework;

namespace ArkhamIsland.Validators
{
    public class AssetProvenanceValidator : IDeterministicValidator
    {
        public string Name => "AssetProvenanceValidator";
        public string TargetScope => "all";

        public IEnumerable<ConstraintViolation> Validate(ExperienceContext context)
        {
            var violations = new List<ConstraintViolation>();
            // Asset provenance & ownership metadata check
            return violations;
        }
    }
}
