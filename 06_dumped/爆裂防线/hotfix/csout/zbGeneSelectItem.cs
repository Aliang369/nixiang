using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbGeneSelectItem : zbViewBaseItem
{
	private zbGeneSelectWnd m_wndParent;

	private zbGeneSelectTab m_parent;

	private VisualElement m_spriteFrame;

	private VisualElement m_spriteIcon;

	private Label m_lbName;

	private Label m_lbCount;

	private Button m_btnTouch;

	private ResourceDetail m_data;

	public zbGeneSelectItem(zbGeneSelectWnd wndParent, zbGeneSelectTab parent)
	{
		m_wndParent = wndParent;
		m_parent = parent;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/hero/tb_geneSelect.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_spriteFrame = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "frame", (string)null);
		m_spriteIcon = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon", (string)null);
		m_lbName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_name", (string)null);
		m_lbCount = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_count", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnTouch, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(ResourceDetail data)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		m_data = data;
		zbGeneCsvRecord recordByKey = zbDataConfig.Instance.geneCsvTable.GetRecordByKey(data.Tid);
		if (recordByKey != null)
		{
			string resourceFrameTex = zbResourceHelper.GetResourceFrameTex((EQuality)recordByKey.quality);
			m_spriteFrame.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resourceFrameTex));
			m_spriteIcon.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.icon));
			((TextElement)m_lbName).text = recordByKey.name;
		}
		((TextElement)m_lbCount).text = data.Count.ToString();
	}

	public void OnTouch(ClickEvent evt)
	{
		if (m_wndParent.m_selectTabIndex != m_parent.m_tabIndex)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10921"));
			return;
		}
		zbGeneComposeWnd zbGeneComposeWnd2 = zbUIMgr.Instance.GetWndById(EWndId.GeneCompose) as zbGeneComposeWnd;
		if ((Object)(object)zbGeneComposeWnd2 != (Object)null && zbGeneComposeWnd2.IsShow())
		{
			zbGeneComposeWnd2.m_geneTids[m_wndParent.m_index] = m_data.Tid;
			zbGeneComposeWnd2.RefreshDetail();
		}
		m_wndParent.OnClose(evt);
	}
}
