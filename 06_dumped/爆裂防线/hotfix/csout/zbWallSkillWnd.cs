using System;
using UnityEngine;
using UnityEngine.UIElements;

public class zbWallSkillWnd : zbBaseWnd
{
	private Button m_btnTouchTest;

	private Button m_btnClose;

	private Label[] m_lbStars = (Label[])(object)new Label[5];

	private VisualElement[] m_spriteSkills = (VisualElement[])(object)new VisualElement[5];

	private VisualElement[] m_spriteLocks = (VisualElement[])(object)new VisualElement[5];

	private Label[] m_lbDescs = (Label[])(object)new Label[5];

	private zbWallCsvRecord m_record;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouchTest = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouchTest, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		for (int i = 0; i < 5; i++)
		{
			VisualElement val = UQueryExtensions.Q<VisualElement>(m_RootElement, "skill" + i, (string)null);
			m_lbStars[i] = UQueryExtensions.Q<Label>(val, "lb_star", (string)null);
			m_spriteSkills[i] = UQueryExtensions.Q<VisualElement>(val, "bg_skill", (string)null);
			m_spriteLocks[i] = UQueryExtensions.Q<VisualElement>(val, "bg_lock", (string)null);
			m_lbDescs[i] = UQueryExtensions.Q<Label>(val, "lb_desc", (string)null);
		}
		AddDark();
	}

	public void RefreshCustom(int wallId, bool isPreview = false)
	{
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		m_record = zbDataConfig.Instance.wallCsvTable.GetRecordByKey(wallId);
		int num = zbSelf.Instance.wallHandler.GetWallDetail(wallId)?.Star ?? 0;
		if (isPreview)
		{
			num = 5;
		}
		for (int i = 0; i < 5; i++)
		{
			((TextElement)m_lbStars[i]).text = string.Format(zbUtil.Language("lang.10843"), i + 1);
			m_spriteSkills[i].style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(m_record.skillIcon));
			m_spriteLocks[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((i + 1 <= num) ? 1 : 0));
			((TextElement)m_lbDescs[i]).text = zbSelf.Instance.wallHandler.GetSkillDesc(wallId, i + 1);
			((TextElement)m_lbDescs[i]).text = ((TextElement)m_lbDescs[i]).text.Replace("<color=#64FF26>", "<color=#3CB317>");
		}
	}

	public void OnClose(ClickEvent evt)
	{
		Hide();
	}
}
