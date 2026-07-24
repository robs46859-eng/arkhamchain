using UnityEngine;
using UnityEditor;
using ArkhamIsland.AgentFramework;

namespace ArkhamIsland.Editor
{
    /// <summary>
    /// Custom inspector for AgentPipeline that displays the thermal cascade
    /// chain with visual temperature indicators and status reporting.
    /// </summary>
    [CustomEditor(typeof(AgentPipeline))]
    public class AgentPipelineInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            AgentPipeline pipeline = (AgentPipeline)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Thermal Cascade Visualization", EditorStyles.boldLabel);

            var chain = pipeline.GetAgentChain();
            if (chain != null && chain.Count > 0)
            {
                for (int i = 0; i < chain.Count; i++)
                {
                    var agent = chain[i];
                    if (agent == null) continue;

                    var descriptor = agent.Descriptor;
                    Color tierColor = GetTierColor(agent.Tier);

                    // Draw colored box for each agent
                    var originalColor = GUI.backgroundColor;
                    GUI.backgroundColor = tierColor;

                    EditorGUILayout.BeginVertical("box");
                    GUI.backgroundColor = originalColor;

                    EditorGUILayout.LabelField(
                        $"[{ThermalTierUtils.GetLabel(agent.Tier)}] {descriptor.agentName}",
                        EditorStyles.boldLabel);
                    EditorGUILayout.LabelField($"Temperature: {descriptor.temperature:F2}");
                    EditorGUILayout.LabelField($"Pending: {agent.PendingCount} | Processed: {agent.ProcessedCount}");

                    EditorGUILayout.EndVertical();

                    // Draw cascade arrow between agents
                    if (i < chain.Count - 1)
                    {
                        EditorGUILayout.LabelField("     ▼ WHY-CHAIN ▼", EditorStyles.centeredGreyMiniLabel);
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No agents in the pipeline chain.", MessageType.Info);
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Log Pipeline Status"))
            {
                Debug.Log(pipeline.GetStatusReport());
            }

            if (Application.isPlaying && GUILayout.Button("Tick Pipeline"))
            {
                pipeline.Tick();
            }
        }

        private Color GetTierColor(ThermalTier tier)
        {
            switch (tier)
            {
                case ThermalTier.Hot: return new Color(1f, 0.3f, 0.2f, 0.4f);
                case ThermalTier.Balanced: return new Color(1f, 0.8f, 0.2f, 0.4f);
                case ThermalTier.Cool: return new Color(0.2f, 0.6f, 1f, 0.4f);
                default: return Color.gray;
            }
        }
    }
}
