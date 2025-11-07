using Fusion;
using UnityEngine;

namespace SpellFlinger.PlayScene
{
    public static class NetworkCharacterControllerExtensions
    {
        // Konstante iz stare verzije (podesi prema potrebi)
        private const float moveSpeed = 8f;
        private const float slowAmount = 0.5f;
        private static readonly float _squareOfTwo = Mathf.Sqrt(2f);

        public static void Move(this NetworkCharacterController controller, Vector2 direction, bool isSlowed, float rotation, bool isGrounded)
        {
            // Provjeri da li Runner postoji
            if (controller.Runner == null)
            {
                Debug.LogWarning("NetworkCharacterController.Runner is null!");
                return;
            }

            var deltaTime = controller.Runner.DeltaTime;
            var moveVelocity = controller.Velocity;

            // Reset Y velocity ako smo na tlu
            if (isGrounded && moveVelocity.y < 0)
            {
                moveVelocity.y = 0f;
            }

            // Izračunaj smjer kretanja relativno prema transformu
            Vector3 moveDirection = controller.transform.right * direction.x + controller.transform.forward * direction.y;
            
            // Primijeni moveSpeed
            moveDirection *= moveSpeed;
            
            // Primijeni slow ako je potrebno
            if (isSlowed) moveDirection *= slowAmount;
            
            // Normaliziraj dijagonalno kretanje (ako se krećeš dijagonalno, brzina je veća)
            if (direction.x != 0 && direction.y != 0) 
                moveDirection /= _squareOfTwo;

            // Primijeni gravitaciju
            moveVelocity.y += controller.gravity * deltaTime;

            // Kombiniraj horizontalni i vertikalni smjer
            moveDirection.y += moveVelocity.y;

            // Primijeni kretanje
            var cc = controller.GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.Move(moveDirection * deltaTime);
            }

            // Postavi rotaciju direktno (yaw iz miša) - samo ako postoji rotacija
            if (Mathf.Abs(rotation) > 0.0001f)
            {
                controller.transform.eulerAngles = new Vector3(0, controller.transform.eulerAngles.y + rotation, 0);
            }

            // Ažuriraj velocity i grounded status
            // Velocity se računa iz pozicije (kao u originalnoj Move metodi)
            var previousPos = controller.transform.position;
            moveVelocity = moveDirection;
            controller.Velocity = moveVelocity;
            controller.Grounded = isGrounded;
        }
    }
}

