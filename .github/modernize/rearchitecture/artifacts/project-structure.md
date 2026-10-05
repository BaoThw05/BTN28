# Project Structure

## Project type

Unity 6.0.3f1 game project. The runtime is a scene-driven 3D game with UI/menu and gameplay scenes. It is not a web backend, library, or CLI.

## Runtime scenes

- `Assets/Scenes/Start.unity` — start/menu scene with a `MainMenu` component and a Play button.
- `Assets/Scenes/Kitchen.unity` — gameplay scene with the player controller, mouse look, customer navigation, NavMesh, and order point.
- `Assets/Scenes/Town.unity` — environment scene currently not wired into the build list.

## Code layout

- `Assets/script/` — project gameplay scripts:
  - `MainMenu.cs` — scene transition from menu to Kitchen.
  - `PlayerMovement.cs` — legacy Input Manager movement, gravity, jump, and CharacterController movement.
  - `MouseMovement.cs` — legacy mouse look and cursor lock.
  - `CustomerAI.cs` — NavMeshAgent movement and order animation state.
- `Assets/TextMesh Pro/Examples & Extras/Scripts/` — imported package/example scripts; not project gameplay scope.
- `Assets/Kevin Iglesias/` and `Assets/TutorialInfo/` — imported demo/tutorial scripts.

## Layers

The project uses the `Ground` layer for player grounding and has a `streets` layer for environment content. The player scene binding currently serializes `groundMask` to `Ground`.

## Build entry

`ProjectSettings/EditorBuildSettings.asset` currently lists only `Assets/Scenes/Kitchen.unity`. This does not match the presence of the Start menu scene and should be corrected before a playable build is validated.
