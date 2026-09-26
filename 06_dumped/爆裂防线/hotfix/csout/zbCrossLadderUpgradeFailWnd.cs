using System;
using UnityEngine.UIElements;

public class zbCrossLadderUpgradeFailWnd : zbBaseWnd
{
	private Button m_btnTouch;

	private zbLadderItem m_itemLadder;

	private Label m_lbScore;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnTouchTest, "", (TrickleDown)0);
		m_itemLadder = UQueryExtensions.Q<zbLadderItem>(m_RootElement, "ladder", (string)null);
		m_itemLadder.OnEnable(this);
		m_lbScore = UQueryExtensions.Q<Label>(m_RootElement, "lb_score", (string)null);
		AddDark();
	}

	public override void Refresh()
	{
		m_itemLadder.Refresh(zbServerData.sGameData.CrossLadderInfo.AchieveInfo.LadderId);
		((TextElement)m_lbScore).text = zbSelf.Instance.crossLadderHandler.GetScoreChangeDesc();
	}

	private void OnTouchTest()
	{
		Hide();
	}
}
