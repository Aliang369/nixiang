using System.Collections.Generic;

public class PurchaseGroupDef : CTScriptableObject
{
	[CTSOAssetName]
	public string GroupName;

	[LocalizeText]
	public string displayName;

	public PurchaseGroupType groupType;

	public PurchaseGroupLayoutType layoutType;

	public ResetTimeType resetTimeType;

	public int layoutSortPriority;

	public List<MatterDef> previewMatters;

	public bool showCellRepeatTime;

	public List<PurchaseInfo> purchases;

	public SeasonDef seasonDef;

	public UnityDateTime? manualStartTime;

	public UnityDateTime? manualEndTime;

	public override string CTAssetIDPrepend => "PURCHASEGROUP";

	public bool ShowWarning
	{
		get
		{
			if (groupType != PurchaseGroupType.RealMoneyBuy)
			{
				return groupType == PurchaseGroupType.SeasonPass;
			}
			return true;
		}
	}

	private bool IsSeasonPurchaseGroup
	{
		get
		{
			if (groupType != PurchaseGroupType.SeasonShuffle)
			{
				return groupType == PurchaseGroupType.SeasonPass;
			}
			return true;
		}
	}

	public bool withinValidTime
	{
		get
		{
			switch (groupType)
			{
			case PurchaseGroupType.SeasonPass:
			case PurchaseGroupType.SeasonShuffle:
				if (seasonDef != null)
				{
					return Bootstrap.applicationState?.seasonManager?.currentSeason?.def == seasonDef;
				}
				return false;
			default:
				return true;
			}
		}
	}

	public bool ShowInMainPurchasePanel
	{
		get
		{
			if (layoutType == PurchaseGroupLayoutType.ItemSkin)
			{
				return false;
			}
			if (groupType != PurchaseGroupType.DailyShuffle && groupType != PurchaseGroupType.SeasonPass && groupType != PurchaseGroupType.PresetBuyPack && groupType != PurchaseGroupType.Stable && groupType != PurchaseGroupType.RealMoneyBuy && groupType != PurchaseGroupType.WeekShuffle)
			{
				return groupType == PurchaseGroupType.SeasonShuffle;
			}
			return true;
		}
	}
}
