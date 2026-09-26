using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Types;
using UnityEngine.UIElements;

public class zbLegionPrivilegeWnd : zbBaseWnd
{
	private Button m_btnPre;

	private Button m_btnNext;

	private Label m_lbLv;

	private Label m_lbLvVal;

	private Label m_lbLvBar;

	private Label m_lbMemberCount;

	private Label m_lbMemberCountVal;

	private Label m_lbDonateCount;

	private Label m_lbDonateCountVal;

	private Label m_lbFuncUnLock;

	private Label m_lbFuncUnLockVal;

	private Label m_lbPro;

	private Label m_lbProVal;

	private Label m_lbProText;

	private Label m_lbLvPre;

	private Button m_btnTouch;

	private int m_curLv;

	private int m_curExp;

	protected override void OnEnable()
	{
		base.OnEnable();
		AddDark();
		m_btnPre = UQueryExtensions.Q<Button>(m_RootElement, "btn_pre", (string)null);
		m_btnNext = UQueryExtensions.Q<Button>(m_RootElement, "btn_next", (string)null);
		m_lbLv = UQueryExtensions.Q<Label>(m_RootElement, "lb_lv", (string)null);
		m_lbLvVal = UQueryExtensions.Q<Label>(m_RootElement, "lb_lvVal", (string)null);
		m_lbPro = UQueryExtensions.Q<Label>(m_RootElement, "lb_pro", (string)null);
		m_lbProVal = UQueryExtensions.Q<Label>(m_RootElement, "lb_proVal", (string)null);
		m_lbProText = UQueryExtensions.Q<Label>(m_RootElement, "lb_proText", (string)null);
		m_lbMemberCount = UQueryExtensions.Q<Label>(m_RootElement, "lb_memberCount", (string)null);
		m_lbMemberCountVal = UQueryExtensions.Q<Label>(m_RootElement, "lb_memberCountVal", (string)null);
		m_lbDonateCount = UQueryExtensions.Q<Label>(m_RootElement, "lb_donateCount", (string)null);
		m_lbDonateCountVal = UQueryExtensions.Q<Label>(m_RootElement, "lb_donateCountVal", (string)null);
		m_lbFuncUnLock = UQueryExtensions.Q<Label>(m_RootElement, "lb_funcUnlock", (string)null);
		m_lbFuncUnLockVal = UQueryExtensions.Q<Label>(m_RootElement, "lb_funcUnlockVal", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		LegionInfo legionInfo = zbSelf.Instance.legionHandler.legionInfo;
		m_curLv = legionInfo.LegionBaseInfo.Level;
		m_curExp = legionInfo.LegionSetInfo.LegionExp;
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnPre, (Action)OnPre, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnNext, (Action)OnNext, "", (TrickleDown)0);
	}

	public override void Refresh()
	{
		InitBylv(zbSelf.Instance.legionHandler.legionInfo.LegionBaseInfo.Level);
	}

	private void OnClose(ClickEvent evt)
	{
		Hide();
	}

	private void OnPre()
	{
		m_curLv = ((m_curLv == 1) ? 10 : (m_curLv - 1));
		InitBylv(m_curLv);
	}

	private void OnNext()
	{
		m_curLv = ((m_curLv == 10) ? 1 : (m_curLv + 1));
		InitBylv(m_curLv);
	}

	private void InitBylv(int lv)
	{
		((TextElement)m_lbLvVal).text = $"Lv.{lv}";
		zbLegionLevelCsvRecord recordByKey = zbDataConfig.Instance.legionLevelCsvTable.GetRecordByKey(lv);
		zbLegionUnlockCsvRecord value = zbDataConfig.Instance.legionUnlockCsvTable.GetRecords().records.FirstOrDefault((KeyValuePair<int, zbLegionUnlockCsvRecord> r) => r.Value.legionLvl == lv).Value;
		if (value != null && value.isShow > 0)
		{
			((TextElement)m_lbFuncUnLockVal).text = zbUtil.Language(value.name);
		}
		else
		{
			((TextElement)m_lbFuncUnLockVal).text = "/";
		}
		((TextElement)m_lbMemberCountVal).text = recordByKey.member.ToString();
		((TextElement)m_lbDonateCountVal).text = recordByKey.donateMax.ToString();
	}
}
