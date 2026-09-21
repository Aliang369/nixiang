using UnityEngine;

public class DisplayPersistentNFXAction : GameAction
{
	private readonly NFXSetup setup;

	private readonly PivotManager.IWithPivot targetPivot;

	private readonly float duration;

	private readonly BattleFieldTransform ownerBft;

	public DisplayPersistentNFXAction(NFXSetup setup, PivotManager.IWithPivot targetPivot, float duration, BattleFieldTransform ownerBft = null)
	{
		this.setup = setup;
		this.targetPivot = targetPivot;
		this.duration = duration;
		this.ownerBft = ownerBft;
	}

	protected override void Update()
	{
		if (setup == null || duration <= 0f)
		{
			FinishAction();
			return;
		}
		if (base.actionManager.skipFrameAnim)
		{
			FinishAction();
			return;
		}
		ConvertablePivot pivot = targetPivot?.GetPivot(gameState);
		if (pivot == null)
		{
			FinishAction();
			return;
		}
		Transform transform = pivot.GetVFXRoot(gameState);
		if (transform == null)
		{
			transform = gameState.UIManager.uiVFXRoot;
		}
		NFXPlayState nFXPlayState = gameState.applicationState.nfxManager.SpawnAndPlay(setup, transform, () => GetPivotPosition(pivot), destroyOnStop: false);
		nFXPlayState.KeepAliveFor(duration);
		if (ownerBft != null)
		{
			BlockyDelicacyPersistentFlyVfx.Attach(nFXPlayState, ownerBft);
		}
		FinishAction();
	}

	private Vector3? GetPivotPosition(ConvertablePivot pivot)
	{
		if (pivot.transform == null || !pivot.transform.gameObject.activeInHierarchy)
		{
			return null;
		}
		return pivot.WorldPosition;
	}
}
