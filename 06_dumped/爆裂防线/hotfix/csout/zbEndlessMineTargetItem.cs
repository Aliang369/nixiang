using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbEndlessMineTargetItem : zbViewBaseItem
{
	private zbItem[] m_items;

	private Label m_lbTarget;

	private Button m_btnGet;

	private Button m_btnUnReach;

	private VisualElement m_spriteGetted;

	private VisualElement m_notify;

	private OperateTargetDetail m_targetDetail;

	public zbEndlessMineTargetItem(zbEndlessMineTargetWnd wndParent)
	{
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		m_items = new zbItem[5];
		base._002Ector();
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/operate/endlessMine/tb_endlessMineTarget.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.CloneTree());
		for (int i = 0; i < 5; i++)
		{
			m_items[i] = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, "item" + i, (string)null);
			m_items[i].OnEnable(wndParent);
		}
		m_lbTarget = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_target", (string)null);
		m_btnGet = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_get", (string)null);
		m_btnUnReach = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_unReach", (string)null);
		m_spriteGetted = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_getted", (string)null);
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnGet, (Vector2?)new Vector2(-5f, -7f), 1f, true);
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGet, (Action)OnGet, "", (TrickleDown)0);
		wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnUnReach, (Action)OnUnReach, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(OperateTargetDetail targetDetail, int reach)
	{
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		m_targetDetail = targetDetail;
		for (int i = 0; i < 5; i++)
		{
			zbItem zbItem2 = m_items[i];
			AwardDetail awardDetail = ((i < targetDetail.AwardList.Count) ? targetDetail.AwardList[i] : null);
			if (awardDetail != null)
			{
				((VisualElement)zbItem2).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				zbItem2.Refresh(new zbItemData(awardDetail.Type, awardDetail.Tid, awardDetail.Count));
			}
			else
			{
				((VisualElement)zbItem2).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
		}
		((TextElement)m_lbTarget).text = $"{reach}/{targetDetail.Target}";
		((VisualElement)m_btnGet).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(targetDetail.State != ECommonState.Yes));
		((VisualElement)m_btnUnReach).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((targetDetail.State != ECommonState.No) ? 1 : 0));
		m_spriteGetted.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(targetDetail.State != ECommonState.End));
	}

	private void OnGet()
	{
		if (m_targetDetail != null)
		{
			zbWsMsgHandler.Instance.EndlessMine_TargetAward_Req(m_targetDetail.Index);
		}
	}

	private void OnUnReach()
	{
		zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10089"));
	}
}
