
using System;
using UnityEngine;

namespace PU.SharedData.Config
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class StringEnumAttribute : PropertyAttribute
    {
        public EnumPath configPath;

        public StringEnumAttribute(EnumPath configPath)
        {
            this.configPath = configPath;
        }
    }

    public enum EnumPath
    {
        TaskType = 1,
        QuestRepeatType = 2,
        QuestStartConditionType = 3,
        AchievementGroup = 4,
        UnitRole = 5,
        ChimeraSkillType = 6,
        MiniGames = 7,
        PortalRewardFunctions = 8,
        PortalBoosterType = 9,
        PortalShopSlotType = 10,
        TutorialSaveGroup = 11
    }

    [CreateAssetMenu(menuName = "CodeBlocks/Config/String Enum Config")]
    public class StringEnumConfig : ScriptableObject
    {
        public string[] values = new string[] { "Value 1", "Value 2", "Value 3" };
    }
}
