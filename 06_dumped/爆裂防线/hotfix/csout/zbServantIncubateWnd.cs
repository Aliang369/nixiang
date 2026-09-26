using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using Proto.Types;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbServantIncubateWnd : zbBaseWnd
{
	private VisualElement[] m_slots = (VisualElement[])(object)new VisualElement[4];

	private VisualElement[] m_usings = (VisualElement[])(object)new VisualElement[4];

	private Label[] m_lbsSpeed = (Label[])(object)new Label[4];

	private Label[] m_usingsTime = (Label[])(object)new Label[4];

	private SpineVisualElement[] m_usingsIcon = (SpineVisualElement[])(object)new SpineVisualElement[4];

	private Button[] m_btnsSpeed = (Button[])(object)new Button[4];

	private VisualElement[] m_dones = (VisualElement[])(object)new VisualElement[4];

	private Button[] m_btnsComplete = (Button[])(object)new Button[4];

	private SpineVisualElement[] m_donesIcon = (SpineVisualElement[])(object)new SpineVisualElement[4];

	private VisualElement[] m_idles = (VisualElement[])(object)new VisualElement[4];

	private Button[] m_btnsIdle = (Button[])(object)new Button[4];

	private VisualElement[] m_locks = (VisualElement[])(object)new VisualElement[4];

	private Label[] m_lbsLock = (Label[])(object)new Label[2];

	private Button[] m_btnsUnlock = (Button[])(object)new Button[4];

	private MultiColumnListView m_listView;

	private List<zbItemData> m_listItemDatas = new List<zbItemData>();

	private List<int> rowIndexes = new List<int>();

	private int columnCount = 5;

	private bool columnsInited;

	private zbServantEggsCsvRecord[] m_eggsRecord;

	private Button m_btnClose;

	private IncubateInfo m_detail;

	private Button m_btnPrivilege;

	private HashSet<int> processingSlots = new HashSet<int>();

	protected override void OnEnable()
	{
		base.OnEnable();
		m_listView = UQueryExtensions.Q<MultiColumnListView>(m_RootElement, "listView", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		InitColumns();
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		for (int i = 0; i < 4; i++)
		{
			int tempIndex = i;
			m_slots[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "slot" + i, (string)null);
			m_usings[i] = UQueryExtensions.Q<VisualElement>(m_slots[i], "using", (string)null);
			m_lbsSpeed[i] = UQueryExtensions.Q<Label>(m_usings[i], "lb_speed", (string)null);
			m_usingsTime[i] = UQueryExtensions.Q<Label>(m_usings[i], "lb_time", (string)null);
			m_usingsIcon[i] = UQueryExtensions.Q<SpineVisualElement>(m_usings[i], "icon_core", (string)null);
			m_dones[i] = UQueryExtensions.Q<VisualElement>(m_slots[i], "done", (string)null);
			m_donesIcon[i] = UQueryExtensions.Q<SpineVisualElement>(m_dones[i], "icon_core", (string)null);
			m_btnsComplete[i] = UQueryExtensions.Q<Button>(m_dones[i], "btn_complete", (string)null);
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnsComplete[i], (Action<ClickEvent>)delegate
			{
				OnComplete(tempIndex + 1);
			}, "", (TrickleDown)0);
			m_btnsSpeed[i] = UQueryExtensions.Q<Button>(m_usings[i], "btn_speed", (string)null);
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnsSpeed[i], (Action<ClickEvent>)delegate
			{
				OnSpeed(tempIndex + 1);
			}, "", (TrickleDown)0);
			m_idles[i] = UQueryExtensions.Q<VisualElement>(m_slots[i], "idle", (string)null);
			m_btnsIdle[i] = UQueryExtensions.Q<Button>(m_idles[i], "btn_idle", (string)null);
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnsIdle[i], (Action<ClickEvent>)delegate
			{
				OnIdle();
			}, "", (TrickleDown)0);
			m_locks[i] = UQueryExtensions.Q<VisualElement>(m_slots[i], "lock", (string)null);
			m_btnsUnlock[i] = UQueryExtensions.Q<Button>(m_slots[i], "btn_unlock", (string)null);
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnsUnlock[i], (Action<ClickEvent>)delegate
			{
				OnUnlock(tempIndex + 1);
			}, "", (TrickleDown)0);
		}
		m_lbsLock[0] = UQueryExtensions.Q<Label>(m_locks[2], "lb_lock", (string)null);
		m_lbsLock[1] = UQueryExtensions.Q<Label>(m_locks[3], "lb_lock", (string)null);
		m_btnPrivilege = UQueryExtensions.Q<Button>(m_RootElement, "btn_privilege", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnPrivilege, (Action)OnPrivilege, "", (TrickleDown)0);
		m_eggsRecord = zbDataConfig.Instance.servantEggsCsvTable.GetRecords().records.Values.ToArray();
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Incubate, OnEventUpdate);
		CreateMoneyPanel(new EMoneyType[1] { EMoneyType.Diamond });
	}

	public void RefreshCustom()
	{
		m_detail = zbServerData.sGameData.IncubateInfo;
		if (m_detail.SlotList.Count == 0)
		{
			zbWsMsgHandler.Instance.Incubate_Data_Req();
		}
		else if (m_detail != null)
		{
			RefreshSlots();
			RefreshTime();
			RefreshListView();
		}
	}

	private void RefreshSlots()
	{
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		int count = m_detail.SlotList.Count;
		for (int i = 0; i < 4; i++)
		{
			bool flag = i < count;
			bool flag2 = flag && m_detail.SlotList[i].PropId > 0;
			bool flag3 = (flag & flag2) && zbSelf.Instance.timeHandler.GetServerTime() > m_detail.SlotList[i].EndTm;
			m_usings[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((!flag2 || flag3) ? 1 : 0));
			if (flag2 && !flag3)
			{
				zbServantEggsCsvRecord recordByKey = zbDataConfig.Instance.servantEggsCsvTable.GetRecordByKey(m_detail.SlotList[i].PropId);
				if (recordByKey != null)
				{
					m_usingsIcon[i].skeletonDataAsset = zbAssetLoader.Instance.LoadResouce<SkeletonDataAsset>(recordByKey.anim + "_SkeletonData.asset");
					m_usingsIcon[i].Initialize(true);
					m_usingsIcon[i].AnimationState.ClearTracks();
					m_usingsIcon[i].AnimationState.SetAnimation(0, "idle", true);
				}
			}
			m_idles[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((!flag || flag2) ? 1 : 0));
			m_locks[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag ? 1 : 0));
			m_dones[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag3));
			if (flag3)
			{
				zbServantEggsCsvRecord recordByKey2 = zbDataConfig.Instance.servantEggsCsvTable.GetRecordByKey(m_detail.SlotList[i].PropId);
				if (recordByKey2 != null)
				{
					m_donesIcon[i].skeletonDataAsset = zbAssetLoader.Instance.LoadResouce<SkeletonDataAsset>(recordByKey2.anim + "_SkeletonData.asset");
					m_donesIcon[i].Initialize(true);
					m_donesIcon[i].AnimationState.ClearTracks();
					m_donesIcon[i].AnimationState.SetAnimation(0, "idle", true);
				}
			}
		}
	}

	private void InitColumns()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Expected O, but got Unknown
		if (columnsInited)
		{
			return;
		}
		for (int i = 0; i < columnCount; i++)
		{
			int columnIndex = i;
			m_listView.columns.Add(new Column
			{
				width = Length.op_Implicit(128f),
				makeCell = delegate
				{
					zbItem obj = new zbItem();
					obj.OnEnable(this);
					return (VisualElement)(object)obj;
				},
				bindCell = delegate(VisualElement element, int row)
				{
					//IL_009b: Unknown result type (might be due to invalid IL or missing references)
					//IL_0078: Unknown result type (might be due to invalid IL or missing references)
					int index = row * columnCount + columnIndex;
					if (index < m_listItemDatas.Count)
					{
						zbItem obj = element as zbItem;
						obj?.Refresh(m_listItemDatas[index]);
						element.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
						obj.SetTouchCallback(delegate
						{
							OnSelect(m_listItemDatas[index].id);
						});
					}
					else
					{
						element.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
					}
				}
			});
		}
		columnsInited = true;
	}

	private void RefreshListView()
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		GenerateData();
		int num = Mathf.CeilToInt((float)m_listItemDatas.Count / (float)columnCount);
		rowIndexes.Clear();
		for (int i = 0; i < num; i++)
		{
			rowIndexes.Add(i);
		}
		((BaseVerticalCollectionView)m_listView).itemsSource = null;
		((BaseVerticalCollectionView)m_listView).itemsSource = rowIndexes;
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_listView, "unity-multi-column-view__header-container", (string)null).style.opacity = StyleFloat.op_Implicit(0f);
	}

	private void GenerateData()
	{
		m_listItemDatas.Clear();
		Dictionary<int, PropDetail> dictionary = ((IEnumerable<PropDetail>)zbServerData.sGameData.PropList).ToDictionary((PropDetail p) => p.Tid);
		zbServantEggsCsvRecord[] eggsRecord = m_eggsRecord;
		foreach (zbServantEggsCsvRecord zbServantEggsCsvRecord2 in eggsRecord)
		{
			if (dictionary.TryGetValue(zbServantEggsCsvRecord2.id, out var value))
			{
				m_listItemDatas.Add(new zbItemData(EResourceType.Prop, value.Tid, value.Count));
			}
		}
	}

	private void OnSpeed(int slotIndex)
	{
		int num = Extensions.Value<int>((IEnumerable<JToken>)zbSelf.Instance.servantHandler.servantBase[(object)"incubate"][(object)"speedDiamond"]);
		long num2 = (zbServerData.sGameData.IncubateInfo.SlotList[slotIndex - 1].EndTm - zbSelf.Instance.timeHandler.GetServerTime()) / 60;
		zbUIMgr.Instance.ShowMessageBox(zbUtil.Language("lang.10143"), string.Format(zbUtil.Language("lang.11044"), (num2 < 1) ? 0 : (num2 * num)), delegate(bool isOk)
		{
			if (isOk)
			{
				zbWsMsgHandler.Instance.Incubate_Speed_Req(slotIndex);
			}
		});
	}

	private void OnUnlock(int slotIndex)
	{
		switch (slotIndex)
		{
		case 3:
		{
			int num = Extensions.Value<int>((IEnumerable<JToken>)zbSelf.Instance.servantHandler.servantBase[(object)"incubate"][(object)"unlock3Diamond"]);
			zbUIMgr.Instance.ShowMessageBox(zbUtil.Language("lang.10143"), string.Format(zbUtil.Language("lang.11043"), num), delegate(bool isOk)
			{
				if (isOk)
				{
					zbWsMsgHandler.Instance.Incubate_SlotUnlock_Req();
				}
			});
			break;
		}
		case 4:
			OnPrivilege();
			break;
		}
	}

	private void OnPrivilege()
	{
		zbUIMgr.Instance.ShowServantPrivilege();
	}

	private void OnSelect(long propId)
	{
		IncubateSlotDetail incubateSlotDetail = FindIsIdleSlotDetail();
		if (incubateSlotDetail != null)
		{
			zbWsMsgHandler.Instance.Incubate_Start_Req(incubateSlotDetail.Index, (int)propId);
		}
		else if (m_detail.SlotList.Count == 2 || m_detail.SlotList.Count == 3)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11045"));
		}
		else
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11046"));
		}
	}

	private void OnIdle()
	{
		zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11071"));
	}

	private void OnComplete(int slotIndex)
	{
		if (processingSlots.Contains(slotIndex))
		{
			return;
		}
		processingSlots.Add(slotIndex);
		if (zbServerData.sGameData.ServantInfo != null && zbServerData.sGameData.ServantInfo.ServantList != null && zbServerData.sGameData.ServantInfo.ServantList.Count == zbServerData.sGameData.ServantInfo.BagLimit)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11079"));
			processingSlots.Remove(slotIndex);
			return;
		}
		IncubateSlotDetail incubateSlotDetail = zbServerData.sGameData.IncubateInfo.SlotList[slotIndex - 1];
		zbServantEggsCsvRecord recordByKey = zbDataConfig.Instance.servantEggsCsvTable.GetRecordByKey(incubateSlotDetail.PropId);
		if (recordByKey != null)
		{
			((MonoBehaviour)this).StartCoroutine(DoAnimationAndRequest(slotIndex, recordByKey));
			return;
		}
		zbWsMsgHandler.Instance.Incubate_Complete_Req(slotIndex);
		processingSlots.Remove(slotIndex);
	}

	private IEnumerator DoAnimationAndRequest(int slotIndex, zbServantEggsCsvRecord eggRecord)
	{
		SpineVisualElement obj = m_donesIcon[slotIndex - 1];
		obj.skeletonDataAsset = zbAssetLoader.Instance.LoadResouce<SkeletonDataAsset>(eggRecord.anim + "_SkeletonData.asset");
		obj.Initialize(true);
		obj.AnimationState.ClearTracks();
		TrackEntry val = obj.AnimationState.SetAnimation(0, "receive", false);
		if (val != null)
		{
			yield return (object)new WaitForSeconds(val.Animation.Duration);
		}
		zbWsMsgHandler.Instance.Incubate_Complete_Req(slotIndex);
		processingSlots.Remove(slotIndex);
	}

	private IncubateSlotDetail FindIsIdleSlotDetail()
	{
		return ((IEnumerable<IncubateSlotDetail>)m_detail.SlotList).FirstOrDefault((IncubateSlotDetail r) => r.PropId == 0);
	}

	protected override void UpdateAtShow()
	{
		base.UpdateAtShow();
		RefreshTime();
	}

	protected override void RefreshWhenAutoVisible()
	{
		base.RefreshWhenAutoVisible();
		RefreshCustom();
	}

	protected override void RefreshWhenAutoVisibleCaseNoScene()
	{
		base.RefreshWhenAutoVisibleCaseNoScene();
		RefreshCustom();
	}

	private void RefreshTime()
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		if (m_detail == null || m_detail.SlotList == null)
		{
			return;
		}
		long serverTime = zbSelf.Instance.timeHandler.GetServerTime();
		int count = m_detail.SlotList.Count;
		for (int i = 0; i < count; i++)
		{
			IncubateSlotDetail incubateSlotDetail = m_detail.SlotList[i];
			if (incubateSlotDetail.PropId > 0)
			{
				long num = incubateSlotDetail.EndTm - serverTime;
				bool num2 = num < 0;
				bool flag = m_dones[i].style.display == StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				if (!num2)
				{
					((TextElement)m_usingsTime[i]).text = zbUtil.FormatTimeHHMMSS(num);
				}
				else if (!flag)
				{
					RefreshSlots();
				}
			}
		}
	}

	private void OnEventUpdate(object data)
	{
		RefreshCustom();
	}

	public void OnClose()
	{
		Hide();
	}
}
