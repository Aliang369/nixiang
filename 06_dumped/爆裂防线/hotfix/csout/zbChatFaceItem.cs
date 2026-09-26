using System;
using UnityEngine;
using UnityEngine.UIElements;

public class zbChatFaceItem : zbViewBaseItem
{
	private Label[] m_dummys = (Label[])(object)new Label[8];

	private zbChatWnd m_wndRoot;

	private zbChatFacePanel m_wndParent;

	public zbChatFaceItem(VisualTreeAsset tAsset)
	{
		((VisualElement)this).Add((VisualElement)(object)tAsset.Instantiate());
	}

	public void Init(zbChatWnd wndRoot, ChatFaceItem item, zbChatFacePanel wndParent)
	{
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		m_wndRoot = wndRoot;
		m_wndParent = wndParent;
		for (int i = 0; i < item.count; i++)
		{
			m_dummys[i] = UQueryExtensions.Q<Label>((VisualElement)(object)this, "dummy_" + i, (string)null);
			string emojiCode = item.rowIndex.ToString() + (i + 1) * 10;
			string resName = "ui/image/chat/icon_" + emojiCode + ".png";
			Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(resName);
			((VisualElement)m_dummys[i]).style.backgroundImage = new StyleBackground(val);
			m_wndRoot.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_dummys[i], (Action)delegate
			{
				OnClickEmoji(emojiCode);
			}, "", (TrickleDown)0);
		}
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	private void OnClickEmoji(string emojiCode)
	{
		if (emojiCode != null)
		{
			m_wndRoot.OnSend("::" + emojiCode + "::");
			m_wndParent.Hide();
		}
	}
}
