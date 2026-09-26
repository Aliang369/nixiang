using System;
using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbPlayerSuitWnd : zbBaseWnd
{
	private zbPlayerAvatar m_spineAvatar;

	private ScrollView m_scView;

	private Button m_btnClose;

	private Button m_btnSkill;

	[NonSerialized]
	public int m_selectSuitId;

	[NonSerialized]
	public int indexSort = 1;

	private Label m_lbName;

	private Label m_lbAttr;

	private Label m_lbLevel;

	private Label m_lbDesc;

	private Label m_lbCost;

	private Label m_lbUse;

	private Button m_btnActive;

	private Button m_btnDown;

	private Button m_btnStar;

	private Button m_btnLeft;

	private Button m_btnRight;

	private VisualElement m_spriteSkillFrame;

	private VisualElement m_spriteSkillIcon;

	private VisualElement m_spriteCostFrame;

	private VisualElement m_spriteCost;

	private List<zbPlayerSuitItem> m_SuitItem = new List<zbPlayerSuitItem>();

	private VisualElement[] m_spriteStars = (VisualElement[])(object)new VisualElement[5];

	private VisualElement m_newSkill;

	private Button m_btnPlayerGun;

	private VisualElement m_notifyGun;

	private Button m_btnShow;

	private Button m_btnPreview;

	private Button m_btnList;

	private List<zbPlayerSuitCsvRecord> sortTemp = new List<zbPlayerSuitCsvRecord>();

	private List<zbPlayerSuitCsvRecord> ownedList = new List<zbPlayerSuitCsvRecord>();

	private List<zbPlayerSuitCsvRecord> otherList = new List<zbPlayerSuitCsvRecord>();

	private List<zbPlayerSuitCsvRecord> m_listReleasedRecords = new List<zbPlayerSuitCsvRecord>();

	private float playActionName;

	private bool m_isSuper;

	private VisualElement m_notifyActive;

	private VisualElement m_notifyStar;

	private List<int> openedSuperSuitIds = new List<int>();

	protected override void OnEnable()
	{
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_029c: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_spineAvatar = ((Component)m_GameObjectSceneRoot.transform.Find("playerAvatar")).GetComponent<zbPlayerAvatar>();
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_btnSkill = UQueryExtensions.Q<Button>(m_RootElement, "btn_skill", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSkill, (Action)OnSkill, "", (TrickleDown)0);
		m_scView = UQueryExtensions.Q<ScrollView>(m_RootElement, "list", (string)null);
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_lbAttr = UQueryExtensions.Q<Label>(m_RootElement, "lb_attr", (string)null);
		m_lbLevel = UQueryExtensions.Q<Label>(m_RootElement, "lb_level", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_lbCost = UQueryExtensions.Q<Label>(m_RootElement, "lb_cost", (string)null);
		m_lbUse = UQueryExtensions.Q<Label>(m_RootElement, "lb_use", (string)null);
		m_newSkill = zbUtil.CreateNewTag((VisualElement)(object)m_btnSkill);
		m_btnActive = UQueryExtensions.Q<Button>(m_RootElement, "button_active", (string)null);
		m_btnDown = UQueryExtensions.Q<Button>(m_RootElement, "button_down", (string)null);
		m_btnStar = UQueryExtensions.Q<Button>(m_RootElement, "button_star", (string)null);
		m_notifyActive = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnActive, (Vector2?)new Vector2(5f, 5f), 1f, true);
		m_notifyStar = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnStar);
		m_btnLeft = UQueryExtensions.Q<Button>(m_RootElement, "button_left", (string)null);
		m_btnRight = UQueryExtensions.Q<Button>(m_RootElement, "button_right", (string)null);
		m_spriteSkillFrame = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_Skillframe", (string)null);
		m_spriteSkillIcon = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_Skillicon", (string)null);
		m_spriteCostFrame = UQueryExtensions.Q<VisualElement>(m_RootElement, "icon_frame", (string)null);
		m_spriteCost = UQueryExtensions.Q<VisualElement>(m_RootElement, "icon_cost", (string)null);
		m_btnPlayerGun = UQueryExtensions.Q<Button>(m_RootElement, "button_playerGun", (string)null);
		m_notifyGun = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnPlayerGun, (Vector2?)new Vector2(10f, 10f), 1f, true);
		m_btnShow = UQueryExtensions.Q<Button>(m_RootElement, "button_show", (string)null);
		m_btnPreview = UQueryExtensions.Q<Button>(m_RootElement, "btn_preview", (string)null);
		m_btnList = UQueryExtensions.Q<Button>(m_RootElement, "btn_list", (string)null);
		for (int i = 0; i < 5; i++)
		{
			m_spriteStars[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "iconstar" + i, (string)null);
		}
		int num = 1;
		foreach (KeyValuePair<int, zbPlayerSuitCsvRecord> record in zbDataConfig.Instance.playerSuitCsvTable.GetRecords().records)
		{
			if (record.Value.id != 1 && zbVersionMgr.IsReleased(record.Value.preShow))
			{
				m_listReleasedRecords.Add(record.Value);
				zbPlayerSuitItem zbPlayerSuitItem2 = new zbPlayerSuitItem(this, num);
				m_SuitItem.Add(zbPlayerSuitItem2);
				((VisualElement)m_scView).Add((VisualElement)(object)zbPlayerSuitItem2);
				num++;
			}
		}
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnActive, (Action)OnActive, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnDown, (Action)OnDown, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnStar, (Action)OnActive, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnLeft, (Action<ClickEvent>)delegate(ClickEvent evt)
		{
			OnGoto(evt, -1);
		}, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRight, (Action<ClickEvent>)delegate(ClickEvent evt)
		{
			OnGoto(evt, 1);
		}, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnPlayerGun, (Action)OnPlayerGun, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnShow, (Action)OnShow, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnPreview, (Action)OnPreview, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnList, (Action)OnPreview, "", (TrickleDown)0);
		zbLanguage.Instance.SetButtonTextVisible((Button[])(object)new Button[1] { m_btnSkill });
		m_gameEevent.RegisterEvent(EGameEvent.Event_SuitSkill_Refresh, OnEventUpdateSuitList);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Suit_Refresh, OnEventUpdateSuitList);
	}

	private void OnEventUpdateSuitList(object obj)
	{
		RefreshCustom();
	}

	public void RefreshCustom(bool isFirst = false)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		playActionName = 0f;
		GetRefreshList(sortTemp, ownedList, otherList);
		((VisualElement)m_btnLeft).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(indexSort == 1));
		((VisualElement)m_btnRight).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((indexSort >= ((VisualElement)m_scView).childCount) ? 1 : 0));
		if (isFirst)
		{
			openedSuperSuitIds.Clear();
			m_selectSuitId = zbSelf.Instance.GetSuitId();
		}
		int num = 0;
		foreach (zbPlayerSuitCsvRecord item in sortTemp)
		{
			zbPlayerSuitItem zbPlayerSuitItem2 = m_SuitItem[num];
			if (m_selectSuitId < 100)
			{
				m_selectSuitId = item.id;
			}
			zbPlayerSuitItem2.RefreshSelect(m_selectSuitId == item.id);
			zbPlayerSuitItem2.Refresh(item);
			if (m_selectSuitId == item.id)
			{
				indexSort = zbPlayerSuitItem2.indexSort;
				RefreshDetail(item);
				if (isFirst)
				{
					zbUtil.ScrollHorizontalToIndex(m_scView, 4, ((VisualElement)m_scView).childCount, 166f, zbPlayerSuitItem2.indexSort - 1);
				}
			}
			num++;
		}
		m_newSkill.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.suitGunHandler.HasNewSuitPoint()));
		m_notifyGun.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.suitGunHandler.CanAllGunActiveOrUpgrade()));
		CheckShouldConvert();
		AutoFontSize();
	}

	private void CheckShouldConvert()
	{
		List<PlayerSuitDetail> suitsShouldConvert = zbSelf.Instance.suitGunHandler.GetSuitsShouldConvert();
		if (suitsShouldConvert.Count > 0)
		{
			zbUIMgr.Instance.ShowPlayerSuitConvert(suitsShouldConvert.ToArray());
		}
	}

	private void AutoFontSize()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		float num = 20f;
		((VisualElement)m_btnSkill).style.fontSize = StyleLength.op_Implicit(num);
		if (((TextElement)m_btnSkill).MeasureTextSize(((TextElement)m_btnSkill).text, 9999f, (MeasureMode)0, 0f, (MeasureMode)0).x > 88f)
		{
			num = 18f;
		}
		((VisualElement)m_btnSkill).style.fontSize = StyleLength.op_Implicit(num);
	}

	public void RefreshDetail(zbPlayerSuitCsvRecord m_record)
	{
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0313: Unknown result type (might be due to invalid IL or missing references)
		//IL_0329: Unknown result type (might be due to invalid IL or missing references)
		//IL_033f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_045b: Unknown result type (might be due to invalid IL or missing references)
		//IL_048d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04de: Unknown result type (might be due to invalid IL or missing references)
		m_spineAvatar.RefreshCustom(m_record.avatarId, zbSelf.Instance.GetGunId());
		m_isSuper = m_record.grade == 2;
		if (m_isSuper)
		{
			zbSelf.Instance.suitGunHandler.SetSuperSuitEnterAnimation(m_spineAvatar, !openedSuperSuitIds.Contains(m_record.id));
			openedSuperSuitIds.Add(m_record.id);
		}
		else
		{
			m_spineAvatar.gunObject.SetActive(true);
		}
		string resName = (m_isSuper ? "ui/image/suitGun/skill_empty1.png" : "ui/image/suitGun/skill_empty.png");
		float num = (m_isSuper ? 1f : 0.8f);
		m_spriteSkillIcon.style.scale = StyleScale.op_Implicit(new Scale(new Vector2(num, num)));
		m_spriteSkillFrame.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resName));
		m_spriteSkillIcon.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(m_record.skillIcon));
		((TextElement)m_lbName).text = m_record.name;
		for (int i = 0; i < 5; i++)
		{
			Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>("ui/image/suitGun/star_0.png");
			m_spriteStars[i].style.backgroundImage = StyleBackground.op_Implicit(val);
		}
		PlayerSuitDetail playerSuitDetail = zbSelf.Instance.suitGunHandler.GetPlayerSuitDetail(m_record.id);
		if (playerSuitDetail != null)
		{
			((TextElement)m_lbLevel).text = "Lv." + playerSuitDetail.Star;
			((VisualElement)m_btnActive).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_btnDown).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_btnStar).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			if (zbSelf.Instance.GetSuitId() == m_selectSuitId)
			{
				((TextElement)m_btnDown).text = zbUtil.Language("lang.10579");
			}
			else
			{
				((TextElement)m_btnDown).text = zbUtil.Language("lang.10580");
			}
			if (zbSelf.Instance.suitGunHandler.IsPlayerSuitSkillUsed(m_selectSuitId))
			{
				((VisualElement)m_lbUse).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			}
			else
			{
				((VisualElement)m_lbUse).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
			zbPlayerSuitUpgradeCsvRecord suitStarUpgrade = zbSelf.Instance.suitGunHandler.GetSuitStarUpgrade(m_record.id, playerSuitDetail.Star);
			((TextElement)m_lbAttr).text = string.Format(zbUtil.Language("lang.10582"), suitStarUpgrade.atk);
			zbLabelLinkUtil.BuildModLink(zbSelf.Instance.suitGunHandler.GetSkillDesc(m_record.id, playerSuitDetail.Star), m_lbDesc);
			zbUIEffect.SetStars(m_spriteStars, playerSuitDetail.Star);
		}
		else
		{
			((VisualElement)m_lbUse).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((TextElement)m_lbLevel).text = "";
			((VisualElement)m_btnActive).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_btnDown).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_btnStar).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			zbPlayerSuitUpgradeCsvRecord suitStarUpgrade2 = zbSelf.Instance.suitGunHandler.GetSuitStarUpgrade(m_record.id, 1);
			long resourceCount = zbSelf.Instance.dataHandler.GetResourceCount(EResourceType.Prop, m_record.propId);
			string text = "";
			text = ((suitStarUpgrade2.costProp > resourceCount) ? zbUtil.AddHighlightStyle(resourceCount.ToString(), "FF0000", 20) : zbUtil.AddHighlightStyle(resourceCount.ToString(), "2DF421", 20));
			((TextElement)m_lbCost).text = text + "/" + suitStarUpgrade2.costProp;
			((TextElement)m_lbAttr).text = string.Format(zbUtil.Language("lang.10582"), suitStarUpgrade2.atk);
			zbLabelLinkUtil.BuildModLink(zbSelf.Instance.suitGunHandler.GetSkillDesc(m_record.id, 1), m_lbDesc);
		}
		zbPropCsvRecord recordByKey = zbDataConfig.Instance.propCsvTable.GetRecordByKey(m_record.propId);
		Texture2D val2 = zbAssetLoader.Instance.LoadResouce<Texture2D>(recordByKey.icon);
		m_spriteCost.style.backgroundImage = StyleBackground.op_Implicit(val2);
		string resourceFrameTex = zbResourceHelper.GetResourceFrameTex((EQuality)recordByKey.quality);
		Texture2D val3 = zbAssetLoader.Instance.LoadResouce<Texture2D>(resourceFrameTex);
		m_spriteCostFrame.style.backgroundImage = StyleBackground.op_Implicit(val3);
		bool flag = zbSelf.Instance.suitGunHandler.CanSuitActiveOrUpgrade(m_record.id);
		m_notifyActive.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		m_notifyStar.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
	}

	public void GetRefreshList(List<zbPlayerSuitCsvRecord> list, List<zbPlayerSuitCsvRecord> ownedList, List<zbPlayerSuitCsvRecord> otherList)
	{
		list.Clear();
		ownedList.Clear();
		otherList.Clear();
		foreach (zbPlayerSuitCsvRecord listReleasedRecord in m_listReleasedRecords)
		{
			if (zbSelf.Instance.suitGunHandler.GetPlayerSuitDetail(listReleasedRecord.id) != null)
			{
				ownedList.Add(listReleasedRecord);
			}
			else
			{
				otherList.Add(listReleasedRecord);
			}
		}
		ownedList.Sort((zbPlayerSuitCsvRecord a, zbPlayerSuitCsvRecord b) => a.sort.CompareTo(b.sort));
		otherList.Sort((zbPlayerSuitCsvRecord a, zbPlayerSuitCsvRecord b) => a.sort.CompareTo(b.sort));
		list.AddRange(ownedList);
		list.AddRange(otherList);
	}

	private void OnDown()
	{
		if (m_selectSuitId == zbSelf.Instance.GetSuitId())
		{
			zbWsMsgHandler.Instance.Suit_Remove_Req();
		}
		else
		{
			zbWsMsgHandler.Instance.Suit_Set_Req(m_selectSuitId);
		}
	}

	private void OnActive()
	{
		if (zbSelf.Instance.suitGunHandler.GetPlayerSuitDetail(m_selectSuitId) == null)
		{
			zbPlayerSuitCsvRecord recordByKey = zbDataConfig.Instance.playerSuitCsvTable.GetRecordByKey(m_selectSuitId);
			long resourceCount = zbSelf.Instance.dataHandler.GetResourceCount(EResourceType.Prop, recordByKey.propId);
			zbPlayerSuitUpgradeCsvRecord suitStarUpgrade = zbSelf.Instance.suitGunHandler.GetSuitStarUpgrade(recordByKey.id, 1);
			if (resourceCount < suitStarUpgrade.costProp)
			{
				zbUIMgr.Instance.ShowTip(EResourceType.Prop, recordByKey.propId, suitStarUpgrade.costProp);
			}
			else
			{
				zbWsMsgHandler.Instance.Suit_Active_Req(m_selectSuitId);
			}
		}
		else
		{
			zbUIMgr.Instance.ShowPlaySuitUpgrade(m_selectSuitId);
		}
	}

	private void OnSkill()
	{
		zbUIMgr.Instance.ShowPlayerSuitSkill();
	}

	private void OnGoto(ClickEvent evt, int day)
	{
		foreach (zbPlayerSuitItem item in m_SuitItem)
		{
			if (item.indexSort == indexSort + day)
			{
				item.OnTouch();
				zbUtil.ScrollHorizontalToIndex(m_scView, 4, ((VisualElement)m_scView).childCount, 166f, item.indexSort - 1);
				break;
			}
		}
	}

	private void OnPlayerGun()
	{
		zbUIMgr.Instance.ShowPlayerGun();
	}

	private void OnShow()
	{
		if (!(m_spineAvatar.GetAnimationName(0) == "show"))
		{
			m_spineAvatar.SetAnimation(0, "show", loop: false);
			m_spineAvatar.AddAnimation(0, "idle", loop: true);
		}
	}

	private void OnPreview()
	{
		if (!(m_spineAvatar.GetAnimationName(0) == "show"))
		{
			zbUIMgr.Instance.ShowPlayerSuitUpgradePreview(m_selectSuitId, EPreviewType.Suit);
		}
	}

	private void OnClose()
	{
		Hide();
	}

	protected override void UpdateAtShow()
	{
		base.UpdateAtShow();
		playActionName += Time.deltaTime;
		if (playActionName > 5f && !(m_spineAvatar.GetAnimationName(0) == "show"))
		{
			m_spineAvatar.SetAnimation(0, "action", loop: false);
			m_spineAvatar.AddAnimation(0, "idle", loop: true);
			playActionName = 0f;
		}
	}

	protected override void RefreshWhenAutoVisible()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		base.RefreshWhenAutoVisible();
		m_notifyGun.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.suitGunHandler.CanAllGunActiveOrUpgrade()));
	}
}
