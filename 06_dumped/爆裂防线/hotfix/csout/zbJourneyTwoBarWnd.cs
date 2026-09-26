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

public class zbJourneyTwoBarWnd : zbBaseWnd
{
	private zbSpineScene m_spineScene;

	private SkeletonAnimation[] m_spineChuChangs = (SkeletonAnimation[])(object)new SkeletonAnimation[4];

	private zbPlayerAvatar[] m_spineAvatars = new zbPlayerAvatar[4];

	private Button[] m_btnHeros = (Button[])(object)new Button[4];

	private static readonly int[] m_suitIds = new int[4] { 1131, 1041, 1141, 1101 };

	private static readonly int[] m_gunIds = new int[4] { 2131, 2041, 2141, 2101 };

	private Label m_lbTitle;

	private Label m_lbTime;

	private Label m_lbRareHit;

	private VisualElement m_nodeAwards;

	private zbItem[] m_awards = new zbItem[12];

	private Label[] m_lbCounts = (Label[])(object)new Label[12];

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
		//IL_044d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0478: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_spineScene = ((Component)m_GameObjectSceneRoot.transform.Find("spineScene")).GetComponent<zbSpineScene>();
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
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		((TextElement)m_lbTitle).text = zbUtil.Language("lang.11098");
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_lbRareHit = UQueryExtensions.Q<Label>(m_RootElement, "lb_rareHit", (string)null);
		m_nodeAwards = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_awards", (string)null);
		VisualElement val = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_counts", (string)null);
		for (int num = 0; num < 12; num++)
		{
			m_awards[num] = UQueryExtensions.Q<zbItem>(m_nodeAwards, "award" + num, (string)null);
			m_awards[num].OnEnable(this);
			m_lbCounts[num] = UQueryExtensions.Q<Label>(val, "lb_count" + num, (string)null);
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
		m_gameEevent.RegisterEvent(EGameEvent.Event_Journey_ChapterTwoBarDraw_Ack, OnEventBarDrawAck);
		InitRecord();
		InitSuitGun();
	}

