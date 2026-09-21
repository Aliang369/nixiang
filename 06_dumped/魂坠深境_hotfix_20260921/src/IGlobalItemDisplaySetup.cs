using System.Collections.Generic;
using UnityEngine;

public interface IGlobalItemDisplaySetup
{
	Sprite GlobalItemCellIcon => ScriptableObjectSingleton<GameDataManager>.instance.itemPlaceHolderIcon;

	int GlobalItemCount => -1;

	string GlobalItemCellName => null;

	string GlobalItemTitle => null;

	QualityLevel GlobalItemQuality => QualityLevel.Common;

	ProfileSkill GlobalItemRefSkill => null;

	SkillGem GlobalSkillGem => null;

	ItemSkinDef GlobalItemRefItemSkin => null;

	ItemSkinSetDef GlobalItemSkinSetDef => null;

	List<IGlobalItemDisplaySetup> RandomStartGameBuffs => null;

	List<IGlobalItemDisplaySetup> RandomSkillDefs => null;

	List<IGlobalItemDisplaySetup> RandomMatterDefs => null;

	PlayerCharacterDef GlobalItemRefPlayerCharacter => null;

	Item GlobalItemRefItem => null;

	Rune GlobalItemRefRune => null;

	Influence globalItemRefInfluence => null;

	bool isNew => false;

	bool isHidden => false;

	bool JumpToGachaAnnouncement => false;

	bool JumpToStoreButton => false;

	string JumpToStoreButtonText => ScriptableObjectSingleton<TextDataPreset>.instance.term_global_item_jump_to_store;

	PurchaseGroupType JumpToStorePurchaseGroupType => PurchaseGroupType.RealMoneyBuy;

	int JumpToStoreSubIndex => -1;

	bool HideNumberTextOnOnlyOne => false;

	string GetGlobalItemDescription(ApplicationStateInfo applicationState, out TextDataGenerator generator);

	string GetGlobalItemStorageCountText(ApplicationStateInfo applicationState)
	{
		int? globalItemStorageCount = GetGlobalItemStorageCount(applicationState);
		if (!globalItemStorageCount.HasValue)
		{
			return null;
		}
		return $"{globalItemStorageCount}";
	}

	int? GetGlobalItemStorageCount(ApplicationStateInfo applicationState)
	{
		return null;
	}
}
