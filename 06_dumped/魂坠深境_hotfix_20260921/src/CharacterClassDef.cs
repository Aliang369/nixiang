using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class CharacterClassDef : CTScriptableObject
{
	[SerializeField]
	[LocalizeText("职业名称")]
	[CTSOAssetName]
	public string characterName;

	[SerializeField]
	[LocalizeText("职业描述")]
	public string characterDescription;

	[SerializeField]
	public CharacterClassType mClass;

	[FormerlySerializedAs("defaultSkin")]
	[SerializeField]
	public PlayerCharacterDef character;

	[SerializeField]
	public Sprite professionIcon;

	[SerializeField]
	public int sortDisplayOrderIndex;

	[SerializeField]
	public List<RecommendItemGroup.Config> recommendItemConfig;

	[SerializeField]
	public MatterDef classMatter;

	[SerializeField]
	public MatterDef soulMatter;

	[SerializeField]
	public MatterDef classObsessionMatter;

	[SerializeField]
	public Sprite forgetShrinePointIcon;

	public override string CTAssetIDPrepend => "CHACLASS";

	public CharacterClassState state => CTScriptableObject.applicationState.characterManager.GetCharacterClassState(this);

	public List<ItemDefinition> GetPoolItems(ApplicationStateInfo applicationState, bool includeLocked = false)
	{
		List<ItemDefinition> list = applicationState.assetCatalog.itemDefCatalog.ValidAssets.FindAll(delegate(ItemDefinition item)
		{
			if (item.generationInfo == null || !item.generationInfo.toGenerate)
			{
				return false;
			}
			return item.generationInfo.attachClass != null && item.generationInfo.attachClass.Contains(mClass);
		});
		if (!includeLocked && applicationState.globalUnlockManager != null)
		{
			list = list.FindAll((ItemDefinition it) => applicationState.globalUnlockManager.IsUnlocked(it));
		}
		return list;
	}
}
