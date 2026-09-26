using System;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbServantTabModule
{
	private zbServantWnd m_wndParent;

	private VisualElement m_RootElement;

	private Button m_btnAdd;

	private Button m_btnReSelect;

	private VisualElement m_panel0;

	private VisualElement m_selectedServant;

	private VisualElement m_unSelectServant;

	private VisualElement m_panel1;

	private VisualElement m_subSkill;

	private zbServantSubSkill[] m_subSkills = new zbServantSubSkill[6];

	public string m_costServantId;

	private zbServantItem m_costServantItem;

	private zbServantSubSkill[] m_costServantSubSkills = new zbServantSubSkill[6];

	private Button m_btnUpgradeQuality;

	private VisualElement m_process;

	private Sprite m_processBlock0;

	private Sprite m_processBlock1;

	public void OnEnable(zbServantWnd wndParent, VisualElement root)
	{
		m_wndParent = wndParent;
		m_RootElement = root;
		m_btnReSelect = UQueryExtensions.Q<Button>(m_RootElement, "btn_reSelect", (string)null);
		m_btnAdd = UQueryExtensions.Q<Button>(m_RootElement, "btn_add", (string)null);
		m_panel0 = UQueryExtensions.Q<VisualElement>(m_RootElement, "upgradePanel0", (string)null);
		m_selectedServant = UQueryExtensions.Q<VisualElement>(m_panel0, "selectedServant", (string)null);
		m_unSelectServant = UQueryExtensions.Q<VisualElement>(m_panel0, "unSelectServant", (string)null);
		m_panel1 = UQueryExtensions.Q<VisualElement>(m_RootElement, "upgradePanel1", (string)null);
		m_subSkill = UQueryExtensions.Q<VisualElement>(m_RootElement, "subSkill", (string)null);
		m_costServantItem = UQueryExtensions.Q<zbServantItem>(m_RootElement, "costServant", (string)null);
		m_costServantItem.OnEnable(m_wndParent, needTouch: false);
		for (int i = 0; i < 6; i++)
		{
			m_subSkills[i] = UQueryExtensions.Q<zbServantSubSkill>(m_subSkill, "subSkill" + i, (string)null);
			m_subSkills[i].OnEnable(m_wndParent);
			m_costServantSubSkills[i] = UQueryExtensions.Q<zbServantSubSkill>(m_selectedServant, "subSkill" + i, (string)null);
			m_costServantSubSkills[i].OnEnable(m_wndParent);
		}
		m_btnUpgradeQuality = UQueryExtensions.Q<Button>(m_RootElement, "btn_upgradeQuality", (string)null);
		m_process = UQueryExtensions.Q<VisualElement>(m_RootElement, "process", (string)null);
		m_processBlock0 = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/servant/servant_process0.png");
		m_processBlock1 = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/servant/servant_process1.png");
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnUpgradeQuality, (Action)OnUpgradeQuality, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAdd, (Action)OnSelectServant, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnReSelect, (Action)OnReSelect, "", (TrickleDown)0);
	}

	public void Refresh()
	{
		if (m_wndParent.m_detail != null)
		{
			m_costServantId = null;
			RefreshSubSkills();
			RefreshSelectServant();
			RefreshProcess();
		}
	}

	public void RefreshSelectServant()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		m_panel0.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_panel1.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		bool flag = m_costServantId != null;
		m_selectedServant.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		m_unSelectServant.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag ? 1 : 0));
		if (flag)
		{
			ServantDetail servantDetail = zbSelf.Instance.servantHandler.GetServantDetail(m_costServantId);
			m_costServantItem.Refresh(servantDetail);
			for (int i = 0; i < 6; i++)
			{
				bool flag2 = i < servantDetail.SubSkillList.Count;
				m_costServantSubSkills[i].Refresh(flag2 ? servantDetail.SubSkillList[i] : null);
			}
		}
	}

	private void RefreshSubSkills()
	{
		for (int i = 0; i < 6; i++)
		{
			bool flag = i < m_wndParent.m_detail.SubSkillList.Count;
			m_subSkills[i].Refresh(flag ? m_wndParent.m_detail.SubSkillList[i] : null);
		}
	}

	private void RefreshProcess()
	{
		m_process.Clear();
		zbServantQualityCsvRecord recordByKey = zbDataConfig.Instance.servantQualityCsvTable.GetRecordByKey((int)m_wndParent.m_detail.Quality);
		for (int i = 0; i < Mathf.Max(1, recordByKey.costNum); i++)
		{
			bool isUpgraded = i < m_wndParent.m_detail.QualityCount;
			VisualElement val = CreateProcessBlock(isUpgraded);
			m_process.Add(val);
		}
	}

	private VisualElement CreateProcessBlock(bool isUpgraded)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		VisualElement val = new VisualElement();
		val.style.backgroundImage = new StyleBackground(m_processBlock0);
		val.style.flexGrow = StyleFloat.op_Implicit(1f);
		if (isUpgraded)
		{
			VisualElement val2 = new VisualElement();
			val2.style.backgroundImage = new StyleBackground(m_processBlock1);
			val2.style.flexGrow = StyleFloat.op_Implicit(1f);
			val.Add(val2);
		}
		return val;
	}

	private void OnSelectServant()
	{
		zbUIMgr.Instance.ShowServantSelect();
	}

	private void OnReSelect()
	{
		if (m_costServantId != null)
		{
			(zbUIMgr.Instance.GetWndById(EWndId.ServantSelect) as zbServantSelectWnd).m_selectedServantId = m_costServantId;
			zbUIMgr.Instance.ShowServantSelect();
		}
	}

	private void OnUpgradeQuality()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (m_wndParent.m_detail != null && (int)m_panel0.resolvedStyle.display == 0 && m_costServantId != null)
		{
			zbWsMsgHandler.Instance.Servant_UpgradeQuality_Req(m_wndParent.m_detail.ServantId, m_costServantId);
		}
	}
}
