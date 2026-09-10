using System;
using UnityEngine;

public class EnemyBftUIDisplay : BftUIDisplay
{
	[SerializeField]
	public EnemyItemArea tableItemArea;

	[SerializeField]
	public CompanionItemArea companionItemArea;

	[SerializeField]
	public EnemyInfluenceDisplayArea influenceDisplayArea;

	[SerializeField]
	public CanvasGroup canvasGroup;

	[NonSerialized]
	public ConvertablePivot pivot;

	protected override void Init()
	{
		base.Init();
		pivot = new ConvertablePivot(base.transform);
		manager.HideUI(this);
		InitUI(tableItemArea);
		InitUI(influenceDisplayArea);
		InitUI(companionItemArea);
	}

	protected override void OnSetup(BattleFieldTransform dailyContract)
	{
		base.OnSetup(dailyContract);
		base.gameObject.SetActive(value: true);
		companionItemArea.Setup(dailyContract.companionContainer);
		tableItemArea.Setup();
		canvasGroup.alpha = 1f;
		SetDirty();
	}

	protected override void OnUnset()
	{
		manager.HideUI(this);
		tableItemArea.Unset();
	}
}
