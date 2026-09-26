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

public class zbLimitTimeRouletteWnd : zbBaseWnd
{
	private SkeletonAnimation[] m_spineChuChangs = (SkeletonAnimation[])(object)new SkeletonAnimation[4];

	private zbPlayerAvatar[] m_spineAvatars = new zbPlayerAvatar[4];

	private Button[] m_btnHeros = (Button[])(object)new Button[4];

	private static readonly int[] m_suitIds = new int[4] { 1061, 1091, 1081, 1051 };

	private static readonly int[] m_gunIds = new int[4] { 2061, 2091, 2081, 2051 };

	private Label m_lbTime;

	private Label m_lbName;

	private Label m_lbTitle;

	private Label m_lbCount;

	private Label m_lbDesc;

	private Button m_btnOne;

	private Label m_lbOne;

	private Button m_btnTen;

	private Label m_lbTen;

	private VisualElement m_notifyOne;

	private VisualElement m_notifyTen;

	private VisualElement m_nodeToggle;

	private VisualElement m_spriteOk;

	private Button m_btnBack;

	private Button m_btnProbability;

	private VisualElement m_nodeAwards;

	private VisualElement m_nodeCounts;

	private zbLimitTimeRouletteItem[] m_awardList = new zbLimitTimeRouletteItem[12];

	private Label[] m_lbCounts = (Label[])(object)new Label[11];

	private zbItem[] m_targetAwardList = new zbItem[4];

	private Label[] m_lbTargets = (Label[])(object)new Label[4];

	private VisualElement m_process;

	private bool closeAnim;

	private LimitTimeRouletteDetail m_data;

	private bool isRoration;

	private static float startReqTime = 0f;

	private float m_intervalTime = 0.02f;

