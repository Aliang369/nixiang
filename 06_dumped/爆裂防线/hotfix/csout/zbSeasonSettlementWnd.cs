using System;
using UnityEngine.UIElements;

public class zbSeasonSettlementWnd : zbTabWnd
{
	private Label m_lbTitle;

	private VisualElement[] m_layouts = (VisualElement[])(object)new VisualElement[3];

	private Button[] m_btns = (Button[])(object)new Button[3];

	private Button m_btnBack;

	private VisualElement m_selectTab;

	private zbSeasonSettlementTabHonor m_moduleHonor;

	private zbSeasonSettlementTabRewardPreview m_moduleRewardPreivew;

	private zbSeasonSettlementTabHonorPreview m_moduleHonorPreview;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_selectTab = UQueryExtensions.Q<VisualElement>(m_RootElement, "selectTab", (string)null);
		for (int i = 0; i < 3; i++)
		{
			m_layouts[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "panel" + i, (string)null);
			m_btns[i] = UQueryExtensions.Q<Button>(m_RootElement, "tab_btn" + i, (string)null);
		}
		m_btnBack = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBack, (Action)OnClose, "", (TrickleDown)0);
		InitTab(m_btns, m_layouts, null);
	}

	public void RefreshCustom()
	{
	}

	public override void DoSelectTab()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		base.DoSelectTab();
		switch (m_currTab)
		{
		case 0:
			m_selectTab.style.left = StyleLength.op_Implicit(new Length(0f, (LengthUnit)0));
			if (m_moduleHonor == null)
			{
				m_moduleHonor = new zbSeasonSettlementTabHonor();
				m_moduleHonor.OnEnable(this, m_layouts[0]);
			}
			m_moduleHonor.Refresh();
			break;
		case 1:
			m_selectTab.style.left = StyleLength.op_Implicit(new Length(240f, (LengthUnit)0));
			if (m_moduleRewardPreivew == null)
			{
				m_moduleRewardPreivew = new zbSeasonSettlementTabRewardPreview();
				m_moduleRewardPreivew.OnEnable(this, m_layouts[1]);
			}
			m_moduleRewardPreivew.Refresh();
			break;
		case 2:
			m_selectTab.style.left = StyleLength.op_Implicit(new Length(480f, (LengthUnit)0));
			if (m_moduleHonorPreview == null)
			{
				m_moduleHonorPreview = new zbSeasonSettlementTabHonorPreview();
				m_moduleHonorPreview.OnEnable(this, m_layouts[2]);
			}
			m_moduleHonorPreview.Refresh();
			break;
		}
	}

	public void OnClose()
	{
		Hide();
	}
}
