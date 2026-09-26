using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Collections;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbLimitTimeTreasureWnd : zbBaseWnd
{
	private zbSpineScene m_spineScene;

	private Label m_lbTime;

	private Label m_lbName;

	private Label m_lbTitle;

	private zbLimitTimeTreasureItem[] m_awardList = new zbLimitTimeTreasureItem[11];

	private Label[] m_lbCounts = (Label[])(object)new Label[11];

	private zbLimitTimeTreasureGrandPrizeItem m_grandPrize;

	private Button[] m_btnHeros = (Button[])(object)new Button[4];

	private zbItem[] m_targetAwardList = new zbItem[4];

	private Label[] m_lbTargets = (Label[])(object)new Label[4];

	private VisualElement m_process;

	private Label m_lbCount;

	private Label m_lbLucky;

	private VisualElement m_notifyOne;

	private VisualElement m_notifyTen;

	private Button m_btnBack;

	private Button m_btnOne;

	private Label m_lbOne;

	private Button m_btnTen;

	private Label m_lbTen;

	private VisualElement m_nodeToggle;

	private VisualElement m_spriteOk;

	private Button m_btnProbability;

	private bool closeAnim;

	private LimitTimeRouletteDetail m_detail;

	private bool isRoration;

	private static float startReqTime = 0f;

	private float m_intervalTime = 0.02f;

	private static readonly int[] m_propIds = new int[4] { 81131, 81041, 81141, 81101 };

	protected override void OnEnable()
	{
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ec: Invalid comparison between Unknown and I4
		//IL_03fe: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_spineScene = ((Component)m_GameObjectSceneRoot.transform.Find("spineScene")).GetComponent<zbSpineScene>();
		m_btnBack = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_grandPrize = new zbLimitTimeTreasureGrandPrizeItem();
		m_grandPrize = UQueryExtensions.Q<zbLimitTimeTreasureGrandPrizeItem>(m_RootElement, "grandPrize", (string)null);
		m_grandPrize.OnEnable(this);
		m_process = UQueryExtensions.Q<VisualElement>(m_RootElement, "progress", (string)null);
		m_lbCount = UQueryExtensions.Q<Label>(m_RootElement, "lb_count", (string)null);
		m_lbLucky = UQueryExtensions.Q<Label>(m_RootElement, "lb_lucky", (string)null);
		m_btnOne = UQueryExtensions.Q<Button>(m_RootElement, "btn_one", (string)null);
		m_lbOne = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnOne, "lb_cost", (string)null);
		m_btnTen = UQueryExtensions.Q<Button>(m_RootElement, "btn_ten", (string)null);
		m_lbTen = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnTen, "lb_cost", (string)null);
		m_notifyOne = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnOne, (Vector2?)new Vector2(2f, 0f), 1f, true);
		m_notifyTen = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnTen, (Vector2?)new Vector2(2f, 0f), 1f, true);
		m_nodeToggle = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_toggle", (string)null);
		m_spriteOk = UQueryExtensions.Q<VisualElement>(m_nodeToggle, "sprite_ok", (string)null);
		m_btnProbability = UQueryExtensions.Q<Button>(m_RootElement, "btn_probability", (string)null);
		for (int i = 0; i < 11; i++)
		{
			int index = i;
			m_awardList[i] = new zbLimitTimeTreasureItem();
			m_awardList[i] = UQueryExtensions.Q<zbLimitTimeTreasureItem>(m_RootElement, "item" + i, (string)null);
			m_awardList[i].OnEnable(this, index);
			m_lbCounts[i] = UQueryExtensions.Q<Label>(m_RootElement, "lb_count" + i, (string)null);
			((VisualElement)m_lbCounts[i]).visible = false;
		}
		for (int j = 0; j < 4; j++)
		{
			m_targetAwardList[j] = UQueryExtensions.Q<zbItem>(m_RootElement, "target" + j, (string)null);
			m_targetAwardList[j].OnEnable(this);
			m_lbTargets[j] = UQueryExtensions.Q<Label>(m_RootElement, "lb_target" + j, (string)null);
			m_btnHeros[j] = UQueryExtensions.Q<Button>(m_RootElement, "btn_hero" + j, (string)null);
			int index2 = j;
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnHeros[j], (Action)delegate
			{
				OnHeros(index2);
			}, "", (TrickleDown)0);
		}
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBack, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>(m_nodeToggle, (Action)OnToggle, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnOne, (Action)OnOne, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTen, (Action)OnTen, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnProbability, (Action)OnProbability, "", (TrickleDown)0);
		if ((int)Application.platform == 8)
		{
			((VisualElement)m_lbTime).style.top = StyleLength.op_Implicit(47f);
			CreateMoneyPanel(new EMoneyType[1] { EMoneyType.LimitTimeRoulette });
		}
		else
		{
			CreateMoneyPanel(new EMoneyType[2]
			{
				EMoneyType.Diamond,
				EMoneyType.LimitTimeRoulette
			});
		}
		m_gameEevent.RegisterEvent(EGameEvent.Event_LimitTimeTreasure_Ack, OnLimitTimeTreasureAck);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Operate, OnUpdateEventOperate);
	}

	public void RefreshCustom(LimitTimeRouletteDetail detail, bool isFirst = false)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (isFirst)
		{
			m_spineScene.PlaySequenceAnimation("in", "loop");
			((VisualElement)m_lbTitle).style.opacity = StyleFloat.op_Implicit(0f);
			((MonoBehaviour)this).StartCoroutine(PlayAnimationSequence());
		}
		m_detail = detail;
		InitTitle();
		ChangeAnimPlay();
		InitAwardList();
		InitProcess();
		InitLucky();
		InitBtns();
		UpdateTime();
		UpdateNotify();
	}

	private IEnumerator PlayAnimationSequence()
	{
		yield return (object)new WaitForSeconds(1f / 3f);
		((VisualElement)m_lbTitle).style.opacity = StyleFloat.op_Implicit(1f);
	}

	private void OnToggle()
	{
		if (!isRoration)
		{
			closeAnim = !closeAnim;
			ChangeAnimPlay();
		}
	}

	private void InitTitle()
	{
		zbOperateCsvRecord recordByKey = zbDataConfig.Instance.operateCsvTable.GetRecordByKey(9);
		((TextElement)m_lbName).text = zbUtil.Language(recordByKey.name);
	}

	private void InitBtns()
	{
		((TextElement)m_lbOne).text = "x" + m_detail.BaseData.OnceCost;
		((TextElement)m_lbTen).text = "x" + m_detail.BaseData.TenCost;
	}

	private void InitLucky()
	{
		((TextElement)m_lbLucky).text = string.Format(zbUtil.Language("lang.10665"), (m_detail.BaseData.MaxLucky - m_detail.BaseData.Lucky) / m_detail.BaseData.OnceLucky);
	}

	private void OnProbability()
	{
		zbUIMgr.Instance.ShowLimitTimeRoulettProbability(((IEnumerable<LimitTimeRouletteRandDetail>)m_detail.RandList).ToArray(), EOperateType.LimitTimeTreasure);
	}

	private void InitAwardList()
	{
		RepeatedField<LimitTimeRouletteRandDetail> randList = m_detail.RandList;
		List<zbItemData> list = new List<zbItemData>();
		foreach (LimitTimeRouletteRandDetail item in randList)
		{
			if (item.Rare == EBoolState.False)
			{
				list.Add(new zbItemData(item.AwardDetail.Type, item.AwardDetail.Tid, item.AwardDetail.Count));
				continue;
			}
			zbItemData zbItemData2 = new zbItemData(item.AwardDetail.Type, item.AwardDetail.Tid, item.AwardDetail.Count);
			m_grandPrize.Refresh(zbItemData2);
		}
		for (int i = 0; i < 11; i++)
		{
			m_awardList[i].Refresh(list[i]);
		}
	}

	private void InitProcess()
	{
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)m_lbCount).text = string.Format(zbUtil.Language("lang.10664"), m_detail.BaseData.Count);
		for (int i = 0; i < 4; i++)
		{
			((TextElement)m_lbTargets[i]).text = m_detail.TargetList[i].Target.ToString();
			AwardDetail awardDetail = m_detail.TargetList[i].AwardList[0];
			OperateTargetDetail target = m_detail.TargetList[i];
			zbItemData data = new zbItemData(awardDetail.Type, awardDetail.Tid, awardDetail.Count);
			m_targetAwardList[i].Refresh(data);
			if (target.State == ECommonState.End)
			{
				m_targetAwardList[i].SetGettedVisible(vis: true);
				m_targetAwardList[i].SetTouchCallback(null);
			}
			else if (target.State == ECommonState.Yes)
			{
				m_targetAwardList[i].SetTouchCallback(delegate(ClickEvent evt)
				{
					OnTargetAward(evt, target.Index, target.State);
				});
				m_targetAwardList[i].SetSpineVisible(vis: true);
			}
			else if (target.State == ECommonState.No)
			{
				m_targetAwardList[i].SetTouchCallback(null);
			}
		}
		GetProgress();
		m_process.style.width = StyleLength.op_Implicit(new Length(GetProgress() * 584f, (LengthUnit)0));
	}

	private float GetProgress()
	{
		RepeatedField<OperateTargetDetail> targetList = m_detail.TargetList;
		if (targetList == null || targetList.Count == 0)
		{
			return 0f;
		}
		int count = targetList.Count;
		float num = 1f / (float)count;
		for (int i = 0; i < count; i++)
		{
			int target = targetList[i].Target;
			if (m_detail.BaseData.Count < target)
			{
				int num2 = ((i != 0) ? targetList[i - 1].Target : 0);
				float num3 = (float)(m_detail.BaseData.Count - num2) / (float)(target - num2);
				return Mathf.Clamp01((float)i * num + num3 * num);
			}
		}
		return 1f;
	}

	private void OnTargetAward(ClickEvent evt, int index, ECommonState canReceive)
	{
		if (!isRoration && canReceive == ECommonState.Yes)
		{
			zbWsMsgHandler.Instance.Operate_LimitTimeTreasureTarget_Req(m_detail.BaseInfo.UUID, index);
		}
	}

	protected override void UpdatePerSecond()
	{
		UpdateTime();
		UpdateNotify();
		if (isRoration && Time.time - startReqTime >= 3f)
		{
			isRoration = false;
		}
	}

	private void UpdateNotify()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		m_notifyOne.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.moneyHandler.HasEnoughMoney(34, m_detail.BaseData.OnceCost)));
		m_notifyTen.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.moneyHandler.HasEnoughMoney(34, m_detail.BaseData.TenCost)));
	}

	private void UpdateTime()
	{
		base.UpdateAtShow();
		long serverTime = zbSelf.Instance.timeHandler.GetServerTime();
		long startTm = m_detail.BaseInfo.StartTm;
		long endTm = m_detail.BaseInfo.EndTm;
		bool flag = startTm < serverTime && serverTime < endTm;
		((TextElement)m_lbTime).text = (flag ? zbUtil.FormatTimeHHMMSS(endTm - serverTime) : zbUtil.Language("lang.10043"));
	}

	private void ChangeAnimPlay()
	{
		m_spriteOk.visible = closeAnim;
	}

	private void OnHeros(int index)
	{
		zbUIMgr.Instance.ShowTip(EResourceType.Prop, m_propIds[index], 1L);
	}

	private void OnOne()
	{
		if (!isRoration)
		{
			InitCountAndLight();
			if (zbSelf.Instance.moneyHandler.HasEnoughMoney(34, m_detail.BaseData.OnceCost))
			{
				zbWsMsgHandler.Instance.Operate_LimitTimeTreasureDraw_Req(m_detail.BaseInfo.UUID, 1);
				isRoration = true;
				startReqTime = Time.time;
			}
			else
			{
				zbUIMgr.Instance.ShowTip(EResourceType.Money, 34L, m_detail.BaseData.OnceCost);
			}
		}
	}

	private void InitCountAndLight()
	{
		for (int i = 0; i < 11; i++)
		{
			((VisualElement)m_lbCounts[i]).visible = false;
			m_awardList[i].SetLight(light: false);
		}
	}

	private void OnTen()
	{
		if (!isRoration)
		{
			InitCountAndLight();
			if (zbSelf.Instance.moneyHandler.HasEnoughMoney(34, m_detail.BaseData.TenCost))
			{
				zbWsMsgHandler.Instance.Operate_LimitTimeTreasureDraw_Req(m_detail.BaseInfo.UUID, 10);
				isRoration = true;
				startReqTime = Time.time;
			}
			else
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10257"));
				zbUIMgr.Instance.ShowTip(EResourceType.Money, 34L, m_detail.BaseData.TenCost);
			}
		}
	}

	private void OnUpdateEventOperate(object data)
	{
		if (m_detail != null)
		{
			LimitTimeRouletteDetail limitTimeTreasureInfoByUUID = zbSelf.Instance.operateHandler.GetLimitTimeTreasureInfoByUUID(m_detail.BaseInfo.UUID);
			RefreshCustom(limitTimeTreasureInfoByUUID);
		}
	}

	private void OnLimitTimeTreasureAck(object data)
	{
		((MonoBehaviour)this).StartCoroutine(RefreshDrawAck());
	}

	private void InitAnim()
	{
		m_grandPrize.ClearResult();
		for (int i = 0; i < 11; i++)
		{
			m_awardList[i].ClearResult();
		}
	}

	public IEnumerator RefreshDrawAck()
	{
		if (zbSelf.Instance.operateHandler.timeTreasureDrawAckData == null)
		{
			yield break;
		}
		OperateLimitTimeTreasureDrawAck awardData = zbSelf.Instance.operateHandler.timeTreasureDrawAckData;
		InitAnim();
		if (!closeAnim)
		{
			zbAudioMgr.Instance.PlayAudio("audio/ui/rise_normal.ogg");
			for (int i = 0; i < 36; i++)
			{
				yield return (object)new WaitForSeconds(m_intervalTime);
				int num = i % 12;
				if (num < m_awardList.Length)
				{
					m_awardList[num].PlayEffect();
				}
				else
				{
					m_grandPrize.PlayEffect();
				}
			}
		}
		List<LimitTimeRouletteRandDetail> list = new List<LimitTimeRouletteRandDetail>();
		foreach (AwardDetail award in awardData.AwardList)
		{
			foreach (LimitTimeRouletteRandDetail rand in m_detail.RandList)
			{
				if (rand.AwardDetail.Type == award.Type && rand.AwardDetail.Count == award.Count && rand.AwardDetail.Tid == award.Tid)
				{
					list.Add(rand);
					break;
				}
			}
		}
		int num2 = 0;
		foreach (LimitTimeRouletteRandDetail item in list)
		{
			if (item.Rare == EBoolState.True && item.Index == 1)
			{
				m_grandPrize.PlayResult(num2 == 0);
			}
			else
			{
				m_awardList[item.Index - 2].PlayResult(num2 == 0);
			}
			num2++;
		}
		if (!closeAnim)
		{
			yield return (object)new WaitForSeconds(0.5f);
		}
		Dictionary<(int, long, long, int), int> dictionary = new Dictionary<(int, long, long, int), int>();
		foreach (LimitTimeRouletteRandDetail item2 in list)
		{
			(int, long, long, int) key = (item2.Index, item2.AwardDetail.Tid, item2.AwardDetail.Count, (int)item2.AwardDetail.Type);
			if (dictionary.ContainsKey(key))
			{
				dictionary[key]++;
			}
			else
			{
				dictionary[key] = 1;
			}
		}
		foreach (KeyValuePair<(int, long, long, int), int> item3 in dictionary)
		{
			if (item3.Value > 1)
			{
				((VisualElement)m_lbCounts[item3.Key.Item1 - 2]).visible = true;
				((TextElement)m_lbCounts[item3.Key.Item1 - 2]).text = $"x{item3.Value}";
				m_awardList[item3.Key.Item1 - 2].SetLight(light: true);
			}
		}
		zbSelf.Instance.dataHandler.DataChange(awardData.DataChange, ((IEnumerable<AwardDetail>)awardData.AwardList).ToArray());
		zbSelf.Instance.operateHandler.timeTreasureDrawAckData = null;
		LimitTimeRouletteDetail limitTimeTreasureInfoByUUID = zbSelf.Instance.operateHandler.GetLimitTimeTreasureInfoByUUID(m_detail.BaseInfo.UUID);
		isRoration = false;
		RefreshCustom(limitTimeTreasureInfoByUUID);
	}

	private void OnClose()
	{
		if (!isRoration)
		{
			Hide();
		}
	}
}
