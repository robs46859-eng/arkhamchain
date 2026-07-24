using System.Collections.Generic;
using UnityEngine;
using ArkhamIsland.AgentFramework;

namespace ArkhamIsland.Validators
{
    /// <summary>
    /// Runner for all deterministic validators.
    /// Executes non-AI deterministic code checks against ExperienceContext.
    /// Singleton pattern for global access during integration pipeline runs.
    /// </summary>
    public class ValidatorRunner : MonoBehaviour
    {
        public static ValidatorRunner Instance { get; private set; }

        private List<IDeterministicValidator> validators = new List<IDeterministicValidator>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                RegisterDefaultValidators();
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void RegisterDefaultValidators()
        {
            validators.Add(new PerformanceBudgetValidator());
            validators.Add(new NavMeshContinuityValidator());
            validators.Add(new CollisionValidator());
            validators.Add(new QuestReachabilityValidator());
            validators.Add(new HallucinationSafetyValidator());
            validators.Add(new AudioExposureValidator());
            validators.Add(new HapticIntensityValidator());
            validators.Add(new BiometricFeedbackLoopValidator());
            validators.Add(new AssetProvenanceValidator());

            Debug.Log($"[ValidatorRunner] Registered {validators.Count} deterministic validators.");
        }

        public void RegisterValidator(IDeterministicValidator validator)
        {
            if (validator != null && !validators.Contains(validator))
            {
                validators.Add(validator);
            }
        }

        /// <summary>
        /// Runs all registered deterministic validators against the shared ExperienceContext.
        /// </summary>
        public List<ConstraintViolation> RunAll(ExperienceContext context)
        {
            var results = new List<ConstraintViolation>();
            if (context == null) return results;

            foreach (var validator in validators)
            {
                var violations = validator.Validate(context);
                if (violations != null)
                {
                    results.AddRange(violations);
                }
            }

            Debug.Log($"[ValidatorRunner] Executed {validators.Count} validators. Found {results.Count} violations.");
            return results;
        }

        /// <summary>
        /// Runs validators registered for a specific scope ("world", "narrative", etc.).
        /// </summary>
        public List<ConstraintViolation> RunScope(ExperienceContext context, string scope)
        {
            var results = new List<ConstraintViolation>();
            if (context == null) return results;

            foreach (var validator in validators)
            {
                if (validator.TargetScope == scope || validator.TargetScope == "all")
                {
                    var violations = validator.Validate(context);
                    if (violations != null)
                    {
                        results.AddRange(violations);
                    }
                }
            }

            return results;
        }
    }
}
