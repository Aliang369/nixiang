using System;
using Google.Protobuf.Collections;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbServantGetWnd : zbBaseWnd
{
	private Button m_btnTouch;

	private Button m_btnClose;

	private VisualElement m_iconSkillType;

	private Label m_lbName;

	private VisualElement m_iconSkill;

	private Label m_lbSkillDesc;

	private zbServantSubSkill[] m_subSkills = new zbServantSubSkill[6];

	private Label m_lbLv;

	private VisualElement m_mainAttr;

	private Label m_lbMainAttrKey;

	private Label m_lbMainAttrVal;

	private VisualElement[] m_subAttrs = (VisualElement[])(object)new VisualElement[5];

	private Label[] m_subAttrsKey = (Label[])(object)new Label[5];

	private Label[] m_subAttrsVal = (Label[])(object)new Label[5];

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
		m_mainAttr = UQueryExtensions.Q<VisualElement>(m_RootElement, "mainAttr", (string)null);
		m_lbMainAttrKey = UQueryExtensions.Q<Label>(m_mainAttr, "lb_key", (string)null);
		m_lbMainAttrVal = UQueryExtensions.Q<Label>(m_mainAttr, "lb_val", (string)null);
		m_lbLv = UQueryExtensions.Q<Label>(m_RootElement, "lb_servantLv", (string)null);
		for (int i = 0; i < 5; i++)
		{
			m_subAttrs[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "attr" + i, (string)null);
			m_subAttrsKey[i] = UQueryExtensions.Q<Label>(m_subAttrs[i], "lb_key", (string)null);
			m_subAttrsVal[i] = UQueryExtensions.Q<Label>(m_subAttrs[i], "lb_val", (string)null);
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
		zbAudioMgr.Instance.PlayAudio(zbAudioDefine.UI_AWARD);
		RefreshServant();
		RefreshTitle();
		RefreshSkill();
		RefreshSubSkill();
		RefreshAttr();
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
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		string skillTypeIconPathByType = zbResourceHelper.GetSkillTypeIconPathByType((ESkillType)m_record.skillType);
		m_iconSkillType.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(skillTypeIconPathByType));
		((TextElement)m_lbName).text = zbUtil.Language(m_record.name);
		Color val = zbUtil.ParseHexColor(zbSelf.Instance.servantHandler.GetColorByQuality(m_detail.Quality));
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
		for (int i = 0; i < 6; i++)
		{
			if (i < m_detail.SubSkillList.Count)
			{
				m_subSkills[i].Refresh(m_detail.SubSkillList[i]);
			}
			else
			{
				m_subSkills[i].Refresh(null);
			}
		}
	}

	private void RefreshAttr()
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		zbServantQualityCsvRecord recordByKey = zbDataConfig.Instance.servantQualityCsvTable.GetRecordByKey((int)m_detail.Quality);
		((TextElement)m_lbLv).text = "1/" + recordByKey.lvLimit;
		zbServantAttrCsvRecord recordByKey2 = zbDataConfig.Instance.servantAttrCsvTable.GetRecordByKey(m_record.mainAttrId);
		Color val = zbUtil.ParseHexColor(zbSelf.Instance.servantHandler.GetColorByQuality(m_detail.Quality));
		((VisualElement)m_lbMainAttrKey).style.color = StyleColor.op_Implicit(val);
		((TextElement)m_lbMainAttrKey).text = zbAttrHelper.GetAttrName((EAttrType)recordByKey2.attrType, recordByKey2.attr[0], recordByKey2.attr[1]);
		zbSelf.Instance.servantHandler.GetAttrValByQuality(m_detail.Quality, recordByKey2);
		((TextElement)m_lbMainAttrVal).text = zbSelf.Instance.servantHandler.FormatAttrVal(m_record.mainAttrId, m_detail.Quality, m_detail.Level);
		for (int i = 0; i < 5; i++)
		{
			bool flag = i < m_detail.SubAttrList.Count;
			m_subAttrs[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			if (flag)
			{
				RepeatedField<ServantAttrDetail> subAttrList = m_detail.SubAttrList;
				zbServantAttrCsvRecord recordByKey3 = zbDataConfig.Instance.servantAttrCsvTable.GetRecordByKey(subAttrList[i].AttrId);
				Color val2 = zbUtil.ParseHexColor(zbSelf.Instance.servantHandler.GetColorByQuality(subAttrList[i].Quality));
				((VisualElement)m_subAttrsKey[i]).style.color = StyleColor.op_Implicit(val2);
				((TextElement)m_subAttrsKey[i]).text = zbAttrHelper.GetAttrName((EAttrType)recordByKey3.attrType, recordByKey3.attr[0], recordByKey3.attr[1]);
				zbSelf.Instance.servantHandler.GetAttrValByQuality(subAttrList[i].Quality, recordByKey3);
				_ = m_detail.Level;
				((TextElement)m_subAttrsVal[i]).text = zbSelf.Instance.servantHandler.FormatAttrVal(subAttrList[i].AttrId, subAttrList[i].Quality, m_detail.Level);
			}
		}
	}

	public void OnClose()
	{
		Hide();
	}
}
