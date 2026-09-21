using System;
using System.Collections.Generic;
using System.Linq;
using Common.UI;
using UnityEngine;
using UnityEngine.UI;

public class IdolLevelArea : CellUI<IdolLevelArea.Data>
{
	public class Data
	{
		public IdolState idolState;

		public int level;
	}

	[SerializeField]
	private CellUIPrefabPoolConfig<GlobalItemDisplay, IGlobalItemDisplaySetup> rewardPool;

	[SerializeField]
	private CellUIPrefabPoolConfig<IdolLevelUnlockCellIcon, int> unlockCellIconPool;

	[SerializeField]
	private Image unlockedMask;

	[SerializeField]
	private GameObject classRewardBackground;

	[SerializeField]
	private GameObject globalRewardBackground;

	protected override void Init()
	{
		Init(rewardPool);
		Init(unlockCellIconPool);
	}

	protected override void OnSetup(Data setupData)
	{
		rewardPool?.Sync(GetRewards());
	}

	protected override void UpdateDisplay()
	{
		bool flag = base.setupData?.idolState != null && base.setupData.level <= base.setupData.idolState.currentLevel;
		unlockedMask.gameObject.SetActive(flag);
		unlockCellIconPool?.Sync(flag ? GetUnlockCellIconIndexes() : new List<int>());
		UpdateBackground();
	}

	private List<IGlobalItemDisplaySetup> GetRewards()
	{
		return (base.setupData?.idolState?.idolDef?.GetLevel(base.setupData.level)?.reward)?.Select((Func<GlobalRewardSetup, IGlobalItemDisplaySetup>)((GlobalRewardSetup r) => r)).ToList();
	}

	private List<int> GetUnlockCellIconIndexes()
	{
		int valueOrDefault = (base.setupData?.idolState?.idolDef?.GetLevel(base.setupData.level)?.reward?.Count).GetValueOrDefault();
		return Enumerable.Range(0, valueOrDefault).ToList();
	}

	private void UpdateBackground()
	{
		bool flag = base.setupData.idolState.idolDef.GetLevel(base.setupData.level).reward?.Any(IdolLevelRewardUtility.IsClassReward) ?? false;
		classRewardBackground.SetActive(flag);
		globalRewardBackground.SetActive(!flag);
	}
}
