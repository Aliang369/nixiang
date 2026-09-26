using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbTruckMainTabRob
{
	public zbTruckMainWnd m_wndParent;

	private VisualElement m_rootElement;

	private zbTruckMainTabRobItem[] m_items = new zbTruckMainTabRobItem[12];

	public VisualElement[] m_goldHead = (VisualElement[])(object)new VisualElement[12];

	private static int[] pool = new int[12];

	private List<int> result = new List<int>();

	private Button m_emptyTrain;

	private VisualElement m_trainDetal;

	private VisualElement m_trainTimeBg;

	private Label m_trainTitle;

	private Label m_trainTime;

	private SpineVisualElement m_spineTrain;

	private Button[] m_fires = (Button[])(object)new Button[4];

	private SpineVisualElement[] m_spineFires = (SpineVisualElement[])(object)new SpineVisualElement[4];

	private VisualElement[] m_playerInfos = (VisualElement[])(object)new VisualElement[4];

	protected static readonly int[,] numbers = new int[12, 2]
	{
		{ 90, 684 },
		{ 90, 970 },
		{ 90, 1238 },
		{ 190, 743 },
		{ 190, 1000 },
		{ 190, 1291 },
		{ 320, 650 },
		{ 320, 940 },
		{ 320, 1200 },
		{ 420, 715 },
		{ 420, 973 },
		{ 420, 1251 }
	};

	private readonly int m_refreshInterval = 60;

	private long m_nextRefreshTm;

	public void OnEnable(zbTruckMainWnd wndParent, VisualElement root)
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		m_rootElement = root;
		InitPool();
		for (int i = 0; i < 12; i++)
		{
			m_goldHead[i] = UQueryExtensions.Q<VisualElement>(wndParent.GetRootElement(), "goldHead_" + (i + 1), (string)null);
			m_items[i] = new zbTruckMainTabRobItem();
			m_items[i].OnEnable(this, UQueryExtensions.Q<VisualElement>(m_rootElement, "che_" + (i + 1), (string)null), i + 1, m_goldHead[i]);
			m_items[i].m_rootElement.style.left = StyleLength.op_Implicit((float)numbers[i, 0]);
			m_items[i].m_rootElement.style.bottom = StyleLength.op_Implicit((float)numbers[i, 1]);
			m_goldHead[i].style.left = StyleLength.op_Implicit((float)numbers[i, 0]);
			m_goldHead[i].style.bottom = StyleLength.op_Implicit((float)numbers[i, 1]);
		}
		m_trainTimeBg = UQueryExtensions.Q<VisualElement>(m_rootElement, "time_Bg", (string)null);
		m_trainTitle = UQueryExtensions.Q<Label>(m_rootElement, "lb_jiJiangChuXian", (string)null);
		m_trainTime = UQueryExtensions.Q<Label>(m_rootElement, "lb_trainTime", (string)null);
		m_emptyTrain = UQueryExtensions.Q<Button>(m_rootElement, "empty_train", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_emptyTrain, (Action)OnEmptyTrain, "", (TrickleDown)0);
		m_spineTrain = UQueryExtensions.Q<SpineVisualElement>(m_rootElement, "spine_train", (string)null);
		m_trainDetal = UQueryExtensions.Q<VisualElement>(m_rootElement, "train", (string)null);
		for (int j = 0; j < 4; j++)
		{
			VisualElement val = UQueryExtensions.Q<VisualElement>(m_trainDetal, "playerInfo" + j, (string)null);
			m_fires[j] = UQueryExtensions.Q<Button>(val, "fire", (string)null);
			m_spineFires[j] = UQueryExtensions.Q<SpineVisualElement>((VisualElement)(object)m_fires[j], "spine_fire1", (string)null);
			m_playerInfos[j] = UQueryExtensions.Q<VisualElement>(val, "playerInfo", (string)null);
			((VisualElement)UQueryExtensions.Q<Label>(m_playerInfos[j], "lb_level", (string)null)).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		Button visualElement = UQueryExtensions.Q<Button>(m_trainDetal, "btn_player", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)visualElement, (Action<ClickEvent>)delegate
		{
			onBtnPlayer();
		}, "", (TrickleDown)0);
	}

	private static void InitPool()
	{
		for (int i = 0; i < 12; i++)
		{
			pool[i] = i;
		}
	}

	public void Refresh()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		if (zbSelf.Instance.truckHandler.MatchList != null)
		{
			int num = Math.Min(zbSelf.Instance.truckHandler.MatchList.Count(), 12);
			for (int i = 0; i < 12; i++)
			{
				m_items[i].m_rootElement.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				m_goldHead[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
			if (!m_wndParent.b_btnTab || result.Count() <= 0)
			{
				result = Rand.GetRandomItems(pool, num);
			}
			for (int j = 0; j < num; j++)
			{
				m_items[result[j]].Refresh(zbSelf.Instance.truckHandler.MatchList[j]);
				m_items[result[j]].m_rootElement.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			}
			RefreshTrain();
		}
	}

	public void RefreshTrain()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0353: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0418: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d8: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_emptyTrain).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_trainDetal.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		if (zbSelf.Instance.truckHandler.trainInfo == null)
		{
			return;
		}
		if (zbSelf.Instance.truckHandler.trainInfo.MatchInfo.NextTm > zbSelf.Instance.timeHandler.GetServerTime() || zbSelf.Instance.truckHandler.currRobTrain == null || zbSelf.Instance.truckHandler.currRobTrain.TransportId == 0)
		{
			((VisualElement)m_emptyTrain).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			UpdatePerSecond();
		}
		else
		{
			m_trainDetal.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_spineTrain.AnimationState.SetAnimation(0, "track1_che_" + 1, true);
			m_spineTrain.AnimationState.SetAnimation(1, "track2_idle", true);
		}
		if (zbSelf.Instance.truckHandler.currRobTrain == null || zbSelf.Instance.truckHandler.currRobTrain.TransportId == 0)
		{
			((VisualElement)m_emptyTrain).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			return;
		}
		m_trainDetal.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		TrainDetail currRobTrain = zbSelf.Instance.truckHandler.currRobTrain;
		zbTrainCsvRecord recordByKey = zbDataConfig.Instance.trainCsvTable.GetRecordByKey((int)currRobTrain.Quality);
		m_spineTrain.AnimationState.SetAnimation(0, "track1_che_" + recordByKey.skinId, true);
		if (currRobTrain.StartTm > zbSelf.Instance.timeHandler.GetServerTime())
		{
			m_spineTrain.AnimationState.SetAnimation(1, "track2_idle", true);
			((TextElement)m_trainTime).text = zbUtil.FormatTimeHHMMSS(currRobTrain.StartTm - zbSelf.Instance.timeHandler.GetServerTime());
		}
		else if (currRobTrain.StartTm < zbSelf.Instance.timeHandler.GetServerTime() && currRobTrain.ArriveTm > zbSelf.Instance.timeHandler.GetServerTime())
		{
			m_spineTrain.AnimationState.SetAnimation(1, "track2_move", true);
			((TextElement)m_trainTime).text = zbUtil.FormatTimeHHMMSS(currRobTrain.ArriveTm - zbSelf.Instance.timeHandler.GetServerTime());
		}
		else
		{
			m_spineTrain.AnimationState.SetAnimation(1, "track2_rev", true);
		}
		int num = 0;
		foreach (TrainCarriageDetail carriage in currRobTrain.CarriageList)
		{
			if (carriage.RoleBaseInfo == null || carriage.RoleBaseInfo.RoleId == 0L)
			{
				m_playerInfos[num].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				((VisualElement)m_fires[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
			else
			{
				m_playerInfos[num].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				string icon = zbDataConfig.Instance.playerHeadCsvTable.GetRecordByKey(carriage.RoleBaseInfo.HeadId).icon;
				Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(icon);
				UQueryExtensions.Q<VisualElement>(m_playerInfos[num], "header", (string)null).style.backgroundImage = new StyleBackground(val);
				string icon2 = zbDataConfig.Instance.playerHeadFrameCsvTable.GetRecordByKey(carriage.RoleBaseInfo.HeadFrameId).icon;
				Sprite val2 = zbAssetLoader.Instance.LoadResouce<Sprite>(icon2);
				UQueryExtensions.Q<VisualElement>(m_playerInfos[num], "frame", (string)null).style.backgroundImage = new StyleBackground(val2);
				int num2 = Mathf.Min(carriage.RobList.Count, 3);
				if (num2 > 0)
				{
					((VisualElement)m_fires[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
					m_spineFires[num].AnimationState.SetAnimation(1, "fire" + num2, true);
				}
				else
				{
					((VisualElement)m_fires[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				}
			}
			num++;
		}
	}

	public void UpdatePerSecond()
	{
		if (zbSelf.Instance.truckHandler.trainInfo == null)
		{
			return;
		}
		TrainMatchInfo matchInfo = zbSelf.Instance.truckHandler.trainInfo.MatchInfo;
		if (matchInfo.NextTm > zbSelf.Instance.timeHandler.GetServerTime())
		{
			zbUtil.SetDisplayIfChanged(m_trainTimeBg, (DisplayStyle)0);
			zbUtil.SetDisplayIfChanged((VisualElement)(object)m_trainTime, (DisplayStyle)0);
			((TextElement)m_trainTime).text = zbUtil.FormatTimeHHMMSS(matchInfo.NextTm - zbSelf.Instance.timeHandler.GetServerTime());
			return;
		}
		zbUtil.SetDisplayIfChanged(m_trainTimeBg, (DisplayStyle)1);
		zbUtil.SetDisplayIfChanged((VisualElement)(object)m_trainTime, (DisplayStyle)1);
		if ((zbSelf.Instance.truckHandler.currRobTrain == null || zbSelf.Instance.truckHandler.currRobTrain.TransportId == 0) && m_nextRefreshTm <= zbSelf.Instance.timeHandler.GetServerTime())
		{
			m_nextRefreshTm = zbSelf.Instance.timeHandler.GetServerTime() + m_refreshInterval;
			zbWsMsgHandler.Instance.Train_RobMatch_Req();
		}
	}

	public void onBtnPlayer()
	{
		zbUIMgr.Instance.ShowTrainRob(zbSelf.Instance.truckHandler.currRobTrain, isSelf: false);
	}

	public void OnEmptyTrain()
	{
		zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11373"));
	}
}
