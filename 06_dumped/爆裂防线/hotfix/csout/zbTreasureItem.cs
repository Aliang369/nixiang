using System;
using Proto.Enums;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbTreasureItem : zbViewBaseItem
{
	private zbTreasureWnd m_parentWnd;

	private SpineVisualElement m_spineSlot;

	private VisualElement m_spriteIcon;

	private VisualElement m_progressBg;

	private VisualElement m_progress;

	private Label m_lbProgress;

	private Label m_lbName;

	private Label m_lbLevel;

	private Button m_btnTouch;

	private VisualElement m_notify;

	private zbTreasureCsvRecord m_record;

	private int m_level;

	public zbTreasureItem(zbTreasureWnd parentWnd)
	{
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		base._002Ector();
		m_parentWnd = parentWnd;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/treasure/tb_treasure.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_spineSlot = UQueryExtensions.Q<SpineVisualElement>((VisualElement)(object)this, "spine_slot", (string)null);
		m_spriteIcon = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_icon", (string)null);
		m_progressBg = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "progressBg", (string)null);
		m_progress = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "progress", (string)null);
		m_lbProgress = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_progress", (string)null);
		m_lbName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_name", (string)null);
		m_lbLevel = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_level", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		m_parentWnd.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnTouch, "", (TrickleDown)0);
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnTouch, (Vector2?)new Vector2(15f, 15f), 1f, true);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Init(zbTreasureCsvRecord record)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		m_record = record;
		m_spriteIcon.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(record.icon));
		((TextElement)m_lbName).text = record.name;
	}

	public void Refresh()
	{
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		m_level = zbSelf.Instance.treasureHandler.GetTreasureLevel(m_record.id);
		zbUtil.SetElementGray(m_spriteIcon, m_level == 0);
		zbUtil.SetElementGray((VisualElement)(object)m_lbProgress, m_level == 0);
		zbUtil.SetElementGray((VisualElement)(object)m_lbName, m_level == 0);
		string text = null;
		if (m_record.quality == 6)
		{
			text = "red";
		}
		else if (m_record.quality == 5)
		{
			text = "gold";
		}
		else if (m_record.quality == 4)
		{
			text = "purple";
		}
		if (text != null)
		{
			m_spineSlot.AnimationState.SetAnimation(0, (m_level == 0) ? (text + "_gray") : text, true);
		}
		if (m_level > 0)
		{
			zbUIEffect.CreateUpDownAction(m_spriteIcon, 10f, 2f);
			((TextElement)m_lbLevel).text = "Lv." + m_level;
		}
		else
		{
			((TextElement)m_lbLevel).text = "";
		}
		if (m_level >= zbSelf.Instance.treasureHandler.GetMaxLevel(m_record.quality))
		{
			((TextElement)m_lbProgress).text = "MAX";
			m_progress.style.width = StyleLength.op_Implicit(new Length(124f, (LengthUnit)0));
		}
		else
		{
			long resourceCount = zbSelf.Instance.dataHandler.GetResourceCount(EResourceType.Prop, m_record.propId);
			int costNum = zbDataConfig.Instance.treasureUpgradeCsvTable.GetRecordByKey(m_level + 1).costNum;
			float num = Mathf.Clamp((float)resourceCount / (float)costNum, 0f, 1f);
			m_progress.style.width = StyleLength.op_Implicit(new Length(num * 124f, (LengthUnit)0));
			((TextElement)m_lbProgress).text = resourceCount + "/" + costNum;
		}
		bool flag = zbSelf.Instance.treasureHandler.HasTreasureActiveNotify(m_record.id) || zbSelf.Instance.treasureHandler.HasTreasureUpgradeNotify(m_record.id);
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		string resName = (flag ? "ui/image/progress/progress124x28_1.png" : "ui/image/progress/progress124x28_2.png");
		m_progressBg.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resName));
	}

	public void OnTouch(ClickEvent evt)
	{
		if (m_level == 0 && !zbSelf.Instance.treasureHandler.HasTreasureActiveNotify(m_record.id))
		{
			int costNum = zbDataConfig.Instance.treasureUpgradeCsvTable.GetRecordByKey(1).costNum;
			zbUIMgr.Instance.ShowTip(EResourceType.Prop, m_record.propId, costNum);
		}
		else
		{
			zbUIMgr.Instance.ShowTreasureDetail(m_record);
		}
	}
}
