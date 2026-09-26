using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbWallTabSet
{
	private zbWallWnd m_wndParent;

	private VisualElement m_RootElement;

	private Label m_lbDerc;

	private VisualElement m_nodeSkill;

	private VisualElement m_spriteSkill;

	private Label m_lbLevel;

	private Label m_lbDesc;

	private Label m_lbUsed;

	private Button m_btnList;

	private VisualElement m_nodeActive;

	private Label m_lbCost;

	private zbItem m_item = new zbItem();

	private Button m_btnActive;

	private Button m_btnSet;

	private Button m_btnStar;

	private Label m_lbTotalDerc;

	private ScrollView m_scrollView;

	private List<zbWallItem> m_wallItems = new List<zbWallItem>();

	private VisualElement m_notifyActive;

	private VisualElement m_notifyStar;

	private zbWallCsvRecord[] m_allRecords;

	private zbWallCsvRecord m_record;

	private WallDetail m_data;

	public int m_selectId;

	private bool m_isFirstIn = true;

	public void OnEnable(zbWallWnd wndParent, VisualElement root)
	{
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		m_RootElement = root;
		m_lbDerc = UQueryExtensions.Q<Label>(m_RootElement, "lb_derc", (string)null);
		m_nodeSkill = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_skill", (string)null);
		m_spriteSkill = UQueryExtensions.Q<VisualElement>(m_nodeSkill, "bg_skill", (string)null);
		m_lbLevel = UQueryExtensions.Q<Label>(m_nodeSkill, "lb_level", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_nodeSkill, "lb_desc", (string)null);
		m_lbUsed = UQueryExtensions.Q<Label>(m_nodeSkill, "lb_used", (string)null);
		m_btnList = UQueryExtensions.Q<Button>(m_nodeSkill, "btn_list", (string)null);
		m_nodeActive = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_active", (string)null);
		m_lbCost = UQueryExtensions.Q<Label>(m_nodeActive, "lb_cost", (string)null);
		m_item = UQueryExtensions.Q<zbItem>(m_nodeActive, "item", (string)null);
		m_item.OnEnable(m_wndParent);
		m_btnActive = UQueryExtensions.Q<Button>(m_nodeActive, "btn_active", (string)null);
		m_btnSet = UQueryExtensions.Q<Button>(m_RootElement, "btn_set", (string)null);
		m_btnStar = UQueryExtensions.Q<Button>(m_RootElement, "btn_star", (string)null);
		m_lbTotalDerc = UQueryExtensions.Q<Label>(m_RootElement, "lb_totalDerc", (string)null);
		m_scrollView = UQueryExtensions.Q<ScrollView>(m_RootElement, "scrollView", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnList, (Action)OnList, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnActive, (Action)OnActive, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSet, (Action)OnSet, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnStar, (Action)OnStar, "", (TrickleDown)0);
		m_notifyActive = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnActive, (Vector2?)new Vector2(-3f, -5f), 1f, true);
		m_notifyStar = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnStar, (Vector2?)new Vector2(-3f, -5f), 1f, true);
		InitListView();
	}

	private void InitListView()
	{
		m_allRecords = (from r in zbDataConfig.Instance.wallCsvTable.GetRecords().records.Values
			where zbVersionMgr.IsReleased(r.preShow)
			orderby r.sort
			select r).ToArray();
		zbWallCsvRecord[] allRecords = m_allRecords;
		for (int num = 0; num < allRecords.Length; num++)
		{
			_ = allRecords[num];
			zbWallItem zbWallItem2 = new zbWallItem(m_wndParent, this);
			m_wallItems.Add(zbWallItem2);
			((VisualElement)m_scrollView).Add((VisualElement)(object)zbWallItem2);
		}
	}

	public void Refresh()
	{
		for (int i = 0; i < m_wallItems.Count; i++)
		{
			zbWallCsvRecord record = m_allRecords[i];
			m_wallItems[i].Refresh(record);
		}
		if (m_isFirstIn)
		{
			RefreshSelectList(zbServerData.sGameData.WallInfo.WallId);
		}
		else
		{
			RefreshSelectList(m_selectId);
		}
		((TextElement)m_lbTotalDerc).text = zbUtil.Language("lang.10840") + $"<color=#FFE600>+{zbServerData.sGameData.WallInfo.TotalImmunity}</color> ";
	}

	public void RefreshSelectList(int selectId)
	{
		m_selectId = selectId;
		int num = -1;
		for (int i = 0; i < m_wallItems.Count; i++)
		{
			m_wallItems[i].RefreshSelect();
			zbWallCsvRecord zbWallCsvRecord2 = m_allRecords[i];
			if (m_selectId == zbWallCsvRecord2.id)
			{
				num = i;
			}
		}
		RefreshDetail();
		if (num >= 0 && m_isFirstIn)
		{
			m_isFirstIn = false;
			int indexToScroll = num;
			((VisualElement)m_scrollView).schedule.Execute((Action)delegate
			{
				zbUtil.ScrollHorizontalToIndex(m_scrollView, 4, ((VisualElement)m_scrollView).childCount, 174f, indexToScroll);
			}).ExecuteLater(50L);
		}
	}

	private void RefreshDetail()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0260: Unknown result type (might be due to invalid IL or missing references)
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c4: Unknown result type (might be due to invalid IL or missing references)
		m_notifyActive.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.wallHandler.HasActiveNotify(m_selectId)));
		m_notifyStar.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.wallHandler.HasStarNotify(m_selectId)));
		m_wndParent.RefreshDetail(m_selectId);
		m_record = zbDataConfig.Instance.wallCsvTable.GetRecordByKey(m_selectId);
		m_data = zbSelf.Instance.wallHandler.GetWallDetail(m_selectId);
		if (m_record == null)
		{
			return;
		}
		((TextElement)m_btnSet).text = ((zbServerData.sGameData.WallInfo.WallId == m_record.id) ? zbUtil.Language("lang.10822") : zbUtil.Language("lang.10821"));
		if (zbServerData.sGameData.WallInfo.WallId == m_record.id)
		{
			((VisualElement)m_btnSet).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btnGray_260x88.png"));
		}
		else
		{
			((VisualElement)m_btnSet).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_260_88_green.png"));
		}
		if (m_record.open == 1)
		{
			((VisualElement)m_lbDerc).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_nodeSkill.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_nodeActive.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_btnSet).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_btnStar).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			return;
		}
		((VisualElement)m_lbDerc).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_data == null));
		m_nodeSkill.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_nodeActive.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_data != null));
		((VisualElement)m_btnSet).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_data == null));
		((VisualElement)m_btnStar).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_data == null));
		m_spriteSkill.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(m_record.skillIcon));
		((VisualElement)m_lbUsed).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbServerData.sGameData.WallInfo.UseList.Contains(m_record.id)));
		if (m_data != null)
		{
			zbWallStarUpgradeCsvRecord zbWallStarUpgradeCsvRecord2 = zbDataConfig.Instance.wallStarUpgradeCsvTable.GetRecords().records.FirstOrDefault((zbWallStarUpgradeCsvRecord r) => r.id == m_record.starUpgradeId && r.star == m_data.Star);
			if (zbWallStarUpgradeCsvRecord2 != null)
			{
				((TextElement)m_lbDerc).text = zbUtil.Language("lang.10839") + $"<color=#FFE600>+{zbWallStarUpgradeCsvRecord2.damageImmunity}</color> ";
			}
			((TextElement)m_lbLevel).text = "Lv." + m_data.Star;
			zbLabelLinkUtil.BuildModLink(zbSelf.Instance.wallHandler.GetSkillDesc(m_record.id, m_data.Star), m_lbDesc);
			return;
		}
		((TextElement)m_lbLevel).text = "";
		((TextElement)m_lbDesc).text = zbSelf.Instance.wallHandler.GetSkillDesc(m_record.id, 1);
		m_item.Refresh(new zbItemData(EResourceType.Prop, m_record.propId, 1L), isNameVis: false, isCountVis: false);
		long resourceCount = zbSelf.Instance.dataHandler.GetResourceCount(EResourceType.Prop, m_record.propId);
		((TextElement)m_lbCost).text = zbUtil.GetNeedHaveRichString(resourceCount, 1L);
		if (1 > resourceCount)
		{
			m_item.SetTouchCallback(delegate
			{
				zbUIMgr.Instance.ShowTip(EResourceType.Prop, m_record.propId, 1L);
			});
		}
		else
		{
			m_item.SetTouchCallback(delegate
			{
				zbUIMgr.Instance.ShowTip(EResourceType.Prop, m_record.propId, 0L);
			});
		}
	}

	private void OnList()
	{
		if (m_selectId != 1)
		{
			zbUIMgr.Instance.ShowPlayerSuitUpgradePreview(m_selectId, EPreviewType.Wall);
		}
	}

	private void OnActive()
	{
		if (zbSelf.Instance.dataHandler.GetResourceCount(EResourceType.Prop, m_record.propId) < 1)
		{
			zbUIMgr.Instance.ShowTip(EResourceType.Prop, m_record.propId, 1L);
		}
		else
		{
			zbWsMsgHandler.Instance.Wall_Active_Req(m_record.id);
		}
	}

	private void OnSet()
	{
		if (zbServerData.sGameData.WallInfo.WallId != m_record.id)
		{
			zbWsMsgHandler.Instance.Wall_Set_Req(m_record.id);
		}
	}

	private void OnStar()
	{
		zbUIMgr.Instance.ShowWallStar(m_record.id);
	}
}
