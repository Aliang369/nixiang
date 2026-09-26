using System;
using System.Collections;
using Proto.Enums;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbTreasureDetailWnd : zbBaseWnd
{
	private Button m_btnTouch;

	private SpineVisualElement m_spine;

	private VisualElement m_spriteIcon;

	private Label m_lbName;

	private Label m_lbLevel;

	private Label m_lbAttr;

	private Label m_lbAttrValue;

	private Label m_lbDesc;

	private Label m_lbLimit;

	private Label m_lbMax;

	private Label m_lbCost;

	private zbItem m_item;

	private Button m_btnUpgrade;

	private Button m_btnClose;

	private VisualElement m_notify;

	private zbTreasureCsvRecord m_record;

	private int m_level;

	private int m_cost;

	private Coroutine m_delayCoroutine;

	protected override void Awake()
	{
		base.Awake();
		m_item = new zbItem();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_spine = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "spine", (string)null);
		m_spriteIcon = UQueryExtensions.Q<VisualElement>(m_RootElement, "icon", (string)null);
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_lbLevel = UQueryExtensions.Q<Label>(m_RootElement, "lb_level", (string)null);
		m_lbAttr = UQueryExtensions.Q<Label>(m_RootElement, "lb_attr", (string)null);
		m_lbAttrValue = UQueryExtensions.Q<Label>(m_RootElement, "lb_attrValue", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_lbLimit = UQueryExtensions.Q<Label>(m_RootElement, "lb_limit", (string)null);
		m_lbMax = UQueryExtensions.Q<Label>(m_RootElement, "lb_max", (string)null);
		m_lbCost = UQueryExtensions.Q<Label>(m_RootElement, "lb_cost", (string)null);
		m_item = UQueryExtensions.Q<zbItem>(m_RootElement, "item", (string)null);
		m_item.OnEnable(this);
		m_btnUpgrade = UQueryExtensions.Q<Button>(m_RootElement, "btn_upgrade", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnUpgrade, (Action<ClickEvent>)OnUpgrade, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnUpgrade);
		AddDark();
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_TreasureInfo, OnEventUpdate);
	}

	public void RefreshCustom(zbTreasureCsvRecord record)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		m_record = record;
		m_spriteIcon.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(record.icon));
		((TextElement)m_lbName).text = record.name;
		((TextElement)m_lbDesc).text = record.desc;
		RefreshDetail();
	}

	private void RefreshDetail()
	{
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_035c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0336: Unknown result type (might be due to invalid IL or missing references)
		m_level = zbSelf.Instance.treasureHandler.GetTreasureLevel(m_record.id);
		zbUtil.SetElementGray(m_spriteIcon, m_level == 0);
		((TextElement)m_lbLevel).text = "Lv." + m_level;
		if (m_level == 0)
		{
			zbUIEffect.StopUpDownAction(m_spriteIcon);
		}
		else
		{
			zbUIEffect.CreateUpDownAction(m_spriteIcon, 10f, 2f);
		}
		if (m_level >= zbSelf.Instance.treasureHandler.GetMaxLevel(m_record.quality))
		{
			((VisualElement)m_lbMax).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_lbLimit).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_btnUpgrade).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_lbCost).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		else
		{
			((VisualElement)m_lbMax).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_lbCost).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			int roleLvl = zbDataConfig.Instance.treasureUpgradeCsvTable.GetRecordByKey(m_level + 1).roleLvl;
			m_cost = zbDataConfig.Instance.treasureUpgradeCsvTable.GetRecordByKey(m_level + 1).costNum;
			long resourceCount = zbSelf.Instance.dataHandler.GetResourceCount(EResourceType.Prop, m_record.propId);
			((TextElement)m_lbCost).text = zbUtil.GetNeedHaveRichString(resourceCount, m_cost);
			m_item.Refresh(new zbItemData(EResourceType.Prop, m_record.propId, m_cost), isNameVis: false, isCountVis: false);
			if (m_cost > resourceCount)
			{
				m_item.SetTouchCallback(delegate
				{
					zbUIMgr.Instance.ShowTip(EResourceType.Prop, m_record.propId, m_cost);
				});
			}
			else
			{
				m_item.SetTouchCallback(delegate
				{
					zbUIMgr.Instance.ShowTip(EResourceType.Prop, m_record.propId, 0L);
				});
			}
			if (zbSelf.Instance.GetLevel() < roleLvl)
			{
				((VisualElement)m_btnUpgrade).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				((VisualElement)m_lbLimit).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				((TextElement)m_lbLimit).text = string.Format(zbUtil.Language("lang.10816"), roleLvl);
			}
			else
			{
				((VisualElement)m_btnUpgrade).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				((VisualElement)m_lbLimit).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				((TextElement)m_btnUpgrade).text = ((m_level > 0) ? zbUtil.Language("lang.10350") : zbUtil.Language("lang.10949"));
				if (m_level == 0)
				{
					((TextElement)m_lbLevel).text = "Lv.1";
					((VisualElement)m_btnUpgrade).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_250_80_green.png"));
				}
				if (!zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Prop, m_record.propId, m_cost))
				{
					((VisualElement)m_btnUpgrade).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btnGray_250x80.png"));
				}
				else
				{
					((VisualElement)m_btnUpgrade).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_250_80_orange.png"));
				}
			}
		}
		bool flag = zbSelf.Instance.treasureHandler.HasTreasureActiveNotify(m_record.id) || zbSelf.Instance.treasureHandler.HasTreasureUpgradeNotify(m_record.id);
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		((TextElement)m_lbAttr).text = zbAttrHelper.GetTeamAttrName((int)m_record.attr[0]);
		zbTeamAttrCsvRecord recordByKey = zbDataConfig.Instance.teamAttrCsvTable.GetRecordByKey((int)m_record.attr[0]);
		((TextElement)m_lbAttrValue).text = zbAttrHelper.FormatAttrValue(m_record.attr[1] + m_record.attr[2] * (float)Mathf.Max(0, m_level - 1), recordByKey.showPercent == 1, isDisplayAdd: true);
	}

	private void OnUpgrade(ClickEvent evt)
	{
		if (!zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Prop, m_record.propId, m_cost))
		{
			zbUIMgr.Instance.ShowTip(EResourceType.Prop, m_record.propId, m_cost);
		}
		else if (m_level > 0)
		{
			zbWsMsgHandler.Instance.Treasure_UpgradeLevel_Req(m_record.id);
		}
		else
		{
			zbWsMsgHandler.Instance.Treasure_Active_Req(m_record.id);
		}
	}

	private void OnClose(ClickEvent evt)
	{
		Hide();
	}

	private void OnEventUpdate(object data)
	{
		if (m_delayCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(m_delayCoroutine);
		}
		m_delayCoroutine = ((MonoBehaviour)this).StartCoroutine(DelayRefreshDetail());
	}

	private IEnumerator DelayRefreshDetail()
	{
		m_spine.AnimationState.SetAnimation(0, "act", false);
		m_spine.AnimationState.AddAnimation(0, "idle", true, 0f);
		yield return (object)new WaitForSeconds(1f / 3f);
		RefreshDetail();
		m_delayCoroutine = null;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (m_delayCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(m_delayCoroutine);
			m_delayCoroutine = null;
		}
	}
}
