using System;
using UnityEngine.UIElements;

public class zbSeasonSettlementTabHonorPreview
{
	private zbSeasonSettlementWnd m_wndParent;

	private VisualElement m_RootElement;

	private VisualElement[] m_settleAwards = (VisualElement[])(object)new VisualElement[4];

	private Button[] m_btnsSettleAward = (Button[])(object)new Button[4];

	public void OnEnable(zbSeasonSettlementWnd wndParent, VisualElement root)
	{
		m_wndParent = wndParent;
		m_RootElement = root;
		for (int i = 0; i < 4; i++)
		{
			m_settleAwards[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "settleAward" + i, (string)null);
			m_btnsSettleAward[i] = UQueryExtensions.Q<Button>(m_settleAwards[i], "btn_touch", (string)null);
			int tempIndex = i;
			m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnsSettleAward[i], (Action)delegate
			{
				OnSettlePreview(tempIndex);
			}, "", (TrickleDown)0);
		}
	}

	public void Refresh()
	{
	}

	private void OnSettlePreview(int index)
	{
		zbUIMgr.Instance.ShowSeasonSettleAwardPreview();
	}
}
