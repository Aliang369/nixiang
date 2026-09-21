using System;
using System.Collections;
using System.Collections.Generic;
using ChillyRoom.Services.ContainerOfSoul;
using Common.UI;
using ContainerOfSoul.Display.Fortress.IndexDisplay;
using TMPro;
using UnityEngine;

public class ContractRankingPanel : CellUI<ContractDef>, IShowAsync, IHideAsync
{
	private class RankingDataWithStatus
	{
		public int TotalPages;

		public int CachedPages;

		public List<RankingManager.RankingData> Datas;
	}

	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private TextMeshProUGUI pageText;

	[SerializeField]
	public ReocrdInfoPanel recordInfoPanel;

	[SerializeField]
	private RankingRecordCell rankingRecordCellPrefab;

	[SerializeField]
	public RankingRecordCell currentBestRecordCell;

	[SerializeField]
	private Transform rankingRecordCellParent;

	[SerializeField]
	private GameObject rankingRecordListNotDataHint;

	[SerializeField]
	private GameObject currentBestRecordNotDataHint;

	[SerializeField]
	private GameButton backButton;

	[SerializeField]
	private GameButton previousPageButton;

	[SerializeField]
	private GameButton nextPageButton;

	[SerializeField]
	private Animator animator;

	[NonSerialized]
	private CellUIPrefabPool<RankingRecordCell, RankingManager.RankingData> rankingRecordCellPool;

	[NonSerialized]
	private List<RankingManager.RankingData> rankingDatas;

	[NonSerialized]
	private RankingManager.RankingData localBestRecord;

	[NonSerialized]
	public int CurrentPage = 1;

	[NonSerialized]
	public int RankCountPerPage = 10;

	private bool prevPage;

	private Dictionary<ContractDef, RankingDataWithStatus> cachedRankingDatas = new Dictionary<ContractDef, RankingDataWithStatus>();

	private ContractDef contractDef => base.setupData;

	protected override void Init()
	{
		InitUI(recordInfoPanel);
		InitUI(currentBestRecordCell);
		GetPrefabPool(ref rankingRecordCellPool, rankingRecordCellPrefab);
		previousPageButton.onClick.AddListener(PreviousPage);
		nextPageButton.onClick.AddListener(NextPage);
		backButton.onClick.AddListener(base.Unset);
		void NextPage()
		{
			if (HasNextPage())
			{
				CurrentPage++;
				prevPage = false;
				RequestRankingData();
			}
		}
		void PreviousPage()
		{
			if (CurrentPage != 1)
			{
				CurrentPage--;
				prevPage = true;
				RequestRankingData();
			}
		}
	}

	protected override void OnSetup(ContractDef _)
	{
		rankingDatas = null;
		localBestRecord = null;
		CurrentPage = 1;
		prevPage = false;
		RequestRankingData();
	}

	protected override void OnUnset()
	{
		rankingDatas = null;
		base.gameObject.SetActive(value: false);
	}

	protected override void UpdateDisplay()
	{
		titleText.text = contractDef.Name;
		pageText.text = $"{CurrentPage}";
		previousPageButton.gameObject.SetActive(CurrentPage != 1);
		nextPageButton.gameObject.SetActive(HasNextPage());
		rankingRecordCellPool.Sync(rankingDatas);
		rankingRecordListNotDataHint.gameObject.SetActive(rankingDatas.IsNullOrEmpty());
		if (localBestRecord != null)
		{
			currentBestRecordCell.Setup(localBestRecord);
			currentBestRecordCell.SetActiveAndDirty(active: true);
		}
		else
		{
			currentBestRecordCell.SetActiveAndDirty(active: false);
		}
		currentBestRecordNotDataHint.gameObject.SetActive(localBestRecord == null);
	}

