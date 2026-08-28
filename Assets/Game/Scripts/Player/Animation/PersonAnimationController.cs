using UnityEngine;

namespace NuevaAndinia.Animation
{
    using NuevaAndinia.Audio;
    using NuevaAndinia.Core;

    [RequireComponent(typeof(Animator))]
    public class PersonAnimationController : MonoBehaviour, IPlayerAnimator
    {
        private Animator _anim;
        private PlayerAudioController _audioController;

        private int _idSpeed;
        private int _idGrounded;
        private int _idJump;
        private int _idFreeFall;
        private int _idMotionSpeed;

        private void Awake()
        {
            _anim = GetComponent<Animator>();
            _audioController = GetComponent<PlayerAudioController>();
            AssignIDs();
        }

        private void AssignIDs()
        {
            _idSpeed = Animator.StringToHash("Speed");
            _idGrounded = Animator.StringToHash("Grounded");
            _idJump = Animator.StringToHash("Jump");
            _idFreeFall = Animator.StringToHash("FreeFall");
            _idMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        public void UpdateMovement(float speedBlend, float motionSpeed)
        {
            _anim.SetFloat(_idSpeed, speedBlend);
            _anim.SetFloat(_idMotionSpeed, motionSpeed);
        }

        public void SetJump(bool value) => _anim.SetBool(_idJump, value);
        public void SetFreeFall(bool value) => _anim.SetBool(_idFreeFall, value);
        public void SetGrounded(bool value) => _anim.SetBool(_idGrounded, value);

        private void OnFootstep(AnimationEvent evt)
        {
            if (_audioController != null)
                _audioController.OnFootstep(evt);
            
        }

        private void OnLand(AnimationEvent evt)
        {
            if (_audioController != null)
            _audioController.OnLand(evt);
        }
    }
}

