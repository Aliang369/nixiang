using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbPassPortPanel
{
	private Label m_lbLv;

	private VisualElement m_iconTime;

	private Label m_lbTime;

	private Label m_lbProVal;

	private Label m_lbProText;

	private Label m_lbActiveVal;

	private Label m_lbTitle1;

	private Label m_lbTitle2;

	private Button m_btnBuyLv;

	private Button m_btnReceive;

	private Button m_btnActivate;

	private zbPassportItem m_grandPrize;

	private ListView m_listView;

	private ScrollView m_scrollView;

	private zbBaseWnd m_wndParent;

	private JToken m_passportBaseRecord;

	private int m_currentGrandIndex = -1;

	private List<int> m_grandIndexes;

	private List<zbPassportLevelAwardCsvRecord> m_records;

	public void OnEnable(zbBaseWnd wndParent, VisualElement root)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Invalid comparison between Unknown and I4
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		m_lbLv = UQueryExtensions.Q<Label>(root, "lb_lv", (string)null);
		m_iconTime = UQueryExtensions.Q<VisualElement>(root, "icon_time", (string)null);
		if ((int)Application.platform == 8)
		{
			m_iconTime.style.top = StyleLength.op_Implicit(180f);
		}
		m_lbTime = UQueryExtensions.Q<Label>(root, "lb_time", (string)null);
		m_lbProVal = UQueryExtensions.Q<Label>(root, "lb_proVal", (string)null);
		m_lbProText = UQueryExtensions.Q<Label>(root, "lb_proText", (string)null);
		m_lbActiveVal = UQueryExtensions.Q<Label>(root, "lb_activeVal", (string)null);
		m_lbTitle1 = UQueryExtensions.Q<Label>(root, "lb_title1", (string)null);
		m_lbTitle2 = UQueryExtensions.Q<Label>(root, "lb_title2", (string)null);
		m_btnBuyLv = UQueryExtensions.Q<Button>(root, "btn_buyLv", (string)null);
		m_btnReceive = UQueryExtensions.Q<Button>(root, "btn_receive", (string)null);
		m_btnActivate = UQueryExtensions.Q<Button>(root, "btn_activate", (string)null);
		m_listView = UQueryExtensions.Q<ListView>(root, (string)null, (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		((BaseVerticalCollectionView)m_listView).fixedItemHeight = 110f;
		m_scrollView = UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null);
		((BaseVerticalCollectionView)m_listView).virtualizationMethod = (CollectionVirtualizationMethod)0;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		m_grandPrize = new zbPassportItem();
		m_grandPrize = UQueryExtensions.Q<zbPassportItem>(root, "grandPrize", (string)null);
		m_grandPrize.OnEnable(wndParent);
		m_records = zbDataConfig.Instance.passportLevelAwardCsvTable.GetRecords().records.Values.ToList();
		m_grandIndexes = new List<int>();
		for (int i = 0; i < m_records.Count; i++)
		{
			if (m_records[i].isGrand > 0)
			{
				m_grandIndexes.Add(i);
			}
		}
		wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBuyLv, (Action)OnBuyLevel, "", (TrickleDown)0);
		wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnReceive, (Action)OnReceive, "", (TrickleDown)0);
		wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnActivate, (Action)OnActivate, "", (TrickleDown)0);
	}

	public void Refresh()
	{
		Init();
		UpdateListView();
		InitGrandPrize();
	}

	private void InitGrandPrize()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		int num = Mathf.FloorToInt(m_scrollView.scrollOffset.y / 110f);
		int num2 = -1;
		for (int i = 0; i < m_grandIndexes.Count; i++)
		{
			if (m_grandIndexes[i] > num + 5)
			{
				num2 = i;
				break;
			}
		}
		if (num2 != -1 && num2 != m_currentGrandIndex)
		{
			m_currentGrandIndex = num2;
			int index = m_grandIndexes[m_currentGrandIndex];
			m_grandPrize.Init(m_records[index]);
			m_grandPrize.SetMaskVisible(visible: false);
		}
	}

	private void Init()
	{
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		PassportInfo passportInfo = zbServerData.sGameData.RechargeActiveInfo.PassportInfo;
		m_passportBaseRecord = (JToken)(object)zbDataConfig.Instance.passportBaseJsonTable.GetRecord();
		int num = Extensions.Value<int>((IEnumerable<JToken>)m_passportBaseRecord[(object)"levelExp"]);
		bool flag = passportInfo.BaseInfo.Active == EBoolState.True;
		if (passportInfo != null)
		{
			PassportLevelInfo levelInfo = passportInfo.LevelInfo;
			_ = passportInfo.BaseInfo;
			bool flag2 = levelInfo.Level == m_passportBaseRecord[(object)"buyLevel"][(object)0].Value<int>((object)"limit");
			((TextElement)m_lbProText).text = (flag2 ? "Max" : $"{levelInfo.Exp}/{num}");
			float num2 = (flag2 ? 1f : Mathf.Clamp01((float)levelInfo.Exp / (float)num));
			((VisualElement)m_lbProVal).style.width = new StyleLength(Length.Percent(num2 * 100f));
			((TextElement)m_lbActiveVal).text = (flag2 ? "Max" : levelInfo.Level.ToString());
			((TextElement)m_lbLv).text = levelInfo.Level.ToString();
			string priceByGoodsId = zbSelf.Instance.vipHandler.GetPriceByGoodsId(passportInfo.BaseInfo.GoodsId);
			((TextElement)m_btnActivate).text = priceByGoodsId;
		}
		((VisualElement)m_btnActivate).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag ? 1 : 0));
	}

	public void UpdateListView()
	{
		((VisualElement)m_listView).Clear();
		KeyValuePair<int, zbPassportLevelAwardCsvRecord>[] itemsSource = zbDataConfig.Instance.passportLevelAwardCsvTable.GetRecords().records.ToArray();
		m_listView.makeItem = delegate
		{
			zbPassportItem obj = new zbPassportItem();
			obj.OnEnable(m_wndParent);
			return (VisualElement)(object)obj;
		};
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			zbPassportLevelAwardCsvRecord recordByKey = zbDataConfig.Instance.passportLevelAwardCsvTable.GetRecordByKey(index + 1);
			(element as zbPassportItem).Init(recordByKey);
		};
		((BaseVerticalCollectionView)m_listView).itemsSource = null;
		((BaseVerticalCollectionView)m_listView).itemsSource = itemsSource;
		int level = zbServerData.sGameData.RechargeActiveInfo.PassportInfo.LevelInfo.Level;
		int scrollTo = Mathf.Min((level > 2) ? (level + 2) : level, 47);
		((VisualElement)m_listView).schedule.Execute((Action)delegate
		{
			((BaseVerticalCollectionView)m_listView).ScrollToItemById(scrollTo);
		});
	}

	private void OnBuyLevel()
	{
		if (zbServerData.sGameData.RechargeActiveInfo.PassportInfo.LevelInfo.Level == zbServerData.sGameData.RechargeActiveInfo.PassportInfo.LevelInfo.BuyLevelList[0].LimitNum)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10398"));
		}
		else
		{
			zbUIMgr.Instance.ShowPassportBuyLevel();
		}
	}

	private void OnReceive()
	{
		if (zbSelf.Instance.passportHandler.HasPassportRedPoint())
		{
			PassportInfo passportInfo = zbServerData.sGameData.RechargeActiveInfo.PassportInfo;
			zbWsMsgHandler.Instance.Passport_LevelAward_Req(passportInfo.LevelInfo.Level);
		}
		else
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10426"));
		}
	}

	private void OnActivate()
	{
		if (zbServerData.sGameData.RechargeActiveInfo.PassportInfo.BaseInfo.Active == EBoolState.False)
		{
			zbChannel_Pay.OnPay(zbServerData.sGameData.RechargeActiveInfo.PassportInfo.BaseInfo.GoodsId);
		}
	}

	public void UpdatePerSecond()
	{
		long startTm = zbServerData.sGameData.RechargeActiveInfo.PassportInfo.BaseInfo.StartTm;
		long endTm = zbServerData.sGameData.RechargeActiveInfo.PassportInfo.BaseInfo.EndTm;
		long serverTime = zbSelf.Instance.timeHandler.GetServerTime();
		if (startTm <= serverTime && serverTime <= endTm)
		{
			((TextElement)m_lbTime).text = zbUtil.FormatTimeHHMMSS(endTm - serverTime);
		}
		else
		{
			((TextElement)m_lbTime).text = zbUtil.Language("lang.10345");
		}
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btnReceive, "red_dot", (string)null).visible = zbSelf.Instance.passportHandler.HasPassportRedPoint();
	}

	public void UpdatePassportPanel()
	{
		if (!(((BaseVerticalCollectionView)m_listView).fixedItemHeight <= 0f))
		{
			InitGrandPrize();
		}
	}
}
