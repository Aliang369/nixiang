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

public class zbPrayLanternWnd : zbBaseWnd
{
	public SkeletonAnimation[] m_spineAwards = (SkeletonAnimation[])(object)new SkeletonAnimation[13];

	private Label m_lbTime;

	private Button m_btnClose;

	private Label m_lbTitle;

	private VisualElement m_ultimate;

	private zbItem m_itemUltimate;

	private VisualElement m_ultimateProgressBg;

	private VisualElement m_ultimateProgress;

	private Label m_lbUltimateProgress;

	private Button m_btnTip;

	private Label m_lbTipDesc;

	private AutoHideTipController m_tipController;

	private Button m_btnUltimate;

	private Button m_btnChange;

	private zbPrayLanternItem[] m_items = new zbPrayLanternItem[12];

	private VisualElement m_nodeTarget;

	private Label m_lbCostCount;

	private VisualElement m_progress;

	private VisualElement[] m_targets = (VisualElement[])(object)new VisualElement[4];

	private zbItem[] m_itemTargets = new zbItem[4];

	private Label[] m_lbTargets = (Label[])(object)new Label[4];

	private Button m_btnOne;

	private Label m_lbOne;

	private VisualElement m_spriteOne;

	private Label m_lbFree;

	private Button m_btnTen;

	private Label m_lbTen;

	private VisualElement m_spriteTen;

	private VisualElement m_notifyFree;

	private VisualElement m_notifyTen;

	private PrayLanternDetail m_data;

	private UltimateAwardDetail m_ultimateAward;

	private int m_preAngle = 1;

	private float m_intervalTime = 0.02f;

	private bool isRoration;

	protected override void Awake()
	{
		base.Awake();
		for (int i = 0; i < 12; i++)
		{
			m_items[i] = new zbPrayLanternItem();
		}
	}

