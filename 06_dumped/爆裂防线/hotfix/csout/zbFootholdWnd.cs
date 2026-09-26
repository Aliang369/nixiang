using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbFootholdWnd : zbBaseWnd
{
	private zbSpineScene m_spineScene;

	private SkeletonAnimation m_spineDikuai;

	public SpriteRenderer nodeTarget;

	private VisualElement m_bgPick;

	private Label m_lbName;

	private VisualElement m_nodeStar;

	private VisualElement[] m_stars = (VisualElement[])(object)new VisualElement[5];

	private Label m_lbDamage;

	private Label m_lbDesc;

	private VisualElement m_spriteSkill;

	private Button m_btnList;

	private VisualElement m_nodeSkill;

	private Button m_btnActive;

	private VisualElement m_spriteCostFrame;

	private VisualElement m_spriteCost;

	private Label m_lbCost;

	private Button m_btnUse;

	private Button m_btnStar;

	private Label m_lbAllDamage;

	private Button m_btnClose;

	private MultiColumnListView m_listView;

	private int columnCount = 4;

	private List<zbFootholdItem> m_items = new List<zbFootholdItem>();

	private VisualElement m_notifyActive;

	private VisualElement m_notifyStar;

	private int m_index;

	public int m_selectId;

	protected override void OnEnable()
	{
		//IL_032e: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_spineScene = ((Component)m_GameObjectSceneRoot.transform.Find("spineScene")).GetComponent<zbSpineScene>();
		GameObject gameObject = ((Component)GetMyGameObjectSceneRoot().transform.Find("spine_dikuai")).gameObject;
		m_spineDikuai = gameObject.GetComponent<SkeletonAnimation>();
		nodeTarget = ((Component)GetMyGameObjectSceneRoot().transform.Find("nodeTarget")).GetComponent<SpriteRenderer>();
		m_bgPick = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_pick", (string)null);
		m_nodeSkill = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_skill", (string)null);
		m_spriteSkill = UQueryExtensions.Q<VisualElement>(m_nodeSkill, "bg_skill", (string)null);
		m_btnList = UQueryExtensions.Q<Button>(m_nodeSkill, "btn_list", (string)null);
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_nodeStar = UQueryExtensions.Q<VisualElement>(m_RootElement, "stars", (string)null);
		for (int i = 0; i < 5; i++)
		{
			m_stars[i] = UQueryExtensions.Q<VisualElement>(m_nodeStar, "star" + i, (string)null);
		}
		m_lbDamage = UQueryExtensions.Q<Label>(m_RootElement, "lb_damage", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_btnActive = UQueryExtensions.Q<Button>(m_RootElement, "btn_active", (string)null);
		m_spriteCostFrame = UQueryExtensions.Q<VisualElement>(m_RootElement, "icon_frame", (string)null);
		m_spriteCost = UQueryExtensions.Q<VisualElement>(m_RootElement, "icon_cost", (string)null);
		m_lbCost = UQueryExtensions.Q<Label>(m_RootElement, "lb_cost", (string)null);
		m_btnUse = UQueryExtensions.Q<Button>(m_RootElement, "btn_use", (string)null);
		m_btnStar = UQueryExtensions.Q<Button>(m_RootElement, "btn_star", (string)null);
		m_lbAllDamage = UQueryExtensions.Q<Label>(m_RootElement, "lb_allDamage", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnList, (Action)OnList, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>(m_bgPick, (Action<ClickEvent>)OnMonsterPreview, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnActive, (Action)OnActive, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnUse, (Action)OnUse, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnStar, (Action)OnActive, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_listView = UQueryExtensions.Q<MultiColumnListView>(m_RootElement, "listView", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		m_notifyActive = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnActive, (Vector2?)new Vector2(5f, 5f), 1f, true);
		m_notifyStar = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnStar);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_FootholdInfo, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_PvpSet_ModSubmit, OnEventUpdate);
		InitColumns();
	}

	private void InitColumns()
	{
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Expected O, but got Unknown
		if (m_listView.columns.Count > 0)
		{
			return;
		}
		List<zbFootholdCsvRecord> records = new List<zbFootholdCsvRecord>();
		foreach (zbFootholdCsvRecord value in zbDataConfig.Instance.footholdCsvTable.GetRecords().records.Values)
		{
			if (value.show == 1)
			{
				if (m_selectId == 0)
				{
					m_selectId = value.id;
				}
				records.Add(value);
			}
		}
		for (int i = 0; i < columnCount; i++)
		{
			int columnIndex = i;
			m_listView.columns.Add(new Column
			{
				width = Length.op_Implicit(170f),
				makeCell = () => (VisualElement)(object)new zbFootholdItem(this),
				bindCell = delegate(VisualElement element, int row)
				{
					//IL_0086: Unknown result type (might be due to invalid IL or missing references)
					//IL_0074: Unknown result type (might be due to invalid IL or missing references)
					int num = row * columnCount + columnIndex;
					if (num >= 0 && num < records.Count)
					{
						zbFootholdCsvRecord record = records[num];
						zbFootholdItem zbFootholdItem2 = element as zbFootholdItem;
						zbFootholdItem2.InitRecord(record);
						zbFootholdItem2.Refresh();
						m_items.Add(zbFootholdItem2);
						element.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
					}
					else
					{
						element.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
					}
				}
			});
		}
		((BaseVerticalCollectionView)m_listView).itemsSource = new List<int>(Enumerable.Range(0, Mathf.CeilToInt((float)records.Count / (float)columnCount)));
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_listView, "unity-multi-column-view__header-container", (string)null).style.height = StyleLength.op_Implicit(0f);
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_listView, "unity-multi-column-view__header-container", (string)null).style.opacity = StyleFloat.op_Implicit(0f);
	}

	public void RefreshCustom(int index)
	{
		m_index = index;
		RefreshSelectList(m_selectId);
		string text = zbUtil.AddHighlightStyle((zbSelf.Instance.guardHandler.GetAllDamageUp() * 100f).ToString("F0") + "%", "ffe400", 30);
		((TextElement)m_lbAllDamage).text = zbUtil.Language("lang.10863") + " " + text;
		CheckShouldConvert();
	}

	public void RefreshDetail()
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_031c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0332: Unknown result type (might be due to invalid IL or missing references)
		//IL_0348: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_044e: Unknown result type (might be due to invalid IL or missing references)
		//IL_047c: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_04dc: Unknown result type (might be due to invalid IL or missing references)
		zbFootholdCsvRecord recordByKey = zbDataConfig.Instance.footholdCsvTable.GetRecordByKey(m_selectId);
		if (recordByKey == null)
		{
			return;
		}
		zbUtil.RefreshSpine(m_spineDikuai, recordByKey.mainScene + "_SkeletonData.asset", "in");
		m_spineDikuai.AnimationState.AddAnimation(0, "loop", true, 0f);
		m_spriteSkill.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.skillIcon));
		((TextElement)m_lbName).text = recordByKey.name;
		if (m_selectId == zbServerData.sGameData.RoleBaseInfo.FootholdId)
		{
			((VisualElement)m_btnUse).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btnGray_280x80.png"));
			((TextElement)m_btnUse).text = zbUtil.Language("lang.10162");
		}
		else
		{
			((VisualElement)m_btnUse).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_280_80_green.png"));
			((TextElement)m_btnUse).text = zbUtil.Language("lang.10161");
		}
		if (recordByKey.open == 1)
		{
			m_nodeStar.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((TextElement)m_lbDamage).text = "";
			((VisualElement)m_btnActive).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_btnUse).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_btnStar).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_lbDesc).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_nodeSkill.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			return;
		}
		m_nodeSkill.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_nodeStar.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		((VisualElement)m_lbDesc).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		FootholdDetail footholdDetail = zbSelf.Instance.guardHandler.GetFootholdDetail(m_selectId);
		zbUIEffect.SetStars(m_stars, footholdDetail?.Star ?? 0);
		int toStar = 1;
		if (footholdDetail != null)
		{
			toStar = footholdDetail.Star;
		}
		zbLabelLinkUtil.BuildModLink(zbSelf.Instance.guardHandler.GetFootholdEffectDesc(recordByKey, toStar), m_lbDesc);
		zbUtil.AutoFontSize((TextElement)(object)m_lbDesc, 22f, 14f, 555f, 92f);
		if (footholdDetail != null)
		{
			((VisualElement)m_btnActive).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_btnUse).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_btnStar).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			zbFootholdStarUpgradeCsvRecord footholdStarUpgradeRecord = zbSelf.Instance.guardHandler.GetFootholdStarUpgradeRecord(recordByKey.id, footholdDetail.Star);
			((TextElement)m_lbDamage).text = zbUtil.Language("lang.10862") + "+" + $"{footholdStarUpgradeRecord.dmg:0.##%}";
		}
		else
		{
			((VisualElement)m_btnActive).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_btnUse).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_btnStar).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			zbFootholdStarUpgradeCsvRecord footholdStarUpgradeRecord2 = zbSelf.Instance.guardHandler.GetFootholdStarUpgradeRecord(recordByKey.id, 1);
			long resourceCount = zbSelf.Instance.dataHandler.GetResourceCount(EResourceType.Prop, recordByKey.propId);
			string text = "";
			text = ((footholdStarUpgradeRecord2.costProp > resourceCount) ? zbUtil.AddHighlightStyle(resourceCount.ToString(), "FF0000", 20) : zbUtil.AddHighlightStyle(resourceCount.ToString(), "2DF421", 20));
			((TextElement)m_lbCost).text = text + "/" + footholdStarUpgradeRecord2.costProp;
			((TextElement)m_lbDamage).text = zbUtil.Language("lang.10862") + "+" + $"{footholdStarUpgradeRecord2.dmg:0.##%}";
		}
		zbPropCsvRecord recordByKey2 = zbDataConfig.Instance.propCsvTable.GetRecordByKey(recordByKey.propId);
		m_spriteCost.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey2.icon));
		string resourceFrameTex = zbResourceHelper.GetResourceFrameTex((EQuality)recordByKey2.quality);
		m_spriteCostFrame.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resourceFrameTex));
		m_notifyActive.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.guardHandler.HasFootholdNotify(m_selectId)));
		m_notifyStar.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.guardHandler.HasFootholdNotify(m_selectId)));
	}

	private void CheckShouldConvert()
	{
		List<FootholdDetail> footholdsShouldConvert = zbSelf.Instance.guardHandler.GetFootholdsShouldConvert();
		if (footholdsShouldConvert.Count > 0)
		{
			zbUIMgr.Instance.ShowFootholdConvert(footholdsShouldConvert.ToArray());
		}
	}

	public void RefreshSelectList(int selectId)
	{
		m_selectId = selectId;
		foreach (zbFootholdItem item in m_items)
		{
			item.Refresh();
		}
		RefreshDetail();
	}

	private void OnMonsterPreview(ClickEvent evt)
	{
		zbSelf.Instance.monsterHandler.OnShowMonsterPreview(evt);
	}

	private void OnActive()
	{
		if (zbSelf.Instance.guardHandler.GetFootholdDetail(m_selectId) == null)
		{
			zbFootholdCsvRecord recordByKey = zbDataConfig.Instance.footholdCsvTable.GetRecordByKey(m_selectId);
			long resourceCount = zbSelf.Instance.dataHandler.GetResourceCount(EResourceType.Prop, recordByKey.propId);
			zbFootholdStarUpgradeCsvRecord footholdStarUpgradeRecord = zbSelf.Instance.guardHandler.GetFootholdStarUpgradeRecord(recordByKey.id, 1);
			if (resourceCount < footholdStarUpgradeRecord.costProp)
			{
				zbUIMgr.Instance.ShowTip(EResourceType.Prop, recordByKey.propId, footholdStarUpgradeRecord.costProp);
			}
			else
			{
				zbWsMsgHandler.Instance.Foothold_Active_Req(m_selectId);
			}
		}
		else
		{
			zbUIMgr.Instance.ShowFootholdStar(m_selectId);
		}
	}

	private void OnUse()
	{
		if (m_selectId != zbServerData.sGameData.RoleBaseInfo.FootholdId)
		{
			zbWsMsgHandler.Instance.Foothold_Set_Req(m_index, m_selectId);
		}
	}

	private void OnList()
	{
		zbUIMgr.Instance.ShowPlayerSuitUpgradePreview(m_selectId, EPreviewType.Foothold);
	}

	public void OnClose()
	{
		Hide();
	}

	private void OnEventUpdate(object data)
	{
		RefreshCustom(m_index);
	}
}