	private void InitRecord()
	{
		zbChapterTwoBarPoolCsvRecord zbChapterTwoBarPoolCsvRecord2 = zbDataConfig.Instance.chapterTwoBarPoolCsvTable.GetRecords()?.records.FirstOrDefault((zbChapterTwoBarPoolCsvRecord r) => r.poolType == 4);
		if (zbChapterTwoBarPoolCsvRecord2 != null)
		{
			m_bigAwardName = zbResourceHelper.GetResorceIconPath((EResourceType)zbChapterTwoBarPoolCsvRecord2.type, zbChapterTwoBarPoolCsvRecord2.id).strName;
		}
		zbChapterTwoBarProbabilityCsvTableData records = zbDataConfig.Instance.chapterTwoBarProbabilityCsvTable.GetRecords();
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
				zbChapterTwoBarProbabilityCsvRecord zbChapterTwoBarProbabilityCsvRecord2 = records.records[num2];
				if (zbChapterTwoBarProbabilityCsvRecord2 != null)
				{
					zbItemData zbItemData2 = new zbItemData((EResourceType)zbChapterTwoBarProbabilityCsvRecord2.type, zbChapterTwoBarProbabilityCsvRecord2.id, zbChapterTwoBarProbabilityCsvRecord2.count);
					m_awards[num].Refresh(zbItemData2);
					m_awardsDictionary.Add(num, zbItemData2);
				}
			}
		}
		zbChapterTwoBarTargetCsvTableData records2 = zbDataConfig.Instance.chapterTwoBarTargetCsvTable.GetRecords();
		for (int num3 = 0; num3 < 5; num3++)
		{
			zbChapterTwoBarTargetCsvRecord zbChapterTwoBarTargetCsvRecord2 = records2.records[num3];
			if (zbChapterTwoBarTargetCsvRecord2 != null)
			{
				((TextElement)m_lbTargets[num3]).text = zbChapterTwoBarTargetCsvRecord2.target.ToString();
				m_items[num3].Refresh(new zbItemData((EResourceType)zbChapterTwoBarTargetCsvRecord2.award[0], zbChapterTwoBarTargetCsvRecord2.award[1], zbChapterTwoBarTargetCsvRecord2.award[2]));
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
		if (zbServerData.sGameData.JourneyInfo == null || zbServerData.sGameData.JourneyInfo.ChapterTwo == null || zbServerData.sGameData.JourneyInfo.ChapterTwo.BarDraw == null)
		{
			return;
		}
		((TextElement)m_lbRareHit).text = string.Format(zbUtil.Language("lang.10804"), zbServerData.sGameData.JourneyInfo.ChapterTwo.BarDraw.RareHit, m_bigAwardName);
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
			ResetResult();
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
		((TextElement)m_lbCount).text = zbServerData.sGameData.JourneyInfo.ChapterTwo.BarDraw.TotalCount.ToString();
		for (int i = 0; i < 5; i++)
		{
			JourneyTargetRecordDetail journeyTargetRecordDetail = zbServerData.sGameData.JourneyInfo.ChapterTwo.BarDraw.TargetList[i];
			if (journeyTargetRecordDetail.State == ECommonState.Yes)
			{
				m_items[i].SetTouchCallback(delegate
				{
					zbWsMsgHandler.Instance.Journey_ChapterTwoBarTarget_Req();
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
		zbChapterTwoBarTargetCsvRecord[] records = zbDataConfig.Instance.chapterTwoBarTargetCsvTable.GetRecords().records;
		if (records == null || records.Length == 0)
		{
			return 0f;
		}
		int num = records.Length;
		float num2 = 1f / (float)num;
		for (int i = 0; i < num; i++)
		{
			int target = records[i].target;
			if (zbServerData.sGameData.JourneyInfo.ChapterTwo.BarDraw.TotalCount < target)
			{
				int num3 = ((i != 0) ? records[i - 1].target : 0);
				float num4 = (float)(zbServerData.sGameData.JourneyInfo.ChapterTwo.BarDraw.TotalCount - num3) / (float)(target - num3);
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
			zbWsMsgHandler.Instance.Journey_ChapterTwoBarDraw_Req(count);
		}
	}

	private void OnRate()
	{
		zbUIMgr.Instance.ShowJourneyOneBarRate(2);
	}

	private void RefreshLeftTime()
	{
		long endLeftTime = zbSelf.Instance.journeyTwoHandler.GetEndLeftTime();
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

	private IEnumerator PlayAnimOnceCoroutine(bool isTen = false)
	{
		float duration = (isTen ? 2.4333334f : 1.3333334f);
		string animationName = (isTen ? "act2" : "act1");
		TrackEntry trackEntry = m_spineScene.SetAnimation(animationName, isLoop: false);
		trackEntry.TimeScale = 1f;
		while (true)
		{
			if (m_bImmediateOver)
			{
				m_spineScene.SetAnimation("animation", isLoop: true);
				break;
			}
			if (!(trackEntry.TrackTime >= duration))
			{
				yield return null;
				continue;
			}
			break;
		}
	}

	private IEnumerator PlayerAnimSequenceCoroutine(bool isTen)
	{
		ResetResult();
		zbAudioMgr.Instance.PlayAudio("audio/ui/rise_normal.ogg");
		m_bExeciing = true;
		zbUIMgr.Instance.ShowModel();
		m_bImmediateOver = false;
		if (!m_bSkipAnim)
		{
			yield return PlayAnimOnceCoroutine(isTen);
		}
		if (m_ackAwardList != null && m_ackDataChange != null)
		{
			zbSelf.Instance.dataHandler.DataChange(m_ackDataChange, m_ackAwardList.ToArray());
			ShowResult(m_ackAwardList.ToArray());
		}
		m_bExeciing = false;
		zbUIMgr.Instance.HideModel();
		yield return null;
	}

	private void OnEventBarDrawAck(object data)
	{
		RefreshCustom(isFirstIn: false);
		JourneyChapterTwoBarDrawAck ack = data as JourneyChapterTwoBarDrawAck;
		if (ack != null)
		{
			m_ackAwardList = ((IEnumerable<AwardDetail>)ack.AwardList).ToList();
			m_ackDataChange = ack.DataChange;
			_ = m_awardsDictionary.FirstOrDefault((KeyValuePair<int, zbItemData> r) => r.Value.type == ack.AwardList[0].Type && r.Value.count == ack.AwardList[0].Count && r.Value.id == ack.AwardList[0].Tid).Key;
			if (m_animCoroutine != null)
			{
				((MonoBehaviour)this).StopCoroutine(m_animCoroutine);
			}
			m_animCoroutine = ((MonoBehaviour)this).StartCoroutine(PlayerAnimSequenceCoroutine(ack.AwardList.Count > 1));
			return;
		}
		AdvertisementChapterTwoBarAck ack2 = data as AdvertisementChapterTwoBarAck;
		if (ack2 != null)
		{
			m_ackAwardList = ((IEnumerable<AwardDetail>)ack2.AwardList).ToList();
			m_ackDataChange = ack2.DataChange;
			_ = m_awardsDictionary.FirstOrDefault((KeyValuePair<int, zbItemData> r) => r.Value.type == ack2.AwardList[0].Type && r.Value.count == ack2.AwardList[0].Count && r.Value.id == ack2.AwardList[0].Tid).Key;
			if (m_animCoroutine != null)
			{
				((MonoBehaviour)this).StopCoroutine(m_animCoroutine);
			}
			m_animCoroutine = ((MonoBehaviour)this).StartCoroutine(PlayerAnimSequenceCoroutine(ack2.AwardList.Count > 1));
		}
	}

	private void ShowResult(AwardDetail[] awardList)
	{
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		Dictionary<(EResourceType, int, int), int> dictionary = new Dictionary<(EResourceType, int, int), int>();
		foreach (AwardDetail awardDetail in awardList)
		{
			(EResourceType, int, int) key = (awardDetail.Type, (int)awardDetail.Tid, (int)awardDetail.Count);
			if (!dictionary.ContainsKey(key))
			{
				dictionary[key] = 0;
			}
			dictionary[key]++;
		}
		foreach (KeyValuePair<int, zbItemData> item in m_awardsDictionary)
		{
			zbItemData value = item.Value;
			(EResourceType, int, int) key2 = (value.type, (int)value.id, (int)value.count);
			if (dictionary.TryGetValue(key2, out var value2) && value2 > 0)
			{
				m_awards[item.Key].SetSpineVisible(vis: true);
				if (value2 > 1)
				{
					((VisualElement)m_lbCounts[item.Key]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
					((TextElement)m_lbCounts[item.Key]).text = value2.ToString();
				}
			}
		}
	}

	private void ResetResult()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 12; i++)
		{
			m_awards[i].SetSpineVisible(vis: false);
			((VisualElement)m_lbCounts[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
	}
}
