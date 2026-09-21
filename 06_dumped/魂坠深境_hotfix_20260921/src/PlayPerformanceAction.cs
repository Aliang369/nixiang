using System;

public class PlayPerformanceAction : GameAction
{
	public EnemyAnimationType animationType;

	public Action effectAction;

	public DisplayUnit.EnemyPerformanceState epfs;

	public Item sourceItem;

	public BattleFieldTransform target;

	public bool waitForCompletion;

	public PlayPerformanceAction(Item sourceItem, BattleFieldTransform target, EnemyAnimationType animationType, bool waitForCompletion = false, Action effectAction = null)
	{
		this.sourceItem = sourceItem;
		this.target = target;
		this.animationType = animationType;
		this.waitForCompletion = waitForCompletion;
		this.effectAction = effectAction;
	}

	protected override void Update()
	{
		BattleFieldTransformDisplay battleFieldTransformDisplay = target?.attachDisplay;
		if (battleFieldTransformDisplay == null)
		{
			FinishAction();
			return;
		}
		if (epfs == null)
		{
			epfs = new DisplayUnit.EnemyPerformanceState();
			epfs.animationType = animationType;
			epfs.sourceItem = sourceItem;
			epfs.effectAction = effectAction;
			battleFieldTransformDisplay.PlayPerformance(epfs);
		}
		if (!waitForCompletion)
		{
			FinishAction();
		}
		else if (epfs.isFinished || !battleFieldTransformDisplay.HasAnimationPlaying())
		{
			FinishAction();
		}
	}
}
