using System;
using System.Text.RegularExpressions;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbChatWorldItem : zbViewBaseItem
{
	private zbChatWnd m_parent;

	private ChatDetail m_chatData;

	private bool m_isMyChat;

	private bool m_isEmoji;

	private EmojiMapper m_mapper;

	private VisualElement m_myChat;

	private VisualElement m_otherChat;

	private Label m_myName;

	private VisualElement m_myHead;

	private VisualElement m_myHeadFrame;

	private Label m_myMsg;

	private Label m_myVal;

	private Label m_myEmoji;

	private Button m_myBtnHead;

	private Label m_myLevel;

	private Label m_otherName;

	private VisualElement m_otherHead;

	private VisualElement m_otherHeadFrame;

	private Label m_otherMsg;

	private Label m_otherVal;

	private Label m_otherEmoji;

	private Button m_otherBtnHead;

	private Label m_otherLevel;

	public zbChatWorldItem(VisualTreeAsset tAsset, zbChatWnd parent)
	{
		m_parent = parent;
		((VisualElement)this).Add((VisualElement)(object)tAsset.Instantiate());
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
		m_mapper = new EmojiMapper();
		m_myChat = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "myChat", (string)null);
		m_myName = UQueryExtensions.Q<Label>(m_myChat, "name", (string)null);
		m_myHead = UQueryExtensions.Q<VisualElement>(m_myChat, "header", (string)null);
		m_myHeadFrame = UQueryExtensions.Q<VisualElement>(m_myChat, "frame", (string)null);
		m_myMsg = UQueryExtensions.Q<Label>(m_myChat, "lb_msg", (string)null);
		m_myVal = UQueryExtensions.Q<Label>(m_myChat, "lb_val", (string)null);
		m_myEmoji = UQueryExtensions.Q<Label>(m_myChat, "lb_emoji", (string)null);
		m_myBtnHead = UQueryExtensions.Q<Button>(m_myChat, "btn_head", (string)null);
		m_myLevel = UQueryExtensions.Q<Label>(m_myChat, "lb_level", (string)null);
		m_otherChat = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "otherChat", (string)null);
		m_otherName = UQueryExtensions.Q<Label>(m_otherChat, "name", (string)null);
		m_otherHead = UQueryExtensions.Q<VisualElement>(m_otherChat, "header", (string)null);
		m_otherHeadFrame = UQueryExtensions.Q<VisualElement>(m_otherChat, "frame", (string)null);
		m_otherMsg = UQueryExtensions.Q<Label>(m_otherChat, "lb_msg", (string)null);
		m_otherVal = UQueryExtensions.Q<Label>(m_otherChat, "lb_val", (string)null);
		m_otherEmoji = UQueryExtensions.Q<Label>(m_otherChat, "lb_emoji", (string)null);
		m_otherBtnHead = UQueryExtensions.Q<Button>(m_otherChat, "btn_head", (string)null);
		m_otherLevel = UQueryExtensions.Q<Label>(m_otherChat, "lb_level", (string)null);
		parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_myBtnHead, (Action<ClickEvent>)OnHead, "", (TrickleDown)0);
		parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_otherBtnHead, (Action<ClickEvent>)OnHead, "", (TrickleDown)0);
	}

	public void Init(zbChatWnd parent, ChatDetail data)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		m_chatData = data;
		m_isMyChat = data.RoleBaseInfo.RoleId == zbServerData.sGameData.RoleBaseInfo.RoleId;
		m_myChat.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!m_isMyChat));
		m_otherChat.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_isMyChat ? 1 : 0));
		if (m_isMyChat)
		{
			((TextElement)m_myName).text = data.RoleBaseInfo.RoleName;
			zbUtil.InitPlayerInfo(m_myHead, data.RoleBaseInfo.HeadId, m_myHeadFrame, data.RoleBaseInfo.HeadFrameId, m_myLevel, data.RoleBaseInfo.Level);
			ResetUI(m_myMsg, m_myEmoji);
		}
		else
		{
			((TextElement)m_otherName).text = data.RoleBaseInfo.RoleName;
			zbUtil.InitPlayerInfo(m_otherHead, data.RoleBaseInfo.HeadId, m_otherHeadFrame, data.RoleBaseInfo.HeadFrameId, m_otherLevel, data.RoleBaseInfo.Level);
			ResetUI(m_otherMsg, m_otherEmoji);
		}
		Refresh();
	}

	private void ResetUI(Label lbMsg, Label lbEmoji)
	{
		((VisualElement)lbMsg).visible = false;
		((VisualElement)lbEmoji).visible = false;
	}

	private void Refresh()
	{
		if (m_chatData == null)
		{
			return;
		}
		m_isEmoji = IsEmoji(m_chatData);
		if (m_isMyChat)
		{
			if (m_isEmoji)
			{
				ShowEmoji(m_myEmoji, m_myMsg, m_myVal);
			}
			else
			{
				ShowText(m_myEmoji, m_myMsg, m_myVal);
			}
		}
		else if (m_isEmoji)
		{
			ShowEmoji(m_otherEmoji, m_otherMsg, m_otherVal);
		}
		else
		{
			ShowText(m_otherEmoji, m_otherMsg, m_otherVal);
		}
	}

	private void ShowText(Label lbEmoji, Label lbMsg, Label lbVal)
	{
		((VisualElement)lbMsg).visible = true;
		((VisualElement)lbEmoji).visible = false;
		string label = m_chatData.Action.BasicData.Text.Label;
		label = zbCharChecker.CensorSensitiveWords(label);
		((TextElement)lbVal).text = label;
	}

	private void ShowEmoji(Label lbEmoji, Label lbMsg, Label lbVal)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)lbMsg).visible = false;
		((VisualElement)lbEmoji).visible = true;
		string imagePath = m_mapper.GetImagePath(m_chatData.Action.BasicData.Text.Label);
		if (!string.IsNullOrEmpty(imagePath))
		{
			Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(imagePath);
			((VisualElement)lbEmoji).style.backgroundImage = new StyleBackground(val);
		}
	}

	private void OnHead(ClickEvent evt)
	{
		zbWsMsgHandler.Instance.Role_TipsInfo_Req((int)m_chatData.RoleBaseInfo.RoleId);
	}

	public static bool IsEmoji(ChatDetail chatData)
	{
		return new Regex("^::\\d+::$").IsMatch(chatData.Action.BasicData.Text.Label);
	}
}
