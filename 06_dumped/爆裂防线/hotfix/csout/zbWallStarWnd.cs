using System;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbWallStarWnd : zbBaseWnd
{
	private Button m_btnTouchTest;

	private Button m_btnClose;

	private Label m_lbName;

	private VisualElement m_nodeCurStar;

	private VisualElement[] m_curStars = (VisualElement[])(object)new VisualElement[5];

	private VisualElement m_nodeNextStar;

	private VisualElement[] m_nextStars = (VisualElement[])(object)new VisualElement[5];

	private VisualElement m_arrowStar;

	private VisualElement m_nodeCurSkill;

	private VisualElement m_curSkill;

	private VisualElement[] m_curSmallStars = (VisualElement[])(object)new VisualElement[5];

	private Button m_btnCurSkill;

	private VisualElement m_nodeNextSkill;

	private VisualElement m_nextSkill;

	private VisualElement[] m_nextSmallStars = (VisualElement[])(object)new VisualElement[5];

	private Button m_btnNextSkill;

	private VisualElement m_arrowSkill;

	private Label m_lbCurAttr;

	private Label m_lbNextAttr;

	private VisualElement m_arrowAttr;

	private Label[] m_costLabels = (Label[])(object)new Label[2];

	private zbItem[] m_items = new zbItem[2];

	private zbItem m_convertItem;

	private Label m_lbConvertItem;

	private Label m_lbOr;

	private Button m_btnStar;

	private Label m_lbMax;

	private Label m_lbMaxDesc;

	private VisualElement m_notify;

	private int m_wallId;

	private zbWallCsvRecord m_record;

	private WallDetail m_data;

	private zbWallStarUpgradeCsvRecord m_curStarRecord;

	private zbWallStarUpgradeCsvRecord m_nextStarRecord;

	private int[][] m_starCostList;

	protected override void OnEnable()
	{
		//IL_033c: Unknown result type (might be due to invalid IL or missing references)
		//IL_041b: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_btnTouchTest = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouchTest, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		VisualElement val = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_star", (string)null);
		m_nodeCurStar = UQueryExtensions.Q<VisualElement>(val, "curStar", (string)null);
		m_nodeNextStar = UQueryExtensions.Q<VisualElement>(val, "nextStar", (string)null);
		for (int i = 0; i < 5; i++)
		{
			m_curStars[i] = UQueryExtensions.Q<VisualElement>(m_nodeCurStar, "star" + i, (string)null);
			m_nextStars[i] = UQueryExtensions.Q<VisualElement>(m_nodeNextStar, "star" + i, (string)null);
		}
		m_arrowStar = UQueryExtensions.Q<VisualElement>(val, "arrow", (string)null);
		VisualElement val2 = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_skill", (string)null);
		m_nodeCurSkill = UQueryExtensions.Q<VisualElement>(val2, "curSkill", (string)null);
		m_curSkill = UQueryExtensions.Q<VisualElement>(m_nodeCurSkill, "bg_skill", (string)null);
		for (int j = 0; j < 5; j++)
		{
			m_curSmallStars[j] = UQueryExtensions.Q<VisualElement>(m_nodeCurSkill, "star" + j, (string)null);
		}
		m_btnCurSkill = UQueryExtensions.Q<Button>(m_nodeCurSkill, "btn_skill", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCurSkill, (Action<ClickEvent>)OnCurSkill, "", (TrickleDown)0);
		m_nodeNextSkill = UQueryExtensions.Q<VisualElement>(val2, "nextSkill", (string)null);
		m_nextSkill = UQueryExtensions.Q<VisualElement>(m_nodeNextSkill, "bg_skill", (string)null);
		for (int k = 0; k < 5; k++)
		{
			m_nextSmallStars[k] = UQueryExtensions.Q<VisualElement>(m_nodeNextSkill, "star" + k, (string)null);
		}
		m_btnNextSkill = UQueryExtensions.Q<Button>(m_nodeNextSkill, "btn_skill", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnNextSkill, (Action<ClickEvent>)OnNextSkill, "", (TrickleDown)0);
		m_arrowSkill = UQueryExtensions.Q<VisualElement>(val2, "arrow", (string)null);
		VisualElement val3 = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_attr", (string)null);
		m_lbCurAttr = UQueryExtensions.Q<Label>(val3, "lb_curAttr", (string)null);
		m_lbNextAttr = UQueryExtensions.Q<Label>(val3, "lb_nextAttr", (string)null);
		m_arrowAttr = UQueryExtensions.Q<VisualElement>(val3, "arrow", (string)null);
		for (int l = 0; l < 2; l++)
		{
			m_costLabels[l] = UQueryExtensions.Q<Label>(m_RootElement, "lb_cost" + l, (string)null);
			m_items[l] = UQueryExtensions.Q<zbItem>(m_RootElement, "item" + l, (string)null);
			m_items[l].OnEnable(this);
			((VisualElement)m_costLabels[l]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		m_lbConvertItem = UQueryExtensions.Q<Label>(m_RootElement, "lb_convertItem", (string)null);
		m_convertItem = UQueryExtensions.Q<zbItem>(m_RootElement, "convertItem", (string)null);
		m_convertItem.OnEnable(this);
		m_lbOr = UQueryExtensions.Q<Label>(m_RootElement, "lb_or", (string)null);
		m_btnStar = UQueryExtensions.Q<Button>(m_RootElement, "btn_star", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnStar, (Action<ClickEvent>)OnStar, "", (TrickleDown)0);
		m_lbMax = UQueryExtensions.Q<Label>(m_RootElement, "lb_max", (string)null);
		m_lbMaxDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_maxDesc", (string)null);
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnStar, (Vector2?)new Vector2(-3f, -5f), 1f, true);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_WallInfo, OnEventUpdate);
		AddDark();
	}

	private void OnEventUpdate(object obj)
	{
		RefreshCustom(m_wallId);
	}

	public void RefreshCustom(int wallId)
	{
		m_wallId = wallId;
		m_record = zbDataConfig.Instance.wallCsvTable.GetRecordByKey(m_wallId);
		m_data = zbSelf.Instance.wallHandler.GetWallDetail(m_wallId);
		if (m_data != null)
		{
			((TextElement)m_lbMaxDesc).text = zbSelf.Instance.itemHandler.GetStarMaxDesc(m_record.propId);
			m_curStarRecord = zbDataConfig.Instance.wallStarUpgradeCsvTable.GetRecords().records.FirstOrDefault((zbWallStarUpgradeCsvRecord r) => r.id == m_record.starUpgradeId && r.star == m_data.Star);
			m_nextStarRecord = zbDataConfig.Instance.wallStarUpgradeCsvTable.GetRecords().records.FirstOrDefault((zbWallStarUpgradeCsvRecord r) => r.id == m_record.starUpgradeId && r.star == m_data.Star + 1);
			RefreshAttrs();
			RefreshCostList();
		}
	}

	public void RefreshAttrs()
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)m_lbName).text = m_record.name;
		if (m_curStarRecord != null)
		{
			zbUIEffect.SetStars(m_curStars, m_curStarRecord.star);
			m_curSkill.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(m_record.skillIcon));
			zbUIEffect.SetStars(m_curSmallStars, m_curStarRecord.star, moreVis: false);
			((TextElement)m_lbCurAttr).text = zbUtil.Language("lang.10839") + "+" + m_curStarRecord.damageImmunity;
		}
		if (m_nextStarRecord != null)
		{
			zbUIEffect.SetStars(m_nextStars, m_nextStarRecord.star);
			m_nextSkill.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(m_record.skillIcon));
			zbUIEffect.SetStars(m_nextSmallStars, m_nextStarRecord.star, moreVis: false);
			((TextElement)m_lbNextAttr).text = zbUtil.Language("lang.10839") + $"<color=#0F6B0F>+{m_nextStarRecord.damageImmunity}</color> ";
		}
		m_nodeNextStar.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_nextStarRecord == null));
		m_arrowStar.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_nextStarRecord == null));
		m_nodeNextSkill.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_nextStarRecord == null));
		m_arrowSkill.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_nextStarRecord == null));
		((VisualElement)m_lbNextAttr).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_nextStarRecord == null));
		m_arrowAttr.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_nextStarRecord == null));
	}

	private void RefreshCostList()
	{
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		if (m_nextStarRecord != null)
		{
			((VisualElement)m_lbMax).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_lbMaxDesc).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_btnStar).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_starCostList = new int[1][] { new int[3] { 2, m_record.propId, m_nextStarRecord.costProp } };
			bool flag = zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Prop, m_record.convertPropId, 1);
			((VisualElement)m_lbConvertItem).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			((VisualElement)m_lbOr).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			if (flag)
			{
				zbItemData data = new zbItemData(EResourceType.Prop, m_record.convertPropId, 1L);
				m_convertItem.Refresh(data, isNameVis: false, isCountVis: false);
				int[][] costLists = new int[1][] { new int[3] { 2, m_record.convertPropId, m_nextStarRecord.costProp } };
				zbSelf.Instance.homelandHandler.UpdateCostUI(costLists, (Label[])(object)new Label[1] { m_lbConvertItem }, new zbItem[1] { m_convertItem });
			}
		}
		else
		{
			((VisualElement)m_lbMax).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_lbMaxDesc).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_btnStar).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_starCostList = new int[0][];
			((VisualElement)m_lbConvertItem).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_lbOr).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		zbSelf.Instance.homelandHandler.UpdateCostUI(m_starCostList, m_costLabels, m_items);
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.wallHandler.HasStarNotify(m_record.id)));
	}

	private void OnCurSkill(ClickEvent evt)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		if (m_curStarRecord != null)
		{
			IEventHandler target = ((EventBase)evt).target;
			Vector2 pos = VisualElementExtensions.LocalToWorld((VisualElement)(object)((target is VisualElement) ? target : null), Vector2.zero);
			pos.y += 180f;
			string skillDesc = zbSelf.Instance.wallHandler.GetSkillDesc(m_wallId, m_curStarRecord.star);
			zbUIMgr.Instance.ShowTipMod(skillDesc, pos);
		}
	}

	private void OnNextSkill(ClickEvent evt)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		if (m_nextStarRecord != null)
		{
			IEventHandler target = ((EventBase)evt).target;
			Vector2 pos = VisualElementExtensions.LocalToWorld((VisualElement)(object)((target is VisualElement) ? target : null), Vector2.zero);
			pos.y += 180f;
			string skillDesc = zbSelf.Instance.wallHandler.GetSkillDesc(m_wallId, m_nextStarRecord.star);
			zbUIMgr.Instance.ShowTipMod(skillDesc, pos);
		}
	}

	private void OnStar(ClickEvent evt)
	{
		if (m_data == null)
		{
			return;
		}
		long resourceCount = zbSelf.Instance.dataHandler.GetResourceCount(EResourceType.Prop, m_record.propId);
		if (m_nextStarRecord != null && resourceCount < m_nextStarRecord.costProp)
		{
			long resourceCount2 = zbSelf.Instance.dataHandler.GetResourceCount(EResourceType.Prop, m_record.convertPropId);
			if (resourceCount2 > 0 && resourceCount2 + resourceCount >= m_nextStarRecord.costProp)
			{
				zbWsMsgHandler.Instance.Wall_UpgradeStar_Req(m_record.id);
			}
			else
			{
				zbUIMgr.Instance.ShowTip(EResourceType.Prop, m_record.propId, m_nextStarRecord.costProp);
			}
		}
		else
		{
			zbWsMsgHandler.Instance.Wall_UpgradeStar_Req(m_record.id);
		}
	}

	public void OnClose(ClickEvent evt)
	{
		Hide();
	}
}
