# Arkham Island — Creator & Creator-AI Guide

Welcome to the **Arkham Island VR Thermal Cascade System**! This guide is designed for game designers, narrative directors, level artists, and AI prompt engineers working on *Arkham Island*. 

---

## 1. Engine & Prerequisites

| Requirement | Specification |
|-------------|---------------|
| **Game Engine** | **Unity 6000.5.x** (or Unity 6 / 2026 LTS release) |
| **Render Pipeline** | **Universal Render Pipeline (URP 17.x)** |
| **VR / XR Plugin** | Unity OpenXR Plugin & Input System 1.19+ |
| **Navigation** | Unity AI Navigation Package (`com.unity.ai.navigation`) |
| **Asset Loader** | Unity glTFast Package (`com.unity.cloud.gltfast`) |

> 💡 **Key Note for Creators:**  
> The core system is pre-configured to run on Unity PCVR and Meta Quest targets. No external python backend or API key management is required for developer testing — all thermal cascade agents, shared state contracts, and deterministic validators run directly within Unity's C# runtime.

---

## 2. Architecture Top-Level Spider Graph

The system uses a **Thermal Cascade Model**. "Hot" creative agents generate high-variance horror concepts, while "Cool" deterministic agents enforce VR frame budgets, physics, and comfort.

```
                         ┌─────────────────────────────────┐
                         │   PIPELINE ORCHESTRATOR AGENT   │
                         │   (Workflow & Context Control)  │
                         └────────────────┬────────────────┘
                                          │
          ┌───────────────────────────────┼───────────────────────────────┐
          │                               │                               │
          ▼                               ▼                               ▼
  ┌───────────────┐               ┌───────────────┐               ┌───────────────┐
  │ World Design  │               │   Character   │               │   Narrative   │
  │   Pipeline    │               │   Pipeline    │               │   Pipeline    │
  │ ┌───────────┐ │               │ ┌───────────┐ │               │ ┌───────────┐ │
  │ │MapDesign  │ │               │ │CharPicker │ │               │ │MythosWeavr│ │
  │ │(Hot 1.0)  │ │               │ │(Hot 0.95) │ │               │ │(Hot 1.0)  │ │
  │ └─────┬─────┘ │               │ └─────┬─────┘ │               │ └─────┬─────┘ │
  │       │       │               │       │       │               │       │       │
  │ ┌─────▼─────┐ │               │ ┌─────▼─────┐ │               │ ┌─────▼─────┐ │
  │ │PhysicAnchor││               │ │PCEngineer │ │               │ │SanityAnchor││
  │ │(Cool 0.1) │ │               │ │(Cool 0.25)│ │               │ │(Cool 0.15)│ │
  │ └───────────┘ │               │ └───────────┘ │               │ └───────────┘ │
  └───────┬───────┘               └───────┬───────┘               └───────┬───────┘
          │                               │                               │
          └───────────────────────────────┼───────────────────────────────┘
                                          │
        ┌─────────────────────────────────┼─────────────────────────────────┐
        │ (Parallel Execution)            │                                 │
        ▼                                 ▼                                 ▼
┌───────────────┐                 ┌───────────────┐                 ┌───────────────┐
│Audio Pipeline │                 │Render Pipeline│                 │Haptic Pipeline│
│ ┌───────────┐ │                 │ ┌───────────┐ │                 │ ┌───────────┐ │
│ │PsychWeaver│ │                 │ │AtmosDirect│ │                 │ │EldritchObj│ │
│ │(Hot 0.85) │ │                 │ │(Hot 0.9)  │ │                 │ │(Hot 0.9)  │ │
│ └─────┬─────┘ │                 │ └─────┬─────┘ │                 │ └─────┬─────┘ │
│ ┌─────▼─────┐ │                 │ ┌─────▼─────┐ │                 │ ┌─────▼─────┐ │
│ │HRTFSpatial│ │                 │ │VRShaderOpt│ │                 │ │HapticErgo │ │
│ │(Cool 0.1) │ │                 │ │(Cool 0.05)│ │                 │ │(Cool 0.2) │ │
│ └───────────┘ │                 │ └───────────┘ │                 │ └───────────┘ │
└───────┬───────┘                 └───────┬───────┘                 └───────┬───────┘
        │                                 │                                 │
        └─────────────────────────────────┼─────────────────────────────────┘
                                          │
                                          ▼
                         ┌─────────────────────────────────┐
                         │    DETERMINISTIC VALIDATORS     │
                         │(Frame Time, NavMesh, Decibels)  │
                         └────────────────┬────────────────┘
                                          │
                                          ▼
                         ┌─────────────────────────────────┐
                         │     INTEGRATION REVIEW AGENT    │
                         │    (Final Review & Verdict)     │
                         └─────────────────────────────────┘
```

