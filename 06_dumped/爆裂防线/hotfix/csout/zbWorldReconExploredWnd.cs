using System;
using Proto.Types;
using UnityEngine.UIElements;

public class zbWorldReconExploredWnd : zbBaseWnd
{
	private Label m_lbName;

	private Label m_lbDesc;

	private Label m_lbTeamChoose;

	private Button m_btnConfirm;

	private Button m_btnClose;

	private Button m_btnTouch;

	private zbItem[] m_awards = new zbItem[4];

	private AwardDetail[] m_awardList;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnConfirm = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_lbTeamChoose = UQueryExtensions.Q<Label>(m_RootElement, "lb_teamChoose", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		for (int i = 0; i < 4; i++)
		{
			m_awards[i] = UQueryExtensions.Q<zbItem>(m_RootElement, $"award{i}", (string)null);
			m_awards[i].OnEnable(this);
		}
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnConfirm, (Action)OnClose, "", (TrickleDown)0);
		AddDark();
	}

	public void RefreshCustom(AwardDetail[] awardList)
	{
		m_awardList = awardList;
		InitAward();
	}

	private void InitAward()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 4; i++)
		{
			bool flag = i < m_awardList.Length;
			((VisualElement)m_awards[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			if (flag)
			{
				zbItemData data = new zbItemData(m_awardList[i].Type, m_awardList[i].Tid, m_awardList[i].Count);
				m_awards[i].Refresh(data);
			}
		}
	}

	private void OnClose()
	{
		zbUIMgr.Instance.ShowAward(m_awardList, OnCallBack);
		Hide();
	}

	private void OnCallBack(bool isOK)
	{
		if (zbSelf.Instance.worldHandler.HasWorldReconExploredNotify(1))
		{
			zbWsMsgHandler.Instance.World_ReconExplored_Req(1);
		}
		else if (zbSelf.Instance.worldHandler.HasWorldReconExploredNotify(2))
		{
			zbWsMsgHandler.Instance.World_ReconExplored_Req(2);
		}
	}
}
