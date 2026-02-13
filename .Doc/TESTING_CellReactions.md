# Cell Reactions Runtime Check (Draft)

## Scope
This document covers **manual runtime checks** for the current Cell Reactions infrastructure (DecisionService + MonoBehaviour reaction components). **Movement integration is not implemented yet**, so robot behavior will not change in Play Mode.

## Preconditions
- Open the project in Unity.
- Load a scene that instantiates level objects (e.g., `Assets/Scenes/GameScene.unity`).
- Make sure level data contains objects with `objectTypeId` values `Wall`, `Door`, and `Button` where applicable.

## What Is Implemented Now
- `ObjectReactionComponent`-based reactions (`WallReaction`, `DoorReaction`, `ButtonReaction`).
- `MovementDecisionService` uses **components on runtime object instances** to resolve object reactions.
- `LevelRuntimeManager` auto-attaches reaction components to instantiated objects if the prefab does not already include them.

## Manual Checks in Play Mode
1. **Component attachment**
   - Enter Play Mode.
   - In the Hierarchy, select an instantiated object (e.g., `Wall_2_3`, `Door_1_4`, `Button_0_2`).
   - Verify the corresponding component is present:
     - `Wall` -> `WallReaction`
     - `Door` -> `DoorReaction`
     - `Button` -> `ButtonReaction`

2. **Door state parameters**
   - In the Level asset, set door parameters in `GridObject.parameters`:
     - `isOpen` = `true` or `false`, **or**
     - `state` = `open` or `closed`
   - (Note: there is no runtime toggle yet. This only affects evaluation logic when integration is added.)

3. **Button target parameters**
   - In the Level asset, set button parameters:
     - `targetId` = `<objectInstanceId>` of a door
     - or `targetIds` = `door_1,door_2` (comma or semicolon separated)

## Expected Results (Current Stage)
- Components are visible on instantiated objects at runtime.
- No movement behavior changes yet (DecisionService not wired into movement).

## Known Limitations
- Movement is not gated by `MovementDecisionService` yet.
- Button does not toggle door state yet (event wiring not implemented).

## Next Step After Integration
Once movement is wired, add a quick runtime test:
- Move toward a `Wall` or closed `Door` -> expect bounce/stop and no cell transition.
- Move onto `Button` -> expect door state toggle by target ID.
- Move onto `Water/Ice` -> expect speed modifier behavior.
