using System;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbScratchCardItem : zbViewBaseItem
{
	private zbScratchCardWnd m_wndParent;

	private Label m_lbName;

	private VisualElement[] m_cards;

	private SpineVisualElement[] m_spines;

	private zbItem m_targetItem;

	private Label m_lbStatus;

	private Button m_btnReceive;

	private VisualElement m_spriteGetted;

	private VisualElement m_notify;

	private OperateScratchCardDetail m_detail;

	public zbScratchCardItem(VisualTreeAsset tAsset, zbScratchCardWnd wndParent)
	{
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		m_cards = (VisualElement[])(object)new VisualElement[3];
		m_spines = (SpineVisualElement[])(object)new SpineVisualElement[3];
		base._002Ector();
		((VisualElement)this).Add((VisualElement)(object)tAsset.Instantiate());
		m_wndParent = wndParent;
		m_lbName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_name", (string)null);
		m_targetItem = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, "item_target", (string)null);
		m_lbStatus = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_status", (string)null);
		m_btnReceive = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_receive", (string)null);
		m_spriteGetted = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_getted", (string)null);
		for (int i = 0; i < 3; i++)
		{
			m_cards[i] = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "card" + i, (string)null);
			m_spines[i] = UQueryExtensions.Q<SpineVisualElement>(m_cards[i], "spine", (string)null);
		}
		m_targetItem.OnEnable(m_wndParent);
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnReceive, (Vector2?)new Vector2(-8f, -10f), 1f, true);
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnReceive, (Action)OnReceive, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Init(OperateScratchCardDetail detail)
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		m_detail = detail;
		bool flag = m_detail.Index == 1;
		((TextElement)m_lbName).text = (flag ? zbUtil.Language("lang.10991") : string.Format(zbUtil.Language("lang.10992"), m_detail.Prize));
		for (int i = 0; i < detail.CardList.Count; i++)
		{
			m_cards[i].style.backgroundImage = new StyleBackground(m_wndParent.GetIconById(detail.CardList[i]));
		}
		for (int j = 0; j < detail.AwardList.Count; j++)
		{
			zbItemData data = new zbItemData(detail.AwardList[j].Type, detail.AwardList[j].Tid, detail.AwardList[j].Count);
			m_targetItem.Refresh(data);
		}
		((VisualElement)m_lbStatus).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((detail.State != ECommonState.No) ? 1 : 0));
		((VisualElement)m_btnReceive).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(detail.State != ECommonState.Yes));
		m_spriteGetted.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(detail.State != ECommonState.End));
		for (int k = 0; k < 3; k++)
		{
			((VisualElement)m_spines[k]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(detail.State != ECommonState.Yes));
		}
	}

	private void OnReceive()
	{
		zbWsMsgHandler.Instance.Operate_ScratchCardTarget_Req(m_wndParent.m_scratchCardDetail.BaseInfo.UUID, m_detail.Index);
	}
}
