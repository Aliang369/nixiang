using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbDailySignWnd : zbBaseWnd
{
	private ScrollView signAwardList;

	[SerializeField]
	private VisualTreeAsset signItemTemplate;

	private List<zbSignCsvRecord> allSignRecords;

	private Button m_btnClose;

	private Button m_btnOnGet;

	private Label m_lbdayNum;

	private Label m_lbDailyLoginDayNum;

	private zbItem[] m_items = new zbItem[3];

	private List<VisualElement> m_vItems = new List<VisualElement>();

	protected override void OnEnable()
	{
		base.OnEnable();
		InitUI();
		InitSignList();
		CreateMoneyPanel(new EMoneyType[2]
		{
			EMoneyType.Diamond,
			EMoneyType.Gold
		});
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Activity, OnEventUpdate);
	}

	private void InitUI()
	{
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_btnOnGet = UQueryExtensions.Q<Button>(m_RootElement, "btn_onGet", (string)null);
		m_lbdayNum = UQueryExtensions.Q<Label>(m_RootElement, "lb_dayNum", (string)null);
		m_lbDailyLoginDayNum = UQueryExtensions.Q<Label>(m_RootElement, "lb_dailyLoginDayNum", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnOnGet, (Action<ClickEvent>)OnGetAward, "", (TrickleDown)0);
		for (int i = 0; i < m_items.Length; i++)
		{
			m_items[i] = UQueryExtensions.Q<zbItem>(m_RootElement, $"item{i}", (string)null);
			m_items[i]?.OnEnable(this);
		}
	}

	private void InitSignList()
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		signAwardList = UQueryExtensions.Q<ScrollView>(m_RootElement, "signAwardList", (string)null);
		signAwardList.mode = (ScrollViewMode)0;
		if (signAwardList == null || (Object)(object)signItemTemplate == (Object)null)
		{
			return;
		}
		((VisualElement)signAwardList).Clear();
		m_vItems.Clear();
		VisualElement val = UQueryExtensions.Q<VisualElement>((VisualElement)(object)signAwardList, "unity-content-container", (string)null);
		if (val != null)
		{
			val.style.flexDirection = StyleEnum<FlexDirection>.op_Implicit((FlexDirection)2);
			val.style.flexWrap = StyleEnum<Wrap>.op_Implicit((Wrap)1);
			val.style.alignContent = StyleEnum<Align>.op_Implicit((Align)1);
			val.style.alignSelf = StyleEnum<Align>.op_Implicit((Align)1);
		}
		for (int i = 0; i < 30; i++)
		{
			VisualElement val2 = (VisualElement)(object)signItemTemplate.Instantiate();
			val2.name = $"item_{i}";
			UQueryExtensions.Q<zbItem>(val2, "item", (string)null)?.OnEnable(this);
			((TextElement)UQueryExtensions.Q<Label>(val2, "lb_day", (string)null)).text = $"Day {i + 1}";
			Button val3 = UQueryExtensions.Q<Button>(val2, "btn_item", (string)null);
			if (val3 != null)
			{
				int day = i + 1;
				m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)val3, (Action<ClickEvent>)delegate(ClickEvent evt)
				{
					OnTouchItem(evt, day);
				}, "", (TrickleDown)0);
			}
			m_vItems.Add(val2);
			val2.style.marginRight = StyleLength.op_Implicit(20f);
			((VisualElement)signAwardList).Add(val2);
		}
	}

	private void OnEventUpdate(object data)
	{
		Refresh();
	}

	public override void Refresh()
	{
		Dictionary<int, zbSignCsvRecord> records = zbDataConfig.Instance.signCsvTable.GetRecords().records;
		if (records == null)
		{
			return;
		}
		for (int i = 0; i < m_vItems.Count; i++)
		{
			int key = i + 1;
			if (records.TryGetValue(key, out var value))
			{
				VisualElement val = m_vItems[i];
				zbItem obj = UQueryExtensions.Q<zbItem>(val, "item", (string)null);
				zbItemData data = new zbItemData((EResourceType)value.award[0], value.award[1], value.award[2]);
				obj?.Refresh(data);
				RefreshItemState(val, value);
			}
		}
		RefreshTarget();
	}

	private void RefreshItemState(VisualElement itemRoot, zbSignCsvRecord record)
	{
		int signDay = zbServerData.sGameData.ActivityInfo.DailySign.SignDay;
		ECommonState dailyState = zbServerData.sGameData.ActivityInfo.DailySign.DailyState;
		int day = record.day;
		bool flag = day == signDay;
		bool visible = day < signDay || (flag && dailyState == ECommonState.End);
		bool flag2 = flag && dailyState == ECommonState.Yes;
		bool visible2 = day > signDay;
		VisualElement val = UQueryExtensions.Q(itemRoot, "doubleTag", (string)null);
		VisualElement val2 = UQueryExtensions.Q(itemRoot, "spriteRedDot", (string)null);
		VisualElement val3 = UQueryExtensions.Q(itemRoot, "spriteGetAward", (string)null);
		Button val4 = UQueryExtensions.Q<Button>(itemRoot, "btn_item", (string)null);
		VisualElement val5 = UQueryExtensions.Q<VisualElement>(itemRoot, "bg", (string)null);
		VisualElement val6 = UQueryExtensions.Q<VisualElement>(itemRoot, "sign_cell_sel", (string)null);
		VisualElement val7 = UQueryExtensions.Q<VisualElement>(itemRoot, "layer", (string)null);
		if (val != null)
		{
			val.visible = record.doubleTag == 1;
		}
		if (val2 != null)
		{
			val2.visible = flag2;
		}
		if (val3 != null)
		{
			val3.visible = visible;
		}
		if (val4 != null)
		{
			((VisualElement)val4).visible = flag2;
		}
		if (val6 != null)
		{
			val6.visible = flag2;
		}
		if (val5 != null)
		{
			val5.visible = !flag2;
		}
		if (val7 != null)
		{
			val7.visible = visible2;
		}
	}

	private void RefreshTarget()
	{
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < m_items.Length; i++)
		{
			((VisualElement)m_items[i]).visible = false;
		}
		Dictionary<int, zbSignTargetCsvRecord> records = zbDataConfig.Instance.signTargetCsvTable.GetRecords().records;
		int key = records.Keys.Max();
		zbSignTargetCsvRecord zbSignTargetCsvRecord2 = records[key];
		int num = zbSignTargetCsvRecord2.day;
		if (zbServerData.sGameData.ActivityInfo.DailySign.TargetAward == null)
		{
			((VisualElement)m_btnOnGet).visible = false;
			UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btnOnGet, "spriteRedDot", (string)null).visible = false;
			((TextElement)m_lbDailyLoginDayNum).text = zbServerData.sGameData.ActivityInfo.DailySign.SignTotal + "/" + zbServerData.sGameData.ActivityInfo.DailySign.SignTotal;
		}
		else
		{
			zbSignTargetCsvRecord2 = zbDataConfig.Instance.signTargetCsvTable.GetRecordByKey(zbServerData.sGameData.ActivityInfo.DailySign.TargetAward.TargetIndex);
			num = zbServerData.sGameData.ActivityInfo.DailySign.SignTotal;
			bool flag = zbServerData.sGameData.ActivityInfo.DailySign.TargetAward.TargetState == ECommonState.Yes || zbServerData.sGameData.ActivityInfo.DailySign.SignTotal >= zbSignTargetCsvRecord2.day;
			((VisualElement)m_btnOnGet).visible = true;
			UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btnOnGet, "spriteRedDot", (string)null).visible = flag;
			if (flag)
			{
				((TextElement)m_btnOnGet).text = zbUtil.Language("lang.10090");
				((VisualElement)m_btnOnGet).pickingMode = (PickingMode)0;
				((VisualElement)m_btnOnGet).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_180_80_yellow.png"));
			}
			else
			{
				((TextElement)m_btnOnGet).text = zbUtil.Language("lang.10089");
				((VisualElement)m_btnOnGet).pickingMode = (PickingMode)1;
				((VisualElement)m_btnOnGet).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_180_80_gray.png"));
			}
		}
		if (zbSignTargetCsvRecord2 != null)
		{
			((TextElement)m_lbdayNum).text = string.Format(zbUtil.Language("lang.10179"), zbSignTargetCsvRecord2.day);
			((TextElement)m_lbDailyLoginDayNum).text = num + "/" + zbSignTargetCsvRecord2.day;
			for (int j = 0; j < m_items.Length && j < zbSignTargetCsvRecord2.awardList.Length; j++)
			{
				zbItemData data = new zbItemData((EResourceType)zbSignTargetCsvRecord2.awardList[j][0], zbSignTargetCsvRecord2.awardList[j][1], zbSignTargetCsvRecord2.awardList[j][2]);
				m_items[j].Refresh(data);
				((VisualElement)m_items[j]).visible = true;
			}
		}
	}

	private void OnGetAward(ClickEvent evt)
	{
		zbWsMsgHandler.Instance.Activity_SignTargetAward_Req();
	}

	private void OnTouchItem(ClickEvent evt, int day)
	{
		zbWsMsgHandler.Instance.Activity_Sign_Req();
	}

	private void OnClose(ClickEvent evt)
	{
		Hide();
	}
}
