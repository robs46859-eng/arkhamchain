using System.Collections.Generic;
using ArkhamIsland.AgentFramework;

namespace ArkhamIsland.Validators
{
    /// <summary>
    /// Contract for deterministic validators.
    /// Unlike AI agents, deterministic validators evaluate exact hard limits, mathematical bounds,
    /// and structural invariants using traditional software logic.
    /// </summary>
    public interface IDeterministicValidator
    {
        /// <summary>Unique name of the validator.</summary>
        string Name { get; }

        /// <summary>Target scope or system this validator checks.</summary>
        string TargetScope { get; }

        /// <summary>
        /// Evaluates the shared ExperienceContext and returns any constraint violations.
        /// </summary>
        IEnumerable<ConstraintViolation> Validate(ExperienceContext context);
    }
}
