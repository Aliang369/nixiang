using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine.UIElements;

public class zbRebateAwardWnd : zbBaseWnd
{
	private Button m_btnClose;

	private Button m_btnRule;

	private Label m_lblTitle;

	private Button m_btnGet;

	private zbItem[] m_getAwards = new zbItem[5];

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnRule = UQueryExtensions.Q<Button>(m_RootElement, "btnrule", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btnclose", (string)null);
		m_btnGet = UQueryExtensions.Q<Button>(m_RootElement, "btnsure", (string)null);
		m_lblTitle = UQueryExtensions.Q<Label>(m_RootElement, "lblget", (string)null);
		for (int i = 0; i < 5; i++)
		{
			m_getAwards[i] = UQueryExtensions.Q<zbItem>(m_RootElement, $"item{i}", (string)null);
			m_getAwards[i].OnEnable(this);
		}
		AddDark();
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)Hide, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGet, (Action)OnGet, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRule, (Action)OnRule, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_RebateAward_Ack, OnEventUpdate);
	}

	private void OnEventUpdate(object data)
	{
		Refresh();
	}

	public override void Refresh()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		base.Refresh();
		zbItem[] getAwards = m_getAwards;
		for (int i = 0; i < getAwards.Length; i++)
		{
			((VisualElement)getAwards[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		if (zbServerData.sGameData.AuxAwardInfo == null)
		{
			return;
		}
		RechargeRebate rechargeRebate = zbServerData.sGameData.AuxAwardInfo.AccountAwardInfo.RechargeRebate;
		if (rechargeRebate == null)
		{
			return;
		}
		if (rechargeRebate.State == ERebateState.SuccessRebate)
		{
			Hide();
			return;
		}
		((TextElement)m_lblTitle).text = $"您在测试期间累计充值:{rechargeRebate.Amount}元";
		int num = 0;
		foreach (AwardDetail award in rechargeRebate.AwardList)
		{
			if (num >= m_getAwards.Length)
			{
				break;
			}
			zbItem zbItem2 = m_getAwards[num];
			if (zbItem2 != null)
			{
				((VisualElement)zbItem2).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				zbItem2.Refresh(new zbItemData(award.Type, award.Tid, award.Count));
			}
			num++;
		}
	}

	private void OnGet()
	{
		zbWsMsgHandler.Instance.Account_RebateAward_Req();
	}

	private void OnRule()
	{
		string[] langKeys = new string[1] { "lang.11497" };
		zbUIMgr.Instance.ShowRule(langKeys);
	}
}
