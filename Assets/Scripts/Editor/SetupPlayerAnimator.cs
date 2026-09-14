using System.Linq;
using MetroidJumper.Player;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MetroidJumper.EditorTools
{
    public static class SetupPlayerAnimator
    {
        private const string ControllerPath = "Assets/Animation/Player.controller";
        private const string ClipsFolder = "Assets/Animation/Clips";

        private const string RunningPath = "Assets/sprites/Animations/Running.png";
        private const string JumpingPath = "Assets/sprites/Animations/Jumping.png";
        private const string DoubleJumpPath = "Assets/sprites/Animations/Double_Jump.png";
        private const string FallingPath = "Assets/sprites/Animations/Falling.png";
        private const string WallJumpPath = "Assets/sprites/Animations/Wall Jump.png";
        private const string WallClimbingPath = "Assets/sprites/Animations/Wall_Climbing.png";
        private const string LandingPath = "Assets/sprites/Animations/Landing.png";
        private const string StopRunningPath = "Assets/sprites/Animations/Stop_Running.png";

        private const float TargetCharacterHeight = 2f;

        [MenuItem("Tools/Senna/Setup Player Animator")]
        private static void Setup()
        {
            GameObject player = GameObject.Find("player");
            if (player == null)
            {
                Debug.LogError("SetupPlayerAnimator: could not find 'player' in the open scene.");
                return;
            }

            AnimatorController controller = BuildController();

            Animator animator = player.GetComponent<Animator>();
            if (animator == null) animator = player.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;

            if (player.GetComponent<PlayerAnimator>() == null) player.AddComponent<PlayerAnimator>();

            ResizePlayerForSprite(player);

            EditorUtility.SetDirty(player);
            EditorSceneManager.MarkSceneDirty(player.scene);

            Debug.Log("SetupPlayerAnimator: Animator Controller built from sprite sheets and attached to player.");
        }

        private static void ResizePlayerForSprite(GameObject player)
        {
            Sprite reference = LoadSprite(RunningPath, "Running_0");
            if (reference == null)
            {
                Debug.LogWarning("SetupPlayerAnimator: could not find a reference sprite to size the player against.");
                return;
            }

            float nativeHeight = reference.bounds.size.y;
            if (nativeHeight <= 0f) return;

            float scale = TargetCharacterHeight / nativeHeight;
            player.transform.localScale = new Vector3(scale, scale, 1f);

            SpriteRenderer spriteRenderer = player.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null) spriteRenderer.sprite = reference;

            CapsuleCollider2D capsule = player.GetComponent<CapsuleCollider2D>();
            if (capsule == null) return;

            capsule.size = reference.bounds.size;
            capsule.offset = reference.bounds.center;

            Transform groundCheck = player.transform.Find("GroundCheck");
            if (groundCheck != null)
            {
                float bottomY = capsule.offset.y - capsule.size.y * 0.5f;
                groundCheck.localPosition = new Vector3(capsule.offset.x, bottomY, 0f);
            }
        }

        private static Sprite LoadSprite(string texturePath, string spriteName)
        {
            return AssetDatabase.LoadAllAssetsAtPath(texturePath)
                .OfType<Sprite>()
                .FirstOrDefault(s => s.name == spriteName);
        }

        private static AnimatorController BuildController()
        {
            EnsureFolder();

            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath) != null)
            {
                AssetDatabase.DeleteAsset(ControllerPath);
            }

            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("VerticalVelocity", AnimatorControllerParameterType.Float);
            controller.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);
            controller.AddParameter("IsTouchingWall", AnimatorControllerParameterType.Bool);
            controller.AddParameter("JumpsUsed", AnimatorControllerParameterType.Int);
            controller.AddParameter("Jump", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("WallJump", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Land", AnimatorControllerParameterType.Trigger);

            AnimatorStateMachine root = controller.layers[0].stateMachine;

            AnimatorState idle = root.AddState("Idle");
            idle.motion = CreateClipFromSpriteSheet(RunningPath, "Idle", 1f, true, 1);

            AnimatorState run = root.AddState("Run");
            run.motion = CreateClipFromSpriteSheet(RunningPath, "Run", 12f, true);

            AnimatorState stopRunning = root.AddState("StopRunning");
            stopRunning.motion = CreateClipFromSpriteSheet(StopRunningPath, "StopRunning", 12f, false);

            AnimatorState jump = root.AddState("Jump");
            jump.motion = CreateClipFromSpriteSheet(JumpingPath, "Jump", 12f, false);

            AnimatorState doubleJump = root.AddState("DoubleJump");
            doubleJump.motion = CreateClipFromSpriteSheet(DoubleJumpPath, "DoubleJump", 20f, false);

            AnimatorState wallJump = root.AddState("WallJump");
            wallJump.motion = CreateClipFromSpriteSheet(WallJumpPath, "WallJump", 12f, false);

            AnimatorState falling = root.AddState("Falling");
            falling.motion = CreateClipFromSpriteSheet(FallingPath, "Falling", 10f, true, 9);

            AnimatorState wallSlide = root.AddState("WallSlide");
            wallSlide.motion = CreateClipFromSpriteSheet(WallClimbingPath, "WallSlide", 12f, true);

            AnimatorState landing = root.AddState("Landing");
            landing.motion = CreateClipFromSpriteSheet(LandingPath, "Landing", 12f, false);

            root.defaultState = idle;

            AddAnyStateTransition(root, jump, ("Jump", AnimatorConditionMode.If, 0f), ("JumpsUsed", AnimatorConditionMode.Equals, 1f));
            AddAnyStateTransition(root, doubleJump, ("Jump", AnimatorConditionMode.If, 0f), ("JumpsUsed", AnimatorConditionMode.Equals, 2f));
            AddAnyStateTransition(root, wallJump, ("WallJump", AnimatorConditionMode.If, 0f));
            AddAnyStateTransition(root, landing, ("Land", AnimatorConditionMode.If, 0f));

            AddTransition(jump, falling, ("VerticalVelocity", AnimatorConditionMode.Less, 0f));
            AddExitTimeTransition(doubleJump, falling, 1f);
            AddTransition(wallJump, falling, ("VerticalVelocity", AnimatorConditionMode.Less, 0f));

            AddTransition(falling, wallSlide, ("IsTouchingWall", AnimatorConditionMode.If, 0f));
            AddTransition(wallSlide, falling, ("IsTouchingWall", AnimatorConditionMode.IfNot, 0f));

            AddTransition(idle, falling, ("IsGrounded", AnimatorConditionMode.IfNot, 0f));
            AddTransition(run, falling, ("IsGrounded", AnimatorConditionMode.IfNot, 0f));

            AddTransition(idle, run, ("Speed", AnimatorConditionMode.Greater, 0.1f));
            AddTransition(run, stopRunning, ("Speed", AnimatorConditionMode.Less, 0.1f));

            AddExitTimeTransition(landing, idle, 0.9f, ("Speed", AnimatorConditionMode.Less, 0.1f));
            AddExitTimeTransition(landing, run, 0.9f, ("Speed", AnimatorConditionMode.Greater, 0.1f));
            AddExitTimeTransition(stopRunning, idle, 0.9f);

            return controller;
        }

        private static AnimationClip CreateClipFromSpriteSheet(string texturePath, string clipName, float frameRate, bool loop, int maxFrames = -1)
        {
            Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(texturePath)
                .OfType<Sprite>()
                .OrderBy(FrameIndex)
                .ToArray();

            if (sprites.Length == 0)
            {
                Debug.LogError($"SetupPlayerAnimator: no sliced sprites found at '{texturePath}'.");
                return null;
            }

            if (maxFrames > 0 && maxFrames < sprites.Length)
            {
                sprites = sprites.Take(maxFrames).ToArray();
            }

            var clip = new AnimationClip { frameRate = frameRate };

            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            var binding = new EditorCurveBinding
            {
                path = "",
                type = typeof(SpriteRenderer),
                propertyName = "m_Sprite"
            };

            var keyframes = new ObjectReferenceKeyframe[sprites.Length];
            for (int i = 0; i < sprites.Length; i++)
            {
                keyframes[i] = new ObjectReferenceKeyframe { time = i / frameRate, value = sprites[i] };
            }

            AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

            EnsureFolder();

            string assetPath = $"{ClipsFolder}/{clipName}.anim";
            if (AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath) != null)
            {
                AssetDatabase.DeleteAsset(assetPath);
            }
            AssetDatabase.CreateAsset(clip, assetPath);

            return clip;
        }

        private static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Animation")) AssetDatabase.CreateFolder("Assets", "Animation");
            if (!AssetDatabase.IsValidFolder(ClipsFolder)) AssetDatabase.CreateFolder("Assets/Animation", "Clips");
        }

        private static int FrameIndex(Sprite sprite)
        {
            string name = sprite.name;
            int underscoreIndex = name.LastIndexOf('_');
            if (underscoreIndex < 0) return 0;
            return int.TryParse(name.Substring(underscoreIndex + 1), out int index) ? index : 0;
        }

        private static void AddAnyStateTransition(AnimatorStateMachine root, AnimatorState to,
            params (string parameter, AnimatorConditionMode mode, float threshold)[] conditions)
        {
            AnimatorStateTransition transition = root.AddAnyStateTransition(to);
            transition.hasExitTime = false;
            transition.duration = 0.05f;
            transition.canTransitionToSelf = false;
            foreach (var (parameter, mode, threshold) in conditions)
            {
                transition.AddCondition(mode, threshold, parameter);
            }
        }

        private static void AddTransition(AnimatorState from, AnimatorState to,
            params (string parameter, AnimatorConditionMode mode, float threshold)[] conditions)
        {
            AnimatorStateTransition transition = from.AddTransition(to);
            transition.hasExitTime = false;
            transition.duration = 0.05f;
            foreach (var (parameter, mode, threshold) in conditions)
            {
                transition.AddCondition(mode, threshold, parameter);
            }
        }

        private static void AddExitTimeTransition(AnimatorState from, AnimatorState to, float exitTime,
            params (string parameter, AnimatorConditionMode mode, float threshold)[] conditions)
        {
            AnimatorStateTransition transition = from.AddTransition(to);
            transition.hasExitTime = true;
            transition.exitTime = exitTime;
            transition.duration = 0.05f;
            foreach (var (parameter, mode, threshold) in conditions)
            {
                transition.AddCondition(mode, threshold, parameter);
            }
        }
    }
}
