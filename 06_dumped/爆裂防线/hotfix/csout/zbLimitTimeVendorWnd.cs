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
using zbType;

public class zbLimitTimeVendorWnd : zbBaseWnd
{
	private SkeletonAnimation[] m_spineChuChangs = (SkeletonAnimation[])(object)new SkeletonAnimation[4];

	private zbPlayerAvatar[] m_spineAvatars = new zbPlayerAvatar[4];

	private Button[] m_btnHeros = (Button[])(object)new Button[4];

	private static readonly int[] m_suitIds = new int[4] { 1031, 1071, 1121, 1111 };

	private static readonly int[] m_gunIds = new int[4] { 2031, 2071, 2121, 2111 };

	private Label m_lbName;

	private Label m_lbTitle;

	private Label m_lbTime;

	private Label m_lbCountVal;

	private Label m_lbLuckyVal;

	private Button m_btnOne;

	private Button m_btnTen;

	private Label m_lbOne;

	private Label m_lbTen;

	private VisualElement m_notifyOne;

	private VisualElement m_notifyTen;

	private VisualElement m_nodeToggle;

	private VisualElement m_spriteOk;

	private Button m_btnBack;

	private Button m_btnProbability;

	private VisualElement m_nodeAwards;

	private VisualElement m_nodeCounts;

	private zbLimitTimeVendorItem[] m_awardList = new zbLimitTimeVendorItem[12];

	private zbItem[] m_targetList = new zbItem[4];

	private Label[] m_lbTargets = (Label[])(object)new Label[4];

	private Label[] m_lbCounts = (Label[])(object)new Label[11];

	private VisualElement m_processVal;

	private bool closeAnim;

	private LimitTimeRouletteDetail m_detail;

	private bool isRoration;

	private static float startReqTime = 0f;

	private float m_intervalTime = 0.08f;

