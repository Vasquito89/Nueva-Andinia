using UnityEngine;

namespace NuevaAndinia.Core
{
    public interface IPlayerAnimator
    {
        void UpdateMovement(float speed, float motionSpeed);
        void SetGrounded(bool isGrounded);
        void SetJump(bool isJumping);
        void SetFreeFall(bool isFreeFalling);
    }
}
