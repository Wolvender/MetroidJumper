# MetroidJumper — Todo

## Day 1: Core movement
- [x] `PlayerMovement.cs` — A/D + Space, ground-checked jump (Rigidbody2D)
- [x] Editor tool: `Tools/Senna/Setup Player Movement (2D)` — converts player/floor to 2D physics, attaches + wires the script
- [x] Double jump capped at 2, resets on ground or wall touch (`maxJumps`)
- [x] Wall detection (tag "Wall"), wall-stick (1s) then slow wall-slide, frictionless player material
- [x] Wall-jump launches away from the wall + brief input lock, so hugging one wall no longer cheeses the climb
- [ ] Tune `moveSpeed` / `jumpForce` / `wallJumpHorizontalForce` to feel right
- [x] Coyote time + jump buffering — `coyoteTime`/`jumpBufferTime` fields on `PlayerMovement.cs`, gates the first jump instead of the old "infinite grace" bug
- [x] Wall-jump punch increased — `wallJumpHorizontalForce` default 6 → 9

## Animation
- [x] `PlayerAnimator.cs` — reads `PlayerMovement` state (grounded/wall/jumps/velocity) and drives the Animator
- [x] Editor tool: `Tools/Senna/Setup Player Animator` — builds AnimationClips from the sprite sheets in `Assets/sprites/Animations`, assembles the `Player.controller` state machine, attaches it
- [x] Idle clip now sourced from `Walking_0` instead of `Running_0` — re-run `Setup Player Animator` to regenerate `Idle.anim`
- [ ] Run the menu item, playtest all transitions (idle/run/skid/jump/double-jump/wall-jump/falling/wall-slide/landing), hand-tune any transition in the Animator window that feels off
- [ ] Watch for frame-to-frame jitter — the sprite sheets use a bottom-left pivot per frame with varying trim sizes, which can cause slight vertical/horizontal shifting between frames

## Juice (movement feel pass)
- [x] `PlayerJuice.cs` — squash/stretch on jump/wall-jump/land + dust particle burst; run `Tools/Senna/Setup Player Juice` to attach
- [x] `CameraFollow.cs` — smooth-damped follow + shake on land/wall-jump; run `Tools/Senna/Setup Camera Follow` to attach (needs `Setup Player Movement (2D)` run first)
- [ ] Playtest: confirm coyote time cuts off after ~0.12s (jump near a ledge edge, wait, then try), jump buffer catches a jump pressed just before landing, wall-jump distance feels right, camera shake isn't too strong

## Day 2: Level + juice
- [ ] Greybox vertical tower
- [ ] Fall-death zone + respawn at bottom
- [ ] Goal zone + win UI
