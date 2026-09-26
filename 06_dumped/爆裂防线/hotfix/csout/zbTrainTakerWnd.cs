using System;
using System.Collections;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbTrainTakerWnd : zbBaseWnd
{
	private VisualElement m_bottom;

	private Button m_btnBack;

	private VisualElement ve_train;

	private SpineVisualElement m_spineTrain;

	private VisualElement[] m_goldHeads = (VisualElement[])(object)new VisualElement[4];

	private VisualElement[] m_playerInfos = (VisualElement[])(object)new VisualElement[4];

	private VisualElement[] m_bubbles = (VisualElement[])(object)new VisualElement[4];

	private Button[] m_btnPlayerBgs = (Button[])(object)new Button[4];

	private Label[] m_lbYous = (Label[])(object)new Label[4];

	private Label m_lbTruckWait;

	private Label m_lbTruckTime;

	private zbItem[] m_item1s = new zbItem[6];

	private zbItem[] m_item2s = new zbItem[6];

	private zbItem[] m_item3s = new zbItem[6];

	private int selectIndex = -1;

	private Button m_btnRefresh;

	private Label m_lbRefreshFree;

	private Label m_lbRefreshIcon;

	private Label m_lbRefreshCost;

	private VisualElement ve_OK;

	private VisualElement[] m_spTrainCosts = (VisualElement[])(object)new VisualElement[2];

	private Label[] m_lbTrainCosts = (Label[])(object)new Label[2];

	private Button m_btnCancel;

	private Button m_btnConfirm;

	private VisualElement ve_takeTip;

	private VisualElement ve_noTip;

	private Button btn_driver;

	private int[][] refreshDiamond;

	private int moveStep;

	private float _lastTop = -1f;

	private Button m_btnRule;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_bottom = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_bottom", (string)null);
		ve_train = UQueryExtensions.Q<VisualElement>(m_RootElement, "train", (string)null);
		m_spineTrain = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "spine_train", (string)null);
		m_lbTruckTime = UQueryExtensions.Q<Label>(UQueryExtensions.Q<VisualElement>(m_RootElement, "goldHead_0", (string)null), "lb_time", (string)null);
		m_lbTruckWait = UQueryExtensions.Q<Label>(UQueryExtensions.Q<VisualElement>(m_RootElement, "goldHead_0", (string)null), "lb_wait", (string)null);
		m_btnBack = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBack, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_btnRefresh = UQueryExtensions.Q<Button>(m_RootElement, "btn_refresh", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRefresh, (Action<ClickEvent>)OnRefresh, "", (TrickleDown)0);
		m_lbRefreshFree = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnRefresh, "lb_free", (string)null);
		m_lbRefreshIcon = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnRefresh, "lb_diamond", (string)null);
		m_lbRefreshCost = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnRefresh, "lb_cost0", (string)null);
		JObject record = zbDataConfig.Instance.truckBaseJsonTable.GetRecord();
		refreshDiamond = record["train"][(object)"refreshCostList"].ToObject<int[][]>();
		((TextElement)m_lbRefreshCost).text = refreshDiamond[0][2].ToString();
		ve_OK = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_Confirm", (string)null);
		m_btnCancel = UQueryExtensions.Q<Button>(ve_OK, "btn_cancel", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCancel, (Action<ClickEvent>)OnCancel, "", (TrickleDown)0);
		m_btnConfirm = UQueryExtensions.Q<Button>(ve_OK, "btn_OK", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnConfirm, (Action<ClickEvent>)OnConfirm, "", (TrickleDown)0);
		m_btnRule = UQueryExtensions.Q<Button>(m_RootElement, "btn_rule", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRule, (Action)OnRule, "", (TrickleDown)0);
		ve_takeTip = UQueryExtensions.Q<VisualElement>(m_RootElement, "panel0", (string)null);
		ve_noTip = UQueryExtensions.Q<VisualElement>(m_RootElement, "panel1", (string)null);
		btn_driver = UQueryExtensions.Q<Button>(ve_noTip, "btn_driver", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)btn_driver, (Action<ClickEvent>)OnDriver, "", (TrickleDown)0);
		for (int i = 0; i < 1; i++)
		{
			m_spTrainCosts[i] = UQueryExtensions.Q<VisualElement>(ve_OK, "lb_cost" + i, (string)null);
			m_lbTrainCosts[i] = UQueryExtensions.Q<Label>(ve_OK, "lb_diamond" + i, (string)null);
		}
		for (int j = 0; j < 4; j++)
		{
			int index = j;
			m_goldHeads[j] = UQueryExtensions.Q<VisualElement>(m_RootElement, "goldHead_" + index, (string)null);
			m_playerInfos[j] = (VisualElement)(object)UQueryExtensions.Q<Button>(m_goldHeads[j], "playerInfo", (string)null);
			m_bubbles[j] = UQueryExtensions.Q<VisualElement>(m_goldHeads[j], "bubble", (string)null);
			m_btnPlayerBgs[j] = UQueryExtensions.Q<Button>(m_goldHeads[j], "playerBg", (string)null);
			m_lbYous[j] = UQueryExtensions.Q<Label>(m_goldHeads[j], "lb_you", (string)null);
			int tempIndex = j;
			m_uiEvent.RegisterCallback<ClickEvent>(m_playerInfos[j], (Action<ClickEvent>)delegate(ClickEvent evt)
			{
				OnPlayers(evt, tempIndex);
			}, "", (TrickleDown)0);
			if (m_btnPlayerBgs[j] != null)
			{
				m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnPlayerBgs[j], (Action<ClickEvent>)delegate
				{
					onPlayerBg(index);
				}, "", (TrickleDown)0);
			}
			if (j <= 0)
			{
				continue;
			}
			for (int num = 0; num < 6; num++)
			{
				switch (j)
				{
				case 1:
					m_item1s[num] = UQueryExtensions.Q<zbItem>(m_bubbles[j], $"item{num}", (string)null);
					m_item1s[num].OnEnable(this);
					break;
				case 2:
					m_item2s[num] = UQueryExtensions.Q<zbItem>(m_bubbles[j], $"item{num}", (string)null);
					m_item2s[num].OnEnable(this);
					break;
				case 3:
					m_item3s[num] = UQueryExtensions.Q<zbItem>(m_bubbles[j], $"item{num}", (string)null);
					m_item3s[num].OnEnable(this);
					break;
				}
			}
		}
		CreateMoneyPanel(new EMoneyType[2]
		{
			EMoneyType.Diamond,
			EMoneyType.TrainTicket
		});
		m_gameEevent.RegisterEvent(EGameEvent.Event_TrainTransportTakeRefresh_Ack, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_TrainTransportOpen_Ack, TrainTransportOpen);
	}

	public override void Refresh()
	{
		base.Refresh();
		selectIndex = -1;
	}

	private void OnEventUpdate(object data)
	{
		selectIndex = -1;
		((MonoBehaviour)this).StartCoroutine(RefreshNewTrain());
	}

	private void TrainTransportOpen(object data)
	{
		if (!IsShow())
		{
			return;
		}
		TrainInfo trainInfo = zbSelf.Instance.truckHandler.trainInfo;
		if (trainInfo == null)
		{
			return;
		}
		if (trainInfo.SelfTrain.StartTm <= 0)
		{
			if (trainInfo.WaitTrain.StartTm <= 0)
			{
				zbUIMgr.Instance.ShowMessageConfirm(zbUtil.Language("lang.11385"), delegate
				{
					zbWsMsgHandler.Instance.Train_TransportTake_Req();
				});
			}
			else if (trainInfo.WaitTrain.StartTm > zbSelf.Instance.timeHandler.GetServerTime())
			{
				zbUIMgr.Instance.ShowMessageConfirm(zbUtil.Language("lang.11386"), delegate
				{
					zbWsMsgHandler.Instance.Train_TransportTake_Req(openTaker: true);
				});
			}
		}
		else
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11372"));
			OnClose();
		}
	}

	public void RefreshCustom()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_039f: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0430: Unknown result type (might be due to invalid IL or missing references)
		//IL_0487: Unknown result type (might be due to invalid IL or missing references)
		//IL_049d: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		moveStep = 0;
		TrainDetail waitTrain = zbSelf.Instance.truckHandler.trainInfo.WaitTrain;
		if (waitTrain.StartTm <= 0)
		{
			ve_takeTip.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			ve_noTip.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			return;
		}
		for (int i = 0; i < 4; i++)
		{
			m_goldHeads[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		m_bottom.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		zbUtil.SetStyleIfChanged(ref _lastTop, 267f, delegate(float num3)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			ve_train.style.top = StyleLength.op_Implicit(num3);
		});
		ve_takeTip.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		ve_noTip.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		zbTrainCsvRecord recordByKey = zbDataConfig.Instance.trainCsvTable.GetRecordByKey((int)waitTrain.Quality);
		m_spineTrain.AnimationState.SetAnimation(0, "track1_che_" + recordByKey.skinId, true);
		m_spineTrain.AnimationState.SetAnimation(1, "track2_idle", true);
		int num = 0;
		foreach (TrainCarriageDetail carriage in waitTrain.CarriageList)
		{
			((VisualElement)m_lbYous[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			if (carriage.RoleBaseInfo == null || carriage.RoleBaseInfo.RoleId == 0L)
			{
				m_playerInfos[num].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				if (m_btnPlayerBgs[num] != null)
				{
					((VisualElement)m_btnPlayerBgs[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				}
			}
			else
			{
				m_playerInfos[num].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				if (m_btnPlayerBgs[num] != null)
				{
					((VisualElement)m_btnPlayerBgs[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				}
				string icon = zbDataConfig.Instance.playerHeadCsvTable.GetRecordByKey(carriage.RoleBaseInfo.HeadId).icon;
				Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(icon);
				UQueryExtensions.Q<VisualElement>(m_playerInfos[num], "header", (string)null).style.backgroundImage = new StyleBackground(val);
				string icon2 = zbDataConfig.Instance.playerHeadFrameCsvTable.GetRecordByKey(carriage.RoleBaseInfo.HeadFrameId).icon;
				Sprite val2 = zbAssetLoader.Instance.LoadResouce<Sprite>(icon2);
				UQueryExtensions.Q<VisualElement>(m_playerInfos[num], "frame", (string)null).style.backgroundImage = new StyleBackground(val2);
				((TextElement)UQueryExtensions.Q<Label>(m_playerInfos[num], "lb_level", (string)null)).text = carriage.RoleBaseInfo.Level.ToString();
			}
			if (num > 0)
			{
				zbItem[] array = num switch
				{
					1 => m_item1s, 
					2 => m_item2s, 
					_ => m_item3s, 
				};
				int num2 = 0;
				foreach (AwardDetail award in carriage.AwardList)
				{
					zbItemData data = new zbItemData(award.Type, award.Tid, award.Count);
					array[num2].Refresh(data);
					num2++;
				}
			}
			num++;
		}
		if (selectIndex > 0)
		{
			((VisualElement)m_lbYous[selectIndex]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_playerInfos[selectIndex].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_btnPlayerBgs[selectIndex]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			string icon3 = zbDataConfig.Instance.playerHeadCsvTable.GetRecordByKey(zbSelf.Instance.GetHeadId()).icon;
			Sprite val3 = zbAssetLoader.Instance.LoadResouce<Sprite>(icon3);
			UQueryExtensions.Q<VisualElement>(m_playerInfos[selectIndex], "header", (string)null).style.backgroundImage = new StyleBackground(val3);
			string icon4 = zbDataConfig.Instance.playerHeadFrameCsvTable.GetRecordByKey(zbSelf.Instance.GetHeadFrameId()).icon;
			Sprite val4 = zbAssetLoader.Instance.LoadResouce<Sprite>(icon4);
			UQueryExtensions.Q<VisualElement>(m_playerInfos[selectIndex], "frame", (string)null).style.backgroundImage = new StyleBackground(val4);
			((VisualElement)m_btnRefresh).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			ve_OK.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		else
		{
			((VisualElement)m_btnRefresh).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			ve_OK.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		UpdatePerSecond();
	}

	protected override void UpdatePerSecond()
	{
		if (zbSelf.Instance.truckHandler.trainInfo != null)
		{
			TrainDetail waitTrain = zbSelf.Instance.truckHandler.trainInfo.WaitTrain;
			if (waitTrain.StartTm >= zbSelf.Instance.timeHandler.GetServerTime())
			{
				zbUtil.SetDisplayIfChanged((VisualElement)(object)m_lbRefreshFree, (DisplayStyle)1);
				zbUtil.SetDisplayIfChanged((VisualElement)(object)m_lbRefreshIcon, (DisplayStyle)0);
				zbUtil.SetDisplayIfChanged((VisualElement)(object)m_lbRefreshCost, (DisplayStyle)0);
				((TextElement)m_lbTruckTime).text = zbUtil.FormatTimeHHMMSS(waitTrain.StartTm - zbSelf.Instance.timeHandler.GetServerTime());
				((TextElement)m_lbTruckWait).text = zbUtil.Language("lang.11378");
			}
			else
			{
				zbUtil.SetDisplayIfChanged((VisualElement)(object)m_lbRefreshFree, (DisplayStyle)0);
				zbUtil.SetDisplayIfChanged((VisualElement)(object)m_lbRefreshIcon, (DisplayStyle)1);
				zbUtil.SetDisplayIfChanged((VisualElement)(object)m_lbRefreshCost, (DisplayStyle)1);
				((TextElement)m_lbTruckTime).text = "00:00:00";
				((TextElement)m_lbTruckWait).text = zbUtil.Language("lang.11369");
			}
		}
	}

	private void onPlayerBg(int index = 0)
	{
		TrainDetail waitTrain = zbSelf.Instance.truckHandler.trainInfo.WaitTrain;
		int num = 0;
		foreach (TrainCarriageDetail carriage in waitTrain.CarriageList)
		{
			if (index == num && (carriage.RoleBaseInfo == null || carriage.RoleBaseInfo.RoleId == 0L))
			{
				selectIndex = num;
			}
			num++;
		}
		RefreshCustom();
	}

	public void OnPlayers(ClickEvent evt, int index)
	{
		TrainDetail waitTrain = zbSelf.Instance.truckHandler.trainInfo.WaitTrain;
		int num = 0;
		foreach (TrainCarriageDetail carriage in waitTrain.CarriageList)
		{
			if (carriage.RoleBaseInfo != null && num == index)
			{
				zbWsMsgHandler.Instance.Role_TipsInfo_Req((int)carriage.RoleBaseInfo.RoleId);
				break;
			}
			num++;
		}
	}

	private void OnRefresh(object data = null)
	{
		int[][] array = refreshDiamond;
		foreach (int[] array2 in array)
		{
			if (!zbSelf.Instance.dataHandler.HasResourceEnough((EResourceType)array2[0], array2[1], array2[2]))
			{
				zbUIMgr.Instance.ShowTip((EResourceType)array2[0], array2[1], array2[2]);
				return;
			}
		}
		if (zbSelf.Instance.truckHandler.trainInfo.WaitTrain.StartTm <= zbSelf.Instance.timeHandler.GetServerTime())
		{
			zbUIMgr.Instance.ShowMessageBox("Tip", zbUtil.Language("lang.11377"), delegate(bool isOK)
			{
				if (isOK)
				{
					zbWsMsgHandler.Instance.Train_TransportTake_Req();
				}
			});
		}
		else
		{
			zbWsMsgHandler.Instance.Train_TransportTakeRefresh_Req();
		}
	}

	private void OnCancel(object data = null)
	{
		selectIndex = -1;
		RefreshCustom();
	}

	private void OnConfirm(object data = null)
	{
		if (!zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Money, 38, 1))
		{
			zbUIMgr.Instance.ShowTip(EResourceType.Money, 38L, 1L);
		}
		else if (zbSelf.Instance.truckHandler.trainInfo != null && zbSelf.Instance.truckHandler.trainInfo.WaitTrain.StartTm <= zbSelf.Instance.timeHandler.GetServerTime())
		{
			zbWsMsgHandler.Instance.Train_TransportTake_Req();
		}
		else
		{
			zbWsMsgHandler.Instance.Train_TransportTakeConfirm_Req(zbSelf.Instance.truckHandler.trainInfo.WaitTrain.TransportId, selectIndex);
		}
	}

	private void OnDriver(object data = null)
	{
		OnClose();
		zbUIMgr.Instance.ShowTrainDriver();
	}

	public void OnRule()
	{
		string[] langKeys = new string[7] { "lang.11374", "", "lang.11375", "", "lang.11381", "", "lang.11382" };
		zbUIMgr.Instance.ShowRule(langKeys);
	}

	private void OnClose(object data = null)
	{
		Hide();
	}

	protected override void UpdateAtShow()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		base.UpdateAtShow();
		StyleLength top;
		Length value;
		if (moveStep == 1)
		{
			if (_lastTop > -900f)
			{
				top = ve_train.style.top;
				value = ((StyleLength)(ref top)).value;
				float value2 = ((Length)(ref value)).value;
				zbUtil.SetStyleIfChanged(ref _lastTop, value2 - 15f, delegate(float val)
				{
					//IL_000c: Unknown result type (might be due to invalid IL or missing references)
					ve_train.style.top = StyleLength.op_Implicit(val);
				});
			}
			else
			{
				moveStep = 0;
			}
		}
		else
		{
			if (moveStep != 2)
			{
				return;
			}
			if (_lastTop > 267f)
			{
				top = ve_train.style.top;
				value = ((StyleLength)(ref top)).value;
				float value3 = ((Length)(ref value)).value;
				zbUtil.SetStyleIfChanged(ref _lastTop, value3 - 15f, delegate(float val)
				{
					//IL_000c: Unknown result type (might be due to invalid IL or missing references)
					ve_train.style.top = StyleLength.op_Implicit(val);
				});
			}
			else
			{
				moveStep = 0;
			}
		}
	}

	public IEnumerator RefreshNewTrain()
	{
		zbAudioMgr.Instance.PlayAudio("audio/combat/motuo_2.mp3");
		for (int i = 0; i < 4; i++)
		{
			m_goldHeads[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		ve_OK.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		((VisualElement)m_btnRefresh).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_bottom.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		moveStep = 1;
		m_spineTrain.AnimationState.SetAnimation(1, "track2_move", true);
		while (moveStep == 1)
		{
			yield return null;
		}
		TrainDetail waitTrain = zbSelf.Instance.truckHandler.trainInfo.WaitTrain;
		zbTrainCsvRecord recordByKey = zbDataConfig.Instance.trainCsvTable.GetRecordByKey((int)waitTrain.Quality);
		m_spineTrain.AnimationState.SetAnimation(0, "track1_che_" + recordByKey.skinId, true);
		m_spineTrain.AnimationState.SetAnimation(1, "track2_move", true);
		zbAudioMgr.Instance.PlayAudio("audio/combat/motuo_2.mp3");
		zbUtil.SetStyleIfChanged(ref _lastTop, 1600f, delegate(float val)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			ve_train.style.top = StyleLength.op_Implicit(val);
		});
		moveStep = 2;
		while (moveStep == 2)
		{
			yield return null;
		}
		RefreshCustom();
	}
}
