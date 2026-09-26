using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

internal class zbThemeTaskItem : zbViewBaseItem
{
	private VisualElement m_bg;

	private zbItem m_item;

	private Label m_lbDesc;

	private Label m_lbProval;

	private Label m_lbVal;

	private Label m_lbScore;

	private Button m_btnTouch;

	private TextElement m_lbTouch;

	private VisualElement m_spriteGetted;

	private VisualElement m_notify;

	private Sprite m_spriteBtnGoto;

	private Sprite m_spriteBtnGet;

	private TaskDetail m_taskDetail;

	private string m_uuid;

	private zbTaskTypeCsvRecord m_taskTypeRecord;

	public zbThemeTaskItem(VisualTreeAsset tAssets, zbThemeTaskWnd wndParent)
	{
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		base._002Ector();
		m_spriteBtnGoto = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_180_80_green.png");
		m_spriteBtnGet = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_180_80_yellow.png");
		((VisualElement)this).Add((VisualElement)(object)tAssets.Instantiate());
		m_bg = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg", (string)null);
		m_item = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, "item", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_desc", (string)null);
		m_lbProval = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_proVal", (string)null);
		m_lbVal = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_val", (string)null);
		m_lbScore = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_score", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		m_lbTouch = UQueryExtensions.Q<TextElement>((VisualElement)(object)m_btnTouch, (string)null, (string)null);
		m_spriteGetted = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_get", (string)null);
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnTouch, (Vector2?)new Vector2(2f, 2f), 1f, true);
		m_item.OnEnable(wndParent);
		wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnTouch, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(TaskDetail data, string uuid)
	{
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		m_taskDetail = data;
		m_uuid = uuid;
		zbThemeTaskTaskCsvRecord recordByKey = zbDataConfig.Instance.themeTaskTaskCsvTable.GetRecordByKey(data.TaskId);
		zbItemData data2 = new zbItemData((EResourceType)recordByKey.award[0], recordByKey.award[1], recordByKey.award[2]);
		m_item.Refresh(data2);
		((TextElement)m_lbScore).text = "+" + recordByKey.exp;
		m_taskTypeRecord = zbDataConfig.Instance.taskTypeCsvTable.GetRecordByKey(m_taskDetail.TaskType);
		((TextElement)m_lbDesc).text = string.Format(m_taskTypeRecord.desc, m_taskDetail.Target);
		((VisualElement)m_lbProval).style.width = new StyleLength(new Length((float)m_taskDetail.Reach / (float)m_taskDetail.Target * 100f, (LengthUnit)1));
		((TextElement)m_lbVal).text = $"{m_taskDetail.Reach} / {m_taskDetail.Target}";
		if (m_taskDetail.State == EActiveState.Active)
		{
			m_lbTouch.text = zbUtil.Language("lang.10090");
			((VisualElement)m_btnTouch).style.backgroundImage = new StyleBackground(m_spriteBtnGet);
		}
		else
		{
			m_lbTouch.text = zbUtil.Language("lang.10155");
			((VisualElement)m_btnTouch).style.backgroundImage = new StyleBackground(m_spriteBtnGoto);
		}
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_taskDetail.State != EActiveState.Active));
		m_spriteGetted.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_taskDetail.State != EActiveState.Done));
		((VisualElement)m_lbTouch).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_taskDetail.State == EActiveState.Done));
	}

	private void OnTouch()
	{
		if (m_taskDetail != null)
		{
			if (m_taskDetail.State == EActiveState.Active)
			{
				zbWsMsgHandler.Instance.Operate_ThemeTaskTaskAward_Req(m_uuid, m_taskDetail.TaskId);
			}
			else
			{
				zbLinkHelper.OpenLink(m_taskTypeRecord.link, m_taskDetail.Param);
			}
		}
	}
}