	protected override void OnEnable()
	{
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0472: Unknown result type (might be due to invalid IL or missing references)
		//IL_0478: Invalid comparison between Unknown and I4
		//IL_048a: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_process = UQueryExtensions.Q<VisualElement>(m_RootElement, "progress", (string)null);
		m_lbCount = UQueryExtensions.Q<Label>(UQueryExtensions.Q<VisualElement>(m_RootElement, "target", (string)null), "lb_count", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_btnOne = UQueryExtensions.Q<Button>(m_RootElement, "btn_one", (string)null);
		m_lbOne = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnOne, "lb_cost", (string)null);
		m_btnTen = UQueryExtensions.Q<Button>(m_RootElement, "btn_ten", (string)null);
		m_lbTen = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnTen, "lb_cost", (string)null);
		m_notifyOne = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnOne, (Vector2?)new Vector2(2f, 0f), 1f, true);
		m_notifyTen = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnTen, (Vector2?)new Vector2(2f, 0f), 1f, true);
		m_btnBack = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_nodeToggle = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_toggle", (string)null);
		m_spriteOk = UQueryExtensions.Q<VisualElement>(m_nodeToggle, "sprite_ok", (string)null);
		m_btnProbability = UQueryExtensions.Q<Button>(m_RootElement, "btn_probability", (string)null);
		m_nodeAwards = UQueryExtensions.Q<VisualElement>(m_RootElement, "awardList", (string)null);
		for (int i = 0; i < 12; i++)
		{
			int index = i;
			m_awardList[i] = new zbLimitTimeRouletteItem();
			m_awardList[i] = UQueryExtensions.Q<zbLimitTimeRouletteItem>(m_nodeAwards, "item" + i, (string)null);
			m_awardList[i].OnEnable(this, index);
		}
		m_nodeCounts = UQueryExtensions.Q<VisualElement>(m_RootElement, "countList", (string)null);
		for (int j = 0; j < 11; j++)
		{
			m_lbCounts[j] = UQueryExtensions.Q<Label>(m_nodeCounts, "lb_count" + j, (string)null);
			((VisualElement)m_lbCounts[j]).visible = false;
		}
		zbUIEffect.CreateUpDownAction(m_nodeAwards, 10f, 2f);
		zbUIEffect.CreateUpDownAction(m_nodeCounts, 10f, 2f);
		for (int k = 0; k < 4; k++)
		{
			m_spineChuChangs[k] = ((Component)m_GameObjectSceneRoot.transform.Find("chuChang" + k)).GetComponent<SkeletonAnimation>();
			m_spineAvatars[k] = ((Component)m_GameObjectSceneRoot.transform.Find("playerAvatar" + k)).GetComponent<zbPlayerAvatar>();
			m_btnHeros[k] = UQueryExtensions.Q<Button>(m_RootElement, "btn_hero" + k, (string)null);
			int index2 = k;
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnHeros[k], (Action)delegate
			{
				OnHeros(index2);
			}, "", (TrickleDown)0);
			m_targetAwardList[k] = UQueryExtensions.Q<zbItem>(m_RootElement, "target" + k, (string)null);
			m_targetAwardList[k].OnEnable(this);
			m_lbTargets[k] = UQueryExtensions.Q<Label>(m_RootElement, "lb_target" + k, (string)null);
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
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Operate, OnUpdateEventOperate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_TimeRoulette_Ack, OnLimitTimeRouletteAck);
		InitSuitGun();
	}

	public void RefreshCustom(LimitTimeRouletteDetail data, bool isFirst = false)
	{
		m_data = data;
		((TextElement)m_lbTitle).text = zbUtil.Language("lang.11034");
		((TextElement)m_lbDesc).text = string.Format(zbUtil.Language("lang.10665"), (m_data.BaseData.MaxLucky - m_data.BaseData.Lucky) / data.BaseData.OnceLucky);
		((TextElement)m_lbOne).text = "x" + data.BaseData.OnceCost;
		((TextElement)m_lbTen).text = "x" + data.BaseData.TenCost;
		InitAwards();
		InitProcess();
		UpdateTime();
		UpdateNotify();
		ChangeAnimPlay();
		if (isFirst)
		{
			for (int i = 0; i < 4; i++)
			{
				((Component)m_spineChuChangs[i]).gameObject.SetActive(false);
				((Component)m_spineAvatars[i]).gameObject.SetActive(false);
			}
			((MonoBehaviour)this).StartCoroutine(ChuchangSequence());
		}
	}

	private IEnumerator ChuchangSequence()
	{
		zbUIMgr.Instance.ShowModel();
		for (int i = 0; i < 4; i++)
		{
			((MonoBehaviour)this).StartCoroutine(FadeIn(i));
			float num = ((i == 3) ? 0.18f : 0.1f);
			yield return (object)new WaitForSecondsRealtime(num);
		}
		yield return (object)new WaitForSecondsRealtime(0.3f);
		zbUIMgr.Instance.HideModel();
	}

	private IEnumerator FadeIn(int index)
	{
		((Component)m_spineChuChangs[index]).gameObject.SetActive(true);
		m_spineChuChangs[index].AnimationState.SetAnimation(0, "animation", false);
		yield return (object)new WaitForSecondsRealtime(0.15f);
		((Component)m_spineAvatars[index]).gameObject.SetActive(true);
	}

	private void InitSuitGun()
	{
		for (int i = 0; i < 4; i++)
		{
			zbPlayerSuitCsvRecord recordByKey = zbDataConfig.Instance.playerSuitCsvTable.GetRecordByKey(m_suitIds[i]);
			if (recordByKey != null)
			{
				m_spineAvatars[i].RefreshCustom(recordByKey.avatarId, m_gunIds[i]);
			}
		}
	}

	private void InitAwards()
	{
		for (int i = 0; i < m_data.RandList.Count; i++)
		{
			AwardDetail awardDetail = m_data.RandList[i].AwardDetail;
			zbItemData zbItemData2 = new zbItemData(awardDetail.Type, awardDetail.Tid, awardDetail.Count);
			m_awardList[i].Refresh(zbItemData2);
		}
	}

	private void InitProcess()
	{
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)m_lbCount).text = string.Format(zbUtil.Language("lang.10664"), m_data.BaseData.Count);
		for (int i = 0; i < 4; i++)
		{
			((TextElement)m_lbTargets[i]).text = m_data.TargetList[i].Target.ToString();
			AwardDetail awardDetail = m_data.TargetList[i].AwardList[0];
			OperateTargetDetail target = m_data.TargetList[i];
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
			if (m_data.BaseData.Count < target)
			{
				int num2 = ((i != 0) ? targetList[i - 1].Target : 0);
				float num3 = (float)(m_data.BaseData.Count - num2) / (float)(target - num2);
				return Mathf.Clamp01((float)i * num + num3 * num);
			}
		}
		return 1f;
	}

	private void OnTargetAward(ClickEvent evt, int index, ECommonState canReceive)
	{
		if (!isRoration && canReceive == ECommonState.Yes)
		{
			zbWsMsgHandler.Instance.Operate_LimitTimeRouletteTarget_Req(m_data.BaseInfo.UUID, index);
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
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		if (m_data != null)
		{
			m_notifyOne.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.moneyHandler.HasEnoughMoney(34, m_data.BaseData.OnceCost)));
			m_notifyTen.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.moneyHandler.HasEnoughMoney(34, m_data.BaseData.TenCost)));
		}
	}

	private void OnOne()
	{
		if (!isRoration)
		{
			InitCountAndLight();
			if (zbSelf.Instance.moneyHandler.HasEnoughMoney(34, m_data.BaseData.OnceCost))
			{
				zbWsMsgHandler.Instance.Operate_LimitTimeRouletteDraw_Req(m_data.BaseInfo.UUID, 1);
				isRoration = true;
				startReqTime = Time.time;
			}
			else
			{
				zbUIMgr.Instance.ShowTip(EResourceType.Money, 34L, m_data.BaseData.OnceCost);
			}
		}
	}

	private void OnTen()
	{
		if (!isRoration)
		{
			InitCountAndLight();
			if (zbSelf.Instance.moneyHandler.HasEnoughMoney(34, m_data.BaseData.TenCost))
			{
				zbWsMsgHandler.Instance.Operate_LimitTimeRouletteDraw_Req(m_data.BaseInfo.UUID, 10);
				isRoration = true;
				startReqTime = Time.time;
			}
			else
			{
				zbUIMgr.Instance.ShowTip(EResourceType.Money, 34L, m_data.BaseData.TenCost);
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

	private void OnHeros(int index)
	{
		zbPlayerSuitCsvRecord zbPlayerSuitCsvRecord2 = zbDataConfig.Instance.playerSuitCsvTable.GetRecords().records.Values.FirstOrDefault((zbPlayerSuitCsvRecord r) => r.id == m_suitIds[index]);
		if (zbPlayerSuitCsvRecord2 != null)
		{
			zbUIMgr.Instance.ShowTip(EResourceType.Prop, zbPlayerSuitCsvRecord2.propId, 1L);
		}
	}

	private void OnProbability()
	{
		zbUIMgr.Instance.ShowLimitTimeRoulettProbability(((IEnumerable<LimitTimeRouletteRandDetail>)m_data.RandList).ToArray());
	}

	private void ChangeAnimPlay()
	{
		m_spriteOk.visible = closeAnim;
	}

	private void OnClose()
	{
		if (!isRoration)
		{
			Hide();
		}
	}

	private void OnToggle()
	{
		if (!isRoration)
		{
			closeAnim = !closeAnim;
			ChangeAnimPlay();
		}
	}

	private void UpdateTime()
	{
		long serverTime = zbSelf.Instance.timeHandler.GetServerTime();
		long startTm = m_data.BaseInfo.StartTm;
		long endTm = m_data.BaseInfo.EndTm;
		bool flag = startTm < serverTime && serverTime < endTm;
		((TextElement)m_lbTime).text = (flag ? zbUtil.FormatTimeHHMMSS(endTm - serverTime) : zbUtil.Language("lang.10043"));
	}

	private void OnUpdateEventOperate(object data)
	{
		if (m_data != null)
		{
			LimitTimeRouletteDetail limitTimeRouletteInfoByUUID = zbSelf.Instance.operateHandler.GetLimitTimeRouletteInfoByUUID(m_data.BaseInfo.UUID);
			RefreshCustom(limitTimeRouletteInfoByUUID);
		}
	}

	private void OnLimitTimeRouletteAck(object data)
	{
		((MonoBehaviour)this).StartCoroutine(RefreshDrawAck());
	}

	private void InitAnim()
	{
		zbLimitTimeRouletteItem[] awardList = m_awardList;
		for (int i = 0; i < awardList.Length; i++)
		{
			awardList[i].ClearResult();
		}
	}

	private IEnumerator RefreshDrawAck()
	{
		if (zbSelf.Instance.operateHandler.timeRouletteAckData == null)
		{
			yield break;
		}
		OperateLimitTimeRouletteDrawAck awardData = zbSelf.Instance.operateHandler.timeRouletteAckData;
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
		List<LimitTimeRouletteRandDetail> list = new List<LimitTimeRouletteRandDetail>();
		foreach (AwardDetail award in awardData.AwardList)
		{
			foreach (LimitTimeRouletteRandDetail rand in m_data.RandList)
			{
				if (rand.AwardDetail.Type == award.Type && rand.AwardDetail.Count == award.Count && rand.AwardDetail.Tid == award.Tid)
				{
					list.Add(rand);
					break;
				}
			}
		}
		int num = 0;
		foreach (LimitTimeRouletteRandDetail item in list)
		{
			m_awardList[item.Index - 1].PlayResult(num == 0);
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
			if (item3.Value > 1 && item3.Key.Item1 != 1)
			{
				((TextElement)m_lbCounts[item3.Key.Item1 - 2]).text = $"x{item3.Value}";
				((VisualElement)m_lbCounts[item3.Key.Item1 - 2]).visible = true;
			}
		}
		zbSelf.Instance.dataHandler.DataChange(awardData.DataChange, ((IEnumerable<AwardDetail>)awardData.AwardList).ToArray());
		zbSelf.Instance.operateHandler.timeRouletteAckData = null;
		LimitTimeRouletteDetail limitTimeRouletteInfoByUUID = zbSelf.Instance.operateHandler.GetLimitTimeRouletteInfoByUUID(m_data.BaseInfo.UUID);
		isRoration = false;
		RefreshCustom(limitTimeRouletteInfoByUUID);
	}
}
