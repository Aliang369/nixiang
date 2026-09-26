using System;
using Proto.Enums;
using UnityEngine.UIElements;

public class zbSetNotifyItem : zbViewBaseItem
{
	private zbSetWnd m_parent;

	private Label m_title;

	private Label m_desc;

	private VisualElement m_check;

	private VisualElement m_checkOk;

	private zbPushSetCsvRecord m_record;

	public bool m_bClose;

	public zbSetNotifyItem(zbSetWnd parent)
	{
		m_parent = parent;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/notifyset/tb_notifyset.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_title = UQueryExtensions.Q<Label>((VisualElement)(object)this, "title", (string)null);
		m_desc = UQueryExtensions.Q<Label>((VisualElement)(object)this, "desc", (string)null);
		m_check = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "check_bg", (string)null);
		m_checkOk = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "check_ok", (string)null);
		m_parent.m_uiEvent.RegisterCallback<ClickEvent>(m_check, (Action)OnCheck, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Init(zbPushSetCsvRecord record)
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		m_record = record;
		((TextElement)m_title).text = record.name;
		((TextElement)m_desc).text = record.desc;
		m_bClose = zbSelf.Instance.notifySetHandler.IsPushTypeClose((EPushType)record.type);
		m_checkOk.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_bClose ? 1 : 0));
	}

	private void OnCheck()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		m_bClose = !m_bClose;
		m_checkOk.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_bClose ? 1 : 0));
		zbSelf.Instance.notifySetHandler.SetPushTypeClose((EPushType)m_record.type, m_bClose);
	}
}
