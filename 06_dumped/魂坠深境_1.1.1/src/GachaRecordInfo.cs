using System;
using System.Collections.Generic;
using System.Linq;
using Common.UI;
using TMPro;
using UnityEngine;

public class GachaRecordInfo : UIBase
{
	[SerializeField]
	private GachaRecordInfoCell prefab;

	[SerializeField]
	private GameButton previousButton;

	[SerializeField]
	private TextMeshProUGUI indexText;

	[SerializeField]
	private GameButton nextButton;

	[SerializeField]
	private GameButton closeButton;

	[NonSerialized]
	private CellUIPrefabPool<GachaRecordInfoCell, GachaHistoryInfo> pool;

	[NonSerialized]
	private int _index;

	[NonSerialized]
	private List<GachaHistoryInfo> displayInfos = new List<GachaHistoryInfo>();

	[NonSerialized]
	private bool requestInFlight;

	private int index
	{
		get
		{
			return Math.Clamp(_index, 0, displayInfos.Count / 10);
		}
		set
		{
			_index = Math.Clamp(value, 0, displayInfos.Count / 10);
			SetDirty();
		}
	}

	public GachaPoolState Pool => (parent as GachaPanel).targetState;

	protected override void Init()
	{
		pool = GetPrefabPool<GachaRecordInfoCell, GachaHistoryInfo>(prefab);
		previousButton.onClick.AddListener(delegate
		{
			index--;
		});
		nextButton.onClick.AddListener(delegate
		{
			index++;
		});
		closeButton.onClick.AddListener(delegate
		{
			base.gameObject.SetActive(value: false);
		});
		SetDirty();
	}

	public void Show()
	{
		base.gameObject.SetActive(value: true);
		_index = 0;
		RefreshDisplayInfosFromLocal();
		if (NeedRequestHistory())
		{
			RequestDisplayHistory();
		}
		SetDirty();
	}

	protected override void UpdateDisplay()
	{
		indexText.text = (index + 1).ToString();
		previousButton.interactable = index > 0;
		nextButton.interactable = index < (displayInfos.Count - 1) / 10;
		List<GachaHistoryInfo> list = displayInfos.OrderByDescending((GachaHistoryInfo info) => info.date).ToList();
		pool.Sync(list.GetRange(index * 10, Math.Min(list.Count - index * 10, 10)));
	}

	private List<GachaPoolState> GetDisplayPools()
	{
		List<GachaPoolDef> sharedGroup = UIBase.applicationState.gachaManager.GetHistorySharedGroup(Pool.def);
		if (sharedGroup.Count == 0)
		{
			return new List<GachaPoolState> { Pool };
		}
		return UIBase.applicationState.gachaManager.gachaPoolStates.Where((GachaPoolState state) => state?.def != null && sharedGroup.Contains(state.def)).ToList();
	}

	private void RefreshDisplayInfosFromLocal()
	{
		displayInfos = (from info in GetDisplayPools().SelectMany((GachaPoolState state) => (state.gachaDrawHistoryInfos ?? new List<GachaHistoryInfo>()).Select(delegate(GachaHistoryInfo info)
			{
				if (info != null && info.gachaPoolDef == null)
				{
					info.gachaPoolDef = state.def;
				}
				return info;
			}))
			where info != null
			orderby info.date
			select info).ToList();
	}

	private bool NeedRequestHistory()
	{
		if (GlobalConfigManager.instance.enableNetwork && !requestInFlight)
		{
			return GetDisplayPools().Any((GachaPoolState state) => !state.gachaHistoryReady);
		}
		return false;
	}

	private void RequestDisplayHistory()
	{
		GeneralGameAsyncOperationResultOnly<GachaManager.GachaHistoryDisplaySnapshot> op = new GeneralGameAsyncOperationResultOnly<GachaManager.GachaHistoryDisplaySnapshot>(() => UIBase.applicationState.gachaManager.FetchHistoryDisplaySnapshotAsync(Pool.def));
		requestInFlight = true;
		op.OnComplete(delegate
		{
			if (!UIBase.applicationState.gachaManager.ApplyHistoryDisplaySnapshot(op.OpResult))
			{
				TraceLog.Error("抽卡历史展示同步结果无效: pool=" + Pool.def.FullID);
			}
			RefreshDisplayInfosFromLocal();
			SetDirty();
		}).OnOver(delegate
		{
			requestInFlight = false;
			SetDirty();
		}).EnqueueRun(GameAsyncOperationLane.Foreground, ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_syncing_gacha_history);
	}
}
