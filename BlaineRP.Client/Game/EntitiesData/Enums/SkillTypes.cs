namespace BlaineRP.Client.Game.EntitiesData.Enums
{
    public enum SkillTypes
    {
        [Language.Localized("SKILLS_STRENGTH_NAME_0", "NAME_0")]
        [Language.Localized("SKILLS_STRENGTH_NAME_1", "NAME_1")]
        /// <summary>Сила</summary>
        Strength = 0,

        [Language.Localized("SKILLS_SHOOTING_NAME_0", "NAME_0")]
        [Language.Localized("SKILLS_SHOOTING_NAME_1", "NAME_1")]
        /// <summary>Стрельба</summary>
        Shooting,

        [Language.Localized("SKILLS_COOKING_NAME_0", "NAME_0")]
        [Language.Localized("SKILLS_COOKING_NAME_1", "NAME_1")]
        /// <summary>Кулинария</summary>
        Cooking,

        [Language.Localized("SKILLS_FISHING_NAME_0", "NAME_0")]
        [Language.Localized("SKILLS_FISHING_NAME_1", "NAME_1")]
        /// <summary>Рыбалка</summary>
        Fishing,
    }
}