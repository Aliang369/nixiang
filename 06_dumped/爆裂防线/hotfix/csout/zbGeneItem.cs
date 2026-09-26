using System;
using UnityEngine;
using UnityEngine.UIElements;

public class zbGeneItem : zbViewBaseItem
{
	private VisualElement m_spriteFrame;

	private VisualElement m_spriteIcon;

	private Label m_lbCount;

	private Label m_lbName;

	private Button m_btnTouch;

	private zbGeneWnd m_wndParent;

	private zbItemData m_data;

	public zbGeneItem(zbGeneWnd wndParent)
	{
		m_wndParent = wndParent;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/hero/tb_gene.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_spriteFrame = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "frame", (string)null);
		m_spriteIcon = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon", (string)null);
		m_lbCount = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_count", (string)null);
		m_lbName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_name", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "touch", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnTouch, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(zbItemData data)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		m_data = data;
		string resourceFrameTex = zbResourceHelper.GetResourceFrameTex(zbResourceHelper.GetResourceQuality(m_data.type, m_data.id));
		m_spriteFrame.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resourceFrameTex));
		stResorceIconInfo resorceIconPath = zbResourceHelper.GetResorceIconPath(m_data.type, m_data.id);
		m_spriteIcon.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resorceIconPath.strIconPath));
		((TextElement)m_lbName).text = resorceIconPath.strName;
		((TextElement)m_lbCount).text = zbUtil.ConvertToTBMK(m_data.count);
	}

	public void OnTouch(ClickEvent evt)
	{
		if ((Object)(object)m_wndParent != (Object)null && m_data != null)
		{
			zbUIMgr.Instance.ShowBagItemDetail(m_data);
		}
	}
}
