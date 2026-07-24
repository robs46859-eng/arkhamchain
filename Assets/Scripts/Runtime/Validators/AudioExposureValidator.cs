using System.Collections.Generic;
using ArkhamIsland.AgentFramework;

namespace ArkhamIsland.Validators
{
    public class AudioExposureValidator : IDeterministicValidator
    {
        public string Name => "AudioExposureValidator";
        public string TargetScope => "audio";

        public IEnumerable<ConstraintViolation> Validate(ExperienceContext context)
        {
            var violations = new List<ConstraintViolation>();
            if (context?.Comfort == null) return violations;

            if (context.Comfort.maxAudioDecibelSPL > 85f)
            {
                violations.Add(new ConstraintViolation
                {
                    sourceAgentName = Name,
                    constraintName = "AudioDecibelSPLOverrun",
                    severity = ConstraintSeverity.Warning,
                    description = $"Max audio decibel SPL ({context.Comfort.maxAudioDecibelSPL} dB) exceeds 85 dB safety standard.",
                    suggestedFix = "Cap audio volume via HRTFSpatialAudioAgent."
                });
            }

            return violations;
        }
    }
}
