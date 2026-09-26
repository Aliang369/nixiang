using System;
using UnityEngine.UIElements;

public class zbLegionWarBebackWnd : zbBaseWnd
{
	public class BossChallengeInfo
	{
		public string name;

		public int[][] awardList;

		public int id;
	}

	private Button m_btnBack;

	protected override void OnEnable()
	{
		base.OnEnable();
		AddDark();
		m_btnBack = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBack, (Action)OnClose, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_LegionWarRest, OnUpdateEventLegionBossAward);
	}

	public override void Refresh()
	{
	}

	private void OnUpdateEventLegionBossAward(object data)
	{
		Hide();
	}

	private void OnClose()
	{
		zbWsMsgHandler.Instance.LegionWar_Rest_Req();
	}
}
