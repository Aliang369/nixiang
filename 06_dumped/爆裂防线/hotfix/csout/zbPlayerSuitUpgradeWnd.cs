using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbPlayerSuitUpgradeWnd : zbBaseWnd
{
	private Button m_btnTouchTest;

	private Button m_btnClose;

	private Button m_btnConfirm;

	private Label m_lbMaxDesc;

	private VisualElement[] m_spriteStars1 = (VisualElement[])(object)new VisualElement[5];

	private VisualElement[] m_spriteStars2 = (VisualElement[])(object)new VisualElement[5];

	private Label m_lbName;

	private Label m_lbAttr1;

	private Label m_lbAttr2;

	private Label[] m_costLabels = (Label[])(object)new Label[3];

	private zbItem[] m_items = new zbItem[3];

	private VisualElement m_spriteSkillIcon1;

	private VisualElement m_spriteSkillIcon2;

	private VisualElement m_skillIconTemp1;

	private VisualElement m_skillIconTemp2;

	private Label m_lbLevel1;

	private Label m_lbLevel2;

	private zbItem m_convertItem;

	private Label m_lbConvertItem;

	private Label m_lbOr;

	private int suitId;

	private VisualElement m_cost;

	private zbTipElement m_tip;

	protected override void OnEnable()
	{
		//IL_0347: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ac: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_btnTouchTest = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouchTest, (Action<ClickEvent>)OnTouchTest, "", (TrickleDown)0);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnTouchTest, "", (TrickleDown)0);
		m_btnConfirm = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnConfirm, (Action<ClickEvent>)OnConfirm, "", (TrickleDown)0);
		m_lbMaxDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_maxDesc", (string)null);
		m_lbConvertItem = UQueryExtensions.Q<Label>(m_RootElement, "lb_convertItem", (string)null);
		m_convertItem = UQueryExtensions.Q<zbItem>(m_RootElement, "convertItem", (string)null);
		m_convertItem.OnEnable(this);
		m_lbOr = UQueryExtensions.Q<Label>(m_RootElement, "lb_or", (string)null);
		VisualElement val = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_Star0", (string)null);
		VisualElement val2 = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_Star1", (string)null);
		for (int i = 0; i < 5; i++)
		{
			m_spriteStars1[i] = UQueryExtensions.Q<VisualElement>(val, "iconstar" + i, (string)null);
			m_spriteStars2[i] = UQueryExtensions.Q<VisualElement>(val2, "iconstar" + i, (string)null);
		}
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_lbAttr1 = UQueryExtensions.Q<Label>(m_RootElement, "lb_attr0", (string)null);
		m_lbAttr2 = UQueryExtensions.Q<Label>(m_RootElement, "lb_attr1", (string)null);
		m_skillIconTemp1 = UQueryExtensions.Q<VisualElement>(m_RootElement, "temp0", (string)null);
		m_skillIconTemp2 = UQueryExtensions.Q<VisualElement>(m_RootElement, "temp1", (string)null);
		m_spriteSkillIcon1 = UQueryExtensions.Q<VisualElement>(m_RootElement, "detail_icon0", (string)null);
		m_spriteSkillIcon2 = UQueryExtensions.Q<VisualElement>(m_RootElement, "detail_icon1", (string)null);
		m_lbLevel1 = UQueryExtensions.Q<Label>(m_RootElement, "lb_level0", (string)null);
		m_lbLevel2 = UQueryExtensions.Q<Label>(m_RootElement, "lb_level1", (string)null);
		Button visualElement = UQueryExtensions.Q<Button>(m_RootElement, "btn_Icon0", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)visualElement, (Action<ClickEvent>)delegate(ClickEvent evt)
		{
			OnShowCommsEfficTip(evt, 1);
		}, "", (TrickleDown)0);
		Button visualElement2 = UQueryExtensions.Q<Button>(m_RootElement, "btn_Icon1", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)visualElement2, (Action<ClickEvent>)delegate(ClickEvent evt)
		{
			OnShowCommsEfficTip(evt, 2);
		}, "", (TrickleDown)0);
		m_cost = UQueryExtensions.Q<VisualElement>(m_RootElement, "costList", (string)null);
		for (int num = 0; num < 3; num++)
		{
			m_costLabels[num] = UQueryExtensions.Q<Label>(m_cost, $"lb_cost{num}", (string)null);
			m_items[num] = UQueryExtensions.Q<zbItem>(m_cost, $"item{num}", (string)null);
			m_items[num].OnEnable(this);
			((VisualElement)m_costLabels[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		m_tip = UQueryExtensions.Q<zbTipElement>(m_RootElement, "tip", (string)null);
		m_tip.AutoHideDelay = 3f;
		Sprite val3 = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/common/s9_tipBg.png");
		m_tip.BackgroundTexture = val3.texture;
		m_tip.TipTextColor = Color.white;
		m_gameEevent.RegisterEvent(EGameEvent.Event_Suit_Refresh, OnEventUpdateSuitList);
		AddDark();
	}

	private void OnEventUpdateSuitList(object obj)
	{
		RefreshCustom(suitId);
	}

	public void RefreshCustom(int _suitId)
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_0348: Unknown result type (might be due to invalid IL or missing references)
		//IL_0367: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0426: Unknown result type (might be due to invalid IL or missing references)
		//IL_0443: Unknown result type (might be due to invalid IL or missing references)
		suitId = _suitId;
		PlayerSuitDetail playerSuitDetail = zbSelf.Instance.suitGunHandler.GetPlayerSuitDetail(suitId);
		if (playerSuitDetail == null)
		{
			Hide();
			return;
		}
		int num = 0;
		int num2 = 0;
		zbPlayerSuitCsvRecord recordByKey = zbDataConfig.Instance.playerSuitCsvTable.GetRecordByKey(suitId);
		((TextElement)m_lbMaxDesc).text = zbSelf.Instance.itemHandler.GetStarMaxDesc(recordByKey.propId);
		bool flag = recordByKey.grade == 2;
		Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.skillIcon);
		m_spriteSkillIcon1.style.backgroundImage = new StyleBackground(val);
		m_spriteSkillIcon2.style.backgroundImage = new StyleBackground(val);
		m_skillIconTemp1.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(flag ? "ui/image/suitGun/skill_empty1.png" : "ui/image/suitGun/skill_empty.png"));
		m_skillIconTemp2.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(flag ? "ui/image/suitGun/skill_empty1.png" : "ui/image/suitGun/skill_empty.png"));
		m_spriteSkillIcon1.style.width = StyleLength.op_Implicit(flag ? 132f : 120f);
		m_spriteSkillIcon1.style.height = StyleLength.op_Implicit(flag ? 132f : 120f);
		m_spriteSkillIcon2.style.width = StyleLength.op_Implicit(flag ? 132f : 120f);
		m_spriteSkillIcon2.style.height = StyleLength.op_Implicit(flag ? 132f : 120f);
		m_spriteSkillIcon1.style.scale = StyleScale.op_Implicit(new Vector2(flag ? 1f : 0.85f, flag ? 1f : 0.85f));
		m_spriteSkillIcon2.style.scale = StyleScale.op_Implicit(new Vector2(flag ? 1f : 0.85f, flag ? 1f : 0.85f));
		((TextElement)m_lbName).text = recordByKey.name;
		num = playerSuitDetail.Star;
		num2 = Math.Min(playerSuitDetail.Star + 1, 5);
		((TextElement)m_lbLevel1).text = "Lv." + num;
		((TextElement)m_lbLevel2).text = "Lv." + num2;
		zbPlayerSuitUpgradeCsvRecord suitStarUpgrade = zbSelf.Instance.suitGunHandler.GetSuitStarUpgrade(recordByKey.id, num);
		((TextElement)m_lbAttr1).text = string.Format(zbUtil.Language("lang.10582"), suitStarUpgrade.atk);
		zbPlayerSuitUpgradeCsvRecord suitStarUpgrade2 = zbSelf.Instance.suitGunHandler.GetSuitStarUpgrade(recordByKey.id, num2);
		string arg = zbUtil.AddHighlightStyle(suitStarUpgrade2.atk.ToString(), "007008", 18);
		((TextElement)m_lbAttr2).text = string.Format(zbUtil.Language("lang.10582"), arg);
		zbUIEffect.SetStars(m_spriteStars1, num);
		zbUIEffect.SetStars(m_spriteStars2, num2);
		((TextElement)m_btnConfirm).text = zbUtil.Language("lang.10583");
		((VisualElement)m_btnConfirm).SetEnabled(false);
		m_cost.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		if (playerSuitDetail.Star < 5)
		{
			m_cost.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			int[][] costLists = new int[1][] { new int[3] { 2, recordByKey.propId, suitStarUpgrade2.costProp } };
			zbSelf.Instance.homelandHandler.UpdateCostUI(costLists, m_costLabels, m_items);
			((VisualElement)m_btnConfirm).SetEnabled(true);
			((TextElement)m_btnConfirm).text = zbUtil.Language("lang.10584");
		}
		((VisualElement)m_lbMaxDesc).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(playerSuitDetail.Star < 5));
		bool flag2 = zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Prop, recordByKey.convertPropId, 1);
		((VisualElement)m_lbConvertItem).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag2));
		((VisualElement)m_lbOr).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag2));
		if (flag2)
		{
			zbItemData data = new zbItemData(EResourceType.Prop, recordByKey.convertPropId, 1L);
			m_convertItem.Refresh(data, isNameVis: false, isCountVis: false);
			int[][] costLists2 = new int[1][] { new int[3] { 2, recordByKey.convertPropId, suitStarUpgrade2.costProp } };
			zbSelf.Instance.homelandHandler.UpdateCostUI(costLists2, (Label[])(object)new Label[1] { m_lbConvertItem }, new zbItem[1] { m_convertItem });
		}
		if (m_tip != null)
		{
			m_tip.Hide();
		}
	}

	public void OnConfirm(ClickEvent evt)
	{
		PlayerSuitDetail playerSuitDetail = zbSelf.Instance.suitGunHandler.GetPlayerSuitDetail(suitId);
		if (playerSuitDetail.Star >= 5)
		{
			return;
		}
		zbPlayerSuitCsvRecord recordByKey = zbDataConfig.Instance.playerSuitCsvTable.GetRecordByKey(suitId);
		long resourceCount = zbSelf.Instance.dataHandler.GetResourceCount(EResourceType.Prop, recordByKey.propId);
		zbPlayerSuitUpgradeCsvRecord suitStarUpgrade = zbSelf.Instance.suitGunHandler.GetSuitStarUpgrade(recordByKey.id, playerSuitDetail.Star + 1);
		if (resourceCount < suitStarUpgrade.costProp)
		{
			long resourceCount2 = zbSelf.Instance.dataHandler.GetResourceCount(EResourceType.Prop, recordByKey.convertPropId);
			if (resourceCount2 > 0 && resourceCount2 + resourceCount >= suitStarUpgrade.costProp)
			{
				zbWsMsgHandler.Instance.Suit_UpgradeStar_Req(suitId);
			}
			else
			{
				zbUIMgr.Instance.ShowTip(EResourceType.Prop, recordByKey.propId, suitStarUpgrade.costProp);
			}
		}
		else
		{
			zbWsMsgHandler.Instance.Suit_UpgradeStar_Req(suitId);
		}
	}

	private void OnShowCommsEfficTip(ClickEvent evt, int bindType)
	{
		PlayerSuitDetail playerSuitDetail = zbSelf.Instance.suitGunHandler.GetPlayerSuitDetail(suitId);
		int star = playerSuitDetail.Star;
		if (bindType != 1)
		{
			star = Math.Min(playerSuitDetail.Star + 1, 5);
		}
		ShowTip(evt, zbSelf.Instance.suitGunHandler.GetSkillDesc(playerSuitDetail.SuitId, star));
	}

	private void ShowTip(ClickEvent evt, string tipText)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		IEventHandler target = ((EventBase)evt).target;
		VisualElement val = (VisualElement)(object)((target is VisualElement) ? target : null);
		if (val != null)
		{
			Vector2 val2 = Vector2.op_Implicit(((PointerEventBase<ClickEvent>)(object)evt).localPosition);
			Vector2 val3 = VisualElementExtensions.LocalToWorld(val, val2);
			VisualElement val4 = UQueryExtensions.Q<VisualElement>(GetRootElement(), "root", (string)null);
			Vector2 val5 = VisualElementExtensions.WorldToLocal(val4, val3);
			Vector2 val6 = default(Vector2);
			((Vector2)(ref val6))._002Ector(val4.resolvedStyle.width * 0.01f, val4.resolvedStyle.height * 0.02f);
			val5 += val6;
			m_tip.ShowAt(val5, tipText, val4);
		}
	}

	public void OnTouchTest(ClickEvent evt)
	{
		Hide();
	}
}
