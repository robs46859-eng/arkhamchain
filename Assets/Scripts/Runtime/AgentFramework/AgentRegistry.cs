using System.Collections.Generic;
using UnityEngine;

namespace ArkhamIsland.AgentFramework
{
    /// <summary>
    /// Global registry of all active agents in the Thermal Cascade network.
    /// Provides lookup by name, ID, pipeline, and thermal tier.
    /// Singleton pattern — exists as a persistent GameObject in the scene.
    /// </summary>
    public class AgentRegistry : MonoBehaviour
    {
        public static AgentRegistry Instance { get; private set; }

        private Dictionary<string, AgentBase> agentsById = new Dictionary<string, AgentBase>();
        private Dictionary<string, AgentBase> agentsByName = new Dictionary<string, AgentBase>();
        private Dictionary<string, List<AgentBase>> agentsByPipeline = new Dictionary<string, List<AgentBase>>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Registers an agent with the global registry.
        /// Called automatically by AgentBase.Awake().
        /// </summary>
        public void RegisterAgent(AgentBase agent)
        {
            if (agent == null) return;

            string id = agent.Descriptor.agentId;
            string name = agent.Descriptor.agentName;
            string pipeline = agent.Descriptor.pipeline;

            agentsById[id] = agent;
            agentsByName[name] = agent;

            if (!string.IsNullOrEmpty(pipeline))
            {
                if (!agentsByPipeline.ContainsKey(pipeline))
                {
                    agentsByPipeline[pipeline] = new List<AgentBase>();
                }
                agentsByPipeline[pipeline].Add(agent);
            }

            Debug.Log($"[AgentRegistry] Registered: {name} (ID: {id}, Tier: {agent.Tier}, Pipeline: {pipeline})");
        }

        /// <summary>
        /// Unregisters an agent from the global registry.
        /// Called automatically by AgentBase.OnDestroy().
        /// </summary>
        public void UnregisterAgent(AgentBase agent)
        {
            if (agent == null) return;

            string id = agent.Descriptor.agentId;
            string name = agent.Descriptor.agentName;
            string pipeline = agent.Descriptor.pipeline;

            agentsById.Remove(id);
            agentsByName.Remove(name);

            if (!string.IsNullOrEmpty(pipeline) && agentsByPipeline.ContainsKey(pipeline))
            {
                agentsByPipeline[pipeline].Remove(agent);
            }

            Debug.Log($"[AgentRegistry] Unregistered: {name}");
        }

        /// <summary>
        /// Looks up an agent by its unique ID.
        /// </summary>
        public AgentBase GetAgentById(string id)
        {
            agentsById.TryGetValue(id, out AgentBase agent);
            return agent;
        }

        /// <summary>
        /// Looks up an agent by its name.
        /// </summary>
        public AgentBase GetAgentByName(string name)
        {
            agentsByName.TryGetValue(name, out AgentBase agent);
            return agent;
        }

        /// <summary>
        /// Returns all agents in a given pipeline (e.g., "world", "character", "narrative").
        /// </summary>
        public List<AgentBase> GetAgentsByPipeline(string pipeline)
        {
            if (agentsByPipeline.TryGetValue(pipeline, out List<AgentBase> agents))
            {
                return new List<AgentBase>(agents);
            }
            return new List<AgentBase>();
        }

        /// <summary>
        /// Returns all agents at a given thermal tier.
        /// </summary>
        public List<AgentBase> GetAgentsByTier(ThermalTier tier)
        {
            var result = new List<AgentBase>();
            foreach (var agent in agentsById.Values)
            {
                if (agent.Tier == tier)
                {
                    result.Add(agent);
                }
            }
            return result;
        }

        /// <summary>
        /// Returns a snapshot of all registered agents for debugging.
        /// </summary>
        public List<AgentBase> GetAllAgents()
        {
            return new List<AgentBase>(agentsById.Values);
        }

        /// <summary>
        /// Returns the total count of registered agents.
        /// </summary>
        public int AgentCount => agentsById.Count;

        /// <summary>
        /// Logs a status report of all registered agents.
        /// </summary>
        public void LogRegistryStatus()
        {
            Debug.Log($"[AgentRegistry] === STATUS REPORT === ({AgentCount} agents registered)");
            foreach (var agent in agentsById.Values)
            {
                Debug.Log($"  {agent}");
            }
        }
    }
}
