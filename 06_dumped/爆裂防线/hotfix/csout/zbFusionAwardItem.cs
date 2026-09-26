using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine.UIElements;

public class zbFusionAwardItem : zbViewBaseItem
{
	private zbFusionAward m_wndParent;

	private zbItem[] m_items = new zbItem[4];

	private Label m_lbScore;

	private Button m_btnTouch;

	private OperateTargetDetail m_targetDetail;

	public zbFusionAwardItem(zbFusionAward wndParent)
	{
		m_wndParent = wndParent;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/operate/tb_fusionaward.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.CloneTree());
		for (int i = 0; i < 4; i++)
		{
			m_items[i] = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, "item" + i, (string)null);
			m_items[i].OnEnable(m_wndParent);
		}
		m_lbScore = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_score", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_award", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnTouch, "", (TrickleDown)0);
	}

	public void Refresh(OperateTargetDetail targetDetail)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		m_targetDetail = targetDetail;
		for (int i = 0; i < 4; i++)
		{
			((VisualElement)m_items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			if (i < targetDetail.AwardList.Count && targetDetail.AwardList[i] != null)
			{
				((VisualElement)m_items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_items[i].Refresh(new zbItemData(targetDetail.AwardList[i].Type, targetDetail.AwardList[i].Tid, targetDetail.AwardList[i].Count));
				m_items[i].SetSpineVisible(targetDetail.State == ECommonState.Yes);
				m_items[i].SetGettedVisible(targetDetail.State == ECommonState.End);
			}
		}
		((TextElement)m_lbScore).text = zbUtil.ConvertToTBMK(targetDetail.Target);
		((VisualElement)m_btnTouch).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(targetDetail.State != ECommonState.Yes));
	}

	public void OnTouch()
	{
		if (m_targetDetail != null)
		{
			zbWsMsgHandler.Instance.Operate_FusionTargetAward_Req(m_targetDetail.Target);
		}
	}
}
