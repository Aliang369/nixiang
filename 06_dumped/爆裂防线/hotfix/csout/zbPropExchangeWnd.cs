using System;
using System.Collections;
using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbPropExchangeWnd : zbBaseWnd
{
	private VisualTreeAsset m_itemTA;

	private Button m_btnTip;

	private Label m_lbTipDesc;

	private AutoHideTipController m_tipController;

	private Button m_btnBack;

	private VisualElement m_spriteBanner;

	private Label m_lbTitle;

	private Label m_lbTime;

	private PropExchangeDetail m_data;

	private VisualElement m_progressVal;

	private zbItem m_targetItem;

	private Label m_lbTargetCount;

	private VisualElement[] m_targets = (VisualElement[])(object)new VisualElement[4];

	private zbItem[] m_items = new zbItem[4];

	private Label[] m_lbTargets = (Label[])(object)new Label[4];

	private ListView m_listview;

	private float TARGET_WIDTH = 100f;

	private float PROGRESS_WIDTH = 531f;

	private Dictionary<int, int> m_batchCountMap = new Dictionary<int, int>();

	protected override void OnEnable()
	{
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Invalid comparison between Unknown and I4
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_itemTA = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/operate/tb_propExchangeItem.uxml");
		m_btnTip = UQueryExtensions.Q<Button>(m_RootElement, "btn_tipDesc", (string)null);
		m_lbTipDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_tipDesc", (string)null);
		m_tipController = new AutoHideTipController(m_uiEvent, m_btnTip, m_lbTipDesc);
		m_btnBack = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_listview = UQueryExtensions.Q<ListView>(m_RootElement, "ListView", (string)null);
		m_spriteBanner = UQueryExtensions.Q<VisualElement>(m_RootElement, "banner", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_progressVal = UQueryExtensions.Q<VisualElement>(m_RootElement, "progressVal", (string)null);
		m_targetItem = UQueryExtensions.Q<zbItem>(m_RootElement, "item", (string)null);
		m_targetItem.OnEnable(this);
		m_lbTargetCount = UQueryExtensions.Q<Label>(m_RootElement, "lb_targetCount", (string)null);
		for (int i = 0; i < 4; i++)
		{
			m_targets[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, $"target{i}", (string)null);
			m_items[i] = UQueryExtensions.Q<zbItem>(m_RootElement, $"item{i}", (string)null);
			m_items[i].OnEnable(this);
			m_lbTargets[i] = UQueryExtensions.Q<Label>(m_RootElement, $"lb_target{i}", (string)null);
		}
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBack, (Action)OnClose, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Operate, OnUpdateEventOperate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Prop, OnUpdateEventOperate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Item, OnUpdateEventOperate);
		if ((int)Application.platform == 8)
		{
			((VisualElement)m_btnTip).style.top = StyleLength.op_Implicit(160f);
		}
	}

	public void RefreshCustom(PropExchangeDetail data)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		m_data = data;
		Init();
		m_tipController.ShowAndAutoHide();
		RefreshMoneyPanel();
		zbOperateCsvRecord recordByKey = zbDataConfig.Instance.operateCsvTable.GetRecordByKey(14);
		if (recordByKey != null)
		{
			m_spriteBanner.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.banner));
			((TextElement)m_lbTitle).text = recordByKey.name;
		}
		RefreshTime();
	}

	private void Init()
	{
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		zbItemData data = new zbItemData(EResourceType.Prop, m_data.PropId, 1L);
		m_targetItem.Refresh(data, isNameVis: false, isCountVis: false);
		((TextElement)m_lbTargetCount).text = m_data.Count.ToString();
		float num = m_data.TargetList[m_data.TargetList.Count - 1].Target;
		for (int i = 0; i < 4; i++)
		{
			bool flag = i < m_data.TargetList.Count;
			m_targets[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			if (!flag)
			{
				continue;
			}
			float num2 = (float)m_data.TargetList[i].Target / num * PROGRESS_WIDTH - TARGET_WIDTH / 1.5f;
			m_targets[i].style.left = StyleLength.op_Implicit(num2);
			zbItemData data2 = new zbItemData(m_data.TargetList[i].AwardList[0].Type, m_data.TargetList[i].AwardList[0].Tid, m_data.TargetList[i].AwardList[0].Count);
			m_items[i].Refresh(data2);
			bool flag2 = m_data.TargetList[i].State == ECommonState.Yes;
			bool gettedVisible = m_data.TargetList[i].State == ECommonState.End;
			m_items[i].SetSpineVisible(flag2);
			int tempIndex = m_data.TargetList[i].Index;
			if (flag2)
			{
				m_items[i].SetTouchCallback(delegate
				{
					OnReceive(tempIndex);
				});
			}
			else
			{
				m_items[i].SetTouchCallback(null);
			}
			m_items[i].SetGettedVisible(gettedVisible);
			((TextElement)m_lbTargets[i]).text = m_data.TargetList[i].Target.ToString();
		}
		float num3 = (float)m_data.Count / num * 100f;
		m_progressVal.style.width = StyleLength.op_Implicit(new Length((num3 <= 100f) ? num3 : 100f, (LengthUnit)1));
		UpdateListview();
	}

	private void RefreshMoneyPanel()
	{
		CreateMoneyPanel(new EMoneyType[0]);
		long propId = m_data.PropId;
		long countByPropTid = zbSelf.Instance.itemHandler.GetCountByPropTid((int)propId);
		stResorceIconInfo resorceIconPath = zbResourceHelper.GetResorceIconPath(EResourceType.Prop, propId);
		m_moneyPanel.RefreshOther(0, resorceIconPath.strIconPath, countByPropTid, delegate
		{
			zbUIMgr.Instance.ShowTip(EResourceType.Prop, propId, 0L);
		});
	}

	private void OnReceive(int index)
	{
		zbWsMsgHandler.Instance.Operate_PropExchangeTarget_Req(m_data.BaseInfo.UUID, index);
	}

	private void UpdateListview()
	{
		((VisualElement)m_listview).Clear();
		m_listview.makeItem = () => (VisualElement)(object)new zbPropExchangeItem(this, m_itemTA);
		m_listview.bindItem = delegate(VisualElement element, int index)
		{
			(element as zbPropExchangeItem).Init(m_data.ExchangeList[index], m_data);
		};
		((BaseVerticalCollectionView)m_listview).itemsSource = null;
		((BaseVerticalCollectionView)m_listview).itemsSource = (IList)m_data.ExchangeList;
	}

	private void OnClose()
	{
		Hide();
	}

	private void RefreshTime()
	{
		if (m_data != null)
		{
			((TextElement)m_lbTime).text = zbUtil.FormatTimeHHMMSS(m_data.BaseInfo.EndTm - zbSelf.Instance.timeHandler.GetServerTime());
		}
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		RefreshTime();
	}

	private void OnUpdateEventOperate(object data)
	{
		PropExchangeDetail exchangeInfoByUUID = zbSelf.Instance.operateHandler.GetExchangeInfoByUUID(m_data.BaseInfo.UUID);
		RefreshCustom(exchangeInfoByUUID);
	}

	public int GetBatchCount(int index)
	{
		if (m_batchCountMap.TryGetValue(index, out var value))
		{
			return value;
		}
		return 1;
	}

	public void SetBatchCount(int index, int count)
	{
		m_batchCountMap[index] = count;
	}
}
