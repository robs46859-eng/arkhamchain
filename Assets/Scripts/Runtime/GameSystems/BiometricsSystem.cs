using System;
using UnityEngine;

namespace ArkhamIsland.GameSystems
{
    /// <summary>
    /// Biometrics integration system for VR — tracks real-time physiological data
    /// from the player (heart rate, eye tracking, skin conductance) and maps it
    /// to in-game mechanics (stamina, visual distortion, sanity drain).
    /// 
    /// In the current scaffold, biometrics are simulated. The system is designed
    /// to plug into real VR biometric APIs (Meta Quest Pro, Valve Index, etc.)
    /// when hardware is available.
    /// </summary>
    public class BiometricsSystem : MonoBehaviour
    {
        public static BiometricsSystem Instance { get; private set; }

        [Header("Heart Rate")]
        [Tooltip("Current heart rate (BPM). Simulated in scaffold mode.")]
        [SerializeField] private float heartRate = 72f;

        [Tooltip("Heart rate baseline for the player.")]
        [SerializeField] private float heartRateBaseline = 72f;

        [Tooltip("Heart rate stress threshold that triggers gameplay effects.")]
        [SerializeField] private float stressThresholdBPM = 100f;

        [Header("Eye Tracking")]
        [Tooltip("Current gaze position (normalized screen coords).")]
        [SerializeField] private Vector2 gazePosition = new Vector2(0.5f, 0.5f);

        [Tooltip("Current pupil dilation (0.0–1.0).")]
        [Range(0f, 1f)]
        [SerializeField] private float pupilDilation = 0.5f;

        [Tooltip("Saccade frequency (rapid eye movements per second).")]
        [SerializeField] private float saccadeFrequency = 2f;

        [Header("Skin Conductance")]
        [Tooltip("Simulated galvanic skin response (μS). Higher = more arousal/stress.")]
        [SerializeField] private float skinConductance = 5f;

        [Header("Simulation Mode")]
        [Tooltip("Whether to simulate biometrics for testing (no real hardware).")]
        [SerializeField] private bool simulationMode = true;

        [Tooltip("Simulation: heart rate noise amplitude.")]
        [SerializeField] private float simNoiseAmplitude = 5f;

        /// <summary>Current heart rate in BPM.</summary>
        public float HeartRate => heartRate;

        /// <summary>Stress level normalized 0.0–1.0 based on heart rate.</summary>
        public float StressLevel => Mathf.Clamp01((heartRate - heartRateBaseline) / (stressThresholdBPM - heartRateBaseline));

        /// <summary>Current gaze position (normalized).</summary>
        public Vector2 GazePosition => gazePosition;

        /// <summary>Pupil dilation (0.0–1.0, higher = more dilated / fear response).</summary>
        public float PupilDilation => pupilDilation;

        /// <summary>Event fired when stress level crosses a threshold.</summary>
        public event Action<float> OnStressLevelChanged;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else if (Instance != this)
                Destroy(gameObject);
        }

        private void Update()
        {
            if (simulationMode)
            {
                SimulateBiometrics();
            }
            // In production, this would poll real VR biometric APIs
        }

        private void SimulateBiometrics()
        {
            // Simulate heart rate with Perlin noise
            float noise = Mathf.PerlinNoise(Time.time * 0.3f, 0f) * 2f - 1f;
            heartRate = heartRateBaseline + noise * simNoiseAmplitude;

            // Simulate pupil dilation based on ambient lighting (stub)
            pupilDilation = 0.5f + Mathf.Sin(Time.time * 0.5f) * 0.15f;

            // Simulate saccade frequency
            saccadeFrequency = 2f + Mathf.PerlinNoise(Time.time * 0.1f, 5f) * 3f;

            // Simulate gaze jitter
            gazePosition = new Vector2(
                0.5f + Mathf.Sin(Time.time * 1.2f) * 0.1f,
                0.5f + Mathf.Cos(Time.time * 0.8f) * 0.1f
            );
        }

        /// <summary>
        /// Injects a stress event (e.g., horror encounter) that spikes heart rate.
        /// </summary>
        public void InjectStressEvent(float intensityBPMDelta, float durationSeconds)
        {
            // In full implementation, this would ramp heart rate simulation
            heartRate += intensityBPMDelta;
            Debug.Log($"[BiometricsSystem] Stress event injected: +{intensityBPMDelta} BPM " +
                      $"for {durationSeconds}s. Current HR: {heartRate:F0} BPM");
        }

        /// <summary>
        /// Returns the stamina drain multiplier based on current biometric state.
        /// Higher stress = faster stamina drain.
        /// </summary>
        public float GetStaminaDrainMultiplier()
        {
            return 1f + StressLevel * 1.5f; // Up to 2.5x drain at max stress
        }
    }
}
