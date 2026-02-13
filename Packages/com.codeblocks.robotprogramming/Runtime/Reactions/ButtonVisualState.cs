using UnityEngine;
using CodeBlocks.Core;

namespace CodeBlocks.Reactions
{
    public class ButtonVisualState : MonoBehaviour
    {
        private static readonly int IsPressedHash = Animator.StringToHash("IsPressed");
        private Animator cachedAnimator;
        private bool hasIsPressedParameter;
        private Renderer[] cachedRenderers;
        private Color[] originalColors;

        private void Awake()
        {
            cachedAnimator = GetComponent<Animator>();
            hasIsPressedParameter = AnimatorParameterUtility.HasParameter(cachedAnimator, IsPressedHash, AnimatorControllerParameterType.Bool);
            cachedRenderers = GetComponentsInChildren<Renderer>(true);
            if (cachedRenderers != null)
            {
                originalColors = new Color[cachedRenderers.Length];
                for (int i = 0; i < cachedRenderers.Length; i++)
                {
                    originalColors[i] = cachedRenderers[i] != null ? cachedRenderers[i].material.color : Color.white;
                }
            }

            // Evaluate animator state right away so visuals are stable on the first rendered frame.
            if (cachedAnimator != null)
            {
                cachedAnimator.Update(0f);
            }
        }

        public void SetPressed(bool isPressed)
        {
            if (cachedAnimator != null && hasIsPressedParameter)
            {
                AnimatorParameterUtility.TrySetBool(cachedAnimator, IsPressedHash, isPressed);
                cachedAnimator.Update(0f);
                return;
            }

            if (cachedRenderers == null || originalColors == null)
                return;

            for (int i = 0; i < cachedRenderers.Length; i++)
            {
                if (cachedRenderers[i] == null)
                    continue;

                Color baseColor = originalColors[i];
                cachedRenderers[i].material.color = isPressed
                    ? baseColor * 0.7f
                    : baseColor;
            }
        }

    }
}
