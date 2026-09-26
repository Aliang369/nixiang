using System;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbFacilityMineItem : zbViewBaseItem
{
	public class UxmlFactory : UxmlFactory<zbFacilityMineItem>
	{
	}

	private zbFacilityHubWnd m_wndParent;

	private zbMineFacility m_parent;

	private VisualElement m_spriteBg;

	private VisualElement m_doubleAward;

	private Label m_lbName;

	private Button m_btnTouch;

	private zbItem[] m_items = new zbItem[5];

	private VisualElement[] m_stars = (VisualElement[])(object)new VisualElement[6];

	private Label m_lbCost;

	private zbItem m_costItem;

	private VisualElement m_spriteSelect;

	private VisualElement m_notify;

	private zbFacilityCsvRecord m_record;

	public zbFacilityMineItem()
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/facility/tb_facilityMine.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
	}

	public void OnEnable(zbFacilityHubWnd wndParent, zbMineFacility parent, int index)
	{
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		m_parent = parent;
		m_spriteBg = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg", (string)null);
		m_lbName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_name", (string)null);
		m_doubleAward = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "double_award", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnTouch, "", (TrickleDown)0);
		for (int i = 0; i < 3; i++)
		{
			m_items[i] = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, "item" + i, (string)null);
			m_items[i].OnEnable(m_wndParent);
		}
		for (int j = 0; j < 6; j++)
		{
			m_stars[j] = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "star" + j, (string)null);
		}
		m_lbCost = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_cost", (string)null);
		m_costItem = UQueryExtensions.Q<zbItem>((VisualElement)(object)m_lbCost, "itemCost", (string)null);
		m_costItem.OnEnable(m_wndParent);
		m_spriteSelect = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_select", (string)null);
		m_spriteSelect.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_notify = zbUtil.CreateNotifyTag(m_spriteBg);
		InitRecord(index + 1);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	private void InitRecord(int type)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		m_record = zbDataConfig.Instance.facilityCsvTable.GetRecordByKey(type);
		if (m_record != null)
		{
			m_spriteBg.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>($"ui/image/facility/facility_{m_record.type}.png"));
			((TextElement)m_lbName).text = m_record.name;
			m_costItem.Refresh(new zbItemData(EResourceType.Item, m_record.createCostItem, 1L), isNameVis: false, isCountVis: false);
			RefreshStars();
		}
	}

	private void RefreshStars()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		int quality = m_record.quality;
		for (int i = 0; i < 6; i++)
		{
			bool flag = i < quality;
			m_stars[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		}
	}

	public void Refresh(zbFacilityCsvRecord selectRecord)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		m_spriteSelect.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((selectRecord.type != m_record.type) ? 1 : 0));
		m_doubleAward.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbSelf.Instance.vipHandler.GetDoubleAwardFunType() != EFuncType.Facility));
		long resourceCount = zbSelf.Instance.dataHandler.GetResourceCount(EResourceType.Item, m_record.createCostItem);
		((TextElement)m_lbCost).text = zbUtil.GetNeedHaveRichString(resourceCount, 1L);
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((zbSelf.Instance.itemHandler.GetCountByItemTid(m_record.createCostItem) <= 0 || !zbSelf.Instance.moneyHandler.HasEnoughMoney(4, zbSelf.Instance.facilityHandler.tiliCost * 10)) ? 1 : 0));
		RefreshAwards();
	}

	private void RefreshAwards()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		if (m_record == null)
		{
			return;
		}
		for (int i = 0; i < 3; i++)
		{
			((VisualElement)m_items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		int num = 0;
		int num2 = ((zbSelf.Instance.vipHandler.GetDoubleAwardFunType() != EFuncType.Facility) ? 1 : 2);
		int[][] tierAwardList = m_record.tierAwardList1;
		foreach (int[] array in tierAwardList)
		{
			if (num < m_items.Length)
			{
				((VisualElement)m_items[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_items[num].Refresh(new zbItemData((EResourceType)array[0], array[1], array[2] * num2));
				num++;
			}
		}
		tierAwardList = m_record.tierAwardList2;
		foreach (int[] array2 in tierAwardList)
		{
			if (num < m_items.Length)
			{
				((VisualElement)m_items[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_items[num].Refresh(new zbItemData((EResourceType)array2[0], array2[1], array2[2] * num2));
				num++;
			}
		}
		tierAwardList = m_record.tierAwardList3;
		foreach (int[] array3 in tierAwardList)
		{
			if (num < m_items.Length)
			{
				((VisualElement)m_items[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_items[num].Refresh(new zbItemData((EResourceType)array3[0], array3[1], array3[2] * num2));
				num++;
			}
		}
	}

	private void OnTouch()
	{
		m_wndParent.m_facilityType = m_record.type;
		m_parent.RefreshFacility();
	}
}