	private bool HasNextPage()
	{
		if (CurrentPage < ScriptableObjectSingleton<FortressDataManager>.instance.rankMaxPages)
		{
			if (cachedRankingDatas.TryGetValue(contractDef, out var value))
			{
				return value.TotalPages > CurrentPage;
			}
			return false;
		}
		return false;
	}

	IEnumerator IShowAsync.ShowAnim()
	{
		base.gameObject.SetActive(value: true);
		((Behaviour)(object)animator).enabled = true;
		animator.Play(AnimKeywords.show);
		yield return new WaitUntil(() => !((Behaviour)(object)animator).enabled || !base.gameObject.activeInHierarchy);
	}

	IEnumerator IHideAsync.HideAnim()
	{
		((Behaviour)(object)animator).enabled = true;
		animator.Play(AnimKeywords.hide);
		yield return new WaitUntil(() => !((Behaviour)(object)animator).enabled || !base.gameObject.activeInHierarchy);
		base.gameObject.SetActive(value: false);
	}

	public void ClearCachedRankingData()
	{
		cachedRankingDatas.Clear();
	}

	private void RequestRankingData()
	{
		if (!GlobalConfigManager.instance.enableNetwork)
		{
			return;
		}
		if (cachedRankingDatas.TryGetValue(contractDef, out var value))
		{
			if (value.CachedPages == 0)
			{
				return;
			}
			int num = (CurrentPage - 1) * RankCountPerPage;
			if (value.CachedPages >= CurrentPage)
			{
				if (prevPage)
				{
					rankingDatas = value.Datas.GetRange(num, RankCountPerPage);
				}
				else
				{
					rankingDatas = value.Datas.GetRange(num, Math.Min(RankCountPerPage, value.Datas.Count - num));
				}
				SetDirty();
				return;
			}
		}
		GeneralGameAsyncOperation<FetchRankInfoRequest, FetchRankInfoResult> op = new GeneralGameAsyncOperation<FetchRankInfoRequest, FetchRankInfoResult>(new FetchRankInfoRequest(contractDef, CurrentPage), CellUI<ContractDef>.applicationState.ChillyRoomSDK.Rank.FetchRankInfoAsync);
		op.OnComplete(delegate
		{
			OnComplete(op.OpResult.GetRankingDatas(CellUI<ContractDef>.applicationState), op.OpResult.GetPlayerBestRankingData(CellUI<ContractDef>.applicationState));
			SetDirty();
		}).EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_loading_ranking);
		void OnComplete(List<RankingManager.RankingData> rankData, RankingManager.RankingData localBest)
		{
			rankingDatas = rankData;
			if (localBest != null)
			{
				localBestRecord = localBest;
			}
			if (GlobalConfigManager.instance.enableNetwork)
			{
				if (rankingDatas == null)
				{
					SetDirty();
					if (!cachedRankingDatas.TryGetValue(contractDef, out var _))
					{
						cachedRankingDatas.TryAdd(contractDef, new RankingDataWithStatus
						{
							TotalPages = 0,
							CachedPages = 0,
							Datas = null
						});
					}
					return;
				}
				bool flag = rankingDatas.Count > RankCountPerPage;
				rankingDatas = rankingDatas?.GetRange(0, Mathf.Min(RankCountPerPage, rankingDatas.Count));
				if (cachedRankingDatas.TryGetValue(contractDef, out var value3))
				{
					if (flag)
					{
						value3.TotalPages++;
					}
					value3.CachedPages++;
					value3.Datas.AddRange(rankingDatas);
				}
				else
				{
					RankingDataWithStatus rankingDataWithStatus = new RankingDataWithStatus
					{
						TotalPages = 1,
						CachedPages = 1,
						Datas = new List<RankingManager.RankingData>(rankingDatas)
					};
					if (flag)
					{
						rankingDataWithStatus.TotalPages++;
					}
					cachedRankingDatas.TryAdd(contractDef, rankingDataWithStatus);
				}
			}
			SetDirty();
		}
	}
}
