using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbMechaSubjectTabRoulette
{
	private zbMechaSubjectWnd m_wndParent;

	private VisualElement m_RootElement;

	private Label m_lbName;

	private Label m_lbDesc;

	private Button m_btnRate;

	private Button m_btnOne;

	private Label m_lbOne;

	private Button m_btnTen;

	private Label m_lbTen;

	private VisualElement m_notifyOne;

	private VisualElement m_notifyTen;

	private VisualElement m_nodeToggle;

	private VisualElement m_spriteOk;

	private VisualElement m_nodeAwards;

	private VisualElement m_nodeCounts;

	private zbMechaSubjectRouletteItem[] m_awardList = new zbMechaSubjectRouletteItem[12];

	private Label[] m_lbCounts = (Label[])(object)new Label[11];

	private VisualElement m_nodeTarget;

	private Button m_btnTipDesc;

	private Label m_lbTipDesc;

	private AutoHideTipController m_tipController;

	private Label m_lbCount;

	private VisualElement m_spriteProgress;

	private Label[] m_lbTargets = (Label[])(object)new Label[5];

	private zbItem[] m_items = new zbItem[5];

	private bool closeAnim;

	public bool isRoration;

	private static float startReqTime;

	private float m_intervalTime = 0.02f;

	public void OnEnable(zbMechaSubjectWnd wndParent, VisualElement root)
	{
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		m_RootElement = root;
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_btnRate = UQueryExtensions.Q<Button>(m_RootElement, "btn_rate", (string)null);
		m_btnOne = UQueryExtensions.Q<Button>(m_RootElement, "btn_one", (string)null);
		m_lbOne = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnOne, "lb_cost", (string)null);
		m_btnTen = UQueryExtensions.Q<Button>(m_RootElement, "btn_ten", (string)null);
		m_lbTen = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnTen, "lb_cost", (string)null);
		m_notifyOne = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnOne, (Vector2?)new Vector2(2f, 0f), 1f, true);
		m_notifyTen = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnTen, (Vector2?)new Vector2(2f, 0f), 1f, true);
		m_nodeToggle = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_toggle", (string)null);
		m_spriteOk = UQueryExtensions.Q<VisualElement>(m_nodeToggle, "sprite_ok", (string)null);
		m_nodeAwards = UQueryExtensions.Q<VisualElement>(m_RootElement, "awardList", (string)null);
		for (int i = 0; i < 12; i++)
		{
			int index = i;
			m_awardList[i] = new zbMechaSubjectRouletteItem();
			m_awardList[i] = UQueryExtensions.Q<zbMechaSubjectRouletteItem>(m_nodeAwards, "item" + i, (string)null);
			m_awardList[i].OnEnable(m_wndParent, index);
		}
		m_nodeCounts = UQueryExtensions.Q<VisualElement>(m_RootElement, "countList", (string)null);
		for (int j = 0; j < 11; j++)
		{
			m_lbCounts[j] = UQueryExtensions.Q<Label>(m_nodeCounts, "lb_count" + j, (string)null);
			((VisualElement)m_lbCounts[j]).visible = false;
		}
		m_nodeTarget = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_target", (string)null);
		m_btnTipDesc = UQueryExtensions.Q<Button>(m_nodeTarget, "btn_tipDesc", (string)null);
		m_lbTipDesc = UQueryExtensions.Q<Label>(m_nodeTarget, "lb_tipDesc", (string)null);
		m_tipController = new AutoHideTipController(m_wndParent.m_uiEvent, m_btnTipDesc, m_lbTipDesc);
		m_lbCount = UQueryExtensions.Q<Label>(m_nodeTarget, "lb_count", (string)null);
		m_spriteProgress = UQueryExtensions.Q<VisualElement>(m_nodeTarget, "progress", (string)null);
		for (int k = 0; k < 5; k++)
		{
			m_lbTargets[k] = UQueryExtensions.Q<Label>(m_nodeTarget, "lb_target" + k, (string)null);
			m_items[k] = UQueryExtensions.Q<zbItem>(m_nodeTarget, "item" + k, (string)null);
			m_items[k].OnEnable(m_wndParent);
		}
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>(m_nodeToggle, (Action)OnToggle, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRate, (Action)OnRate, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnOne, (Action)OnOne, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTen, (Action)OnTen, "", (TrickleDown)0);
		m_wndParent.m_gameEevent.RegisterEvent(EGameEvent.Event_MechaSubjectRoulette_Ack, OnMechaSubjectRouletteAck);
	}

	public void Refresh()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		if (m_wndParent.m_data != null)
		{
			bool flag = zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Money, 35, zbSelf.Instance.mechaSubjectHandler.rouletteCost);
			((VisualElement)m_lbOne).style.color = StyleColor.op_Implicit(flag ? Color.white : Color.red);
			bool flag2 = zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Money, 35, 10 * zbSelf.Instance.mechaSubjectHandler.rouletteCost);
			((VisualElement)m_lbTen).style.color = StyleColor.op_Implicit(flag2 ? Color.white : Color.red);
			int num = (m_wndParent.m_data.RouletteInfo.MaxLucky - m_wndParent.m_data.RouletteInfo.Lucky + m_wndParent.m_data.RouletteInfo.OnceLucky - 1) / m_wndParent.m_data.RouletteInfo.OnceLucky;
			((TextElement)m_lbDesc).text = string.Format(zbUtil.Language("lang.10665"), num);
			InitAwards();
			RefreshTarget();
			UpdateNotify();
			ChangeAnimPlay();
		}
	}

	private void InitAwards()
	{
		for (int i = 0; i < m_wndParent.m_data.RouletteInfo.SlotList.Count; i++)
		{
			AwardDetail awardDetail = m_wndParent.m_data.RouletteInfo.SlotList[i].AwardDetail;
			zbItemData zbItemData2 = new zbItemData(awardDetail.Type, awardDetail.Tid, awardDetail.Count);
			m_awardList[i].Refresh(zbItemData2, m_wndParent.m_data.RouletteInfo.SlotList[i].Rare);
		}
		((TextElement)m_lbOne).text = "x" + zbSelf.Instance.mechaSubjectHandler.rouletteCost;
		((TextElement)m_lbTen).text = "x" + zbSelf.Instance.mechaSubjectHandler.rouletteCost * 10;
	}

	private void RefreshTarget()
	{
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)m_lbCount).text = m_wndParent.m_data.RouletteInfo.TotalCount.ToString();
		for (int i = 0; i < 5; i++)
		{
			OperateTargetDetail operateTargetDetail = m_wndParent.m_data.RouletteInfo.TargetList[i];
			((TextElement)m_lbTargets[i]).text = operateTargetDetail.Target.ToString();
			AwardDetail awardDetail = operateTargetDetail.AwardList[0];
			if (awardDetail != null)
			{
				m_items[i].Refresh(new zbItemData(awardDetail.Type, awardDetail.Tid, awardDetail.Count));
			}
			if (operateTargetDetail.State == ECommonState.Yes)
			{
				m_items[i].SetTouchCallback(delegate
				{
					OnTargetAward();
				});
			}
			else
			{
				m_items[i].SetTouchCallback(null);
			}
			m_items[i].SetSpineVisible(operateTargetDetail.State == ECommonState.Yes);
			m_items[i].SetGettedVisible(operateTargetDetail.State == ECommonState.End);
		}
		m_spriteProgress.style.width = StyleLength.op_Implicit(new Length(GetProgress() * 536f, (LengthUnit)0));
	}

	private float GetProgress()
	{
		int count = m_wndParent.m_data.RouletteInfo.TargetList.Count;
		float num = 1f / (float)count;
		for (int i = 0; i < count; i++)
		{
			int target = m_wndParent.m_data.RouletteInfo.TargetList[i].Target;
			if (m_wndParent.m_data.RouletteInfo.TotalCount < target)
			{
				int num2 = ((i != 0) ? m_wndParent.m_data.RouletteInfo.TargetList[i - 1].Target : 0);
				float num3 = (float)(m_wndParent.m_data.RouletteInfo.TotalCount - num2) / (float)(target - num2);
				return Mathf.Clamp01((float)i * num + num3 * num);
			}
		}
		return 1f;
	}

	private void OnTargetAward()
	{
		if (!isRoration)
		{
			zbWsMsgHandler.Instance.Operate_MechaSubjectRouletteTarget_Req(m_wndParent.m_data.BaseInfo.UUID);
		}
	}

	public void Update()
	{
		UpdateNotify();
		if (isRoration && Time.time - startReqTime >= 3f)
		{
			isRoration = false;
		}
	}

	private void UpdateNotify()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		if (m_wndParent.m_data != null)
		{
			m_notifyOne.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.moneyHandler.HasEnoughMoney(35, zbSelf.Instance.mechaSubjectHandler.rouletteCost)));
			m_notifyTen.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.moneyHandler.HasEnoughMoney(35, 10 * zbSelf.Instance.mechaSubjectHandler.rouletteCost)));
		}
	}

	private void OnRate()
	{
		zbUIMgr.Instance.ShowLimitTimeRoulettProbability(((IEnumerable<LimitTimeRouletteRandDetail>)m_wndParent.m_data.RouletteInfo.SlotList).ToArray(), EOperateType.MechaSubjectRoulette);
	}

	private void OnOne()
	{
		if (!isRoration)
		{
			InitCountAndLight();
			if (zbSelf.Instance.moneyHandler.HasEnoughMoney(35, zbSelf.Instance.mechaSubjectHandler.rouletteCost))
			{
				zbWsMsgHandler.Instance.Operate_MechaSubjectRoulette_Req(m_wndParent.m_data.BaseInfo.UUID, 1);
				isRoration = true;
				startReqTime = Time.time;
			}
			else
			{
				zbUIMgr.Instance.ShowTip(EResourceType.Money, 35L, zbSelf.Instance.mechaSubjectHandler.rouletteCost);
			}
		}
	}

	private void OnTen()
	{
		if (!isRoration)
		{
			InitCountAndLight();
			if (zbSelf.Instance.moneyHandler.HasEnoughMoney(35, 10 * zbSelf.Instance.mechaSubjectHandler.rouletteCost))
			{
				zbWsMsgHandler.Instance.Operate_MechaSubjectRoulette_Req(m_wndParent.m_data.BaseInfo.UUID, 10);
				isRoration = true;
				startReqTime = Time.time;
			}
			else
			{
				zbUIMgr.Instance.ShowTip(EResourceType.Money, 35L, 10 * zbSelf.Instance.mechaSubjectHandler.rouletteCost);
			}
		}
	}

	private void InitCountAndLight()
	{
		Label[] lbCounts = m_lbCounts;
		for (int i = 0; i < lbCounts.Length; i++)
		{
			((VisualElement)lbCounts[i]).visible = false;
		}
	}

	private void ChangeAnimPlay()
	{
		m_spriteOk.visible = closeAnim;
	}

	private void OnToggle()
	{
		if (!isRoration)
		{
			closeAnim = !closeAnim;
			ChangeAnimPlay();
		}
	}

	private void OnMechaSubjectRouletteAck(object data)
	{
		if (data is OperateMechaSubjectRouletteAck operateMechaSubjectRouletteAck)
		{
			((MonoBehaviour)m_wndParent).StartCoroutine(RefreshDrawAck(((IEnumerable<int>)operateMechaSubjectRouletteAck.HitList).ToArray(), ((IEnumerable<AwardDetail>)operateMechaSubjectRouletteAck.AwardList).ToArray(), operateMechaSubjectRouletteAck.DataChange));
		}
		else if (data is AdvertisementMechaSubjectRouletteAck advertisementMechaSubjectRouletteAck)
		{
			((MonoBehaviour)m_wndParent).StartCoroutine(RefreshDrawAck(((IEnumerable<int>)advertisementMechaSubjectRouletteAck.HitList).ToArray(), ((IEnumerable<AwardDetail>)advertisementMechaSubjectRouletteAck.AwardList).ToArray(), advertisementMechaSubjectRouletteAck.DataChange));
		}
	}

	private void InitAnim()
	{
		zbMechaSubjectRouletteItem[] awardList = m_awardList;
		for (int i = 0; i < awardList.Length; i++)
		{
			awardList[i].ClearResult();
		}
	}

	private IEnumerator RefreshDrawAck(int[] hitList, AwardDetail[] awardList, DataChange dataChange)
	{
		InitAnim();
		if (!closeAnim)
		{
			zbAudioMgr.Instance.PlayAudio("audio/ui/rise_normal.ogg");
			for (int round = 0; round < 3; round++)
			{
				for (int index = 0; index < 12; index++)
				{
					yield return (object)new WaitForSeconds(m_intervalTime);
					m_awardList[index].PlayEffect();
				}
			}
		}
		int num = 0;
		int[] array = hitList;
		foreach (int num2 in array)
		{
			m_awardList[num2].PlayResult(num == 0);
			num++;
		}
		if (!closeAnim)
		{
			yield return (object)new WaitForSeconds(0.5f);
		}
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		array = hitList;
		foreach (int key in array)
		{
			if (dictionary.ContainsKey(key))
			{
				dictionary[key]++;
			}
			else
			{
				dictionary[key] = 1;
			}
		}
		foreach (KeyValuePair<int, int> item in dictionary)
		{
			if (item.Value > 1 && item.Key != 0)
			{
				((TextElement)m_lbCounts[item.Key - 1]).text = $"x{item.Value}";
				((VisualElement)m_lbCounts[item.Key - 1]).visible = true;
			}
		}
		zbSelf.Instance.dataHandler.DataChange(dataChange, awardList.ToArray());
		isRoration = false;
		Refresh();
	}
}
