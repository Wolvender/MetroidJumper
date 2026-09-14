using UnityEngine;

namespace MetroidJumper.Player
{
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimator : MonoBehaviour
    {
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int VerticalVelocityHash = Animator.StringToHash("VerticalVelocity");
        private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
        private static readonly int IsTouchingWallHash = Animator.StringToHash("IsTouchingWall");
        private static readonly int JumpsUsedHash = Animator.StringToHash("JumpsUsed");
        private static readonly int JumpHash = Animator.StringToHash("Jump");
        private static readonly int WallJumpHash = Animator.StringToHash("WallJump");
        private static readonly int LandHash = Animator.StringToHash("Land");

        private Animator _animator;
        private Rigidbody2D _rigidbody;
        private PlayerMovement _movement;
        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _rigidbody = GetComponent<Rigidbody2D>();
            _movement = GetComponent<PlayerMovement>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            _movement.Jumped += HandleJumped;
            _movement.WallJumped += HandleWallJumped;
            _movement.Landed += HandleLanded;
        }

        private void OnDisable()
        {
            _movement.Jumped -= HandleJumped;
            _movement.WallJumped -= HandleWallJumped;
            _movement.Landed -= HandleLanded;
        }

        private void Update()
        {
            Vector2 velocity = _rigidbody.linearVelocity;

            _animator.SetFloat(SpeedHash, Mathf.Abs(velocity.x));
            _animator.SetFloat(VerticalVelocityHash, velocity.y);
            _animator.SetBool(IsGroundedHash, _movement.IsGrounded);
            _animator.SetBool(IsTouchingWallHash, _movement.IsTouchingWall);
            _animator.SetInteger(JumpsUsedHash, _movement.JumpsUsed);

            if (Mathf.Abs(_movement.HorizontalInput) > 0.01f)
            {
                _spriteRenderer.flipX = _movement.HorizontalInput < 0f;
            }
        }

        private void HandleJumped() => _animator.SetTrigger(JumpHash);
        private void HandleWallJumped() => _animator.SetTrigger(WallJumpHash);
        private void HandleLanded() => _animator.SetTrigger(LandHash);
    }
}
