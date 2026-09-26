using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbMechaDetailWnd : zbBaseWnd
{
	private Button m_btnTouch;

	private Button m_btnClose;

	private VisualElement m_spriteFrame;

	private VisualElement m_spriteMecha;

	private Label m_lbName;

	private VisualElement m_spriteSkill;

	private Label m_skillName;

	private Label m_skillDesc;

	private Button[] m_btnSkills = (Button[])(object)new Button[5];

	private VisualElement m_stars;

	private VisualElement[] m_starsIcon = (VisualElement[])(object)new VisualElement[5];

	private VisualElement[] m_nodeAttrs = (VisualElement[])(object)new VisualElement[4];

	private Label[] m_lbAttrKeys = (Label[])(object)new Label[4];

	private Label[] m_lbAttrValues = (Label[])(object)new Label[4];

	private Button[] m_btnAttrs = (Button[])(object)new Button[4];

	private Label m_lbDesc;

	private Button m_btnUpgrade;

	private Button m_btnFormation;

	private Button m_btnLeft;

	private Button m_btnRight;

	private VisualElement m_notifyUpgrade;

	private zbMechaCsvRecord m_record;

	private zbMechaCsvRecord[] m_records;

	private MechaDetail m_mechaDetail;

	private List<int> m_allSkillIds = new List<int>();

	private VisualElement m_mechaSkillsWrapper;

	private VisualElement[] m_mechaSkills = (VisualElement[])(object)new VisualElement[5];

	private Button[] m_btnsMechaSkills = (Button[])(object)new Button[5];

	private Button m_btnMask;

	private Label m_lbMechaSkill;

	protected override void OnEnable()
	{
		//IL_045f: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_spriteFrame = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_frame", (string)null);
		m_spriteMecha = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_icon", (string)null);
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_spriteSkill = UQueryExtensions.Q<VisualElement>(m_RootElement, "skill", (string)null);
		m_skillName = UQueryExtensions.Q<Label>(m_RootElement, "lb_skill", (string)null);
		m_skillDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_skill_desc", (string)null);
		m_mechaSkillsWrapper = UQueryExtensions.Q<VisualElement>(m_RootElement, "mechaSkills", (string)null);
		m_btnMask = UQueryExtensions.Q<Button>(m_RootElement, "btn_mask", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnMask, (Action)OnMask, "", (TrickleDown)0);
		m_lbMechaSkill = UQueryExtensions.Q<Label>(m_RootElement, "lb_mechaSkill", (string)null);
		m_stars = UQueryExtensions.Q<VisualElement>(m_RootElement, "stars", (string)null);
		for (int i = 0; i < 5; i++)
		{
			m_starsIcon[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "star" + i, (string)null);
			m_btnSkills[i] = UQueryExtensions.Q<Button>(m_RootElement, "btn_skill" + i, (string)null);
			int curIndex = i;
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSkills[i], (Action<ClickEvent>)delegate(ClickEvent evt)
			{
				OnSkills(evt, curIndex);
			}, "", (TrickleDown)0);
			m_mechaSkills[i] = UQueryExtensions.Q<VisualElement>(m_mechaSkillsWrapper, "skill" + i, (string)null);
			m_btnsMechaSkills[i] = UQueryExtensions.Q<Button>(m_mechaSkillsWrapper, "btn_skill" + i, (string)null);
			int tempIndex = i;
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnsMechaSkills[i], (Action)delegate
			{
				OnMechaSkills(tempIndex);
			}, "", (TrickleDown)0);
		}
		for (int num = 0; num < 4; num++)
		{
			m_nodeAttrs[num] = UQueryExtensions.Q<VisualElement>(m_RootElement, "attr" + num, (string)null);
			m_lbAttrKeys[num] = UQueryExtensions.Q<Label>(m_nodeAttrs[num], "lb_key", (string)null);
			m_lbAttrValues[num] = UQueryExtensions.Q<Label>(m_nodeAttrs[num], "lb_value", (string)null);
			m_btnAttrs[num] = UQueryExtensions.Q<Button>(m_nodeAttrs[num], "btn_attr", (string)null);
			int curIndex2 = num;
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAttrs[num], (Action<ClickEvent>)delegate(ClickEvent evt)
			{
				OnAttrs(evt, curIndex2);
			}, "", (TrickleDown)0);
		}
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_btnUpgrade = UQueryExtensions.Q<Button>(m_RootElement, "btn_upgrade", (string)null);
		m_btnFormation = UQueryExtensions.Q<Button>(m_RootElement, "btn_formation", (string)null);
		m_btnLeft = UQueryExtensions.Q<Button>(m_RootElement, "btn_left", (string)null);
		m_btnRight = UQueryExtensions.Q<Button>(m_RootElement, "btn_right", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnUpgrade, (Action<ClickEvent>)OnUpgrade, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnFormation, (Action<ClickEvent>)OnFormation, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnLeft, (Action)OnLeft, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRight, (Action)OnRight, "", (TrickleDown)0);
		m_notifyUpgrade = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnUpgrade, (Vector2?)new Vector2(-6f, -6f), 1f, true);
		AddDark();
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_HomelandInfo, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Hero, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Money, OnEventUpdate);
		m_records = zbSelf.Instance.mechaHandler.GetAllMechaRecords();
	}

	public void RefreshCustom(zbMechaCsvRecord record)
	{
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		m_record = record;
		m_mechaDetail = zbSelf.Instance.mechaHandler.GetMechaDetail(record.id);
		RefreshDetail();
		RefreshSkills();
		RefreshAttrList();
		if (m_mechaDetail == null)
		{
			((TextElement)m_btnUpgrade).text = zbUtil.Language("lang.10786");
			((TextElement)m_btnFormation).text = zbUtil.Language("lang.10821");
			((VisualElement)m_btnFormation).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btnGray_280x80.png"));
		}
		else
		{
			((TextElement)m_btnUpgrade).text = zbUtil.Language("lang.10787");
			((TextElement)m_btnFormation).text = ((zbSelf.Instance.GetMechaId() == m_record.id) ? zbUtil.Language("lang.10822") : zbUtil.Language("lang.10821"));
			if (zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Mecha) || zbSelf.Instance.GetMechaId() == m_record.id)
			{
				((VisualElement)m_btnFormation).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btnGray_280x80.png"));
			}
			else
			{
				((VisualElement)m_btnFormation).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_280_80_orange.png"));
			}
		}
		m_notifyUpgrade.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.mechaHandler.HasMechaNotify(record.id)));
	}

	private void RefreshDetail()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		string heroFrameTex = zbResourceHelper.GetHeroFrameTex((EQuality)m_record.quality);
		m_spriteFrame.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(heroFrameTex));
		zbMechaAvatarCsvRecord recordByKey = zbDataConfig.Instance.mechaAvatarCsvTable.GetRecordByKey(m_record.avatarId);
		if (recordByKey != null)
		{
			m_spriteMecha.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.icon));
		}
		((TextElement)m_lbName).text = m_record.name;
		((TextElement)m_lbDesc).text = m_record.desc;
		zbMechaSkillCsvRecord recordByKey2 = zbDataConfig.Instance.mechaSkillCsvTable.GetRecordByKey(m_record.skillList[0]);
		if (recordByKey2 != null)
		{
			m_spriteSkill.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey2.icon));
			((TextElement)m_skillName).text = recordByKey2.name;
		}
		((TextElement)m_skillDesc).text = zbSelf.Instance.mechaHandler.GetCurMechaSkillDesc(m_record.skillList[0]);
		m_allSkillIds.Clear();
		m_allSkillIds = m_record.skillList.ToList();
	}

	private void RefreshSkills()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		bool flag = m_mechaDetail != null;
		int num = (flag ? m_mechaDetail.Star : 0);
		m_stars.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		if (flag)
		{
			for (int i = 0; i < 5; i++)
			{
				bool flag2 = i < num;
				m_starsIcon[i].style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(flag2 ? "ui/image/suitGun/star_1.png" : "ui/image/suitGun/star_0.png"));
			}
		}
		for (int j = 0; j < 5; j++)
		{
			zbMechaSkillCsvRecord recordByKey = zbDataConfig.Instance.mechaSkillCsvTable.GetRecordByKey(m_record.skillList[j]);
			m_mechaSkills[j].style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.icon));
		}
	}

	private void RefreshAttrList()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 1; i < 4; i++)
		{
			m_nodeAttrs[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		((TextElement)m_lbAttrValues[0]).text = zbSelf.Instance.mechaHandler.GetMechaTypeString(m_record.id);
		if (m_mechaDetail != null)
		{
			int num = 1;
			foreach (BattleMechaAttrDetail attr in m_mechaDetail.AttrList)
			{
				if (m_nodeAttrs[num] != null)
				{
					m_nodeAttrs[num].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
					if (attr.Type == EAttrSkillType.Damage)
					{
						((TextElement)m_lbAttrKeys[num]).text = zbUtil.Language("lang.10797");
						((TextElement)m_lbAttrValues[num]).text = attr.Value.ToString();
					}
					else if (attr.Type == EAttrSkillType.Duration)
					{
						((TextElement)m_lbAttrKeys[num]).text = zbUtil.Language("lang.10796");
						((TextElement)m_lbAttrValues[num]).text = attr.Value + "s";
					}
				}
				num++;
			}
			m_nodeAttrs[num].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((TextElement)m_lbAttrKeys[num]).text = zbUtil.Language("lang.10798");
			((TextElement)m_lbAttrValues[num]).text = "+" + m_mechaDetail.Attack;
		}
		else
		{
			m_nodeAttrs[1].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((TextElement)m_lbAttrKeys[1]).text = zbUtil.Language("lang.10797");
			float num2 = (float)zbSelf.Instance.GetFightValue() * m_record.damageRate * m_record.levelDamageRate;
			((TextElement)m_lbAttrValues[1]).text = Mathf.Floor(num2).ToString();
		}
	}

	private void OnSkills(ClickEvent evt, int index)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		if (m_allSkillIds[index] > 0)
		{
			zbMechaSkillCsvRecord recordByKey = zbDataConfig.Instance.mechaSkillCsvTable.GetRecordByKey(m_allSkillIds[index]);
			if (recordByKey != null)
			{
				IEventHandler target = ((EventBase)evt).target;
				Vector2 pos = VisualElementExtensions.LocalToWorld((VisualElement)(object)((target is VisualElement) ? target : null), Vector2.zero);
				pos.y += 100f;
				zbUIMgr.Instance.ShowTipMod(zbSelf.Instance.mechaHandler.GetCurMechaSkillDesc(recordByKey.id), pos);
			}
		}
	}

	private void OnAttrs(ClickEvent evt, int index)
	{
	}

	private void OnUpgrade(ClickEvent evt)
	{
		zbUIMgr.Instance.ShowMecha(m_record);
		OnClose();
	}

	private void OnFormation(ClickEvent evt)
	{
		if (!zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.Mecha) && m_mechaDetail != null)
		{
			zbWsMsgHandler.Instance.Mecha_Set_Req(m_record.id);
			OnClose();
		}
	}

	private void OnClose()
	{
		OnMask();
		Hide();
	}

	private void OnRight()
	{
		SwitchHero(1);
	}

	private void OnLeft()
	{
		SwitchHero(-1);
	}

	private void SwitchHero(int direction)
	{
		OnMask();
		if (m_record == null)
		{
			return;
		}
		int num = Array.FindIndex(m_records, (zbMechaCsvRecord r) => r.id == m_record.id);
		int num2 = m_records.Length;
		if (num >= 0 && num2 > 0)
		{
			int num3 = (num + direction + num2) % num2;
			if (m_records[num3] != null)
			{
				RefreshCustom(m_records[num3]);
			}
		}
	}

	private void OnMechaSkills(int index)
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		string text = ((index == 0) ? GetMechaPowerSkillDescByIndex(index) : (zbUtil.Language("lang.11477") + GetMechaBaseSkillDescByIndex(index) + "\n" + string.Format(zbUtil.Language("lang.11478"), index + 1) + GetMechaPowerSkillDescByIndex(index)));
		((TextElement)m_lbMechaSkill).text = text;
		((VisualElement)m_btnMask).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
	}

	private string GetMechaBaseSkillDescByIndex(int index)
	{
		int num = 10011;
		switch (index)
		{
		case 0:
			num = m_record.skillList[0];
			break;
		case 1:
			num = m_record.skillList[1];
			break;
		case 2:
			num = m_record.skillList[2];
			break;
		case 3:
			num = m_record.skillList[3];
			break;
		case 4:
			num = m_record.skillList[4];
			break;
		}
		zbDataConfig.Instance.mechaSkillCsvTable.GetRecordByKey(num);
		return zbSelf.Instance.mechaHandler.GetMechaSkillDesc(num);
	}

	private string GetMechaPowerSkillDescByIndex(int index)
	{
		int num = 10011;
		switch (index)
		{
		case 0:
			num = m_record.skillList[0];
			break;
		case 1:
			num = m_record.skill2;
			break;
		case 2:
			num = m_record.skill3;
			break;
		case 3:
			num = m_record.skill4;
			break;
		case 4:
			num = m_record.skill5;
			break;
		}
		zbDataConfig.Instance.mechaSkillCsvTable.GetRecordByKey(num);
		return zbSelf.Instance.mechaHandler.GetMechaSkillDesc(num);
	}

	private void OnMask()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_btnMask).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
	}

	private void OnEventUpdate(object obj)
	{
		RefreshCustom(m_record);
	}
}
