using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArkhamIsland.AgentFramework
{
    /// <summary>
    /// Audit log for all Why-Chain transactions across the Thermal Cascade network.
    /// Records every envelope acceptance, rejection, and processing event.
    /// 
    /// Used for debugging cascade failures — when a hallucination breaks a quest,
    /// or a collision mesh clips, trace the entire Why-Chain to find the intent gap.
    /// </summary>
    public class WhyChainAuditLog : MonoBehaviour
    {
        public static WhyChainAuditLog Instance { get; private set; }

        [Header("Configuration")]
        [Tooltip("Maximum number of audit entries to retain in memory.")]
        [SerializeField] private int maxEntries = 1000;

        [Tooltip("Whether to log entries to Unity Console in real-time.")]
        [SerializeField] private bool verboseConsoleLogging = true;

        /// <summary>Types of audit events.</summary>
        public enum AuditEventType
        {
            Accepted,
            Rejected,
            Processed,
            CascadeViolation
        }

        /// <summary>A single audit log entry.</summary>
        [Serializable]
        public class AuditEntry
        {
            public string timestamp;
            public AuditEventType eventType;
            public string transactionId;
            public string sourceAgentName;
            public float sourceTemperature;
            public string targetAgentName;
            public float targetTemperature;
            public string upstreamIntent;
            public string downstreamIntent;
            public string errorMessage;
        }

        private List<AuditEntry> entries = new List<AuditEntry>();

        /// <summary>Event fired when a new audit entry is recorded.</summary>
        public event Action<AuditEntry> OnAuditEntryAdded;

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
        /// Logs that an envelope was accepted by the target agent.
        /// </summary>
        public void LogAcceptance(WhyChainEnvelope envelope)
        {
            AddEntry(AuditEventType.Accepted, envelope, null);
        }

        /// <summary>
        /// Logs that an envelope was rejected by the target agent.
        /// </summary>
        public void LogRejection(WhyChainEnvelope envelope, string reason)
        {
            AddEntry(AuditEventType.Rejected, envelope, reason);
        }

        /// <summary>
        /// Logs that an envelope has been fully processed by an agent.
        /// </summary>
        public void LogProcessed(WhyChainEnvelope envelope)
        {
            AddEntry(AuditEventType.Processed, envelope, null);
        }

        /// <summary>
        /// Logs a temperature cascade violation.
        /// </summary>
        public void LogCascadeViolation(WhyChainEnvelope envelope, string description)
        {
            AddEntry(AuditEventType.CascadeViolation, envelope, description);
        }

        private void AddEntry(AuditEventType eventType, WhyChainEnvelope envelope, string error)
        {
            var entry = new AuditEntry
            {
                timestamp = DateTime.UtcNow.ToString("o"),
                eventType = eventType,
                transactionId = envelope?.transactionId ?? "NULL",
                sourceAgentName = envelope?.sourceAgent?.agentName ?? "NULL",
                sourceTemperature = envelope?.sourceAgent?.temperature ?? -1f,
                targetAgentName = envelope?.targetAgent?.agentName ?? "NULL",
                targetTemperature = envelope?.targetAgent?.temperature ?? -1f,
                upstreamIntent = envelope?.whyChain?.upstreamIntentReceived ?? "",
                downstreamIntent = envelope?.whyChain?.downstreamIntentPassed ?? "",
                errorMessage = error
            };

            entries.Add(entry);

            // Enforce max entries (circular buffer behavior)
            while (entries.Count > maxEntries)
            {
                entries.RemoveAt(0);
            }

            OnAuditEntryAdded?.Invoke(entry);

            if (verboseConsoleLogging)
            {
                string logLevel = eventType == AuditEventType.Rejected || eventType == AuditEventType.CascadeViolation
                    ? "ERROR" : "INFO";

                string msg = $"[WhyChainAudit][{logLevel}] {eventType}: " +
                             $"{entry.sourceAgentName} ({entry.sourceTemperature:F2}) → " +
                             $"{entry.targetAgentName} ({entry.targetTemperature:F2}) | " +
                             $"TX: {entry.transactionId}";

                if (!string.IsNullOrEmpty(error))
                {
                    msg += $" | ERROR: {error}";
                }

                if (logLevel == "ERROR")
                    Debug.LogError(msg);
                else
                    Debug.Log(msg);
            }
        }

        /// <summary>
        /// Returns all audit entries for a specific transaction ID.
        /// </summary>
        public List<AuditEntry> GetEntriesForTransaction(string transactionId)
        {
            return entries.FindAll(e => e.transactionId == transactionId);
        }

        /// <summary>
        /// Returns all audit entries of a specific event type.
        /// </summary>
        public List<AuditEntry> GetEntriesByType(AuditEventType eventType)
        {
            return entries.FindAll(e => e.eventType == eventType);
        }

        /// <summary>
        /// Returns the most recent N entries.
        /// </summary>
        public List<AuditEntry> GetRecentEntries(int count)
        {
            int start = Mathf.Max(0, entries.Count - count);
            return entries.GetRange(start, entries.Count - start);
        }

        /// <summary>
        /// Returns total counts by event type.
        /// </summary>
        public Dictionary<AuditEventType, int> GetSummary()
        {
            var summary = new Dictionary<AuditEventType, int>();
            foreach (AuditEventType type in Enum.GetValues(typeof(AuditEventType)))
            {
                summary[type] = 0;
            }
            foreach (var entry in entries)
            {
                summary[entry.eventType]++;
            }
            return summary;
        }

        /// <summary>
        /// Clears all audit entries.
        /// </summary>
        public void Clear()
        {
            entries.Clear();
            Debug.Log("[WhyChainAuditLog] Audit log cleared.");
        }

        /// <summary>
        /// Total number of audit entries.
        /// </summary>
        public int EntryCount => entries.Count;
    }
}
