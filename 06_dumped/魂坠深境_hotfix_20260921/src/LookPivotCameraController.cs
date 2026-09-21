using System;
using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

public class LookPivotCameraController : MonoBehaviour
{
	public VirtualCamera virtualCamera;

	[SerializeField]
	private Transform lookTransform;

	[SerializeField]
	public bool lockCameraTransform;

	[SerializeField]
	public float playerTurnEndMoveTime;

	[SerializeField]
	public Vector3 playerTurnEndMoveOffset;

	[SerializeField]
	public float playerTurnStartMoveTime;

	[SerializeField]
	public Vector3 playerTurnStartMoveOffset;

	[NonSerialized]
	private Vector3 offset;

	[NonSerialized]
	private Sequence offsetTween;

	private GameStateInfo gameState => SceneInfoSingleton.instance?.gameState;

	private void Update()
	{
		if (!(virtualCamera == null) && lockCameraTransform)
		{
			UpdateLookPivot();
			lookTransform.localPosition = offset;
		}
	}

	public void ResetLookRotation()
	{
		lookTransform.rotation = Quaternion.identity;
	}

	private void UpdateLookPivot()
	{
		if (gameState == null)
		{
			return;
		}
		if (gameState.currentEncounter?.enemyAI != null && gameState.currentEncounter.enemyAI.BftDisplay != null && gameState.currentEncounter.enemyAI.BftDisplay.lookPivot != null)
		{
			Transform lookPivot = gameState.currentEncounter.enemyAI.BftDisplay.lookPivot;
			if (lookPivot != null)
			{
				lookTransform.rotation = Quaternion.LookRotation((lookPivot.transform.position - lookTransform.position).normalized);
			}
		}
		else if (gameState.currentEncounter?.majorRoomObjectDisplay != null)
		{
			Transform lookPivot2 = gameState.currentEncounter.majorRoomObjectDisplay.lookPivot;
			if (lookPivot2 != null)
			{
				lookTransform.rotation = Quaternion.LookRotation((lookPivot2.transform.position - lookTransform.position).normalized);
			}
		}
	}

	public IEnumerator PlayerTurnStartOffsetCor()
	{
		offsetTween?.Kill();
		offsetTween = DOTween.Sequence();
		offsetTween.timeScale = gameState.actionManager.GetBattleScale();
		TweenerCore<Vector3, Vector3, VectorOptions> t = DOTween.To(() => offset, delegate(Vector3 x)
		{
			offset = x;
		}, playerTurnStartMoveOffset, playerTurnStartMoveTime);
		t.SetOptions(snapping: false).SetTarget(offset);
		offsetTween.Append(t);
		Sequence sequence = offsetTween;
		sequence.onKill = (TweenCallback)Delegate.Combine(sequence.onKill, (TweenCallback)delegate
		{
			offsetTween = null;
		});
		offsetTween.Play();
		yield return offsetTween.WaitForKill();
	}

	public IEnumerator PlayerTurnEndOffsetCor()
	{
		offsetTween?.Kill();
		offsetTween = DOTween.Sequence();
		offsetTween.timeScale = gameState.actionManager.GetBattleScale();
		TweenerCore<Vector3, Vector3, VectorOptions> t = DOTween.To(() => offset, delegate(Vector3 x)
		{
			offset = x;
		}, playerTurnEndMoveOffset, playerTurnEndMoveTime);
		t.SetOptions(snapping: false).SetTarget(offset);
		offsetTween.Append(t);
		Sequence sequence = offsetTween;
		sequence.onKill = (TweenCallback)Delegate.Combine(sequence.onKill, (TweenCallback)delegate
		{
			offsetTween = null;
		});
		offsetTween.Play();
		yield return offsetTween.WaitForKill();
	}
}
