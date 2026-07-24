using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArkhamIsland.AgentFramework
{
    /// <summary>
    /// Bounded escalation loop for the Thermal Cascade.
    ///
    /// Flow:
    ///   Hot proposal → Cool constraint review → Deterministic validation →
    ///   Integration review → Targeted revision request → Max 2–3 retries →
    ///   Fallback or human escalation.
    ///
    /// Without a retry limit, agents may enter an endless aesthetic-versus-performance loop.
    /// </summary>
    public class EscalationLoop : MonoBehaviour
    {
        public static EscalationLoop Instance { get; private set; }

        [Header("Retry Configuration")]
        [Tooltip("Maximum revision attempts before fallback.")]
        [SerializeField] private int maxRetries = 3;

        [Tooltip("Maximum total escalation time before forced fallback (seconds).")]
        [SerializeField] private float maxEscalationTimeSec = 30f;

        [Header("Fallback Configuration")]
        [Tooltip("Whether to use safe defaults when escalation exhausts retries.")]
        [SerializeField] private bool useSafeFallbacks = true;

        [Tooltip("Whether to notify a human operator when escalation fails.")]
        [SerializeField] private bool notifyHumanOnFailure = true;

        /// <summary>Event fired when an escalation exhausts retries and falls back.</summary>
        public event Action<EscalationRecord> OnEscalationFallback;

        /// <summary>Event fired when human escalation is requested.</summary>
        public event Action<EscalationRecord> OnHumanEscalation;

        /// <summary>Active escalation records indexed by pipeline name.</summary>
        private Dictionary<string, EscalationRecord> activeEscalations
            = new Dictionary<string, EscalationRecord>();

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
        /// Begins tracking an escalation loop for a pipeline.
        /// Call when a cool agent vetoes a hot agent's output.
        /// </summary>
        public EscalationRecord BeginEscalation(
            string pipelineName,
            string hotAgent,
            string coolAgent,
            string constraint,
            string initialProposal)
        {
            var record = new EscalationRecord
            {
                pipelineName = pipelineName,
                hotAgentName = hotAgent,
                coolAgentName = coolAgent,
                constraintViolated = constraint,
                startTimeUtc = DateTime.UtcNow.ToString("o"),
                startTimeUnscaled = Time.unscaledTime,
                currentAttempt = 0,
                maxAttempts = maxRetries,
                status = EscalationStatus.InProgress,
                attempts = new List<EscalationAttempt>()
            };

            record.attempts.Add(new EscalationAttempt
            {
                attemptNumber = 0,
                proposalDescription = initialProposal,
                result = AttemptResult.Rejected,
                rejectionReason = constraint
            });

            activeEscalations[pipelineName] = record;

            Debug.Log($"[EscalationLoop] Started escalation: {pipelineName} " +
                      $"({hotAgent} ↔ {coolAgent}). Max retries: {maxRetries}");

            return record;
        }

        /// <summary>
        /// Records a revision attempt. Returns the status after this attempt.
        /// </summary>
        public EscalationStatus RecordAttempt(
            string pipelineName,
            string revisedProposal,
            bool meetsConstraint,
            string rejectionReason = null)
        {
            if (!activeEscalations.TryGetValue(pipelineName, out EscalationRecord record))
            {
                Debug.LogError($"[EscalationLoop] No active escalation for {pipelineName}.");
                return EscalationStatus.Failed;
            }

            record.currentAttempt++;

            var attempt = new EscalationAttempt
            {
                attemptNumber = record.currentAttempt,
                proposalDescription = revisedProposal,
                result = meetsConstraint ? AttemptResult.Accepted : AttemptResult.Rejected,
                rejectionReason = rejectionReason
            };
            record.attempts.Add(attempt);

            if (meetsConstraint)
            {
                record.status = EscalationStatus.Resolved;
                Debug.Log($"[EscalationLoop] RESOLVED: {pipelineName} after {record.currentAttempt} attempt(s).");
                return EscalationStatus.Resolved;
            }

            // Check time limit
            float elapsed = Time.unscaledTime - record.startTimeUnscaled;
            if (elapsed > maxEscalationTimeSec)
            {
                return HandleExhaustion(record, "Time limit exceeded");
            }

            // Check retry limit
            if (record.currentAttempt >= maxRetries)
            {
                return HandleExhaustion(record, "Maximum retries exhausted");
            }

            Debug.Log($"[EscalationLoop] Attempt {record.currentAttempt}/{maxRetries} failed " +
                      $"for {pipelineName}: {rejectionReason}. Requesting revision.");
            return EscalationStatus.InProgress;
        }

        private EscalationStatus HandleExhaustion(EscalationRecord record, string reason)
        {
            Debug.LogWarning($"[EscalationLoop] EXHAUSTED: {record.pipelineName}. {reason}.");

            if (useSafeFallbacks)
            {
                record.status = EscalationStatus.FallenBack;
                record.fallbackReason = reason;
                OnEscalationFallback?.Invoke(record);
                Debug.Log($"[EscalationLoop] Applying safe fallback for {record.pipelineName}.");
                return EscalationStatus.FallenBack;
            }

            if (notifyHumanOnFailure)
            {
                record.status = EscalationStatus.HumanEscalation;
                record.fallbackReason = reason;
                OnHumanEscalation?.Invoke(record);
                Debug.LogWarning($"[EscalationLoop] HUMAN ESCALATION requested for {record.pipelineName}.");
                return EscalationStatus.HumanEscalation;
            }

            record.status = EscalationStatus.Failed;
            return EscalationStatus.Failed;
        }

        /// <summary>
        /// Returns the current escalation record for a pipeline (null if none active).
        /// </summary>
        public EscalationRecord GetEscalation(string pipelineName)
        {
            activeEscalations.TryGetValue(pipelineName, out EscalationRecord record);
            return record;
        }

        /// <summary>
        /// Clears all completed/failed escalation records.
        /// </summary>
        public void ClearCompleted()
        {
            var toRemove = new List<string>();
            foreach (var kvp in activeEscalations)
            {
                if (kvp.Value.status != EscalationStatus.InProgress)
                    toRemove.Add(kvp.Key);
            }
            foreach (string key in toRemove)
                activeEscalations.Remove(key);
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // Escalation Types
    // ══════════════════════════════════════════════════════════════════════════

    public enum EscalationStatus
    {
        InProgress,
        Resolved,
        FallenBack,
        HumanEscalation,
        Failed
    }

    public enum AttemptResult
    {
        Accepted,
        Rejected
    }

    [Serializable]
    public sealed class EscalationRecord
    {
        public string pipelineName;
        public string hotAgentName;
        public string coolAgentName;
        public string constraintViolated;
        public string startTimeUtc;
        public float startTimeUnscaled;
        public int currentAttempt;
        public int maxAttempts;
        public EscalationStatus status;
        public string fallbackReason;
        public List<EscalationAttempt> attempts;
    }

    [Serializable]
    public sealed class EscalationAttempt
    {
        public int attemptNumber;
        public string proposalDescription;
        public AttemptResult result;
        public string rejectionReason;
    }
}
