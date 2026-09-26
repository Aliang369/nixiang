using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine.UIElements;

public class zbTilibankWnd : zbBaseWnd
{
	private ScrollView m_scView;

	private Label m_lblTime;

	private Label m_lblTip;

	private Button m_btnGet;

	private Button m_btnClose;

	protected override void OnEnable()
	{
		base.OnEnable();
		VisualElement val = UQueryExtensions.Q<VisualElement>(m_RootElement, "rootbg", (string)null);
		m_lblTime = UQueryExtensions.Q<Label>(val, "lbltime", (string)null);
		VisualElement val2 = UQueryExtensions.Q<VisualElement>(val, "content", (string)null);
		m_lblTip = UQueryExtensions.Q<Label>(val2, "lbltip", (string)null);
		m_scView = UQueryExtensions.Q<ScrollView>(val2, "listtili", (string)null);
		m_btnGet = UQueryExtensions.Q<Button>(val2, "btnget", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGet, (Action)OnGet, "", (TrickleDown)0);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btnclose", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)Hide, "", (TrickleDown)0);
		CreateMoneyPanel(new EMoneyType[1] { EMoneyType.Tili });
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Activity, OnUpdateActivity);
	}

	private void OnUpdateActivity(object data)
	{
		Refresh();
	}

	public override void Refresh()
	{
		base.Refresh();
		TiliBank tiliBank = null;
		if (zbServerData.sGameData.ActivityInfo != null)
		{
			tiliBank = zbServerData.sGameData.ActivityInfo.TiliBank;
		}
		if (tiliBank == null)
		{
			return;
		}
		int num = 0;
		foreach (TiliBankDetail tili in tiliBank.TiliList)
		{
			if (tili.TiliTm < zbSelf.Instance.timeHandler.GetServerTime())
			{
				num++;
			}
		}
		((TextElement)m_lblTip).text = string.Format(zbUtil.Language("lang.10235"), num, tiliBank.MaxCount);
		RefreshList(tiliBank);
	}

	private void RefreshList(TiliBank tiliBank)
	{
		((VisualElement)m_scView).Clear();
		((VisualElement)m_scView).visible = false;
		if (tiliBank.TiliList.Count == 0)
		{
			return;
		}
		List<TiliBankDetail> list = ((IEnumerable<TiliBankDetail>)tiliBank.TiliList).ToList();
		((VisualElement)m_scView).visible = true;
		long serverTime = zbSelf.Instance.timeHandler.GetServerTime();
		bool flag = false;
		foreach (TiliBankDetail item in list)
		{
			if (item.TiliTm <= serverTime)
			{
				zbTilibankItem zbTilibankItem2 = new zbTilibankItem(this);
				zbTilibankItem2.Init(item);
				((VisualElement)m_scView).Add((VisualElement)(object)zbTilibankItem2);
			}
			else if (!flag)
			{
				zbTilibankItem zbTilibankItem3 = new zbTilibankItem(this);
				zbTilibankItem3.Init(item);
				((VisualElement)m_scView).Add((VisualElement)(object)zbTilibankItem3);
				flag = true;
			}
		}
	}

	private void OnGet()
	{
		zbWsMsgHandler.Instance.Activity_TiliBankAwardAll_Req();
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		foreach (VisualElement item in ((VisualElement)m_scView).Children())
		{
			if (item is zbTilibankItem zbTilibankItem2)
			{
				zbTilibankItem2.UpdatePerSecond();
			}
		}
	}
}
