using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbFootholdStarWnd : zbBaseWnd
{
	private Button m_btnTouchTest;

	private Button m_btnClose;

	private Label m_lbName;

	private Image m_descImg;

	private VisualElement m_nodeSkill;

	private VisualElement m_spriteSkill;

	private Button m_btnList;

	private Label m_lbDesc;

	private VisualElement m_nodeCurStar;

	private VisualElement[] m_curStars = (VisualElement[])(object)new VisualElement[5];

	private VisualElement m_nodeNextStar;

	private VisualElement[] m_nextStars = (VisualElement[])(object)new VisualElement[5];

	private VisualElement m_arrowStar;

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

	private int m_footholdId;

	private zbFootholdCsvRecord m_record;

	private FootholdDetail m_data;

	private zbFootholdStarUpgradeCsvRecord m_curStarRecord;

	private zbFootholdStarUpgradeCsvRecord m_nextStarRecord;

	private int[][] m_starCostList;

	protected override void OnEnable()
	{
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0317: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f6: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_btnTouchTest = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouchTest, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		VisualElement val = UQueryExtensions.Q<VisualElement>(m_RootElement, "node", (string)null);
		if (m_descImg == null)
		{
			m_descImg = new Image();
			((VisualElement)m_descImg).style.left = StyleLength.op_Implicit(0f);
			((VisualElement)m_descImg).style.top = StyleLength.op_Implicit(0f);
			((VisualElement)m_descImg).style.width = StyleLength.op_Implicit(630f);
			((VisualElement)m_descImg).style.height = StyleLength.op_Implicit(420f);
			((VisualElement)m_descImg).style.position = StyleEnum<Position>.op_Implicit((Position)1);
			val.Add((VisualElement)(object)m_descImg);
		}
		m_nodeSkill = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_skill", (string)null);
		m_spriteSkill = UQueryExtensions.Q<VisualElement>(m_nodeSkill, "bg_skill", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_nodeSkill, "lb_desc", (string)null);
		m_btnList = UQueryExtensions.Q<Button>(m_nodeSkill, "btn_list", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnList, (Action)OnList, "", (TrickleDown)0);
		VisualElement val2 = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_star", (string)null);
		m_nodeCurStar = UQueryExtensions.Q<VisualElement>(val2, "curStar", (string)null);
		m_nodeNextStar = UQueryExtensions.Q<VisualElement>(val2, "nextStar", (string)null);
		for (int i = 0; i < 5; i++)
		{
			m_curStars[i] = UQueryExtensions.Q<VisualElement>(m_nodeCurStar, "star" + i, (string)null);
			m_nextStars[i] = UQueryExtensions.Q<VisualElement>(m_nodeNextStar, "star" + i, (string)null);
		}
		m_arrowStar = UQueryExtensions.Q<VisualElement>(val2, "arrow", (string)null);
		VisualElement val3 = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_attr", (string)null);
		m_lbCurAttr = UQueryExtensions.Q<Label>(val3, "lb_curAttr", (string)null);
		m_lbNextAttr = UQueryExtensions.Q<Label>(val3, "lb_nextAttr", (string)null);
		m_arrowAttr = UQueryExtensions.Q<VisualElement>(val3, "arrow", (string)null);
		for (int j = 0; j < 2; j++)
		{
			m_costLabels[j] = UQueryExtensions.Q<Label>(m_RootElement, "lb_cost" + j, (string)null);
			m_items[j] = UQueryExtensions.Q<zbItem>(m_RootElement, "item" + j, (string)null);
			m_items[j].OnEnable(this);
			((VisualElement)m_costLabels[j]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
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
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_FootholdInfo, OnEventUpdate);
		AddDark();
	}

	private void OnEventUpdate(object obj)
	{
		RefreshCustom(m_footholdId);
	}

	public void RefreshCustom(int footholdId)
	{
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		if (zbUIMgr.Instance.IsShow(EWndId.Foothold))
		{
			if ((Object)(object)zbHomeMgr.Instance.renderTarget == (Object)null)
			{
				zbHomeMgr.Instance.renderTarget = ((Component)GameObject.Find("Renders").transform.Find("rendertarget")).GetComponent<zbRenderTarget>();
			}
			zbFootholdWnd zbFootholdWnd2 = zbUIMgr.Instance.GetWndById(EWndId.Foothold) as zbFootholdWnd;
			zbHomeMgr.Instance.renderTarget.StartRender(((Component)zbFootholdWnd2.nodeTarget).gameObject, 630, 420);
		}
		m_footholdId = footholdId;
		m_record = zbDataConfig.Instance.footholdCsvTable.GetRecordByKey(m_footholdId);
		m_data = zbSelf.Instance.guardHandler.GetFootholdDetail(m_footholdId);
		if (m_data != null)
		{
			((TextElement)m_lbMaxDesc).text = zbSelf.Instance.itemHandler.GetStarMaxDesc(m_record.propId);
			m_curStarRecord = zbSelf.Instance.guardHandler.GetFootholdStarUpgradeRecord(m_footholdId, m_data.Star);
			m_nextStarRecord = zbSelf.Instance.guardHandler.GetFootholdStarUpgradeRecord(m_footholdId, m_data.Star + 1);
			m_spriteSkill.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(m_record.skillIcon));
			RefreshAttrs();
			RefreshCostList();
		}
	}

	public void RefreshAttrs()
	{
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)m_lbName).text = m_record.name;
		if (m_curStarRecord != null)
		{
			zbUIEffect.SetStars(m_curStars, m_curStarRecord.star);
			((TextElement)m_lbCurAttr).text = zbUtil.Language("lang.10862") + "+" + $"{m_curStarRecord.dmg:0.##%}";
		}
		if (m_nextStarRecord != null)
		{
			zbUIEffect.SetStars(m_nextStars, m_nextStarRecord.star);
			((TextElement)m_lbNextAttr).text = zbUtil.Language("lang.10862") + "<color=#0F6B0F>+" + $"{m_nextStarRecord.dmg:0.##%}" + "</color>";
			zbLabelLinkUtil.BuildModLink(zbSelf.Instance.guardHandler.GetFootholdEffectDesc(m_record, m_nextStarRecord.star), m_lbDesc);
			zbUtil.AutoFontSize((TextElement)(object)m_lbDesc, 22f, 14f, 555f, 92f);
		}
		m_nodeNextStar.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_nextStarRecord == null));
		m_arrowStar.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_nextStarRecord == null));
		((VisualElement)m_lbNextAttr).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_nextStarRecord == null));
		m_arrowAttr.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_nextStarRecord == null));
	}

	private void RefreshCostList()
	{
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		if (m_nextStarRecord != null)
		{
			((TextElement)m_lbMax).text = "";
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
			zbFootholdEffectCsvRecord footholdEffectRecord = zbSelf.Instance.guardHandler.GetFootholdEffectRecord(m_record.id, m_curStarRecord.star);
			zbFootholdEffectCsvRecord footholdEffectRecord2 = zbSelf.Instance.guardHandler.GetFootholdEffectRecord(m_record.id, m_nextStarRecord.star);
			m_nodeSkill.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((footholdEffectRecord.id == footholdEffectRecord2.id) ? 1 : 0));
		}
		else
		{
			m_nodeSkill.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((TextElement)m_lbMax).text = zbUtil.Language("lang.10583");
			((VisualElement)m_btnStar).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_starCostList = new int[0][];
			((VisualElement)m_lbConvertItem).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_lbOr).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		zbSelf.Instance.homelandHandler.UpdateCostUI(m_starCostList, m_costLabels, m_items);
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.guardHandler.HasFootholdNotify(m_record.id)));
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
				zbWsMsgHandler.Instance.Foothold_UpgradeStar_Req(m_record.id);
			}
			else
			{
				zbUIMgr.Instance.ShowTip(EResourceType.Prop, m_record.propId, m_nextStarRecord.costProp);
			}
		}
		else
		{
			zbWsMsgHandler.Instance.Foothold_UpgradeStar_Req(m_record.id);
		}
	}

	public void OnClose(ClickEvent evt)
	{
		zbHomeMgr.Instance.renderTarget.StopRender();
		Hide();
	}

	private void OnList()
	{
		zbUIMgr.Instance.ShowPlayerSuitUpgradePreview(m_record.id, EPreviewType.Foothold);
	}

	protected override void UpdateAtShow()
	{
		base.UpdateAtShow();
		if (m_descImg != null && !((Object)(object)zbHomeMgr.Instance.renderTarget == (Object)null))
		{
			m_descImg.image = (Texture)(object)zbHomeMgr.Instance.renderTarget.GetRenderTexture();
		}
	}
}
