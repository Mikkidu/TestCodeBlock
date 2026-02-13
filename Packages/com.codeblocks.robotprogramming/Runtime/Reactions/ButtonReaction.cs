using CodeBlocks.Core;
using System;
using System.Collections.Generic;

namespace CodeBlocks.Reactions
{
    public class ButtonReaction : ObjectReactionComponent
    {
        private const string TargetIdKey = "targetId";
        private const string TargetIdsKey = "targetIds";

        public override bool CanHandle(string objectTypeId)
        {
            return string.Equals(objectTypeId, "Button", StringComparison.OrdinalIgnoreCase);
        }

        public override ObjectReactionResult Evaluate(ObjectReactionContext context)
        {
            if (context.Object == null)
                return ObjectReactionResult.NoReaction();

            string[] targetIds = ExtractTargetIds(context.Object);
            if (targetIds == null || targetIds.Length == 0)
            {
                return ObjectReactionResult.NoReaction();
            }

            return ObjectReactionResult.NoReaction(targetIds);
        }

        private static string[] ExtractTargetIds(GridObject obj)
        {
            if (obj.parameters == null)
                return null;

            if (obj.parameters.TryGetValue(TargetIdsKey, out var listValue))
            {
                return SplitTargets(listValue);
            }

            if (obj.parameters.TryGetValue(TargetIdKey, out var singleValue))
            {
                return SplitTargets(singleValue);
            }

            return null;
        }

        private static string[] SplitTargets(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var parts = value.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            var trimmed = new List<string>(parts.Length);
            foreach (var part in parts)
            {
                var s = part.Trim();
                if (s.Length > 0)
                    trimmed.Add(s);
            }

            return trimmed.Count > 0 ? trimmed.ToArray() : null;
        }
    }
}
