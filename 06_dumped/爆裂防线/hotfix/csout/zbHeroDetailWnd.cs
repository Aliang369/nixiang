using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbHeroDetailWnd : zbBaseWnd
{
	private VisualTreeAsset m_itemTA;

	private Button m_btnTouch;

	private Button m_btnClose;

	private VisualElement m_spriteFrame;

	private VisualElement m_spriteHero;

	private VisualElement m_spriteRarity;

	private Label m_lbHeroName;

	private VisualElement m_progress;

	private VisualElement m_progressBg;

	private Label m_lbProgress;

	private VisualElement m_spriteSkill;

	private Label m_skillName;

	private Label m_lbDamage;

	private Label m_skillDesc;

	private Button[] m_btnMods = (Button[])(object)new Button[6];

	private VisualElement m_spriteAttrs;

	private ListView m_listView;

	private Label m_lbRoomDesc;

	private Button m_btnUpgrade;

	private Button m_btnFormationed;

	private Button m_btnStar;

	private Button m_btnFormation;

	private Button m_btnGetway;

	private Button m_btnCompose;

	private Button m_btnPreview;

	private Button m_btnLeft;

	private Button m_btnRight;

	private VisualElement m_notifyUpgrade;

	private VisualElement m_notifyStar;

	private VisualElement m_notifyCompose;

	private VisualElement m_starTag;

	private zbHeroCsvRecord m_record;

	private List<zbHeroCsvRecord> m_records;

	private HeroDetail m_heroDetail;

	private HomelandRoomDetail m_roomDetail;

	private WarRoomDetail m_warRoomDetail;

	private List<int> m_allMods = new List<int>();

	private float maxWidth = 130f;

	private bool m_showLeftRight = true;

	protected override void OnEnable()
	{
		//IL_048e: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_050a: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_itemTA = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/hero/tb_heroDetailAttr.uxml");
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_spriteFrame = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_frame", (string)null);
		m_spriteHero = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_icon", (string)null);
		m_spriteRarity = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_rarity", (string)null);
		m_lbHeroName = UQueryExtensions.Q<Label>(m_RootElement, "lb_heroName", (string)null);
		m_progress = UQueryExtensions.Q<VisualElement>(m_RootElement, "progress", (string)null);
		m_progressBg = UQueryExtensions.Q<VisualElement>(m_RootElement, "progressBg", (string)null);
		m_lbProgress = UQueryExtensions.Q<Label>(m_RootElement, "lb_progress", (string)null);
		m_spriteSkill = UQueryExtensions.Q<VisualElement>(m_RootElement, "skill", (string)null);
		m_skillName = UQueryExtensions.Q<Label>(m_RootElement, "lb_skill", (string)null);
		m_skillDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_skill_desc", (string)null);
		m_lbDamage = UQueryExtensions.Q<Label>(m_RootElement, "lb_damage", (string)null);
		for (int i = 0; i < 6; i++)
		{
			m_btnMods[i] = UQueryExtensions.Q<Button>(m_RootElement, "btn_mod" + i, (string)null);
			int curIndex = i;
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnMods[i], (Action<ClickEvent>)delegate(ClickEvent evt)
			{
				OnMods(evt, curIndex);
			}, "", (TrickleDown)0);
		}
		m_spriteAttrs = UQueryExtensions.Q<VisualElement>(m_RootElement, "sub_bg", (string)null);
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, "listView", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		m_lbRoomDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_roomDesc", (string)null);
		VisualElement btn = UQueryExtensions.Q<VisualElement>(m_RootElement, "buttons", (string)null);
		m_btnUpgrade = UQueryExtensions.Q<Button>(m_RootElement, "btn_upgrade", (string)null);
		m_btnFormationed = UQueryExtensions.Q<Button>(m_RootElement, "btn_formationed", (string)null);
		m_btnStar = UQueryExtensions.Q<Button>(m_RootElement, "btn_star", (string)null);
		m_btnFormation = UQueryExtensions.Q<Button>(m_RootElement, "btn_formation", (string)null);
		m_btnGetway = UQueryExtensions.Q<Button>(m_RootElement, "btn_getway", (string)null);
		m_btnCompose = UQueryExtensions.Q<Button>(m_RootElement, "btn_compose", (string)null);
		m_btnPreview = UQueryExtensions.Q<Button>(m_RootElement, "btn_preview", (string)null);
		m_btnLeft = UQueryExtensions.Q<Button>(m_RootElement, "btn_left", (string)null);
		m_btnRight = UQueryExtensions.Q<Button>(m_RootElement, "btn_right", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnUpgrade, (Action<ClickEvent>)OnUpgrade, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnFormationed, (Action<ClickEvent>)OnFormationed, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnStar, (Action<ClickEvent>)OnStar, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnFormation, (Action<ClickEvent>)OnFormation, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGetway, (Action<ClickEvent>)OnGetway, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCompose, (Action<ClickEvent>)OnCompose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnPreview, (Action<ClickEvent>)OnPreview, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnLeft, (Action)OnLeft, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRight, (Action)OnRight, "", (TrickleDown)0);
		m_notifyUpgrade = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnUpgrade, (Vector2?)new Vector2(-6f, -6f), 1f, true);
		m_notifyStar = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnStar, (Vector2?)new Vector2(-6f, -6f), 1f, true);
		m_notifyCompose = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnCompose, (Vector2?)new Vector2(-6f, -6f), 1f, true);
		m_starTag = zbUtil.CreateHeroStarTag(btn, (Vector2?)new Vector2(320f, 6f));
		AddDark();
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_HomelandInfo, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Hero, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Money, OnEventUpdate);
	}

	public void RefreshCustom(zbHeroCsvRecord record, bool showLeftRight = true)
	{
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_032f: Unknown result type (might be due to invalid IL or missing references)
		//IL_037b: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0428: Unknown result type (might be due to invalid IL or missing references)
		//IL_042e: Unknown result type (might be due to invalid IL or missing references)
		//IL_03dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0491: Unknown result type (might be due to invalid IL or missing references)
		//IL_0445: Unknown result type (might be due to invalid IL or missing references)
		//IL_044b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0412: Unknown result type (might be due to invalid IL or missing references)
		//IL_047a: Unknown result type (might be due to invalid IL or missing references)
		m_record = record;
		m_showLeftRight = showLeftRight;
		m_records = zbSelf.Instance.heroHandler.GetHeroRecordsBySkillType(m_record.type);
		m_heroDetail = zbSelf.Instance.heroHandler.GetHeroDetail(record.id);
		m_warRoomDetail = zbSelf.Instance.heroHandler.GetWarRoomDetailBySkillType((ESkillType)record.type);
		m_roomDetail = zbSelf.Instance.homelandHandler.GetHomelandRoomDetail(m_warRoomDetail.RoomId);
		zbHomeRoomTypeCsvRecord recordByKey = zbDataConfig.Instance.homeRoomTypeCsvTable.GetRecordByKey(m_roomDetail.TypeId);
		((TextElement)m_lbRoomDesc).text = recordByKey.desc;
		RefreshHero();
		RefreshDamage();
		RefreshAttrList();
		((VisualElement)m_btnLeft).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((!m_showLeftRight || m_records.Count <= 1) ? 1 : 0));
		((VisualElement)m_btnRight).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((!m_showLeftRight || m_records.Count <= 1) ? 1 : 0));
		((VisualElement)m_btnUpgrade).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_heroDetail == null || m_warRoomDetail.HeroTid != record.id) ? 1 : 0));
		((VisualElement)m_btnFormationed).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_heroDetail == null || m_warRoomDetail.HeroTid != record.id) ? 1 : 0));
		((VisualElement)m_btnStar).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_heroDetail == null || m_warRoomDetail.HeroTid == record.id) ? 1 : 0));
		((VisualElement)m_btnFormation).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_heroDetail == null || m_warRoomDetail.HeroTid == record.id) ? 1 : 0));
		((VisualElement)m_btnGetway).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_heroDetail != null || zbSelf.Instance.heroHandler.HasHeroComposeNotify(record.id)) ? 1 : 0));
		((VisualElement)m_btnPreview).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_heroDetail != null || zbSelf.Instance.heroHandler.HasHeroComposeNotify(record.id)) ? 1 : 0));
		((VisualElement)m_btnCompose).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_heroDetail != null || !zbSelf.Instance.heroHandler.HasHeroComposeNotify(record.id)) ? 1 : 0));
		m_notifyUpgrade.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((!zbSelf.Instance.heroHandler.HasHeroUpgradeNotify(record.id) && !zbSelf.Instance.heroHandler.HasHeroAwakenNotify(record.id) && !zbSelf.Instance.heroHandler.HasHeroStarNotify(record.id) && !zbSelf.Instance.heroHandler.HasHeroRarityNotify(m_record.id) && !zbSelf.Instance.geneHandler.HasHeroTabGeneNotify((ESkillType)m_record.type, m_record.id)) ? 1 : 0));
		m_notifyStar.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((!zbSelf.Instance.heroHandler.HasHeroStarNotify(record.id) && !zbSelf.Instance.heroHandler.HasHeroRarityNotify(m_record.id)) ? 1 : 0));
		m_notifyCompose.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.heroHandler.HasHeroComposeNotify(record.id)));
		if (((VisualElement)m_btnUpgrade).style.display == StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0) && m_notifyUpgrade.style.display == StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1) && zbSelf.Instance.heroHandler.HasHeroStarFragNotify(record.id))
		{
			m_starTag.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		else if (((VisualElement)m_btnStar).style.display == StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0) && m_notifyStar.style.display == StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1) && zbSelf.Instance.heroHandler.HasHeroStarFragNotify(record.id))
		{
			m_starTag.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		else
		{
			m_starTag.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
	}

	private void RefreshHero()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_028b: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)m_lbHeroName).text = m_record.name;
		zbSkillCsvRecord recordByKey = zbDataConfig.Instance.skillCsvTable.GetRecordByKey(m_record.skillId);
		if (recordByKey != null)
		{
			m_spriteSkill.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.icon));
			((TextElement)m_skillName).text = recordByKey.name;
			((TextElement)m_skillDesc).text = recordByKey.desc;
		}
		int avatarId = m_record.avatarId;
		EQuality quality = zbResourceHelper.GetResourceQuality(EResourceType.Hero, m_record.id);
		ERarityType rarity = (ERarityType)m_record.rarity;
		if (m_heroDetail != null)
		{
			avatarId = m_heroDetail.AvatarId;
			quality = m_heroDetail.Quality;
			rarity = m_heroDetail.Rarity;
			RefreshMods(m_heroDetail.Quality);
		}
		else
		{
			RefreshMods(EQuality.EmptyValue);
		}
		zbHeroAvatarCsvRecord recordByKey2 = zbDataConfig.Instance.heroAvatarCsvTable.GetRecordByKey(avatarId);
		if (recordByKey2 != null)
		{
			m_spriteHero.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey2.heroIcon));
		}
		string heroFrameTex = zbResourceHelper.GetHeroFrameTex(quality);
		m_spriteFrame.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(heroFrameTex));
		string heroRarityIconPath = zbResourceHelper.GetHeroRarityIconPath(rarity);
		m_spriteRarity.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(heroRarityIconPath));
		long heroFragCount = zbSelf.Instance.heroHandler.GetHeroFragCount(m_record.id);
		long composeOrUpgradeCount = zbSelf.Instance.heroHandler.GetComposeOrUpgradeCount(m_record.id);
		if (composeOrUpgradeCount > 0)
		{
			((TextElement)m_lbProgress).text = heroFragCount + "/" + composeOrUpgradeCount;
			float num = Mathf.Clamp((float)heroFragCount / (float)composeOrUpgradeCount, 0f, 1f);
			m_progress.style.width = StyleLength.op_Implicit(new Length(num * maxWidth, (LengthUnit)0));
		}
		else
		{
			((TextElement)m_lbProgress).text = heroFragCount.ToString();
			m_progress.style.width = StyleLength.op_Implicit(new Length(1f * maxWidth, (LengthUnit)0));
		}
		Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>("ui/image/progress/process130x18_1.png");
		if (heroFragCount >= composeOrUpgradeCount && composeOrUpgradeCount != 0L)
		{
			val = zbAssetLoader.Instance.LoadResouce<Texture2D>("ui/image/progress/process130x18_2.png");
		}
		m_progressBg.style.backgroundImage = StyleBackground.op_Implicit(val);
	}

	private void RefreshDamage()
	{
		if (m_heroDetail == null)
		{
			zbHeroRarityCsvRecord recordByKey = zbDataConfig.Instance.heroRarityCsvTable.GetRecordByKey(m_record.rarity);
			float damage = zbSelf.Instance.heroHandler.GetDamage((EQuality)recordByKey.quality, (ERarityType)m_record.rarity, 0);
			((TextElement)m_lbDamage).text = zbUtil.Language("ShangHaiXiShu") + ": " + $"{damage * 100f:0.##}%";
			return;
		}
		float damage2 = zbSelf.Instance.heroHandler.GetDamage(m_heroDetail.Quality, m_heroDetail.Rarity, m_heroDetail.QualityStar);
		float followDamages = zbSelf.Instance.heroHandler.GetFollowDamages((ESkillType)m_record.type);
		string text = zbUtil.Language("ShangHaiXiShu") + ": " + $"{damage2 * 100f:0.##}%";
		if (followDamages > 0f && zbSelf.Instance.heroHandler.GetWarRoomDetailByHeroTid(m_record.id) != null)
		{
			text = text + "<color=#2DF421>+" + $"{followDamages * 100f:0.##}%" + "</color>";
		}
		((TextElement)m_lbDamage).text = text;
	}

	private void RefreshMods(EQuality quality)
	{
		m_allMods.Clear();
		m_allMods = new List<int> { m_record.mod1, m_record.mod2, m_record.mod3, m_record.mod4, m_record.mod5, m_record.mod6 };
		for (int i = 0; i < m_allMods.Count; i++)
		{
			if (zbDataConfig.Instance.modCsvTable.GetRecordByKey(m_allMods[i]) != null)
			{
				zbUtil.SetElementGray((VisualElement)(object)m_btnMods[i], (int)quality < i + 2);
			}
		}
	}

	private void RefreshAttrList()
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		List<WarRoomAttrDetail> attrList = ((IEnumerable<WarRoomAttrDetail>)m_warRoomDetail.AttrList).OrderBy((WarRoomAttrDetail a) => a.Type).ToList();
		m_spriteAttrs.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(attrList.Count <= 0));
		((VisualElement)m_listView).Clear();
		((BaseVerticalCollectionView)m_listView).fixedItemHeight = 55f;
		m_listView.makeItem = () => (VisualElement)(object)new zbHeroDetailAttrItem(this, m_itemTA);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			if (index >= 0 && index < attrList.Count && attrList[index] != null)
			{
				(element as zbHeroDetailAttrItem).Refresh(attrList[index], (ESkillType)m_record.type, m_record.id);
			}
		};
		((BaseVerticalCollectionView)m_listView).itemsSource = null;
		((BaseVerticalCollectionView)m_listView).itemsSource = attrList;
		((BaseVerticalCollectionView)m_listView).RefreshItems();
	}

	private void OnMods(ClickEvent evt, int index)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		if (m_allMods[index] > 0)
		{
			zbModCsvRecord recordByKey = zbDataConfig.Instance.modCsvTable.GetRecordByKey(m_allMods[index]);
			if (recordByKey != null)
			{
				IEventHandler target = ((EventBase)evt).target;
				Vector2 pos = VisualElementExtensions.LocalToWorld((VisualElement)(object)((target is VisualElement) ? target : null), Vector2.zero);
				pos.y += 100f;
				zbUIMgr.Instance.ShowTipMod(recordByKey.desc, pos);
			}
		}
	}

	private void OnUpgrade(ClickEvent evt)
	{
		zbUIMgr.Instance.ShowHero(m_record);
		OnClose(null);
	}

	private void OnFormationed(ClickEvent evt)
	{
		zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10726"));
	}

	private void OnStar(ClickEvent evt)
	{
		zbUIMgr.Instance.ShowHeroStar(m_record);
	}

	private void OnFormation(ClickEvent evt)
	{
		zbWsMsgHandler.Instance.WarRoom_HeroSet_Req(m_warRoomDetail.SkillType, m_record.id);
	}

	private void OnGetway(ClickEvent evt)
	{
		zbHeroRarityCsvRecord recordByKey = zbDataConfig.Instance.heroRarityCsvTable.GetRecordByKey(m_record.rarity);
		if (recordByKey != null)
		{
			zbUIMgr.Instance.ShowTip(EResourceType.Hero, m_record.id, recordByKey.frag);
		}
	}

	private void OnCompose(ClickEvent evt)
	{
		zbWsMsgHandler.Instance.Hero_Compose_Req(m_record.id);
	}

	private void OnPreview(ClickEvent evt)
	{
		zbUIMgr.Instance.ShowPreview(EPreviewType.Hero, m_record.id);
	}

	private void OnClose(ClickEvent evt)
	{
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
		if (m_record == null)
		{
			return;
		}
		int num = m_records.FindIndex((zbHeroCsvRecord r) => r.id == m_record.id);
		int count = m_records.Count;
		if (num >= 0 && count > 0)
		{
			int index = (num + direction + count) % count;
			if (m_records[index] != null)
			{
				RefreshCustom(m_records[index]);
			}
		}
	}

	private void OnEventUpdate(object obj)
	{
		RefreshCustom(m_record);
	}
}
