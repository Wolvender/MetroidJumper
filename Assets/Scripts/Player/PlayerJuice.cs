using System.Collections;
using UnityEngine;

namespace MetroidJumper.Player
{
    [RequireComponent(typeof(PlayerMovement))]
    public class PlayerJuice : MonoBehaviour
    {
        private PlayerMovement _movement;
        private Vector3 _baseScale;
        private Coroutine _scaleCoroutine;

        [SerializeField] private ParticleSystem dustParticles;
        [SerializeField] private Vector2 jumpStretch = new Vector2(0.85f, 1.2f);
        [SerializeField] private Vector2 wallJumpStretch = new Vector2(0.85f, 1.2f);
        [SerializeField] private Vector2 landSquash = new Vector2(1.25f, 0.75f);
        [SerializeField] private float squashStretchDuration = 0.12f;

        private void Awake()
        {
            _movement = GetComponent<PlayerMovement>();
            _baseScale = transform.localScale;
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

        private void HandleJumped()
        {
            PlaySquashStretch(jumpStretch);
            dustParticles?.Play();
        }

        private void HandleWallJumped()
        {
            PlaySquashStretch(wallJumpStretch);
            dustParticles?.Play();
        }

        private void HandleLanded()
        {
            PlaySquashStretch(landSquash);
            dustParticles?.Play();
        }

        private void PlaySquashStretch(Vector2 factor)
        {
            if (_scaleCoroutine != null) StopCoroutine(_scaleCoroutine);
            _scaleCoroutine = StartCoroutine(AnimateScale(new Vector3(_baseScale.x * factor.x, _baseScale.y * factor.y, _baseScale.z)));
        }

        private IEnumerator AnimateScale(Vector3 fromScale)
        {
            transform.localScale = fromScale;
            float elapsed = 0f;
            while (elapsed < squashStretchDuration)
            {
                elapsed += Time.deltaTime;
                transform.localScale = Vector3.Lerp(fromScale, _baseScale, elapsed / squashStretchDuration);
                yield return null;
            }

            transform.localScale = _baseScale;
            _scaleCoroutine = null;
        }
    }
}
