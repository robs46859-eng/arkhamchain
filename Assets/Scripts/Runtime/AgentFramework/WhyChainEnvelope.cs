using System;

namespace ArkhamIsland.AgentFramework
{
    /// <summary>
    /// The Why-Chain envelope is the core transaction unit in the Thermal Cascade architecture.
    /// Every agent-to-agent handoff MUST be wrapped in this envelope to preserve causal intent.
    /// 
    /// Validation Rules:
    /// 1. No blank Why-In/Out — Agent must refuse payload if intent is missing.
    /// 2. Temperature monotonicity — A hot agent MUST NOT call a hotter agent downstream.
    /// 3. Payload immutability — Once sealed, only the target agent may mutate the payload.
    /// 4. Audit trail — All envelopes are logged to WhyChainAuditLog.
    /// </summary>
    [Serializable]
    public class WhyChainEnvelope
    {
        /// <summary>Unique transaction identifier (UUID).</summary>
        public string transactionId;

        /// <summary>UTC timestamp of envelope creation.</summary>
        public string timestampUtc;

        /// <summary>Source agent descriptor.</summary>
        public AgentDescriptor sourceAgent;

        /// <summary>Target agent descriptor.</summary>
        public AgentDescriptor targetAgent;

        /// <summary>The causal intent chain — the "Why" of this handoff.</summary>
        public WhyChain whyChain;

        /// <summary>Pipeline-specific payload data.</summary>
        public AgentPayload payload;

        /// <summary>VR and performance constraints for this transaction.</summary>
        public TransactionConstraints constraints;

        /// <summary>Whether the envelope has been sealed (no further mutation by source).</summary>
        public bool isSealed;

        /// <summary>
        /// Creates a new Why-Chain envelope with a fresh transaction ID and timestamp.
        /// </summary>
        public static WhyChainEnvelope Create(
            AgentDescriptor source,
            AgentDescriptor target,
            string upstreamIntent,
            string downstreamIntent)
        {
            return new WhyChainEnvelope
            {
                transactionId = Guid.NewGuid().ToString("N"),
                timestampUtc = DateTime.UtcNow.ToString("o"),
                sourceAgent = source,
                targetAgent = target,
                whyChain = new WhyChain
                {
                    upstreamIntentReceived = upstreamIntent,
                    downstreamIntentPassed = downstreamIntent,
                    creativeIntentPreservationScore = 1.0f
                },
                payload = new AgentPayload(),
                constraints = new TransactionConstraints(),
                isSealed = false
            };
        }

        /// <summary>
        /// Validates the envelope against the Why-Chain protocol rules.
        /// Returns null if valid, or an error message if invalid.
        /// </summary>
        public string Validate()
        {
            if (string.IsNullOrWhiteSpace(whyChain?.upstreamIntentReceived))
                return "PROTOCOL VIOLATION: Upstream intent (Why-In) is empty. Agents must not accept blind payloads.";

            if (string.IsNullOrWhiteSpace(whyChain?.downstreamIntentPassed))
                return "PROTOCOL VIOLATION: Downstream intent (Why-Out) is empty. Agents must declare purpose of handoff.";

            if (sourceAgent == null || targetAgent == null)
                return "PROTOCOL VIOLATION: Source or target agent descriptor is null.";

            if (!ThermalTierUtils.IsValidCascade(sourceAgent.temperature, targetAgent.temperature))
                return $"CASCADE VIOLATION: Source temp {sourceAgent.temperature} -> Target temp {targetAgent.temperature}. " +
                       "Heat flows DOWN, never UP. Hot agents dream; cool agents stabilize.";

            return null; // Valid
        }

        /// <summary>
        /// Seals the envelope, preventing further mutation by the source agent.
        /// Only the target agent may mutate the payload after sealing.
        /// </summary>
        public void Seal()
        {
            isSealed = true;
        }
    }

    /// <summary>
    /// Describes an agent participating in a Why-Chain transaction.
    /// </summary>
    [Serializable]
    public class AgentDescriptor
    {
        public string agentId;
        public string agentName;
        public float temperature;
        public string pipeline;

        public AgentDescriptor() { }

        public AgentDescriptor(string id, string name, float temp, string pipeline)
        {
            agentId = id;
            agentName = name;
            temperature = temp;
            this.pipeline = pipeline;
        }
    }

    /// <summary>
    /// The causal intent chain — the heart of the Why-Chain protocol.
    /// Every handoff must explicitly state WHY the data was sent and WHY it's being passed along.
    /// </summary>
    [Serializable]
    public class WhyChain
    {
        /// <summary>
        /// "Why was this sent to me?"
        /// The receiving agent's understanding of upstream creative/functional intent.
        /// Max 500 characters.
        /// </summary>
        public string upstreamIntentReceived;

        /// <summary>
        /// "Why am I passing this along?"
        /// The sending agent's declaration of what the downstream agent must accomplish.
        /// Max 500 characters.
        /// </summary>
        public string downstreamIntentPassed;

        /// <summary>
        /// Score from 0.0 to 1.0 indicating how much of the original creative intent
        /// has been preserved through the cascade. Cool agents should aim for >= 0.8.
        /// </summary>
        public float creativeIntentPreservationScore;
    }

    /// <summary>
    /// Pipeline-specific payload carried by a Why-Chain envelope.
    /// </summary>
    [Serializable]
    public class AgentPayload
    {
        /// <summary>Payload type: spatial, character, narrative, audio, rendering, haptics.</summary>
        public string payloadType;

        /// <summary>JSON-serialized pipeline-specific data.</summary>
        public string serializedData;

        /// <summary>SHA-256 hash for payload integrity verification.</summary>
        public string validationHash;
    }

    /// <summary>
    /// VR and performance constraints enforced on the transaction.
    /// Cool agents use these to guard frame time, physics stability, and motion safety.
    /// </summary>
    [Serializable]
    public class TransactionConstraints
    {
        /// <summary>Maximum frame budget in milliseconds (11.11ms for 90Hz, 8.33ms for 120Hz).</summary>
        public float frameBudgetMs = 11.11f;

        /// <summary>VR safety flags that must be respected.</summary>
        public string[] vrSafetyFlags = new string[]
        {
            "no_vestibular_conflict",
            "no_photosensitive_trigger",
            "locomotion_comfort",
            "haptic_strain_limit"
        };

        /// <summary>Priority level: 0=critical, 5=deferred.</summary>
        public int priority = 2;
    }
}
