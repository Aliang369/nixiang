using System;
using System.Collections;
using PathCreation;
using UnityEngine;

public class PlayTransformVFXAction : GameAction
{
	private readonly bool is3d;

	private readonly NFXSetup setup;

	private Vector3 startPosition;

	private Vector3 endPosition;

	private readonly bool waitForFinish;

	private NFXPlayState transformNFXState;

	private Func<bool> prepareAction;

	private bool started => transformNFXState != null;

	public PlayTransformVFXAction(NFXSetup setup, PivotManager.IWithPivot start, PivotManager.IWithPivot target, bool is3d, bool waitForFinish = true)
	{
		PlayTransformVFXAction playTransformVFXAction = this;
		this.setup = setup;
		this.waitForFinish = waitForFinish;
		this.is3d = is3d;
		prepareAction = delegate
		{
			ConvertablePivot convertablePivot = start?.GetPivot(playTransformVFXAction.gameState);
			ConvertablePivot convertablePivot2 = target?.GetPivot(playTransformVFXAction.gameState);
			if (convertablePivot == null || convertablePivot2 == null)
			{
				return false;
			}
			playTransformVFXAction.startPosition = convertablePivot.ToCanvasWorldPosition(playTransformVFXAction.gameState.UIManager.mainCanvas);
			playTransformVFXAction.endPosition = convertablePivot2.ToCanvasWorldPosition(playTransformVFXAction.gameState.UIManager.mainCanvas);
			return true;
		};
	}

	protected override void Update()
	{
		if (base.actionManager.skipFrameAnim)
		{
			FinishAction();
		}
		else if (!started)
		{
			Start();
		}
	}

	private void Start()
	{
		if (!setup)
		{
			FinishAction();
			return;
		}
		if (prepareAction != null && !prepareAction())
		{
			FinishAction();
			return;
		}
		if (!is3d)
		{
			startPosition.z = 0f;
			endPosition.z = 0f;
		}
		if (Vector3.Distance(startPosition, endPosition) < 0.1f)
		{
			FinishAction();
			return;
		}
		transformNFXState = gameState.applicationState.nfxManager.SpawnAndPlay(setup, is3d ? gameState.UIManager.sceneVFXRoot : gameState.UIManager.uiVFXRoot, () => transformNFXState?.nfxInstance?.transform.position, destroyOnStop: false);
		transformNFXState.nfxInstance.transform.LookAt(endPosition, Vector3.back);
		gameState.StartCoroutine(DoCubicBezierTween);
		if (!waitForFinish)
		{
			FinishAction();
		}
	}

	private IEnumerator DoCubicBezierTween()
	{
		VFXDataPreset instance = ScriptableObjectSingleton<VFXDataPreset>.instance;
		BezierLineHelper.BeizerCreatorData beizerCreatorData = new BezierLineHelper.BeizerCreatorData();
		beizerCreatorData.type = instance.bezierType;
		beizerCreatorData.anchors = new Vector3[2] { startPosition, endPosition };
		beizerCreatorData.controlDirType = instance.controlDirType;
		beizerCreatorData.startControl = instance.startDirection;
		beizerCreatorData.endControl = instance.endDirection;
		beizerCreatorData.controlLengthType = instance.controlLengthType;
		beizerCreatorData.controlScale = instance.controlScale;
		beizerCreatorData.controlInvertByScreen = instance.controlInvertByScreen;
		beizerCreatorData.camera = Camera.main;
		BezierPath bezier = BezierLineHelper.GetBezier(beizerCreatorData);
		float num = instance.shootDuration * setup.shootTimeMultiply;
		BezierLineEvaluator evaluator = new BezierLineEvaluator(bezier);
		float t = 0f;
		float factor = 1f / num;
		float waitTime = num;
		do
		{
			transformNFXState.nfxInstance.transform.position = evaluator.GetPoint(t * factor);
			transformNFXState.nfxInstance.transform.rotation = Quaternion.LookRotation(endPosition - startPosition);
			yield return new WaitForEndOfFrame();
			t += Time.deltaTime * base.actionTimeScale;
		}
		while (t < waitTime);
		float num2 = 0f;
		if (transformNFXState.nfxInstance.GetComponentInChildren<TrailRenderer>() != null)
		{
			num2 += transformNFXState.nfxInstance.GetComponentInChildren<TrailRenderer>().time;
		}
		transformNFXState.Stop(num2);
		if (waitForFinish)
		{
			FinishAction();
		}
	}
}
