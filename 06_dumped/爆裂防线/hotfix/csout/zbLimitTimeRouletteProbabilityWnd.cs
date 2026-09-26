using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine.UIElements;
using zbType;

public class zbLimitTimeRouletteProbabilityWnd : zbBaseWnd
{
	private Button m_btnTouch;

	private Button m_btnClose;

	private Label m_lbTitle;

	private VisualElement[] m_items = (VisualElement[])(object)new VisualElement[15];

	private LimitTimeRouletteRandDetail[] m_details;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		for (int i = 0; i < 15; i++)
		{
			m_items[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "item" + i, (string)null);
			UQueryExtensions.Q<zbItem>(m_items[i], (string)null, (string)null).OnEnable(this);
		}
		AddDark();
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
	}

	public void RefreshCustom(LimitTimeRouletteRandDetail[] limitTimeRouletteDetails, EOperateType type)
	{
		//IL_0435: Unknown result type (might be due to invalid IL or missing references)
		m_details = limitTimeRouletteDetails;
		zbLimitTimeRouletteCsvRecord[] records = zbDataConfig.Instance.limitTimeRouletteCsvTable.GetRecords().records;
		zbLimitTimeTreasureCsvRecord[] records2 = zbDataConfig.Instance.limitTimeTreasureCsvTable.GetRecords().records;
		zbLimitTimeVendorCsvRecord[] records3 = zbDataConfig.Instance.limitTimeVendorCsvTable.GetRecords().records;
		zbBossRouletteCsvRecord[] records4 = zbDataConfig.Instance.bossRouletteCsvTable.GetRecords().records;
		zbWallSubjectRouletteCsvRecord[] records5 = zbDataConfig.Instance.wallSubjectRouletteCsvTable.GetRecords().records;
		zbMechaSubjectRouletteCsvRecord[] records6 = zbDataConfig.Instance.mechaSubjectRouletteCsvTable.GetRecords().records;
		List<zbItemData> list = new List<zbItemData>();
		List<int> list2 = new List<int>();
		LimitTimeRouletteRandDetail[] details = m_details;
		foreach (LimitTimeRouletteRandDetail award in details)
		{
			switch (type)
			{
			case EOperateType.LimitTimeRoulette:
			{
				zbLimitTimeRouletteCsvRecord zbLimitTimeRouletteCsvRecord2 = records.FirstOrDefault((zbLimitTimeRouletteCsvRecord r) => r.type == (int)award.AwardDetail.Type && r.id == award.AwardDetail.Tid && r.count == award.AwardDetail.Count);
				if (zbLimitTimeRouletteCsvRecord2 != null)
				{
					list.Add(new zbItemData((EResourceType)zbLimitTimeRouletteCsvRecord2.type, zbLimitTimeRouletteCsvRecord2.id, zbLimitTimeRouletteCsvRecord2.count));
					list2.Add(zbLimitTimeRouletteCsvRecord2.rate);
				}
				break;
			}
			case EOperateType.LimitTimeTreasure:
			{
				zbLimitTimeTreasureCsvRecord zbLimitTimeTreasureCsvRecord2 = records2.FirstOrDefault((zbLimitTimeTreasureCsvRecord r) => r.type == (int)award.AwardDetail.Type && r.id == award.AwardDetail.Tid && r.count == award.AwardDetail.Count);
				if (zbLimitTimeTreasureCsvRecord2 != null)
				{
					list.Add(new zbItemData((EResourceType)zbLimitTimeTreasureCsvRecord2.type, zbLimitTimeTreasureCsvRecord2.id, zbLimitTimeTreasureCsvRecord2.count));
					list2.Add(zbLimitTimeTreasureCsvRecord2.rate);
				}
				break;
			}
			case EOperateType.LimitTimeVendor:
			{
				zbLimitTimeVendorCsvRecord zbLimitTimeVendorCsvRecord2 = records3.FirstOrDefault((zbLimitTimeVendorCsvRecord r) => r.type == (int)award.AwardDetail.Type && r.id == award.AwardDetail.Tid && r.count == award.AwardDetail.Count);
				if (zbLimitTimeVendorCsvRecord2 != null)
				{
					list.Add(new zbItemData((EResourceType)zbLimitTimeVendorCsvRecord2.type, zbLimitTimeVendorCsvRecord2.id, zbLimitTimeVendorCsvRecord2.count));
					list2.Add(zbLimitTimeVendorCsvRecord2.rate);
				}
				break;
			}
			case EOperateType.BossSubjectRoulette:
			{
				zbBossRouletteCsvRecord zbBossRouletteCsvRecord2 = records4.FirstOrDefault((zbBossRouletteCsvRecord r) => r.type == (int)award.AwardDetail.Type && r.rare == (int)award.Rare && r.rare == 1 && r.count == award.AwardDetail.Count);
				if (zbBossRouletteCsvRecord2 != null)
				{
					list.Add(new zbItemData(award.AwardDetail.Type, award.AwardDetail.Tid, award.AwardDetail.Count));
					list2.Add(zbBossRouletteCsvRecord2.showWeight);
				}
				zbBossRouletteCsvRecord zbBossRouletteCsvRecord3 = records4.FirstOrDefault((zbBossRouletteCsvRecord r) => r.type == (int)award.AwardDetail.Type && r.id == award.AwardDetail.Tid && r.count == award.AwardDetail.Count);
				if (zbBossRouletteCsvRecord3 != null)
				{
					list.Add(new zbItemData((EResourceType)zbBossRouletteCsvRecord3.type, zbBossRouletteCsvRecord3.id, zbBossRouletteCsvRecord3.count));
					list2.Add(zbBossRouletteCsvRecord3.showWeight);
				}
				break;
			}
			case EOperateType.WallSubjectRoulette:
			{
				zbWallSubjectRouletteCsvRecord zbWallSubjectRouletteCsvRecord2 = records5.FirstOrDefault((zbWallSubjectRouletteCsvRecord r) => r.rare == (int)award.Rare && r.rare == 1);
				if (zbWallSubjectRouletteCsvRecord2 != null)
				{
					list.Add(new zbItemData(award.AwardDetail.Type, award.AwardDetail.Tid, award.AwardDetail.Count));
					list2.Add(zbWallSubjectRouletteCsvRecord2.rate);
				}
				zbWallSubjectRouletteCsvRecord zbWallSubjectRouletteCsvRecord3 = records5.FirstOrDefault((zbWallSubjectRouletteCsvRecord r) => r.type == (int)award.AwardDetail.Type && r.id == award.AwardDetail.Tid && r.count == award.AwardDetail.Count);
				if (zbWallSubjectRouletteCsvRecord3 != null)
				{
					list.Add(new zbItemData((EResourceType)zbWallSubjectRouletteCsvRecord3.type, zbWallSubjectRouletteCsvRecord3.id, zbWallSubjectRouletteCsvRecord3.count));
					list2.Add(zbWallSubjectRouletteCsvRecord3.rate);
				}
				break;
			}
			case EOperateType.MechaSubjectRoulette:
			{
				zbMechaSubjectRouletteCsvRecord zbMechaSubjectRouletteCsvRecord2 = records6.FirstOrDefault((zbMechaSubjectRouletteCsvRecord r) => r.rare == (int)award.Rare && r.rare == 1);
				if (zbMechaSubjectRouletteCsvRecord2 != null)
				{
					list.Add(new zbItemData(award.AwardDetail.Type, award.AwardDetail.Tid, award.AwardDetail.Count));
					list2.Add(zbMechaSubjectRouletteCsvRecord2.rate);
				}
				zbMechaSubjectRouletteCsvRecord zbMechaSubjectRouletteCsvRecord3 = records6.FirstOrDefault((zbMechaSubjectRouletteCsvRecord r) => r.type == (int)award.AwardDetail.Type && r.id == award.AwardDetail.Tid && r.count == award.AwardDetail.Count);
				if (zbMechaSubjectRouletteCsvRecord3 != null)
				{
					list.Add(new zbItemData((EResourceType)zbMechaSubjectRouletteCsvRecord3.type, zbMechaSubjectRouletteCsvRecord3.id, zbMechaSubjectRouletteCsvRecord3.count));
					list2.Add(zbMechaSubjectRouletteCsvRecord3.rate);
				}
				break;
			}
			}
		}
		for (int num = 0; num < 15; num++)
		{
			bool flag = num < list.Count;
			m_items[num].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			if (flag)
			{
				UQueryExtensions.Q<zbItem>(m_items[num], (string)null, (string)null).Refresh(list[num]);
				((TextElement)UQueryExtensions.Q<Label>(m_items[num], "lb_desc", (string)null)).text = list2[num] + "%";
			}
		}
	}

	private void OnClose()
	{
		Hide();
	}
}
