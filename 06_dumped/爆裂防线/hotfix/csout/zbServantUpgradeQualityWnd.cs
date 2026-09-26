using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbServantUpgradeQualityWnd : zbBaseWnd
{
	private Button m_btnTouch;

	private Button m_btnClose;

	private VisualElement m_iconSkillType;

	private Label m_lbName;

	private VisualElement m_iconSkill;

	private Label m_lbSkillDesc;

	private zbServantSubSkill[] m_subSkills = new zbServantSubSkill[6];

	private Label m_lbCurLv;

	private Label m_lbNextLv;

	private VisualElement m_mainAttr;

	private Label m_mainAttrKey;

	private Label m_mainAttrValLeft;

	private Label m_mainAttrValRight;

	private ServantDetail m_detail;

	public zbServantCsvRecord m_record;

	private SkeletonAnimation m_servantAnimation;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_servantAnimation = ((Component)m_GameObjectSceneRoot.transform.Find("servant")).GetComponent<SkeletonAnimation>();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_iconSkillType = UQueryExtensions.Q<VisualElement>(m_RootElement, "icon_skillType", (string)null);
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_iconSkill = UQueryExtensions.Q<VisualElement>(m_RootElement, "icon_skill", (string)null);
		m_lbSkillDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_skillDesc", (string)null);
		m_lbCurLv = UQueryExtensions.Q<Label>(m_RootElement, "lb_curLv", (string)null);
		m_lbNextLv = UQueryExtensions.Q<Label>(m_RootElement, "lb_nextLv", (string)null);
		m_mainAttr = UQueryExtensions.Q<VisualElement>(m_RootElement, "mainAttr", (string)null);
		m_mainAttrKey = UQueryExtensions.Q<Label>(m_mainAttr, "lb_key", (string)null);
		m_mainAttrValLeft = UQueryExtensions.Q<Label>(m_mainAttr, "lb_val", (string)null);
		m_mainAttrValRight = UQueryExtensions.Q<Label>(m_mainAttr, "lb_nextVal", (string)null);
		for (int i = 0; i < 5; i++)
		{
		}
		for (int j = 0; j < 6; j++)
		{
			m_subSkills[j] = UQueryExtensions.Q<zbServantSubSkill>(m_RootElement, "subSkill" + j, (string)null);
			m_subSkills[j].OnEnable(this);
		}
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
	}

	public void RefreshCustom(ServantDetail detail)
	{
		m_detail = detail;
		m_record = zbDataConfig.Instance.servantCsvTable.GetRecordByKey(m_detail.ServantTid);
		RefreshServant();
		RefreshTitle();
		RefreshSkill();
		RefreshSubSkill();
		RefreshAttrs();
	}

	private void RefreshServant()
	{
		if (m_detail != null)
		{
			zbServantCsvRecord servantRecord = zbSelf.Instance.servantHandler.GetServantRecord(m_detail.ServantTid);
			string spinePath = zbDataConfig.Instance.servantAvatarCsvTable.GetRecordByKey(servantRecord.avatarId).frontBone + "_SkeletonData.asset";
			zbUtil.RefreshSpine(m_servantAnimation, spinePath, "idle", loop: true);
		}
	}

	private void RefreshTitle()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		ServantDetail servantDetail = zbSelf.Instance.servantHandler.GetServantDetail(m_detail.ServantId);
		string skillTypeIconPathByType = zbResourceHelper.GetSkillTypeIconPathByType((ESkillType)m_record.skillType);
		m_iconSkillType.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(skillTypeIconPathByType));
		((TextElement)m_lbName).text = zbUtil.Language(m_record.name);
		Color val = zbUtil.ParseHexColor(zbSelf.Instance.servantHandler.GetColorByQuality(servantDetail.Quality));
		((VisualElement)m_lbName).style.color = StyleColor.op_Implicit(val);
	}

	private void RefreshSkill()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		int skillId = m_record.skillId;
		zbServantSkillCsvRecord recordByKey = zbDataConfig.Instance.servantSkillCsvTable.GetRecordByKey(skillId);
		m_iconSkill.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.icon));
		((TextElement)m_lbSkillDesc).text = string.Format(zbUtil.Language(recordByKey.desc), recordByKey.baseVar + recordByKey.growthVar * (float)m_detail.MainSkill.Level);
	}

	private void RefreshSubSkill()
	{
		ServantDetail servantDetail = zbSelf.Instance.servantHandler.GetServantDetail(m_detail.ServantId);
		HashSet<int> hashSet = ((IEnumerable<ServantSkillDetail>)m_detail.SubSkillList).Select((ServantSkillDetail s) => s.SkillId).ToHashSet();
		for (int num = 0; num < 6; num++)
		{
			if (num < servantDetail.SubSkillList.Count)
			{
				ServantSkillDetail servantSkillDetail = servantDetail.SubSkillList[num];
				bool isNewAdded = !hashSet.Contains(servantSkillDetail.SkillId);
				m_subSkills[num].Refresh(servantSkillDetail, isNewAdded);
			}
			else
			{
				m_subSkills[num].Refresh(null);
			}
		}
	}

	private void RefreshAttrs()
	{
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		zbServantQualityCsvRecord recordByKey = zbDataConfig.Instance.servantQualityCsvTable.GetRecordByKey((int)m_detail.Quality);
		((TextElement)m_lbCurLv).text = recordByKey.lvLimit.ToString();
		zbServantQualityCsvRecord recordByKey2 = zbDataConfig.Instance.servantQualityCsvTable.GetRecordByKey((int)(m_detail.Quality + 1));
		string text = "Max";
		if (recordByKey2 != null)
		{
			text = recordByKey2.lvLimit.ToString();
		}
		((TextElement)m_lbNextLv).text = text;
		zbServantAttrCsvRecord recordByKey3 = zbDataConfig.Instance.servantAttrCsvTable.GetRecordByKey(m_record.mainAttrId);
		Color val = zbUtil.ParseHexColor(zbSelf.Instance.servantHandler.GetColorByQuality(m_detail.Quality));
		((VisualElement)m_mainAttrKey).style.color = StyleColor.op_Implicit(val);
		((TextElement)m_mainAttrKey).text = zbAttrHelper.GetAttrName((EAttrType)recordByKey3.attrType, recordByKey3.attr[0], recordByKey3.attr[1]);
		zbSelf.Instance.servantHandler.GetAttrValByQuality(m_detail.Quality, recordByKey3);
		_ = m_detail.Level;
		((TextElement)m_mainAttrValLeft).text = zbSelf.Instance.servantHandler.FormatAttrVal(m_record.mainAttrId, m_detail.Quality, m_detail.Level);
		_ = recordByKey.lvLimit;
		_ = m_detail.Level;
		_ = m_detail.Quality;
		_ = zbDataConfig.Instance.servantQualityCsvTable.GetRecords().records.Count;
		zbSelf.Instance.servantHandler.GetAttrValByQuality(m_detail.Quality, recordByKey3);
		_ = m_detail.Level;
		((TextElement)m_mainAttrValRight).text = zbSelf.Instance.servantHandler.FormatAttrVal(m_record.mainAttrId, m_detail.Quality + 1, m_detail.Level);
	}

	public void OnClose()
	{
		Hide();
	}
}
