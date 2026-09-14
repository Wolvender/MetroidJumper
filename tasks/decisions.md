# MetroidJumper — Decisions

## 2D physics (Rigidbody2D/Collider2D) over 3D
The player/floor were initially set up with 3D `Rigidbody`/`BoxCollider` (and the player had no collider at all). Switched to `Rigidbody2D`/`CapsuleCollider2D`/`BoxCollider2D` since the project is URP-2D with the Physics2D module and 2D packages installed — 3D physics would've meant Z-locking everywhere for no benefit. Done via an Editor menu tool (`Tools/Senna/Setup Player Movement (2D)`) rather than hand-editing the `.unity` scene YAML, since component add/remove touches fileIDs that Unity should own.

## New Input System via `Keyboard.current`, no Input Actions asset yet
`ProjectSettings/ProjectSettings.asset` has `activeInputHandler: 1` (Input System only — legacy `Input` class is disabled). For this first movement pass, reading `Keyboard.current` directly in `Update()` was simpler than wiring the `InputSystem_Actions` asset. Revisit if/when more actions are needed (the asset still has the default FPS-template bindings and needs trimming per the approved plan).

## Hand-rolled `CameraFollow.cs` instead of Cinemachine
CLAUDE.md says to use Cinemachine if it's already in the project, but no Cinemachine package is installed here. Rather than pull in a new dependency for a single smooth-follow + shake behavior, wrote a small `CameraFollow.cs` (SmoothDamp follow + a `Shake(duration, magnitude)` coroutine) instead. Revisit and swap to Cinemachine if camera needs grow (room-bounds confiners, multiple virtual cameras per area, etc. — the genre-standard Metroidvania camera-zone pattern).

## Coyote time gates the *first* jump only, not every jump
`PlayerMovement.cs`'s jump counter (`_jumpsUsed`) only resets to 0 on ground contact, which meant leaving the ground without jumping left it at 0 indefinitely — the "first" jump was already unintentionally allowed forever mid-air-fall (accidental infinite coyote time), while a jump pressed slightly too early or late got no grace at all. Fixed with explicit `_coyoteTimer` (counts down after leaving ground) and `_jumpBufferTimer` (counts down after a jump press) — the first jump (`_jumpsUsed == 0`) now requires grounded/wall-touching/within-coyote-window, while subsequent jumps (already used at least one) stay unrestricted as the double-jump mechanic intends.
