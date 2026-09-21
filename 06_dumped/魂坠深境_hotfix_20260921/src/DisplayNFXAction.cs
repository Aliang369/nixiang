using System;
using UnityEngine;

public class DisplayNFXAction : GameAction
{
	private Vector3 vfxPosition;

	private Transform vfxParent;

	private NFXSetup setup;

	private bool waitForEnd;

	private Func<Vector3?> positionUpdater;

	private NFXPlayState nfxState;

	private Func<bool> preparerAction;

	public DisplayNFXAction(NFXSetup setup, Vector3 vfxPosition, Transform vfxParent, bool waitForEnd = false)
	{
		this.setup = setup;
		this.vfxPosition = vfxPosition;
		this.vfxParent = vfxParent;
		this.waitForEnd = waitForEnd;
	}

	public DisplayNFXAction(NFXSetup setup, PivotManager.IWithPivot targetPivot, bool waitForEnd = false)
	{
		DisplayNFXAction displayNFXAction = this;
		this.setup = setup;
		this.waitForEnd = waitForEnd;
		preparerAction = delegate
		{
			ConvertablePivot pivot = targetPivot?.GetPivot(displayNFXAction.gameState);
			if (pivot == null)
			{
				return false;
			}
			displayNFXAction.vfxParent = pivot.GetVFXRoot(displayNFXAction.gameState);
			displayNFXAction.positionUpdater = () => (!(pivot.transform != null) || !pivot.transform.gameObject.activeInHierarchy) ? ((Vector3?)null) : new Vector3?(pivot.WorldPosition);
			return true;
		};
	}

	protected override void Update()
	{
		if (setup == null)
		{
			FinishAction();
			return;
		}
		if (base.actionManager.skipFrameAnim)
		{
			FinishAction();
			return;
		}
		if (nfxState == null)
		{
			if (vfxParent == null)
			{
				vfxParent = gameState.UIManager.uiVFXRoot;
			}
			if (preparerAction != null && !preparerAction())
			{
				FinishAction();
				return;
			}
			if (positionUpdater != null)
			{
				nfxState = gameState.applicationState.nfxManager.SpawnAndPlay(setup, vfxParent, positionUpdater);
			}
			else
			{
				nfxState = gameState.applicationState.nfxManager.SpawnAndPlay(setup, vfxParent, vfxPosition);
			}
		}
		if (!waitForEnd)
		{
			FinishAction();
		}
		else if (nfxState.IsCompleted())
		{
			FinishAction();
		}
	}
}
