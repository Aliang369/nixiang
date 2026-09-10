using System;
using System.Collections.Generic;
using System.Linq;
using ChillyRoom.Services.ContainerOfSoul;
using Common.UI;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;

public class RecycleObsessionPanel : UIBase
{
	private const int ObsessionDebrisRecycleDailyLimit = 100;

	[SerializeField]
	private GlobalItemDisplay prefab;

	[SerializeField]
	private GameButton backButton;

	[SerializeField]
	private GameButton confirmButton;

	[SerializeField]
	private TMP_Text dailyRecycleLimitText;

	[SerializeField]
	private Sprite selectionMarkSprite;

	[NonSerialized]
	private CellUIPrefabPool<GlobalItemDisplay, IGlobalItemDisplaySetup> pool;

	[NonSerialized]
	private List<IGlobalItemDisplaySetup> recycleMatters;

	[NonSerialized]
	private HashSet<MatterInfo> selectedRecycleMatters;

	[NonSerialized]
	private int recycleDailyLimit = 100;

	[NonSerialized]
	private int networkRemainingRecycleCount = 100;

	protected override void Init()
	{
		GetPrefabPool(ref pool, prefab);
		backButton.onClick.AddListener(delegate
		{
			base.gameObject.SetActive(value: false);
		});
		confirmButton.onClick.AddListener(delegate
		{
			RecycleObsession(GetSelectedRecycleMatters().ConvertAll((Converter<MatterInfo, IGlobalItemDisplaySetup>)((MatterInfo m) => m)));
			base.gameObject.SetActive(value: false);
		});
		static List<MatterInfo> MattersFromServer(IEnumerable<ChillyRoom.Services.ContainerOfSoul.MatterInfo> matters)
		{
			List<MatterInfo> list = new List<MatterInfo>();
			if (matters == null)
			{
				return list;
			}
			foreach (ChillyRoom.Services.ContainerOfSoul.MatterInfo matter in matters)
			{
				MatterDef matterDef = UIBase.applicationState.assetCatalog.FullIdToCTAsset<MatterDef>(matter.MatterId);
				if (matterDef != null && matter.Amount > 0)
				{
					list.Add(new MatterInfo(matterDef, matter.Amount));
				}
			}
			return list;
		}
		void OnComplete(List<MatterInfo> gainMatters, List<MatterInfo> consumedMatters, int dailyRecycledCount)
		{
			TraceLog.TryDebug(() => "[RecycleObsessionPanel] RecycleObsession.OnComplete(" + string.Join(",", gainMatters.Select((MatterInfo m) => $"{m.matterDef?.matterName}x{m.count}")) + ")");
			foreach (MatterInfo gainMatter in gainMatters)
			{
				UIBase.applicationState.matterManager.AddMatter(gainMatter, "回收执念");
			}
			foreach (MatterInfo consumedMatter in consumedMatters)
			{
				UIBase.applicationState.matterManager.ConsumeMatter(consumedMatter, "回收执念");
			}
			UIBase.applicationState.playerInfoManager.dailyObsessionDebrisRecycleCount = Mathf.Clamp(dailyRecycledCount, 0, GetRecycleDailyLimit());
			UIBase.applicationState.savingManager.SetProfileDirtyToSave();
			UpdateRecycleSummaryText();
			ThinkingAnalyticsExtends.TrackFortressRecycleObsession(UIBase.applicationState, consumedMatters);
			if (UIBase.applicationState.fortressState != null && gainMatters.Count > 0)
			{
				List<GlobalRewardInfo> rewards = gainMatters.Select((MatterInfo m) => new GlobalRewardInfo
				{
					matter = m
				}).ToList();
				UIBase.fortressState.uiManager.globalRewardPackageDisplay.Setup(new GlobalRewardPackage(rewards));
				UIBase.applicationState.fortressState.uiManager.SetDirty();
			}
		}
		void RecycleObsession(List<IGlobalItemDisplaySetup> obsessions)
		{
			if (GlobalConfigManager.instance.enableNetwork)
			{
				List<ChillyRoom.Services.ContainerOfSoul.MatterInfo> matterInfos = obsessions.Where((IGlobalItemDisplaySetup s) => s is MatterInfo).Convert((object s) => (s as MatterInfo).ToServer()).ToList();
				GeneralGameAsyncOperation<RecycleObsessionRequest, RecycleObsessionResult> req = new GeneralGameAsyncOperation<RecycleObsessionRequest, RecycleObsessionResult>(new RecycleObsessionRequest
				{
					MatterInfos = matterInfos
				}, UIBase.applicationState.ChillyRoomSDK.Character.RecycleObsessionAsync);
				req.OnComplete(delegate
				{
					RecycleObsessionResult opResult = req.OpResult;
					if (opResult?.Quota != null)
					{
						ApplyServerQuota(opResult.Quota);
					}
					OnComplete(MattersFromServer(opResult?.GainMatters), MattersFromServer(opResult?.ConsumedMatters), UIBase.applicationState.playerInfoManager.dailyObsessionDebrisRecycleCount);
				}).OnFault(delegate
				{
					if (!(req.errorCode != 104011.ToString()))
					{
						networkRemainingRecycleCount = 0;
						UIBase.applicationState.playerInfoManager.dailyObsessionDebrisRecycleCount = recycleDailyLimit;
						UIBase.applicationState.savingManager.SetProfileDirtyToSave();
					}
				}).EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_recycling_obsession);
			}
		}
	}

	public void Show()
	{
		if (GlobalConfigManager.instance.enableNetwork)
		{
			GeneralGameAsyncOperationResultOnly<ObsessionDebrisRecycleQuota> request = new GeneralGameAsyncOperationResultOnly<ObsessionDebrisRecycleQuota>(UIBase.applicationState.ChillyRoomSDK.Character.GetObsessionDebrisRecycleQuotaAsync);
			request.OnComplete(delegate
			{
				ApplyServerQuota(request.OpResult);
				ShowPanel();
			}).EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_recycling_obsession);
		}
		else
		{
			ShowPanel();
		}
	}

	private void ShowPanel()
	{
		base.gameObject.SetActive(value: true);
		ResetLocalDailyCountIfNeeded();
		recycleMatters = new List<IGlobalItemDisplaySetup>();
		CollectRecycleMatters();
		pool.Sync(recycleMatters);
		selectedRecycleMatters = recycleMatters.OfType<MatterInfo>().ToHashSet();
		foreach (GlobalItemDisplay activeElement in pool.GetActiveElements())
		{
			(activeElement.GetComponent<RecycleObsessionSelectionDisplay>() ?? activeElement.gameObject.AddComponent<RecycleObsessionSelectionDisplay>()).Configure(ToggleSelection, selectedRecycleMatters.Contains(activeElement.setupData as MatterInfo), selectionMarkSprite);
		}
		UpdateRecycleSummaryText();
		ForceUpdate();
		void CollectRecycleMatters()
		{
			foreach (PlayerCharacterState characterState in UIBase.applicationState.characterManager.characterStates)
			{
				MatterInfo item = characterState.GetRemainObsession().Item2;
				if (item != null && item.count > 0)
				{
					recycleMatters.Add(item);
				}
			}
		}
		void ToggleSelection(GlobalItemDisplay itemDisplay)
		{
			if (itemDisplay.setupData is MatterInfo item)
			{
				if (!selectedRecycleMatters.Add(item))
				{
					selectedRecycleMatters.Remove(item);
				}
				itemDisplay.GetComponent<RecycleObsessionSelectionDisplay>().SetSelected(selectedRecycleMatters.Contains(item));
				UpdateRecycleSummaryText();
				SetDirty();
			}
		}
	}

	private int GetObsessionDebrisRecycleRemainCount()
	{
		if (GlobalConfigManager.instance.enableNetwork)
		{
			return Mathf.Clamp(networkRemainingRecycleCount, 0, GetRecycleDailyLimit());
		}
		return Mathf.Max(0, 100 - UIBase.applicationState.playerInfoManager.dailyObsessionDebrisRecycleCount);
	}

	private int GetRecycleDailyLimit()
	{
		if (!GlobalConfigManager.instance.enableNetwork)
		{
			return 100;
		}
		return Mathf.Max(0, recycleDailyLimit);
	}

	private List<MatterInfo> GetSelectedRecycleMatters()
	{
		if (selectedRecycleMatters == null)
		{
			return new List<MatterInfo>();
		}
		return recycleMatters.OfType<MatterInfo>().Where(selectedRecycleMatters.Contains).ToList();
	}

	private int GetSelectedRecycleCount()
	{
		return GetSelectedRecycleMatters().Sum((MatterInfo m) => m.count);
	}

	private void UpdateRecycleSummaryText()
	{
		int obsessionDebrisRecycleRemainCount = GetObsessionDebrisRecycleRemainCount();
		int num = Mathf.Min(GetSelectedRecycleCount(), obsessionDebrisRecycleRemainCount);
		dailyRecycleLimitText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_recycle_obsession_debris_summary_arg0_remain_arg1_limit_arg2.FormatWith(num, obsessionDebrisRecycleRemainCount, GetRecycleDailyLimit());
	}

	private void ApplyServerQuota(ObsessionDebrisRecycleQuota quota)
	{
		recycleDailyLimit = Mathf.Max(0, quota?.DailyLimit ?? 0);
		networkRemainingRecycleCount = Mathf.Clamp(quota?.RemainingCount ?? 0, 0, recycleDailyLimit);
		UIBase.applicationState.playerInfoManager.dailyObsessionDebrisRecycleCount = Mathf.Clamp(quota?.RecycledCount ?? 0, 0, recycleDailyLimit);
		UIBase.applicationState.savingManager.SetProfileDirtyToSave();
	}

	private void ResetLocalDailyCountIfNeeded()
	{
		if (!GlobalConfigManager.instance.enableNetwork)
		{
			recycleDailyLimit = 100;
			int currentDailyPeriod = ScriptableObjectSingleton<CalendarDataManager>.instance.CurrentDailyPeriod;
			if (UIBase.applicationState.playerInfoManager.localObsessionDebrisRecycleDailyPeriod != currentDailyPeriod)
			{
				UIBase.applicationState.playerInfoManager.localObsessionDebrisRecycleDailyPeriod = currentDailyPeriod;
				UIBase.applicationState.playerInfoManager.dailyObsessionDebrisRecycleCount = 0;
				UIBase.applicationState.savingManager.SetProfileDirtyToSave();
			}
		}
	}

	protected override void UpdateDisplay()
	{
		confirmButton.interactable = GetObsessionDebrisRecycleRemainCount() > 0 && GetSelectedRecycleCount() > 0;
	}
}
