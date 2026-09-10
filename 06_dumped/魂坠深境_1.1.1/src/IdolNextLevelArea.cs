using System.Linq;
using Common.UI;
using UnityEngine;

public class IdolNextLevelArea : CellUI<IdolNextLevelArea.Data>
{
	public class Data
	{
		public IdolState idolState;

		public int level;
	}

	[SerializeField]
	private CellUIPrefabPoolConfig<IdolLevelRewardCell, IdolLevelRewardCell.Data> rewardPool;

	protected override void Init()
	{
		Init(rewardPool);
	}

	protected override void OnSetup(Data setupData)
	{
		SetDirty();
	}

	protected override void UpdateDisplay()
	{
		rewardPool?.Sync(base.setupData?.idolState?.idolDef?.GetLevel(base.setupData.level)?.reward?.Select((GlobalRewardSetup reward) => new IdolLevelRewardCell.Data
		{
			idolState = base.setupData.idolState,
			reward = reward
		}).ToList());
	}
}
