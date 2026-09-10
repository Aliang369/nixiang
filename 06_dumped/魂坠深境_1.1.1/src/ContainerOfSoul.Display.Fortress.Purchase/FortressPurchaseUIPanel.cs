using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common.UI;
using UnityEngine;

namespace ContainerOfSoul.Display.Fortress.Purchase;

public class FortressPurchaseUIPanel : UIBase, IStackPanel, IShowAsync, IFrameUpdate
{
	[SerializeField]
	public GameButton backButton;

	[SerializeField]
	public MatterCell matterCellPrefab;

	[SerializeField]
	public RectTransform matterCellParent;

	[SerializeField]
	public MatterHintPanel matterHintPanel;

	[SerializeField]
	public GameObject warningPanel;

	[SerializeField]
	public GameButton openTestReturnRuleButton;

	[SerializeField]
	public GameButton recycleObsessionButton;

	[SerializeField]
	public GameButton closeTestReturnRuleButton;

	[SerializeField]
	public GameObject testReturnRulePanel;

	[SerializeField]
	public CellUIPrefabPoolConfig<PurchaseGroupDefDisplay, PurchaseGroupDef> smallGridGroupPool;

	[SerializeField]
	public CellUIPrefabPoolConfig<PurchaseGroupDefDisplay, PurchaseGroupDef> horizontalGridGroupPool;

	[SerializeField]
	public CellUIPrefabPoolConfig<PurchaseGroupDefDisplay, PurchaseGroupDef> itemSkinGroupPool;

	[SerializeField]
	public CellUIPrefabPoolConfig<PurchaseGroupDefDisplay, PurchaseGroupDef> seasonGroupPool;

	[SerializeField]
	public SwitchPanel switchPanel;

	[SerializeField]
	public SwitchPanel switchPanel2;

	[SerializeField]
	public GameObject switchPanel2Area;

	[SerializeField]
	public SwitchImageSwitchTag[] switchTags;

	[SerializeField]
	public SwitchImageSwitchTag[] switchTag2s;

	[NonSerialized]
	private CellUIPrefabPool<MatterCell, MatterInfo> groupMatterCellDisplayPool;

	[NonSerialized]
	private bool realMoneyViewed;

	[NonSerialized]
	private HashSet<int> viewedRealMoneySubGroups = new HashSet<int>();

	private const int RealMoneySubGroupIndex_Bundle = 0;

	private const int RealMoneySubGroupIndex_Period = 1;

	private const int RealMoneySubGroupIndex_ItemSkin = 2;

	private const int RealMoneySubGroupIndex_Credit = 3;

	public bool CanOpenPurchasePanel
	{
		get
		{
			ApplicationStateInfo applicationStateInfo = UIBase.applicationState;
			if (applicationStateInfo == null)
			{
				return false;
			}
			return applicationStateInfo.globalVariableManager?.GetVariableFlag(ApplicationGlobalVariableManager.EnablePurchasePanel) == true;
		}
	}

	public Func<PurchaseInfo, bool> showChecker
	{
		get
		{
			if (SortedGroups[switchPanel.targetShowTag.setupData.i].groupType != PurchaseGroupType.RealMoneyBuy)
			{
				return null;
			}
			return GetRealMoneySubGroupShowChecker(switchPanel2.targetShowTag.setupData.i);
		}
	}

	private List<PurchaseGroupDef> SortedGroups => UIBase.applicationState.purchaseManager.GetPurchaseGroupShowInMainPurchasePanel();

	public GlobalRewardPackageDisplay fortressGlobalRewardPackageDisplay => UIBase.applicationState.fortressState.uiManager.globalRewardPackageDisplay;

	public bool TryShowPurchasePanel()
	{
		if (!CanOpenPurchasePanel)
		{
			return false;
		}
		this.ShowUI();
		return true;
	}

	public bool TryShowPurchaseGroup(PurchaseGroupType type)
	{
		if (!TryShowPurchasePanel())
		{
			return false;
		}
		OpenTypePurchaseGroup(type);
		return true;
	}

