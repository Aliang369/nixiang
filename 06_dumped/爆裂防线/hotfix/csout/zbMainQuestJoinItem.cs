using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbMainQuestJoinItem : zbViewBaseItem
{
	private RoleBaseInfo m_roleBaseInfo;

	private zbMainQuestJoinWnd m_Parent;

	private VisualElement m_headFrame;

	private VisualElement m_head;

	private Label m_lbLevel;

	private VisualElement m_btnNoode0;

	private Label m_lbName;

	private Label m_lbFv;

	private Label m_lbLeftNum;

	private Button m_btnGet;

	private Button m_btnTouch;

	private RoleLoginInfo m_loginInfo;

	public zbMainQuestJoinItem(zbMainQuestJoinWnd parent, VisualTreeAsset tAsset)
	{
		((VisualElement)this).Add((VisualElement)(object)tAsset.Instantiate());
		m_Parent = parent;
		m_lbName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_Name", (string)null);
		m_lbFv = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_Fv", (string)null);
		m_headFrame = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "headFrame", (string)null);
		m_head = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "head", (string)null);
		m_lbLevel = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_level", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		m_btnNoode0 = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "btn_noode0", (string)null);
		m_btnGet = UQueryExtensions.Q<Button>(m_btnNoode0, "btn_get", (string)null);
		m_lbLeftNum = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_Left", (string)null);
		m_Parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnTouch, "", (TrickleDown)0);
		m_Parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGet, (Action<ClickEvent>)OnGet, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Init(RoleBaseInfo info, RoleLoginInfo loginInfo = null)
	{
		m_roleBaseInfo = info;
		m_loginInfo = loginInfo;
		refreshItems();
	}

	private void refreshItems()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		int helperedNumber = zbSelf.Instance.eliteQuestHandler.GetHelperedNumber(m_roleBaseInfo.RoleId);
		string playerHeadTex = zbResourceHelper.GetPlayerHeadTex(m_roleBaseInfo.HeadId);
		string playerHeadFrameTex = zbResourceHelper.GetPlayerHeadFrameTex(m_roleBaseInfo.HeadFrameId);
		m_head.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(playerHeadTex));
		m_headFrame.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(playerHeadFrameTex));
		((TextElement)m_lbLevel).text = m_roleBaseInfo.Level.ToString();
		((TextElement)m_lbName).text = m_roleBaseInfo.RoleName;
		((TextElement)m_lbFv).text = m_roleBaseInfo.TeamAttack.ToString();
		((TextElement)m_lbLeftNum).text = zbUtil.Language("lang.10625") + (1 - helperedNumber);
		((VisualElement)m_btnGet).style.unityBackgroundImageTintColor = StyleColor.op_Implicit((helperedNumber > 0) ? Color.gray : Color.white);
	}

	private void OnGet(ClickEvent evt)
	{
		if (zbSelf.Instance.eliteQuestHandler.GetHelperedNumber(m_roleBaseInfo.RoleId) <= 0)
		{
			m_Parent.OnClose(null);
			if (m_Parent.m_funcType == EFuncType.EliteQuest)
			{
				(zbUIMgr.Instance.GetWndById(EWndId.EliteQuest) as zbEliteQuestWnd).SetJoinPlayer(m_roleBaseInfo);
			}
			else if (m_Parent.m_funcType == EFuncType.Legendary)
			{
				(zbUIMgr.Instance.GetWndById(EWndId.Legendary) as zbLegendaryWnd).SetJoinPlayer(m_roleBaseInfo);
			}
		}
	}

	private void OnTouch(ClickEvent evt)
	{
		zbWsMsgHandler.Instance.Role_TipsInfo_Req((int)m_roleBaseInfo.RoleId);
	}
}
