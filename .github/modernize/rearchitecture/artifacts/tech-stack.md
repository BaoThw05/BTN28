# Technology Stack

- Engine/runtime: Unity `6000.3.7f1`.
- Language: C# scripts compiled by Unity's generated `Assembly-CSharp` assembly.
- Rendering: Universal Render Pipeline package `17.3.0` is configured as the active custom render pipeline; HDRP packages are also present in the manifest.
- UI/text: Unity UI `2.0.0` and TextMesh Pro assets/examples.
- Input: legacy `UnityEngine.Input` / `ProjectSettings/InputManager.asset` axes (`Horizontal`, `Vertical`, `Mouse X`, `Mouse Y`) and button (`Jump`). The Input System package `1.18.0` is installed but the project scripts inspected here use the legacy API.
- Navigation: `com.unity.ai.navigation` `2.0.9`, `NavMeshAgent` in `CustomerAI`.
- Animation: Unity animation module and Animator parameters `IsWalking` and `IsOrdering` referenced by `CustomerAI`.
- Testing: Unity Test Framework `1.6.0` is installed; no project-specific test scripts were found in the inspected source scope.
- Build configuration: `ProjectSettings/EditorBuildSettings.asset`, `ProjectSettings/ProjectSettings.asset`, `ProjectSettings/GraphicsSettings.asset`, and `ProjectSettings/QualitySettings.asset`.

Generated Unity caches (`Library`, `Temp`, `obj`, and generated solution files) are excluded from architecture source anchors.
