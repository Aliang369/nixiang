using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbSeasonForceMemberManageWnd : zbBaseWnd
{
	private VisualElement m_spriteFrame;

	private VisualElement m_spriteIcon;

	private Label m_lbName;

	private VisualElement m_spriteOffice;

	private Button[] m_btnOffices = (Button[])(object)new Button[3];

	private VisualElement[] m_spriteOks = (VisualElement[])(object)new VisualElement[3];

	private Button m_btnConfirm;

	private Button m_btnClose;

	private Button m_btnTouch;

	private SeasonMemberInfo m_data;

	private int m_selectOffice;

	protected override void OnEnable()
	{
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_spriteFrame = UQueryExtensions.Q<VisualElement>(m_RootElement, "frame", (string)null);
		m_spriteIcon = UQueryExtensions.Q<VisualElement>(m_RootElement, "header", (string)null);
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_spriteOffice = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_office", (string)null);
		for (int i = 0; i < 3; i++)
		{
			m_btnOffices[i] = UQueryExtensions.Q<Button>(m_RootElement, "btn_office" + (i + 1), (string)null);
			m_spriteOks[i] = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btnOffices[i], "sprite_ok", (string)null);
			int curIndex = i;
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnOffices[i], (Action<ClickEvent>)delegate
			{
				OnOffices(curIndex);
			}, "", (TrickleDown)0);
			if (i > 0)
			{
				m_spriteOks[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
			else
			{
				m_spriteOks[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			}
		}
		m_btnConfirm = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnConfirm, (Action)OnConfirm, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_SeasonMember, OnEventUpdate);
		AddDark();
	}

	public void RefreshCustom(SeasonMemberInfo data)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		m_data = data;
		m_selectOffice = (int)m_data.Office;
		string playerHeadFrameTex = zbResourceHelper.GetPlayerHeadFrameTex(m_data.RoleBaseInfo.HeadFrameId);
		m_spriteFrame.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(playerHeadFrameTex));
		string playerHeadTex = zbResourceHelper.GetPlayerHeadTex(m_data.RoleBaseInfo.HeadId);
		m_spriteIcon.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(playerHeadTex));
		((TextElement)m_lbName).text = m_data.RoleBaseInfo.RoleName;
		m_spriteOffice.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>($"ui/image/season/season_r{m_selectOffice}.png"));
	}

	private void RefreshOffices()
	{
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 3; i++)
		{
			m_spriteOks[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_selectOffice != i + 1) ? 1 : 0));
		}
		if (m_selectOffice != (int)m_data.Office)
		{
			((VisualElement)m_btnConfirm).SetEnabled(true);
			((VisualElement)m_btnConfirm).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_260_88_yellow.png"));
		}
		else
		{
			((VisualElement)m_btnConfirm).SetEnabled(false);
			((VisualElement)m_btnConfirm).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_260_88_gray.png"));
		}
	}

	private void OnOffices(int index)
	{
		m_selectOffice = index + 1;
		RefreshOffices();
	}

	private void OnConfirm()
	{
		zbWsMsgHandler.Instance.Season_Office_Appoint_Req(m_data.RoleBaseInfo.RoleId, (ESeasonOffice)m_selectOffice);
	}

	private void OnClose()
	{
		Hide();
	}

	private void OnEventUpdate(object data)
	{
		zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10068"));
		OnClose();
	}
}
