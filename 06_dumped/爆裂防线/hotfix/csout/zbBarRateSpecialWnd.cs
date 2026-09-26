using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbBarRateSpecialWnd : zbBaseWnd
{
	private Button m_btnTouchTest;

	private Button m_btnClose;

	private Button m_btnRule;

	private Label m_lbTitle;

	private ScrollView m_listView;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouchTest = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_btnRule = UQueryExtensions.Q<Button>(m_RootElement, "btn_rule", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_listView = UQueryExtensions.Q<ScrollView>(m_RootElement, "listView", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouchTest, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRule, (Action<ClickEvent>)OnRule, "", (TrickleDown)0);
		AddDark();
	}

	public override void Refresh()
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Expected O, but got Unknown
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_listView).Clear();
		zbBarProbabilityCsvRecord[] records = zbDataConfig.Instance.barProbabilityCsvTable.GetRecords().records;
		foreach (zbBarProbabilityCsvRecord zbBarProbabilityCsvRecord2 in records)
		{
			if (zbBarProbabilityCsvRecord2.barType == 2)
			{
				zbBarRateSpecialItemTitle zbBarRateSpecialItemTitle2 = new zbBarRateSpecialItemTitle(zbBarProbabilityCsvRecord2);
				((VisualElement)m_listView).Add((VisualElement)(object)zbBarRateSpecialItemTitle2);
			}
		}
		Label val = new Label(zbUtil.Language("lang.10331"));
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)val);
		((VisualElement)val).style.color = StyleColor.op_Implicit(Color.white);
		((VisualElement)val).style.fontSize = StyleLength.op_Implicit(24f);
		((VisualElement)val).style.left = StyleLength.op_Implicit(20f);
		((VisualElement)val).style.marginBottom = StyleLength.op_Implicit(6f);
		((VisualElement)val).style.marginTop = StyleLength.op_Implicit(10f);
		((VisualElement)m_listView).Add((VisualElement)(object)val);
		RefreshPool();
	}

	private void RefreshPool()
	{
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Expected O, but got Unknown
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Expected O, but got Unknown
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Expected O, but got Unknown
		List<zbRateData> list = new List<zbRateData>();
		zbBarPoolSpecialCsvTableData records = zbDataConfig.Instance.barPoolSpecialCsvTable.GetRecords();
		long num = 0L;
		zbBarPoolSpecialCsvRecord[] records2 = records.records;
		foreach (zbBarPoolSpecialCsvRecord zbBarPoolSpecialCsvRecord2 in records2)
		{
			num += zbBarPoolSpecialCsvRecord2.weight;
		}
		Dictionary<(EResourceType, int), zbRateData> dictionary = new Dictionary<(EResourceType, int), zbRateData>();
		records2 = records.records;
		foreach (zbBarPoolSpecialCsvRecord zbBarPoolSpecialCsvRecord3 in records2)
		{
			if (zbBarPoolSpecialCsvRecord3.type == 5)
			{
				(EResourceType, int) key = ((EResourceType)zbBarPoolSpecialCsvRecord3.type, zbBarPoolSpecialCsvRecord3.id);
				float num2 = (float)zbBarPoolSpecialCsvRecord3.weight / (float)num;
				if (dictionary.TryGetValue(key, out var value))
				{
					value.rate += num2;
					continue;
				}
				zbItemData newItemData = new zbItemData((EResourceType)zbBarPoolSpecialCsvRecord3.type, zbBarPoolSpecialCsvRecord3.id, zbBarPoolSpecialCsvRecord3.count);
				dictionary[key] = new zbRateData(newItemData, num2);
			}
		}
		list = dictionary.Values.ToList();
		list.Sort(delegate(zbRateData a, zbRateData b)
		{
			zbHeroCsvRecord recordByKey = zbDataConfig.Instance.heroCsvTable.GetRecordByKey((int)a.itemData.id);
			return zbDataConfig.Instance.heroCsvTable.GetRecordByKey((int)b.itemData.id).rarity.CompareTo(recordByKey.rarity);
		});
		VisualElement val = new VisualElement();
		((VisualElement)m_listView).Add(val);
		int num3 = Mathf.CeilToInt((float)list.Count / 4f);
		for (int num4 = 0; num4 < num3; num4++)
		{
			VisualElement val2 = new VisualElement();
			val2.style.flexDirection = new StyleEnum<FlexDirection>((FlexDirection)2);
			val.Add(val2);
			for (int num5 = 0; num5 < 4 && num4 * 4 + num5 < list.Count; num5++)
			{
				VisualElement val3 = new VisualElement();
				zbBarRateSpecialItem zbBarRateSpecialItem2 = new zbBarRateSpecialItem(this);
				zbBarRateSpecialItem2.Refresh(list[num4 * 4 + num5]);
				val3.Add((VisualElement)(object)zbBarRateSpecialItem2);
				val2.Add(val3);
			}
		}
	}

	private void OnClose(ClickEvent evt)
	{
		Hide();
	}

	private void OnRule(ClickEvent evt)
	{
	}
}
