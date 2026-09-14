using MetroidJumper.Player;
using UnityEngine;

namespace MetroidJumper.Cameras
{
    public class CameraFollow : MonoBehaviour
    {
        private Vector3 _followVelocity;
        private Vector3 _followPosition;
        private float _shakeTimer;
        private float _shakeDuration;
        private float _shakeMagnitude;

        [SerializeField] private Transform target;
        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private float followSmoothTime = 0.15f;
        [SerializeField] private float landShakeDuration = 0.15f;
        [SerializeField] private float landShakeMagnitude = 0.15f;
        [SerializeField] private float wallJumpShakeDuration = 0.12f;
        [SerializeField] private float wallJumpShakeMagnitude = 0.12f;

        private void Awake()
        {
            _followPosition = transform.position;
        }

        private void OnEnable()
        {
            if (playerMovement == null) return;
            playerMovement.Landed += HandleLanded;
            playerMovement.WallJumped += HandleWallJumped;
        }

        private void OnDisable()
        {
            if (playerMovement == null) return;
            playerMovement.Landed -= HandleLanded;
            playerMovement.WallJumped -= HandleWallJumped;
        }

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 targetPosition = new Vector3(target.position.x, target.position.y, transform.position.z);
            _followPosition = Vector3.SmoothDamp(_followPosition, targetPosition, ref _followVelocity, followSmoothTime);

            Vector3 shakeOffset = Vector3.zero;
            if (_shakeTimer > 0f)
            {
                _shakeTimer -= Time.deltaTime;
                float falloff = _shakeTimer / _shakeDuration;
                shakeOffset = (Vector3)Random.insideUnitCircle * (_shakeMagnitude * falloff);
            }

            transform.position = _followPosition + shakeOffset;
        }

        public void Shake(float duration, float magnitude)
        {
            _shakeDuration = duration;
            _shakeTimer = duration;
            _shakeMagnitude = magnitude;
        }

        private void HandleLanded() => Shake(landShakeDuration, landShakeMagnitude);
        private void HandleWallJumped() => Shake(wallJumpShakeDuration, wallJumpShakeMagnitude);
    }
}
