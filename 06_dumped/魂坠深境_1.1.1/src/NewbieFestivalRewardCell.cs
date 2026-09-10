using System;
using System.Collections.Generic;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewbieFestivalRewardCell : CellUI<NewbieFestivalRewardSlot>, IFrameUpdate
{
	[SerializeField]
	private GlobalItemDisplay globalItemDisplay;

	[SerializeField]
	private TextMeshProUGUI qualityText;

	[SerializeField]
	private Image darken;

	private Color qualityTextColor;

	protected override void Init()
	{
		InitUI(globalItemDisplay);
		qualityTextColor = qualityText.color;
	}

	protected override void OnSetup(NewbieFestivalRewardSlot slot)
	{
		globalItemDisplay.Setup(slot.reward);
		TextMeshProUGUI textMeshProUGUI = qualityText;
		textMeshProUGUI.text = slot.rewardLevel switch
		{
			QualityLevel.Epic => CellUI<NewbieFestivalRewardSlot>.textData.term_newbie_festival_special_prize, 
			QualityLevel.Rare => CellUI<NewbieFestivalRewardSlot>.textData.term_newbie_festival_first_prize, 
			QualityLevel.Uncommon => CellUI<NewbieFestivalRewardSlot>.textData.term_newbie_festival_second_prize, 
			QualityLevel.Common => CellUI<NewbieFestivalRewardSlot>.textData.term_newbie_festival_third_prize, 
			_ => throw new ArgumentOutOfRangeException("rewardLevel", slot.rewardLevel, "新生游艺奖励等级配置错误"), 
		};
	}

	protected override void UpdateDisplay()
	{
		if (base.setupData != null)
		{
			bool flag = GetAppendData<List<int>>()?.Contains(base.setupData.slotIndex) ?? false;
			qualityText.color = qualityTextColor * (flag ? Color.gray : Color.white);
			darken.gameObject.SetActive(flag);
		}
	}

	void IFrameUpdate.FrameUpdate()
	{
		base.transform.rotation = Quaternion.identity;
	}
}
