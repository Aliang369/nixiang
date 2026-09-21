using System;
using System.Collections.Generic;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MultiInfoDisplayPanel : CellUI<MultiInfoDisplayPanel.SetupInfo>, IDisplayModulesController
{
	public class MultiInfoPanelDisplayConfig
	{
		public float hideMoveTime = 1f;
	}

	public class SetupInfo
	{
		public int coin;

		public int crystal;

		public List<Influence> influences;

		public List<Item> items;

		public Action onHide;

		public bool showUncollectedStatus;

		public List<(Rune, Item, int)> runes;

		public List<GameSkill> skills;
	}

	public class DisplayModule : UIDisplayModule
	{
		public MultiInfoDisplayPanel display => parent as MultiInfoDisplayPanel;

		public SetupInfo setupInfo => display.setupInfo;
	}

	[SerializeField]
	public HorizontalLayoutGroup displaysRoot;

	[SerializeField]
	private ItemInfoDisplay_Normal itemInfoDisplay;

	[SerializeField]
	public RectTransform itemInfoDisplayParent;

	[SerializeField]
	private InfluenceDisplay influenceDisplay;

	[SerializeField]
	public RectTransform influenceInfoDisplayParent;

	[SerializeField]
	private RuneDisplay runeDisplayPrefab;

	[SerializeField]
	public RectTransform runeDisplayParent;

	[SerializeField]
	private SkillInfoDisplay skillInfoDisplay;

	[SerializeField]
	public RectTransform skillInfoDisplayParent;

	[SerializeField]
	private TextMeshProUGUI crystalText;

	[SerializeField]
	public RectTransform crystalInfoDisplayParent;

	[SerializeField]
	private TextMeshProUGUI coinText;

	[SerializeField]
	public RectTransform coinInfoDisplayParent;

	[SerializeField]
	private GameButton backButton;

	[NonSerialized]
	public CellUIPrefabPool<InfluenceDisplay, Influence> influenceInfoDisplayPool;

	[NonSerialized]
	public CellUIPrefabPool<ItemInfoDisplay_Normal, Item> itemInfoDisplayPool;

	[NonSerialized]
	public CellUIPrefabPool<RuneDisplay, Rune> runeDisplayPool;

	[NonSerialized]
	protected SetupInfo setupInfo;

	[NonSerialized]
	public CellUIPrefabPool<SkillInfoDisplay, GameSkill> skillInfoDisplayPool;

	public InfluenceDisplay InfluenceInfoDisplayPrefab => influenceDisplay;

	protected override void Init()
	{
		itemInfoDisplayPool = GetPrefabPool<ItemInfoDisplay_Normal, Item>(itemInfoDisplay, itemInfoDisplayParent);
		influenceInfoDisplayPool = GetPrefabPool<InfluenceDisplay, Influence>(influenceDisplay, influenceInfoDisplayParent);
		runeDisplayPool = GetPrefabPool<RuneDisplay, Rune>(runeDisplayPrefab, runeDisplayParent);
		skillInfoDisplayPool = GetPrefabPool<SkillInfoDisplay, GameSkill>(skillInfoDisplay, skillInfoDisplayParent);
		if (backButton != null)
		{
			backButton.onClick.AddListener(_OnBackgroundButtonClicked);
		}
		itemInfoDisplay.gameObject.SetActive(value: false);
		influenceDisplay.gameObject.SetActive(value: false);
		runeDisplayPrefab.gameObject.SetActive(value: false);
		base.gameObject.SetActive(value: false);
	}

	protected override void OnSetup(SetupInfo setupInfo)
	{
		if (this.setupInfo != null)
		{
			OnUnset();
		}
		base.gameObject.SetActive(value: true);
		this.setupInfo = setupInfo;
		if (setupInfo.items.IsNullOrEmpty())
		{
			itemInfoDisplayParent.gameObject.SetActive(value: false);
			itemInfoDisplayParent.SetWidth(0f);
		}
		else
		{
			itemInfoDisplayParent.gameObject.SetActive(value: true);
			itemInfoDisplayParent.SetWidth(424 * setupInfo.items.Count);
			foreach (Item item2 in setupInfo.items.OrEmptyIfNull())
			{
				SetupItemUncollectedStatus(itemInfoDisplayPool.Spawn(item2), item2, setupInfo.showUncollectedStatus);
			}
		}
		if (setupInfo.influences.IsNullOrEmpty())
		{
			influenceInfoDisplayParent.gameObject.SetActive(value: false);
		}
		else
		{
			influenceInfoDisplayParent.gameObject.SetActive(value: true);
			foreach (Influence item3 in setupInfo.influences.OrEmptyIfNull())
			{
				SetupInfluenceUncollectedStatus(influenceInfoDisplayPool.Spawn(item3), item3, setupInfo.showUncollectedStatus);
			}
		}
		runeDisplayParent.gameObject.SetActive(value: false);
		if (!setupInfo.runes.IsNullOrEmpty())
		{
			Dictionary<Item, Item> dictionary = new Dictionary<Item, Item>();
			foreach (var (data, item, _) in setupInfo.runes.OrEmptyIfNull())
			{
				if (item == null)
				{
					runeDisplayParent.gameObject.SetActive(value: true);
					runeDisplayPool.Spawn(data);
					continue;
				}
				itemInfoDisplayParent.gameObject.SetActive(value: true);
				if (!dictionary.ContainsKey(item))
				{
					Item value = item.Copy();
					dictionary.Add(item, value);
				}
				else
				{
					Item value = dictionary[item];
				}
			}
			foreach (KeyValuePair<Item, Item> item4 in dictionary)
			{
				ItemInfoDisplay_Normal itemInfoDisplay_Normal = itemInfoDisplayPool.Spawn(item4.Value);
				SetupItemUncollectedStatus(itemInfoDisplay_Normal, item4.Value, setupInfo.showUncollectedStatus);
				itemInfoDisplay_Normal.GetModule<ItemInfoReferenceModule>().ShowAllSideReference();
			}
			itemInfoDisplayParent.SetWidth(itemInfoDisplayParent.GetWidth() + (float)(424 * dictionary.Count));
		}
		if (setupInfo.skills.IsNullOrEmpty())
		{
			skillInfoDisplayParent.gameObject.SetActive(value: false);
		}
		else
		{
			skillInfoDisplayParent.gameObject.SetActive(value: true);
			foreach (GameSkill item5 in setupInfo.skills.OrEmptyIfNull())
			{
				skillInfoDisplayPool.Spawn(item5);
			}
		}
		if (setupInfo.crystal == 0)
		{
			crystalInfoDisplayParent.gameObject.SetActive(value: false);
		}
		else
		{
			crystalText.text = setupInfo.crystal.ToString();
			crystalInfoDisplayParent.gameObject.SetActive(value: true);
		}
		if (setupInfo.coin == 0)
		{
			coinInfoDisplayParent.gameObject.SetActive(value: false);
			return;
		}
		coinText.text = setupInfo.coin.ToString();
		coinInfoDisplayParent.gameObject.SetActive(value: true);
	}

	protected override void OnUnset()
	{
		itemInfoDisplayPool.DespawnAll();
		influenceInfoDisplayPool.DespawnAll();
		runeDisplayPool.DespawnAll();
		skillInfoDisplayPool.DespawnAll();
		base.gameObject.SetActive(value: false);
		setupInfo?.onHide?.Invoke();
		setupInfo = null;
	}

	private void SetupItemUncollectedStatus(ItemInfoDisplay_Normal display, Item item, bool showUncollectedStatus)
	{
		if (!(display == null))
		{
			display.showUncollectedStatus = showUncollectedStatus;
			display.mainItemInfoDisplay?.SetDirty();
		}
	}

	private void SetupInfluenceUncollectedStatus(InfluenceDisplay display, Influence influence, bool showUncollectedStatus)
	{
		if (!(display == null))
		{
			RewardUncollectedTag rewardUncollectedTag = RewardUncollectedTag.AttachTo((display.image != null) ? ((MonoBehaviour)display.image) : ((MonoBehaviour)display), RewardUncollectedTagLayout.InfluenceReward);
			if (showUncollectedStatus && DiscoverManager.IsContentInConfiguredIndexPage(influence?.def, CellUI<SetupInfo>.applicationState.assetCatalog))
			{
				rewardUncollectedTag?.Setup(influence?.def, CellUI<SetupInfo>.applicationState);
			}
			else
			{
				rewardUncollectedTag?.Hide();
			}
		}
	}

	private void _OnBackgroundButtonClicked()
	{
		Unset();
	}
}