	public bool TryShowPurchaseGroup(PurchaseGroupType type, int subIndex)
	{
		if (!TryShowPurchasePanel())
		{
			return false;
		}
		OpenTypePurchaseGroup(type, subIndex);
		return true;
	}

	public bool TryShowSwitchPanelIndex(int index, int subIndex = -1)
	{
		if (!TryShowPurchasePanel())
		{
			return false;
		}
		SetSwitchPanelIndex(index, subIndex);
		return true;
	}

	protected override void Init()
	{
		InitUI(matterHintPanel);
		Init(smallGridGroupPool);
		Init(horizontalGridGroupPool);
		Init(itemSkinGroupPool);
		Init(seasonGroupPool);
		backButton.onClick.AddListener(_OnBackButtonClicked);
		openTestReturnRuleButton.gameObject.SetActive(value: false);
		closeTestReturnRuleButton.onClick.AddListener(_OnCloseTestReturnRuleButtonClicked);
		recycleObsessionButton.onClick.AddListener(delegate
		{
			UIBase.fortressState.uiManager.recycleObsessionPanel.Show();
		});
		UIBase.applicationState.eventManager.matterChangeEvent.AddListener(_OnMatterChange);
		SwitchPanel obj = switchPanel;
		obj.onTagSwitched = (Action)Delegate.Combine(obj.onTagSwitched, new Action(_OnTagSwitched));
		SwitchPanel obj2 = switchPanel2;
		obj2.onTagSwitched = (Action)Delegate.Combine(obj2.onTagSwitched, new Action(_OnSecondTagSwitched));
		groupMatterCellDisplayPool = GetPrefabPool<MatterCell, MatterInfo>(matterCellPrefab, matterCellParent);
		CellUIPrefabPool<MatterCell, MatterInfo> cellUIPrefabPool = groupMatterCellDisplayPool;
		cellUIPrefabPool.onSetup = (Action<MatterCell>)Delegate.Combine(cellUIPrefabPool.onSetup, new Action<MatterCell>(ConfigureGroupMatterCellInfoPanel));
		SetDirty();
		InitSwitchTag();
	}

	private Func<PurchaseInfo, bool> GetRealMoneySubGroupShowChecker(int subGroupIndex)
	{
		return subGroupIndex switch
		{
			0 => (PurchaseInfo p) => p.resetTimesType == ResetTimeType.None && p is RewardBundlePurchase, 
			1 => (PurchaseInfo p) => p.resetTimesType != ResetTimeType.None, 
			3 => (PurchaseInfo p) => p is RealMoneyToMatterPurchase, 
			_ => (PurchaseInfo p) => true, 
		};
	}

	protected override void Dispose()
	{
		UIBase.applicationState.eventManager.matterChangeEvent.RemoveListener(_OnMatterChange);
	}

	void IShowAsync.OnShowStart()
	{
		if (!CanOpenPurchasePanel)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		foreach (MatterCell activeElement in groupMatterCellDisplayPool.GetActiveElements())
		{
			ConfigureGroupMatterCellInfoPanel(activeElement);
		}
		realMoneyViewed = false;
		viewedRealMoneySubGroups.Clear();
		Bootstrap.instance.adsManager.LoadAd("free_money");
		base.gameObject.SetActive(value: true);
		ThinkingAnalyticsExtends.TrackOpenFortressSubPanel(UIBase.applicationState, "地下室-商店");
		CheckRealMoneyViewed();
		MarkCurrentRealMoneySubGroupViewed();
		MarkCurrentCardShopViewed();
		SetDirty();
		if (UIBase.applicationState.purchaseManager.ShouldRefreshRecord())
		{
			StartCoroutine(UIBase.applicationState.purchaseManager.RefreshPurchaseProfileData(OnRefreshPurchaseProfileData));
		}
	}

	IEnumerator IShowAsync.ShowAnim()
	{
		yield return UIBase.applicationState.matterManager.SyncMatter();
		base.gameObject.SetActive(value: true);
	}

