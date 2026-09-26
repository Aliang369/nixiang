using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbPlayerGunWnd : zbBaseWnd
{
	private SkeletonAnimation m_spineAvatar;

	private MultiColumnListView m_listView;

	private ListView m_listDownView;

	private Button m_btnClose;

	[NonSerialized]
	public int m_selectGunId;

	private VisualElement m_bgBigDetail;

	private VisualElement m_bgSmaDetail;

	private Label m_lbName;

	private Label m_lbAttr;

	private Label m_lbCost;

	private Label m_lbTotleAttr;

	private Label m_lbUse_big;

	private Label m_lbDesc1_big;

	private Label m_lbDesc2_big;

	private VisualElement m_spriteSkillIcon_big;

	private Label m_lbUse_sma;

	private Label m_lbDesc1_sma;

	private VisualElement m_spriteSkillIcon_sma;

	private Button m_btnActive;

	private Button m_btnDown;

	private Button m_btnStar;

	private Button m_btnChange;

	private VisualElement m_spriteCostFrame;

	private VisualElement m_spriteCost;

	private List<zbPlayerGunItem> m_GunItem = new List<zbPlayerGunItem>();

	private VisualElement[] m_spriteStars = (VisualElement[])(object)new VisualElement[5];

	private Button m_btnPlayerSuit;

	private VisualElement m_notifySuit;

	private VisualElement m_notifyActive;

	private VisualElement m_notifyStar;

	private VisualElement m_spriteComm;

	private VisualElement m_spriteDown;

	private Button m_btnTouch;

	private List<int> m_sortDataList = new List<int>();

	private Label m_lbCanUse;

	[NonSerialized]
	private Bone boneShoot;

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void OnEnable()
	{
		//IL_030f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0454: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_spineAvatar = ((Component)m_GameObjectSceneRoot.transform.Find("GunAnimation")).GetComponent<SkeletonAnimation>();
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_bgBigDetail = UQueryExtensions.Q<VisualElement>(m_RootElement, "skill_detail1", (string)null);
		m_bgSmaDetail = UQueryExtensions.Q<VisualElement>(m_RootElement, "skill_detail2", (string)null);
		m_spriteComm = UQueryExtensions.Q<VisualElement>(m_RootElement, "skill_Comm", (string)null);
		m_spriteDown = UQueryExtensions.Q<VisualElement>(m_RootElement, "skill_Down", (string)null);
		m_lbCanUse = UQueryExtensions.Q<Label>(m_RootElement, "lb_canuse", (string)null);
		m_listView = UQueryExtensions.Q<MultiColumnListView>(m_RootElement, "listView", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		m_listDownView = UQueryExtensions.Q<ListView>(m_RootElement, "list", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listDownView, (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listDownView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listDownView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_lbAttr = UQueryExtensions.Q<Label>(m_RootElement, "lb_attr", (string)null);
		m_lbCost = UQueryExtensions.Q<Label>(m_RootElement, "lb_cost", (string)null);
		m_lbTotleAttr = UQueryExtensions.Q<Label>(m_RootElement, "lb_totleattr", (string)null);
		m_spriteCost = UQueryExtensions.Q<VisualElement>(m_RootElement, "icon_cost", (string)null);
		m_spriteCostFrame = UQueryExtensions.Q<VisualElement>(m_RootElement, "icon_frame", (string)null);
		m_btnActive = UQueryExtensions.Q<Button>(m_RootElement, "button_active", (string)null);
		m_btnDown = UQueryExtensions.Q<Button>(m_RootElement, "button_down", (string)null);
		m_btnStar = UQueryExtensions.Q<Button>(m_RootElement, "button_star", (string)null);
		m_btnChange = UQueryExtensions.Q<Button>(m_RootElement, "btn_change", (string)null);
		m_lbUse_big = UQueryExtensions.Q<Label>(m_bgBigDetail, "lb_use", (string)null);
		m_lbDesc1_big = UQueryExtensions.Q<Label>(m_bgBigDetail, "lb_desc1", (string)null);
		m_lbDesc2_big = UQueryExtensions.Q<Label>(m_bgBigDetail, "lb_desc2", (string)null);
		m_spriteSkillIcon_big = UQueryExtensions.Q<VisualElement>(m_bgBigDetail, "bg_Skillicon", (string)null);
		m_lbUse_sma = UQueryExtensions.Q<Label>(m_bgSmaDetail, "lb_use", (string)null);
		m_lbDesc1_sma = UQueryExtensions.Q<Label>(m_bgSmaDetail, "lb_desc1", (string)null);
		m_spriteSkillIcon_sma = UQueryExtensions.Q<VisualElement>(m_bgSmaDetail, "bg_Skillicon", (string)null);
		m_btnPlayerSuit = UQueryExtensions.Q<Button>(m_RootElement, "button_playerSuit", (string)null);
		m_notifySuit = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnPlayerSuit, (Vector2?)new Vector2(10f, 10f), 1f, true);
		for (int i = 0; i < 5; i++)
		{
			m_spriteStars[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "iconstar" + i, (string)null);
		}
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnChange, (Action<ClickEvent>)OnSelect, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnTouch, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnActive, (Action)OnActive, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnDown, (Action)OnDown, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnStar, (Action)OnActive, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnPlayerSuit, (Action)OnPlayerSuit, "", (TrickleDown)0);
		m_notifyActive = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnActive, (Vector2?)new Vector2(5f, 5f), 1f, true);
		m_notifyStar = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnStar);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Gun_Refresh, OnEventUpdateSuitList);
		RefreshGunList();
	}

	private void OnEventUpdateSuitList(object obj)
	{
		RefreshEvent();
	}

	public void RefreshCustom()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		m_spriteComm.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_spriteDown.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		RefreshEvent();
	}

	public void RefreshEvent()
	{
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		foreach (zbPlayerGunItem item in m_GunItem)
		{
			if (item.m_record != null)
			{
				item.Refresh();
				item.RefreshSelect(m_selectGunId == item.m_record.id);
			}
		}
		zbPlayerGunCsvRecord recordByKey = zbDataConfig.Instance.playerGunCsvTable.GetRecordByKey(m_selectGunId);
		if (recordByKey != null)
		{
			zbUtil.RefreshSpine(m_spineAvatar, recordByKey.frontBone + "_SkeletonData.asset", "animation", loop: true);
			boneShoot = ((SkeletonRenderer)m_spineAvatar).skeleton.FindBone("position");
			((Component)m_spineAvatar).transform.position = Vector2.op_Implicit(new Vector2(0f - boneShoot.X, ((Component)m_spineAvatar).transform.position.y));
		}
		string arg = zbUtil.AddHighlightStyle((zbServerData.sGameData.GunInfo.TotalDamagePct * 100f).ToString("F0") + "%", "ffe400", 30);
		((TextElement)m_lbTotleAttr).text = string.Format(zbUtil.Language("lang.10585"), arg);
		m_notifySuit.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.suitGunHandler.CanAllSuitActiveOrUpgrade()));
		RefreshDetail();
		RefreshBullet();
		CheckShouldConvert();
		GetAllSkillList();
	}

	private void CheckShouldConvert()
	{
		List<PlayerGunDetail> gunsShouldConvert = zbSelf.Instance.suitGunHandler.GetGunsShouldConvert();
		if (gunsShouldConvert.Count > 0)
		{
			zbUIMgr.Instance.ShowPlayerGunConvert(gunsShouldConvert.ToArray());
		}
	}

	public void RefreshDetail()
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_0394: Unknown result type (might be due to invalid IL or missing references)
		//IL_0399: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0420: Unknown result type (might be due to invalid IL or missing references)
		//IL_0450: Unknown result type (might be due to invalid IL or missing references)
		zbPlayerGunCsvRecord recordByKey = zbDataConfig.Instance.playerGunCsvTable.GetRecordByKey(m_selectGunId);
		((TextElement)m_lbName).text = recordByKey.name;
		Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>(recordByKey.skillIcon);
		if ((Object)(object)val != (Object)null)
		{
			m_spriteSkillIcon_big.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val));
			m_spriteSkillIcon_big.MarkDirtyRepaint();
		}
		if ((Object)(object)val != (Object)null)
		{
			m_spriteSkillIcon_sma.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val));
			m_spriteSkillIcon_sma.MarkDirtyRepaint();
		}
		for (int i = 0; i < 5; i++)
		{
			Texture2D val2 = zbAssetLoader.Instance.LoadResouce<Texture2D>("ui/image/suitGun/star_0.png");
			m_spriteStars[i].style.backgroundImage = StyleBackground.op_Implicit(val2);
		}
		PlayerGunDetail playerGunDetail = zbSelf.Instance.suitGunHandler.GetPlayerGunDetail(m_selectGunId);
		if (playerGunDetail != null)
		{
			((VisualElement)m_btnActive).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_btnDown).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_btnStar).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			if (zbSelf.Instance.GetGunId() == m_selectGunId)
			{
				((TextElement)m_btnDown).text = zbUtil.Language("lang.10579");
				((VisualElement)m_lbUse_big).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				((VisualElement)m_lbUse_sma).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			}
			else
			{
				((TextElement)m_btnDown).text = zbUtil.Language("lang.10580");
				((VisualElement)m_lbUse_big).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				((VisualElement)m_lbUse_sma).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
			zbPlayerGunUpgradeCsvRecord gunStarUpgrade = zbSelf.Instance.suitGunHandler.GetGunStarUpgrade(recordByKey.id, playerGunDetail.Star);
			((TextElement)m_lbAttr).text = string.Format(zbUtil.Language("lang.10581"), gunStarUpgrade.dmg * 100f);
			zbUIEffect.SetStars(m_spriteStars, playerGunDetail.Star);
		}
		else
		{
			((VisualElement)m_lbUse_big).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_lbUse_sma).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_btnActive).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_btnDown).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_btnStar).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			zbPlayerGunUpgradeCsvRecord gunStarUpgrade2 = zbSelf.Instance.suitGunHandler.GetGunStarUpgrade(recordByKey.id, 1);
			long resourceCount = zbSelf.Instance.dataHandler.GetResourceCount(EResourceType.Prop, recordByKey.propId);
			string text = "";
			text = ((gunStarUpgrade2.costProp > resourceCount) ? zbUtil.AddHighlightStyle(resourceCount.ToString(), "FF0000", 20) : zbUtil.AddHighlightStyle(resourceCount.ToString(), "2DF421", 20));
			((TextElement)m_lbCost).text = text + "/" + gunStarUpgrade2.costProp;
			((TextElement)m_lbAttr).text = string.Format(zbUtil.Language("lang.10581"), gunStarUpgrade2.dmg * 100f);
		}
		zbPropCsvRecord recordByKey2 = zbDataConfig.Instance.propCsvTable.GetRecordByKey(recordByKey.propId);
		Texture2D val3 = zbAssetLoader.Instance.LoadResouce<Texture2D>(recordByKey2.icon);
		if ((Object)(object)val3 != (Object)null)
		{
			m_spriteCost.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val3));
			m_spriteCost.MarkDirtyRepaint();
		}
		string resourceFrameTex = zbResourceHelper.GetResourceFrameTex((EQuality)recordByKey2.quality);
		Texture2D val4 = zbAssetLoader.Instance.LoadResouce<Texture2D>(resourceFrameTex);
		if ((Object)(object)val4 != (Object)null)
		{
			m_spriteCostFrame.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val4));
			m_spriteCostFrame.MarkDirtyRepaint();
		}
		m_notifyActive.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.suitGunHandler.CanGunActiveOrUpgrade(m_selectGunId)));
		m_notifyStar.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.suitGunHandler.CanGunActiveOrUpgrade(m_selectGunId)));
	}

	private void RefreshBullet()
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		PlayerGunDetail playerGunDetail = zbSelf.Instance.suitGunHandler.GetPlayerGunDetail(m_selectGunId);
		zbPlayerGunCsvRecord recordByKey = zbDataConfig.Instance.playerGunCsvTable.GetRecordByKey(m_selectGunId);
		m_bgBigDetail.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(recordByKey.quality < 6));
		m_bgSmaDetail.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(recordByKey.quality >= 6));
		int key;
		if (playerGunDetail == null || playerGunDetail.BulletId == 0)
		{
			key = recordByKey.id;
			((VisualElement)m_btnChange).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(playerGunDetail == null));
		}
		else
		{
			key = playerGunDetail.BulletId;
			((VisualElement)m_btnChange).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(recordByKey.quality < 6));
		}
		zbPlayerGunCsvRecord recordByKey2 = zbDataConfig.Instance.playerGunCsvTable.GetRecordByKey(key);
		zbLabelLinkUtil.BuildModLink(zbUtil.Language("lang.11154") + ": " + recordByKey2.desc, m_lbDesc1_big);
		zbLabelLinkUtil.BuildModLink(zbUtil.Language("lang.11154") + ": " + recordByKey2.desc, m_lbDesc1_sma);
		if (recordByKey.mod2 > 0)
		{
			zbLabelLinkUtil.BuildModLink(zbUtil.Language("lang.11155") + ": " + zbUtil.Language(recordByKey.desc2), m_lbDesc2_big);
		}
	}

	private void RefreshGunList()
	{
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Expected O, but got Unknown
		if (m_listView.columns.Count > 0)
		{
			return;
		}
		List<zbPlayerGunCsvRecord> showRecords = new List<zbPlayerGunCsvRecord>();
		foreach (KeyValuePair<int, zbPlayerGunCsvRecord> record2 in zbDataConfig.Instance.playerGunCsvTable.GetRecords().records)
		{
			if (record2.Value.id != 2 && zbVersionMgr.IsReleased(record2.Value.preShow))
			{
				showRecords.Add(record2.Value);
			}
		}
		showRecords.Sort((zbPlayerGunCsvRecord a, zbPlayerGunCsvRecord b) => a.sort.CompareTo(b.sort));
		if (m_selectGunId == 0)
		{
			m_selectGunId = showRecords[0].id;
		}
		for (int num = 0; num < 5; num++)
		{
			int columnIndex = num;
			m_listView.columns.Add(new Column
			{
				width = Length.op_Implicit(144f),
				makeCell = () => (VisualElement)(object)new zbPlayerGunItem(this),
				bindCell = delegate(VisualElement element, int row)
				{
					//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
					//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
					int num2 = row * 5 + columnIndex;
					if (num2 >= 0 && num2 < showRecords.Count)
					{
						zbPlayerGunCsvRecord record = showRecords[num2];
						zbPlayerGunItem zbPlayerGunItem2 = element as zbPlayerGunItem;
						zbPlayerGunItem2.RefreshBase(record);
						zbPlayerGunItem2.Refresh();
						zbPlayerGunItem2.RefreshSelect(m_selectGunId == zbPlayerGunItem2.m_record.id);
						if (!m_GunItem.Contains(zbPlayerGunItem2))
						{
							m_GunItem.Add(zbPlayerGunItem2);
						}
						element.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
					}
					else
					{
						element.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
					}
				}
			});
		}
		((BaseVerticalCollectionView)m_listView).itemsSource = new List<int>(Enumerable.Range(0, Mathf.CeilToInt((float)showRecords.Count / 5f)));
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_listView, "unity-multi-column-view__header-container", (string)null).style.height = StyleLength.op_Implicit(0f);
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_listView, "unity-multi-column-view__header-container", (string)null).style.opacity = StyleFloat.op_Implicit(0f);
	}

	private void OnDown()
	{
		if (m_selectGunId == zbSelf.Instance.GetGunId())
		{
			zbWsMsgHandler.Instance.Gun_Remove_Req();
		}
		else
		{
			zbWsMsgHandler.Instance.Gun_Set_Req(m_selectGunId);
		}
	}

	private void OnActive()
	{
		if (zbSelf.Instance.suitGunHandler.GetPlayerGunDetail(m_selectGunId) == null)
		{
			zbPlayerGunCsvRecord recordByKey = zbDataConfig.Instance.playerGunCsvTable.GetRecordByKey(m_selectGunId);
			long resourceCount = zbSelf.Instance.dataHandler.GetResourceCount(EResourceType.Prop, recordByKey.propId);
			zbPlayerGunUpgradeCsvRecord gunStarUpgrade = zbSelf.Instance.suitGunHandler.GetGunStarUpgrade(recordByKey.id, 1);
			if (resourceCount < gunStarUpgrade.costProp)
			{
				zbUIMgr.Instance.ShowTip(EResourceType.Prop, recordByKey.propId, gunStarUpgrade.costProp);
			}
			else
			{
				zbWsMsgHandler.Instance.Gun_Active_Req(m_selectGunId);
			}
		}
		else
		{
			zbUIMgr.Instance.ShowPlayGunUpgrade(m_selectGunId);
		}
	}

	private void OnPlayerSuit()
	{
		zbUIMgr.Instance.ShowPlayerSuit();
	}

	private void OnSelect(ClickEvent evt)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		m_spriteComm.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_spriteDown.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		GetAllSkillList();
	}

	public void RefreshList()
	{
		((VisualElement)m_listDownView).Clear();
		((BaseVerticalCollectionView)m_listDownView).itemsSource = null;
		m_listDownView.makeItem = () => (VisualElement)(object)new zbPlayerGunSkillItem(this);
		m_listDownView.bindItem = delegate(VisualElement element, int index)
		{
			(element as zbPlayerGunSkillItem).Init(m_selectGunId, m_sortDataList[index]);
		};
		((BaseVerticalCollectionView)m_listDownView).itemsSource = m_sortDataList;
		((BaseVerticalCollectionView)m_listDownView).ScrollToItem(0);
	}

	private void GetAllSkillList()
	{
		zbPlayerGunCsvRecord recordByKey = zbDataConfig.Instance.playerGunCsvTable.GetRecordByKey(m_selectGunId);
		if (recordByKey == null || recordByKey.quality < 6)
		{
			return;
		}
		PlayerGunDetail playerGunDetail = zbSelf.Instance.suitGunHandler.GetPlayerGunDetail(m_selectGunId);
		if (playerGunDetail == null)
		{
			return;
		}
		m_sortDataList.Clear();
		foreach (PlayerGunDetail gun in zbServerData.sGameData.GunInfo.GunList)
		{
			if (playerGunDetail.BulletId == gun.GunId)
			{
				m_sortDataList.Add(gun.GunId);
			}
		}
		foreach (PlayerGunDetail gun2 in zbServerData.sGameData.GunInfo.GunList)
		{
			if (gun2.GunId != 2 && m_selectGunId != gun2.GunId && playerGunDetail.BulletId != gun2.GunId)
			{
				m_sortDataList.Add(gun2.GunId);
			}
		}
		RefreshList();
		((TextElement)m_lbCanUse).text = string.Format(zbUtil.Language("lang.10587"), m_sortDataList.Count);
	}

	private void OnClose()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (m_spriteDown.style.display == StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0))
		{
			OnTouch();
		}
		else
		{
			Hide();
		}
	}

	private void OnTouch()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		m_spriteDown.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_spriteComm.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
	}

	protected override void RefreshWhenAutoVisible()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		base.RefreshWhenAutoVisible();
		m_notifySuit.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.suitGunHandler.CanAllSuitActiveOrUpgrade()));
	}
}
