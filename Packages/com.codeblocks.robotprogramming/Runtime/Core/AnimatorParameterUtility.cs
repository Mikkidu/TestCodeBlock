using UnityEngine;

namespace CodeBlocks.Core
{
    public static class AnimatorParameterUtility
    {
        public static bool HasParameter(Animator animator, int hash)
        {
            if (animator == null)
            {
                return false;
            }

            var parameters = animator.parameters;
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].nameHash == hash)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool HasParameter(Animator animator, int hash, AnimatorControllerParameterType type)
        {
            if (animator == null)
            {
                return false;
            }

            var parameters = animator.parameters;
            for (int i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                if (parameter.nameHash == hash && parameter.type == type)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool TrySetBool(Animator animator, int hash, bool value)
        {
            if (!HasParameter(animator, hash, AnimatorControllerParameterType.Bool))
            {
                return false;
            }

            animator.SetBool(hash, value);
            return true;
        }

        public static bool TrySetTrigger(Animator animator, int hash)
        {
            if (!HasParameter(animator, hash, AnimatorControllerParameterType.Trigger))
            {
                return false;
            }

            animator.SetTrigger(hash);
            return true;
        }
    }
}
