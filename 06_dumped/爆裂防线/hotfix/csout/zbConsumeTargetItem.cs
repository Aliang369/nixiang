using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbConsumeTargetItem : zbViewBaseItem
{
	public class UxmlFactory : UxmlFactory<zbConsumeTargetItem>
	{
	}

	private Label m_lbDesc;

	private Label m_lbNeed;

	private zbItem[] m_items = new zbItem[5];

	private VisualElement m_spriteProgress;

	private Button m_btnTouch;

	private VisualElement m_notify;

	private OperateConsumTargetData m_data;

	public zbConsumeTargetItem()
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/shop/tb_consumeTarget.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
	}

	public void OnEnable(zbBaseWnd wndParent)
	{
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 5; i++)
		{
			m_items[i] = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, "item" + i, (string)null);
			m_items[i].OnEnable(wndParent);
		}
		m_lbDesc = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_desc", (string)null);
		m_lbNeed = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_need", (string)null);
		m_spriteProgress = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "progress", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnTouch, "", (TrickleDown)0);
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnTouch, (Vector2?)new Vector2(5f, 5f), 1f, true);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(bool needShow = true)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
		m_data = zbSelf.Instance.shopHandler.GetConsumeTargetDetail();
		if (!needShow || m_data == null)
		{
			((VisualElement)this).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			return;
		}
		((VisualElement)this).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		int count = m_data.Count;
		int target = m_data.TargetDetail.Target;
		float num = Mathf.Min((float)count / (float)target, 1f);
		m_spriteProgress.style.width = StyleLength.op_Implicit(new Length(num * 200f, (LengthUnit)0));
		if (m_data.OperateType == EOperateType.ConsumeDailySum)
		{
			((TextElement)m_lbDesc).text = string.Format(string.Format("{0}x{1}", zbUtil.Language("lang.10182"), target), zbDataConfig.Instance.moneyCsvTable.GetRecordByKey(2).name);
		}
		else if (m_data.OperateType == EOperateType.ConsumeSum)
		{
			((TextElement)m_lbDesc).text = string.Format(string.Format("{0}x{1}", zbUtil.Language("lang.10500"), target), zbDataConfig.Instance.moneyCsvTable.GetRecordByKey(2).name);
		}
		((VisualElement)m_lbDesc).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/icon/money/money_2.png"));
		((TextElement)m_lbNeed).text = $"{count}/{target}";
		for (int i = 0; i < 5; i++)
		{
			((VisualElement)m_items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		int num2 = 0;
		foreach (AwardDetail award in m_data.TargetDetail.AwardList)
		{
			if (num2 < m_items.Length)
			{
				zbItemData data = new zbItemData(award.Type, award.Tid, award.Count);
				((VisualElement)m_items[num2]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_items[num2].Refresh(data);
			}
			num2++;
		}
		if (m_data.TargetDetail.State == ECommonState.Yes)
		{
			((TextElement)m_btnTouch).text = zbUtil.Language("lang.10090");
			((VisualElement)m_btnTouch).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_180_80_green.png"));
		}
		else if (m_data.TargetDetail.State == ECommonState.No)
		{
			((TextElement)m_btnTouch).text = zbUtil.Language("lang.10155");
			((VisualElement)m_btnTouch).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_180_80_yellow.png"));
		}
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_data.TargetDetail.State != ECommonState.Yes));
	}

	public void OnTouch()
	{
		if (m_data == null)
		{
			return;
		}
		if (m_data.TargetDetail.State == ECommonState.Yes)
		{
			if (m_data.UUID == "")
			{
				zbWsMsgHandler.Instance.Journey_ChapterTwoDiamondCost_Req(m_data.TargetDetail.Index);
			}
			else if (m_data.OperateType == EOperateType.ConsumeDailySum)
			{
				zbWsMsgHandler.Instance.Operate_ConsumeDailySum_Req(m_data.UUID, m_data.TargetDetail.Index);
			}
			else if (m_data.OperateType == EOperateType.ConsumeSum)
			{
				zbWsMsgHandler.Instance.Operate_ConsumeSum_Req(m_data.UUID, m_data.TargetDetail.Index);
			}
		}
		else if (m_data.UUID == "")
		{
			zbUIMgr.Instance.ShowJourneyTwoDiamondCost();
		}
		else
		{
			zbOperateCsvRecord recordByKey = zbDataConfig.Instance.operateCsvTable.GetRecordByKey((int)m_data.OperateType);
			if (m_data.OperateType == EOperateType.ConsumeDailySum)
			{
				zbUIMgr.Instance.ShowConsumeDailySum(m_data.UUID, recordByKey);
			}
			else if (m_data.OperateType == EOperateType.ConsumeSum)
			{
				zbUIMgr.Instance.ShowConsumeSum(m_data.UUID, recordByKey);
			}
		}
	}
}
