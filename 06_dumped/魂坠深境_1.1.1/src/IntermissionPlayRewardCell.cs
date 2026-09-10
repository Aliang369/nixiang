using System;
using ChillyRoom.Services.ContainerOfSoul;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IntermissionPlayRewardCell : CellUI<IntermissionPlayRewardSlotDto>, IFrameUpdate
{
	[SerializeField]
	private GlobalItemDisplay globalItemDisplay;

	[SerializeField]
	private TextMeshProUGUI qualityText;

	[SerializeField]
	private Image darken;

	[SerializeField]
	private GameObject winnerHighlight;

	private Color qualityTextColor;

	private bool isWinner;

	private bool suppressDrawnState;

	protected override void Init()
	{
		InitUI(globalItemDisplay);
		qualityTextColor = qualityText.color;
	}

	public void Setup(IntermissionPlayRewardSlotDto slot, bool winner, bool suppressDrawn)
	{
		isWinner = winner;
		suppressDrawnState = suppressDrawn;
		Setup(slot);
	}

	protected override void OnSetup(IntermissionPlayRewardSlotDto slot)
	{
		globalItemDisplay.Setup(slot.RewardView.ToDisplaySetup(CellUI<IntermissionPlayRewardSlotDto>.applicationState));
		TextMeshProUGUI textMeshProUGUI = qualityText;
		textMeshProUGUI.text = (QualityLevel)slot.RewardLevel switch
		{
			QualityLevel.Epic => CellUI<IntermissionPlayRewardSlotDto>.textData.term_newbie_festival_special_prize, 
			QualityLevel.Rare => CellUI<IntermissionPlayRewardSlotDto>.textData.term_newbie_festival_first_prize, 
			QualityLevel.Uncommon => CellUI<IntermissionPlayRewardSlotDto>.textData.term_newbie_festival_second_prize, 
			QualityLevel.Common => CellUI<IntermissionPlayRewardSlotDto>.textData.term_newbie_festival_third_prize, 
			_ => throw new ArgumentOutOfRangeException("RewardLevel", slot.RewardLevel, "幕间游艺奖励等级配置错误"), 
		};
	}

	protected override void UpdateDisplay()
	{
		bool flag = base.setupData.IsDrawn && !suppressDrawnState;
		qualityText.color = qualityTextColor * (flag ? Color.gray : Color.white);
		darken.gameObject.SetActive(flag);
		winnerHighlight.SetActive(flag);
	}

	void IFrameUpdate.FrameUpdate()
	{
		base.transform.rotation = Quaternion.identity;
	}
}
