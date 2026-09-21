using System;
using Common.UI;
using UnityEngine;

public class BftUIDisplay : CellUI<BattleFieldTransform>, IDisplayModulesController
{
	public abstract class DisplayModule : UIDisplayModule
	{
		protected BftUIDisplay bftDisplay => parent as BftUIDisplay;
	}

	[SerializeField]
	public DialoguePlayer dialoguePlayer;

	[SerializeField]
	private RectTransform _heartPivot;

	[NonSerialized]
	public BattleFieldTransform bft;

	[NonSerialized]
	public ConvertablePivot heartPivot;

	protected override void Init()
	{
		CellUI<BattleFieldTransform>.applicationState.runningGameState.actionManager.actionFrameFinishEvent += _OnActionFrameFinish;
		heartPivot = new ConvertablePivot(_heartPivot);
	}

	protected override void OnSetup(BattleFieldTransform dailyContract)
	{
		bft = dailyContract;
	}

	protected override void Dispose()
	{
		Unset();
		CellUI<BattleFieldTransform>.gameState.actionManager.actionFrameFinishEvent -= _OnActionFrameFinish;
	}

	private void _OnActionFrameFinish()
	{
		SetDirty();
	}
}
