using System;
using System.Collections.Generic;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbTrainRobWnd : zbBaseWnd
{
	private Button m_btnTouch;

	private VisualElement m_bg;

	private SpineVisualElement m_spineTrain;

	private VisualElement m_spriteQuality;

	private Label m_lbTime;

	private VisualElement[] m_playerInfos = (VisualElement[])(object)new VisualElement[4];

	private VisualElement[] m_spriteHeads = (VisualElement[])(object)new VisualElement[4];

	private VisualElement[] m_spriteFrames = (VisualElement[])(object)new VisualElement[4];

	private Label[] m_lbLevels = (Label[])(object)new Label[4];

	private MultiColumnListView m_listView;

	private List<zbRobItemData> m_listItemDatas = new List<zbRobItemData>();

	private List<int> rowIndexes = new List<int>();

	private int columnCount = 10;

	private bool columnsInited;

	private Button m_btnRob;

	private Button m_btnRefresh;

	private Label m_lbRefresh;

	private TrainDetail m_trainDetail;

	private readonly int m_refreshInterval = 10;

	private long m_nextRefreshTm;

	private bool isSelf;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_bg = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg", (string)null);
		m_spineTrain = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "spine_che", (string)null);
		m_spriteQuality = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_quality", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		for (int i = 0; i < 4; i++)
		{
			m_playerInfos[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "playerInfo" + i, (string)null);
			m_spriteHeads[i] = UQueryExtensions.Q<VisualElement>(m_playerInfos[i], "head", (string)null);
			m_spriteFrames[i] = UQueryExtensions.Q<VisualElement>(m_playerInfos[i], "frame", (string)null);
			m_lbLevels[i] = UQueryExtensions.Q<Label>(m_playerInfos[i], "lb_level", (string)null);
			Button visualElement = UQueryExtensions.Q<Button>(m_playerInfos[i], "btn_player", (string)null);
			int tempIndex = i;
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)visualElement, (Action<ClickEvent>)delegate(ClickEvent evt)
			{
				OnPlayers(evt, tempIndex);
			}, "", (TrickleDown)0);
		}
		m_listView = UQueryExtensions.Q<MultiColumnListView>(m_RootElement, "listView", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		m_btnRob = UQueryExtensions.Q<Button>(m_RootElement, "btn_rob", (string)null);
		m_btnRefresh = UQueryExtensions.Q<Button>(m_RootElement, "btn_refresh", (string)null);
		m_lbRefresh = UQueryExtensions.Q<Label>(m_RootElement, "lb_refresh", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRob, (Action)OnRob, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRefresh, (Action)OnRefresh, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_TrainRobMatchAck_Ack, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_CommFight_Ack, OnCommFightAck);
		AddDark();
		InitColumns();
	}

	private void InitColumns()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Expected O, but got Unknown
		if (columnsInited)
		{
			return;
		}
		((BaseVerticalCollectionView)m_listView).fixedItemHeight = 64f;
		for (int i = 0; i < columnCount; i++)
		{
			int columnIndex = i;
			m_listView.columns.Add(new Column
			{
				width = Length.op_Implicit(64f),
				makeCell = delegate
				{
					//IL_001c: Unknown result type (might be due to invalid IL or missing references)
					//IL_0021: Unknown result type (might be due to invalid IL or missing references)
					//IL_0036: Unknown result type (might be due to invalid IL or missing references)
					//IL_0040: Unknown result type (might be due to invalid IL or missing references)
					//IL_0045: Unknown result type (might be due to invalid IL or missing references)
					//IL_004a: Unknown result type (might be due to invalid IL or missing references)
					//IL_005b: Unknown result type (might be due to invalid IL or missing references)
					zbItem obj = new zbItem();
					obj.OnEnable(this);
					((VisualElement)obj).style.scale = StyleScale.op_Implicit(new Vector2(0.5f, 0.5f));
					((VisualElement)obj).style.translate = StyleTranslate.op_Implicit(new Translate(Length.op_Implicit(0f), Length.op_Implicit(-10f)));
					((VisualElement)obj).style.alignSelf = StyleEnum<Align>.op_Implicit((Align)2);
					return (VisualElement)(object)obj;
				},
				bindCell = delegate(VisualElement element, int row)
				{
					//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
					//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
					int num = row * columnCount + columnIndex;
					if (num < m_listItemDatas.Count)
					{
						zbItem obj = element as zbItem;
						obj?.Refresh(m_listItemDatas[num].itemData);
						obj?.AddBestPrizeTag(m_listItemDatas[num].isPrize);
						obj?.SetTruckLossItemVisible(m_listItemDatas[num].isRob);
						element.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
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

	public void RefreshCustom(TrainDetail trainDetail, bool _isSelf)
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		m_trainDetail = trainDetail;
		isSelf = _isSelf;
		zbTrainCsvRecord recordByKey = zbDataConfig.Instance.trainCsvTable.GetRecordByKey((int)m_trainDetail.Quality);
		m_spineTrain.AnimationState.SetAnimation(0, "train_" + recordByKey.skinId, true);
		m_bg.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/truck/truck_qualitybg" + (int)m_trainDetail.Quality + ".png"));
		m_spriteQuality.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/truck/truck_title" + (int)m_trainDetail.Quality + ".png"));
		((VisualElement)m_btnRefresh).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(isSelf ? 1 : 0));
		if (isSelf)
		{
			((TextElement)m_btnRob).text = zbUtil.Language("lang.10206");
		}
		else
		{
			((TextElement)m_btnRob).text = zbUtil.Language("掠夺");
		}
		RefreshCarriageList();
		RefreshAwardList();
		RefreshTime();
	}

	private void RefreshCarriageList()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 4; i++)
		{
			m_playerInfos[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		int num = 0;
		foreach (TrainCarriageDetail carriage in m_trainDetail.CarriageList)
		{
			if (carriage.RoleBaseInfo == null || carriage.RoleBaseInfo.RoleId == 0L)
			{
				m_playerInfos[num].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
			else
			{
				m_playerInfos[num].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				string playerHeadTex = zbResourceHelper.GetPlayerHeadTex(carriage.RoleBaseInfo.HeadId);
				m_spriteHeads[num].style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(playerHeadTex));
				string playerHeadFrameTex = zbResourceHelper.GetPlayerHeadFrameTex(carriage.RoleBaseInfo.HeadFrameId);
				m_spriteFrames[num].style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(playerHeadFrameTex));
				((TextElement)m_lbLevels[num]).text = carriage.RoleBaseInfo.Level.ToString();
			}
			num++;
		}
	}

	private void RefreshAwardList()
	{
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		m_listItemDatas.Clear();
		int num = 0;
		foreach (TrainCarriageDetail carriage in m_trainDetail.CarriageList)
		{
			int num2 = 0;
			foreach (AwardDetail award in carriage.AwardList)
			{
				zbRobItemData item = new zbRobItemData(num, num2 == 0, carriage.RobList.Contains(num2), new zbItemData(award.Type, award.Tid, award.Count));
				m_listItemDatas.Add(item);
				num2++;
				num++;
			}
		}
		int num3 = Mathf.CeilToInt((float)m_listItemDatas.Count / (float)columnCount);
		rowIndexes.Clear();
		for (int i = 0; i < num3; i++)
		{
			rowIndexes.Add(i);
		}
		((BaseVerticalCollectionView)m_listView).itemsSource = null;
		((BaseVerticalCollectionView)m_listView).itemsSource = rowIndexes;
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_listView, "unity-multi-column-view__header-container", (string)null).style.opacity = StyleFloat.op_Implicit(0f);
	}

	private void RefreshTime()
	{
		long num = m_nextRefreshTm - zbSelf.Instance.timeHandler.GetServerTime();
		if (num > 0)
		{
			((TextElement)m_lbRefresh).text = zbUtil.FormatTimeMMSS(num);
			((VisualElement)m_btnRefresh).SetEnabled(false);
		}
		else
		{
			((TextElement)m_lbRefresh).text = "";
			((VisualElement)m_btnRefresh).SetEnabled(true);
		}
		if (m_trainDetail == null)
		{
			return;
		}
		if (isSelf)
		{
			long num2 = m_trainDetail.StartTm - zbSelf.Instance.timeHandler.GetServerTime();
			if (num2 > 0)
			{
				((TextElement)m_lbTime).text = zbUtil.Language("lang.11368") + ": " + zbUtil.FormatTimeHHMMSS(num2);
				return;
			}
			num2 = m_trainDetail.ArriveTm - zbSelf.Instance.timeHandler.GetServerTime();
			if (num2 > 0)
			{
				((TextElement)m_lbTime).text = zbUtil.Language("lang.11122") + ": " + zbUtil.FormatTimeHHMMSS(num2);
			}
			else
			{
				((TextElement)m_lbTime).text = zbUtil.Language("lang.11136");
			}
		}
		else
		{
			long num3 = m_trainDetail.ArriveTm - zbSelf.Instance.timeHandler.GetServerTime();
			if (num3 > 0)
			{
				((TextElement)m_lbTime).text = zbUtil.Language("lang.11122") + ": " + zbUtil.FormatTimeHHMMSS(num3);
			}
			else
			{
				((TextElement)m_lbTime).text = zbUtil.Language("lang.11128");
			}
		}
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		RefreshTime();
	}

	public void OnPlayers(ClickEvent evt, int index)
	{
		int num = 0;
		foreach (TrainCarriageDetail carriage in m_trainDetail.CarriageList)
		{
			if (num == index)
			{
				zbWsMsgHandler.Instance.Role_TipsInfo_Req((int)carriage.RoleBaseInfo.RoleId);
				break;
			}
			num++;
		}
	}

	private void OnRob()
	{
		if (isSelf)
		{
			OnClose();
		}
		else
		{
			zbWsMsgHandler.Instance.Train_RobFightStart_Req(m_trainDetail.TransportId);
		}
	}

	private void OnRefresh()
	{
		if (m_nextRefreshTm <= zbSelf.Instance.timeHandler.GetServerTime())
		{
			m_nextRefreshTm = zbSelf.Instance.timeHandler.GetServerTime() + m_refreshInterval;
			zbWsMsgHandler.Instance.Train_RobMatch_Req();
		}
	}

	private void OnClose()
	{
		Hide();
	}

	private void OnEventUpdate(object data)
	{
		if (!isSelf)
		{
			RefreshCustom(zbSelf.Instance.truckHandler.currRobTrain, isSelf);
		}
		else
		{
			RefreshCustom(zbSelf.Instance.truckHandler.trainInfo.SelfTrain, isSelf);
		}
	}

	private void OnCommFightAck(object obj)
	{
		OnClose();
	}
}
