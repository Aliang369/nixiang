using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Proto.Types;
using UnityEngine.UIElements;

public class zbServantSubSkillHandBookWnd : zbBaseWnd
{
	private Button m_btnTouch;

	private Button m_btnClose;

	private Label m_lbTitle;

	private VisualElement m_content;

	private zbServantSkillCsvRecord[] m_records;

	private int m_lvLimit;

	protected override void OnEnable()
	{
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_content = UQueryExtensions.Q<VisualElement>(m_RootElement, "content", (string)null);
		m_records = zbDataConfig.Instance.servantSkillCsvTable.GetRecords().records.Values.Where((zbServantSkillCsvRecord r) => r.skillType == 0).ToArray();
		m_lvLimit = Extensions.Value<int>((IEnumerable<JToken>)zbDataConfig.Instance.servantBaseJsonTable.GetRecord()["subSkill"][(object)"levelLimit"]);
		AddDark();
		m_content.Clear();
		zbServantSkillCsvRecord[] records = m_records;
		foreach (zbServantSkillCsvRecord zbServantSkillCsvRecord2 in records)
		{
			zbServantSubSkill zbServantSubSkill2 = new zbServantSubSkill();
			zbServantSubSkill2.OnEnable(this);
			ServantSkillDetail servantSkillDetail = new ServantSkillDetail();
			servantSkillDetail.SkillId = zbServantSkillCsvRecord2.id;
			servantSkillDetail.Level = m_lvLimit;
			zbServantSubSkill2.Refresh(servantSkillDetail);
			((VisualElement)zbServantSubSkill2).style.marginBottom = StyleLength.op_Implicit(20f);
			((VisualElement)zbServantSubSkill2).style.marginLeft = StyleLength.op_Implicit(20f);
			((VisualElement)zbServantSubSkill2).style.marginRight = StyleLength.op_Implicit(25f);
			((VisualElement)zbServantSubSkill2).style.marginTop = StyleLength.op_Implicit(20f);
			m_content.Add((VisualElement)(object)zbServantSubSkill2);
		}
	}

	public override void Refresh()
	{
		base.Refresh();
	}

	public void OnClose()
	{
		Hide();
	}
}
