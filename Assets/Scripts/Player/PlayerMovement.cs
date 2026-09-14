using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MetroidJumper.Player
{
    public class PlayerMovement : MonoBehaviour
    {
        private Rigidbody2D _rigidbody;
        private readonly Collider2D[] _groundHits = new Collider2D[1];
        private float _horizontalInput;
        private bool _jumpRequested;
        private int _jumpsUsed;
        private int _wallContacts;
        private float _wallStickTimer;
        private float _wallJumpLockTimer;
        private Vector2 _wallNormal;

        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float jumpForce = 4f;
        [SerializeField] private int maxJumps = 2;
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundCheckRadius = 0.15f;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private string wallTag = "Wall";
        [SerializeField] private float wallStickTime = 1f;
        [SerializeField] private float wallSlideSpeed = 3f;
        [SerializeField] private float wallJumpHorizontalForce = 6f;
        [SerializeField] private float wallJumpLockTime = 0.2f;

        public bool IsGrounded { get; private set; }
        public bool IsTouchingWall { get; private set; }
        public int JumpsUsed => _jumpsUsed;
        public float HorizontalInput => _horizontalInput;

        public event Action Jumped;
        public event Action WallJumped;
        public event Action Landed;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return;

            _horizontalInput = 0f;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) _horizontalInput -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) _horizontalInput += 1f;

            if (keyboard.spaceKey.wasPressedThisFrame) _jumpRequested = true;
        }

        private void FixedUpdate()
        {
            bool isGrounded = groundCheck != null &&
                Physics2D.OverlapCircleNonAlloc(groundCheck.position, groundCheckRadius, _groundHits, groundLayer) > 0;
            bool isTouchingWall = _wallContacts > 0 && !isGrounded;

            if (isGrounded && !IsGrounded) Landed?.Invoke();
            IsGrounded = isGrounded;
            IsTouchingWall = isTouchingWall;

            if (isGrounded) _jumpsUsed = 0;
            _wallStickTimer = isTouchingWall ? _wallStickTimer + Time.fixedDeltaTime : 0f;

            Vector2 velocity = _rigidbody.linearVelocity;

            if (_wallJumpLockTimer > 0f)
            {
                _wallJumpLockTimer -= Time.fixedDeltaTime;
            }
            else
            {
                velocity.x = _horizontalInput * moveSpeed;
            }

            if (_jumpRequested && _jumpsUsed < maxJumps)
            {
                if (isTouchingWall)
                {
                    velocity.x = _wallNormal.x * wallJumpHorizontalForce;
                    _wallJumpLockTimer = wallJumpLockTime;
                    _jumpsUsed++;
                    _wallStickTimer = 0f;
                    velocity.y = jumpForce;
                    WallJumped?.Invoke();
                }
                else
                {
                    _jumpsUsed++;
                    _wallStickTimer = 0f;
                    velocity.y = jumpForce;
                    Jumped?.Invoke();
                }
            }
            _jumpRequested = false;

            if (isTouchingWall)
            {
                float minVerticalSpeed = _wallStickTimer < wallStickTime ? 0f : -wallSlideSpeed;
                velocity.y = Mathf.Max(velocity.y, minVerticalSpeed);
            }

            _rigidbody.linearVelocity = velocity;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!collision.collider.CompareTag(wallTag)) return;
            _wallContacts++;
            _wallNormal = collision.GetContact(0).normal;
            _jumpsUsed = Mathf.Max(0, _jumpsUsed - 1);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (collision.collider.CompareTag(wallTag)) _wallNormal = collision.GetContact(0).normal;
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.collider.CompareTag(wallTag)) _wallContacts--;
        }
    }
}
