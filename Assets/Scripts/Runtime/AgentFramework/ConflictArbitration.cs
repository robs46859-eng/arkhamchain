using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArkhamIsland.AgentFramework
{
    /// <summary>
    /// Conflict arbitration system with explicit precedence rules.
    ///
    /// Hot and cool agents disagree by design. This system defines which constraints
    /// take priority when they conflict, and enables cool agents to veto hot agent
    /// output while requesting revision rather than silently discarding creative intent.
    ///
    /// Precedence hierarchy (highest to lowest):
    ///   1. Safety constraints         — override all
    ///   2. Quest/gameplay correctness — override performance and aesthetics
    ///   3. VR performance budgets     — override narrative and aesthetics
    ///   4. Narrative continuity       — override aesthetics
    ///   5. Aesthetic intensity        — lowest priority
    ///
    /// When a cool agent vetoes, the hot agent must revise WITHIN the stated constraint,
    /// preserving as much creative intent as possible.
    /// </summary>
    public static class ConflictArbitration
    {
        /// <summary>
        /// Constraint priority levels. Higher numeric value = higher precedence.
        /// </summary>
        public enum ConstraintPriority
        {
            AestheticIntensity = 0,
            NarrativeContinuity = 1,
            PerformanceBudget = 2,
            QuestGameplayCorrectness = 3,
            SafetyConstraint = 4
        }

        /// <summary>
        /// A veto issued by a cool agent against a hot agent's output.
        /// Contains the constraint violated and a revision directive.
        /// </summary>
        [Serializable]
        public sealed class Veto
        {
            public string issuingAgent;
            public float issuingTemperature;
            public string targetAgent;
            public float targetTemperature;
            public ConstraintPriority priority;
            public string constraintViolated;
            public string revisionDirective;
            public float maxAllowedValue;
            public string timestampUtc;
        }

        /// <summary>
        /// A revision response from a hot agent after receiving a veto.
        /// </summary>
        [Serializable]
        public sealed class RevisionResponse
        {
            public string revisingAgent;
            public string originalVetoId;
            public string revisedDescription;
            public float creativeIntentPreserved;
            public bool meetsConstraint;
            public int revisionAttempt;
        }

        /// <summary>
        /// Known veto relationships: which cool agents can veto which hot agents.
        /// </summary>
        private static readonly Dictionary<string, string[]> VetoAuthority = new Dictionary<string, string[]>
        {
            // HapticErgonomicsAgent can veto EldritchObjectAgent
            { "HapticErgonomicsAgent", new[] { "EldritchObjectAgent" } },
            // VRShaderOptimizationAgent can reject AtmosphereDirectorAgent
            { "VRShaderOptimizationAgent", new[] { "AtmosphereDirectorAgent" } },
            // SanityEngineAnchorAgent can veto hallucinations that block quests
            { "SanityEngineAnchorAgent", new[] { "CosmicMythosWeaverAgent" } },
            // PhysicsMeshAnchorAgent can reject impossible geometry
            { "PhysicsMeshAnchorAgent", new[] { "MapDesignAgent" } },
            // PCEngineerAgent can reject character designs that break IK
            { "PCEngineerAgent", new[] { "CharacterPickerAgent" } },
            // HRTFSpatialAudioAgent can reject unsafe audio designs
            { "HRTFSpatialAudioAgent", new[] { "PsychoacousticWeaverAgent" } }
        };

        /// <summary>
        /// Checks whether a cool agent has veto authority over a hot agent.
        /// </summary>
        public static bool HasVetoAuthority(string coolAgentName, string hotAgentName)
        {
            if (VetoAuthority.TryGetValue(coolAgentName, out string[] targets))
            {
                foreach (string target in targets)
                {
                    if (target == hotAgentName) return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Issues a veto from a cool agent against a hot agent's output.
        /// The hot agent must revise within the stated constraint.
        /// </summary>
        public static Veto IssueVeto(
            string coolAgent, float coolTemp,
            string hotAgent, float hotTemp,
            ConstraintPriority priority,
            string constraintViolated,
            string revisionDirective,
            float maxAllowedValue = float.MaxValue)
        {
            if (!HasVetoAuthority(coolAgent, hotAgent))
            {
                Debug.LogError($"[ConflictArbitration] {coolAgent} does NOT have veto authority over {hotAgent}.");
                return null;
            }

            var veto = new Veto
            {
                issuingAgent = coolAgent,
                issuingTemperature = coolTemp,
                targetAgent = hotAgent,
                targetTemperature = hotTemp,
                priority = priority,
                constraintViolated = constraintViolated,
                revisionDirective = revisionDirective,
                maxAllowedValue = maxAllowedValue,
                timestampUtc = DateTime.UtcNow.ToString("o")
            };

            Debug.Log($"[ConflictArbitration] VETO: {coolAgent} → {hotAgent} | " +
                      $"Priority: {priority} | Constraint: {constraintViolated} | " +
                      $"Directive: {revisionDirective}");

            return veto;
        }

        /// <summary>
        /// Resolves a conflict between two constraints by precedence.
        /// Returns true if constraintA wins over constraintB.
        /// </summary>
        public static bool Resolves(ConstraintPriority constraintA, ConstraintPriority constraintB)
        {
            return (int)constraintA >= (int)constraintB;
        }

        /// <summary>
        /// Returns a human-readable precedence explanation.
        /// </summary>
        public static string ExplainPrecedence(ConstraintPriority winner, ConstraintPriority loser)
        {
            return $"{winner} (priority {(int)winner}) overrides {loser} (priority {(int)loser}). " +
                   "Safety > Quest > Performance > Narrative > Aesthetic.";
        }
    }
}
