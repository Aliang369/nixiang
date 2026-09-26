using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbArenaBottom : zbViewBaseItem
{
	public class UxmlFactory : UxmlFactory<zbArenaBottom>
	{
	}

	private zbBaseWnd m_wndParent;

	private SpriteRenderer m_spriteFoothold;

	private zbPlayerAvatar m_spineAvatar;

	private Label m_lbAllDamage;

	private Label m_lbDiamond;

	[NonSerialized]
	public Button m_btnEdit;

	private Button m_btnAvatar;

	private Label m_lbFree;

	private Label m_lbFootholdEdit;

	private Label m_lbPower;

	private VisualElement m_notify;

	private VisualElement[] m_nodeDetail = (VisualElement[])(object)new VisualElement[5];

	private TemplateContainer[] tb_skillMod = (TemplateContainer[])(object)new TemplateContainer[5];

	private zbPieProgress[] m_skillOutProgress = new zbPieProgress[5];

	private zbPieProgress[] m_skillInProgress = new zbPieProgress[5];

	private VisualElement[] m_skillIcon = (VisualElement[])(object)new VisualElement[5];

	private Label[] m_lbSkillLevel = (Label[])(object)new Label[5];

	private Button[] m_btnSkill = (Button[])(object)new Button[5];

	private List<SkillDetail> heroList = new List<SkillDetail>();

	public zbArenaBottom()
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/arena/arenaBottom.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
	}

	public void OnEnable(zbBaseWnd wndParent, SpriteRenderer foothold = null, zbPlayerAvatar avatar = null)
	{
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		m_spriteFoothold = foothold;
		m_spineAvatar = avatar;
		m_lbAllDamage = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_allDamages", (string)null);
		m_lbDiamond = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_diamond", (string)null);
		m_lbFree = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_free", (string)null);
		m_lbFootholdEdit = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_footholdEdit", (string)null);
		m_lbPower = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_power", (string)null);
		for (int i = 0; i < 5; i++)
		{
			tb_skillMod[i] = UQueryExtensions.Q<TemplateContainer>((VisualElement)(object)this, "skill" + (i + 1), (string)null);
			m_nodeDetail[i] = UQueryExtensions.Q<VisualElement>((VisualElement)(object)tb_skillMod[i], "nodeDetail", (string)null);
			m_skillOutProgress[i] = UQueryExtensions.Q<zbPieProgress>((VisualElement)(object)tb_skillMod[i], "outProgress", (string)null);
			m_skillOutProgress[i].progress = 100f;
			m_skillInProgress[i] = UQueryExtensions.Q<zbPieProgress>((VisualElement)(object)tb_skillMod[i], "inProgress", (string)null);
			m_skillInProgress[i].progress = 100f;
			m_skillIcon[i] = UQueryExtensions.Q<VisualElement>((VisualElement)(object)tb_skillMod[i], "skillIcon", (string)null);
			m_lbSkillLevel[i] = UQueryExtensions.Q<Label>((VisualElement)(object)tb_skillMod[i], "lb_skill", (string)null);
			m_btnSkill[i] = UQueryExtensions.Q<Button>((VisualElement)(object)tb_skillMod[i], "btn_skill", (string)null);
			int curIndex = i + 1;
			m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSkill[i], (Action<ClickEvent>)delegate(ClickEvent evt)
			{
				OnSkillClick(evt, curIndex);
			}, "", (TrickleDown)0);
		}
		((TextElement)m_lbDiamond).text = zbSelf.Instance.pvpHandler.modResetDiamond.ToString();
		m_btnEdit = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_edit", (string)null);
		m_btnAvatar = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_avatar", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnEdit, (Action)OnEdit, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAvatar, (Action)OnFoothold, "", (TrickleDown)0);
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnAvatar, (Vector2?)new Vector2(5f, 2f), 1f, true);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
		wndParent.m_gameEevent.RegisterEvent(EGameEvent.Event_PvpSet_ModSubmit, OnEventUpdate);
	}

	public void Refresh()
	{
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)m_spineAvatar != (Object)null)
		{
			m_spineAvatar.RefreshCustom(zbSelf.Instance.GetAvatarId(), zbSelf.Instance.GetGunId());
		}
		RoleBaseInfo roleBaseInfo = zbServerData.sGameData.RoleBaseInfo;
		if (roleBaseInfo != null && roleBaseInfo.FootholdId > 0)
		{
			zbFootholdCsvRecord recordByKey = zbDataConfig.Instance.footholdCsvTable.GetRecordByKey(zbServerData.sGameData.RoleBaseInfo.FootholdId);
			if ((Object)(object)m_spriteFoothold != (Object)null)
			{
				m_spriteFoothold.sprite = zbAssetLoader.Instance.LoadSpriteAsset(recordByKey.icon);
			}
			((TextElement)m_lbFootholdEdit).text = recordByKey.name;
		}
		((VisualElement)m_lbDiamond).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbServerData.sGameData.PvpInfo.SetCount == 0));
		((VisualElement)m_lbFree).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((zbServerData.sGameData.PvpInfo.SetCount != 0) ? 1 : 0));
		string text = (zbSelf.Instance.guardHandler.GetAllDamageUp() * 100f).ToString("F0");
		string text2 = zbUtil.AddHighlightStyle("+" + text + "%", "ffe400", 22);
		((TextElement)m_lbAllDamage).text = zbUtil.Language("lang.10862") + " " + text2;
		((VisualElement)m_lbAllDamage).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		((TextElement)m_lbPower).text = zbUtil.ConvertToTBMK(zbServerData.sGameData.RoleBaseInfo.TeamAttack);
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.guardHandler.HasFootholdsNotify()));
		refreshSkill();
	}

	public void refreshSkill()
	{
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0300: Unknown result type (might be due to invalid IL or missing references)
		//IL_0317: Unknown result type (might be due to invalid IL or missing references)
		//IL_0321: Unknown result type (might be due to invalid IL or missing references)
		heroList.Clear();
		List<zbModCsvRecord> list = new List<zbModCsvRecord>();
		foreach (int mod in zbServerData.sGameData.PvpInfo.ValidModInfo.ModList)
		{
			zbModCsvRecord recordByKey = zbDataConfig.Instance.modCsvTable.GetRecordByKey(mod);
			list.Add(recordByKey);
		}
		foreach (int select in zbServerData.sGameData.PvpInfo.ValidModInfo.SelectList)
		{
			zbModCsvRecord recordByKey2 = zbDataConfig.Instance.modCsvTable.GetRecordByKey(select);
			list.Add(recordByKey2);
		}
		foreach (int mod2 in zbServerData.sGameData.PvpInfo.ValidModInfo.ModList)
		{
			foreach (SkillDetail skill in zbServerData.sGameData.PvpInfo.ValidModInfo.SkillList)
			{
				if (skill.SkillType + 20010 == (ESkillType)mod2)
				{
					SkillDetail skillDetail = new SkillDetail();
					skillDetail.HeroTid = skill.HeroTid;
					skillDetail.SkillType = skill.SkillType;
					skillDetail.SkillId = skill.SkillId;
					heroList.Add(skillDetail);
				}
			}
		}
		foreach (int select2 in zbServerData.sGameData.PvpInfo.ValidModInfo.SelectList)
		{
			foreach (SkillDetail skill2 in zbServerData.sGameData.PvpInfo.ValidModInfo.SkillList)
			{
				if (skill2.SkillType + 20010 == (ESkillType)select2)
				{
					SkillDetail skillDetail2 = new SkillDetail();
					skillDetail2.HeroTid = skill2.HeroTid;
					skillDetail2.SkillType = skill2.SkillType;
					skillDetail2.SkillId = skill2.SkillId;
					heroList.Add(skillDetail2);
				}
			}
		}
		for (int i = 0; i < 5; i++)
		{
			m_nodeDetail[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		int num = 0;
		foreach (SkillDetail hero in heroList)
		{
			((VisualElement)tb_skillMod[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_nodeDetail[num].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			zbSkillCsvRecord recordByKey3 = zbDataConfig.Instance.skillCsvTable.GetRecordByKey(hero.SkillId);
			Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>(recordByKey3.icon);
			m_skillIcon[num].style.backgroundImage = StyleBackground.op_Implicit(val);
			m_skillIcon[num].transform.scale = Vector3.one * 0.45f;
			ESkillType type = hero.SkillType;
			int num2 = list.Count((zbModCsvRecord mod) => mod.skillLvUp == 1 && Enumerable.Contains(mod.skillType, (int)type));
			((TextElement)m_lbSkillLevel[num]).text = num2.ToString();
			num++;
		}
	}

	private void OnFoothold()
	{
		zbWsMsgHandler.Instance.Foothold_Data_Req(1);
	}

	private void OnEdit()
	{
		if (!zbSelf.Instance.moneyHandler.HasEnoughMoney(2, zbSelf.Instance.pvpHandler.modResetDiamond))
		{
			zbUIMgr.Instance.ShowTip(EResourceType.Money, 2L, zbSelf.Instance.pvpHandler.modResetDiamond);
		}
		else if (zbServerData.sGameData.PvpInfo.SetModList.Count > 0)
		{
			zbUIMgr.Instance.ShowPvpModSelect();
		}
		else
		{
			zbWsMsgHandler.Instance.PvpSet_ModStart_Req();
		}
	}

	public void OnSkillClick(ClickEvent evt, int skillIndex)
	{
		List<int> list = new List<int>();
		foreach (int mod in zbServerData.sGameData.PvpInfo.ValidModInfo.ModList)
		{
			list.Add(mod);
		}
		foreach (int select in zbServerData.sGameData.PvpInfo.ValidModInfo.SelectList)
		{
			list.Add(select);
		}
		zbUIMgr.Instance.ShowSkillFightModWnd(skillIndex, heroList.ToArray(), list, isResumeFight: false);
	}

	private void OnEventUpdate(object data)
	{
		Refresh();
	}
}
