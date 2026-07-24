using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArkhamIsland.AgentFramework
{
    /// <summary>
    /// Defines and orchestrates a linear pipeline of agents in the Thermal Cascade.
    /// A pipeline connects a chain of agents (typically Hot → Balanced → Cool)
    /// and manages the flow of Why-Chain envelopes through the cascade.
    /// 
    /// Each pipeline corresponds to one of the core workflows:
    /// - World: MapDesign → PhysicsMeshAnchor
    /// - Character: CharacterPicker → PCEngineer
    /// - Narrative: CosmicMythosWeaver → SanityEngineAnchor
    /// - Audio: PsychoacousticWeaver → HRTFSpatialAudio
    /// - Rendering: AtmosphereDirector → VRShaderOptimization
    /// - Haptics: EldritchObject → HapticErgonomics
    /// </summary>
    public class AgentPipeline : MonoBehaviour
    {
        [Header("Pipeline Identity")]
        [SerializeField] private string pipelineName;
        [SerializeField] private string pipelineDescription;

        [Header("Agent Chain")]
        [Tooltip("Ordered list of agents in this pipeline. Must be ordered from highest to lowest temperature.")]
        [SerializeField] private List<AgentBase> agentChain = new List<AgentBase>();

        /// <summary>Pipeline name for identification.</summary>
        public string PipelineName => pipelineName;

        /// <summary>Event fired when a pipeline execution completes.</summary>
        public event Action<string, WhyChainEnvelope> OnPipelineCompleted;

        /// <summary>Event fired when a pipeline execution fails.</summary>
        public event Action<string, string> OnPipelineFailed;

        private void Start()
        {
            ValidateChainOrder();
        }

        /// <summary>
        /// Validates that the agent chain is ordered by descending temperature
        /// (hot → cool), enforcing the thermal cascade invariant.
        /// </summary>
        private void ValidateChainOrder()
        {
            if (agentChain.Count < 2) return;

            for (int i = 0; i < agentChain.Count - 1; i++)
            {
                float currentTemp = agentChain[i].Descriptor.temperature;
                float nextTemp = agentChain[i + 1].Descriptor.temperature;

                if (nextTemp > currentTemp)
                {
                    Debug.LogError($"[AgentPipeline:{pipelineName}] CASCADE INVARIANT VIOLATION: " +
                                   $"{agentChain[i].Descriptor.agentName} (temp={currentTemp:F2}) → " +
                                   $"{agentChain[i + 1].Descriptor.agentName} (temp={nextTemp:F2}). " +
                                   "Temperature must decrease along the chain.");
                }
            }

            Debug.Log($"[AgentPipeline:{pipelineName}] Chain validated: {agentChain.Count} agents. " +
                      $"Temp range: {agentChain[0].Descriptor.temperature:F2} → " +
                      $"{agentChain[agentChain.Count - 1].Descriptor.temperature:F2}");
        }

        /// <summary>
        /// Injects a new Why-Chain envelope into the pipeline at the head (hottest agent).
        /// This is the primary entry point for triggering a pipeline execution.
        /// </summary>
        public bool Inject(WhyChainEnvelope envelope)
        {
            if (agentChain.Count == 0)
            {
                Debug.LogError($"[AgentPipeline:{pipelineName}] Cannot inject — agent chain is empty.");
                OnPipelineFailed?.Invoke(pipelineName, "Empty agent chain");
                return false;
            }

            AgentBase headAgent = agentChain[0];

            Debug.Log($"[AgentPipeline:{pipelineName}] Injecting envelope {envelope.transactionId} " +
                      $"into head agent {headAgent.Descriptor.agentName}");

            bool accepted = headAgent.ReceiveEnvelope(envelope);
            if (!accepted)
            {
                OnPipelineFailed?.Invoke(pipelineName, "Head agent rejected envelope");
            }

            return accepted;
        }

        /// <summary>
        /// Triggers processing across the entire pipeline chain.
        /// Each agent processes its next pending envelope and hands off downstream.
        /// Call this from Update or a coroutine depending on desired execution timing.
        /// </summary>
        public void Tick()
        {
            foreach (AgentBase agent in agentChain)
            {
                if (agent.PendingCount > 0)
                {
                    agent.ProcessNext();
                }
            }
        }

        /// <summary>
        /// Returns a status snapshot of all agents in the pipeline.
        /// </summary>
        public string GetStatusReport()
        {
            var report = $"[Pipeline: {pipelineName}] ({agentChain.Count} agents)\n";
            foreach (AgentBase agent in agentChain)
            {
                report += $"  {agent}\n";
            }
            return report;
        }

        /// <summary>
        /// Returns the agent chain for external inspection.
        /// </summary>
        public IReadOnlyList<AgentBase> GetAgentChain()
        {
            return agentChain.AsReadOnly();
        }
    }
}
