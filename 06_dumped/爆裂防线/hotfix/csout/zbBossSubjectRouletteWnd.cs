using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbBossSubjectRouletteWnd
{
	private zbBossSubjectWnd m_wndParent;

	private VisualElement m_RootElement;

	private Label m_lbTime;

	private Label m_lbName;

	private Label m_lbTitle;

	private Label m_lbDesc;

	private Button m_btnRate;

	private Button m_btnAvatar;

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

	private zbBossSubjectRouletteItem[] m_awardList = new zbBossSubjectRouletteItem[12];

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

	private BossSubjectDetail m_data;

	public zbBossSubjectCsvRecord bossSubjectRecord;

	public bool isRoration;

	private static float startReqTime;

	private float m_intervalTime = 0.02f;

	private SpriteRenderer sceneBg;

	private SkeletonAnimation skeletonAnimation;

	private SpineVisualElement m_spine;

	public zbPlayerAvatar m_spineAvatar;

	private bool firstIn = true;

	public void OnEnable(zbBossSubjectWnd wndParent, VisualElement root)
	{
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		m_RootElement = root;
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_btnRate = UQueryExtensions.Q<Button>(m_RootElement, "btn_rate", (string)null);
		m_btnAvatar = UQueryExtensions.Q<Button>(m_RootElement, "btn_avatar", (string)null);
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
			m_awardList[i] = new zbBossSubjectRouletteItem();
			m_awardList[i] = UQueryExtensions.Q<zbBossSubjectRouletteItem>(m_nodeAwards, "item" + i, (string)null);
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
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAvatar, (Action)OnAvatar, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnOne, (Action)OnOne, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTen, (Action)OnTen, "", (TrickleDown)0);
		Transform val = m_wndParent.GetMyGameObjectSceneRoot().transform.Find("rouletteObject");
		m_spineAvatar = ((Component)val.Find("playerAvatar")).GetComponent<zbPlayerAvatar>();
		sceneBg = ((Component)val.Find("spineScene").Find("SceneBg")).GetComponent<SpriteRenderer>();
		GameObject gameObject = ((Component)val.Find("spineScene").Find("SkeletonAnimation")).gameObject;
		skeletonAnimation = gameObject.GetComponent<SkeletonAnimation>();
		m_spine = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "SpineVisualElement", (string)null);
		m_wndParent.m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Money, OnUpdateEventOperate);
		m_wndParent.m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Operate, OnUpdateEventOperate);
		m_wndParent.m_gameEevent.RegisterEvent(EGameEvent.Event_BossSubRoulette_Ack, OnBossSubRouletteAck);
		Refresh();
	}

	public void Refresh()
	{
		firstIn = true;
		Init();
	}

	private void Init()
	{
		zbSelf.Instance.bossSubjectHandler.SetSpineVisualElement(m_spine, "bossSubjectZhiyuan");
		zbSelf.Instance.bossSubjectHandler.SetVisualElementSprite((VisualElement)(object)m_lbTitle, "bossSubject_rouletteTitle.png");
		zbSelf.Instance.bossSubjectHandler.SetVisualElementSprite((VisualElement)(object)m_lbDesc, "roulette_label.png");
		zbSelf.Instance.bossSubjectHandler.SetGameObjectSprite(sceneBg, "bossSubject_roulette.jpg");
		zbSelf.Instance.bossSubjectHandler.SetSkeletonAnimation(skeletonAnimation, "bossSubjectZhuanpanbg");
	}

	public void RefreshCustom(BossSubjectDetail data, zbBossSubjectCsvRecord _bossSubjectRecord)
	{
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		m_data = data;
		bossSubjectRecord = _bossSubjectRecord;
		((TextElement)m_lbTitle).text = zbUtil.Language("lang.11034");
		m_spineAvatar.RefreshCustom(bossSubjectRecord.suitAvatarId, bossSubjectRecord.gunId);
		zbSelf.Instance.suitGunHandler.SetSuperSuitEnterAnimation(m_spineAvatar, firstIn);
		firstIn = false;
		((TextElement)m_lbOne).text = "x" + zbSelf.Instance.bossSubjectHandler.rouletteCost;
		((TextElement)m_lbTen).text = "x" + zbSelf.Instance.bossSubjectHandler.rouletteCost * 10;
		bool flag = zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Money, 35, zbSelf.Instance.bossSubjectHandler.rouletteCost);
		((VisualElement)m_lbOne).style.color = StyleColor.op_Implicit(flag ? Color.white : Color.red);
		bool flag2 = zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Money, 35, zbSelf.Instance.bossSubjectHandler.rouletteCost * 10);
		((VisualElement)m_lbTen).style.color = StyleColor.op_Implicit(flag2 ? Color.white : Color.red);
		int num = (m_data.RouletteInfo.MaxLucky - m_data.RouletteInfo.Lucky + m_data.RouletteInfo.OnceLucky - 1) / m_data.RouletteInfo.OnceLucky;
		((TextElement)m_lbDesc).text = string.Format(zbUtil.Language("lang.10665"), num);
		InitAwards();
		RefreshTarget();
		UpdateTime();
		UpdateNotify();
		ChangeAnimPlay();
	}

	private void InitAwards()
	{
		for (int i = 0; i < m_data.RouletteInfo.SlotList.Count; i++)
		{
			AwardDetail awardDetail = m_data.RouletteInfo.SlotList[i].AwardDetail;
			zbItemData zbItemData2 = new zbItemData(awardDetail.Type, awardDetail.Tid, awardDetail.Count);
			m_awardList[i].Refresh(zbItemData2, m_data.RouletteInfo.SlotList[i].Rare);
		}
	}

	private void RefreshTarget()
	{
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)m_lbCount).text = m_data.RouletteInfo.Count.ToString();
		for (int i = 0; i < 5; i++)
		{
			OperateTargetDetail operateTargetDetail = m_data.RouletteInfo.TargetList[i];
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
		int count = m_data.RouletteInfo.TargetList.Count;
		float num = 1f / (float)count;
		for (int i = 0; i < count; i++)
		{
			int target = m_data.RouletteInfo.TargetList[i].Target;
			if (m_data.RouletteInfo.Count < target)
			{
				int num2 = ((i != 0) ? m_data.RouletteInfo.TargetList[i - 1].Target : 0);
				float num3 = (float)(m_data.RouletteInfo.Count - num2) / (float)(target - num2);
				return Mathf.Clamp01((float)i * num + num3 * num);
			}
		}
		return 1f;
	}

	private void OnTargetAward()
	{
		if (!isRoration)
		{
			zbWsMsgHandler.Instance.Operate_BossSubRouletteTarget_Req(m_data.BaseInfo.UUID);
		}
	}

	public void UpdateAtShow()
	{
	}

	public void UpdatePerSecond()
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
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		if (m_data != null)
		{
			m_notifyOne.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.moneyHandler.HasEnoughMoney(35, zbSelf.Instance.bossSubjectHandler.rouletteCost)));
			m_notifyTen.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.moneyHandler.HasEnoughMoney(35, zbSelf.Instance.bossSubjectHandler.rouletteCost * 10)));
		}
	}

	private void OnRate()
	{
		zbUIMgr.Instance.ShowLimitTimeRoulettProbability(((IEnumerable<LimitTimeRouletteRandDetail>)m_data.RouletteInfo.SlotList).ToArray(), EOperateType.BossSubjectRoulette);
	}

	private void OnAvatar()
	{
		m_spineAvatar.SetAnimation(0, "show", loop: false);
		m_spineAvatar.AddAnimation(0, "idle", loop: true);
	}

	private void OnOne()
	{
		if (!isRoration)
		{
			InitCountAndLight();
			if (zbSelf.Instance.moneyHandler.HasEnoughMoney(35, zbSelf.Instance.bossSubjectHandler.rouletteCost))
			{
				zbWsMsgHandler.Instance.Operate_BossSubRoulette_Req(m_data.BaseInfo.UUID, 1);
				isRoration = true;
				startReqTime = Time.time;
			}
			else
			{
				zbUIMgr.Instance.ShowTip(EResourceType.Money, 35L, zbSelf.Instance.bossSubjectHandler.rouletteCost);
			}
		}
	}

	private void OnTen()
	{
		if (!isRoration)
		{
			InitCountAndLight();
			if (zbSelf.Instance.moneyHandler.HasEnoughMoney(35, zbSelf.Instance.bossSubjectHandler.rouletteCost * 10))
			{
				zbWsMsgHandler.Instance.Operate_BossSubRoulette_Req(m_data.BaseInfo.UUID, 10);
				isRoration = true;
				startReqTime = Time.time;
			}
			else
			{
				zbUIMgr.Instance.ShowTip(EResourceType.Money, 35L, zbSelf.Instance.bossSubjectHandler.rouletteCost * 10);
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
			BossSubjectDetail bossSubjectDetail = zbSelf.Instance.bossSubjectHandler.GetBossSubjectDetail();
			RefreshCustom(bossSubjectDetail, bossSubjectRecord);
		}
	}

	private void OnBossSubRouletteAck(object data)
	{
		if (data is OperateBossSubRouletteAck operateBossSubRouletteAck)
		{
			((MonoBehaviour)m_wndParent).StartCoroutine(RefreshDrawAck(((IEnumerable<int>)operateBossSubRouletteAck.HitList).ToArray(), ((IEnumerable<AwardDetail>)operateBossSubRouletteAck.AwardList).ToArray(), operateBossSubRouletteAck.DataChange));
		}
		else if (data is AdvertisementBossSubRouletteAck advertisementBossSubRouletteAck)
		{
			((MonoBehaviour)m_wndParent).StartCoroutine(RefreshDrawAck(((IEnumerable<int>)advertisementBossSubRouletteAck.HitList).ToArray(), ((IEnumerable<AwardDetail>)advertisementBossSubRouletteAck.AwardList).ToArray(), advertisementBossSubRouletteAck.DataChange));
		}
	}

	private void InitAnim()
	{
		zbBossSubjectRouletteItem[] awardList = m_awardList;
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
		BossSubjectDetail bossSubjectDetail = zbSelf.Instance.bossSubjectHandler.GetBossSubjectDetail();
		isRoration = false;
		RefreshCustom(bossSubjectDetail, bossSubjectRecord);
	}
}