	protected override void OnEnable()
	{
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0467: Unknown result type (might be due to invalid IL or missing references)
		//IL_046d: Invalid comparison between Unknown and I4
		//IL_047f: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_lbCountVal = UQueryExtensions.Q<Label>(m_RootElement, "lb_countVal", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_lbLuckyVal = UQueryExtensions.Q<Label>(m_RootElement, "lb_luckyVal", (string)null);
		m_btnOne = UQueryExtensions.Q<Button>(m_RootElement, "btn_one", (string)null);
		m_lbOne = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnOne, "lb_cost", (string)null);
		m_btnTen = UQueryExtensions.Q<Button>(m_RootElement, "btn_ten", (string)null);
		m_lbTen = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnTen, "lb_cost", (string)null);
		m_notifyOne = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnOne, (Vector2?)new Vector2(2f, 0f), 1f, true);
		m_notifyTen = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnTen, (Vector2?)new Vector2(2f, 0f), 1f, true);
		m_nodeToggle = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_toggle", (string)null);
		m_spriteOk = UQueryExtensions.Q<VisualElement>(m_nodeToggle, "sprite_ok", (string)null);
		m_btnBack = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_btnProbability = UQueryExtensions.Q<Button>(m_RootElement, "btn_probability", (string)null);
		m_processVal = UQueryExtensions.Q<VisualElement>(m_RootElement, "processVal", (string)null);
		m_nodeAwards = UQueryExtensions.Q<VisualElement>(m_RootElement, "awardList", (string)null);
		m_nodeCounts = UQueryExtensions.Q<VisualElement>(m_RootElement, "countList", (string)null);
		for (int i = 0; i < 12; i++)
		{
			int index = i;
			m_awardList[i] = new zbLimitTimeVendorItem();
			m_awardList[i] = UQueryExtensions.Q<zbLimitTimeVendorItem>(m_nodeAwards, "item" + i, (string)null);
			m_awardList[i].OnEnable(this, index);
		}
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
			m_targetList[k] = UQueryExtensions.Q<zbItem>(m_RootElement, "target" + k, (string)null);
			m_targetList[k].OnEnable(this);
			m_lbTargets[k] = UQueryExtensions.Q<Label>(m_RootElement, "lb_target" + k, (string)null);
		}
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnOne, (Action)OnOne, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTen, (Action)OnTen, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>(m_nodeToggle, (Action)OnToggle, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBack, (Action)OnClose, "", (TrickleDown)0);
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
		m_gameEevent.RegisterEvent(EGameEvent.Event_LimitTimeVendor_Ack, OnLimitTimeVendorAck);
		InitSuitGun();
	}

	public void RefreshCustom(LimitTimeRouletteDetail detail, bool isFirst = false)
	{
		m_detail = detail;
		InitTitle();
		InitBtns();
		InitAwardList();
		InitTargetList();
		InitLucky();
		UpdateRedDot();
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

	private void InitBtns()
	{
		((TextElement)m_lbOne).text = "x" + m_detail.BaseData.OnceCost;
		((TextElement)m_lbTen).text = "x" + m_detail.BaseData.TenCost;
	}

	private void InitTitle()
	{
		zbOperateCsvRecord recordByKey = zbDataConfig.Instance.operateCsvTable.GetRecordByKey(36);
		((TextElement)m_lbName).text = zbUtil.Language(recordByKey.name);
	}

	private void InitLucky()
	{
		((TextElement)m_lbLuckyVal).text = string.Format(zbUtil.Language("lang.10665"), (m_detail.BaseData.MaxLucky - m_detail.BaseData.Lucky) / m_detail.BaseData.OnceLucky);
	}

	private void InitAwardList()
	{
		List<zbItemData> list = new List<zbItemData>();
		foreach (LimitTimeRouletteRandDetail rand in m_detail.RandList)
		{
			list.Add(new zbItemData(rand.AwardDetail.Type, rand.AwardDetail.Tid, rand.AwardDetail.Count));
		}
		for (int i = 0; i < 12; i++)
		{
			m_awardList[i].Refresh(list[i]);
		}
	}

	private void InitTargetList()
	{
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)m_lbCountVal).text = m_detail.BaseData.Count.ToString();
		for (int i = 0; i < 4; i++)
		{
			((TextElement)m_lbTargets[i]).text = m_detail.TargetList[i].Target.ToString();
			AwardDetail awardDetail = m_detail.TargetList[i].AwardList[0];
			OperateTargetDetail target = m_detail.TargetList[i];
			zbItemData data = new zbItemData(awardDetail.Type, awardDetail.Tid, awardDetail.Count);
			m_targetList[i].Refresh(data);
			if (target.State == ECommonState.End)
			{
				m_targetList[i].SetGettedVisible(vis: true);
				m_targetList[i].SetTouchCallback(null);
			}
			else if (target.State == ECommonState.Yes)
			{
				m_targetList[i].SetTouchCallback(delegate(ClickEvent evt)
				{
					OnTargetAward(evt, target.Index, target.State);
				});
				m_targetList[i].SetSpineVisible(vis: true);
			}
			else if (target.State == ECommonState.No)
			{
				m_targetList[i].SetTouchCallback(null);
			}
			GetProgress();
			m_processVal.style.height = StyleLength.op_Implicit(new Length(GetProgress() * 484f, (LengthUnit)0));
		}
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
			zbWsMsgHandler.Instance.Operate_LimitTimeVendorTarget_Req(m_detail.BaseInfo.UUID, index);
		}
	}

	private void OnOne()
	{
		if (!isRoration)
		{
			InitCountAndLight();
			if (zbSelf.Instance.moneyHandler.HasEnoughMoney(34, m_detail.BaseData.OnceCost))
			{
				zbWsMsgHandler.Instance.Operate_LimitTimeVendorDraw_Req(m_detail.BaseInfo.UUID, 1);
				isRoration = true;
				startReqTime = Time.time;
			}
			else
			{
				zbUIMgr.Instance.ShowTip(EResourceType.Money, 34L, m_detail.BaseData.OnceCost);
			}
		}
	}

	private void OnTen()
	{
		if (!isRoration)
		{
			InitCountAndLight();
			if (zbSelf.Instance.moneyHandler.HasEnoughMoney(34, m_detail.BaseData.TenCost))
			{
				zbWsMsgHandler.Instance.Operate_LimitTimeVendorDraw_Req(m_detail.BaseInfo.UUID, 10);
				isRoration = true;
				startReqTime = Time.time;
			}
			else
			{
				zbUIMgr.Instance.ShowTip(EResourceType.Money, 34L, m_detail.BaseData.TenCost);
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

	private void OnToggle()
	{
		if (!isRoration)
		{
			closeAnim = !closeAnim;
			ChangeAnimPlay();
		}
	}

	private void OnProbability()
	{
		zbUIMgr.Instance.ShowLimitTimeRoulettProbability(((IEnumerable<LimitTimeRouletteRandDetail>)m_detail.RandList).ToArray(), EOperateType.LimitTimeVendor);
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

	protected override void UpdateAtShow()
	{
		UpdateTime();
		UpdateRedDot();
		if (isRoration && Time.time - startReqTime >= 3f)
		{
			isRoration = false;
		}
	}

	private void UpdateRedDot()
	{
		if (m_detail != null)
		{
			m_notifyOne.visible = zbSelf.Instance.moneyHandler.HasEnoughMoney(34, m_detail.BaseData.OnceCost);
			m_notifyTen.visible = zbSelf.Instance.moneyHandler.HasEnoughMoney(34, m_detail.BaseData.TenCost);
		}
	}

	private void OnUpdateEventOperate(object data)
	{
		if (m_detail != null)
		{
			LimitTimeRouletteDetail limitTimeVendorInfoByUUID = zbSelf.Instance.operateHandler.GetLimitTimeVendorInfoByUUID(m_detail.BaseInfo.UUID);
			RefreshCustom(limitTimeVendorInfoByUUID);
		}
	}

	private void OnLimitTimeVendorAck(object data)
	{
		((MonoBehaviour)this).StartCoroutine(RefreshDrawAck());
	}

	private void InitAnim()
	{
		zbLimitTimeVendorItem[] awardList = m_awardList;
		for (int i = 0; i < awardList.Length; i++)
		{
			awardList[i].ClearResult();
		}
	}

	private IEnumerator RefreshDrawAck()
	{
		if (zbSelf.Instance.operateHandler.timeVendorDrawData == null)
		{
			yield break;
		}
		OperateLimitTimeVendorDrawAck awardData = zbSelf.Instance.operateHandler.timeVendorDrawData;
		InitAnim();
		if (!closeAnim)
		{
			zbAudioMgr.Instance.PlayAudio("audio/ui/rise_normal.ogg");
			for (int round = 0; round < 3; round++)
			{
				List<int> list = Enumerable.Range(0, 12).ToList();
				for (int num = list.Count - 1; num > 0; num--)
				{
					int num2 = Rand.Range(0, num + 1);
					int index = num;
					List<int> list2 = list;
					int index2 = num2;
					int value = list[num2];
					int value2 = list[num];
					list[index] = value;
					list2[index2] = value2;
				}
				foreach (int index3 in list)
				{
					yield return (object)new WaitForSeconds(m_intervalTime);
					m_awardList[index3].PlayEffect();
				}
			}
		}
		List<LimitTimeRouletteRandDetail> list3 = new List<LimitTimeRouletteRandDetail>();
		foreach (AwardDetail award in awardData.AwardList)
		{
			foreach (LimitTimeRouletteRandDetail rand in m_detail.RandList)
			{
				if (rand.AwardDetail.Type == award.Type && rand.AwardDetail.Count == award.Count && rand.AwardDetail.Tid == award.Tid)
				{
					list3.Add(rand);
					break;
				}
			}
		}
		int num3 = 0;
		foreach (LimitTimeRouletteRandDetail item in list3)
		{
			m_awardList[item.Index - 1].PlayResult(num3 == 0);
			num3++;
		}
		if (!closeAnim)
		{
			yield return (object)new WaitForSeconds(0.5f);
		}
		Dictionary<(int, long, long, int), int> dictionary = new Dictionary<(int, long, long, int), int>();
		foreach (LimitTimeRouletteRandDetail item2 in list3)
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
		zbSelf.Instance.operateHandler.timeVendorDrawData = null;
		LimitTimeRouletteDetail limitTimeVendorInfoByUUID = zbSelf.Instance.operateHandler.GetLimitTimeVendorInfoByUUID(m_detail.BaseInfo.UUID);
		isRoration = false;
		RefreshCustom(limitTimeVendorInfoByUUID);
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
}
