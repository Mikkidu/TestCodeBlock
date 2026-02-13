using UnityEngine;
using CodeBlocks.Core;

namespace CodeBlocks.Reactions
{
    public class DoorVisualState : MonoBehaviour
    {
        private static readonly int IsOpenHash = Animator.StringToHash("IsOpen");
        private Animator cachedAnimator;
        private bool hasIsOpenParameter;
        private Renderer[] cachedRenderers;
        private Collider[] cachedColliders;

        private void Awake()
        {
            cachedAnimator = GetComponent<Animator>();
            hasIsOpenParameter = AnimatorParameterUtility.HasParameter(cachedAnimator, IsOpenHash, AnimatorControllerParameterType.Bool);
            cachedRenderers = GetComponentsInChildren<Renderer>(true);
            cachedColliders = GetComponentsInChildren<Collider>(true);

            // Force animator to evaluate immediately to avoid a visible first-frame pose jump.
            if (cachedAnimator != null)
            {
                cachedAnimator.Update(0f);
            }
        }

        public void SetOpen(bool isOpen)
        {
            if (cachedAnimator != null && hasIsOpenParameter)
            {
                AnimatorParameterUtility.TrySetBool(cachedAnimator, IsOpenHash, isOpen);
                cachedAnimator.Update(0f);
            }
            else
            {
                SetVisible(isOpen);
            }

            SetCollision(!isOpen);
        }

        private void SetVisible(bool isOpen)
        {
            if (cachedRenderers == null) return;
            for (int i = 0; i < cachedRenderers.Length; i++)
            {
                if (cachedRenderers[i] != null)
                {
                    cachedRenderers[i].enabled = !isOpen;
                }
            }
        }

        private void SetCollision(bool blocked)
        {
            if (cachedColliders == null) return;
            for (int i = 0; i < cachedColliders.Length; i++)
            {
                if (cachedColliders[i] != null)
                {
                    cachedColliders[i].enabled = blocked;
                }
            }
        }

    }
}
