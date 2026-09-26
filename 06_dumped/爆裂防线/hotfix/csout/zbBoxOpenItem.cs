using System;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbBoxOpenItem : zbViewBaseItem
{
	private zbBoxOpenWnd m_wndParent;

	private zbItem m_item = new zbItem();

	private Label m_lbNum;

	private Button m_btnSubtract;

	private Button m_btnAdd;

	private AwardDetail m_award;

	private int m_index;

	private long m_numSel = 1L;

	private long m_numMin;

	private long m_numMax = 100L;

	public zbBoxOpenItem(zbBoxOpenWnd wndParent)
	{
		m_wndParent = wndParent;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/bag/tb_boxOpen.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_item = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, "item", (string)null);
		m_item.OnEnable(m_wndParent);
		m_lbNum = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_num", (string)null);
		m_btnSubtract = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_subtract", (string)null);
		m_btnAdd = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_add", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSubtract, (Action<ClickEvent>)OnSubtract, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAdd, (Action<ClickEvent>)OnAdd, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(AwardDetail data, int index, long numMax)
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		m_award = data;
		m_index = index;
		m_numMax = numMax;
		m_item.Refresh(new zbItemData(m_award.Type, m_award.Tid, m_award.Count));
		((VisualElement)m_item.m_lbName).style.color = new StyleColor(Color32.op_Implicit(new Color32((byte)27, (byte)27, (byte)27, byte.MaxValue)));
		((VisualElement)m_item.m_lbName).style.unityTextOutlineWidth = StyleFloat.op_Implicit((StyleKeyword)3);
		m_numSel = 0L;
		RefreshNum();
	}

	private void RefreshNum()
	{
		((TextElement)m_lbNum).text = m_numSel.ToString();
		SelectDetail selectDetail = new SelectDetail
		{
			Index = m_index,
			Count = (int)m_numSel
		};
		m_wndParent.RefreshSelectList(selectDetail);
	}

	public void OnSubtract(ClickEvent evt)
	{
		if (m_numSel > m_numMin)
		{
			m_numSel--;
			RefreshNum();
		}
	}

	public void OnAdd(ClickEvent evt)
	{
		if (m_wndParent.GetSelectCount() >= m_numMax)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10236"));
		}
		else if (m_numSel < m_numMax)
		{
			m_numSel++;
			RefreshNum();
		}
	}
}
