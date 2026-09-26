using System;
using UnityEngine;
using UnityEngine.UIElements;

public class zbStrongerItem : zbViewBaseItem
{
	private zbStrongerWnd m_wndParent;

	private zbStrongerCsvRecord m_record;

	private VisualElement m_spriteIcon;

	private Label m_lbName;

	private Label m_lbDesc;

	private Button m_btnGoto;

	public zbStrongerItem()
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/comm/tb_stronger.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
	}

	public void OnEnable(zbStrongerWnd wndParent)
	{
		m_wndParent = wndParent;
		m_spriteIcon = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon", (string)null);
		m_lbName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_name", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_desc", (string)null);
		m_btnGoto = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_goto", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGoto, (Action)OnGoto, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(zbStrongerCsvRecord record)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		m_record = record;
		m_spriteIcon.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(m_record.icon));
		((TextElement)m_lbName).text = m_record.name;
		((TextElement)m_lbDesc).text = m_record.desc;
	}

	private void OnGoto()
	{
		zbLinkHelper.OpenLink(m_record.link, 0);
		m_wndParent.OnCallBack();
	}
}