---

## 3. How to Prompt & Trigger the System

You do not prompt the system like a simple chatbot ("Build a room"). Instead, you supply **Causal Intent Prompts ("Why-Chain")** through Unity inspectable scriptable objects or code.

### Standard Why-Chain Prompting Format

Every trigger must answer two questions:
1. **Upstream Intent (Why-In):** *"Why are we generating this?"* (e.g. "Player's sanity dropped below 30% inside the flooded basement.")
2. **Downstream Goal (Why-Out):** *"What must the receiving agent accomplish?"* (e.g. "Produce a vertigo effect without clipping the player through walls.")

### Example Code Trigger

```csharp
// Triggering a World & Environment Transformation:
var envelope = WhyChainEnvelope.Create(
    source: new AgentDescriptor("creator_input", "LevelDesigner", 1.0f, "world"),
    target: new AgentDescriptor("map_agent", "MapDesignAgent", 1.0f, "world"),
    upstreamIntent: "Transform Innsmouth coastline elevation data into a decaying Lovecraftian ritual site.",
    downstreamIntent: "Generate non-Euclidean cliff geometry while preserving walkable teleportation bounds."
);

// Inject into the Central Orchestrator
PipelineOrchestratorAgent.Instance.BeginCascade();
```

---

## 4. Conflict Arbitration: Precedence Rules

When a creative (Hot) agent and an engine (Cool) agent disagree, the system uses strict **Precedence Rules**:

$$\text{Safety Constraints} > \text{Quest Correctness} > \text{VR Performance} > \text{Narrative} > \text{Aesthetics}$$

- **Haptics:** `HapticErgonomicsAgent` overrides `EldritchObjectAgent` if vibrations exceed 30 seconds.
- **Rendering:** `VRShaderOptimizationAgent` rejects `AtmosphereDirectorAgent` volumetric fog if rendering takes $>11.11\text{ms}$.
- **Narrative:** `SanityEngineAnchorAgent` vetoes hallucinations that block critical quest items.
- **World:** `PhysicsMeshAnchorAgent` rejects impossible shapes that break VR teleportation or NavMesh connectivity.

The Hot agent will automatically revise its output to fit within the Cool agent's limits.

---

## 5. When to Perform Human Review

The AI handles standard iteration automatically up to **3 retries**. Human review is required only under specific conditions:

```
[Hot Proposal] ──▸ [Cool Check] ──▸ [Validators] ──▸ [Review Agent]
                                                          │
                    ┌─────────────────────────────────────┴─────────────────────────────────────┐
                    ▼                                                                           ▼
           ✅ Pass (< 3 retries)                                                      ❌ Human Review Flagged
         Proceeds to Gameplay                                                    (Requires Human Intervention)
```

### Flags Requiring Human Action

1. **Max Retries Exhausted (Escalation Status: `HumanEscalation`):**
   - The Hot agent and Cool agent entered an unresolved conflict loop (e.g. artistic fog vs. Quest 3 GPU budget).
   - *Action:* Lower the atmospheric fog density setting in `AtmosphereDirectorAgent` or increase the GPU budget.

2. **Photosensitive / Motion Sickness Warnings:**
   - Flash frequency exceeding 3Hz or VR movement velocity over $5.0\text{ m/s}$.
   - *Action:* Approve an accessibility override or adjust dynamic vignette strength.

3. **Quest Soft-Lock Alert:**
   - Low sanity (<15%) overlaps with a non-repeatable quest step.
   - *Action:* Verify in the Unity inspector that quest-critical items are assigned to layer `8 (QuestInteractables)`.

---

## 6. Quick Creator Checklist

- [ ] Ensure **Unity 6000.5.x** with URP is installed.
- [ ] Drag `islandseg.glb` into `Assets/Models/Island/`.
- [ ] Add `PipelineOrchestratorAgent`, `ValidatorRunner`, and `IntegrationReviewAgent` to your active scene.
- [ ] Run `IslandSegmentProcessor.ProcessSegments()` in the Unity Editor menu.
- [ ] Inspect the output in the custom Unity Editor Inspector window (`AgentPipelineInspector`).
