using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArkhamIsland.AgentFramework
{
    /// <summary>
    /// Abstract base class for all agents in the Thermal Cascade architecture.
    /// Every agent has a temperature, a pipeline affiliation, and must implement
    /// the Why-Chain protocol for receiving and sending envelopes.
    /// 
    /// Lifecycle:
    /// 1. Agent registers itself with AgentRegistry on Awake.
    /// 2. Agent receives envelopes via ReceiveEnvelope().
    /// 3. Agent validates the Why-Chain, processes the payload, and optionally
    ///    creates a downstream envelope via CreateDownstreamEnvelope().
    /// 4. All transactions are logged to WhyChainAuditLog.
    /// </summary>
    public abstract class AgentBase : MonoBehaviour
    {
        [Header("Agent Identity")]
        [Tooltip("Unique identifier for this agent instance.")]
        [SerializeField] protected string agentId;

        [Tooltip("Human-readable agent name.")]
        [SerializeField] protected string agentName;

        [Tooltip("Temperature value controlling creative freedom (0.0 = deterministic, 1.0 = creative).")]
        [Range(0f, 1.1f)]
        [SerializeField] protected float temperature = 0.5f;

        [Tooltip("Pipeline this agent belongs to (world, character, narrative, audio, rendering, haptics).")]
        [SerializeField] protected string pipeline;

        /// <summary>The thermal tier classification derived from temperature.</summary>
        public ThermalTier Tier => ThermalTierUtils.Classify(temperature);

        /// <summary>This agent's descriptor for use in Why-Chain envelopes.</summary>
        public AgentDescriptor Descriptor => new AgentDescriptor(agentId, agentName, temperature, pipeline);

        /// <summary>Queue of pending inbound envelopes.</summary>
        protected Queue<WhyChainEnvelope> inboundQueue = new Queue<WhyChainEnvelope>();

        /// <summary>History of processed envelopes for audit trail.</summary>
        protected List<WhyChainEnvelope> processedHistory = new List<WhyChainEnvelope>();

        /// <summary>The downstream agent this agent hands off to (if any).</summary>
        [Header("Cascade Configuration")]
        [SerializeField] protected AgentBase downstreamAgent;

        protected virtual void Awake()
        {
            if (string.IsNullOrEmpty(agentId))
            {
                agentId = $"{agentName}_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
            }

            // Self-register with the global registry
            if (AgentRegistry.Instance != null)
            {
                AgentRegistry.Instance.RegisterAgent(this);
            }
        }

        protected virtual void OnDestroy()
        {
            if (AgentRegistry.Instance != null)
            {
                AgentRegistry.Instance.UnregisterAgent(this);
            }
        }

        /// <summary>
        /// Receives a Why-Chain envelope from an upstream agent.
        /// Validates the envelope against the protocol before enqueueing.
        /// </summary>
        /// <returns>True if the envelope was accepted, false if rejected.</returns>
        public bool ReceiveEnvelope(WhyChainEnvelope envelope)
        {
            if (envelope == null)
            {
                Debug.LogError($"[{agentName}] Rejected null envelope.");
                return false;
            }

            string validationError = envelope.Validate();
            if (validationError != null)
            {
                Debug.LogError($"[{agentName}] Envelope REJECTED: {validationError}");
                WhyChainAuditLog.Instance?.LogRejection(envelope, validationError);
                return false;
            }

            envelope.Seal();
            inboundQueue.Enqueue(envelope);

            Debug.Log($"[{agentName}] Accepted envelope {envelope.transactionId} from {envelope.sourceAgent.agentName}. " +
                      $"Queue depth: {inboundQueue.Count}");

            WhyChainAuditLog.Instance?.LogAcceptance(envelope);
            return true;
        }

        /// <summary>
        /// Processes the next inbound envelope. Called by the pipeline orchestrator
        /// or during Update (depending on agent's processing mode).
        /// </summary>
        public void ProcessNext()
        {
            if (inboundQueue.Count == 0) return;

            WhyChainEnvelope envelope = inboundQueue.Dequeue();

            Debug.Log($"[{agentName}] Processing envelope {envelope.transactionId}. " +
                      $"Why-In: \"{envelope.whyChain.upstreamIntentReceived}\"");

            // Invoke the agent-specific processing logic
            WhyChainEnvelope downstreamEnvelope = ProcessEnvelope(envelope);

            processedHistory.Add(envelope);
            WhyChainAuditLog.Instance?.LogProcessed(envelope);

            // If there's a downstream envelope and a downstream agent, hand off
            if (downstreamEnvelope != null && downstreamAgent != null)
            {
                Debug.Log($"[{agentName}] Handing off to {downstreamAgent.agentName}. " +
                          $"Why-Out: \"{downstreamEnvelope.whyChain.downstreamIntentPassed}\"");

                bool accepted = downstreamAgent.ReceiveEnvelope(downstreamEnvelope);
                if (!accepted)
                {
                    Debug.LogWarning($"[{agentName}] Downstream agent {downstreamAgent.agentName} REJECTED our envelope!");
                }
            }
        }

        /// <summary>
        /// Agent-specific processing logic. Subclasses MUST implement this.
        /// 
        /// Receives the inbound envelope, processes the payload according to the agent's
        /// temperature/role, and optionally returns a new envelope for downstream handoff.
        /// 
        /// Returns null if this agent is a terminal node in the pipeline.
        /// </summary>
        protected abstract WhyChainEnvelope ProcessEnvelope(WhyChainEnvelope inbound);

        /// <summary>
        /// Helper method to create a properly structured downstream envelope.
        /// Ensures temperature monotonicity and copies constraints.
        /// </summary>
        protected WhyChainEnvelope CreateDownstreamEnvelope(
            WhyChainEnvelope source,
            string downstreamIntentDescription,
            AgentPayload newPayload = null)
        {
            if (downstreamAgent == null)
            {
                Debug.LogWarning($"[{agentName}] Cannot create downstream envelope — no downstream agent configured.");
                return null;
            }

            var envelope = WhyChainEnvelope.Create(
                Descriptor,
                downstreamAgent.Descriptor,
                source.whyChain.downstreamIntentPassed, // Our received downstream intent becomes the next upstream intent
                downstreamIntentDescription
            );

            envelope.payload = newPayload ?? source.payload;
            envelope.constraints = source.constraints;

            return envelope;
        }

        /// <summary>
        /// Returns the number of pending envelopes in the inbound queue.
        /// </summary>
        public int PendingCount => inboundQueue.Count;

        /// <summary>
        /// Returns the total number of envelopes this agent has processed.
        /// </summary>
        public int ProcessedCount => processedHistory.Count;

        /// <summary>
        /// Returns agent identity info as a formatted string for debugging.
        /// </summary>
        public override string ToString()
        {
            return $"[{agentName}] Tier={Tier} Temp={temperature:F2} Pipeline={pipeline} " +
                   $"Pending={PendingCount} Processed={ProcessedCount}";
        }
    }
}
