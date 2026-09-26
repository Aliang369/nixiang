using System;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Collections;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbWorldReconExploreWnd : zbBaseWnd
{
	private Button m_btnClose;

	public Button m_btnConfirm;

	private Button m_btnTouch;

	private Label m_lbName;

	private Label m_lbDesc;

	private VisualElement m_teamChoose;

	private VisualElement[] m_teams = (VisualElement[])(object)new VisualElement[2];

	private VisualElement m_timeSpend;

	private VisualElement m_resourceList;

	private VisualElement[] m_resources = (VisualElement[])(object)new VisualElement[3];

	private WorldMapGridDetail m_gridDetail;

	private zbWorldBuildingCsvRecord m_buildInfo;

	private readonly Button[] m_btnInfo = (Button[])(object)new Button[2];

	private int m_teamId = 1;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_teamChoose = UQueryExtensions.Q<VisualElement>(m_RootElement, "teamChoose", (string)null);
		m_timeSpend = UQueryExtensions.Q<VisualElement>(m_RootElement, "timeSpend", (string)null);
		m_resourceList = UQueryExtensions.Q<VisualElement>(m_RootElement, "resourceList", (string)null);
		m_btnConfirm = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		for (int i = 0; i < 2; i++)
		{
			m_teams[i] = UQueryExtensions.Q<VisualElement>(m_teamChoose, $"team{i}", (string)null);
			int tempIndex = i;
			UQueryExtensions.Q<VisualElement>(m_teams[i], "icon_check", (string)null).visible = m_teamId == i + 1;
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)UQueryExtensions.Q<Button>(m_teams[i], "btn_toggle", (string)null), (Action<ClickEvent>)delegate
			{
				OnToggle(tempIndex);
			}, "", (TrickleDown)0);
			m_btnInfo[i] = UQueryExtensions.Q<Button>(m_teams[i], "btn_info", (string)null);
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnInfo[i], (Action<ClickEvent>)delegate
			{
				ShowWorldTeamInfo(tempIndex);
			}, "", (TrickleDown)0);
		}
		for (int num = 0; num < 3; num++)
		{
			m_resources[num] = UQueryExtensions.Q<VisualElement>(m_resourceList, $"resource{num}", (string)null);
			UQueryExtensions.Q<zbItem>(m_resources[num], (string)null, (string)null).OnEnable(this);
		}
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnConfirm, (Action)OnConfrim, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		AddDark();
		CreateMoneyPanel(new EMoneyType[2]
		{
			EMoneyType.Diamond,
			EMoneyType.Gold
		});
		m_gameEevent.RegisterEvent(EGameEvent.Event_WorldReconExplore_Ack, OnWorldReconExploreAck);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Money, OnUpdateEvent);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_HomelandInfo, OnUpdateEvent);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_WorldInfo, OnUpdateEvent);
	}

	public void RefreshCustom(WorldMapGridDetail gridDetail, bool isFirstIn = true)
	{
		m_gridDetail = gridDetail;
		if (m_gridDetail.BuildingId > 0)
		{
			m_buildInfo = zbDataConfig.Instance.worldBuildingCsvTable.GetRecordByKey(m_gridDetail.BuildingId);
		}
		WorldReconDetail worldReconDetail = zbSelf.Instance.worldHandler.GetWorldReconDetail(1);
		WorldReconDetail worldReconDetail2 = zbSelf.Instance.worldHandler.GetWorldReconDetail(2);
		if (isFirstIn)
		{
			if (worldReconDetail.State == EWorldReconState.Idle)
			{
				OnToggle(0);
			}
			else if (worldReconDetail2 == null || (worldReconDetail2 != null && worldReconDetail2.State == EWorldReconState.Idle))
			{
				OnToggle(1);
			}
			else
			{
				OnToggle(0);
			}
		}
		if (m_buildInfo == null || m_gridDetail.State == EWorldMapGridState.Explorable)
		{
			InitBuildInfo();
		}
		InitSpendTime();
		InitTeamChoose();
		InitResourceList();
	}

	private void InitBuildInfo()
	{
		if (m_gridDetail.State == EWorldMapGridState.Explorable)
		{
			((TextElement)m_lbName).text = zbUtil.Language("lang.10525");
			((TextElement)m_lbDesc).text = zbUtil.Language("lang.10526");
		}
	}

	private void InitTeamChoose()
	{
		RepeatedField<WorldReconDetail> reconList = zbServerData.sGameData.WorldInfo.ReconList;
		for (int i = 0; i < 2; i++)
		{
			UQueryExtensions.Q<VisualElement>(m_teams[i], "icon_check", (string)null).visible = ((i + 1 == m_teamId) ? true : false);
			((TextElement)UQueryExtensions.Q<Label>(m_teams[i], "lb_name", (string)null)).text = zbUtil.Language("lang.10254") + (i + 1);
			if (i < reconList.Count)
			{
				((TextElement)UQueryExtensions.Q<Label>(m_teams[i], "lb_status", (string)null)).text = zbSelf.Instance.worldHandler.GetStateString(reconList[i].State);
			}
			else
			{
				((TextElement)UQueryExtensions.Q<Label>(m_teams[i], "lb_status", (string)null)).text = zbUtil.Language("lang.10600");
			}
		}
	}

	private void InitSpendTime()
	{
		Label val = UQueryExtensions.Q<Label>(m_timeSpend, "lb_timeSpendVal", (string)null);
		int key = Mathf.Abs(m_gridDetail.X) + Mathf.Abs(m_gridDetail.Y);
		zbWorldDistanceCsvRecord recordByKey = zbDataConfig.Instance.worldDistanceCsvTable.GetRecordByKey(key);
		if (recordByKey == null)
		{
			((TextElement)val).text = "--:--:--";
		}
		else
		{
			((TextElement)val).text = zbUtil.FormatTimeHHMMSS(recordByKey.time);
		}
	}

	private void InitResourceList()
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		JObject record = zbDataConfig.Instance.worldBaseJsonTable.GetRecord();
		if (record == null)
		{
			return;
		}
		for (int i = 0; i < 3; i++)
		{
			bool flag = i < ((IEnumerable<JToken>)record["explore"][(object)"costList"]).ToArray().Count();
			VisualElement val = m_resources[i];
			val.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			if (flag)
			{
				JToken val2 = record["explore"][(object)"costList"][(object)i];
				zbItemData data = new zbItemData((EResourceType)Extensions.Value<int>((IEnumerable<JToken>)((IEnumerable<JToken>)val2).ToArray()[0]), Extensions.Value<int>((IEnumerable<JToken>)((IEnumerable<JToken>)val2).ToArray()[1]), Extensions.Value<int>((IEnumerable<JToken>)((IEnumerable<JToken>)val2).ToArray()[2]));
				UQueryExtensions.Q<zbItem>(m_resources[i], (string)null, (string)null).Refresh(data, isNameVis: false, isCountVis: false);
				((TextElement)UQueryExtensions.Q<Label>(val, "lb_count", (string)null)).text = $"{val2[(object)2]}";
			}
		}
	}

	private void OnClose()
	{
		Hide();
	}

	private void OnConfrim()
	{
		if (m_teamId > zbServerData.sGameData.WorldInfo.ReconList.Count)
		{
			if (zbSelf.Instance.vipHandler.CheckVipFuncLimited(EVipFuncType.WorldRecon2, zbSelf.Instance.GetVipLevel()) <= 0)
			{
				zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.11492"), zbSelf.Instance.vipHandler.CheckVipFuncUnlock(EVipFuncType.WorldRecon2)) + "【" + zbUtil.Language("lang.11495") + "】");
			}
			return;
		}
		if (zbServerData.sGameData.WorldInfo.ReconList[m_teamId - 1].State != EWorldReconState.Idle)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10336"));
			return;
		}
		JObject record = zbDataConfig.Instance.worldBaseJsonTable.GetRecord();
		if (record != null)
		{
			foreach (JToken item in (IEnumerable<JToken>)record["explore"][(object)"costList"])
			{
				if (!zbSelf.Instance.dataHandler.HasResourceEnough((EResourceType)Extensions.Value<int>((IEnumerable<JToken>)((IEnumerable<JToken>)item).ToArray()[0]), Extensions.Value<int>((IEnumerable<JToken>)((IEnumerable<JToken>)item).ToArray()[1]), Extensions.Value<int>((IEnumerable<JToken>)((IEnumerable<JToken>)item).ToArray()[2])))
				{
					zbUIMgr.Instance.ShowTip((EResourceType)Extensions.Value<int>((IEnumerable<JToken>)((IEnumerable<JToken>)item).ToArray()[0]), Extensions.Value<int>((IEnumerable<JToken>)((IEnumerable<JToken>)item).ToArray()[1]), Extensions.Value<int>((IEnumerable<JToken>)((IEnumerable<JToken>)item).ToArray()[2]));
					return;
				}
			}
		}
		zbWsMsgHandler.Instance.World_ReconExplore_Req(m_teamId, m_gridDetail.GridId);
	}

	private void OnToggle(int tempIndex)
	{
		for (int i = 0; i < 2; i++)
		{
			bool visible = i == tempIndex;
			UQueryExtensions.Q<VisualElement>(m_teams[i], "icon_check", (string)null).visible = visible;
		}
		m_teamId = tempIndex + 1;
	}

	private void ShowWorldTeamInfo(int index)
	{
		if (zbSelf.Instance.homelandHandler.GetHomelandRoomDetail(zbServerData.sGameData.HomelandInfo.BaseInfo.RoomMark.HallId).Level > 0)
		{
			WorldReconDetail worldReconDetail = zbSelf.Instance.worldHandler.GetWorldReconDetail(index + 1);
			if (worldReconDetail != null)
			{
				zbUIMgr.Instance.ShowWorldRecon(worldReconDetail);
			}
		}
		else
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10680"));
		}
	}

	private void OnUpdateEvent(object data)
	{
		RefreshCustom(m_gridDetail, isFirstIn: false);
	}

	private void OnWorldReconExploreAck(object data)
	{
		OnClose();
	}
}
