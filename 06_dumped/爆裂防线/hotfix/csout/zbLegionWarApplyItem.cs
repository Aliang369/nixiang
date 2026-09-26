using System;
using Proto.Types;
using UnityEngine.UIElements;

public class zbLegionWarApplyItem : zbViewBaseItem
{
	private Label m_iconFlag;

	private Label m_lbName;

	private Label m_lbLv;

	private Label m_lbPersonNum;

	private Label m_lbActivity;

	private Button m_btnClickItem;

	private LegionBaseInfo m_item;

	public zbLegionWarApplyItem(VisualTreeAsset tAsset, zbBaseWnd parent)
	{
		((VisualElement)this).Add((VisualElement)(object)tAsset.Instantiate());
		m_btnClickItem = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_clickItem", (string)null);
		m_iconFlag = UQueryExtensions.Q<Label>((VisualElement)(object)this, "icon_flag", (string)null);
		m_lbName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_name", (string)null);
		m_lbLv = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_lv", (string)null);
		m_lbPersonNum = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_personNum", (string)null);
		m_lbActivity = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_activity", (string)null);
		parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClickItem, (Action<ClickEvent>)OnCheckDetail, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Init(LegionBaseInfo item)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		m_item = item;
		((VisualElement)m_iconFlag).style.backgroundImage = new StyleBackground(zbSelf.Instance.legionHandler.GetFlagIconById(item.Icon));
		((TextElement)m_lbName).text = item.LegionName;
		((TextElement)m_lbLv).text = "Lv." + item.Level;
		((TextElement)m_lbPersonNum).text = item.MemberCount + "/" + item.MemberMax;
		((TextElement)m_lbActivity).text = item.Active.ToString();
	}

	private void OnCheckDetail(ClickEvent evt)
	{
		zbWsMsgHandler.Instance.Legion_TipsInfo_Req(m_item.LegionId);
	}
}
