using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbHeroGeneSelectItem : zbViewBaseItem
{
	private zbHeroGeneSelectWnd m_wndParent;

	private zbHeroGeneSelectTab m_parent;

	private VisualElement m_spriteFrame;

	private VisualElement m_spriteIcon;

	private Label m_lbName;

	private Label m_lbCount;

	private Button m_btnTouch;

	private ResourceDetail m_data;

	public zbHeroGeneSelectItem(zbHeroGeneSelectWnd wndParent, zbHeroGeneSelectTab parent)
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
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		m_data = data;
		zbGeneCsvRecord recordByKey = zbDataConfig.Instance.geneCsvTable.GetRecordByKey(data.Tid);
		if (recordByKey != null)
		{
			string resourceFrameTex = zbResourceHelper.GetResourceFrameTex((EQuality)recordByKey.quality);
			m_spriteFrame.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resourceFrameTex));
			m_spriteIcon.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.icon));
			((TextElement)m_lbName).text = recordByKey.name;
		}
		long num = zbSelf.Instance.geneHandler.getBagGeneCount(data.Tid);
		if (m_wndParent.m_geneSlotDetail.GeneTid == data.Tid)
		{
			num++;
		}
		((TextElement)m_lbCount).text = num.ToString();
		zbUtil.SetElementGray((VisualElement)(object)this, data.Tid == m_wndParent.m_geneSlotDetail.GeneTid);
		((VisualElement)m_btnTouch).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((data.Tid == m_wndParent.m_geneSlotDetail.GeneTid) ? 1 : 0));
	}

	public void OnTouch(ClickEvent evt)
	{
		if (m_wndParent.m_selectTabIndex != m_parent.m_tabIndex)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10921"));
			return;
		}
		zbWsMsgHandler.Instance.HeroAwaken_GeneSet_Req(m_wndParent.m_skillType, m_wndParent.m_index, m_data.Tid);
		m_wndParent.OnClose(evt);
	}
}
