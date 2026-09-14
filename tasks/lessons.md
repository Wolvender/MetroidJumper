# MetroidJumper — Lessons

## Don't guess at a compile-error fix without seeing the actual error text
When the user reported "no Animator, nothing happened" with no console output attached, I guessed the cause (assumed `AnimationClipSettings` needed to be qualified as `AnimationUtility.AnimationClipSettings`) instead of asking for the actual Console error first. The guess was wrong — `AnimationClipSettings` is a top-level type in `UnityEditor`, not nested under `AnimationUtility` — and the "fix" introduced a real CS0426 compile error where the original code had been fine.
**Rule:** when a Unity change "does nothing" with no visible cause, ask for (or check) the actual Console error text before changing code. A guessed fix can turn a working-but-unconfirmed situation into an actually broken one.

## A single compile error anywhere in Assets/Scripts/Editor breaks every Tools/Senna menu item
There's no per-tool `.asmdef`, so everything under `Assets/Scripts/Editor` compiles into one assembly — one bad file makes every `Tools/Senna/...` menu item vanish silently, including ones that previously worked. If a previously-working menu item disappears too, suspect a compile error over a logic bug, and check the Console first.

## AnimatorController asset creation needs its target folder to already exist
`AnimatorController.CreateAnimatorControllerAtPath("Assets/Animation/Player.controller")` throws "Parent directory must exist" if `Assets/Animation` isn't already an existing folder — unlike some other `AssetDatabase.CreateAsset` call sites, it won't create intermediate folders for you. Ensure the folder exists (`AssetDatabase.CreateFolder`) before creating the controller, not just before writing sub-assets like clips into it.
