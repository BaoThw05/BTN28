# Architecture Index

This index is not the full contract. Do not implement from this file alone; follow the artifact paths below.

## Global artifacts

- `project-structure.md` — scene/module boundaries and project type.
- `tech-stack.md` — existing Unity version, packages, input, navigation, and test framework.
- `unit_graph.yaml` — units, scene triggers, serialized entry points, and public method signatures.
- `wire_contracts.yaml` — external contract inventory; empty for this project scope.
- `shared_modules.yaml` — shared project-authored code; empty for this project scope.
- `cross_unit_state.yaml` — implicit state flows; empty for this project scope.

## Implementation Guide

### Unit: start-menu

- external trigger: Start scene page; Play button.
- read `units/start-menu/behavior.yaml` for scene transition behavior and failure path.
- read `units/start-menu/bindings.yaml` for Unity scene/button wiring.
- read `units/start-menu/unit_decomposition.yaml` for advisory split candidates only; `commit: false` is intentional.
- filter `unit_graph.yaml` to `start-menu`; preserve `MainMenu.PlayGame` and its scene target.
- before DONE report artifacts read, preserved scene transition, and Unity play/build evidence.

### Unit: player-movement

- external trigger: Kitchen scene per-frame player input.
- read `units/player-movement/behavior.yaml` for grounding, movement direction, gravity, and jump behavior.
- read `units/player-movement/bindings.yaml` for serialized CharacterController, GroundCheck, layer mask, and legacy input names.
- read `units/player-movement/unit_decomposition.yaml` for advisory split candidates only.
- filter global artifacts by unit; there are no shared/state/wire rows for this unit.
- before DONE report artifacts read, preserved input/physics behavior, and play-mode evidence.

### Unit: camera-look

- external trigger: Kitchen scene per-frame mouse input.
- read `units/camera-look/behavior.yaml` for cursor lock, clamping, and rotation behavior.
- read `units/camera-look/bindings.yaml` for legacy mouse-axis configuration.
- read `units/camera-look/unit_decomposition.yaml` for advisory split candidates only.
- filter global artifacts by unit; there are no shared/state/wire rows for this unit.
- before DONE report artifacts read, preserved camera behavior, and play-mode evidence.

### Unit: customer-order-ai

- external trigger: Kitchen scene NavMeshAgent customer.
- read `units/customer-order-ai/behavior.yaml` for destination, arrival threshold, and animation behavior.
- read `units/customer-order-ai/bindings.yaml` for NavMeshAgent, Animator, order point, and serialized scene references.
- read `units/customer-order-ai/unit_decomposition.yaml` for advisory split candidates only.
- filter global artifacts by unit; there are no shared/state/wire rows for this unit.
- before DONE report artifacts read, preserved arrival/order behavior, and play-mode evidence.
