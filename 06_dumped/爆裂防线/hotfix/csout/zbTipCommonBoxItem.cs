using System;
using Proto.Enums;
using UnityEngine.UIElements;

public class zbTipCommonBoxItem : zbViewBaseItem
{
	private zbTipCommonWnd m_wndParent;

	private Label m_lbTitle;

	private Label m_lbName;

	private Label m_lbProWrap;

	private Label m_lbProval;

	private Label m_lbVal;

	private Label m_lbCount;

	private zbItem m_item;

	private Button m_btnTouch;

	private zbBoxCsvRecord m_record;

	private long m_count;

	private bool m_canCompose;

	public zbTipCommonBoxItem(zbTipCommonWnd wndParent)
	{
		m_wndParent = wndParent;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/tip/tb_tipCommon_box.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_lbTitle = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_title", (string)null);
		m_lbName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_name", (string)null);
		m_lbProWrap = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_proWrap", (string)null);
		m_lbProval = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_proVal", (string)null);
		m_lbVal = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_val", (string)null);
		m_lbCount = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_count", (string)null);
		m_item = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, "item", (string)null);
		m_item.OnEnable(m_wndParent);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)onTouch, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(zbBoxCsvRecord record)
	{
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		m_record = record;
		m_item.Refresh(new zbItemData(EResourceType.Box, record.id, 1L), isNameVis: false, isCountVis: false);
		stResorceIconInfo resorceIconPath = zbResourceHelper.GetResorceIconPath(EResourceType.Box, record.id);
		((TextElement)m_lbName).text = resorceIconPath.strName;
		((TextElement)m_btnTouch).text = zbUtil.Language("lang.10184");
		m_canCompose = false;
		m_count = zbSelf.Instance.dataHandler.GetResourceCount(EResourceType.Box, record.id);
		if (m_record.type == 3)
		{
			((TextElement)m_lbTitle).text = zbUtil.Language("lang.10433");
			((VisualElement)m_lbProWrap).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_lbCount).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((TextElement)m_lbVal).text = m_count + "/" + m_record.boxCost;
			float num = (float)m_count / (float)m_record.boxCost * 100f;
			((VisualElement)m_lbProval).style.width = new StyleLength(new Length((num > 100f) ? 100f : num, (LengthUnit)1));
			if (m_count > m_record.boxCost)
			{
				((TextElement)m_btnTouch).text = zbUtil.Language("lang.10002");
				m_canCompose = true;
			}
		}
		else
		{
			((TextElement)m_lbTitle).text = zbUtil.Language("lang.10030");
			((VisualElement)m_lbProWrap).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_lbCount).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((TextElement)m_lbCount).text = zbUtil.Language("lang.10019") + ": " + m_count;
			if (m_count > 0)
			{
				((TextElement)m_btnTouch).text = zbUtil.Language("lang.10433");
			}
		}
	}

	public void onTouch(ClickEvent evt)
	{
		if (m_record == null)
		{
			return;
		}
		if (m_record.type == 1 || m_record.type == 4)
		{
			if (m_count == 1)
			{
				zbWsMsgHandler.Instance.Bag_RandBoxOpen_Req(m_record.id, 1);
			}
			else if (m_count > 1)
			{
				zbUIMgr.Instance.ShowBagRandBoxOpen(new zbItemData(EResourceType.Box, m_record.id, m_count));
			}
			else
			{
				zbUIMgr.Instance.ShowTip(EResourceType.Box, m_record.id, 0L);
			}
		}
		else if (m_record.type == 2 && m_count > 0)
		{
			zbWsMsgHandler.Instance.Bag_SelectBoxView_Req(m_record.id);
		}
		else if (m_record.type == 3 && m_canCompose)
		{
			zbUIMgr.Instance.ShowBoxCompose(new zbItemData(EResourceType.Box, m_record.id, m_count));
		}
		else
		{
			zbUIMgr.Instance.ShowTip(EResourceType.Box, m_record.id, 0L);
		}
	}
}
