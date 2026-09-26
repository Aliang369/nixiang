using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbJourneyOneBarWnd : zbBaseWnd
{
	private SkeletonAnimation m_point;

	private SkeletonAnimation[] m_spineChuChangs = (SkeletonAnimation[])(object)new SkeletonAnimation[4];

	private zbPlayerAvatar[] m_spineAvatars = new zbPlayerAvatar[4];

	private Button[] m_btnHeros = (Button[])(object)new Button[4];

	private static readonly int[] m_suitIds = new int[4] { 1061, 1091, 1081, 1051 };

	private static readonly int[] m_gunIds = new int[4] { 2061, 2091, 2081, 2051 };

	private Label m_lbTime;

	private Label m_lbRareHit;

	private VisualElement m_nodeAwards;

	private zbItem[] m_awards = new zbItem[12];

	private VisualElement m_nodeTarget;

	private Button m_btnTipDesc;

	private Label m_lbTipDesc;

	private AutoHideTipController m_tipController;

	private Label m_lbCount;

	private VisualElement m_spriteProgress;

	private Label[] m_lbTargets = (Label[])(object)new Label[5];

	private zbItem[] m_items = new zbItem[5];

	private Button m_btnOne;

	private Label m_lbOne;

	private Button m_btnTen;

	private Label m_lbTen;

	private VisualElement m_nodeToggle;

	private VisualElement m_spriteOk;

	private Button m_btnRate;

	private Button m_btnClose;

	private VisualElement m_notifyOne;

	private VisualElement m_notifyTen;

	private string m_bigAwardName = "";

	private Dictionary<int, zbItemData> m_awardsDictionary = new Dictionary<int, zbItemData>();

	private bool m_bSkipAnim;

	private Coroutine m_animCoroutine;

	private List<AwardDetail> m_ackAwardList;

	private DataChange m_ackDataChange;

	private bool m_bExeciing;

	private bool m_bImmediateOver;

	protected override void OnEnable()
	{
		//IL_03f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0424: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_point = ((Component)m_GameObjectSceneRoot.transform.Find("point")).GetComponent<SkeletonAnimation>();
		((Component)m_point).gameObject.SetActive(false);
		for (int i = 0; i < 4; i++)
		{
			m_spineChuChangs[i] = ((Component)m_GameObjectSceneRoot.transform.Find("chuChang" + i)).GetComponent<SkeletonAnimation>();
			m_spineAvatars[i] = ((Component)m_GameObjectSceneRoot.transform.Find("playerAvatar" + i)).GetComponent<zbPlayerAvatar>();
			m_btnHeros[i] = UQueryExtensions.Q<Button>(m_RootElement, "btn_hero" + i, (string)null);
			int index = i;
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnHeros[i], (Action)delegate
			{
				OnHeros(index);
			}, "", (TrickleDown)0);
		}
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_lbRareHit = UQueryExtensions.Q<Label>(m_RootElement, "lb_rareHit", (string)null);
		m_nodeAwards = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_awards", (string)null);
		for (int num = 0; num < 12; num++)
		{
			m_awards[num] = UQueryExtensions.Q<zbItem>(m_nodeAwards, "award" + num, (string)null);
			m_awards[num].OnEnable(this);
		}
		m_nodeTarget = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_target", (string)null);
		m_btnTipDesc = UQueryExtensions.Q<Button>(m_nodeTarget, "btn_tipDesc", (string)null);
		m_lbTipDesc = UQueryExtensions.Q<Label>(m_nodeTarget, "lb_tipDesc", (string)null);
		m_tipController = new AutoHideTipController(m_uiEvent, m_btnTipDesc, m_lbTipDesc);
		m_lbCount = UQueryExtensions.Q<Label>(m_nodeTarget, "lb_count", (string)null);
		m_spriteProgress = UQueryExtensions.Q<VisualElement>(m_nodeTarget, "progress", (string)null);
		for (int num2 = 0; num2 < 5; num2++)
		{
			m_lbTargets[num2] = UQueryExtensions.Q<Label>(m_nodeTarget, "lb_target" + num2, (string)null);
			m_items[num2] = UQueryExtensions.Q<zbItem>(m_nodeTarget, "item" + num2, (string)null);
			m_items[num2].OnEnable(this);
		}
		m_btnOne = UQueryExtensions.Q<Button>(m_RootElement, "btn_one", (string)null);
		m_lbOne = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnOne, "lb_cost", (string)null);
		m_btnTen = UQueryExtensions.Q<Button>(m_RootElement, "btn_ten", (string)null);
		m_lbTen = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnTen, "lb_cost", (string)null);
		m_nodeToggle = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_toggle", (string)null);
		m_spriteOk = UQueryExtensions.Q<VisualElement>(m_nodeToggle, "sprite_ok", (string)null);
		m_btnRate = UQueryExtensions.Q<Button>(m_RootElement, "btn_rate", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnOne, (Action<ClickEvent>)delegate
		{
			OnBar(1);
		}, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTen, (Action<ClickEvent>)delegate
		{
			OnBar(10);
		}, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>(m_nodeToggle, (Action)OnToggle, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRate, (Action)OnRate, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_notifyOne = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnOne, (Vector2?)new Vector2(2f, 0f), 1f, true);
		m_notifyTen = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnTen, (Vector2?)new Vector2(2f, 0f), 1f, true);
		CreateMoneyPanel(new EMoneyType[2]
		{
			EMoneyType.Diamond,
			EMoneyType.LimitTimeRoulette
		});
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Money, OnUpdateEvent);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_JourneyInfo, OnUpdateEvent);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Journey_ChapterOneBarDraw_Ack, OnEventBarDrawAck);
		InitRecord();
		InitSuitGun();
	}

	private void InitRecord()
	{
		zbChapterOneBarPoolCsvRecord zbChapterOneBarPoolCsvRecord2 = zbDataConfig.Instance.chapterOneBarPoolCsvTable.GetRecords()?.records.FirstOrDefault((zbChapterOneBarPoolCsvRecord r) => r.poolType == 4);
		if (zbChapterOneBarPoolCsvRecord2 != null)
		{
			m_bigAwardName = zbResourceHelper.GetResorceIconPath((EResourceType)zbChapterOneBarPoolCsvRecord2.type, zbChapterOneBarPoolCsvRecord2.id).strName;
		}
		zbChapterOneBarProbabilityCsvTableData records = zbDataConfig.Instance.chapterOneBarProbabilityCsvTable.GetRecords();
		int[] array = new int[12]
		{
			0, 1, 2, 4, 10, 7, 9, 3, 6, 8,
			5, 11
		};
		for (int num = 0; num < array.Length; num++)
		{
			int num2 = array[num];
			if (num2 < records.records.Length)
			{
				zbChapterOneBarProbabilityCsvRecord zbChapterOneBarProbabilityCsvRecord2 = records.records[num2];
				if (zbChapterOneBarProbabilityCsvRecord2 != null)
				{
					zbItemData zbItemData2 = new zbItemData((EResourceType)zbChapterOneBarProbabilityCsvRecord2.type, zbChapterOneBarProbabilityCsvRecord2.id, zbChapterOneBarProbabilityCsvRecord2.count);
					m_awards[num].Refresh(zbItemData2);
					m_awardsDictionary.Add(num, zbItemData2);
				}
			}
		}
		zbChapterOneBarTargetCsvTableData records2 = zbDataConfig.Instance.chapterOneBarTargetCsvTable.GetRecords();
		for (int num3 = 0; num3 < 5; num3++)
		{
			zbChapterOneBarTargetCsvRecord zbChapterOneBarTargetCsvRecord2 = records2.records[num3];
			if (zbChapterOneBarTargetCsvRecord2 != null)
			{
				((TextElement)m_lbTargets[num3]).text = zbChapterOneBarTargetCsvRecord2.target.ToString();
				m_items[num3].Refresh(new zbItemData((EResourceType)zbChapterOneBarTargetCsvRecord2.award[0], zbChapterOneBarTargetCsvRecord2.award[1], zbChapterOneBarTargetCsvRecord2.award[2]));
			}
		}
	}

	public void RefreshCustom(bool isFirstIn = true)
	{
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		if (zbServerData.sGameData.JourneyInfo == null || zbServerData.sGameData.JourneyInfo.ChapterOne == null || zbServerData.sGameData.JourneyInfo.ChapterOne.BarDraw == null)
		{
			return;
		}
		((TextElement)m_lbRareHit).text = string.Format(zbUtil.Language("lang.10804"), zbServerData.sGameData.JourneyInfo.ChapterOne.BarDraw.RareHit, m_bigAwardName);
		bool flag = zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Money, 34, 1);
		((VisualElement)m_lbOne).style.color = StyleColor.op_Implicit(flag ? Color.white : Color.red);
		bool flag2 = zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Money, 34, 10);
		((VisualElement)m_lbTen).style.color = StyleColor.op_Implicit(flag2 ? Color.white : Color.red);
		m_notifyOne.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.moneyHandler.HasEnoughMoney(34, 1L)));
		m_notifyTen.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.moneyHandler.HasEnoughMoney(34, 10L)));
		ChangeAnimPlay();
		RefreshTarget();
		RefreshLeftTime();
		if (isFirstIn)
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
		m_spineChuChangs[index].AnimationState.SetAnimation(0, "in", false);
		m_spineChuChangs[index].AnimationState.AddAnimation(0, "loop", true, 0f);
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

	private void RefreshTarget()
	{
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)m_lbCount).text = zbServerData.sGameData.JourneyInfo.ChapterOne.BarDraw.TotalCount.ToString();
		for (int i = 0; i < 5; i++)
		{
			JourneyTargetRecordDetail journeyTargetRecordDetail = zbServerData.sGameData.JourneyInfo.ChapterOne.BarDraw.TargetList[i];
			if (journeyTargetRecordDetail.State == ECommonState.Yes)
			{
				m_items[i].SetTouchCallback(delegate
				{
					zbWsMsgHandler.Instance.Journey_ChapterOneBarTarget_Req();
				});
			}
			else
			{
				m_items[i].SetTouchCallback(null);
			}
			m_items[i].SetSpineVisible(journeyTargetRecordDetail.State == ECommonState.Yes);
			m_items[i].SetGettedVisible(journeyTargetRecordDetail.State == ECommonState.End);
		}
		m_spriteProgress.style.width = StyleLength.op_Implicit(new Length(GetProgress() * 536f, (LengthUnit)0));
	}

	private float GetProgress()
	{
		zbChapterOneBarTargetCsvRecord[] records = zbDataConfig.Instance.chapterOneBarTargetCsvTable.GetRecords().records;
		if (records == null || records.Length == 0)
		{
			return 0f;
		}
		int num = records.Length;
		float num2 = 1f / (float)num;
		for (int i = 0; i < num; i++)
		{
			int target = records[i].target;
			if (zbServerData.sGameData.JourneyInfo.ChapterOne.BarDraw.TotalCount < target)
			{
				int num3 = ((i != 0) ? records[i - 1].target : 0);
				float num4 = (float)(zbServerData.sGameData.JourneyInfo.ChapterOne.BarDraw.TotalCount - num3) / (float)(target - num3);
				return Mathf.Clamp01((float)i * num2 + num4 * num2);
			}
		}
		return 1f;
	}

	private void OnHeros(int index)
	{
		if (m_bExeciing)
		{
			m_bImmediateOver = true;
			return;
		}
		zbPlayerSuitCsvRecord zbPlayerSuitCsvRecord2 = zbDataConfig.Instance.playerSuitCsvTable.GetRecords().records.Values.FirstOrDefault((zbPlayerSuitCsvRecord r) => r.id == m_suitIds[index]);
		if (zbPlayerSuitCsvRecord2 != null)
		{
			zbUIMgr.Instance.ShowTip(EResourceType.Prop, zbPlayerSuitCsvRecord2.propId, 1L);
		}
	}

	private void OnToggle()
	{
		if (m_bExeciing)
		{
			m_bImmediateOver = true;
			return;
		}
		m_bSkipAnim = !m_bSkipAnim;
		ChangeAnimPlay();
	}

	private void ChangeAnimPlay()
	{
		m_spriteOk.visible = m_bSkipAnim;
	}

	private void OnBar(int count)
	{
		if (m_bExeciing)
		{
			m_bImmediateOver = true;
		}
		else if (!zbSelf.Instance.moneyHandler.HasEnoughMoney(34, count))
		{
			zbUIMgr.Instance.ShowTip(EResourceType.Money, 34L, count);
		}
		else
		{
			zbWsMsgHandler.Instance.Journey_ChapterOneBarDraw_Req(count);
		}
	}

	private void OnRate()
	{
		zbUIMgr.Instance.ShowJourneyOneBarRate();
	}

	private void RefreshLeftTime()
	{
		long endLeftTime = zbSelf.Instance.journeyOneHandler.GetEndLeftTime();
		if (endLeftTime > 0)
		{
			((TextElement)m_lbTime).text = zbUtil.FormatTimeHHMMSS(endLeftTime);
		}
		else
		{
			OnClose();
		}
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		RefreshLeftTime();
	}

	private void OnClose()
	{
		if (m_bExeciing)
		{
			m_bImmediateOver = true;
		}
		else
		{
			Hide();
		}
	}

	private void OnUpdateEvent(object obj)
	{
		RefreshCustom(isFirstIn: false);
	}

	private IEnumerator PlayAnimOnceCoroutine(int awardIndex, bool isTen = false)
	{
		float accumTime = (isTen ? 24f : 3f);
		float tarRotation = (float)awardIndex * 30f;
		((Component)m_point).gameObject.SetActive(true);
		Skeleton skeleton = ((SkeletonRenderer)m_point).Skeleton;
		Bone rotateBone = skeleton.FindBone("rotate");
		if (rotateBone == null)
		{
			yield break;
		}
		rotateBone.Rotation = 0f;
		float rotationSpeed = tarRotation / accumTime;
		float elapsed = 0f;
		m_point.AnimationState.ClearTrack(0);
		TrackEntry trackEntry = m_point.AnimationState.SetAnimation(0, isTen ? "10x" : "animation", false);
		trackEntry.TimeScale = 1f;
		float stopTime = (isTen ? 3.5f : 3f);
		while (elapsed < accumTime)
		{
			if (m_bImmediateOver)
			{
				((Component)m_point).gameObject.SetActive(false);
				m_point.AnimationState.ClearTrack(0);
				break;
			}
			elapsed += Time.deltaTime;
			rotateBone.Rotation -= rotationSpeed * Time.deltaTime;
			if (isTen && trackEntry.TrackTime >= stopTime)
			{
				trackEntry.TrackTime = stopTime;
				trackEntry.TimeScale = 0f;
				break;
			}
			yield return null;
		}
		rotateBone.Rotation = 0f - tarRotation;
	}

	private IEnumerator PlayerAnimSequenceCoroutine(int awardIndex, bool isTen)
	{
		zbAudioMgr.Instance.PlayAudio("audio/ui/rise_normal.ogg");
		m_bExeciing = true;
		zbUIMgr.Instance.ShowModel();
		m_bImmediateOver = false;
		if (!m_bSkipAnim)
		{
			yield return PlayAnimOnceCoroutine(awardIndex, isTen);
		}
		if (m_ackAwardList != null && m_ackDataChange != null)
		{
			zbSelf.Instance.dataHandler.DataChange(m_ackDataChange, m_ackAwardList.ToArray());
		}
		m_bExeciing = false;
		zbUIMgr.Instance.HideModel();
		yield return null;
	}

	private void OnEventBarDrawAck(object data)
	{
		RefreshCustom(isFirstIn: false);
		JourneyChapterOneBarDrawAck ack = data as JourneyChapterOneBarDrawAck;
		if (ack != null)
		{
			m_ackAwardList = ((IEnumerable<AwardDetail>)ack.AwardList).ToList();
			m_ackDataChange = ack.DataChange;
			int key = m_awardsDictionary.FirstOrDefault((KeyValuePair<int, zbItemData> r) => r.Value.type == ack.AwardList[0].Type && r.Value.count == ack.AwardList[0].Count && r.Value.id == ack.AwardList[0].Tid).Key;
			if (m_animCoroutine != null)
			{
				((MonoBehaviour)this).StopCoroutine(m_animCoroutine);
			}
			m_animCoroutine = ((MonoBehaviour)this).StartCoroutine(PlayerAnimSequenceCoroutine(key, ack.AwardList.Count > 1));
			return;
		}
		AdvertisementChapterOneBarAck ack2 = data as AdvertisementChapterOneBarAck;
		if (ack2 != null)
		{
			m_ackAwardList = ((IEnumerable<AwardDetail>)ack2.AwardList).ToList();
			m_ackDataChange = ack2.DataChange;
			int key2 = m_awardsDictionary.FirstOrDefault((KeyValuePair<int, zbItemData> r) => r.Value.type == ack2.AwardList[0].Type && r.Value.count == ack2.AwardList[0].Count && r.Value.id == ack2.AwardList[0].Tid).Key;
			if (m_animCoroutine != null)
			{
				((MonoBehaviour)this).StopCoroutine(m_animCoroutine);
			}
			m_animCoroutine = ((MonoBehaviour)this).StartCoroutine(PlayerAnimSequenceCoroutine(key2, ack2.AwardList.Count > 1));
		}
	}
}
