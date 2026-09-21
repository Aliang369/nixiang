using System;
using DG.Tweening;
using UnityEngine;

public class PlayerBftUIDisplay : BftUIDisplay
{
	[Serializable]
	public class Config
	{
		[SerializeField]
		public Color slotCountIdleColor;

		[SerializeField]
		public Color slotCountOverflowColor;

		[SerializeField]
		public float HUDShowTime;

		[SerializeField]
		public float bannerTweenTime;

		[SerializeField]
		public float bannerStayTime;
	}

	[SerializeField]
	public PlayerItemArea tableItemArea;

	[SerializeField]
	public SkillPanel_Battle skillPanel;

	[SerializeField]
	public CompanionItemArea companionItemArea;

	[NonSerialized]
	public BftConnectLineDisplay connectLineDisplay;

	[NonSerialized]
	private bool isShow;

	[NonSerialized]
	private Sequence sequence;

	[NonSerialized]
	private Sequence togglePanelButtonSequence;

	protected override void Init()
	{
		base.Init();
		InitUI(tableItemArea);
		InitUI(skillPanel);
		InitUI(companionItemArea);
		base.gameObject.SetActive(value: false);
		connectLineDisplay = GetComponent<BftConnectLineDisplay>();
		isShow = true;
	}

	protected override void OnSetup(BattleFieldTransform dailyContract)
	{
		base.OnSetup(dailyContract);
		base.gameObject.SetActive(value: true);
		companionItemArea.Setup(dailyContract.companionContainer);
		CellUI<BattleFieldTransform>.gameState.UIManager.wastePileArea.Setup(dailyContract.wastePileItemContainer, dailyContract.exilePileItemContainer);
		CellUI<BattleFieldTransform>.gameState.UIManager.drawPileArea.Setup(dailyContract.drawPileItemContainer);
		SetDirty();
	}
}
