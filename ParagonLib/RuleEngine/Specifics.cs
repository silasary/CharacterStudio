namespace ParagonLib.RuleEngine
{
    public enum Specifics
    {
        Unknown,
/// <summary></summary>
/// <affects>Race, Weapon, Companion, Familiar, Power</affects>
Size,
/// <summary></summary>
/// <affects>Race, Armor, Companion, Familiar, Power</affects>
Speed,
/// <summary></summary>
/// <affects>Race</affects>
Characteristics,
/// <summary></summary>
/// <affects>Race</affects>
Physical_Qualities,
/// <summary></summary>
/// <affects>Race</affects>
Playing,
/// <summary></summary>
/// <affects>Race, Power</affects>
Vision,
/// <summary></summary>
/// <affects>Race</affects>
Average_Height,
/// <summary></summary>
/// <affects>Race</affects>
Average_Weight,
/// <summary></summary>
/// <affects>Race, Power</affects>
Ability_Scores,
/// <summary></summary>
/// <affects>Race</affects>
Languages,
/// <summary></summary>
/// <affects>Race</affects>
Skill_Bonuses,
/// <summary></summary>
/// <affects>Race</affects>
Male_Names,
/// <summary></summary>
/// <affects>Race</affects>
Female_Names,
/// <summary></summary>
/// <affects>Race, Class, Hybrid Class, Feat, Racial Trait, Class Feature, Background, Information, Internal, Trait Package, Companion</affects>
Short_Description,
/// <summary></summary>
/// <affects>Race</affects>
Racial_Traits,
/// <summary></summary>
/// <affects>Race</affects>
Names,
/// <summary></summary>
/// <affects>Race</affects>
Tribal_Names,
/// <summary></summary>
/// <affects>Race</affects>
Clan_Names,
/// <summary></summary>
/// <affects>Race</affects>
Bladeling_Names,
/// <summary></summary>
/// <affects>Race</affects>
Bladeling_Surnames,
/// <summary></summary>
/// <affects>Class, Hybrid Class, Build</affects>
Key_Abilities,
/// <summary></summary>
/// <affects>Class, Hybrid Class</affects>
Implements,
/// <summary></summary>
/// <affects>Class, Hybrid Class</affects>
Armor_Proficiencies,
/// <summary></summary>
/// <affects>Class, Hybrid Class</affects>
Weapon_Proficiencies,
/// <summary></summary>
/// <affects>Class, Hybrid Class</affects>
Bonus_to_Defense,
/// <summary></summary>
/// <affects>Class, Hybrid Class, Companion</affects>
Hit_Points_at_1st_Level,
/// <summary></summary>
/// <affects>Class, Hybrid Class, Companion</affects>
Hit_Points_per_Level_Gained,
/// <summary></summary>
/// <affects>Class, Hybrid Class, Power</affects>
Healing_Surges,
/// <summary></summary>
/// <affects>Class, Hybrid Class, Companion, Power</affects>
Trained_Skills,
/// <summary></summary>
/// <affects>Class, Hybrid Class</affects>
Class_Skills,
/// <summary></summary>
/// <affects>Class, Hybrid Class</affects>
Build_Options,
/// <summary></summary>
/// <affects>Class, Hybrid Class, Trait Package, Class Feature</affects>
_PARSED_CLASS_FEATURE,
/// <summary></summary>
/// <affects>Class, Hybrid Class</affects>
Role,
/// <summary></summary>
/// <affects>Class, Hybrid Class</affects>
Power_Source,
/// <summary></summary>
/// <affects>Class, Hybrid Class</affects>
Creating,
/// <summary></summary>
/// <affects>Class, Epic Destiny, Paragon Path, Hybrid Class</affects>
Class_Features,
/// <summary></summary>
/// <affects>Class, Hybrid Class</affects>
Supplemental,
/// <summary></summary>
/// <affects>Class, Hybrid Class, Weapon Group, Magic Item</affects>
Implement,
/// <summary></summary>
/// <affects>Class, Epic Destiny, Paragon Path, Hybrid Class, Racial Trait, Class Feature</affects>
Powers,
/// <summary></summary>
/// <affects>Class, Epic Destiny, Paragon Path, Hybrid Class</affects>
Power_Name,
/// <summary></summary>
/// <affects>Class, Hybrid Class</affects>
_RoleElement,
/// <summary></summary>
/// <affects>Class, Hybrid Class</affects>
_PowerSourceElement,
/// <summary></summary>
/// <affects>Class, Hybrid Class</affects>
_SecondaryPowerSourceElement,
/// <summary></summary>
/// <affects>Class, Hybrid Class</affects>
Secondary_Abilities,
/// <summary></summary>
/// <affects>Class, Hybrid Class</affects>
Hybrid_Talent_Options,
/// <summary></summary>
/// <affects>Class, Hybrid Class</affects>
_ParentClass,
/// <summary></summary>
/// <affects>Class, Hybrid Class, Paragon Path</affects>
Trait_Package,
/// <summary></summary>
/// <affects>Epic Destiny</affects>
Immortality,
/// <summary></summary>
/// <affects>Epic Destiny, Paragon Path, Theme, Feat, Racial Trait, Class Feature, Power, Magic Item, Companion, Item Set Benefit, Internal, Trait Package, Ritual, Ritual Scroll, Background</affects>
_DisplayPowers,
/// <summary></summary>
/// <affects>Hybrid Class</affects>
_BaseClass,
/// <summary></summary>
/// <affects>Deity</affects>
Alignment,
/// <summary></summary>
/// <affects>Deity</affects>
Domains,
/// <summary></summary>
/// <affects>Feat, Class Feature, Magic Item, Companion, Familiar, Item Set</affects>
Tier,
/// <summary></summary>
/// <affects>Feat, Power, Armor, Magic Item</affects>
Special,
/// <summary></summary>
/// <affects>Feat, Ritual, Racial Trait, Class Feature, Background, Ritual Scroll, Companion</affects>
type,
/// <summary></summary>
/// <affects>Feat, Companion</affects>
Associated_Power_Info,
/// <summary></summary>
/// <affects>Feat, Companion</affects>
Associated_Powers,
/// <summary></summary>
/// <affects>Feat</affects>
Container,
/// <summary></summary>
/// <affects>Feat, Racial Trait, Background, Class Feature, Power, Ritual, Ritual Scroll</affects>
Type,
/// <summary></summary>
/// <affects>Feat, Racial Trait, Class Feature, Power, Weapon, Magic Item, Item Set Benefit, Internal, Background Choice, CountsAsRace, CountsAsClass, Ritual Scroll, Grants, Build Suggestions, Category</affects>
_SupportsID,
/// <summary></summary>
/// <affects>Feat, Class Feature</affects>
_RequiresID,
/// <summary></summary>
/// <affects>Ritual, Gear</affects>
Category,
/// <summary></summary>
/// <affects>Ritual, Magic Item</affects>
Key_Skill,
/// <summary></summary>
/// <affects>Ritual</affects>
Component_Cost,
/// <summary></summary>
/// <affects>Ritual</affects>
Duration,
/// <summary></summary>
/// <affects>Ritual, Racial Trait, Class Feature, Power, Magic Item, Ritual Scroll</affects>
Level,
/// <summary></summary>
/// <affects>Ritual, Magic Item</affects>
Time,
/// <summary></summary>
/// <affects>Ritual, Ritual Scroll</affects>
Market_Price,
/// <summary></summary>
/// <affects>Ritual, Power, Magic Item</affects>
Prerequisite,
/// <summary></summary>
/// <affects>Ritual</affects>
_RELATED_ITEMS,
/// <summary></summary>
/// <affects>Skill, Magic Item</affects>
Key_Ability,
/// <summary></summary>
/// <affects>Skill Usage</affects>
Trained_Only,
/// <summary></summary>
/// <affects>Skill Usage</affects>
Skill,
/// <summary></summary>
/// <affects>Racial Trait, Class Feature, Build, Power</affects>
Class,
/// <summary></summary>
/// <affects>Racial Trait, Class Feature, Trait Package</affects>
_CS_ShortDescription,
/// <summary></summary>
/// <affects>Racial Trait, Class Feature, Theme, Feat</affects>
_PARSED_SUB_FEATURES,
/// <summary></summary>
/// <affects>Racial Trait, Class Feature</affects>
_PARSED_CHILD_FEATURES,
/// <summary></summary>
/// <affects>Racial Trait, Class Feature</affects>
Damage_Stat,
/// <summary></summary>
/// <affects>Racial Trait, Weapon</affects>
InternalOnly,
/// <summary></summary>
/// <affects>Class Feature</affects>
Usage,
/// <summary></summary>
/// <affects>Class Feature, Trait Package, Paragon Path</affects>
_CLASSNAME,
/// <summary></summary>
/// <affects>Class Feature, Magic Item</affects>
_ImplementForPower,
/// <summary></summary>
/// <affects>Build</affects>
Suggested,
/// <summary></summary>
/// <affects>Build, Trait Package, Class, Hybrid Class</affects>
_SUGGESTED_FEATS,
/// <summary></summary>
/// <affects>Build, Paragon Path</affects>
_SUGGESTED_POWERS,
/// <summary></summary>
/// <affects>Power, God Fragment</affects>
Power_Usage,
/// <summary></summary>
/// <affects>Power, God Fragment</affects>
Keywords,
/// <summary></summary>
/// <affects>Power, God Fragment</affects>
Action_Type,
/// <summary></summary>
/// <affects>Power, God Fragment</affects>
Effect,
/// <summary></summary>
/// <affects>Power, Companion</affects>
Attack,
/// <summary></summary>
/// <affects>Power, Magic Item</affects>
Requirement,
/// <summary></summary>
/// <affects>Power, God Fragment</affects>
Trigger,
/// <summary></summary>
/// <affects>Power, Magic Item, Class Feature</affects>
Weapon,
/// <summary></summary>
/// <affects>Power, Magic Item, Gear</affects>
Property,
/// <summary></summary>
/// <affects>Power, Magic Item, Companion, Familiar</affects>
Power,
/// <summary></summary>
/// <affects>Power, Armor</affects>
Check,
/// <summary></summary>
/// <affects>Armor, Weapon, Superior Implement, Gear</affects>
Full_Text,
/// <summary></summary>
/// <affects>Armor, Weapon, Superior Implement, Gear, Magic Item</affects>
Weight,
/// <summary></summary>
/// <affects>Armor</affects>
Armor_Bonus,
/// <summary></summary>
/// <affects>Armor</affects>
Minimum_Enhancement_Bonus,
/// <summary></summary>
/// <affects>Armor, Weapon, Superior Implement, Gear, Magic Item, Power</affects>
Gold,
/// <summary></summary>
/// <affects>Armor, Weapon</affects>
Armor_Type,
/// <summary></summary>
/// <affects>Armor, Weapon, Superior Implement, Gear, Magic Item</affects>
Item_Slot,
/// <summary></summary>
/// <affects>Armor, Weapon</affects>
Armor_Category,
/// <summary></summary>
/// <affects>Weapon, Magic Item</affects>
Range,
/// <summary></summary>
/// <affects>Weapon, Companion</affects>
Damage,
/// <summary></summary>
/// <affects>Weapon</affects>
Proficiency_Bonus,
/// <summary></summary>
/// <affects>Weapon</affects>
_Primary_End,
/// <summary></summary>
/// <affects>Weapon</affects>
Weapon_Category,
/// <summary></summary>
/// <affects>Weapon</affects>
Hands_Required,
/// <summary></summary>
/// <affects>Weapon, Superior Implement</affects>
Group,
/// <summary></summary>
/// <affects>Weapon, Superior Implement, Magic Item</affects>
Properties,
/// <summary></summary>
/// <affects>Weapon, Gear</affects>
Silver,
/// <summary></summary>
/// <affects>Weapon, Magic Item</affects>
_AlternateSlot,
/// <summary></summary>
/// <affects>Weapon</affects>
_Secondary_End,
/// <summary></summary>
/// <affects>Weapon</affects>
_IncludeImprovised,
/// <summary></summary>
/// <affects>Weapon, Magic Item</affects>
Critical,
/// <summary></summary>
/// <affects>Weapon, Magic Item</affects>
Enhancement,
/// <summary></summary>
/// <affects>Weapon</affects>
Additional_Slot,
/// <summary></summary>
/// <affects>Superior Implement, Magic Item</affects>
WeaponEquiv,
/// <summary></summary>
/// <affects>Superior Implement</affects>
_DamageType,
/// <summary></summary>
/// <affects>Gear</affects>
Copper,
/// <summary></summary>
/// <affects>Gear</affects>
_RelatedElement,
/// <summary></summary>
/// <affects>Gear</affects>
count,
/// <summary></summary>
/// <affects>Gear, Magic Item</affects>
_Monster,
/// <summary></summary>
/// <affects>Magic Item, Power</affects>
Magic_Item_Type,
/// <summary></summary>
/// <affects>Magic Item, Item Set Benefit</affects>
_Item_Set_ID,
/// <summary></summary>
/// <affects>Magic Item</affects>
Armor,
/// <summary></summary>
/// <affects>Magic Item</affects>
_Rarity,
/// <summary></summary>
/// <affects>Magic Item, Background</affects>
Granted_Powers,
/// <summary></summary>
/// <affects>Magic Item, Power</affects>
Rarity,
/// <summary></summary>
/// <affects>Magic Item</affects>
_IsEnchant,
/// <summary></summary>
/// <affects>Magic Item</affects>
Offhand_Implement_Bonus,
/// <summary></summary>
/// <affects>Magic Item</affects>
_ImplementEquiv,
/// <summary></summary>
/// <affects>Magic Item</affects>
_ImplementForClass,
/// <summary></summary>
/// <affects>Magic Item</affects>
Component,
/// <summary></summary>
/// <affects>Magic Item</affects>
Proficient_As,
/// <summary></summary>
/// <affects>Magic Item, Power</affects>
Utility_Power,
/// <summary></summary>
/// <affects>Magic Item</affects>
utility_Power,
/// <summary></summary>
/// <affects>Magic Item</affects>
_UniversalImplement,
/// <summary></summary>
/// <affects>Magic Item</affects>
EnchantEquiv,
/// <summary></summary>
/// <affects>Magic Item</affects>
_AlwaysExecute,
/// <summary></summary>
/// <affects>Magic Item</affects>
_CannotBeBought,
/// <summary></summary>
/// <affects>Background</affects>
Common_Knowledge,
/// <summary></summary>
/// <affects>Background</affects>
Benefit,
/// <summary></summary>
/// <affects>Background</affects>
Campaign,
/// <summary></summary>
/// <affects>Background</affects>
Associated_Skills,
/// <summary></summary>
/// <affects>Background</affects>
Associated_Languages,
/// <summary></summary>
/// <affects>source</affects>
IsBeta,
/// <summary></summary>
/// <affects>source</affects>
URL,
/// <summary></summary>
/// <affects>source</affects>
Release_Date,
/// <summary></summary>
/// <affects>Companion, Power</affects>
Strength,
/// <summary></summary>
/// <affects>Companion, Power</affects>
Intelligence,
/// <summary></summary>
/// <affects>Companion, Power</affects>
Wisdom,
/// <summary></summary>
/// <affects>Companion, Power</affects>
Dexterity,
/// <summary></summary>
/// <affects>Companion, Power</affects>
Constitution,
/// <summary></summary>
/// <affects>Companion, Power</affects>
Charisma,
/// <summary></summary>
/// <affects>Companion</affects>
Attack_Bonus,
/// <summary></summary>
/// <affects>Companion</affects>
Armor_Class,
/// <summary></summary>
/// <affects>Companion</affects>
Fortitude_Defense,
/// <summary></summary>
/// <affects>Companion</affects>
Reflex_Defense,
/// <summary></summary>
/// <affects>Companion</affects>
Will_Defense,
/// <summary></summary>
/// <affects>Companion, Familiar, Power</affects>
Secondary_Speed,
/// <summary></summary>
/// <affects>Companion</affects>
Ability_Score,
/// <summary></summary>
/// <affects>Companion</affects>
Charge,
/// <summary></summary>
/// <affects>Companion, Power</affects>
Combat_Advantage,
/// <summary></summary>
/// <affects>Companion</affects>
Opportunity_Attacks,
/// <summary></summary>
/// <affects>Companion, God Fragment, Power, Familiar</affects>
Quirks,
/// <summary></summary>
/// <affects>Companion</affects>
Companion_Power,
/// <summary></summary>
/// <affects>Familiar, God Fragment, Power</affects>
Constant_Benefits,
/// <summary></summary>
/// <affects>Familiar, Power</affects>
Senses,
/// <summary></summary>
/// <affects>Item Set</affects>
Lore,
/// <summary></summary>
/// <affects>Item Set</affects>
Set_Items,
/// <summary></summary>
/// <affects>Item Set</affects>
Benefits,
/// <summary></summary>
/// <affects>Item Set Benefit</affects>
Piece_Count,
/// <summary></summary>
/// <affects>God Fragment</affects>
Deity,
/// <summary></summary>
/// <affects>Level</affects>
XP_Needed,
/// <summary></summary>
/// <affects>Background Choice</affects>
_IsBenefit,
/// <summary></summary>
/// <affects>Class Feature, Class</affects>
_DisplayName,
/// <summary></summary>
/// <affects>Magic Item, Weapon, Class Feature, Power</affects>
_Tags,
/// <summary></summary>
/// <affects>Magic Item</affects>
_Bonus,
/// <summary></summary>
/// <affects>Magic Item</affects>
_Enhancement,
/// <summary></summary>
/// <affects>Magic Item, Gear</affects>
_AssociatedMonsters,
/// <summary></summary>
/// <affects>Magic Item</affects>
Level_11_Enhancement,
/// <summary></summary>
/// <affects>Magic Item</affects>
Enhancement_Level_11,
/// <summary></summary>
/// <affects>Magic Item</affects>
Disease:_Foul_Rotting,
/// <summary></summary>
/// <affects>Magic Item</affects>
Formula_Cost,
/// <summary></summary>
/// <affects>Companion, Power</affects>
Standard_Actions,
/// <summary></summary>
/// <affects>Companion, Power</affects>
Move_Actions,
/// <summary></summary>
/// <affects>Companion, Power</affects>
Triggered_Actions,
/// <summary></summary>
/// <affects>Magic Item, Power</affects>
Attack_Power,
/// <summary></summary>
/// <affects>Magic Item</affects>
Alternative_Reward,
/// <summary></summary>
/// <affects>Magic Item</affects>
Stone_Grasp,
/// <summary></summary>
/// <affects>Magic Item</affects>
Uitilty_Power,
/// <summary></summary>
/// <affects>Background</affects>
Common_Kowledge,
/// <summary></summary>
/// <affects>Racial Trait</affects>
_TraitReplacement,
/// <summary></summary>
/// <affects>Race</affects>
Attitudes_and_Beliefs,
/// <summary></summary>
/// <affects>Race</affects>
Tinker_Gnome_Communities,
/// <summary></summary>
/// <affects>Race</affects>
Tinker_Gnomre_Adventurers,
/// <summary></summary>
/// <affects>Class Feature, Power, Feat</affects>
_DisplayAssociates,
/// <summary></summary>
/// <affects>Associate</affects>
_Associated_Power,
/// <summary></summary>
/// <affects>Feat</affects>
Rod_Expertise,
/// <summary></summary>
/// <affects>Class, Paragon Path</affects>
_SUGGESTED_FEATURES,
/// <summary></summary>
/// <affects>Class Feature</affects>
Pact_Reward,
/// <summary></summary>
/// <affects>Class Feature</affects>
_DisplayWeapons,
/// <summary></summary>
/// <affects>Class</affects>
Skald_Powers,
/// <summary></summary>
/// <affects>Class Feature</affects>
Pet_Hit_Points,
/// <summary></summary>
/// <affects>Class Feature</affects>
Pet_Toughness,
/// <summary></summary>
/// <affects>Class Feature</affects>
HALF-CON,
/// <summary></summary>
/// <affects>Companion, Power</affects>
Mount_Companion,
/// <summary></summary>
/// <affects>Companion</affects>
Manipulate_Items,

    }
}
