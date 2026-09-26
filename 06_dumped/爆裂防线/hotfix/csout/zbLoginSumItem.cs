using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine.UIElements;

internal class zbLoginSumItem : zbViewBaseItem
{
	private Label m_lbTitle;

	private zbItem[] m_items = new zbItem[6];

	private Button m_btnReceive;

	private VisualElement m_iconReceived;

	private Label m_lbNotAchieved;

	private OperateTargetDetail m_data;

	private LoginSumDetail m_detail;

	public zbLoginSumItem(VisualTreeAsset tAssets, zbLoginSumWnd wndParent)
	{
		((VisualElement)this).Add((VisualElement)(object)tAssets.Instantiate());
		m_lbTitle = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_title", (string)null);
		m_btnReceive = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_receive", (string)null);
		m_iconReceived = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon_received", (string)null);
		m_lbNotAchieved = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_notAchieved", (string)null);
		for (int i = 0; i < 5; i++)
		{
			m_items[i] = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, $"item{i}", (string)null);
			m_items[i].OnEnable(wndParent);
		}
		wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnReceive, (Action)OnTouch, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Init(OperateTargetDetail operateTargetDetail, LoginSumDetail loginSumDetail)
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		m_data = operateTargetDetail;
		m_detail = loginSumDetail;
		((TextElement)m_lbTitle).text = string.Format(zbUtil.Language("lang.10576"), m_data.Target);
		for (int i = 0; i < 5; i++)
		{
			bool flag = i < m_data.AwardList.Count;
			((VisualElement)m_items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			if (flag)
			{
				zbItemData data = new zbItemData(m_data.AwardList[i].Type, m_data.AwardList[i].Tid, m_data.AwardList[i].Count);
				m_items[i].Refresh(data);
			}
		}
		bool flag2 = m_data.State == ECommonState.Yes;
		bool flag3 = m_data.State == ECommonState.End;
		((VisualElement)m_btnReceive).visible = flag2;
		m_iconReceived.visible = flag3;
		((VisualElement)m_lbNotAchieved).visible = !flag2 && !flag3;
	}

	private void OnTouch()
	{
		zbWsMsgHandler.Instance.Operate_LoginSum_Req(m_detail.BaseInfo.UUID, m_data.Index);
	}
}