	public void FrameUpdate()
	{
		if (base.gameObject.activeInHierarchy && UIBase.applicationState.purchaseManager.ShouldRefreshRecord())
		{
			StartCoroutine(UIBase.applicationState.purchaseManager.RefreshPurchaseProfileData(OnRefreshPurchaseProfileData));
		}
	}

	private void InitSwitchTag()
	{
		InitUI(switchPanel);
		InitUI(switchPanel2);
		SwitchImageSwitchTag[] array = switchTags;
		foreach (SwitchImageSwitchTag child in array)
		{
			InitUI(child);
		}
		array = switchTag2s;
		foreach (SwitchImageSwitchTag child2 in array)
		{
			InitUI(child2);
		}
		switchPanel.ClearTags();
		switchPanel2.ClearTags();
		List<PurchaseGroupDef> sortedGroups = SortedGroups;
		if (sortedGroups.Count > switchTags.Length)
		{
			Debug.LogError($"[FortressPurchaseUIPanel] 购买组数量({sortedGroups.Count})超过预置Tag槽位({switchTags.Length}),超出部分将被丢弃,请补充switchTags或收敛配置");
		}
		int num = Mathf.Min(sortedGroups.Count, switchTags.Length);
		for (int j = 0; j < num; j++)
		{
			SwitchImageSwitchTag switchImageSwitchTag = switchTags[j];
			switchImageSwitchTag.Setup(new SwitchTag.Data
			{
				i = j,
				appendData = sortedGroups[j],
				shouldShowRedDot = ShouldShowRedDot
			});
			switchImageSwitchTag.SetText(sortedGroups[j].displayName);
			switchPanel.AddTag(switchImageSwitchTag);
		}
		if (UIBase.applicationState.seasonManager.currentSeason.seasonPassUnlocked || UIBase.applicationState.seasonManager.currentSeason.highseasonPassUnlocked)
		{
			switchPanel.Select(sortedGroups.FindIndex((PurchaseGroupDef g) => g.groupType == PurchaseGroupType.RealMoneyBuy));
		}
		else
		{
			switchPanel.Select(0);
		}
		SwitchImageSwitchTag switchImageSwitchTag2 = switchTag2s[0];
		switchImageSwitchTag2.Setup(new SwitchTag.Data
		{
			i = 0,
			appendData = null,
			shouldShowRedDot = ShouldShowSubGroupRedDot
		});
		switchImageSwitchTag2.SetText(base.textData.term_real_money_purchase_sub_bundle);
		switchPanel2.AddTag(switchImageSwitchTag2);
		switchImageSwitchTag2 = switchTag2s[1];
		switchImageSwitchTag2.Setup(new SwitchTag.Data
		{
			i = 1,
			appendData = null,
			shouldShowRedDot = ShouldShowSubGroupRedDot
		});
		switchImageSwitchTag2.SetText(base.textData.term_real_money_purchase_sub_period);
		switchPanel2.AddTag(switchImageSwitchTag2);
		switchImageSwitchTag2 = switchTag2s[2];
		switchImageSwitchTag2.Setup(new SwitchTag.Data
		{
			i = 2,
			appendData = null,
			shouldShowRedDot = ShouldShowSubGroupRedDot
		});
		switchImageSwitchTag2.SetText(base.textData.term_real_money_purchase_sub_itemSkin);
		switchPanel2.AddTag(switchImageSwitchTag2);
		switchImageSwitchTag2 = switchTag2s[3];
		switchImageSwitchTag2.Setup(new SwitchTag.Data
		{
			i = 3,
			appendData = null,
			shouldShowRedDot = ShouldShowSubGroupRedDot
		});
		switchImageSwitchTag2.SetText(base.textData.term_real_money_purchase_sub_credit);
		switchPanel2.AddTag(switchImageSwitchTag2);
		switchPanel2.Select(0);
		IEnumerable<PurchaseInfo> GetRealMoneySubGroupPurchases(int subGroupIndex)
		{
			if (subGroupIndex == 2)
			{
				IEnumerable<PurchaseInfo> enumerable = ScriptableObjectSingleton<CalendarDataManager>.instance.skinPurchaseGroupDef?.purchases;
				return enumerable ?? Enumerable.Empty<PurchaseInfo>();
			}
			PurchaseGroupDef purchaseGroupDef = sortedGroups.FirstOrDefault((PurchaseGroupDef g) => g.groupType == PurchaseGroupType.RealMoneyBuy);
			if (purchaseGroupDef?.purchases == null)
			{
				return Enumerable.Empty<PurchaseInfo>();
			}
			return purchaseGroupDef.purchases.Where(GetRealMoneySubGroupShowChecker(subGroupIndex));
		}
		static bool ShouldShowFreePurchaseRedDot(PurchaseInfo purchase)
		{
			if (purchase != null && purchase.IsFreePurchase)
			{
				return purchase.IsAllowPurchase(UIBase.applicationState);
			}
			return false;
		}
		bool ShouldShowRedDot(SwitchTag tag)
		{
			if (tag.setupData?.appendData == null || !(tag.setupData.appendData is PurchaseGroupDef purchaseGroupDef))
			{
				return false;
			}
			if (purchaseGroupDef.groupType == PurchaseGroupType.RealMoneyBuy)
			{
				if (!UIBase.applicationState.playerInfoManager.showFreePurchase || realMoneyViewed)
				{
					return UIBase.applicationState.playerInfoManager.HasUnviewedDailyCardShop;
				}
				return true;
			}
			return false;
		}
		bool ShouldShowSubGroupRedDot(SwitchTag tag)
		{
			if (tag.setupData == null)
			{
				return false;
			}
			if (tag.setupData.i == 2 && UIBase.applicationState.playerInfoManager.HasUnviewedDailyCardShop)
			{
				return true;
			}
			if (!UIBase.applicationState.playerInfoManager.showFreePurchase)
			{
				return false;
			}
			if (viewedRealMoneySubGroups.Contains(tag.setupData.i))
			{
				return false;
			}
			return GetRealMoneySubGroupPurchases(tag.setupData.i).Any(ShouldShowFreePurchaseRedDot);
		}
	}

