using System;
using System.Collections.Generic;
using System.Linq;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PVPDefenderPreviewCell : CellUI<PVPDefender>
{
	public TextMeshProUGUI defenderNameText;

	public Image defenderAvatarText;

	public ItemDisplayBase itemDisplayPrefab;

	public InfluenceDisplay influenceDisplayPrefab;

	public RectTransform deckContentPreviewParent;

	[NonSerialized]
	private CellUIPrefabPool<ItemDisplayBase, Item> itemDisplayPool;

	[NonSerialized]
	private CellUIPrefabPool<InfluenceDisplay, Influence> influenceDisplayPool;

	protected override void Init()
	{
		itemDisplayPool = GetPrefabPool<ItemDisplayBase, Item>(itemDisplayPrefab, deckContentPreviewParent);
		influenceDisplayPool = GetPrefabPool<InfluenceDisplay, Influence>(influenceDisplayPrefab, deckContentPreviewParent);
		if (CellUI<PVPDefender>.gameState?.UIManager?.universalCommonInfoPanel != null)
		{
			itemDisplayPool.ConfigInfoPanel(CellUI<PVPDefender>.gameState.UIManager.universalCommonInfoPanel);
			influenceDisplayPool.ConfigInfoPanel(CellUI<PVPDefender>.gameState.UIManager.universalCommonInfoPanel);
		}
		else if (CellUI<PVPDefender>.fortressState?.uiManager?.universalCommonInfoPanel != null)
		{
			itemDisplayPool.ConfigInfoPanel(CellUI<PVPDefender>.fortressState.uiManager.universalCommonInfoPanel);
			influenceDisplayPool.ConfigInfoPanel(CellUI<PVPDefender>.fortressState.uiManager.universalCommonInfoPanel);
		}
	}

	protected override void OnSetup(PVPDefender defender)
	{
		if (defender == null)
		{
			defenderNameText.text = "";
			defenderAvatarText.sprite = null;
			itemDisplayPool?.DespawnAll();
			influenceDisplayPool?.DespawnAll();
			return;
		}
		string text = ((!string.IsNullOrWhiteSpace(defender.defenderName)) ? defender.defenderName : defender.sourcePlayerName);
		if (string.IsNullOrWhiteSpace(text))
		{
			Debug.LogWarning("PVP defender has no display name.");
			text = ScriptableObjectSingleton<TextDataPreset>.instance.term_unknown_pvp_defender;
		}
		defenderNameText.text = text;
		EnemyInfoData assetPreData = defender.enemyAvatarRef.GetAssetPreData(CellUI<PVPDefender>.applicationState.assetCatalog);
		defenderAvatarText.sprite = assetPreData.avatar;
		List<Item> allItems = defender.deckPreset.GetAllItems();
		List<Influence> allInfluences = defender.deckPreset.GetAllInfluences();
		List<object> list = new List<object>();
		list.AddRange(allItems);
		list.AddRange(allInfluences);
		List<object> randomComps = GetRandomComps(list, defender.guid, 5);
		itemDisplayPool.DespawnAll();
		influenceDisplayPool.DespawnAll();
		foreach (object item in randomComps)
		{
			if (item is Item data)
			{
				itemDisplayPool.Spawn(data, deckContentPreviewParent);
			}
			else if (item is Influence data2)
			{
				influenceDisplayPool.Spawn(data2, deckContentPreviewParent);
			}
		}
	}

	protected override void OnUnset()
	{
		itemDisplayPool?.DespawnAll();
		influenceDisplayPool?.DespawnAll();
	}

	private List<T> GetRandomComps<T>(List<T> comps, string guidSeed, int count)
	{
		if (comps == null || comps.Count == 0)
		{
			return new List<T>();
		}
		int hashCode = guidSeed.GetHashCode();
		System.Random random = new System.Random(hashCode);
		List<T> list = comps.OrderBy((T _) => random.Next()).ToList();
		return list.Take(Math.Min(count, list.Count)).ToList();
	}
}
