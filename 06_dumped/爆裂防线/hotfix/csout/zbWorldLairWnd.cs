using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbWorldLairWnd : zbBaseWnd
{
	private VisualElement[] m_bosses = (VisualElement[])(object)new VisualElement[4];

	private VisualElement[] m_bossAvatars = (VisualElement[])(object)new VisualElement[4];

	private Button[] m_btnBosses = (Button[])(object)new Button[4];

	private VisualElement[] m_sos = (VisualElement[])(object)new VisualElement[4];

	private zbPieProgress[] m_bossesProgress = new zbPieProgress[4];

	private VisualElement[] m_bossHeadFrames = (VisualElement[])(object)new VisualElement[4];

	private Label[] m_lbDifficuitys = (Label[])(object)new Label[4];

	private SpineVisualElement[] m_spines = (SpineVisualElement[])(object)new SpineVisualElement[4];

	private VisualElement m_mineBoss;

	private VisualElement m_mineBossAvatar;

	private VisualElement m_mineBossTitle;

	private zbPieProgress m_mineBossProgress;

	private Button m_btnMineBoss;

	private Label m_lbMineBossDifficuity;

	private SpineVisualElement m_mineBossSpine;

	private Button m_btnRefresh;

	private Label m_lbRefresh;

	private Button m_btnBack;

	private Label m_lbCountVal;

	private Button m_btnAdd;

	private SkeletonAnimation m_spineSaomiao;

	private JToken m_worldBase;

	private WorldLairInfo m_worldLairInfo;

	private WorldLairHelpInfo[] m_worldLairHelpInfos;

	private readonly int m_refreshInterval = 30;

	private long m_nextRefreshTm;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnBack = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBack, (Action)OnClose, "", (TrickleDown)0);
		m_spineSaomiao = ((Component)m_GameObjectSceneRoot.transform.Find("saomiao")).GetComponent<SkeletonAnimation>();
		for (int i = 0; i < 4; i++)
		{
			m_bosses[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "boss" + i, (string)null);
			m_btnBosses[i] = UQueryExtensions.Q<Button>(m_bosses[i], "btn_touch", (string)null);
			m_sos[i] = UQueryExtensions.Q<VisualElement>(m_bosses[i], "sos", (string)null);
			m_bossesProgress[i] = UQueryExtensions.Q<zbPieProgress>(m_bosses[i], (string)null, (string)null);
			m_lbDifficuitys[i] = UQueryExtensions.Q<Label>(m_bosses[i], "lb_difficulty", (string)null);
			m_bossHeadFrames[i] = UQueryExtensions.Q<VisualElement>(m_bosses[i], "headFrame", (string)null);
			m_spines[i] = UQueryExtensions.Q<SpineVisualElement>(m_bosses[i], "spine", (string)null);
			m_bossAvatars[i] = UQueryExtensions.Q<VisualElement>(m_bosses[i], "bossAvatar", (string)null);
			int tempIndex = i;
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBosses[i], (Action<ClickEvent>)delegate
			{
				OnBoss(tempIndex);
			}, "", (TrickleDown)0);
		}
		m_mineBoss = UQueryExtensions.Q<VisualElement>(m_RootElement, "mineBoss", (string)null);
		m_mineBossAvatar = UQueryExtensions.Q<VisualElement>(m_mineBoss, "bossAvatar", (string)null);
		m_mineBossProgress = UQueryExtensions.Q<zbPieProgress>(m_mineBoss, "progress", (string)null);
		m_lbMineBossDifficuity = UQueryExtensions.Q<Label>(m_mineBoss, "lb_difficulty", (string)null);
		m_mineBossSpine = UQueryExtensions.Q<SpineVisualElement>(m_mineBoss, "spine", (string)null);
		m_mineBossTitle = UQueryExtensions.Q<VisualElement>(m_mineBoss, "mineBossTitle", (string)null);
		m_btnMineBoss = UQueryExtensions.Q<Button>(m_mineBoss, "btn_touch", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnMineBoss, (Action)OnMineBoss, "", (TrickleDown)0);
		m_btnRefresh = UQueryExtensions.Q<Button>(m_RootElement, "btn_refresh", (string)null);
		m_lbRefresh = UQueryExtensions.Q<Label>(m_RootElement, "lb_refresh", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRefresh, (Action)OnRefresh, "", (TrickleDown)0);
		m_lbCountVal = UQueryExtensions.Q<Label>(m_RootElement, "lb_countVal", (string)null);
		m_btnAdd = UQueryExtensions.Q<Button>(m_RootElement, "btn_add", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAdd, (Action)OnAdd, "", (TrickleDown)0);
		m_worldBase = (JToken)(object)zbDataConfig.Instance.worldBaseJsonTable.GetRecord();
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_WorldInfo, OnUpdateEventWorldBoss);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_WorldLairRefresh, OnUpdateEventSaoMiao);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_WorldlairBuyNum, OnUpdateEventBuyNum);
	}

	public void RefreshCustom(bool needRefresh = false)
	{
		m_worldLairInfo = zbServerData.sGameData.WorldLairInfo;
		if (needRefresh && (m_worldLairInfo == null || m_worldLairInfo.SelfLairInfo == null))
		{
			OnRefresh();
			return;
		}
		m_worldLairHelpInfos = zbSelf.Instance.worldLairHandler.m_worldLairHelpInfos.ToArray();
		ChangeBossVisible(flag: false);
		RefreshBosses();
		RefreshMineBoss();
		RefreshFightNum();
		((MonoBehaviour)this).StartCoroutine(ChuchangSequence());
	}

	private IEnumerator ChuchangSequence()
	{
		zbUIMgr.Instance.ShowModel();
		for (int i = 0; i < 4; i++)
		{
			bool flag = i < m_worldLairHelpInfos.Length;
			m_bosses[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			if (flag)
			{
				m_sos[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				((VisualElement)m_bossesProgress[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				((VisualElement)m_lbDifficuitys[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				m_bossHeadFrames[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				((MonoBehaviour)this).StartCoroutine(FadeIn(i));
				yield return (object)new WaitForSecondsRealtime(0.2f);
			}
		}
		bool flag2 = m_worldLairInfo != null && m_worldLairInfo.SelfLairInfo != null;
		m_mineBoss.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag2));
		if (flag2)
		{
			m_mineBossTitle.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_mineBossProgress).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_lbMineBossDifficuity).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((MonoBehaviour)this).StartCoroutine(FadeMineBossIn());
			yield return (object)new WaitForSecondsRealtime(0.2f);
		}
		yield return (object)new WaitForSecondsRealtime(0.5f);
		zbUIMgr.Instance.HideModel();
	}

	private IEnumerator FadeMineBossIn()
	{
		m_mineBossSpine.AnimationState.ClearTrack(0);
		m_mineBossSpine.AnimationState.SetAnimation(0, "in2", false);
		m_mineBossSpine.AnimationState.AddAnimation(0, "loop2", true, 0f);
		float duration = 0.4f;
		float timer = 0f;
		AnimationCurve curve = new AnimationCurve((Keyframe[])(object)new Keyframe[3]
		{
			new Keyframe(0f, 0f),
			new Keyframe(0.7f, 1.15f),
			new Keyframe(1f, 1f)
		});
		while (timer < duration)
		{
			timer += Time.deltaTime;
			float num = Mathf.Clamp01(timer / duration);
			float num2 = curve.Evaluate(num);
			m_mineBossAvatar.transform.scale = new Vector3(num2, num2, 1f);
			yield return null;
		}
		m_mineBossAvatar.transform.scale = Vector3.one;
		m_mineBossTitle.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		((VisualElement)m_mineBossProgress).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		((VisualElement)m_lbMineBossDifficuity).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
	}

	private IEnumerator FadeIn(int index)
	{
		m_spines[index].AnimationState.ClearTrack(0);
		m_spines[index].AnimationState.SetAnimation(0, "in", false);
		m_spines[index].AnimationState.AddAnimation(0, "loop", true, 0f);
		float duration = 0.4f;
		float timer = 0f;
		AnimationCurve curve = new AnimationCurve((Keyframe[])(object)new Keyframe[3]
		{
			new Keyframe(0f, 0f),
			new Keyframe(0.7f, 1.15f),
			new Keyframe(1f, 1f)
		});
		while (timer < duration)
		{
			timer += Time.deltaTime;
			float num = Mathf.Clamp01(timer / duration);
			float num2 = curve.Evaluate(num);
			m_bossAvatars[index].transform.scale = new Vector3(num2, num2, 1f);
			yield return null;
		}
		m_bossAvatars[index].transform.scale = Vector3.one;
		m_sos[index].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		((VisualElement)m_bossesProgress[index]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		((VisualElement)m_lbDifficuitys[index]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_bossHeadFrames[index].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
	}

	public void RefreshBosses()
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 4; i++)
		{
			if (i < m_worldLairHelpInfos.Length)
			{
				zbSelf.Instance.worldLairHandler.RefreshHead(m_bossHeadFrames[i], m_worldLairHelpInfos[i].HelpRoleInfo);
				zbWorldLairCsvRecord recordByKey = zbDataConfig.Instance.worldLairCsvTable.GetRecordByKey(m_worldLairHelpInfos[i].HelpLairInfo.LairId);
				m_bossAvatars[i].style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.icon));
				int mstLv = zbDataConfig.Instance.worldLairLevelCsvTable.GetRecordByKey(m_worldLairHelpInfos[i].HelpLairInfo.Level).mstLv;
				string text = "";
				Color white = Color.white;
				int num = mstLv - zbServerData.sGameData.RoleBaseInfo.Level;
				if (num >= 5)
				{
					text = zbUtil.Language("lang.10567");
					white = Color.red;
				}
				else if (num < 5 && num >= -3)
				{
					text = zbUtil.Language("lang.10568");
					white = Color.white;
				}
				else
				{
					text = zbUtil.Language("lang.10569");
					white = Color.green;
				}
				((VisualElement)m_lbDifficuitys[i]).style.color = StyleColor.op_Implicit(white);
				((TextElement)m_lbDifficuitys[i]).text = text;
				float num2 = ((float)m_worldLairHelpInfos[i].HelpLairInfo.PassTarget - (float)m_worldLairHelpInfos[i].HelpLairInfo.KillCount) / (float)m_worldLairHelpInfos[i].HelpLairInfo.PassTarget * 100f;
				m_bossesProgress[i].progress = 100f - ((num2 > 100f) ? 100f : num2);
			}
		}
	}

	public void RefreshMineBoss()
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		if (m_worldLairInfo != null && m_worldLairInfo.SelfLairInfo != null)
		{
			zbWorldLairCsvRecord recordByKey = zbDataConfig.Instance.worldLairCsvTable.GetRecordByKey(m_worldLairInfo.SelfLairInfo.LairId);
			m_mineBossAvatar.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.icon));
			int mstLv = zbDataConfig.Instance.worldLairLevelCsvTable.GetRecordByKey(m_worldLairInfo.SelfLairInfo.Level).mstLv;
			string text = "";
			Color white = Color.white;
			int num = mstLv - zbServerData.sGameData.RoleBaseInfo.Level;
			if (num >= 5)
			{
				text = zbUtil.Language("lang.10567");
				white = Color.red;
			}
			else if (num < 5 && num >= -3)
			{
				text = zbUtil.Language("lang.10568");
				white = Color.white;
			}
			else
			{
				text = zbUtil.Language("lang.10569");
				white = Color.green;
			}
			if (m_worldLairInfo.SelfLairInfo.PassState == EBoolState.True)
			{
				text = zbUtil.Language("lang.10544");
				white = Color.white;
			}
			((VisualElement)m_lbMineBossDifficuity).style.color = StyleColor.op_Implicit(white);
			((TextElement)m_lbMineBossDifficuity).text = text;
			float num2 = ((float)m_worldLairInfo.SelfLairInfo.PassTarget - (float)m_worldLairInfo.SelfLairInfo.KillCount) / (float)m_worldLairInfo.SelfLairInfo.PassTarget * 100f;
			m_mineBossProgress.progress = 100f - ((num2 > 100f) ? 100f : num2);
		}
	}

	public void RefreshFightNum()
	{
		int num = Extensions.Value<int>((IEnumerable<JToken>)m_worldBase[(object)"lair"][(object)"freeNum"]);
		((TextElement)m_lbCountVal).text = (num + zbServerData.sGameData.WorldLairInfo.WorldLairBaseInfo.BuyNum - zbServerData.sGameData.WorldLairInfo.WorldLairBaseInfo.FightNum).ToString();
	}

	private void OnMineBoss()
	{
		if (m_worldLairInfo.SelfLairInfo.PassState == EBoolState.True)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11304"));
		}
		else
		{
			zbUIMgr.Instance.ShowWorldLairFight(m_worldLairInfo.SelfLairInfo);
		}
	}

	private void OnBoss(int index)
	{
		zbUIMgr.Instance.ShowWorldLairFight(m_worldLairHelpInfos[index].HelpLairInfo, isHelp: true, m_worldLairHelpInfos[index].HelpRoleInfo);
	}

	private void OnClose()
	{
		Hide();
		ChangeBossVisible(flag: false);
	}

	private void ChangeBossVisible(bool flag)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 4; i++)
		{
			m_bosses[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		}
		m_mineBoss.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
	}

	private void OnRefresh()
	{
		if (m_nextRefreshTm <= zbSelf.Instance.timeHandler.GetServerTime())
		{
			m_nextRefreshTm = zbSelf.Instance.timeHandler.GetServerTime() + m_refreshInterval;
			zbWsMsgHandler.Instance.WorldLair_Refresh_Req();
		}
	}

	private void OnSaoMiao()
	{
		m_worldLairHelpInfos = zbSelf.Instance.worldLairHandler.m_worldLairHelpInfos.ToArray();
		m_worldLairInfo = zbServerData.sGameData.WorldLairInfo;
		RefreshBosses();
		RefreshMineBoss();
		RefreshFightNum();
		((Component)m_spineSaomiao).gameObject.SetActive(false);
		((MonoBehaviour)this).StartCoroutine(PlaySaoMiaoAnim());
	}

	private IEnumerator PlaySaoMiaoAnim()
	{
		((Component)m_spineSaomiao).gameObject.SetActive(true);
		ChangeBossVisible(flag: false);
		m_spineSaomiao.AnimationState.ClearTrack(0);
		m_spineSaomiao.AnimationState.SetAnimation(0, "animation", false);
		yield return (object)new WaitForSecondsRealtime(1.8666667f);
		((Component)m_spineSaomiao).gameObject.SetActive(false);
		yield return null;
		((MonoBehaviour)this).StartCoroutine(ChuchangSequence());
	}

	private void OnAdd()
	{
		zbUIMgr.Instance.ShowBuyCount(EBattleFuncType.WorldLair);
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		RefreshTime();
	}

	private void OnUpdateEventWorldBoss(object data)
	{
		RefreshFightNum();
	}

	private void OnUpdateEventSaoMiao(object data)
	{
		zbAudioMgr.Instance.PlayAudio("audio/ui/saomiao.ogg");
		OnSaoMiao();
	}

	private void OnUpdateEventBuyNum(object data)
	{
		RefreshFightNum();
	}

	protected override void RefreshWhenAutoVisible()
	{
		base.RefreshWhenAutoVisible();
		zbWsMsgHandler.Instance.WorldLair_Data_Req();
	}

	private void RefreshTime()
	{
		long num = m_nextRefreshTm - zbSelf.Instance.timeHandler.GetServerTime();
		if (num > 0)
		{
			((TextElement)m_lbRefresh).text = zbUtil.FormatTimeMMSS(num);
			((VisualElement)m_btnRefresh).SetEnabled(false);
		}
		else
		{
			((TextElement)m_lbRefresh).text = "";
			((VisualElement)m_btnRefresh).SetEnabled(true);
		}
	}
}
