using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Collections;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

public class zbRestaurantWnd : zbBaseWnd
{
	private VisualElement m_specialTask;

	private zbItem[] m_specialTaskAwards = new zbItem[4];

	private VisualElement m_slotList;

	private VisualElement[] m_slots = (VisualElement[])(object)new VisualElement[15];

	private VisualElement[] m_spriteSlotBgs = (VisualElement[])(object)new VisualElement[15];

	private VisualElement[] m_nodeFoods = (VisualElement[])(object)new VisualElement[15];

	private VisualElement[] m_spriteFoods = (VisualElement[])(object)new VisualElement[15];

	private Label[] m_lbFoodLevels = (Label[])(object)new Label[15];

	private VisualElement[] m_spriteLocks = (VisualElement[])(object)new VisualElement[15];

	private Label[] m_lbLockLevels = (Label[])(object)new Label[15];

	private zbHolidayEventView m_holidayEventView;

	private Button m_btnThemeTask;

	private Button m_btnDailyAward;

	private Label m_lbDailyAwardTime;

	private VisualElement m_notifyThemeTask;

	private VisualElement m_notifyDailyAward;

	private Label m_lbTime;

	private Button m_btnClose;

	private Button m_btnBuy;

	private VisualElement m_spriteOff;

	private Label m_lbOff;

	private Label m_lbCost;

	private Label m_lbBuyName;

	private Button m_btnBuyBatch;

	private Label m_lbCostBatch;

	private Label m_lbBuyBatchName;

	private Button m_btnCompose;

	private Label m_lbComposeName;

	private VisualElement m_iconDelete;

	private VisualElement m_nodeToggle;

	private VisualElement m_spriteOk;

	public RestaurantDetail m_detail;

	public Sprite m_UnlockBg;

	public Sprite m_LockBg;

	public Sprite m_OfferBg;

	public Sprite[] m_foodIcons = (Sprite[])(object)new Sprite[14];

	private bool isDragging;

	private Vector2 dragOffset;

	private VisualElement draggingElement;

	private VisualElement originalSlot;

	private zbRestaurantTarget m_restaurantTarget = new zbRestaurantTarget();

	private VisualElement dragClone;

	private ThemeTaskDetail m_themeTaskDetail;

	private Label m_lbTitle;

	private long m_cost;

	private bool m_isFirstIn = true;

	private float TouchTimeDuration = 20f;

	private int m_targetNeedMaxLevel;

