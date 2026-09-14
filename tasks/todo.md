# MetroidJumper — Todo

## Day 1: Core movement
- [x] `PlayerMovement.cs` — A/D + Space, ground-checked jump (Rigidbody2D)
- [x] Editor tool: `Tools/Senna/Setup Player Movement (2D)` — converts player/floor to 2D physics, attaches + wires the script
- [x] Double jump capped at 2, resets on ground or wall touch (`maxJumps`)
- [x] Wall detection (tag "Wall"), wall-stick (1s) then slow wall-slide, frictionless player material
- [x] Wall-jump launches away from the wall + brief input lock, so hugging one wall no longer cheeses the climb
- [ ] Tune `moveSpeed` / `jumpForce` / `wallJumpHorizontalForce` to feel right
- [ ] Coyote time + jump buffering (still open)

## Animation
- [x] `PlayerAnimator.cs` — reads `PlayerMovement` state (grounded/wall/jumps/velocity) and drives the Animator
- [x] Editor tool: `Tools/Senna/Setup Player Animator` — builds AnimationClips from the sprite sheets in `Assets/sprites/Animations`, assembles the `Player.controller` state machine, attaches it
- [ ] Run the menu item, playtest all transitions (idle/run/skid/jump/double-jump/wall-jump/falling/wall-slide/landing), hand-tune any transition in the Animator window that feels off
- [ ] Watch for frame-to-frame jitter — the sprite sheets use a bottom-left pivot per frame with varying trim sizes, which can cause slight vertical/horizontal shifting between frames
- [ ] No Idle animation exists yet (using a static Run frame 0 as a placeholder) — swap in a real Idle sheet if/when you get one

## Day 2: Level + juice
- [ ] Greybox vertical tower
- [ ] Fall-death zone + respawn at bottom
- [ ] Goal zone + win UI
- [ ] Camera follow + shake, squash/stretch, particle juice
