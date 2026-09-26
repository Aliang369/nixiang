using System;
using Proto.Enums;
using UnityEngine.UIElements;

public class zbTreasurePoolWnd : zbBaseWnd
{
	private Button m_btnTouchTest;

	private Button m_btnClose;

	private ScrollView m_scrollView;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouchTest = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_scrollView = UQueryExtensions.Q<ScrollView>(m_RootElement, "scrollView", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouchTest, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		AddDark();
		InitRecords();
	}

	private void InitRecords()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Expected O, but got Unknown
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		CreateTreasurePool(EBarType.Treasure);
		VisualElement val = new VisualElement();
		val.style.height = StyleLength.op_Implicit(30f);
		((VisualElement)m_scrollView).Add(val);
		CreateTreasurePool(EBarType.TreasurePlus);
	}

	private void CreateTreasurePool(EBarType barType)
	{
		zbTreasurePoolTitleItem zbTreasurePoolTitleItem2 = new zbTreasurePoolTitleItem(barType);
		((VisualElement)m_scrollView).Add((VisualElement)(object)zbTreasurePoolTitleItem2);
		zbBarProbabilityCsvRecord[] records = zbDataConfig.Instance.barProbabilityCsvTable.GetRecords().records;
		foreach (zbBarProbabilityCsvRecord zbBarProbabilityCsvRecord2 in records)
		{
			if (zbBarProbabilityCsvRecord2.barType == (int)barType)
			{
				zbTreasurePoolItem zbTreasurePoolItem2 = new zbTreasurePoolItem();
				zbTreasurePoolItem2.Refresh(zbBarProbabilityCsvRecord2);
				((VisualElement)m_scrollView).Add((VisualElement)(object)zbTreasurePoolItem2);
			}
		}
	}

	private void OnClose(ClickEvent evt)
	{
		Hide();
	}
}