	private void ConfigureGroupMatterCellInfoPanel(MatterCell cell)
	{
		cell.SetInfoPanel(UIBase.fortressState?.uiManager?.universalCommonInfoPanel);
	}

	private CellUIPrefabPoolConfig<PurchaseGroupDefDisplay, PurchaseGroupDef> GetGroupPool(PurchaseGroupDef group)
	{
		return group.layoutType switch
		{
			PurchaseGroupLayoutType.SmallGrid => smallGridGroupPool, 
			PurchaseGroupLayoutType.HorizantalGrid => horizontalGridGroupPool, 
			PurchaseGroupLayoutType.SeasonPass => seasonGroupPool, 
			_ => itemSkinGroupPool, 
		};
	}

	public void OpenTypePurchaseGroup(PurchaseGroupType type)
	{
		PurchaseGroupType purchaseGroupType = type;
		PurchaseGroupDef purchaseGroupDef;
		if (purchaseGroupType != PurchaseGroupType.WeekShuffle)
		{
			if (purchaseGroupType != PurchaseGroupType.Stable || !(ScriptableObjectSingleton<CalendarDataManager>.instance.CurrentDailyStablePurchaseGroup != null))
			{
				goto IL_005a;
			}
			purchaseGroupDef = ScriptableObjectSingleton<CalendarDataManager>.instance.CurrentDailyStablePurchaseGroup;
		}
		else
		{
			if (!(ScriptableObjectSingleton<CalendarDataManager>.instance.CurrentWeeklyPurchaseGroup != null))
			{
				goto IL_005a;
			}
			purchaseGroupDef = ScriptableObjectSingleton<CalendarDataManager>.instance.CurrentWeeklyPurchaseGroup;
		}
		goto IL_007b;
		IL_005a:
		purchaseGroupDef = UIBase.applicationState.purchaseManager.purchaseGroups.FirstOrDefault((PurchaseGroupDef g) => g.groupType == type);
		goto IL_007b;
		IL_007b:
		PurchaseGroupDef purchaseGroupDef2 = purchaseGroupDef;
		OpenPurchaseGroup(purchaseGroupDef2);
	}

