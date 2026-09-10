using System;
using Coffee.UIExtensions;
using UnityEngine;

public class PlayCollectVFXAction : GameAction
{
	private readonly NFXSetup nfx;

	private readonly ConvertablePivot endPivot;

	private readonly Action<VFXObject> onConfig;

	private bool started;

	private readonly ConvertablePivot startPivot;

	private NFXPlayState collectNFXState;

	private readonly bool waitForFinish;

	public PlayCollectVFXAction(NFXSetup nfx, ConvertablePivot startPivot, ConvertablePivot endPivot, bool waitForFinish)
	{
		this.nfx = nfx;
		this.startPivot = startPivot;
		this.endPivot = endPivot;
		this.waitForFinish = waitForFinish;
		started = false;
	}

	protected override void Update()
	{
		if (nfx == null)
		{
			FinishAction();
			return;
		}
		if (base.actionManager.skipFrameAnim)
		{
			FinishAction();
			return;
		}
		if (!started)
		{
			started = true;
			Start();
		}
		if (collectNFXState == null)
		{
			FinishAction();
		}
	}

	private void Start()
	{
		Vector3 worldPosition = startPivot.ToCanvasWorldPosition(gameState.UIManager.mainCanvas);
		collectNFXState = gameState.applicationState.nfxManager.SpawnAndPlay(nfx, gameState.UIManager.uiVFXRoot, worldPosition);
		((Component)(object)collectNFXState.nfxInstance.GetComponentInChildren<UIParticleAttractor>()).transform.position = endPivot.ToCanvasWorldPosition(gameState.UIManager.mainCanvas);
		if (!waitForFinish)
		{
			FinishAction();
		}
	}
}
