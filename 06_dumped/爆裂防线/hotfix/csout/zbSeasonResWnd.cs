using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbSeasonResWnd : zbBaseWnd
{
	private Label m_lbTitle;

	private Label m_lbOutputDesc;

	private zbItem m_item;

	private Label m_lbOutput;

	private Label m_lbCity;

	private VisualElement m_spriteForce;

	private Button m_btnAttack;

	private Button m_btnClose;

	private Button m_btnTouch;

	private SeasonResNodeInfo m_data;

	private zbSeasonResourceCsvRecord m_record;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_lbOutputDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_outputDesc", (string)null);
		m_item = UQueryExtensions.Q<zbItem>(m_RootElement, "item", (string)null);
		m_item.OnEnable(this);
		m_lbOutput = UQueryExtensions.Q<Label>(m_RootElement, "lb_output", (string)null);
		m_lbCity = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_spriteForce = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_force", (string)null);
		m_btnAttack = UQueryExtensions.Q<Button>(m_RootElement, "btn_attack", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAttack, (Action)OnAttack, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		AddDark();
	}

	public void RefreshCustom(SeasonResNodeInfo data)
	{
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		m_data = data;
		m_record = zbDataConfig.Instance.seasonResourceCsvTable.GetRecordByKey(m_data.ResNodeId);
		((TextElement)m_lbTitle).text = m_record.name + "Lv." + m_data.Level;
		m_item.Refresh(new zbItemData(EResourceType.Money, 4L, 1L), isNameVis: false, isCountVis: false);
		((TextElement)m_lbOutput).text = "+" + m_data.ProducePerHour + "/H";
		m_spriteForce.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/season/faction_flag" + (m_data.ForceId - 1).ToString() + ".png"));
		if (m_data.OwnerRoleId == 0)
		{
			((VisualElement)m_btnAttack).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_260_88_yellow.png"));
			((TextElement)m_btnAttack).text = zbUtil.Language("lang.11461");
		}
		else
		{
			((VisualElement)m_btnAttack).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_260_88_gray.png"));
			((TextElement)m_btnAttack).text = zbUtil.Language("lang.10873");
		}
		AutoFontSize();
	}

	private void AutoFontSize()
	{
		zbUtil.AutoFontSize((TextElement)(object)m_lbOutputDesc, 22f, 16f, 400f);
	}

	private void OnAttack()
	{
		_ = m_data.OwnerRoleId;
	}

	private void OnClose()
	{
		Hide();
	}
}
