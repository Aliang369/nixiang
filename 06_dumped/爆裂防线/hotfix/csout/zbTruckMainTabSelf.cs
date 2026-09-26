using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbTruckMainTabSelf
{
	public zbTruckMainWnd m_wndParent;

	private VisualElement m_rootElement;

	private zbTruckMainTabSelfItem[] m_items = new zbTruckMainTabSelfItem[3];

	private VisualElement m_notifyAdd;

	private Button m_emptyTrain;

	private VisualElement m_trainAdd;

	private VisualElement m_trainDetal;

	private VisualElement m_trainLable;

	private Button m_btnFinish;

	private Label m_trainTitle;

	private Label m_trainTime;

	private SpineVisualElement m_spineTrain;

	private Button[] m_fires = (Button[])(object)new Button[4];

	private SpineVisualElement[] m_spineFires = (SpineVisualElement[])(object)new SpineVisualElement[4];

	private Button[] m_players = (Button[])(object)new Button[4];

	private VisualElement[] m_playerBgs = (VisualElement[])(object)new VisualElement[4];

	private VisualElement[] m_playerInfos = (VisualElement[])(object)new VisualElement[4];

	private float _lastTop = -1f;

	private bool lastBool;

	public void OnEnable(zbTruckMainWnd wndParent, VisualElement root)
	{
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		m_rootElement = root;
		for (int i = 0; i < 3; i++)
		{
			m_items[i] = new zbTruckMainTabSelfItem();
			m_items[i].OnEnable(this, UQueryExtensions.Q<VisualElement>(m_rootElement, "self_" + (i + 1), (string)null), i + 1);
		}
		m_emptyTrain = UQueryExtensions.Q<Button>(m_rootElement, "empty_train", (string)null);
		m_trainAdd = UQueryExtensions.Q<VisualElement>(m_rootElement, "add", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_emptyTrain, (Action)OnEmptyTrain, "", (TrickleDown)0);
		m_btnFinish = UQueryExtensions.Q<Button>(m_rootElement, "btn_finish", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnFinish, (Action)OnFinish, "", (TrickleDown)0);
		m_trainLable = UQueryExtensions.Q<VisualElement>(m_rootElement, "train_lable", (string)null);
		m_trainTitle = UQueryExtensions.Q<Label>(m_trainLable, "lb_trainTitle", (string)null);
		m_trainTime = UQueryExtensions.Q<Label>(m_trainLable, "lb_trainTIme", (string)null);
		m_spineTrain = UQueryExtensions.Q<SpineVisualElement>(m_rootElement, "spine_train", (string)null);
		m_trainDetal = UQueryExtensions.Q<VisualElement>(m_rootElement, "train", (string)null);
		for (int j = 0; j < 4; j++)
		{
			VisualElement val = UQueryExtensions.Q<VisualElement>(m_trainDetal, "playerInfo" + j, (string)null);
			m_fires[j] = UQueryExtensions.Q<Button>(val, "fire", (string)null);
			m_spineFires[j] = UQueryExtensions.Q<SpineVisualElement>((VisualElement)(object)m_fires[j], "spine_fire1", (string)null);
			m_playerBgs[j] = UQueryExtensions.Q<VisualElement>(val, "playerBg", (string)null);
			m_players[j] = UQueryExtensions.Q<Button>(val, "btn_player", (string)null);
			m_playerInfos[j] = UQueryExtensions.Q<VisualElement>(val, "playerInfo", (string)null);
			((VisualElement)UQueryExtensions.Q<Label>(m_playerInfos[j], "lb_level", (string)null)).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			Button visualElement = UQueryExtensions.Q<Button>(val, "btn_player", (string)null);
			int index = j;
			m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)visualElement, (Action<ClickEvent>)delegate
			{
				onBtnPlayer(index);
			}, "", (TrickleDown)0);
		}
		m_notifyAdd = zbUtil.CreateNotifyTag(m_trainAdd);
	}

	public void Refresh()
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0316: Unknown result type (might be due to invalid IL or missing references)
		//IL_032e: Unknown result type (might be due to invalid IL or missing references)
		//IL_034b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0363: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_040b: Unknown result type (might be due to invalid IL or missing references)
		//IL_047c: Unknown result type (might be due to invalid IL or missing references)
		//IL_043c: Unknown result type (might be due to invalid IL or missing references)
		if (zbServerData.sGameData.TruckSelfInfo == null || zbServerData.sGameData.TruckSelfInfo.TruckList == null)
		{
			return;
		}
		((MonoBehaviour)m_wndParent).StopAllCoroutines();
		for (int i = 0; i < 3; i++)
		{
			m_items[i].Refresh();
		}
		zbUtil.SetStyleIfChanged(ref _lastTop, 90f, delegate(float num3)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			m_trainDetal.style.top = StyleLength.op_Implicit(num3);
		});
		((VisualElement)m_emptyTrain).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_trainDetal.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		((VisualElement)m_btnFinish).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_trainLable.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		if (zbSelf.Instance.truckHandler.trainInfo == null)
		{
			return;
		}
		if (zbSelf.Instance.truckHandler.trainInfo.SelfTrain.TransportId == 0)
		{
			((VisualElement)m_emptyTrain).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		else
		{
			m_trainDetal.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			TrainDetail selfTrain = zbSelf.Instance.truckHandler.trainInfo.SelfTrain;
			zbTrainCsvRecord recordByKey = zbDataConfig.Instance.trainCsvTable.GetRecordByKey((int)selfTrain.Quality);
			m_spineTrain.AnimationState.SetAnimation(0, "track1_che_" + recordByKey.skinId, true);
			if (selfTrain.StartTm > zbSelf.Instance.timeHandler.GetServerTime())
			{
				m_spineTrain.AnimationState.SetAnimation(1, "track2_idle", true);
				m_trainLable.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				((TextElement)m_trainTitle).text = zbUtil.Language("lang.11378");
				((TextElement)m_trainTime).text = zbUtil.FormatTimeHHMMSS(selfTrain.StartTm - zbSelf.Instance.timeHandler.GetServerTime());
			}
			else if (selfTrain.StartTm < zbSelf.Instance.timeHandler.GetServerTime() && selfTrain.ArriveTm > zbSelf.Instance.timeHandler.GetServerTime())
			{
				m_spineTrain.AnimationState.SetAnimation(1, "track2_move", true);
				m_trainLable.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				((TextElement)m_trainTitle).text = zbUtil.Language("lang.11369");
				((TextElement)m_trainTime).text = zbUtil.FormatTimeHHMMSS(selfTrain.ArriveTm - zbSelf.Instance.timeHandler.GetServerTime());
			}
			else
			{
				m_spineTrain.AnimationState.SetAnimation(1, "track2_rev", true);
				((VisualElement)m_btnFinish).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			}
			int num = 0;
			foreach (TrainCarriageDetail carriage in selfTrain.CarriageList)
			{
				if (carriage.RoleBaseInfo == null || carriage.RoleBaseInfo.RoleId == 0L)
				{
					m_playerInfos[num].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
					((VisualElement)m_fires[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
					m_playerBgs[num].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				}
				else
				{
					m_playerInfos[num].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
					m_playerBgs[num].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
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
		m_notifyAdd.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((zbSelf.Instance.truckHandler.trainInfo.SelfTrain.TransportId != 0 || !zbSelf.Instance.moneyHandler.HasEnoughMoney(38, 1L)) ? 1 : 0));
	}

	public void UpdatePerSecond()
	{
		for (int i = 0; i < 3; i++)
		{
			m_items[i].UpdatePerSecond();
		}
		if (zbSelf.Instance.truckHandler.trainInfo == null)
		{
			return;
		}
		TrainDetail selfTrain = zbSelf.Instance.truckHandler.trainInfo.SelfTrain;
		if (selfTrain.StartTm >= zbSelf.Instance.timeHandler.GetServerTime())
		{
			((TextElement)m_trainTitle).text = zbUtil.Language("lang.11378");
			((TextElement)m_trainTime).text = zbUtil.FormatTimeHHMMSS(selfTrain.StartTm - zbSelf.Instance.timeHandler.GetServerTime());
		}
		else if (selfTrain.StartTm < zbSelf.Instance.timeHandler.GetServerTime() && selfTrain.ArriveTm > zbSelf.Instance.timeHandler.GetServerTime())
		{
			((TextElement)m_trainTitle).text = zbUtil.Language("lang.11369");
			((TextElement)m_trainTime).text = zbUtil.FormatTimeHHMMSS(selfTrain.ArriveTm - zbSelf.Instance.timeHandler.GetServerTime());
		}
		if (selfTrain.TransportId > 0)
		{
			bool flag = selfTrain.ArriveTm >= zbSelf.Instance.timeHandler.GetServerTime();
			zbUtil.SetDisplayIfChanged(m_trainLable, (DisplayStyle)(!flag));
			zbUtil.SetDisplayIfChanged((VisualElement)(object)m_btnFinish, (DisplayStyle)(flag ? 1 : 0));
			if (lastBool != flag)
			{
				Refresh();
			}
			lastBool = flag;
		}
	}

	public void OnEmptyTrain()
	{
		zbUIMgr.Instance.ShowTrainSelectTip();
	}

	public void onBtnPlayer(int index)
	{
		zbUIMgr.Instance.ShowTrainRob(zbSelf.Instance.truckHandler.trainInfo.SelfTrain, isSelf: true);
	}

	public void OnFinish()
	{
		zbWsMsgHandler.Instance.Train_TransportAward_Req(zbSelf.Instance.truckHandler.trainInfo.SelfTrain.TransportId);
	}

	public void TrainTransportAward(object data)
	{
		((MonoBehaviour)m_wndParent).StartCoroutine(RefreshNewTrain(data));
	}

	public IEnumerator RefreshNewTrain(object data)
	{
		zbAudioMgr.Instance.PlayAudio("audio/combat/motuo_2.mp3");
		((VisualElement)m_emptyTrain).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		((VisualElement)m_btnFinish).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_spineTrain.AnimationState.SetAnimation(1, "track2_move", true);
		do
		{
			StyleLength top = m_trainDetal.style.top;
			Length value = ((StyleLength)(ref top)).value;
			float value2 = ((Length)(ref value)).value;
			zbUtil.SetStyleIfChanged(ref _lastTop, value2 - 15f, delegate(float val)
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				m_trainDetal.style.top = StyleLength.op_Implicit(val);
			});
			yield return null;
		}
		while (!(_lastTop <= -900f));
		if (data is TrainTransportAwardAck trainTransportAwardAck)
		{
			zbSelf.Instance.dataHandler.DataChange(trainTransportAwardAck.DataChange, ((IEnumerable<AwardDetail>)trainTransportAwardAck.AwardList).ToArray());
		}
		Refresh();
	}
}
