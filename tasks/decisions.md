# MetroidJumper — Decisions

## 2D physics (Rigidbody2D/Collider2D) over 3D
The player/floor were initially set up with 3D `Rigidbody`/`BoxCollider` (and the player had no collider at all). Switched to `Rigidbody2D`/`CapsuleCollider2D`/`BoxCollider2D` since the project is URP-2D with the Physics2D module and 2D packages installed — 3D physics would've meant Z-locking everywhere for no benefit. Done via an Editor menu tool (`Tools/Senna/Setup Player Movement (2D)`) rather than hand-editing the `.unity` scene YAML, since component add/remove touches fileIDs that Unity should own.

## New Input System via `Keyboard.current`, no Input Actions asset yet
`ProjectSettings/ProjectSettings.asset` has `activeInputHandler: 1` (Input System only — legacy `Input` class is disabled). For this first movement pass, reading `Keyboard.current` directly in `Update()` was simpler than wiring the `InputSystem_Actions` asset. Revisit if/when more actions are needed (the asset still has the default FPS-template bindings and needs trimming per the approved plan).
