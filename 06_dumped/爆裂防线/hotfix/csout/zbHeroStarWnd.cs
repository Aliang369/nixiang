using System;
using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbHeroStarWnd : zbBaseWnd
{
	private VisualElement m_spriteFrame;

	private VisualElement m_spriteHero;

	private Label m_lbName;

	private VisualElement m_spriteSkill;

	private Label m_lbSkill;

	private Label m_lbDamage;

	private Label m_lbReform;

	private VisualElement m_spriteRarity;

	private VisualElement m_skillIcon;

	private Label m_lbSkillDesc;

	private Label[] m_costLabels = (Label[])(object)new Label[3];

	private zbItem[] m_items = new zbItem[3];

	private VisualElement m_mod;

	private VisualElement[] m_mods = (VisualElement[])(object)new VisualElement[6];

	private Label[] m_lbMods = (Label[])(object)new Label[6];

	private Button[] m_btnLocks = (Button[])(object)new Button[6];

	private Button[] m_modsMask = (Button[])(object)new Button[6];

	private VisualElement[] m_stars = (VisualElement[])(object)new VisualElement[6];

	private SpineVisualElement[] m_upgradeSpines = (SpineVisualElement[])(object)new SpineVisualElement[6];

	private Button m_btnStar;

	private SpineVisualElement m_btnStarSpine;

	private TextElement m_lbButton;

	private Button m_btnRarity;

	private Button m_btnClose;

	private VisualElement m_notify;

	private VisualElement m_notifyRarity;

	private VisualElement m_starTag;

	private VisualElement m_starInfo;

	private Label m_lbStarInfo;

	private zbHeroCsvRecord m_record;

	private HeroDetail m_data;

	private int[][] m_upgradeCostList;

	private int[][] m_composeCostList;

	private int m_nextMod;

	protected override void OnEnable()
	{
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_spriteFrame = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_frame", (string)null);
		m_spriteHero = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_icon", (string)null);
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_spriteSkill = UQueryExtensions.Q<VisualElement>(m_RootElement, "icon_skill", (string)null);
		m_lbSkill = UQueryExtensions.Q<Label>(m_RootElement, "lb_skill_value", (string)null);
		m_lbDamage = UQueryExtensions.Q<Label>(m_RootElement, "lb_dmg_value", (string)null);
		m_lbReform = UQueryExtensions.Q<Label>(m_RootElement, "lb_reform_value", (string)null);
		m_spriteRarity = UQueryExtensions.Q<VisualElement>(m_RootElement, "rarity", (string)null);
		m_skillIcon = UQueryExtensions.Q<VisualElement>(m_RootElement, "skill_icon", (string)null);
		m_lbSkillDesc = UQueryExtensions.Q<Label>(m_RootElement, "skill_content", (string)null);
		m_mod = UQueryExtensions.Q<VisualElement>(m_RootElement, "mods", (string)null);
		for (int i = 0; i < 6; i++)
		{
			m_mods[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "mod" + i, (string)null);
			m_lbMods[i] = UQueryExtensions.Q<Label>(m_mods[i], "lb_mod", (string)null);
			m_btnLocks[i] = UQueryExtensions.Q<Button>(m_mods[i], "btn_lock", (string)null);
			m_modsMask[i] = UQueryExtensions.Q<Button>(m_mods[i], "mask", (string)null);
			m_stars[i] = UQueryExtensions.Q<VisualElement>(m_mods[i], "stars", (string)null);
			m_upgradeSpines[i] = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "spine" + i, (string)null);
			int curIndex = i;
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnLocks[i], (Action<ClickEvent>)delegate(ClickEvent evt)
			{
				OnLocks(evt, curIndex);
			}, "", (TrickleDown)0);
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_modsMask[i], (Action<ClickEvent>)delegate(ClickEvent evt)
			{
				OnLocks(evt, curIndex);
			}, "", (TrickleDown)0);
		}
		for (int num = 0; num < 3; num++)
		{
			m_costLabels[num] = UQueryExtensions.Q<Label>(m_RootElement, "lb_cost" + num, (string)null);
			m_items[num] = UQueryExtensions.Q<zbItem>(m_RootElement, "item" + num, (string)null);
			m_items[num].OnEnable(this);
			((VisualElement)m_costLabels[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		m_btnStar = UQueryExtensions.Q<Button>(m_RootElement, "btn_star", (string)null);
		m_lbButton = UQueryExtensions.Q<TextElement>((VisualElement)(object)m_btnStar, (string)null, (string)null);
		m_btnStarSpine = UQueryExtensions.Q<SpineVisualElement>((VisualElement)(object)m_btnStar, "spine", (string)null);
		((TextElement)m_btnStar).text = "升星";
		m_starInfo = UQueryExtensions.Q<VisualElement>(m_RootElement, "starInfo", (string)null);
		m_lbStarInfo = UQueryExtensions.Q<Label>(m_starInfo, "lb_starInfo", (string)null);
		m_btnRarity = UQueryExtensions.Q<Button>(m_RootElement, "btn_rarity", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnStar, (Action)OnStar, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRarity, (Action)OnRarity, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnStar);
		m_notifyRarity = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnRarity);
		m_starTag = zbUtil.CreateHeroStarTag((VisualElement)(object)m_btnStar);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Hero, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Money, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_HeroFrag, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_HomelandInfo, OnEventUpdate);
	}

	public void RefreshCustom(zbHeroCsvRecord record)
	{
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_029d: Unknown result type (might be due to invalid IL or missing references)
		//IL_030a: Unknown result type (might be due to invalid IL or missing references)
		m_record = record;
		m_data = zbSelf.Instance.heroHandler.GetHeroDetail(m_record.id);
		EQuality quality = zbResourceHelper.GetResourceQuality(EResourceType.Hero, m_record.id);
		ERarityType rarity = (ERarityType)m_record.rarity;
		if (m_data != null)
		{
			quality = m_data.Quality;
			rarity = m_data.Rarity;
			m_lbButton.text = zbUtil.Language("lang.10035");
			RefreshUpgrade(quality, m_data.Rarity);
		}
		else
		{
			m_lbButton.text = zbUtil.Language("lang.10036");
			RefreshCompose();
		}
		((TextElement)m_lbName).text = m_record.name;
		((TextElement)m_lbReform).text = m_record.desc;
		int avatarId = m_record.avatarId;
		if (m_data != null)
		{
			avatarId = m_data.AvatarId;
		}
		zbHeroAvatarCsvRecord recordByKey = zbDataConfig.Instance.heroAvatarCsvTable.GetRecordByKey(avatarId);
		if (recordByKey != null)
		{
			m_spriteHero.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.heroIcon));
		}
		string skillTypeIconPath = zbResourceHelper.GetSkillTypeIconPath(m_record, rarity);
		m_spriteSkill.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(skillTypeIconPath));
		zbSkillCsvRecord recordByKey2 = zbDataConfig.Instance.skillCsvTable.GetRecordByKey(m_record.skillId);
		if (recordByKey2 != null)
		{
			((TextElement)m_lbSkill).text = recordByKey2.name;
			((TextElement)m_lbSkillDesc).text = recordByKey2.desc;
			m_skillIcon.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey2.icon));
		}
		string heroRarityIconPath = zbResourceHelper.GetHeroRarityIconPath(rarity);
		m_spriteRarity.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(heroRarityIconPath));
		string heroFrameTex = zbResourceHelper.GetHeroFrameTex(quality);
		m_spriteFrame.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(heroFrameTex));
		RefreshHeroMods(quality);
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((!zbSelf.Instance.heroHandler.HasHeroComposeNotify(m_record.id) && !zbSelf.Instance.heroHandler.HasHeroStarNotify(m_record.id)) ? 1 : 0));
		m_notifyRarity.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.heroHandler.HasHeroRarityNotify(m_record.id)));
		m_starTag.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((zbSelf.Instance.heroHandler.HasHeroComposeNotify(m_record.id) || zbSelf.Instance.heroHandler.HasHeroStarNotify(m_record.id) || !zbSelf.Instance.heroHandler.HasHeroStarFragNotify(m_record.id)) ? 1 : 0));
	}

	private void RefreshUpgrade(EQuality quality, ERarityType rarity)
	{
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		float damage = zbSelf.Instance.heroHandler.GetDamage(quality, rarity, m_data.QualityStar);
		float followDamages = zbSelf.Instance.heroHandler.GetFollowDamages((ESkillType)m_record.type);
		string text = $"{damage * 100f:0.##}%";
		if (followDamages > 0f && zbSelf.Instance.heroHandler.GetWarRoomDetailByHeroTid(m_record.id) != null)
		{
			text = text + "<color=#1DBA1A>+" + $"{followDamages * 100f:0.##}%" + "</color>";
		}
		((TextElement)m_lbDamage).text = text;
		bool flag = quality + 1 <= EQuality.Orange;
		if (flag)
		{
			zbHeroQualityUpgradeCsvRecord heroNextQualityUpgradeRecord = zbSelf.Instance.heroHandler.GetHeroNextQualityUpgradeRecord((int)m_data.Quality, m_data.QualityStar);
			((VisualElement)m_btnStar).visible = true;
			((VisualElement)m_btnRarity).visible = false;
			m_upgradeCostList = zbSelf.Instance.homelandHandler.AppendExtraCostItem(heroNextQualityUpgradeRecord.costList, m_record.id, heroNextQualityUpgradeRecord.costFrag, 7);
		}
		else
		{
			((VisualElement)m_btnStar).visible = false;
			Button btnRarity = m_btnRarity;
			HeroDetail data = m_data;
			((VisualElement)btnRarity).visible = data != null && data.Rarity < ERarityType.Ss;
			m_upgradeCostList = new int[0][];
		}
		m_starInfo.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		if (flag)
		{
			float damage2 = zbSelf.Instance.heroHandler.GetDamage(m_data.Quality, m_data.Rarity, m_data.QualityStar);
			float damage3 = zbSelf.Instance.heroHandler.GetDamage((m_data.QualityStar == 4) ? (m_data.Quality + 1) : m_data.Quality, m_data.Rarity, (m_data.QualityStar != 4) ? (m_data.QualityStar + 1) : 0);
			((TextElement)m_lbStarInfo).text = "升星加成:伤害系数+" + Mathf.Round((damage3 - damage2) * 100f) + "%";
		}
		bool flag2 = flag && m_data.QualityStar == 4;
		((VisualElement)m_btnStarSpine).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag2));
		if (flag2)
		{
			((TextElement)m_btnStar).text = "突破";
			zbUtil.RefreshSpineUI(m_btnStarSpine, "animation/ui/ui_button04/ui_button04_SkeletonData.asset");
			m_btnStarSpine.AnimationState.SetAnimation(0, "animation", true);
		}
		zbSelf.Instance.homelandHandler.UpdateCostUI(m_upgradeCostList, m_costLabels, m_items);
	}

	private void RefreshCompose()
	{
		zbHeroRarityCsvRecord recordByKey = zbDataConfig.Instance.heroRarityCsvTable.GetRecordByKey(m_record.rarity);
		if (recordByKey != null)
		{
			float damage = zbSelf.Instance.heroHandler.GetDamage(m_data.Quality, m_data.Rarity, m_data.QualityStar);
			((TextElement)m_lbDamage).text = $"{damage * 100f:0.##}%";
			m_composeCostList = new int[1][] { new int[3] { 7, m_record.id, recordByKey.frag } };
			zbSelf.Instance.homelandHandler.UpdateCostUI(m_composeCostList, m_costLabels, m_items);
		}
	}

	private void RefreshHeroMods(EQuality quality)
	{
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0250: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		if (m_record == null)
		{
			return;
		}
		List<int> list = new List<int> { m_record.mod1, m_record.mod2, m_record.mod3, m_record.mod4, m_record.mod5, m_record.mod6 };
		for (int i = 0; i < list.Count; i++)
		{
			zbModCsvRecord recordByKey = zbDataConfig.Instance.modCsvTable.GetRecordByKey(list[i]);
			if (recordByKey == null)
			{
				continue;
			}
			zbLabelLinkUtil.BuildModLink(recordByKey.desc, m_lbMods[i]);
			bool flag = m_data == null || (int)quality < i + 2;
			bool flag2 = zbLabelLinkUtil.HasTerm(recordByKey.desc);
			bool flag3 = quality - 1 == (EQuality)i;
			m_stars[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag3));
			bool flag4 = quality + 1 <= EQuality.Orange && m_data.QualityStar == 4;
			((VisualElement)m_upgradeSpines[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!(flag3 & flag4)));
			if (flag3 & flag4)
			{
				zbUtil.RefreshSpineUI(m_upgradeSpines[i], "animation/ui/ui_button05/ui_button05_SkeletonData.asset");
				m_upgradeSpines[i].AnimationState.SetAnimation(0, "animation", true);
			}
			Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/hero/icon_star" + m_data.QualityStar + ".png");
			m_stars[i].style.backgroundImage = new StyleBackground(val);
			((TextElement)m_lbMods[i]).text = (flag ? ((TextElement)m_lbMods[i]).text.Replace("<color=#64FF26>", "<color=#3CB317>") : ((TextElement)m_lbMods[i]).text.Replace("<color=#64FF26>", "<color=#007008>"));
			if (flag)
			{
				if (flag2)
				{
					((VisualElement)m_btnLocks[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
					((VisualElement)m_modsMask[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
					((VisualElement)m_lbMods[i]).pickingMode = (PickingMode)0;
				}
				else
				{
					((VisualElement)m_btnLocks[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
					((VisualElement)m_btnLocks[i]).pickingMode = (PickingMode)0;
					((VisualElement)m_modsMask[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				}
			}
			else
			{
				((VisualElement)m_btnLocks[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				((VisualElement)m_modsMask[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				((VisualElement)m_lbMods[i]).pickingMode = (PickingMode)0;
			}
		}
		if ((int)(quality - 1) < list.Count)
		{
			m_nextMod = list[(int)(quality - 1)];
		}
	}

	public void OnLocks(ClickEvent evt, int index)
	{
		zbQualityCsvRecord recordByKey = zbDataConfig.Instance.qualityCsvTable.GetRecordByKey(index + 2);
		zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.10413"), recordByKey.name));
	}

	private void OnStar()
	{
		int[][] upgradeCostList;
		if (m_data != null)
		{
			upgradeCostList = m_upgradeCostList;
			foreach (int[] array in upgradeCostList)
			{
				if (!zbSelf.Instance.dataHandler.HasResourceEnough((EResourceType)array[0], array[1], array[2]))
				{
					zbUIMgr.Instance.ShowTip((EResourceType)array[0], array[1], array[2]);
					return;
				}
			}
			zbWsMsgHandler.Instance.Hero_QualityUpgrade_Req(m_record.id, m_nextMod);
			return;
		}
		upgradeCostList = m_composeCostList;
		foreach (int[] array2 in upgradeCostList)
		{
			if (!zbSelf.Instance.dataHandler.HasResourceEnough((EResourceType)array2[0], array2[1], array2[2]))
			{
				zbUIMgr.Instance.ShowTip((EResourceType)array2[0], array2[1], array2[2]);
				return;
			}
		}
		zbWsMsgHandler.Instance.Hero_Compose_Req(m_record.id);
	}

	private void OnRarity()
	{
		if (!zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.HeroRarity))
		{
			zbUIMgr.Instance.ShowHeroRarity(m_record.id);
		}
	}

	private void OnClose()
	{
		Hide();
	}

	private void OnEventUpdate(object data)
	{
		RefreshCustom(m_record);
	}
}
