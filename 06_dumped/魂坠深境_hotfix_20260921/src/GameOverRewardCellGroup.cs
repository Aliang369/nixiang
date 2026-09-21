using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common.UI;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class GameOverRewardCellGroup : CellUI<(string, List<GlobalRewardInfo>, bool)>
{
	public float interval = 0.2f;

	public CanvasGroup canvasGroup;

	public TextMeshProUGUI titleText;

	public GlobalItemDisplay rewardCellPrefab;

	public TextMeshProUGUI textPrefab;

	private CellUIPrefabPool<GlobalItemDisplay, IGlobalItemDisplaySetup> pool;

	private PrefabPool<TextMeshProUGUI> textPool;

	[NonSerialized]
	public Coroutine displayingCor;

	private (string title, List<GlobalRewardInfo> rewards, bool isText) data => base.setupData;

	protected override void Init()
	{
		pool = GetPrefabPool<GlobalItemDisplay, IGlobalItemDisplaySetup>(rewardCellPrefab);
		pool.ConfigInfoPanel(CellUI<(string, List<GlobalRewardInfo>, bool)>.gameState.UIManager.universalCommonInfoPanel);
		textPool = new PrefabPool<TextMeshProUGUI>(textPrefab);
	}

	private IEnumerator DisplayCor()
	{
		pool.DespawnAll();
		textPool.DespawnAll();
		titleText.text = data.title;
		canvasGroup.alpha = 0f;
		if (data.isText)
		{
			foreach (GlobalRewardInfo item in GlobalRewardInfo.UnfoldNestedReward(data.rewards.OrEmptyIfNull().ToList()))
			{
				textPool.Spawn().text = item.GenerateUnlockCTSODescription();
			}
		}
		else
		{
			foreach (GlobalRewardInfo item2 in GlobalRewardInfo.UnfoldNestedReward(data.rewards.OrEmptyIfNull().ToList()))
			{
				pool.Spawn(item2);
			}
		}
		yield return DOTweenModuleUI.DOFade(canvasGroup, 1f, interval).WaitForCompletion();
		displayingCor = null;
	}

	protected override void OnSetup((string, List<GlobalRewardInfo>, bool) _)
	{
		SetActiveAndDirty(active: true);
		if (displayingCor == null)
		{
			displayingCor = StartCoroutine(DisplayCor());
		}
	}
}
