using UnityEngine;

namespace SpellFlinger.PlayScene
{
    public class AnimatorSetLayerOnEnter : StateMachineBehaviour
    {
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.SetLayerWeight(layerIndex, 0);
        }
    }
}