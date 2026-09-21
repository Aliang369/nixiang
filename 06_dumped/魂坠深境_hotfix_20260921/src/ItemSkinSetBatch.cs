using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemSkinSetBatch
{
	[SerializeField]
	public ItemSkinSetDef skinSetDef;

	[SerializeField]
	public int batchIndex;

	public static ApplicationStateInfo applicationState => Bootstrap.applicationState;

	public string DisplayName => skinSetDef?.GetBatchDisplayName(batchIndex);

	public ItemSkinSetDef.SkinSetBatchConfig BatchConfig => skinSetDef?.GetBatchConfig(batchIndex);

	public bool isAllUnlocked => skinSetDef.IsAllUnlocked(applicationState, batchIndex);

	public int lockedCount => allSkinCount - unlockedCount;

	public int unlockedCount => GetSubSkins(ungot: false).Count((ItemSkinDef s) => applicationState.itemSkinManager.IsItemSkinUnlocked(s));

	public int allSkinCount => GetSubSkins(ungot: false).Count;

	private List<ItemSkinDef> PreviewSubSkins_Editor
	{
		get
		{
			try
			{
				return GetSubSkins(ungot: false);
			}
			catch
			{
			}
			return null;
		}
	}

	private ItemSkinSetBatch()
	{
	}

	public ItemSkinSetBatch(ItemSkinSetDef skinSetDef, int batchIndex)
	{
		this.skinSetDef = skinSetDef;
		this.batchIndex = batchIndex;
	}

	public List<ItemSkinDef> GetSubSkins(bool ungot = true)
	{
		return skinSetDef.GetSubSkins(Bootstrap.applicationState.assetCatalog, batchIndex, ungot);
	}

	public string GetSkinUnlockProgressText()
	{
		return ScriptableObjectSingleton<TextDataPreset>.instance.term_item_skin_got_count_arg0_per_all_count_arg1.FormatWith(unlockedCount, allSkinCount);
	}
}