	public void OpenPurchaseGroup(PurchaseGroupDef group)
	{
		if (group != null)
		{
			int index = SortedGroups.IndexOf(group);
			switchPanel.Select(index);
			if (group.groupType == PurchaseGroupType.RealMoneyBuy)
			{
				switchPanel2.Select(0);
			}
		}
	}

	public void OpenTypePurchaseGroup(PurchaseGroupType type, int subIndex)
	{
		OpenTypePurchaseGroup(type);
		if (subIndex != -1)
		{
			switchPanel2.Select(subIndex);
		}
	}

	public void SetSwitchPanelIndex(int index, int subIndex = -1)
	{
		switchPanel.Select(index);
		if (subIndex != -1)
		{
			switchPanel2.Select(subIndex);
		}
	}

	private PurchaseGroupDef GetGroup()
	{
		if (switchPanel?.targetShowTag?.setupData == null)
		{
			return null;
		}
		PurchaseGroupDef purchaseGroupDef = SortedGroups[switchPanel.targetShowTag.setupData.i];
		if (purchaseGroupDef.groupType != PurchaseGroupType.RealMoneyBuy)
		{
			return purchaseGroupDef;
		}
		if (switchPanel2?.targetShowTag?.setupData == null)
		{
			return null;
		}
		if (switchPanel2.targetShowTag.setupData.i == 2)
		{
			return ScriptableObjectSingleton<CalendarDataManager>.instance.skinPurchaseGroupDef;
		}
		return purchaseGroupDef;
	}

	private string GetGroupDescription()
	{
		if (switchPanel?.targetShowTag?.setupData == null)
		{
			return "";
		}
		PurchaseGroupDef purchaseGroupDef = SortedGroups[switchPanel.targetShowTag.setupData.i];
		if (purchaseGroupDef.groupType != PurchaseGroupType.RealMoneyBuy)
		{
			return purchaseGroupDef.displayName;
		}
		string displayName = purchaseGroupDef.displayName;
		if (switchPanel2?.targetShowTag?.setupData == null)
		{
			return null;
		}
		return switchPanel2.targetShowTag.setupData.i switch
		{
			0 => displayName + "-深境之礼", 
			1 => displayName + "-周期之礼", 
			2 => displayName + "-卡牌形态", 
			_ => displayName + "-异界赠礼", 
		};
	}

	private void _OnBackButtonClicked()
	{
		if (realMoneyViewed)
		{
			UIBase.applicationState.playerInfoManager.showFreePurchase = false;
		}
		UIBase.applicationState.fortressState.uiManager.universalCommonInfoPanel.HideAll();
		this.HideUI();
		manager.SetDirty();
	}

	private void _OnMatterChange(MatterInfo mi)
	{
		SetDirty();
	}

	private void _OnTagSwitched()
	{
		CheckRealMoneyViewed();
		if (UIBase.fortressState.uiManager.universalCommonInfoPanel != null)
		{
			UIBase.fortressState.uiManager.universalCommonInfoPanel.HideAll();
		}
		PurchaseGroupDef purchaseGroupDef = GetGroup();
		if (!(purchaseGroupDef == null))
		{
			if (purchaseGroupDef.groupType == PurchaseGroupType.RealMoneyBuy)
			{
				switchPanel2.Select(0);
				MarkCurrentRealMoneySubGroupViewed();
			}
			OnTagSwitchedUpdate();
		}
	}