	protected override void OnEnable()
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Invalid comparison between Unknown and I4
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0423: Unknown result type (might be due to invalid IL or missing references)
		//IL_044e: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d4: Invalid comparison between Unknown and I4
		//IL_04e6: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		for (int i = 0; i < 13; i++)
		{
			m_spineAwards[i] = ((Component)m_GameObjectSceneRoot.transform.Find("award" + i)).GetComponent<SkeletonAnimation>();
		}
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		if ((int)Application.platform == 8)
		{
			((VisualElement)m_lbTime).style.top = StyleLength.op_Implicit(80f);
		}
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_ultimate = UQueryExtensions.Q<VisualElement>(m_RootElement, "ultimate", (string)null);
		m_itemUltimate = UQueryExtensions.Q<zbItem>(m_ultimate, "item", (string)null);
		m_itemUltimate.OnEnable(this);
		m_ultimateProgressBg = UQueryExtensions.Q<VisualElement>(m_ultimate, "progress_bg", (string)null);
		m_ultimateProgress = UQueryExtensions.Q<VisualElement>(m_ultimate, "progress", (string)null);
		m_lbUltimateProgress = UQueryExtensions.Q<Label>(m_ultimate, "lb_progress", (string)null);
		m_btnTip = UQueryExtensions.Q<Button>(m_ultimate, "btn_tipDesc", (string)null);
		m_lbTipDesc = UQueryExtensions.Q<Label>(m_ultimate, "lb_tipDesc", (string)null);
		m_tipController = new AutoHideTipController(m_uiEvent, m_btnTip, m_lbTipDesc);
		m_btnUltimate = UQueryExtensions.Q<Button>(m_ultimate, "btn_touch", (string)null);
		m_btnChange = UQueryExtensions.Q<Button>(m_ultimate, "btn_change", (string)null);
		for (int j = 0; j < 12; j++)
		{
			m_items[j] = UQueryExtensions.Q<zbPrayLanternItem>(m_RootElement, "item" + j, (string)null);
			m_items[j].OnEnable(this, j);
		}
		m_nodeTarget = UQueryExtensions.Q<VisualElement>(m_RootElement, "target", (string)null);
		m_lbCostCount = UQueryExtensions.Q<Label>(m_nodeTarget, "lb_costCount", (string)null);
		m_progress = UQueryExtensions.Q<VisualElement>(m_nodeTarget, "progress", (string)null);
		for (int k = 0; k < 4; k++)
		{
			m_targets[k] = UQueryExtensions.Q<VisualElement>(m_RootElement, $"target{k}", (string)null);
			m_itemTargets[k] = UQueryExtensions.Q<zbItem>(m_targets[k], "itemTarget", (string)null);
			m_itemTargets[k].OnEnable(this);
			m_lbTargets[k] = UQueryExtensions.Q<Label>(m_targets[k], "lb_target", (string)null);
		}
		m_btnOne = UQueryExtensions.Q<Button>(m_RootElement, "btn_one", (string)null);
		m_lbOne = UQueryExtensions.Q<Label>(m_RootElement, "lb_one", (string)null);
		m_spriteOne = UQueryExtensions.Q<VisualElement>(m_RootElement, "sprite_one", (string)null);
		m_lbFree = UQueryExtensions.Q<Label>(m_RootElement, "lb_free", (string)null);
		m_btnTen = UQueryExtensions.Q<Button>(m_RootElement, "btn_ten", (string)null);
		m_lbTen = UQueryExtensions.Q<Label>(m_RootElement, "lb_ten", (string)null);
		m_spriteTen = UQueryExtensions.Q<VisualElement>(m_RootElement, "sprite_ten", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnUltimate, (Action<ClickEvent>)delegate
		{
			OnUltimate();
		}, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnChange, (Action<ClickEvent>)delegate
		{
			OnChange();
		}, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnOne, (Action<ClickEvent>)delegate
		{
			OnBar(1);
		}, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTen, (Action<ClickEvent>)delegate
		{
			OnBar(10);
		}, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_notifyFree = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnOne, (Vector2?)new Vector2(3f, 3f), 1f, true);
		m_notifyTen = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnTen, (Vector2?)new Vector2(3f, 3f), 1f, true);
		CreateMoneyPanel(new EMoneyType[1] { EMoneyType.LimitTimeRoulette });
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Money, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Operate, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_PrayLanternDraw_Ack, OnPrayLanternDrawAck);
		if ((int)Application.platform == 8)
		{
			((VisualElement)m_lbTime).style.top = StyleLength.op_Implicit(80f);
		}
	}

	public void RefreshCustom(PrayLanternDetail data, bool isDrawAck = false)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_036c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0382: Unknown result type (might be due to invalid IL or missing references)
		//IL_0398: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_032a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0340: Unknown result type (might be due to invalid IL or missing references)
		//IL_0408: Unknown result type (might be due to invalid IL or missing references)
		//IL_043c: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_lbTipDesc).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_data = data;
		if (m_data == null)
		{
			return;
		}
		float num = Mathf.Clamp((float)m_data.UnlockList.Count / 12f, 0f, 1f);
		m_ultimateProgress.style.width = StyleLength.op_Implicit(new Length(num * 140f, (LengthUnit)0));
		((TextElement)m_lbUltimateProgress).text = m_data.UnlockList.Count + "/12";
		((VisualElement)m_lbFree).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_data.DailyFree != EBoolState.False) ? 1 : 0));
		m_spriteOne.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_data.DailyFree != EBoolState.True));
		((VisualElement)m_lbOne).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_data.DailyFree != EBoolState.True));
		if (zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Money, 34, 1))
		{
			((VisualElement)m_lbOne).style.color = StyleColor.op_Implicit(Color.white);
		}
		else
		{
			((VisualElement)m_lbOne).style.color = StyleColor.op_Implicit(Color.red);
		}
		if (zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Money, 34, 10))
		{
			((VisualElement)m_lbTen).style.color = StyleColor.op_Implicit(Color.white);
		}
		else
		{
			((VisualElement)m_lbTen).style.color = StyleColor.op_Implicit(Color.red);
		}
		if (m_data.DailyFree == EBoolState.False || (m_data.Ultimate.State != ECommonState.End && zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Money, 34, 1)))
		{
			m_notifyFree.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		else
		{
			m_notifyFree.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		if (m_data.Ultimate.State != ECommonState.End && zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Money, 34, 10))
		{
			m_notifyTen.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		else
		{
			m_notifyTen.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		m_ultimateAward = zbSelf.Instance.operateHandler.GetUltimateAward(data.BaseInfo.UUID);
		if (m_ultimateAward != null)
		{
			((Component)m_spineAwards[12]).gameObject.SetActive(false);
			((VisualElement)m_itemUltimate).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_itemUltimate.Refresh(new zbItemData(m_ultimateAward.Award.Type, m_ultimateAward.Award.Tid, m_ultimateAward.Award.Count));
			m_itemUltimate.SetGettedVisible(m_data.Ultimate.State == ECommonState.End);
			m_ultimateProgressBg.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_btnTip).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		else
		{
			((Component)m_spineAwards[12]).gameObject.SetActive(true);
			((VisualElement)m_itemUltimate).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_ultimateProgressBg.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_btnTip).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		m_itemUltimate.SetSpineVisible(m_data.Ultimate.State == ECommonState.Yes);
		m_itemUltimate.SetGettedVisible(m_data.Ultimate.State == ECommonState.End);
		((VisualElement)m_btnUltimate).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_ultimateAward != null && m_data.Ultimate.State != ECommonState.Yes) ? 1 : 0));
		((VisualElement)m_btnChange).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_ultimateAward == null || m_data.Ultimate.State == ECommonState.End) ? 1 : 0));
		for (int i = 0; i < 12; i++)
		{
			int floor = 0;
			zbPrayLanternAwardCsvRecord recordByKey = zbDataConfig.Instance.prayLanternAwardCsvTable.GetRecordByKey(i + 1);
			if (recordByKey != null)
			{
				floor = recordByKey.floor;
			}
			bool isGray = zbSelf.Instance.operateHandler.IsFloorGray(m_data.BaseInfo.UUID, floor);
			m_items[i].Refresh(zbSelf.Instance.operateHandler.IsUnlock(m_data.BaseInfo.UUID, i + 1), isDrawAck, isGray);
		}
		RefreshTarget();
		RefreshTime();
	}

	private void RefreshTarget()
	{
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)m_lbCostCount).text = m_data.DrawCount.ToString();
		float num = m_data.TargetList.Count;
		for (int i = 0; i < 4; i++)
		{
			bool flag = i < m_data.TargetList.Count;
			m_targets[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			if (!flag)
			{
				continue;
			}
			float num2 = (float)(i + 1) / num * 490f - 66.666664f;
			m_targets[i].style.left = StyleLength.op_Implicit(num2);
			zbItemData data = new zbItemData(m_data.TargetList[i].AwardList[0].Type, m_data.TargetList[i].AwardList[0].Tid, m_data.TargetList[i].AwardList[0].Count);
			m_itemTargets[i].Refresh(data);
			bool flag2 = m_data.TargetList[i].State == ECommonState.Yes;
			bool gettedVisible = m_data.TargetList[i].State == ECommonState.End;
			m_itemTargets[i].SetSpineVisible(flag2);
			int tempIndex = m_data.TargetList[i].Index;
			if (flag2)
			{
				m_itemTargets[i].SetTouchCallback(delegate
				{
					OnTargets(tempIndex);
				});
			}
			else
			{
				m_itemTargets[i].SetTouchCallback(null);
			}
			m_itemTargets[i].SetGettedVisible(gettedVisible);
			((TextElement)m_lbTargets[i]).text = m_data.TargetList[i].Target.ToString();
		}
		m_progress.style.width = StyleLength.op_Implicit(new Length(GetProgress() * 490f, (LengthUnit)0));
	}

	private float GetProgress()
	{
		RepeatedField<OperateTargetDetail> targetList = m_data.TargetList;
		if (targetList == null || targetList.Count == 0)
		{
			return 0f;
		}
		int count = targetList.Count;
		float num = 1f / (float)count;
		for (int i = 0; i < count; i++)
		{
			int target = targetList[i].Target;
			if (m_data.DrawCount < target)
			{
				int num2 = ((i != 0) ? targetList[i - 1].Target : 0);
				float num3 = (float)(m_data.DrawCount - num2) / (float)(target - num2);
				return Mathf.Clamp01((float)i * num + num3 * num);
			}
		}
		return 1f;
	}

	private void OnUltimate()
	{
		if (!isRoration && m_data != null)
		{
			if (m_ultimateAward == null)
			{
				OnChange();
			}
			else if (m_data.Ultimate.State == ECommonState.End)
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10157"));
			}
			else if (m_data.Ultimate.State == ECommonState.Yes)
			{
				zbWsMsgHandler.Instance.Operate_PrayLanternUltimate_Req(m_data.BaseInfo.UUID);
			}
		}
	}

	private void OnChange()
	{
		if (!isRoration && m_data != null)
		{
			zbUIMgr.Instance.ShowPrayLanternUltimate(m_data);
		}
	}

	private void OnBar(int count)
	{
		if (isRoration)
		{
			return;
		}
		if (m_ultimateAward == null)
		{
			OnUltimate();
		}
		else
		{
			if (m_data == null)
			{
				return;
			}
			if ((count != 1 || m_data.DailyFree != EBoolState.False) && !zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Money, 34, count))
			{
				zbUIMgr.Instance.ShowTip(EResourceType.Money, 34L, count);
				return;
			}
			for (int i = 0; i < 12; i++)
			{
				int floor = 0;
				zbPrayLanternAwardCsvRecord recordByKey = zbDataConfig.Instance.prayLanternAwardCsvTable.GetRecordByKey(i + 1);
				if (recordByKey != null)
				{
					floor = recordByKey.floor;
				}
				bool isGray = zbSelf.Instance.operateHandler.IsFloorGray(m_data.BaseInfo.UUID, floor);
				m_items[i].Refresh(zbSelf.Instance.operateHandler.IsUnlock(m_data.BaseInfo.UUID, i + 1), isDrawAck: false, isGray);
			}
			zbWsMsgHandler.Instance.Operate_PrayLanternDraw_Req(m_data.BaseInfo.UUID, count);
			isRoration = true;
		}
	}

	private void OnTargets(int index)
	{
		if (!isRoration && m_data != null)
		{
			zbWsMsgHandler.Instance.Operate_PrayLanternTarget_Req(m_data.BaseInfo.UUID, index);
		}
	}

	private void OnClose()
	{
		if (!isRoration)
		{
			Hide();
		}
	}

	private void RefreshTime()
	{
		if (m_data != null)
		{
			long seconds = m_data.BaseInfo.EndTm - zbSelf.Instance.timeHandler.GetServerTime();
			((TextElement)m_lbTime).text = zbUtil.FormatTimeHHMMSS(seconds);
		}
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		RefreshTime();
	}

	private void OnEventUpdate(object data)
	{
		if (m_data != null && !isRoration)
		{
			PrayLanternDetail prayLanternInfoByUUID = zbSelf.Instance.operateHandler.GetPrayLanternInfoByUUID(m_data.BaseInfo.UUID);
			RefreshCustom(prayLanternInfoByUUID);
		}
	}

	private void OnPrayLanternDrawAck(object data)
	{
		((MonoBehaviour)this).StartCoroutine(RefreshDrawAck());
	}

	private void InitAnim()
	{
		zbPrayLanternItem[] items = m_items;
		for (int i = 0; i < items.Length; i++)
		{
			items[i].ClearResult();
		}
	}

	public IEnumerator RefreshDrawAck()
	{
		if (zbSelf.Instance.operateHandler.prayLanternDrawAckData == null)
		{
			yield break;
		}
		InitAnim();
		int count = zbSelf.Instance.operateHandler.prayLanternDrawAckData.IdList.Count;
		int slot = zbSelf.Instance.operateHandler.prayLanternDrawAckData.IdList[count - 1];
		int maxDrawId = zbSelf.Instance.operateHandler.GetMaxDrawId(m_data.BaseInfo.UUID, ((IEnumerable<int>)zbSelf.Instance.operateHandler.prayLanternDrawAckData.IdList).ToArray());
		List<int> list = new List<int>();
		for (int i = m_preAngle; i <= maxDrawId; i++)
		{
			list.Add(i);
		}
		for (int j = 1; j <= maxDrawId; j++)
		{
			list.Add(j);
		}
		for (int k = 1; k <= slot; k++)
		{
			list.Add(k);
		}
		foreach (int id in list)
		{
			yield return (object)new WaitForSeconds(m_intervalTime);
			m_items[id - 1].PlayEffect();
		}
		int num = 0;
		foreach (int id2 in zbSelf.Instance.operateHandler.prayLanternDrawAckData.IdList)
		{
			m_items[id2 - 1].PlayResult(num == 0);
			num++;
		}
		yield return (object)new WaitForSeconds(0.5f);
		foreach (int id3 in zbSelf.Instance.operateHandler.prayLanternDrawAckData.IdList)
		{
			int num2 = 0;
			foreach (int id4 in zbSelf.Instance.operateHandler.prayLanternDrawAckData.IdList)
			{
				if (id3 == id4)
				{
					num2++;
				}
			}
			m_items[id3 - 1].SetCount(num2);
		}
		zbServerData.sGameData.OperateInfo.PrayLanternList.Clear();
		zbServerData.sGameData.OperateInfo.PrayLanternList.AddRange((IEnumerable<PrayLanternDetail>)zbSelf.Instance.operateHandler.prayLanternDrawAckData.PrayLanternList);
		zbSelf.Instance.dataHandler.DataChange(zbSelf.Instance.operateHandler.prayLanternDrawAckData.DataChange, ((IEnumerable<AwardDetail>)zbSelf.Instance.operateHandler.prayLanternDrawAckData.AwardList).ToArray());
		isRoration = false;
		PrayLanternDetail prayLanternInfoByUUID = zbSelf.Instance.operateHandler.GetPrayLanternInfoByUUID(m_data.BaseInfo.UUID);
		RefreshCustom(prayLanternInfoByUUID, isDrawAck: true);
		zbSelf.Instance.operateHandler.prayLanternDrawAckData = null;
		m_preAngle = slot;
	}
}
