using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Permissions;
using ContainerOfSoul.TargetRequirement;
using Sirenix.Serialization;

[assembly: BindTypeNameToType("PlayerCharacterRelicSelector", typeof(CurrentCharacterPoolItemDefSelector))]
[assembly: BindTypeNameToType("AllSkillsSelector", typeof(AllActivatedSkillsSelector))]
[assembly: BindTypeNameToType("ContainerOfSoul.TargetRequirement.TargetRequirement", typeof(CastPrepare))]
[assembly: BindTypeNameToType("AchievementObserver", typeof(GlobalTriggerObserver))]
[assembly: BindTypeNameToType("GameEventAchivementObserver", typeof(GameEventObserver))]
[assembly: BindTypeNameToType("AddItemSlot", typeof(ChangeItemSlot))]
[assembly: BindTypeNameToType("RemoveItem", typeof(ExileItem))]
[assembly: BindTypeNameToType("GameReward", typeof(GlobalRewardSetup))]
[assembly: BindTypeNameToType("GameRewardSetup", typeof(GlobalRewardSetup))]
[assembly: BindTypeNameToType("ItemCost", typeof(ItemConstraint))]
[assembly: BindTypeNameToType("ItemTagCost", typeof(ItemTagConstraint))]
[assembly: BindTypeNameToType("DamageAbleItemCost", typeof(DamageAbleItemConstraint))]
[assembly: BindTypeNameToType("identicalItemCost", typeof(identicalItemConstraint))]
[assembly: BindTypeNameToType("AnyItemCost", typeof(AnyItemConstraint))]
[assembly: BindTypeNameToType("CoolDowningCost", typeof(CoolDowningConstraint))]
[assembly: BindTypeNameToType("TypeItemCost", typeof(TypeItemConstraint))]
[assembly: BindTypeNameToType("ItemBlackBoard.ValuePair", typeof(ItemBlackBoard.ValueInfo))]
[assembly: BindTypeNameToType("AddItems", typeof(SpawnItems))]
[assembly: BindTypeNameToType("StoryPoint+SpawnWorldEventObjectDisplay", typeof(StoryPoint.SpawnRoomObjectDisplay))]
[assembly: AssemblyVersion("0.0.0.0")]
