using UnityEngine;
using SpellFlinger.Enum;

namespace SpellFlinger.PlayScene
{
    public  class PlayerAnimationController
    {
        private float _currentAngle = 0;
        private float _deltaAngle = 0.3f;
        private float _lerpCutoff = 0.01f;
        private int _attackOrder = 0;
        private int _animationStateParameterId = 0;

        public void Init(ref PlayerAnimationState animationState, Animator animator)
        {
            _animationStateParameterId = Animator.StringToHash("AnimationState");
            animationState = PlayerAnimationState.Idle;
            animator.SetInteger(_animationStateParameterId, (int)animationState);
        }

        public void SetDeadState(ref PlayerAnimationState animationState, Animator animator)
        {
            animationState = PlayerAnimationState.Dead;
            animator.SetBool("DeadState", true);
            animator.SetInteger(_animationStateParameterId, (int)animationState);
        }

        public void SetAliveState(ref PlayerAnimationState animationState, Animator animator)
        {
            animationState = PlayerAnimationState.Idle;
            animator.SetBool("DeadState", false);
            animator.SetInteger(_animationStateParameterId, (int)animationState);
        }

        public void PlayShootAnimation(Animator animator)
        {
            animator.SetLayerWeight(1, 1);

            animator.SetTrigger($"Attack{_attackOrder + 1}");
            _attackOrder++;
            _attackOrder = _attackOrder % 3;
        }

        public void AnimationUpdate(bool isGrounded, float leftRightDirection, float forwardDirection, ref PlayerAnimationState animationState, Animator animator, Transform modelTransform, Transform referenceTransform)
        {
            modelTransform.rotation = referenceTransform.rotation;
            float rotation;
            PlayerAnimationState newPlayerAnimationState;

            leftRightDirection = leftRightDirection > 0 ? 1 : leftRightDirection == 0 ? 0 : -1;
            forwardDirection = forwardDirection > 0 ? 1 : forwardDirection == 0 ? 0 : -1;

            switch ((leftRightDirection, forwardDirection))
            {
                case (1, 1):
                    rotation = 40f;
                    newPlayerAnimationState = PlayerAnimationState.RunForward;
                    break;
                case (1, 0):
                    rotation = 90f;
                    newPlayerAnimationState = PlayerAnimationState.RunForward;
                    break;
                case (1, -1):
                    rotation = -40f;
                    newPlayerAnimationState = PlayerAnimationState.RunBack;
                    break;
                case (0, 1):
                    rotation = 0f;
                    newPlayerAnimationState = PlayerAnimationState.RunForward;
                    break;
                case (0, 0):
                    rotation = 0f;
                    newPlayerAnimationState = PlayerAnimationState.Idle;
                    break;
                case (0, -1):
                    rotation = 0f;
                    newPlayerAnimationState = PlayerAnimationState.RunBack;
                    break;
                case (-1, 1):
                    rotation = -40f;
                    newPlayerAnimationState = PlayerAnimationState.RunForward;
                    break;
                case (-1, 0):
                    rotation = -90f;
                    newPlayerAnimationState = PlayerAnimationState.RunForward;
                    break;
                case (-1, -1):
                    rotation = 40f;
                    newPlayerAnimationState = PlayerAnimationState.RunBack;
                    break;
                default:
                    rotation = 0f;
                    newPlayerAnimationState = PlayerAnimationState.Idle;
                    break;
            }

            if (!isGrounded) newPlayerAnimationState = PlayerAnimationState.Jumping;

            if(Mathf.Abs(_currentAngle - rotation) < _lerpCutoff) _currentAngle = rotation;
            else _currentAngle = Mathf.Lerp(_currentAngle, rotation, _deltaAngle);

            ApplyAnimation(newPlayerAnimationState, ref animationState, _currentAngle, animator, modelTransform);
        }

        private void ApplyAnimation(PlayerAnimationState playerAnimation, ref PlayerAnimationState currentAnimation, float rotation, Animator animator, Transform modelTransform)
        {
            if(currentAnimation != playerAnimation)
            {
                currentAnimation = playerAnimation;
                animator.SetInteger(_animationStateParameterId, (int)currentAnimation);
            }

            modelTransform.Rotate(0, rotation, 0);
        }
    }
}
