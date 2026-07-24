using System;
using System.Collections.Generic;

namespace ArkhamIsland.AgentFramework
{
    /// <summary>
    /// Structured verdict returned by the IntegrationReviewAgent.
    /// Separates blocking issues from warnings and provides targeted revision requests.
    /// </summary>
    [Serializable]
    public sealed class ReviewResult
    {
        /// <summary>Whether the integration review passed all blocking checks.</summary>
        public bool Approved;

        /// <summary>Overall quality score (0.0–1.0).</summary>
        public float OverallScore;

        /// <summary>Issues that MUST be resolved before the cascade can complete.</summary>
        public List<ReviewIssue> BlockingIssues = new List<ReviewIssue>();

        /// <summary>Non-blocking issues that should be addressed.</summary>
        public List<ReviewIssue> Warnings = new List<ReviewIssue>();

        /// <summary>Targeted revision requests sent back to specific agents.</summary>
        public List<RevisionRequest> RevisionRequests = new List<RevisionRequest>();

        /// <summary>Timestamp of the review.</summary>
        public string ReviewTimestamp;

        /// <summary>Session/transaction this review covers.</summary>
        public string SessionId;

        /// <summary>Number of review passes already conducted.</summary>
        public int ReviewPassNumber;

        public bool HasBlockingIssues => BlockingIssues != null && BlockingIssues.Count > 0;

        public static ReviewResult Pass(float score, string sessionId, int pass)
        {
            return new ReviewResult
            {
                Approved = true,
                OverallScore = score,
                ReviewTimestamp = DateTime.UtcNow.ToString("o"),
                SessionId = sessionId,
                ReviewPassNumber = pass
            };
        }

        public static ReviewResult Fail(float score, string sessionId, int pass)
        {
            return new ReviewResult
            {
                Approved = false,
                OverallScore = score,
                ReviewTimestamp = DateTime.UtcNow.ToString("o"),
                SessionId = sessionId,
                ReviewPassNumber = pass
            };
        }
    }

    /// <summary>
    /// A specific issue identified during integration review.
    /// </summary>
    [Serializable]
    public sealed class ReviewIssue
    {
        /// <summary>Which review domain this issue falls under.</summary>
        public ReviewDomain Domain;

        /// <summary>Severity classification.</summary>
        public ConstraintSeverity Severity;

        /// <summary>The agent or system that produced the issue.</summary>
        public string SourceAgent;

        /// <summary>Human-readable description of the issue.</summary>
        public string Description;

        /// <summary>Specific metric or threshold that was violated (if applicable).</summary>
        public string ViolatedConstraint;

        /// <summary>The actual measured value.</summary>
        public string ActualValue;

        /// <summary>The expected/allowed value.</summary>
        public string ExpectedValue;
    }

    /// <summary>
    /// Domains evaluated by the integration review agent.
    /// </summary>
    public enum ReviewDomain
    {
        VRComfort,
        Accessibility,
        NarrativeCoherence,
        CrossSystemConsistency,
        GPUBudget,
        CPUBudget,
        MemoryBudget,
        DrawCallBudget,
        AudioIntensity,
        HapticIntensity,
        NavMeshValidity,
        CollisionValidity,
        HallucinationGameplaySafety,
        SanityQuestSafety,
        BiometricFeedbackLoop,
        AssetProvenance
    }

    /// <summary>
    /// A targeted revision request sent from the reviewer back to a specific agent.
    /// The orchestrator routes these to the correct pipeline for re-execution.
    /// </summary>
    [Serializable]
    public sealed class RevisionRequest
    {
        /// <summary>The agent that must revise its output.</summary>
        public string TargetAgent;

        /// <summary>The pipeline to re-trigger.</summary>
        public string TargetPipeline;

        /// <summary>Description of what must change.</summary>
        public string RevisionDescription;

        /// <summary>The constraint that prompted this revision.</summary>
        public string ConstraintReference;

        /// <summary>Priority (0 = must fix, 5 = optional improvement).</summary>
        public int Priority;

        /// <summary>Maximum budget the revised output may consume.</summary>
        public float BudgetCapMs;
    }
}