	private bool m_isBatchBuy = true;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_specialTask = UQueryExtensions.Q<VisualElement>(m_RootElement, "specialTask", (string)null);
		for (int i = 0; i < 4; i++)
		{
			m_specialTaskAwards[i] = UQueryExtensions.Q<zbItem>(m_specialTask, $"award{i}", (string)null);
			m_specialTaskAwards[i].OnEnable(this);
		}
		m_slotList = UQueryExtensions.Q<VisualElement>(m_RootElement, "slotList", (string)null);
		for (int j = 0; j < 15; j++)
		{
			m_slots[j] = UQueryExtensions.Q<VisualElement>(m_slotList, $"slot{j}", (string)null);
			m_spriteSlotBgs[j] = UQueryExtensions.Q<VisualElement>(m_slots[j], "bg", (string)null);
			m_nodeFoods[j] = UQueryExtensions.Q<VisualElement>(m_slots[j], "foodInfo", (string)null);
			m_spriteFoods[j] = UQueryExtensions.Q<VisualElement>(m_slots[j], "icon_food", (string)null);
			m_lbFoodLevels[j] = UQueryExtensions.Q<Label>(m_slots[j], "lb_foodLv", (string)null);
			m_spriteLocks[j] = UQueryExtensions.Q<VisualElement>(m_slots[j], "lock", (string)null);
			m_lbLockLevels[j] = UQueryExtensions.Q<Label>(m_slots[j], "lb_unlockLv", (string)null);
			int tempIndex = j;
			m_uiEvent.RegisterCallback<PointerDownEvent>(m_spriteFoods[j], (Action<PointerDownEvent>)delegate(PointerDownEvent evt)
			{
				OnPointerDown(evt, tempIndex);
			}, "", (TrickleDown)0);
			m_uiEvent.RegisterCallback<PointerUpEvent>(m_spriteLocks[j], (Action<PointerUpEvent>)delegate
			{
				OnLocks(tempIndex);
			}, "", (TrickleDown)0);
		}
		m_uiEvent.RegisterCallback<PointerMoveEvent>(m_slotList, (Action<PointerMoveEvent>)OnPointerMove, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<PointerUpEvent>(m_slotList, (Action<PointerUpEvent>)OnPointerUp, "", (TrickleDown)0);
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_btnBuy = UQueryExtensions.Q<Button>(m_RootElement, "btn_buy", (string)null);
		m_lbCost = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnBuy, "lb_cost", (string)null);
		m_spriteOff = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btnBuy, "icon_priceOff", (string)null);
		m_lbOff = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnBuy, "lb_priceOff", (string)null);
		m_lbBuyName = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnBuy, "lb_name", (string)null);
		m_btnBuyBatch = UQueryExtensions.Q<Button>(m_RootElement, "btn_buyBatch", (string)null);
		m_lbCostBatch = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnBuyBatch, "lb_cost", (string)null);
		m_lbBuyBatchName = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnBuyBatch, "lb_name", (string)null);
		m_btnCompose = UQueryExtensions.Q<Button>(m_RootElement, "btn_compose", (string)null);
		m_lbComposeName = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnCompose, "lb_name", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_btnThemeTask = UQueryExtensions.Q<Button>(m_RootElement, "btn_themeTask", (string)null);
		m_btnDailyAward = UQueryExtensions.Q<Button>(m_RootElement, "btn_dailyAward", (string)null);
		m_lbDailyAwardTime = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnDailyAward, "lb_dailyAwardTime", (string)null);
		m_notifyThemeTask = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnThemeTask);
		m_notifyDailyAward = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnDailyAward);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_iconDelete = UQueryExtensions.Q<VisualElement>(m_RootElement, "icon_delete", (string)null);
		m_iconDelete.visible = false;
		for (int num = 0; num < 14; num++)
		{
			m_foodIcons[num] = zbAssetLoader.Instance.LoadResouce<Sprite>($"ui/image/operate/restaurant/food_{num}.png");
		}
		SkeletonAnimation component = ((Component)m_GameObjectSceneRoot.transform.Find("avatar")).GetComponent<SkeletonAnimation>();
		VisualElement root = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_target", (string)null);
		m_restaurantTarget.OnEnable(this, component, root);
		m_nodeToggle = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_toggle", (string)null);
		m_spriteOk = UQueryExtensions.Q<VisualElement>(m_nodeToggle, "sprite_ok", (string)null);
		m_UnlockBg = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/operate/restaurant/food_bg0.png");
		m_LockBg = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/operate/restaurant/food_bg1.png");
		m_OfferBg = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/operate/restaurant/food_bg2.png");
		m_holidayEventView = new zbHolidayEventView();
		m_holidayEventView.OnEnable(this, UQueryExtensions.Q<VisualElement>(m_RootElement, "holidayEvent", (string)null));
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBuy, (Action)OnBuy, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBuyBatch, (Action)OnBuyBatch, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCompose, (Action)OnCompose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnThemeTask, (Action)OnThemeTask, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnDailyAward, (Action)OnReceiveDailyAward, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>(m_nodeToggle, (Action)OnToggle, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Prop, RefreshMoneyPanel);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Restaurant, OnUpdateEventRestaurant);
		m_gameEevent.RegisterEvent(EGameEvent.Event_RestaurantTarget_Ack, OnRestaurantTargetAck);
		m_gameEevent.RegisterEvent(EGameEvent.Event_RestaurantOfferPiece_Ack, OnRestaurantOfferPieceAck);
	}

	private void OnThemeTask()
	{
		if (m_themeTaskDetail != null)
		{
			zbUIMgr.Instance.ShowThemeTask(m_themeTaskDetail);
		}
	}

	private void OnReceiveDailyAward()
	{
		OperateRestaurantDailyAwardDetail dailyAward = m_detail.DailyAward;
		if (dailyAward.Count >= dailyAward.Total)
		{
			zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.10678"), dailyAward.Total));
		}
		else if (dailyAward.Count < dailyAward.Total && dailyAward.NextTm < zbSelf.Instance.timeHandler.GetServerTime())
		{
			zbWsMsgHandler.Instance.Operate_RestaurantDailyAward_Req(m_detail.BaseInfo.UUID);
		}
		else if (dailyAward.Count < dailyAward.Total && dailyAward.NextTm > zbSelf.Instance.timeHandler.GetServerTime())
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10279"));
		}
	}

	public void RefreshCustom(RestaurantDetail detail, bool refreshFinger = false)
	{
		zbSelf.Instance.holidayHandler.GetHolidayCount();
		m_detail = detail;
		m_targetNeedMaxLevel = ((IEnumerable<int>)m_detail.TargetDetail.Target).Max();
		m_themeTaskDetail = zbSelf.Instance.holidayHandler.GetThemeTaskDetail();
		TouchTimeDuration = ((m_detail.TargetDetail.Id <= 3) ? 1f : 20f);
		m_holidayEventView.Refresh(m_detail.BaseInfo.PlanId);
		RefreshTime();
		InitSpecialTask();
		InitSlotList();
		if (m_isFirstIn)
		{
			m_isBatchBuy = zbSelf.Instance.newTagHandler.IsRestaurantBatchBuy(m_detail.BaseInfo.UUID);
			m_isFirstIn = false;
			m_restaurantTarget.PlayInAnimation();
		}
		else
		{
			m_restaurantTarget.Refresh();
			if (refreshFinger && !zbSelf.Instance.restaurantHandler.HasAwardNotify(m_detail))
			{
				RefreshGuideFinger(forceRefresh: true);
			}
		}
		RefreshMoneyPanel();
	}

	private void RefreshMoneyPanel(object data = null)
	{
		CreateMoneyPanel(new EMoneyType[0]);
		long resourceCount = zbSelf.Instance.dataHandler.GetResourceCount(m_detail.SingleCost[0].Type, (int)m_detail.SingleCost[0].Tid);
		stResorceIconInfo resorceIconPath = zbResourceHelper.GetResorceIconPath(m_detail.SingleCost[0].Type, (int)m_detail.SingleCost[0].Tid);
		m_moneyPanel.RefreshOther(0, resorceIconPath.strIconPath, resourceCount, delegate
		{
			zbUIMgr.Instance.ShowTip(m_detail.SingleCost[0].Type, (int)m_detail.SingleCost[0].Tid, 0L);
		});
		RefreshBuyOrBatch();
	}

	private void RefreshBuyOrBatch()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		m_spriteOk.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((!m_isBatchBuy || !zbSelf.Instance.restaurantHandler.CanUseBatch(m_detail)) ? 1 : 0));
		m_cost = (long)((float)m_detail.SingleCost[0].Count * ((10f - (float)m_detail.PriceOff) / 10f));
		if (zbSelf.Instance.restaurantHandler.CanUseBatch(m_detail) && m_isBatchBuy)
		{
			((VisualElement)m_btnBuy).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			RefreshBatch();
			return;
		}
		((VisualElement)m_btnBuy).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		((VisualElement)m_btnBuyBatch).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		((VisualElement)m_btnCompose).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		RefreshBuy();
	}

	private void RefreshBuy()
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		stResorceIconInfo resorceIconPath = zbResourceHelper.GetResorceIconPath(m_detail.SingleCost[0].Type, m_detail.SingleCost[0].Tid);
		((VisualElement)m_lbCost).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resorceIconPath.strIconPath));
		((TextElement)m_lbCost).text = m_cost.ToString();
		((VisualElement)m_lbCost).style.color = StyleColor.op_Implicit(zbSelf.Instance.restaurantHandler.HasEnoughCost(m_detail, m_cost, needTip: false) ? Color.white : Color.red);
		bool flag = m_detail.PriceOff != 0;
		m_spriteOff.visible = flag;
		if (flag)
		{
			((TextElement)m_lbOff).text = $"{m_detail.PriceOff}0%Off";
		}
	}

	private void RefreshBatch()
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		if (zbSelf.Instance.restaurantHandler.CanCompose(m_detail))
		{
			((VisualElement)m_btnBuyBatch).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_btnCompose).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			return;
		}
		((VisualElement)m_btnBuyBatch).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		((VisualElement)m_btnCompose).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		stResorceIconInfo resorceIconPath = zbResourceHelper.GetResorceIconPath(m_detail.SingleCost[0].Type, m_detail.SingleCost[0].Tid);
		((VisualElement)m_lbCostBatch).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resorceIconPath.strIconPath));
		long item = zbSelf.Instance.restaurantHandler.GetBuyBatchCostAndCount(m_detail).batchCost;
		((TextElement)m_lbCostBatch).text = Mathf.Max((float)item, (float)m_cost).ToString();
		((VisualElement)m_lbCostBatch).style.color = StyleColor.op_Implicit(zbSelf.Instance.restaurantHandler.HasEnoughCost(m_detail, m_cost, needTip: false) ? Color.white : Color.red);
	}

	private void OnPointerUp(PointerUpEvent evt)
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		if (!isDragging)
		{
			return;
		}
		int[] slotList = ((IEnumerable<int>)m_detail.SlotList).Concat(new int[m_detail.SlotCountMax - m_detail.SlotList.Count]).ToArray();
		Rect worldBound = m_iconDelete.worldBound;
		Rect worldBound2 = dragClone.worldBound;
		if (((Rect)(ref worldBound)).Contains(((Rect)(ref worldBound2)).center))
		{
			int tempTabIndex = ((Focusable)draggingElement).tabIndex;
			zbUIMgr.Instance.ShowMessageBox(zbUtil.Language("lang.10143"), zbUtil.Language("lang.11066"), delegate(bool isOK)
			{
				if (isOK)
				{
					zbWsMsgHandler.Instance.Operate_RestaurantDelPiece_Req(m_detail.BaseInfo.UUID, slotList.ToArray(), tempTabIndex + 1);
				}
				else
				{
					m_spriteFoods[tempTabIndex].visible = true;
				}
			});
		}
		else
		{
			foreach (VisualElement item in m_slotList.Children())
			{
				if (!item.name.StartsWith("slot"))
				{
					continue;
				}
				if (UQueryExtensions.Q<VisualElement>(item, "foodInfo", (string)null) == originalSlot)
				{
					draggingElement.visible = true;
					continue;
				}
				worldBound = dragClone.worldBound;
				Vector2 center = ((Rect)(ref worldBound)).center;
				worldBound = item.worldBound;
				if (!((Rect)(ref worldBound)).Contains(center) || item == originalSlot)
				{
					continue;
				}
				int targetIndex = ((Focusable)item).tabIndex;
				int curIndex = ((Focusable)draggingElement).tabIndex;
				if (curIndex == targetIndex || m_detail.SlotCountMax <= targetIndex)
				{
					break;
				}
				if (slotList[targetIndex] == 0)
				{
					slotList[targetIndex] = slotList[curIndex];
					slotList[curIndex] = 0;
				}
				else if (slotList[targetIndex] == slotList[curIndex])
				{
					int mergeLevel = slotList[targetIndex] + 1;
					if (mergeLevel > m_targetNeedMaxLevel)
					{
						zbUIMgr.Instance.ShowMessageBox(zbUtil.Language("lang.10143"), zbUtil.Language("lang.11093"), delegate(bool isOK)
						{
							if (isOK)
							{
								slotList[targetIndex] = mergeLevel;
								slotList[curIndex] = 0;
								zbWsMsgHandler.Instance.Operate_RestaurantSlotSync_Req(m_detail.BaseInfo.UUID, slotList.ToArray());
							}
							else
							{
								m_spriteFoods[curIndex].visible = true;
							}
						});
						EndDrag(evt);
						return;
					}
					slotList[targetIndex] = mergeLevel;
					slotList[curIndex] = 0;
				}
				else
				{
					int num = slotList[targetIndex];
					slotList[targetIndex] = slotList[curIndex];
					slotList[curIndex] = num;
				}
				zbWsMsgHandler.Instance.Operate_RestaurantSlotSync_Req(m_detail.BaseInfo.UUID, slotList.ToArray());
				break;
			}
		}
		EndDrag(evt);
	}

	private void EndDrag(PointerUpEvent evt)
	{
		if (dragClone != null)
		{
			PointerCaptureHelper.ReleasePointer((IEventHandler)(object)dragClone, ((PointerEventBase<PointerUpEvent>)(object)evt).pointerId);
			dragClone.RemoveFromHierarchy();
			dragClone = null;
		}
		draggingElement = null;
		originalSlot = null;
		isDragging = false;
		((EventBase)evt).StopPropagation();
	}

	private void OnPointerMove(PointerMoveEvent evt)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		if (isDragging && dragClone != null)
		{
			Vector2 val = Vector2.op_Implicit(((PointerEventBase<PointerMoveEvent>)(object)evt).position);
			Vector2 val2 = VisualElementExtensions.WorldToLocal(m_slotList, val - dragOffset);
			dragClone.style.left = StyleLength.op_Implicit(val2.x);
			dragClone.style.top = StyleLength.op_Implicit(val2.y);
			((EventBase)evt).StopPropagation();
		}
	}

	private void OnPointerDown(PointerDownEvent evt, int index)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		isDragging = true;
		draggingElement = m_spriteFoods[index];
		originalSlot = draggingElement.parent;
		draggingElement.visible = false;
		dragClone = new VisualElement();
		dragClone.name = "dragClone";
		dragClone.style.width = StyleLength.op_Implicit(draggingElement.resolvedStyle.width);
		dragClone.style.height = StyleLength.op_Implicit(draggingElement.resolvedStyle.height);
		dragClone.style.backgroundImage = draggingElement.style.backgroundImage;
		dragClone.style.position = StyleEnum<Position>.op_Implicit((Position)1);
		dragClone.style.left = StyleLength.op_Implicit(0f);
		dragClone.style.top = StyleLength.op_Implicit(0f);
		dragClone.style.unityBackgroundImageTintColor = draggingElement.style.unityBackgroundImageTintColor;
		PointerCaptureHelper.CapturePointer((IEventHandler)(object)dragClone, ((PointerEventBase<PointerDownEvent>)(object)evt).pointerId);
		dragClone.BringToFront();
		Vector2 val = Vector2.op_Implicit(((PointerEventBase<PointerDownEvent>)(object)evt).position);
		Rect worldBound = draggingElement.worldBound;
		Vector2 position = ((Rect)(ref worldBound)).position;
		dragOffset = val - position;
		Vector2 val2 = VisualElementExtensions.WorldToLocal(m_slotList, position);
		dragClone.style.left = StyleLength.op_Implicit(val2.x);
		dragClone.style.top = StyleLength.op_Implicit(val2.y);
		m_slotList.Add(dragClone);
		((EventBase)evt).StopPropagation();
	}

	private void OnLocks(int index)
	{
		if (index >= m_detail.SlotCountMax)
		{
			int unlockLevelBySlotIndex = zbSelf.Instance.restaurantHandler.GetUnlockLevelBySlotIndex(index);
			zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.11067"), unlockLevelBySlotIndex));
		}
	}

	public void OnUpdateEventRestaurant(object obj)
	{
		RestaurantDetail restaurantByUUID = zbSelf.Instance.operateHandler.GetRestaurantByUUID(m_detail.BaseInfo.UUID);
		RefreshCustom(restaurantByUUID);
	}

	private void OnRestaurantTargetAck(object data)
	{
		if (data is OperateRestaurantTargetAck ack)
		{
			m_restaurantTarget.PlayOutAndInAnimation(ack);
		}
	}

	private void OnRestaurantOfferPieceAck(object data)
	{
		if (data is OperateRestaurantOfferPieceAck ack)
		{
			((MonoBehaviour)this).StartCoroutine(PlayOfferPiece(ack));
		}
	}

	private IEnumerator PlayOfferPiece(OperateRestaurantOfferPieceAck ack)
	{
		zbUIMgr.Instance.ShowModel();
		int offerSlotIndex = zbSelf.Instance.restaurantHandler.m_offerSlotIndex;
		int offerTargetIndex = zbSelf.Instance.restaurantHandler.m_offerTargetIndex;
		VisualElement val = m_spriteFoods[offerSlotIndex];
		VisualElement val2 = m_restaurantTarget.m_spriteTargets[offerTargetIndex];
		VisualElement parent = m_slotList.parent;
		val.visible = false;
		VisualElement flyIcon = new VisualElement();
		flyIcon.name = "OfferFly";
		flyIcon.style.position = StyleEnum<Position>.op_Implicit((Position)1);
		flyIcon.style.width = StyleLength.op_Implicit(val.resolvedStyle.width);
		flyIcon.style.height = StyleLength.op_Implicit(val.resolvedStyle.height);
		flyIcon.style.backgroundImage = val.style.backgroundImage;
		flyIcon.style.unityBackgroundImageTintColor = StyleColor.op_Implicit(val.resolvedStyle.unityBackgroundImageTintColor);
		flyIcon.pickingMode = (PickingMode)1;
		parent.Add(flyIcon);
		flyIcon.BringToFront();
		Rect worldBound = val.worldBound;
		Vector2 val3 = VisualElementExtensions.WorldToLocal(parent, ((Rect)(ref worldBound)).position);
		flyIcon.style.left = StyleLength.op_Implicit(val3.x);
		flyIcon.style.top = StyleLength.op_Implicit(val3.y);
		Rect worldBound2 = val2.worldBound;
		Vector2 val4 = VisualElementExtensions.WorldToLocal(parent, ((Rect)(ref worldBound2)).position);
		val4.x += (((Rect)(ref worldBound2)).width - ((Rect)(ref worldBound)).width) * 0.5f;
		val4.y += (((Rect)(ref worldBound2)).height - ((Rect)(ref worldBound)).height) * 0.5f;
		bool finish = false;
		flyIcon.experimental.animation.Scale(0.7f, 250).Ease((Func<float, float>)Easing.OutQuad);
		flyIcon.experimental.animation.Position(Vector2.op_Implicit(val4 - val3), 250).Ease((Func<float, float>)Easing.OutQuad).OnCompleted((Action)delegate
		{
			finish = true;
		});
		while (!finish)
		{
			yield return null;
		}
		flyIcon.RemoveFromHierarchy();
		zbServerData.sGameData.OperateInfo.RestaurantList.Clear();
		zbServerData.sGameData.OperateInfo.RestaurantList.AddRange((IEnumerable<RestaurantDetail>)ack.RestaurantList);
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Restaurant);
		zbUIMgr.Instance.HideModel();
		CheckTargetCanAutoGet();
	}

	private void CheckTargetCanAutoGet()
	{
		RestaurantDetail restaurantByUUID = zbSelf.Instance.operateHandler.GetRestaurantByUUID(m_detail.BaseInfo.UUID);
		if (restaurantByUUID != null && restaurantByUUID.TargetDetail?.State == ECommonState.Yes)
		{
			zbWsMsgHandler.Instance.Operate_RestaurantTarget_Req(restaurantByUUID.BaseInfo.UUID, ((IEnumerable<int>)restaurantByUUID.SlotList).ToArray());
		}
	}

	public void InitSlotList()
	{
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		RepeatedField<int> slotList = m_detail.SlotList;
		int slotCountMax = m_detail.SlotCountMax;
		for (int i = 0; i < 15; i++)
		{
			bool flag = i < slotCountMax;
			bool flag2 = i < slotList.Count && slotList[i] > 0;
			m_spriteFoods[i].visible = flag2;
			if (zbSelf.Instance.restaurantHandler.CanOffer(m_detail, i))
			{
				m_spriteSlotBgs[i].style.backgroundImage = new StyleBackground(m_OfferBg);
			}
			else
			{
				m_spriteSlotBgs[i].style.backgroundImage = new StyleBackground(flag ? m_UnlockBg : m_LockBg);
			}
			m_nodeFoods[i].visible = flag & flag2;
			m_spriteLocks[i].visible = !flag;
			((TextElement)m_lbLockLevels[i]).text = "Lv." + zbSelf.Instance.restaurantHandler.GetUnlockLevelBySlotIndex(i);
			if (flag & flag2)
			{
				int num = slotList[i];
				m_spriteFoods[i].style.backgroundImage = new StyleBackground(m_foodIcons[num - 1]);
				((TextElement)m_lbFoodLevels[i]).text = num.ToString();
			}
		}
	}

	private void InitSpecialTask()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		OperateTargetNextGrandDetail targetNextGrand = m_detail.TargetNextGrand;
		bool flag = targetNextGrand.CurrentIndex == targetNextGrand.GrandIndex;
		m_specialTask.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag ? 1 : 0));
		if (flag)
		{
			return;
		}
		for (int i = 0; i < 4; i++)
		{
			bool flag2 = i < targetNextGrand.GrandAwardList.Count;
			((VisualElement)m_specialTaskAwards[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag2));
			if (flag2)
			{
				zbItemData data = new zbItemData(targetNextGrand.GrandAwardList[i].Type, targetNextGrand.GrandAwardList[i].Tid, targetNextGrand.GrandAwardList[i].Count);
				m_specialTaskAwards[i].Refresh(data);
			}
		}
		VisualElement obj = UQueryExtensions.Q<VisualElement>(m_specialTask, "progress", (string)null);
		((TextElement)UQueryExtensions.Q<Label>(obj, "lb_progressVal", (string)null)).text = targetNextGrand.CurrentIndex + "/" + targetNextGrand.GrandIndex;
		UQueryExtensions.Q<VisualElement>(obj, "progressVal", (string)null).style.width = StyleLength.op_Implicit(new Length((float)targetNextGrand.CurrentIndex / (float)targetNextGrand.GrandIndex * 100f, (LengthUnit)1));
	}

	private void OnClose()
	{
		Hide();
	}

	private void OnBuy()
	{
		if (zbSelf.Instance.restaurantHandler.HasEmptySlot(m_detail) && zbSelf.Instance.restaurantHandler.HasEnoughCost(m_detail, m_cost))
		{
			zbWsMsgHandler.Instance.Operate_RestaurantBuyPiece_Req(m_detail.BaseInfo.UUID, ((IEnumerable<int>)m_detail.SlotList).ToArray(), 1);
		}
	}

	private void OnBuyBatch()
	{
		if (zbSelf.Instance.restaurantHandler.HasEmptySlot(m_detail) && zbSelf.Instance.restaurantHandler.HasEnoughCost(m_detail, m_cost))
		{
			int item = zbSelf.Instance.restaurantHandler.GetBuyBatchCostAndCount(m_detail).batchCount;
			zbWsMsgHandler.Instance.Operate_RestaurantBuyPiece_Req(m_detail.BaseInfo.UUID, ((IEnumerable<int>)m_detail.SlotList).ToArray(), item);
		}
	}

	private void OnCompose()
	{
		if (zbSelf.Instance.restaurantHandler.CanOfferAllTarget(m_detail, ((IEnumerable<int>)m_detail.SlotList).ToArray()))
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11384"));
			return;
		}
		int[] array = ((IEnumerable<int>)m_detail.SlotList).Concat(new int[m_detail.SlotCountMax - m_detail.SlotList.Count]).ToArray();
		while (true)
		{
			var (num, num2) = zbSelf.Instance.restaurantHandler.GetCanMergeFromAndToIndex(m_detail, array);
			if (num < 0)
			{
				break;
			}
			array[num2] = array[num] + 1;
			array[num] = 0;
		}
		zbWsMsgHandler.Instance.Operate_RestaurantSlotSync_Req(m_detail.BaseInfo.UUID, array);
	}

	private void OnToggle()
	{
		if (zbSelf.Instance.restaurantHandler.CanUseBatch(m_detail, needTip: true))
		{
			m_isBatchBuy = !m_isBatchBuy;
			RefreshBuyOrBatch();
		}
	}

	private void RefreshTime()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		if (m_detail != null)
		{
			long num = m_detail.BaseInfo.EndTm - zbSelf.Instance.timeHandler.GetServerTime();
			((TextElement)m_lbTime).text = zbUtil.FormatTimeHHMMSS(num);
			((VisualElement)m_lbTime).style.color = StyleColor.op_Implicit((num < 86400) ? Color.red : Color.white);
			if (m_detail.DailyAward.Count >= m_detail.DailyAward.Total)
			{
				((TextElement)m_lbDailyAwardTime).text = m_detail.DailyAward.Count + "/" + m_detail.DailyAward.Total;
				m_notifyDailyAward.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
			else if (m_detail.DailyAward.Count < m_detail.DailyAward.Total && m_detail.DailyAward.NextTm < zbSelf.Instance.timeHandler.GetServerTime())
			{
				((TextElement)m_lbDailyAwardTime).text = zbUtil.Language("lang.10282");
				m_notifyDailyAward.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			}
			else
			{
				((TextElement)m_lbDailyAwardTime).text = zbUtil.FormatTimeHHMMSS(m_detail.DailyAward.NextTm - zbSelf.Instance.timeHandler.GetServerTime());
				m_notifyDailyAward.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
			m_notifyThemeTask.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_themeTaskDetail == null || !zbSelf.Instance.operateHandler.HasThemeTaskNotify(m_themeTaskDetail.BaseInfo.UUID)) ? 1 : 0));
		}
	}

	protected override void UpdateAtShow()
	{
		base.UpdateAtShow();
		m_iconDelete.visible = isDragging;
		m_holidayEventView.Update();
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		RefreshTime();
		RefreshGuideFinger();
	}

	public void RefreshGuideFinger(bool forceRefresh = false)
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		if (!forceRefresh && (!(zbUIMgr.Instance.m_lastTouchTime > TouchTimeDuration) || zbUIMgr.Instance.IsShow(EWndId.GuideFinger) || zbGuide.Instance.IsGuiding() || !zbUIMgr.Instance.IsTopCascadWnd(EWndId.Restaurant)))
		{
			return;
		}
		VisualElement[] btnCanOffers = m_restaurantTarget.m_btnCanOffers;
		foreach (VisualElement val in btnCanOffers)
		{
			if (val.style.display == StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0))
			{
				zbUIMgr.Instance.ShowGuideFinger(val);
				return;
			}
		}
		if (((VisualElement)m_btnCompose).style.display == StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0) && !zbSelf.Instance.restaurantHandler.CanOfferAllTarget(m_detail, ((IEnumerable<int>)m_detail.SlotList).ToArray()))
		{
			zbUIMgr.Instance.ShowGuideFinger((VisualElement)(object)m_btnCompose);
			return;
		}
		var (num, num2) = zbSelf.Instance.restaurantHandler.GetCanMergeFromAndToIndex(m_detail, ((IEnumerable<int>)m_detail.SlotList).ToArray());
		if (num >= 0 && num2 >= 0)
		{
			zbUIMgr.Instance.ShowGuideFinger(m_slots[num], m_slots[num2]);
		}
		else if (zbSelf.Instance.restaurantHandler.HasEmptySlot(m_detail, needTip: false) && zbSelf.Instance.restaurantHandler.HasEnoughCost(m_detail, m_cost, needTip: false))
		{
			if (((VisualElement)m_btnBuy).style.display == StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0))
			{
				zbUIMgr.Instance.ShowGuideFinger((VisualElement)(object)m_btnBuy);
			}
			else if (((VisualElement)m_btnBuyBatch).style.display == StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0))
			{
				zbUIMgr.Instance.ShowGuideFinger((VisualElement)(object)m_btnBuyBatch);
			}
		}
	}

	protected override void OnHide()
	{
		base.OnHide();
		int newState = ((!m_isBatchBuy) ? 1 : 0);
		zbSelf.Instance.newTagHandler.SetRestaurantBatchBuy(m_detail.BaseInfo.UUID, newState);
	}
}
