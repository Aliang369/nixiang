using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

internal class zbThemeTaskTargetItem : zbViewBaseItem
{
	private zbItem m_item;

	private Label m_lbProval;

	private Label m_lbVal;

	private Button m_btnTouch;

	private Button m_btnLast;

	private OperateTargetDetail m_targetDetail;

	private ThemeTaskDetail m_data;

	private int m_index;

	public zbThemeTaskTargetItem(zbThemeTaskWnd wndParent)
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/operate/tb_themeTaskTarget.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.CloneTree());
		m_item = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, "item", (string)null);
		m_item.OnEnable(wndParent);
		m_lbProval = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_proVal", (string)null);
		m_lbVal = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_val", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_item", (string)null);
		m_btnLast = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_last", (string)null);
		wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnTouch, "", (TrickleDown)0);
		wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnLast, (Action)OnLast, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(OperateTargetDetail targetData, ThemeTaskDetail data, int index)
	{
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		m_targetDetail = targetData;
		m_data = data;
		m_index = index;
		int lastTarget = GetLastTarget();
		if (m_data.TotalExp > lastTarget)
		{
			float num = (float)(m_data.TotalExp - lastTarget) / (float)(m_targetDetail.Target - lastTarget) * 100f;
			num = Mathf.Clamp(num, 0f, 100f);
			((VisualElement)m_lbProval).style.width = new StyleLength(new Length(num, (LengthUnit)1));
		}
		else
		{
			((VisualElement)m_lbProval).style.width = new StyleLength(new Length(0f, (LengthUnit)1));
		}
		((TextElement)m_lbVal).text = m_targetDetail.Target.ToString();
		((VisualElement)m_btnTouch).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_targetDetail.State != ECommonState.Yes));
		AwardDetail awardDetail = m_targetDetail.AwardList[0];
		if (m_data.TotalExp >= m_targetDetail.Target)
		{
			m_item.Refresh(new zbItemData(awardDetail.Type, awardDetail.Tid, awardDetail.Count), isNameVis: false, isCountVis: true, m_targetDetail.State != ECommonState.Yes);
			m_item.SetGettedVisible(m_targetDetail.State == ECommonState.End);
			m_item.SetSpineVisible(m_targetDetail.State == ECommonState.Yes);
			if (m_targetDetail.State == ECommonState.Yes)
			{
				m_item.SetTouchCallback(delegate
				{
					OnTouch();
				});
			}
			else
			{
				m_item.SetTouchCallback(null);
			}
		}
		else
		{
			zbUtil.SetElementGray((VisualElement)(object)m_item, bGray: false);
			m_item.Refresh(new zbItemData(awardDetail.Type, awardDetail.Tid, awardDetail.Count));
			m_item.SetGettedVisible(vis: false);
			m_item.SetSpineVisible(vis: false);
			m_item.SetTouchCallback(null);
		}
	}

	private int GetLastTarget()
	{
		if (m_data != null && m_index > 0)
		{
			foreach (OperateTargetDetail target in m_data.TargetList)
			{
				if (target.Index == m_index)
				{
					return target.Target;
				}
			}
		}
		return 0;
	}

	private void OnTouch()
	{
		if (m_data != null)
		{
			zbWsMsgHandler.Instance.Operate_ThemeTaskTargetAward_Req(m_data.BaseInfo.UUID);
		}
	}

	public void OnLast()
	{
		int num = m_index - 1;
		if (num < 0 || num >= m_data.TargetList.Count)
		{
			return;
		}
		OperateTargetDetail operateTargetDetail = m_data.TargetList[num];
		AwardDetail awardDetail = operateTargetDetail.AwardList[0];
		if (awardDetail != null)
		{
			bool flag = operateTargetDetail.State == ECommonState.End;
			if ((operateTargetDetail.State != ECommonState.Yes) | flag)
			{
				zbUIMgr.Instance.ShowTip(awardDetail.Type, awardDetail.Tid, 0L);
			}
			else
			{
				zbWsMsgHandler.Instance.Operate_ThemeTaskTargetAward_Req(m_data.BaseInfo.UUID);
			}
		}
	}
}
