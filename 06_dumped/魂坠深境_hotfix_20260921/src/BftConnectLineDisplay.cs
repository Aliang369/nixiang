using System;
using Common.UI;
using UnityEngine;

public class BftConnectLineDisplay : BftUIDisplay.DisplayModule, IFrameUpdate
{
	[SerializeField]
	public BezierLineDisplay spellTargetLine;

	[NonSerialized]
	private Vector3 currentEnd;

	[NonSerialized]
	private Vector3 currentStart;

	[NonSerialized]
	private Vector3 targetEnd;

	[NonSerialized]
	private bool targetShow;

	[NonSerialized]
	private Vector3 targetStart;

	void IFrameUpdate.FrameUpdate()
	{
		if (!targetShow)
		{
			spellTargetLine.gameObject.SetActive(value: false);
			return;
		}
		spellTargetLine.gameObject.SetActive(value: true);
		if (currentStart != targetStart || currentEnd != targetEnd)
		{
			currentStart = targetStart;
			currentEnd = targetEnd;
			spellTargetLine.Setup(targetStart, targetEnd);
		}
		if (!Input.GetMouseButton(0) || !UIDisplayModule.gameState.battleState.isInBattle || UIDisplayModule.gameState.battleState.turnPhase != TurnPhase.PlayerControlPhase)
		{
			targetShow = false;
		}
	}

	public void SetValue(Vector3 start, Vector3 end)
	{
		targetStart = start;
		targetEnd = end;
		targetShow = true;
	}
}
