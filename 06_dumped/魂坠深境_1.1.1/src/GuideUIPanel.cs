using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Video;

public class GuideUIPanel : UIBase
{
	public VideoPlayer videoPlayer;

	public Canvas guidePanelCanvas;

	public GuideHintPanelSet introHint_teach1_1;

	public GuideHintPanelSet introHint_teach1_2;

	public GuideHintPanelSet introHint_teach1_3;

	public GuideHintPanelSet introHint_teach1_4;

	public GuideHintPanelSet introHint_teach2_1;

	public GuideHintPanelSet introHint_teach3_1;

	public GuideHintPanelSet introHint_teach3_2;

	public GuideHintPanelSet introHint_teach3_3;

	public GuideHintPanelSet introHint_teach3_4;

	public GuideHintPanelSet triggerHint_crystalFreeze;

	public GuideHintPanelSet triggerHint_map;

	public GuideHintPanelSet triggerHint_enchant;

	public GuideHintPanelSet triggerHint_goldStore;

	public GuideHintPanelSet triggerHint_bless;

	public GuideHintPanelSet triggerHint_itemBonus;

	public GuideHintPanelSet triggerHint_activateSkill;

	public GuideHintPanelSet triggerHint_upgradeRoom;

	public GuideHintPanelSet triggerHint_mintageRune;

	public GuideHintPanelSet triggerHint_loot;

	public GuideHintPanelSet triggerHint_XiaKe;

	public GuideHintPanelSet triggerHint_setSkin;

	public GuideHintPanelSet triggerHint_spiderContract;

	public GuideMask guideMask_leaveButton;

	public GuideMask guideMask_endTurnButton;

	public GuideHoverHint guideHoverHint_castItem;

	public GuideHoverHint guideHoverHint_enterLocation;

	public List<MaskGuideSeqSetup> maskGuideSeqSetups = new List<MaskGuideSeqSetup>();

	private List<GuideHintPanel> GetAllFieldGuidePanel()
	{
		List<GuideHintPanel> list = new List<GuideHintPanel>();
		FieldInfo[] fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (FieldInfo fieldInfo in fields)
		{
			if (fieldInfo.FieldType == typeof(GuideHintPanel))
			{
				list.Add((GuideHintPanel)fieldInfo.GetValue(this));
			}
			if (fieldInfo.FieldType == typeof(GuideHintPanelSet))
			{
				InitUI((GuideHintPanelSet)fieldInfo.GetValue(this));
			}
			if (fieldInfo.FieldType == typeof(GuideMask))
			{
				InitUI((GuideMask)fieldInfo.GetValue(this));
			}
			if (fieldInfo.FieldType == typeof(GuideHoverHint))
			{
				InitUI((GuideHoverHint)fieldInfo.GetValue(this));
			}
		}
		return list;
	}

	protected override void Init()
	{
		base.Init();
		foreach (GuideHintPanel item in GetAllFieldGuidePanel())
		{
			InitUI(item);
		}
		foreach (MaskGuideSeqSetup maskGuideSeqSetup in maskGuideSeqSetups)
		{
			InitUI(maskGuideSeqSetup.guideSeq);
			maskGuideSeqSetup.guideSeq.gameObject.SetActive(value: false);
		}
		InitUI(guideMask_leaveButton);
		InitUI(guideMask_endTurnButton);
	}

	protected override void Dispose()
	{
		if (videoPlayer.targetTexture != null)
		{
			videoPlayer.targetTexture.Release();
		}
	}
}
