using System;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Collections;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbWorldResourceCollectWnd : zbBaseWnd
{
	private Button m_btnTouch;

	private Button m_btnClose;

	private Button m_btnCollect;

	private VisualElement m_progress;

	private Label m_lbProgress;

	private int m_settleInterval = 3600;

	private VisualElement[] m_nodes = (VisualElement[])(object)new VisualElement[4];

	private Label[] m_lbNames = (Label[])(object)new Label[4];

	private Label[] m_lbOutputs = (Label[])(object)new Label[4];

	private zbItem[] m_itemIcons = new zbItem[4];

	private Label[] m_lbSumCounts = (Label[])(object)new Label[4];

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_btnCollect = UQueryExtensions.Q<Button>(m_RootElement, "btn_collect", (string)null);
		m_progress = UQueryExtensions.Q<VisualElement>(m_RootElement, "progress", (string)null);
		m_lbProgress = UQueryExtensions.Q<Label>(m_RootElement, "lb_progress", (string)null);
		for (int i = 0; i < m_nodes.Length; i++)
		{
			m_nodes[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "node" + i, (string)null);
			m_lbNames[i] = UQueryExtensions.Q<Label>(m_nodes[i], "lb_name", (string)null);
			m_lbOutputs[i] = UQueryExtensions.Q<Label>(m_nodes[i], "lb_output", (string)null);
			m_itemIcons[i] = UQueryExtensions.Q<zbItem>(m_nodes[i], "item", (string)null);
			m_itemIcons[i].OnEnable(this);
			m_lbSumCounts[i] = UQueryExtensions.Q<Label>(m_nodes[i], "lb_sumCount", (string)null);
		}
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCollect, (Action<ClickEvent>)OnCollect, "", (TrickleDown)0);
		AddDark();
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_WorldInfo, OnUpdateEvent);
	}

	public override void Refresh()
	{
		RefreshAggregatedNodes();
		RefreshTime();
	}

	private void RefreshTime()
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		if (zbServerData.sGameData.WorldInfo.ResIdleInfo != null)
		{
			float num = Mathf.Clamp((float)zbSelf.Instance.worldHandler.worldResIdleSettleSecond / (float)zbServerData.sGameData.WorldInfo.ResIdleInfo.SettleMax, 0f, 1f);
			m_progress.style.width = StyleLength.op_Implicit(new Length(num * 608f, (LengthUnit)0));
			if (num >= 1f)
			{
				((TextElement)m_lbProgress).text = "Max";
			}
			else
			{
				((TextElement)m_lbProgress).text = zbUtil.FormatTimeHHMMSS(zbSelf.Instance.worldHandler.worldResIdleSettleSecond);
			}
			if (zbSelf.Instance.worldHandler.worldResIdleSettleSecond > m_settleInterval)
			{
				((TextElement)m_btnCollect).text = zbUtil.Language("lang.10746");
				((VisualElement)m_btnCollect).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_250_80_orange.png"));
			}
			else
			{
				((TextElement)m_btnCollect).text = zbUtil.FormatTimeHHMMSS(m_settleInterval - zbSelf.Instance.worldHandler.worldResIdleSettleSecond);
				((VisualElement)m_btnCollect).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btnGray_250x80.png"));
			}
		}
	}

	private void RefreshAggregatedNodes()
	{
		//IL_030d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		Dictionary<(EResourceType, int), int> dictionary = new Dictionary<(EResourceType, int), int>();
		if (zbServerData.sGameData.WorldInfo != null && zbServerData.sGameData.WorldInfo.BuildingResList != null)
		{
			foreach (WorldBuildingResDetail buildingRes in zbServerData.sGameData.WorldInfo.BuildingResList)
			{
				if (buildingRes == null || buildingRes.ResLevel <= 0)
				{
					continue;
				}
				WorldMapGridDetail worldMapGridDetailByGridId = zbSelf.Instance.worldHandler.GetWorldMapGridDetailByGridId(buildingRes.GridId);
				if (worldMapGridDetailByGridId == null)
				{
					continue;
				}
				zbWorldBuildingCsvRecord recordByKey = zbDataConfig.Instance.worldBuildingCsvTable.GetRecordByKey(worldMapGridDetailByGridId.BuildingId);
				if (recordByKey != null && recordByKey.resOut != null && recordByKey.resOut.Length >= 3)
				{
					EResourceType item = (EResourceType)recordByKey.resOut[0];
					int item2 = recordByKey.resOut[1];
					int num = recordByKey.resOut[2];
					(EResourceType, int) key = (item, item2);
					if (dictionary.ContainsKey(key))
					{
						dictionary[key] += num;
					}
					else
					{
						dictionary[key] = num;
					}
				}
			}
		}
		var list = (from x in Enumerable.Select(dictionary, (KeyValuePair<(EResourceType resType, int resId), int> kv) => new
			{
				ResType = kv.Key.resType,
				ResId = kv.Key.resId,
				PerHour = kv.Value
			})
			orderby x.PerHour descending
			select x).Take(4).ToList();
		for (int num2 = 0; num2 < m_nodes.Length; num2++)
		{
			if (num2 < list.Count)
			{
				var anon = list[num2];
				m_nodes[num2].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_itemIcons[num2].Refresh(new zbItemData(anon.ResType, anon.ResId, anon.PerHour), isNameVis: false, isCountVis: false);
				stResorceIconInfo resorceIconPath = zbResourceHelper.GetResorceIconPath(anon.ResType, anon.ResId);
				((TextElement)m_lbNames[num2]).text = resorceIconPath.strName;
				((TextElement)m_lbOutputs[num2]).text = anon.PerHour + "/H";
				if (m_lbSumCounts[num2] == null)
				{
					continue;
				}
				long num3 = 0L;
				if (zbServerData.sGameData.WorldInfo != null && zbServerData.sGameData.WorldInfo.ResIdleInfo != null)
				{
					RepeatedField<AwardDetail> bagAwardList = zbServerData.sGameData.WorldInfo.ResIdleInfo.BagAwardList;
					if (bagAwardList != null)
					{
						foreach (AwardDetail item3 in bagAwardList)
						{
							if (item3.Type == anon.ResType && item3.Tid == anon.ResId)
							{
								num3 += item3.Count;
							}
						}
					}
				}
				((TextElement)m_lbSumCounts[num2]).text = string.Format(zbUtil.Language("lang.10761"), num3);
			}
			else
			{
				m_nodes[num2].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
		}
	}

	private void OnCollect(ClickEvent evt)
	{
		if (zbSelf.Instance.worldHandler.worldResIdleSettleSecond >= m_settleInterval)
		{
			zbWsMsgHandler.Instance.WorldResource_IdleAward_Req();
		}
	}

	private void OnClose(ClickEvent evt)
	{
		Hide();
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		RefreshTime();
	}

	private void OnUpdateEvent(object data)
	{
		Refresh();
	}
}