	private void _OnSecondTagSwitched()
	{
		MarkCurrentRealMoneySubGroupViewed();
		if (UIBase.fortressState.uiManager.universalCommonInfoPanel != null)
		{
			UIBase.fortressState.uiManager.universalCommonInfoPanel.HideAll();
		}
		OnTagSwitchedUpdate();
		MarkCurrentCardShopViewed();
	}

	private void OnTagSwitchedUpdate()
	{
		PurchaseGroupDef purchaseGroupDef = GetGroup();
		if (purchaseGroupDef == null)
		{
			return;
		}
		recycleObsessionButton.gameObject.SetActive(purchaseGroupDef.groupType == PurchaseGroupType.SeasonShuffle);
		smallGridGroupPool.DespawnAll();
		horizontalGridGroupPool.DespawnAll();
		itemSkinGroupPool.DespawnAll();
		seasonGroupPool.DespawnAll();
		GetGroupPool(purchaseGroupDef).Spawn(purchaseGroupDef);
		if (base.gameObject.activeInHierarchy)
		{
			ThinkingAnalyticsExtends.TrackOpenFortressSubPanel(UIBase.applicationState, "地下室-商店-子页面", GetGroupDescription());
		}
		if (purchaseGroupDef.groupType == PurchaseGroupType.RealMoneyBuy)
		{
			SwitchPanel obj = switchPanel2;
			if ((object)obj != null && obj.targetShowTag?.setupData?.i == 1)
			{
				Bootstrap.instance.adsManager.EnterAdScene("free_money");
			}
		}
		if (purchaseGroupDef.groupType != PurchaseGroupType.RealMoneyBuy && realMoneyViewed)
		{
			UIBase.applicationState.playerInfoManager.showFreePurchase = false;
		}
		groupMatterCellDisplayPool.DespawnAll();
		foreach (MatterDef previewMatter in purchaseGroupDef.previewMatters)
		{
			groupMatterCellDisplayPool.Spawn(UIBase.applicationState.matterManager.GetMatterInfo(previewMatter));
		}
		warningPanel.gameObject.SetActive(purchaseGroupDef.ShowWarning);
		if (purchaseGroupDef.groupType == PurchaseGroupType.RealMoneyBuy)
		{
			switchPanel2Area.SetActive(value: true);
		}
		else
		{
			switchPanel2Area.SetActive(value: false);
		}
		SetDirty();
	}

	private void OnRefreshPurchaseProfileData()
	{
		InitSwitchTag();
		OnTagSwitchedUpdate();
		SetDirty();
	}

	private void _OnOpenTestReturnRuleButtonClicked()
	{
		testReturnRulePanel.gameObject.SetActive(value: true);
	}

	private void _OnCloseTestReturnRuleButtonClicked()
	{
		testReturnRulePanel.gameObject.SetActive(value: false);
	}

	private void CheckRealMoneyViewed()
	{
		if (switchPanel.targetShowTag?.setupData?.appendData != null && switchPanel.targetShowTag?.setupData?.appendData is PurchaseGroupDef { groupType: PurchaseGroupType.RealMoneyBuy })
		{
			realMoneyViewed = true;
		}
	}

	private void MarkCurrentRealMoneySubGroupViewed()
	{
		if (switchPanel?.targetShowTag?.setupData?.appendData is PurchaseGroupDef { groupType: PurchaseGroupType.RealMoneyBuy } && switchPanel2?.targetShowTag?.setupData != null && viewedRealMoneySubGroups.Add(switchPanel2.targetShowTag.setupData.i))
		{
			switchPanel2.SetDirty();
		}
	}

	private void MarkCurrentCardShopViewed()
	{
		if (switchPanel?.targetShowTag?.setupData?.appendData is PurchaseGroupDef { groupType: PurchaseGroupType.RealMoneyBuy })
		{
			SwitchPanel obj = switchPanel2;
			if ((object)obj != null && obj.targetShowTag?.setupData?.i == 2)
			{
				UIBase.applicationState.playerInfoManager.MarkDailyCardShopViewed();
			}
		}
	}

	public IEnumerator ShowAnim()
	{
		yield break;
	}
}
