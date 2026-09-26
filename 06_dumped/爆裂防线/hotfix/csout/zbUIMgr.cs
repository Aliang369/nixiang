using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbUIMgr : MonoBehaviour
{
	public static zbUIMgr Instance;

	[NonSerialized]
	public Dictionary<EWndId, zbBaseWnd> m_kDictWnd;

	protected UIDocument m_Document;

	protected VisualElement m_RootElement;

	private VisualElement m_rootCascadWnd;

	private VisualElement m_rootBubble;

	private VisualElement m_rootTipConnect;

	private VisualElement m_rootTop;

	private VisualElement m_rootGuide;

	private VisualElement m_rootGuideWeak;

	private int m_touchCount;

	private List<SpineVisualElement> m_effects;

	private VisualElement m_visualTree;

	private int m_lastScreenWidht;

	private int m_lastScreenHeight;

	[NonSerialized]
	public zbScrollBubble m_scrollBubble;

	[NonSerialized]
	public float m_lastTouchTime;

	private int m_uiUnloadCounter;

	private const int m_kReleaseThreshold = 2;

	private void Awake()
	{
		if ((Object)(object)Instance == (Object)null)
		{
			Instance = this;
			Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
			Init();
		}
		else
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
	}

	private void Init()
	{
		m_kDictWnd = new Dictionary<EWndId, zbBaseWnd>();
		m_effects = new List<SpineVisualElement>();
		m_scrollBubble = new zbScrollBubble();
	}

	private void OnEnable()
	{
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		m_Document = ((Component)this).GetComponent<UIDocument>();
		m_RootElement = m_Document.rootVisualElement;
		zbUIScaler.AdapationUI(m_Document);
		m_lastScreenWidht = Screen.width;
		m_lastScreenHeight = Screen.height;
		m_rootCascadWnd = UQueryExtensions.Q<VisualElement>(m_RootElement, "rootCascadWnd", (string)null);
		m_rootBubble = UQueryExtensions.Q<VisualElement>(m_RootElement, "rootBubble", (string)null);
		m_rootTipConnect = UQueryExtensions.Q<VisualElement>(m_RootElement, "rootTipConnect", (string)null);
		m_rootTop = UQueryExtensions.Q<VisualElement>(m_RootElement, "rootTop", (string)null);
		m_rootGuide = UQueryExtensions.Q<VisualElement>(m_RootElement, "rootGuide", (string)null);
		m_rootGuideWeak = UQueryExtensions.Q<VisualElement>(m_RootElement, "rootGuideWeak", (string)null);
		VisualElement val = UQueryExtensions.Q<VisualElement>(m_rootTop, "left", (string)null);
		VisualElement val2 = UQueryExtensions.Q<VisualElement>(m_rootTop, "right", (string)null);
		if (zbChannel_Login.IsOpenTSCheck())
		{
			val.style.unityBackgroundImageTintColor = new StyleColor(Color.white);
			val2.style.unityBackgroundImageTintColor = new StyleColor(Color.white);
		}
		else
		{
			val.style.unityBackgroundImageTintColor = new StyleColor(Color.black);
			val2.style.unityBackgroundImageTintColor = new StyleColor(Color.black);
		}
		m_touchCount = 0;
		m_effects.Clear();
		for (int i = 1; i <= 3; i++)
		{
			string text = "effect" + i;
			SpineVisualElement val3 = UQueryExtensions.Q<SpineVisualElement>(m_rootTop, text, (string)null);
			((VisualElement)val3).visible = false;
			m_effects.Add(val3);
		}
		m_scrollBubble.OnEnable(m_rootTop);
	}

	public VisualElement GetRootElement()
	{
		return m_RootElement;
	}

	public PanelSettings GetPanelSettings()
	{
		return m_Document.panelSettings;
	}

	private void ShowTouchEffect(Vector2 position)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if (m_touchCount >= m_effects.Count)
		{
			m_touchCount = 0;
		}
		SpineVisualElement val = m_effects[m_touchCount];
		if (val != null)
		{
			((VisualElement)val).visible = true;
			((VisualElement)val).style.left = StyleLength.op_Implicit(position.x);
			((VisualElement)val).style.top = StyleLength.op_Implicit(position.y);
			val.AnimationState.SetAnimation(0, "animation", false);
		}
		m_touchCount++;
	}

	public void AddWndElement(zbBaseWnd wnd)
	{
		EWndId wndId = wnd.GetWndId();
		VisualElement rootElement = wnd.GetRootElement();
		switch (wndId)
		{
		case EWndId.Bubble:
			m_rootBubble.Add(rootElement);
			break;
		case EWndId.TipConnect:
			m_rootTipConnect.Add(rootElement);
			break;
		case EWndId.Guide:
			m_rootGuide.Add(rootElement);
			break;
		case EWndId.GuideWeak:
			m_rootGuideWeak.Add(rootElement);
			break;
		default:
			m_rootCascadWnd.Add(rootElement);
			break;
		}
	}

	public VisualElement GetRootCascadWndElement()
	{
		return m_rootCascadWnd;
	}

	public zbBaseWnd GetWndById(EWndId id)
	{
		if (m_kDictWnd.ContainsKey(id))
		{
			return m_kDictWnd[id];
		}
		return null;
	}

	public zbBaseWnd ShowWnd(EWndId id, string assertPath)
	{
		if (id != EWndId.GuideFinger && IsShow(EWndId.GuideFinger))
		{
			HideWnd(EWndId.GuideFinger);
		}
		zbBaseWnd zbBaseWnd2 = GetWndById(id);
		if ((Object)(object)zbBaseWnd2 == (Object)null)
		{
			GameObject val = zbAssetLoader.Instance.LoadResouce<GameObject>(assertPath);
			if (Object.op_Implicit((Object)(object)val))
			{
				GameObject val2 = Object.Instantiate<GameObject>(val);
				if ((Object)(object)val2 != (Object)null)
				{
					zbBaseWnd2 = val2.GetComponent<zbBaseWnd>();
					if ((Object)(object)zbBaseWnd2 == (Object)null)
					{
						return null;
					}
					if (Object.op_Implicit((Object)(object)zbBaseWnd2))
					{
						zbBaseWnd2.Init(id, val2);
						m_kDictWnd[id] = zbBaseWnd2;
					}
				}
			}
			else
			{
				Debug.LogError((object)("error: can not find prefab=" + assertPath));
			}
		}
		if (Object.op_Implicit((Object)(object)zbBaseWnd2))
		{
			zbBaseWnd2.Show();
			zbBaseWnd2.Refresh();
		}
		if (zbSelf.Instance != null)
		{
			zbProgressEventHelper.Instance.SetProgressEvent2(id);
		}
		return zbBaseWnd2;
	}

	public void HideWnd(EWndId id)
	{
		zbBaseWnd wndById = GetWndById(id);
		if (Object.op_Implicit((Object)(object)wndById) && wndById.IsShow())
		{
			wndById.Hide();
		}
	}

	public void PreloadWnd(EWndId id, string assertPath)
	{
		if ((Object)(object)GetWndById(id) != (Object)null)
		{
			return;
		}
		GameObject val = zbAssetLoader.Instance.LoadResouce<GameObject>(assertPath);
		if ((Object)(object)val == (Object)null)
		{
			Debug.LogError((object)("error: can not find prefab=" + assertPath));
			return;
		}
		GameObject val2 = Object.Instantiate<GameObject>(val);
		zbBaseWnd component = val2.GetComponent<zbBaseWnd>();
		if ((Object)(object)component == (Object)null)
		{
			Object.Destroy((Object)(object)val2);
			return;
		}
		component.Init(id, val2);
		m_kDictWnd[id] = component;
		component.HideForPreload();
	}

	public void ShowLogError(string strLog)
	{
		zbLogErrorWnd zbLogErrorWnd2 = ShowWnd(EWndId.LogError, "prefab/wnd/tip/logerror.prefab") as zbLogErrorWnd;
		if (Object.op_Implicit((Object)(object)zbLogErrorWnd2))
		{
			zbLogErrorWnd2.RefreshCustom(strLog);
		}
	}

	public void ShowSecurityVerification()
	{
		zbSecurityVerificationWnd zbSecurityVerificationWnd2 = ShowWnd(EWndId.SecurityVerification, "prefab/wnd/tip/securityVerification.prefab") as zbSecurityVerificationWnd;
		if (Object.op_Implicit((Object)(object)zbSecurityVerificationWnd2))
		{
			zbSecurityVerificationWnd2.RefreshCustom();
		}
	}

	public void ShowLoading()
	{
		zbProgressEventHelper.Instance.SetProgressTrack(602);
		ShowWnd(EWndId.Loading, "prefab/wnd/loading.prefab");
	}

	public void ShowLogin()
	{
		ShowWnd(EWndId.Login, "prefab/wnd/login/login.prefab");
	}

	public void ShowHub(int tabIndex = 0)
	{
		zbHubWnd zbHubWnd2 = ShowWnd(EWndId.Hub, "prefab/wnd/login/hub.prefab") as zbHubWnd;
		if ((Object)(object)zbHubWnd2 != (Object)null)
		{
			zbHubWnd2.RefreshCustom(tabIndex);
		}
	}

	public void ShowNoticDetail(NoticeDetail m_noticeDetail)
	{
		zbNoticDetailWnd zbNoticDetailWnd2 = ShowWnd(EWndId.NoticeDetail, "prefab/wnd/login/noticDetail.prefab") as zbNoticDetailWnd;
		if ((Object)(object)zbNoticDetailWnd2 != (Object)null)
		{
			zbNoticDetailWnd2.RefreshCustom(m_noticeDetail);
		}
	}

	public void ShowModel()
	{
		ShowWnd(EWndId.Model, "prefab/wnd/model.prefab");
	}

	public void HideModel()
	{
		HideWnd(EWndId.Model);
	}

	public void ShowAward(AwardDetail[] awardlist, Action<bool> callback = null)
	{
		zbAwardWnd zbAwardWnd2 = ShowWnd(EWndId.Award, "prefab/wnd/tip/award.prefab") as zbAwardWnd;
		if ((Object)(object)zbAwardWnd2 != (Object)null)
		{
			zbAwardWnd2.RefreshCustom(awardlist, callback);
		}
	}

	public void ShowFunc(zbFunctionUnlockCsvRecord record)
	{
		zbFuncWnd zbFuncWnd2 = ShowWnd(EWndId.Func, "prefab/wnd/tip/func.prefab") as zbFuncWnd;
		if ((Object)(object)zbFuncWnd2 != (Object)null)
		{
			zbFuncWnd2.RefreshCustom(record);
		}
	}

	public void ShowFuncPreview()
	{
		zbFuncPreviewWnd zbFuncPreviewWnd2 = ShowWnd(EWndId.FuncPreview, "prefab/wnd/comm/funcPreview.prefab") as zbFuncPreviewWnd;
		if ((Object)(object)zbFuncPreviewWnd2 != (Object)null)
		{
			zbFuncPreviewWnd2.RefreshCustom();
		}
	}

	public void ShowMainLevel(int tabIndex = 0, int level = -1)
	{
		zbMainLevelWnd zbMainLevelWnd2 = ShowWnd(EWndId.MainLevel, "prefab/wnd/main/mainLevel.prefab") as zbMainLevelWnd;
		if ((Object)(object)zbMainLevelWnd2 != (Object)null)
		{
			zbMainLevelWnd2.RefreshCustom(tabIndex, level);
		}
	}

	public void ShowMainLevelPerfectPassTip(int level, bool isElite = false, long roleId = 0L)
	{
		zbMainLevelPerfectPassTipWnd zbMainLevelPerfectPassTipWnd2 = ShowWnd(EWndId.MainLevelPerfectPassTip, "prefab/wnd/main/mainLevelPerfectPassTip.prefab") as zbMainLevelPerfectPassTipWnd;
		if ((Object)(object)zbMainLevelPerfectPassTipWnd2 != (Object)null)
		{
			zbMainLevelPerfectPassTipWnd2.RefreshCustom(level, isElite, roleId);
		}
	}

	public zbBaseWnd ShowChallenge(EFuncType funcType = EFuncType.LegionBoss)
	{
		zbChallengeWnd zbChallengeWnd2 = ShowWnd(EWndId.Challenge, "prefab/wnd/main/challenge.prefab") as zbChallengeWnd;
		if ((Object)(object)zbChallengeWnd2 != (Object)null)
		{
			zbChallengeWnd2.RefreshCustom(funcType);
		}
		return zbChallengeWnd2;
	}

	public void ShowMineWar()
	{
		zbMineWarWnd zbMineWarWnd2 = ShowWnd(EWndId.MineWar, "prefab/wnd/mineWar/mineWar.prefab") as zbMineWarWnd;
		if ((Object)(object)zbMineWarWnd2 != (Object)null)
		{
			zbMineWarWnd2.RefreshCustom();
		}
	}

	public void ShowMineWarDetail(MineWarPointDataAck mineWarPointDataAck, CrossMinePointDataAck crossMinePointDataAck)
	{
		zbMineWarDetailWnd zbMineWarDetailWnd2 = ShowWnd(EWndId.MineWarDetail, "prefab/wnd/mineWar/mineWarDetail.prefab") as zbMineWarDetailWnd;
		if ((Object)(object)zbMineWarDetailWnd2 != (Object)null)
		{
			zbMineWarDetailWnd2.RefreshCustom(mineWarPointDataAck, crossMinePointDataAck);
		}
	}

	public void ShowMineWarEnemy(MineWarPointDataAck mineWarPointDataAck, CrossMinePointDataAck crossMinePointDataAck)
	{
		zbMineWarEnemyWnd zbMineWarEnemyWnd2 = ShowWnd(EWndId.MineWarEnemy, "prefab/wnd/mineWar/mineWarEnemy.prefab") as zbMineWarEnemyWnd;
		if ((Object)(object)zbMineWarEnemyWnd2 != (Object)null)
		{
			zbMineWarEnemyWnd2.RefreshCustom(mineWarPointDataAck, crossMinePointDataAck);
		}
	}

	public void ShowMineWarServer(CrossMineServerListAck crossMineServerListAck)
	{
		zbMineWarServerWnd zbMineWarServerWnd2 = ShowWnd(EWndId.MineWarServer, "prefab/wnd/mineWar/mineWarServer.prefab") as zbMineWarServerWnd;
		if ((Object)(object)zbMineWarServerWnd2 != (Object)null)
		{
			zbMineWarServerWnd2.RefreshCustom(crossMineServerListAck);
		}
	}

	public void ShowMineWarRecordWnd()
	{
		zbMineWarRecordWnd zbMineWarRecordWnd2 = ShowWnd(EWndId.MineWarRecord, "prefab/wnd/mineWar/mineWarRecord.prefab") as zbMineWarRecordWnd;
		if ((Object)(object)zbMineWarRecordWnd2 != (Object)null)
		{
			zbMineWarRecordWnd2.RefreshCustom();
		}
	}

	public void ShowMineWarOccupyWnd()
	{
		zbMineWarOccupyWnd zbMineWarOccupyWnd2 = ShowWnd(EWndId.MineWarOccupy, "prefab/wnd/mineWar/mineWarOccupy.prefab") as zbMineWarOccupyWnd;
		if ((Object)(object)zbMineWarOccupyWnd2 != (Object)null)
		{
			zbMineWarOccupyWnd2.RefreshCustom();
		}
	}

	public void ShowMineWarAwardWnd(AwardDetail[] awardlist)
	{
		zbMineWarAwardWnd zbMineWarAwardWnd2 = ShowWnd(EWndId.MineWarAward, "prefab/wnd/mineWar/mineWarAward.prefab") as zbMineWarAwardWnd;
		if ((Object)(object)zbMineWarAwardWnd2 != (Object)null)
		{
			zbMineWarAwardWnd2.RefreshCustom(awardlist);
		}
	}

	public zbBaseWnd ShowHome()
	{
		return ShowWnd(EWndId.Home, "prefab/home/home.prefab") as zbHomeWnd;
	}

	public void ShowHomeCreate()
	{
		ShowWnd(EWndId.HomeCreate, "prefab/wnd/home/homeCreate.prefab");
	}

	public void ShowHomeCreateDetail()
	{
		ShowWnd(EWndId.HomeCreateDetail, "prefab/wnd/home/homeCreateDetail.prefab");
	}

	public void ShowHomeEdit()
	{
		ShowWnd(EWndId.HomeEdit, "prefab/wnd/home/homeEdit.prefab");
	}

	public void ShowHomeEditBag()
	{
		ShowWnd(EWndId.HomeEditBag, "prefab/wnd/home/homeEditBag.prefab");
	}

	public void ShowFloorUnlock(int floor)
	{
		zbFloorUnlockWnd zbFloorUnlockWnd2 = ShowWnd(EWndId.FloorUnlock, "prefab/home/roomInfo/floorUnlock.prefab") as zbFloorUnlockWnd;
		if ((Object)(object)zbFloorUnlockWnd2 != (Object)null)
		{
			zbFloorUnlockWnd2.RefreshCustom(floor);
			return;
		}
		zbFloorUnlockWnd2 = ShowWnd(EWndId.FloorUnlock, "prefab/home/room/floorUnlock.prefab") as zbFloorUnlockWnd;
		if ((Object)(object)zbFloorUnlockWnd2 != (Object)null)
		{
			zbFloorUnlockWnd2.RefreshCustom(floor);
		}
	}

	public void ShowRoomDetail(HomelandRoomDetail data)
	{
		if (data == null)
		{
			return;
		}
		zbRoomDetailWnd zbRoomDetailWnd2 = ShowWnd(EWndId.RoomDetail, "prefab/home/roomInfo/roomDetail.prefab") as zbRoomDetailWnd;
		if ((Object)(object)zbRoomDetailWnd2 != (Object)null)
		{
			zbRoomDetailWnd2.RefreshCustom(data);
			return;
		}
		zbRoomDetailWnd2 = ShowWnd(EWndId.RoomDetail, "prefab/home/room/roomDetail.prefab") as zbRoomDetailWnd;
		if ((Object)(object)zbRoomDetailWnd2 != (Object)null)
		{
			zbRoomDetailWnd2.RefreshCustom(data);
		}
	}

	public void ShowRoomAttrList()
	{
		zbRoomAttrListWnd zbRoomAttrListWnd2 = ShowWnd(EWndId.RoomAttrList, "prefab/home/roomInfo/roomAttrList.prefab") as zbRoomAttrListWnd;
		if ((Object)(object)zbRoomAttrListWnd2 != (Object)null)
		{
			zbRoomAttrListWnd2.RefreshCustom();
			return;
		}
		zbRoomAttrListWnd2 = ShowWnd(EWndId.RoomAttrList, "prefab/home/room/roomAttrList.prefab") as zbRoomAttrListWnd;
		if ((Object)(object)zbRoomAttrListWnd2 != (Object)null)
		{
			zbRoomAttrListWnd2.RefreshCustom();
		}
	}

	public void ShowRoomWorkerRefresh(int workerId)
	{
		zbRoomWorkerRefreshWnd zbRoomWorkerRefreshWnd2 = ShowWnd(EWndId.RoomWorkerRefresh, "prefab/wnd/home/roomWorkerRefresh.prefab") as zbRoomWorkerRefreshWnd;
		if ((Object)(object)zbRoomWorkerRefreshWnd2 != (Object)null)
		{
			zbRoomWorkerRefreshWnd2.RefreshCustom(workerId);
		}
	}

	public void ShowRoomHeroFollow(int heroTid)
	{
		zbRoomHeroFollowWnd zbRoomHeroFollowWnd2 = ShowWnd(EWndId.RoomHeroFollow, "prefab/wnd/home/roomHeroFollow.prefab") as zbRoomHeroFollowWnd;
		if ((Object)(object)zbRoomHeroFollowWnd2 != (Object)null)
		{
			zbRoomHeroFollowWnd2.RefreshCustom(heroTid);
		}
	}

	public void ShowWorkerFollow(WorkerDetail _workerDetail)
	{
		zbWorkerFollowWnd zbWorkerFollowWnd2 = ShowWnd(EWndId.WorkerFollow, "prefab/wnd/home/workerFollow.prefab") as zbWorkerFollowWnd;
		if ((Object)(object)zbWorkerFollowWnd2 != (Object)null)
		{
			zbWorkerFollowWnd2.RefreshCustom(_workerDetail);
		}
	}

	public void ShowWorkerDetail(WorkerDetail _workerDetail)
	{
		zbRoomWorkerDetailWnd zbRoomWorkerDetailWnd2 = ShowWnd(EWndId.WorkerDetail, "prefab/wnd/home/roomWorkerDetail.prefab") as zbRoomWorkerDetailWnd;
		if ((Object)(object)zbRoomWorkerDetailWnd2 != (Object)null)
		{
			zbRoomWorkerDetailWnd2.RefreshCustom(_workerDetail);
		}
	}

	public void ShowWallIdle(Action<bool> callback = null)
	{
		zbWallIdleWnd zbWallIdleWnd2 = ShowWnd(EWndId.WallIdle, "prefab/home/wallIdle.prefab") as zbWallIdleWnd;
		if ((Object)(object)zbWallIdleWnd2 != (Object)null)
		{
			zbWallIdleWnd2.RefreshCustom(callback);
		}
	}

	public void ShowWallIdleBatchSweep()
	{
		ShowWnd(EWndId.WallIdleBatchSweep, "prefab/home/wallIdleBatchSweep.prefab");
	}

	public void ShowSubject(ESubjectType type, Action<bool> callback = null, Action<bool> callgoto = null)
	{
		zbSubjectWnd zbSubjectWnd2 = ShowWnd(EWndId.Subject, "prefab/wnd/subject.prefab") as zbSubjectWnd;
		if ((Object)(object)zbSubjectWnd2 != (Object)null)
		{
			zbSubjectWnd2.RefreshCustom(type, callback, callgoto);
		}
	}

	public void ShowSubjectCard(Action<bool> callback = null)
	{
		zbSubjectCardWnd zbSubjectCardWnd2 = ShowWnd(EWndId.SubjectCard, "prefab/wnd/subjectCard.prefab") as zbSubjectCardWnd;
		if ((Object)(object)zbSubjectCardWnd2 != (Object)null)
		{
			zbSubjectCardWnd2.RefreshCustom(callback);
		}
	}

	public void ShowSubjectJourney(Action<bool> callback = null)
	{
		zbSubjectJourneyWnd zbSubjectJourneyWnd2 = ShowWnd(EWndId.SubjectJourney, "prefab/wnd/subjectJourney.prefab") as zbSubjectJourneyWnd;
		if ((Object)(object)zbSubjectJourneyWnd2 != (Object)null)
		{
			zbSubjectJourneyWnd2.RefreshCustom(callback);
		}
	}

	public void ShowSubjectHoliday(Action<bool> callback = null)
	{
		zbSubjectHolidayWnd zbSubjectHolidayWnd2 = ShowWnd(EWndId.SubjectHoliday, "prefab/wnd/subjectHoliday.prefab") as zbSubjectHolidayWnd;
		if ((Object)(object)zbSubjectHolidayWnd2 != (Object)null)
		{
			zbSubjectHolidayWnd2.RefreshCustom(callback);
		}
	}

	public void ShowSubjectOperate(EOperateType operateType, Action<bool> callback = null)
	{
		zbSubjectOperateWnd zbSubjectOperateWnd2 = ShowWnd(EWndId.SubjectOperate, "prefab/wnd/subjectOperate.prefab") as zbSubjectOperateWnd;
		if ((Object)(object)zbSubjectOperateWnd2 != (Object)null)
		{
			zbSubjectOperateWnd2.RefreshCustom(operateType, callback);
		}
	}

	public void ShowSubjectBoss(Action<bool> callback = null)
	{
		zbSubjectBossWnd zbSubjectBossWnd2 = ShowWnd(EWndId.SubjectBoss, "prefab/wnd/subjectBoss.prefab") as zbSubjectBossWnd;
		if ((Object)(object)zbSubjectBossWnd2 != (Object)null)
		{
			zbSubjectBossWnd2.RefreshCustom(callback);
		}
	}

	public void ShowSubjectWall(Action<bool> callback = null)
	{
		zbSubjectWallWnd zbSubjectWallWnd2 = ShowWnd(EWndId.SubjectWall, "prefab/wnd/subjectWall.prefab") as zbSubjectWallWnd;
		if ((Object)(object)zbSubjectWallWnd2 != (Object)null)
		{
			zbSubjectWallWnd2.RefreshCustom(callback);
		}
	}

	public void ShowSubjectMecha(Action<bool> callback = null)
	{
		zbSubjectMechaWnd zbSubjectMechaWnd2 = ShowWnd(EWndId.SubjectMecha, "prefab/wnd/subjectMecha.prefab") as zbSubjectMechaWnd;
		if ((Object)(object)zbSubjectMechaWnd2 != (Object)null)
		{
			zbSubjectMechaWnd2.RefreshCustom(callback);
		}
	}

	public void ShowSubjectLegionWar(Action<bool> callback = null)
	{
		zbSubjectLegionWarWnd zbSubjectLegionWarWnd2 = ShowWnd(EWndId.SubjectLegionWar, "prefab/wnd/subjectLegionWar.prefab") as zbSubjectLegionWarWnd;
		if ((Object)(object)zbSubjectLegionWarWnd2 != (Object)null)
		{
			zbSubjectLegionWarWnd2.RefreshCustom(callback);
		}
	}

	public zbBaseWnd ShowEliteQuest(int level = -1)
	{
		zbEliteQuestWnd zbEliteQuestWnd2 = ShowWnd(EWndId.EliteQuest, "prefab/wnd/elite/eliteQuest.prefab") as zbEliteQuestWnd;
		if ((Object)(object)zbEliteQuestWnd2 != (Object)null)
		{
			zbEliteQuestWnd2.RefreshCustom(level);
		}
		return zbEliteQuestWnd2;
	}

	public void ShowEliteQuestThankLetter(HelpRecordDetail[] recordList)
	{
		zbEliteQuestThankLetterWnd zbEliteQuestThankLetterWnd2 = ShowWnd(EWndId.EliteQuestThankLetter, "prefab/wnd/elite/eliteQuestThankLetter.prefab") as zbEliteQuestThankLetterWnd;
		if ((Object)(object)zbEliteQuestThankLetterWnd2 != (Object)null)
		{
			zbEliteQuestThankLetterWnd2.RefreshCustom(recordList);
		}
	}

	public void ShowMainQuestJoin(int tabIndex = 1, EFuncType funcType = EFuncType.EliteQuest)
	{
		zbMainQuestJoinWnd zbMainQuestJoinWnd2 = ShowWnd(EWndId.MainQuestJoin, "prefab/wnd/main/mainQuestJoin.prefab") as zbMainQuestJoinWnd;
		if ((Object)(object)zbMainQuestJoinWnd2 != (Object)null)
		{
			zbMainQuestJoinWnd2.RefreshCustom(tabIndex, funcType);
		}
	}

	public void ShowMainQuestTest()
	{
		zbMainQuestTestWnd zbMainQuestTestWnd2 = ShowWnd(EWndId.MainQuestTest, "prefab/wnd/mainQuestTest.prefab") as zbMainQuestTestWnd;
		if ((Object)(object)zbMainQuestTestWnd2 != (Object)null)
		{
			zbMainQuestTestWnd2.RefreshCustom();
		}
	}

	public void ShowMainQuestReceiveAward(bool isMain, int curLevel, Action<bool> callback = null)
	{
		zbMainQuestReceiveAwardsWnd zbMainQuestReceiveAwardsWnd2 = ShowWnd(EWndId.MainQuestReceiveAward, "prefab/wnd/main/mainQuestReceiveAwards.prefab") as zbMainQuestReceiveAwardsWnd;
		if ((Object)(object)zbMainQuestReceiveAwardsWnd2 != (Object)null)
		{
			zbMainQuestReceiveAwardsWnd2.RefreshCustom(isMain, curLevel, callback);
		}
	}

	public void ShowQuestList(int tabIndex)
	{
		zbMainQuestListWnd zbMainQuestListWnd2 = ShowWnd(EWndId.QuestList, "prefab/wnd/main/mainQuestList.prefab") as zbMainQuestListWnd;
		if ((Object)(object)zbMainQuestListWnd2 != (Object)null)
		{
			zbMainQuestListWnd2.RefreshCustom(tabIndex);
		}
	}

	public void ShowQuestPassAward(int index, zbMainLevelCsvRecord mainLevelRecord)
	{
		zbMainQuestPassAwardWnd zbMainQuestPassAwardWnd2 = ShowWnd(EWndId.QuestPassAward, "prefab/wnd/main/mainQuestPassAward.prefab") as zbMainQuestPassAwardWnd;
		if ((Object)(object)zbMainQuestPassAwardWnd2 != (Object)null)
		{
			zbMainQuestPassAwardWnd2.RefreshAwards(index, mainLevelRecord);
		}
	}

	public void ShowEliteQuestPassAward(int index, zbEliteLevelCsvRecord eliteLevelRecord)
	{
		zbMainQuestPassAwardWnd zbMainQuestPassAwardWnd2 = ShowWnd(EWndId.QuestPassAward, "prefab/wnd/main/mainQuestPassAward.prefab") as zbMainQuestPassAwardWnd;
		if ((Object)(object)zbMainQuestPassAwardWnd2 != (Object)null)
		{
			zbMainQuestPassAwardWnd2.RefreshEliteAwards(index, eliteLevelRecord);
		}
	}

	public void ShowQuestView(int[][] _record, int[] _monsterids, int level, bool isMain = true)
	{
		zbMainQuestViewWnd zbMainQuestViewWnd2 = ShowWnd(EWndId.QuestView, "prefab/wnd/main/mainQuestView.prefab") as zbMainQuestViewWnd;
		if ((Object)(object)zbMainQuestViewWnd2 != (Object)null)
		{
			zbMainQuestViewWnd2.RefreshCustom(_record, _monsterids, level, isMain);
		}
	}

	public void ShowCombat(BattleInfo battleInfo, int paramId = 0, zbCombatRecorder restoreData = null, BattleResultInfo dataResult = null)
	{
		if (!zbCombatChecker.IsValidBattleInfo(battleInfo))
		{
			Instance.BubbleInfo(zbUtil.Language("lang.10395"));
			return;
		}
		zbCombatWnd zbCombatWnd2 = ShowWnd(EWndId.Combat, "prefab/wnd/combat/combat.prefab") as zbCombatWnd;
		if ((Object)(object)zbCombatWnd2 != (Object)null)
		{
			zbCombatWnd2.StartBattle(battleInfo, paramId, restoreData, dataResult);
		}
	}

	public void ShowCombatPause()
	{
		ShowWnd(EWndId.CombatPause, "prefab/wnd/combat/combatPause.prefab");
	}

	public void ShowCombatVSWnd(BattleInfo battleInfo, int paramId = 0, zbCombatRecorder restoreData = null, BattleResultInfo dataResult = null, RoleBaseInfo roleBaseInfo = null)
	{
		zbCombatVSWnd zbCombatVSWnd2 = ShowWnd(EWndId.CombatVS, "prefab/wnd/combat/combatVS.prefab") as zbCombatVSWnd;
		if ((Object)(object)zbCombatVSWnd2 != (Object)null)
		{
			zbCombatVSWnd2.StartBattle(battleInfo, paramId, restoreData, dataResult, roleBaseInfo);
		}
	}

	public void ShowCombatBalance()
	{
		ShowWnd(EWndId.CombatBalance, "prefab/wnd/combat/combatBalance.prefab");
	}

	public void ShowCombatResume(zbCombatRecorder recorder)
	{
		zbCombatResume zbCombatResume2 = ShowWnd(EWndId.CombatResume, "prefab/wnd/combat/combatResume.prefab") as zbCombatResume;
		if (Object.op_Implicit((Object)(object)zbCombatResume2))
		{
			zbCombatResume2.RefreshCustom(recorder);
		}
	}

	public void ShowSkillFightModWnd(int skillIndex, SkillDetail[] heroList, List<int> modList, bool isResumeFight = true)
	{
		zbSkillFightModWnd zbSkillFightModWnd2 = ShowWnd(EWndId.SkillFightMod, "prefab/wnd/combat/skillFightMod.prefab") as zbSkillFightModWnd;
		if (Object.op_Implicit((Object)(object)zbSkillFightModWnd2))
		{
			zbSkillFightModWnd2.RefreshCustom(skillIndex, heroList, modList, isResumeFight);
		}
	}

	public void ShowSelectMod(zbCombatTeam team, int selectNum = 1)
	{
		if (zbCombatSystem.Instance.dataResult != null)
		{
			foreach (BattleResultModDetail mod in zbCombatSystem.Instance.dataResult.AttackerTeamInfo.MasterTeamInfo.ModList)
			{
				if (mod.Level == (int)zbCombatSystem.Instance.combatData.combatExp.curLevel)
				{
					zbCombatSystem.Instance.combatData.masterTeam.UseSelectMode(mod.ModId);
				}
			}
			zbCombatWnd zbCombatWnd2 = GetWndById(EWndId.Combat) as zbCombatWnd;
			if ((Object)(object)zbCombatWnd2 != (Object)null)
			{
				zbCombatWnd2.refreshSkill();
			}
		}
		else
		{
			zbModSelectWnd zbModSelectWnd2 = ShowWnd(EWndId.SelectMod, "prefab/wnd/combat/modSelect.prefab") as zbModSelectWnd;
			if ((Object)(object)zbModSelectWnd2 != (Object)null)
			{
				zbModSelectWnd2.RefreshCustom(team, selectNum);
			}
		}
	}

	public void ShowModSelectRestoreData(zbCombatTeam team, int _selectNum, int _selectMax, int _curRefreshNum, List<int> _selectMod)
	{
		zbModSelectWnd zbModSelectWnd2 = ShowWnd(EWndId.SelectMod, "prefab/wnd/combat/modSelect.prefab") as zbModSelectWnd;
		if ((Object)(object)zbModSelectWnd2 != (Object)null)
		{
			zbModSelectWnd2.RefreshRestoreData(team, _selectNum, _selectMax, _curRefreshNum, _selectMod);
		}
	}

	public void ShowModSelectAction(List<int> modIdList, List<int> selectIndexList)
	{
		zbModSelectAction zbModSelectAction2 = ShowWnd(EWndId.ModSelectAction, "prefab/wnd/combat/modSelectAction.prefab") as zbModSelectAction;
		if ((Object)(object)zbModSelectAction2 != (Object)null)
		{
			zbModSelectAction2.RefreshCustom(modIdList, selectIndexList);
		}
	}

	public void ShowModWheelWnd(zbCombatTeam team, int monsterTid)
	{
		zbModWheelWnd zbModWheelWnd2 = ShowWnd(EWndId.ModWheel, "prefab/wnd/combat/modWheel.prefab") as zbModWheelWnd;
		if ((Object)(object)zbModWheelWnd2 != (Object)null)
		{
			zbModWheelWnd2.RefreshCustom(team, monsterTid);
		}
	}

	public zbBaseWnd ShowModWheelRestoreData(zbCombatTeam team, stCurrModWheel curModWheel)
	{
		zbModWheelWnd zbModWheelWnd2 = ShowWnd(EWndId.ModWheel, "prefab/wnd/combat/modWheel.prefab") as zbModWheelWnd;
		if ((Object)(object)zbModWheelWnd2 != (Object)null)
		{
			zbModWheelWnd2.RefreshRestoreData(team, curModWheel);
		}
		return zbModWheelWnd2;
	}

	public zbBaseWnd ShowAccount()
	{
		return ShowWnd(EWndId.Account, "prefab/wnd/login/account.prefab");
	}

	public void ShowSdkAccount(bool changeAct = false)
	{
		zbAcSdkWnd zbAcSdkWnd2 = ShowWnd(EWndId.SdkAccount, "prefab/wnd/login/sdkaccount.prefab") as zbAcSdkWnd;
		if ((Object)(object)zbAcSdkWnd2 != (Object)null)
		{
			zbAcSdkWnd2.RefreshCustom(changeAct);
		}
	}

	public zbBaseWnd ShowBubble()
	{
		return ShowWnd(EWndId.Bubble, "prefab/wnd/tip/bubble.prefab");
	}

	public void ShowCode()
	{
		ShowWnd(EWndId.Code, "prefab/wnd/code.prefab");
	}

	public void ShowSet()
	{
		ShowWnd(EWndId.Set, "prefab/wnd/set.prefab");
	}

	public void ShowLanguage()
	{
		ShowWnd(EWndId.Language, "prefab/wnd/setLanguage.prefab");
	}

	public void ShowSelf()
	{
		ShowWnd(EWndId.Self, "prefab/wnd/selfInfo.prefab");
	}

	public zbBaseWnd ShowFriends()
	{
		return ShowWnd(EWndId.Friends, "prefab/wnd/friend/friend.prefab");
	}

	public void ShowFriendsTili()
	{
		ShowWnd(EWndId.FriendsTili, "prefab/wnd/friend/friendTiLi.prefab");
	}

	public void ShowHead()
	{
		ShowWnd(EWndId.Head, "prefab/wnd/head.prefab");
	}

	public void ShowResetName()
	{
		zbResetNameWnd zbResetNameWnd2 = ShowWnd(EWndId.ResetName, "prefab/wnd/resetName.prefab") as zbResetNameWnd;
		if ((Object)(object)zbResetNameWnd2 != (Object)null)
		{
			zbResetNameWnd2.RefreshCustom();
		}
	}

	public void ShowMail()
	{
		ShowWnd(EWndId.Mail, "prefab/wnd/mail/mail.prefab");
	}

	public void ShowServerList()
	{
		zbServerListWnd zbServerListWnd2 = ShowWnd(EWndId.ServerList, "prefab/wnd/login/serverlist.prefab") as zbServerListWnd;
		if ((Object)(object)zbServerListWnd2 != (Object)null)
		{
			zbServerListWnd2.RefreshCustom();
		}
	}

	public void ShowMailDetail(MailDetail mailDetail, MailContent mailContent)
	{
		zbMailDetailWnd zbMailDetailWnd2 = ShowWnd(EWndId.MailDetail, "prefab/wnd/mail/mailDetail.prefab") as zbMailDetailWnd;
		if ((Object)(object)zbMailDetailWnd2 != (Object)null)
		{
			zbMailDetailWnd2.RefreshCustom(mailDetail, mailContent);
		}
	}

	public void ShowLeader(int tabIndex = 0)
	{
		zbLeaderWnd zbLeaderWnd2 = ShowWnd(EWndId.Leader, "prefab/wnd/leader/leader.prefab") as zbLeaderWnd;
		if ((Object)(object)zbLeaderWnd2 != (Object)null)
		{
			zbLeaderWnd2.RefreshCustom(tabIndex);
		}
	}

	public void ShowGemSuit()
	{
		ShowWnd(EWndId.GemSuit, "prefab/wnd/leader/gemSuit.prefab");
	}

	public void ShowEquipSuit()
	{
		ShowWnd(EWndId.EquipSuit, "prefab/wnd/leader/equipSuit.prefab");
	}

	public void ShowPreview(EPreviewType type, int propId, int star = 1, string strAttr = "")
	{
		if (type == EPreviewType.Wall)
		{
			ShowPreviewWall(propId, star);
			return;
		}
		zbPreviewWnd zbPreviewWnd2 = ShowWnd(EWndId.Preview, "prefab/wnd/player/preview.prefab") as zbPreviewWnd;
		if ((Object)(object)zbPreviewWnd2 != (Object)null)
		{
			zbPreviewWnd2.RefreshCustom(type, propId, star);
		}
	}

	public void ShowPreviewWall(int propId, int star)
	{
		zbPreviewWallWnd zbPreviewWallWnd2 = ShowWnd(EWndId.PreviewWall, "prefab/wnd/player/previewWall.prefab") as zbPreviewWallWnd;
		if ((Object)(object)zbPreviewWallWnd2 != (Object)null)
		{
			zbPreviewWallWnd2.RefreshCustom(propId, star);
		}
	}

	public void ShowPlayerSuit()
	{
		zbPlayerSuitWnd zbPlayerSuitWnd2 = ShowWnd(EWndId.PlaySuit, "prefab/wnd/player/playerSuit/playerSuit.prefab") as zbPlayerSuitWnd;
		if ((Object)(object)zbPlayerSuitWnd2 != (Object)null)
		{
			zbPlayerSuitWnd2.RefreshCustom(isFirst: true);
		}
	}

	public void ShowPlayerSuitSkill()
	{
		zbPlayerSuitSkillWnd zbPlayerSuitSkillWnd2 = ShowWnd(EWndId.PlaySuitSkill, "prefab/wnd/player/playerSuit/playerSuitSkill.prefab") as zbPlayerSuitSkillWnd;
		if ((Object)(object)zbPlayerSuitSkillWnd2 != (Object)null)
		{
			zbPlayerSuitSkillWnd2.RefreshCustom();
		}
	}

	public void ShowPlaySuitUpgrade(int _suitId)
	{
		zbPlayerSuitUpgradeWnd zbPlayerSuitUpgradeWnd2 = ShowWnd(EWndId.PlaySuitUpgrade, "prefab/wnd/player/playerSuit/playerSuitUpgrade.prefab") as zbPlayerSuitUpgradeWnd;
		if ((Object)(object)zbPlayerSuitUpgradeWnd2 != (Object)null)
		{
			zbPlayerSuitUpgradeWnd2.RefreshCustom(_suitId);
		}
	}

	public void ShowPlayerGun()
	{
		zbPlayerGunWnd zbPlayerGunWnd2 = ShowWnd(EWndId.PlayGun, "prefab/wnd/player/playerGun/playerGun.prefab") as zbPlayerGunWnd;
		if ((Object)(object)zbPlayerGunWnd2 != (Object)null)
		{
			zbPlayerGunWnd2.RefreshCustom();
		}
	}

	public void ShowPlayGunUpgrade(int _gunId)
	{
		zbPlayerGunUpgradeWnd zbPlayerGunUpgradeWnd2 = ShowWnd(EWndId.PlayGunUpgrade, "prefab/wnd/player/playerGun/playerGunUpgrade.prefab") as zbPlayerGunUpgradeWnd;
		if ((Object)(object)zbPlayerGunUpgradeWnd2 != (Object)null)
		{
			zbPlayerGunUpgradeWnd2.RefreshCustom(_gunId);
		}
	}

	public void ShowPlayerGunGet(int _gunId)
	{
		zbPlayerGunGetWnd zbPlayerGunGetWnd2 = ShowWnd(EWndId.PlayerGunGet, "prefab/wnd/player/playerGun/playerGunGet.prefab") as zbPlayerGunGetWnd;
		if ((Object)(object)zbPlayerGunGetWnd2 != (Object)null)
		{
			zbPlayerGunGetWnd2.RefreshCustom(_gunId);
		}
	}

	public void ShowPlayerGunStarSuccess(int _gunId)
	{
		zbPlayerGunStarSuccessWnd zbPlayerGunStarSuccessWnd2 = ShowWnd(EWndId.PlayerGunStarSuccess, "prefab/wnd/player/playerGun/playerGunStarSuccess.prefab") as zbPlayerGunStarSuccessWnd;
		if ((Object)(object)zbPlayerGunStarSuccessWnd2 != (Object)null)
		{
			zbPlayerGunStarSuccessWnd2.RefreshCustom(_gunId);
		}
	}

	public void ShowPlayerSuitConvert(PlayerSuitDetail[] playerSuitDetails)
	{
		zbPlayerSuitConvertWnd zbPlayerSuitConvertWnd2 = ShowWnd(EWndId.PlayerSuitConvert, "prefab/wnd/player/playerSuit/playerSuitConvert.prefab") as zbPlayerSuitConvertWnd;
		if ((Object)(object)zbPlayerSuitConvertWnd2 != (Object)null)
		{
			zbPlayerSuitConvertWnd2.RefreshCustom(playerSuitDetails);
		}
	}

	public void ShowPlayerGunConvert(PlayerGunDetail[] playerGunDetails)
	{
		zbPlayerGunConvertWnd zbPlayerGunConvertWnd2 = ShowWnd(EWndId.PlayerGunConvert, "prefab/wnd/player/playerGun/playerGunConvert.prefab") as zbPlayerGunConvertWnd;
		if ((Object)(object)zbPlayerGunConvertWnd2 != (Object)null)
		{
			zbPlayerGunConvertWnd2.RefreshCustom(playerGunDetails);
		}
	}

	public void ShowPlayerSuitUpgradePreview(int id, EPreviewType type, int index = -1)
	{
		zbPlayerSuitUpgradePreviewWnd zbPlayerSuitUpgradePreviewWnd2 = ShowWnd(EWndId.PlayerSuitUpgradePreview, "prefab/wnd/player/playerSuit/playerSuitUpgradePreview.prefab") as zbPlayerSuitUpgradePreviewWnd;
		if ((Object)(object)zbPlayerSuitUpgradePreviewWnd2 != (Object)null)
		{
			zbPlayerSuitUpgradePreviewWnd2.RefreshCustom(id, type, index);
		}
	}

	public void ShowPlayerSuitGet(int _suitId)
	{
		zbPlayerSuitGetWnd zbPlayerSuitGetWnd2 = ShowWnd(EWndId.PlayerSuitGet, "prefab/wnd/player/playerSuit/playerSuitGet.prefab") as zbPlayerSuitGetWnd;
		if ((Object)(object)zbPlayerSuitGetWnd2 != (Object)null)
		{
			zbPlayerSuitGetWnd2.RefreshCustom(_suitId);
		}
	}

	public void ShowPlayerSuitStarSuccess(int _suitId)
	{
		zbPlayerSuitStarSuccessWnd zbPlayerSuitStarSuccessWnd2 = ShowWnd(EWndId.PlayerSuitStarSuccess, "prefab/wnd/player/playerSuit/playerSuitStarSuccess.prefab") as zbPlayerSuitStarSuccessWnd;
		if ((Object)(object)zbPlayerSuitStarSuccessWnd2 != (Object)null)
		{
			zbPlayerSuitStarSuccessWnd2.RefreshCustom(_suitId);
		}
	}

	public void ShowAttrTip()
	{
		ShowWnd(EWndId.AttrTip, "prefab/wnd/leader/leaderTip.prefab");
	}

	public void ShowEquipSlot(EquipSlotInfo data, EEquipType equipType, bool visUpgrade = true)
	{
		zbEquipSlotWnd zbEquipSlotWnd2 = ShowWnd(EWndId.EquipSlot, "prefab/wnd/leader/equipSlot.prefab") as zbEquipSlotWnd;
		if ((Object)(object)zbEquipSlotWnd2 != (Object)null)
		{
			zbEquipSlotWnd2.RefreshCustom(data, equipType, visUpgrade);
		}
	}

	public void ShowEquipDecompose()
	{
		ShowWnd(EWndId.EquipDecompose, "prefab/wnd/leader/equipDecompose.prefab");
	}

	public void ShowEquipDecomposeSelect(int index, Action<List<EquipDetail>> callback)
	{
		zbEquipDecomposeSelectWnd zbEquipDecomposeSelectWnd2 = ShowWnd(EWndId.EquipDecomposeSelect, "prefab/wnd/leader/equipDecomposeSelect.prefab") as zbEquipDecomposeSelectWnd;
		if ((Object)(object)zbEquipDecomposeSelectWnd2 != (Object)null)
		{
			zbEquipDecomposeSelectWnd2.RefreshCustom(index, callback);
		}
	}

	public void ShowEquip(EquipDetail data)
	{
		zbEquipWnd zbEquipWnd2 = ShowWnd(EWndId.Equip, "prefab/wnd/leader/equip.prefab") as zbEquipWnd;
		if ((Object)(object)zbEquipWnd2 != (Object)null)
		{
			zbEquipWnd2.RefreshCustom(data);
		}
	}

	public void ShowEquipUpgrade(EEquipType equipType = EEquipType.Neuron, int tabIndex = 0)
	{
		zbEquipUpgradeWnd zbEquipUpgradeWnd2 = ShowWnd(EWndId.EquipUpgrade, "prefab/wnd/leader/equipUpgrade.prefab") as zbEquipUpgradeWnd;
		if ((Object)(object)zbEquipUpgradeWnd2 != (Object)null)
		{
			zbEquipUpgradeWnd2.RefreshCustom(equipType, tabIndex);
		}
	}

	public void ShowGemCompose(ResourceDetail[] previewList)
	{
		zbGemComposeWnd zbGemComposeWnd2 = ShowWnd(EWndId.GemCompose, "prefab/wnd/leader/gemCompose.prefab") as zbGemComposeWnd;
		if ((Object)(object)zbGemComposeWnd2 != (Object)null)
		{
			zbGemComposeWnd2.RefreshCustom(previewList);
		}
	}

	public void ShowGemInsert(ResourceDetail data)
	{
		zbGemInsertWnd zbGemInsertWnd2 = ShowWnd(EWndId.GemInsert, "prefab/wnd/leader/gemInsert.prefab") as zbGemInsertWnd;
		if ((Object)(object)zbGemInsertWnd2 != (Object)null)
		{
			zbGemInsertWnd2.RefreshCustom(data);
		}
	}

	public void ShowGemCast(ResourceDetail data)
	{
		zbGemCastWnd zbGemCastWnd2 = ShowWnd(EWndId.GemCast, "prefab/wnd/leader/gemCast.prefab") as zbGemCastWnd;
		if ((Object)(object)zbGemCastWnd2 != (Object)null)
		{
			zbGemCastWnd2.RefreshCustom(data);
		}
	}

	public void ShowGemDecompose(ResourceDetail data)
	{
		zbGemDecomposeWnd zbGemDecomposeWnd2 = ShowWnd(EWndId.GemDecompose, "prefab/wnd/leader/gemDecompose.prefab") as zbGemDecomposeWnd;
		if ((Object)(object)zbGemDecomposeWnd2 != (Object)null)
		{
			zbGemDecomposeWnd2.RefreshCustom(data);
		}
	}

	public zbBaseWnd ShowFormation()
	{
		return ShowWnd(EWndId.Formation, "prefab/wnd/hero/formation.prefab");
	}

	public void ShowHeroList()
	{
		ShowWnd(EWndId.HeroList, "prefab/wnd/hero/heroList.prefab");
	}

	public zbBaseWnd ShowHero(zbHeroCsvRecord record, int tabIndex = 0)
	{
		zbHeroWnd zbHeroWnd2 = ShowWnd(EWndId.Hero, "prefab/wnd/hero/hero.prefab") as zbHeroWnd;
		if ((Object)(object)zbHeroWnd2 != (Object)null)
		{
			zbHeroWnd2.RefreshCustom(record, tabIndex);
		}
		return zbHeroWnd2;
	}

	public void ShowHeroStar(zbHeroCsvRecord record)
	{
		zbHeroStarWnd zbHeroStarWnd2 = ShowWnd(EWndId.HeroStar, "prefab/wnd/hero/heroStar.prefab") as zbHeroStarWnd;
		if ((Object)(object)zbHeroStarWnd2 != (Object)null)
		{
			zbHeroStarWnd2.RefreshCustom(record);
		}
	}

	public void ShowHeroStarSuccess(int heroTid, int nextMod, bool isUpgrade, int preStar, int curStar)
	{
		zbHeroStarSuccessWnd zbHeroStarSuccessWnd2 = ShowWnd(EWndId.HeroStarSuccess, "prefab/wnd/hero/heroStarSuccess.prefab") as zbHeroStarSuccessWnd;
		if ((Object)(object)zbHeroStarSuccessWnd2 != (Object)null)
		{
			zbHeroStarSuccessWnd2.RefreshCustom(heroTid, nextMod, isUpgrade, preStar, curStar);
		}
	}

	public void ShowHeroDetail(zbHeroCsvRecord record, bool showleftRight = true)
	{
		zbHeroDetailWnd zbHeroDetailWnd2 = ShowWnd(EWndId.HeroDetail, "prefab/wnd/hero/heroDetail.prefab") as zbHeroDetailWnd;
		if ((Object)(object)zbHeroDetailWnd2 != (Object)null)
		{
			zbHeroDetailWnd2.RefreshCustom(record, showleftRight);
		}
	}

	public void ShowHeroFollow(ESkillType skillType)
	{
		zbHeroFollowWnd zbHeroFollowWnd2 = ShowWnd(EWndId.HeroFollow, "prefab/wnd/hero/heroFollow.prefab") as zbHeroFollowWnd;
		if ((Object)(object)zbHeroFollowWnd2 != (Object)null)
		{
			zbHeroFollowWnd2.RefreshCustom(skillType);
		}
	}

	public void ShowHeroRarity(int heroTid = -1)
	{
		zbHeroRarityWnd zbHeroRarityWnd2 = ShowWnd(EWndId.HeroRarity, "prefab/wnd/hero/heroRarity.prefab") as zbHeroRarityWnd;
		if ((Object)(object)zbHeroRarityWnd2 != (Object)null)
		{
			zbHeroRarityWnd2.RefreshCustom(heroTid);
		}
	}

	public void ShowHeroRaritySuccess(HeroDetail heroDetail)
	{
		zbHeroRaritySuccessWnd zbHeroRaritySuccessWnd2 = ShowWnd(EWndId.HeroRaritySuccess, "prefab/wnd/hero/heroRaritySuccess.prefab") as zbHeroRaritySuccessWnd;
		if ((Object)(object)zbHeroRaritySuccessWnd2 != (Object)null)
		{
			zbHeroRaritySuccessWnd2.RefreshCustom(heroDetail);
		}
	}

	public void ShowHeroRaritySelect(int heroTid, Action<int> callback)
	{
		zbHeroRaritySelectWnd zbHeroRaritySelectWnd2 = ShowWnd(EWndId.HeroRaritySelect, "prefab/wnd/hero/heroRaritySelect.prefab") as zbHeroRaritySelectWnd;
		if ((Object)(object)zbHeroRaritySelectWnd2 != (Object)null)
		{
			zbHeroRaritySelectWnd2.RefreshCustom(heroTid, callback);
		}
	}

	public void ShowHeroRarityFragSelect(int needCount, Action<bool> callback)
	{
		zbHeroRarityFragSelectWnd zbHeroRarityFragSelectWnd2 = ShowWnd(EWndId.HeroRarityFragSelect, "prefab/wnd/hero/heroRarityFragSelect.prefab") as zbHeroRarityFragSelectWnd;
		if ((Object)(object)zbHeroRarityFragSelectWnd2 != (Object)null)
		{
			zbHeroRarityFragSelectWnd2.RefreshCustom(needCount, callback);
		}
	}

	public void ShowHeroGet(int[] heroTids, Action<bool> callback = null)
	{
		zbHeroGetWnd zbHeroGetWnd2 = ShowWnd(EWndId.HeroGet, "prefab/wnd/hero/heroGet.prefab") as zbHeroGetWnd;
		if ((Object)(object)zbHeroGetWnd2 != (Object)null)
		{
			zbHeroGetWnd2.RefreshCustom(heroTids, callback);
		}
	}

	public void ShowHeroDecompose()
	{
		ShowWnd(EWndId.HeroDecompose, "prefab/wnd/hero/heroDecompose.prefab");
	}

	public void ShowHeroDecomposeAuto()
	{
		ShowWnd(EWndId.HeroDecomposeAuto, "prefab/wnd/hero/heroDecomposeAuto.prefab");
	}

	public void ShowHeroDecomposeSel(zbHeroDecomposeWnd wndHeroDecompose)
	{
		zbHeroDecomposeSelWnd zbHeroDecomposeSelWnd2 = ShowWnd(EWndId.HeroDecomposeSel, "prefab/wnd/hero/heroDecomposeSel.prefab") as zbHeroDecomposeSelWnd;
		if ((Object)(object)zbHeroDecomposeSelWnd2 != (Object)null)
		{
			zbHeroDecomposeSelWnd2.RefreshCustom(wndHeroDecompose);
		}
	}

	public void ShowHeroDecomposeSelCount(ResourceDetail data, Action<ResourceDetail, long> callback)
	{
		zbHeroDecomposeSelCountWnd zbHeroDecomposeSelCountWnd2 = ShowWnd(EWndId.HeroDecomposeSelCount, "prefab/wnd/hero/heroDecomposeSelCount.prefab") as zbHeroDecomposeSelCountWnd;
		if ((Object)(object)zbHeroDecomposeSelCountWnd2 != (Object)null)
		{
			zbHeroDecomposeSelCountWnd2.RefreshCustom(data, callback);
		}
	}

	public zbBaseWnd ShowGene()
	{
		return ShowWnd(EWndId.Gene, "prefab/wnd/hero/gene.prefab");
	}

	public void ShowGeneAuto(ESkillType skillType, Action<bool> callback = null)
	{
		zbGeneAutoWnd zbGeneAutoWnd2 = ShowWnd(EWndId.GeneAuto, "prefab/wnd/hero/geneAuto.prefab") as zbGeneAutoWnd;
		if ((Object)(object)zbGeneAutoWnd2 != (Object)null)
		{
			zbGeneAutoWnd2.RefreshCustom(skillType, callback);
		}
	}

	public void ShowGeneAutoModel(ESkillType skillType, Action<bool> callback = null)
	{
		zbGeneAutoModelWnd zbGeneAutoModelWnd2 = ShowWnd(EWndId.GeneAutoModel, "prefab/wnd/hero/geneAutoModel.prefab") as zbGeneAutoModelWnd;
		if ((Object)(object)zbGeneAutoModelWnd2 != (Object)null)
		{
			zbGeneAutoModelWnd2.RefreshCustom(skillType, callback);
		}
	}

	public void ShowHeroGeneSelect(GeneSlotDetail geneSlotDetail, ESkillType skillType, int index)
	{
		zbHeroGeneSelectWnd zbHeroGeneSelectWnd2 = ShowWnd(EWndId.HeroGeneSelect, "prefab/wnd/hero/heroGeneSelect.prefab") as zbHeroGeneSelectWnd;
		if ((Object)(object)zbHeroGeneSelectWnd2 != (Object)null)
		{
			zbHeroGeneSelectWnd2.RefreshCustom(geneSlotDetail, skillType, index);
		}
	}

	public void ShowGeneSelect(int geneTid, int index)
	{
		zbGeneSelectWnd zbGeneSelectWnd2 = ShowWnd(EWndId.GeneSelect, "prefab/wnd/hero/geneSelect.prefab") as zbGeneSelectWnd;
		if ((Object)(object)zbGeneSelectWnd2 != (Object)null)
		{
			zbGeneSelectWnd2.RefreshCustom(geneTid, index);
		}
	}

	public void ShowGeneCompose(int geneTid, ESkillType skillType, int index)
	{
		zbGeneComposeWnd zbGeneComposeWnd2 = ShowWnd(EWndId.GeneCompose, "prefab/wnd/hero/geneCompose.prefab") as zbGeneComposeWnd;
		if ((Object)(object)zbGeneComposeWnd2 != (Object)null)
		{
			zbGeneComposeWnd2.RefreshCustom(geneTid, skillType, index);
		}
	}

	public void ShowGeneGeneComposeCalc(AwardDetail[] awardList, int index)
	{
		zbGeneComposeCalcWnd zbGeneComposeCalcWnd2 = ShowWnd(EWndId.GeneComposeCalc, "prefab/wnd/hero/geneComposeCalc.prefab") as zbGeneComposeCalcWnd;
		if ((Object)(object)zbGeneComposeCalcWnd2 != (Object)null)
		{
			zbGeneComposeCalcWnd2.RefreshCustom(awardList, index);
		}
	}

	public void ShowGeneTip(List<Dictionary<string, object>> geneInfo, Vector2 vector2)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		zbGeneTipWnd zbGeneTipWnd2 = ShowWnd(EWndId.GeneTip, "prefab/wnd/hero/heroGeneTip.prefab") as zbGeneTipWnd;
		if ((Object)(object)zbGeneTipWnd2 != (Object)null)
		{
			zbGeneTipWnd2.RefreshCustom(geneInfo, vector2);
		}
	}

	public void ShowGeneRule()
	{
		zbGeneRuleWnd zbGeneRuleWnd2 = ShowWnd(EWndId.GeneRule, "prefab/wnd/hero/geneRule.prefab") as zbGeneRuleWnd;
		if ((Object)(object)zbGeneRuleWnd2 != (Object)null)
		{
			string[] langKeys = new string[5] { "lang.10909", "lang.10910", "lang.10911", "lang.10912", "lang.10913" };
			zbGeneRuleWnd2.RefreshCustom(langKeys);
		}
	}

	public void ShowHeroTrial()
	{
		ShowWnd(EWndId.HeroTrial, "prefab/wnd/heroTrial/heroTrial.prefab");
	}

	public void ShowElementTrial()
	{
		zbElementTrialWnd zbElementTrialWnd2 = ShowWnd(EWndId.ElementTrial, "prefab/wnd/heroTrial/elementTrial.prefab") as zbElementTrialWnd;
		if ((Object)(object)zbElementTrialWnd2 != (Object)null)
		{
			zbElementTrialWnd2.RefreshCustom();
		}
	}

	public void ShowBag(int tabIndex = 0)
	{
		zbBagWnd zbBagWnd2 = ShowWnd(EWndId.Bag, "prefab/wnd/bag/bag.prefab") as zbBagWnd;
		if ((Object)(object)zbBagWnd2 != (Object)null)
		{
			zbBagWnd2.RefreshCustom(tabIndex);
		}
	}

	public void ShowBagItemDetail(zbItemData data, ESkillType skillType = ESkillType.EmptyValue, int index = 0)
	{
		zbBagItemDetailWnd zbBagItemDetailWnd2 = ShowWnd(EWndId.BagItemDetail, "prefab/wnd/bag/bagItemDetail.prefab") as zbBagItemDetailWnd;
		if ((Object)(object)zbBagItemDetailWnd2 != (Object)null)
		{
			zbBagItemDetailWnd2.RefreshCustom(data, skillType, index);
		}
	}

	public void ShowBagItemSale(zbItemData data)
	{
		zbBagItemSaleWnd zbBagItemSaleWnd2 = ShowWnd(EWndId.BagItemSale, "prefab/wnd/bag/bagItemSale.prefab") as zbBagItemSaleWnd;
		if ((Object)(object)zbBagItemSaleWnd2 != (Object)null)
		{
			zbBagItemSaleWnd2.RefreshCustom(data);
		}
	}

	public void ShowBagRandBoxOpen(zbItemData data)
	{
		zbBagRandBoxOpenWnd zbBagRandBoxOpenWnd2 = ShowWnd(EWndId.BagRandBoxOpen, "prefab/wnd/bag/bagRandBoxOpen.prefab") as zbBagRandBoxOpenWnd;
		if ((Object)(object)zbBagRandBoxOpenWnd2 != (Object)null)
		{
			zbBagRandBoxOpenWnd2.RefreshCustom(data);
		}
	}

	public void ShowBoxKeyOpen(zbItemData itemData, Action<bool> callback)
	{
		zbBoxKeyOpenWnd zbBoxKeyOpenWnd2 = ShowWnd(EWndId.BoxKeyOpen, "prefab/wnd/bag/boxKeyOpen.prefab") as zbBoxKeyOpenWnd;
		if ((Object)(object)zbBoxKeyOpenWnd2 != (Object)null)
		{
			zbBoxKeyOpenWnd2.RefreshCustom(itemData, callback);
		}
	}

	public void ShowBoxCompose(zbItemData data)
	{
		zbBoxComposeWnd zbBoxComposeWnd2 = ShowWnd(EWndId.BoxCompose, "prefab/wnd/bag/boxCompose.prefab") as zbBoxComposeWnd;
		if ((Object)(object)zbBoxComposeWnd2 != (Object)null)
		{
			zbBoxComposeWnd2.RefreshCustom(data);
		}
	}

	public void ShowBoxOpen(AwardDetail[] awardList, int boxTid)
	{
		zbBoxOpenWnd zbBoxOpenWnd2 = ShowWnd(EWndId.BoxOpen, "prefab/wnd/bag/boxOpen.prefab") as zbBoxOpenWnd;
		if ((Object)(object)zbBoxOpenWnd2 != (Object)null)
		{
			zbBoxOpenWnd2.RefreshCustom(awardList, boxTid);
		}
	}

	public void ShowEncyclopedia(int tab)
	{
		zbEncyclopediaWnd zbEncyclopediaWnd2 = ShowWnd(EWndId.Encyclopedia, "prefab/wnd/encyclopedia/encyclopedia.prefab") as zbEncyclopediaWnd;
		if ((Object)(object)zbEncyclopediaWnd2 != (Object)null)
		{
			zbEncyclopediaWnd2.RefreshCustom(tab);
		}
	}

	public void ShowModInfoList(zbModHandbookTypeCsvRecord record)
	{
		zbModInfoWnd zbModInfoWnd2 = ShowWnd(EWndId.ModInfoList, "prefab/wnd/encyclopedia/modInfoList.prefab") as zbModInfoWnd;
		if (Object.op_Implicit((Object)(object)zbModInfoWnd2))
		{
			zbModInfoWnd2.RefreshCustom(record);
		}
	}

	public void ShowModDetail(zbModHandbookCsvRecord mod)
	{
		zbModDetailWnd zbModDetailWnd2 = ShowWnd(EWndId.ModDetail, "prefab/wnd/encyclopedia/modDetail.prefab") as zbModDetailWnd;
		if (Object.op_Implicit((Object)(object)zbModDetailWnd2))
		{
			zbModDetailWnd2.RefreshCustom(mod);
		}
	}

	public void ShowMonsterDetail(zbHandbookCsvRecord record, MonsterHandbookDetail monsterDetail = null)
	{
		zbMonsterDetailWnd zbMonsterDetailWnd2 = ShowWnd(EWndId.MonsterDetail, "prefab/wnd/monsterDetail.prefab") as zbMonsterDetailWnd;
		if (Object.op_Implicit((Object)(object)zbMonsterDetailWnd2))
		{
			zbMonsterDetailWnd2.RefreshCustom(record, monsterDetail);
		}
	}

	public void ShowPreviewMonsterDetail(zbMonsterCsvRecord record)
	{
		zbPreViewMonsterDetailWnd zbPreViewMonsterDetailWnd2 = ShowWnd(EWndId.PreViewMonsterDetail, "prefab/wnd/preViewMonsterDetail.prefab") as zbPreViewMonsterDetailWnd;
		if (Object.op_Implicit((Object)(object)zbPreViewMonsterDetailWnd2))
		{
			zbPreViewMonsterDetailWnd2.RefreshCustom(record);
		}
	}

	public void ShowVip(int tabIndex = 0, int subTabIndex = 0, int diff = 0)
	{
		zbVipWnd zbVipWnd2 = ShowWnd(EWndId.Vip, "prefab/wnd/vip/vip.prefab") as zbVipWnd;
		if ((Object)(object)zbVipWnd2 != (Object)null)
		{
			zbVipWnd2.RefreshCustom(tabIndex, subTabIndex, diff);
		}
	}

	public void ShowVipIosReviewShop()
	{
		zbVipIosReviewShop zbVipIosReviewShop2 = ShowWnd(EWndId.VipIosReviewShop, "prefab/wnd/vip/vipiosreviewshop.prefab") as zbVipIosReviewShop;
		if ((Object)(object)zbVipIosReviewShop2 != (Object)null)
		{
			zbVipIosReviewShop2.Refresh();
		}
	}

	public void ShowRebateAwardWnd()
	{
		zbRebateAwardWnd zbRebateAwardWnd2 = ShowWnd(EWndId.RebateAward, "prefab/wnd/vip/rebateaward.prefab") as zbRebateAwardWnd;
		if ((Object)(object)zbRebateAwardWnd2 != (Object)null)
		{
			zbRebateAwardWnd2.Refresh();
		}
	}

	public void ShowVipTequanWnd()
	{
		zbVipTequanWnd zbVipTequanWnd2 = ShowWnd(EWndId.VipTequan, "prefab/wnd/vip/viptequan.prefab") as zbVipTequanWnd;
		if ((Object)(object)zbVipTequanWnd2 != (Object)null)
		{
			zbVipTequanWnd2.Refresh();
		}
	}

	public void ShowVipTequanGiftGetPopWnd(int curTabLv)
	{
		zbVipTequanGiftGetPopWnd zbVipTequanGiftGetPopWnd2 = ShowWnd(EWndId.VipTequanGiftGetPop, "prefab/wnd/vip/vipgiftgetpop.prefab") as zbVipTequanGiftGetPopWnd;
		if ((Object)(object)zbVipTequanGiftGetPopWnd2 != (Object)null)
		{
			zbVipTequanGiftGetPopWnd2.RefreshCustom(curTabLv);
		}
	}

	public void ShowVipLevelUpWnd()
	{
		zbVipLevelUpWnd zbVipLevelUpWnd2 = ShowWnd(EWndId.ViplevelUp, "prefab/wnd/vip/viplevelup.prefab") as zbVipLevelUpWnd;
		if ((Object)(object)zbVipLevelUpWnd2 != (Object)null)
		{
			zbVipLevelUpWnd2.Refresh();
		}
	}

	public void ShowPayFunc(int goodsId)
	{
		zbPayFunc zbPayFunc2 = ShowWnd(EWndId.PayFunc, "prefab/wnd/vip/payfunc.prefab") as zbPayFunc;
		if ((Object)(object)zbPayFunc2 != (Object)null)
		{
			zbPayFunc2.RefreshCustom(goodsId);
		}
	}

	public void ShowLottery(int tabIndex)
	{
		zbLotteryWnd zbLotteryWnd2 = ShowWnd(EWndId.Lottery, "prefab/wnd/vip/lottery.prefab") as zbLotteryWnd;
		if ((Object)(object)zbLotteryWnd2 != (Object)null)
		{
			zbLotteryWnd2.Refresh(tabIndex);
		}
	}

	public void ShowRecharge(int diff)
	{
		zbVipWnd zbVipWnd2 = ShowWnd(EWndId.Vip, "prefab/wnd/vip/vip.prefab") as zbVipWnd;
		if ((Object)(object)zbVipWnd2 != (Object)null)
		{
			zbVipWnd2.RefreshCustom(2, 0, diff);
		}
	}

	public void ShowVipPaying()
	{
		ShowWnd(EWndId.VipPaying, "prefab/wnd/vip/vippaying.prefab");
	}

	public void ShowVipRefund()
	{
		ShowWnd(EWndId.VipRefund, "prefab/wnd/vip/viprefund.prefab");
	}

	public void ShowShop(int index = 0)
	{
		zbShopWnd zbShopWnd2 = ShowWnd(EWndId.Shop, "prefab/wnd/shop/shop.prefab") as zbShopWnd;
		if ((Object)(object)zbShopWnd2 != (Object)null)
		{
			zbShopWnd2.RefreshCustom(index);
		}
	}

	public void ShowShopConfirm(EShopType shopType, ShopSlotDetail data)
	{
		zbShopConfirmWnd zbShopConfirmWnd2 = ShowWnd(EWndId.ShopConfirm, "prefab/wnd/shop/shopConfirm.prefab") as zbShopConfirmWnd;
		if ((Object)(object)zbShopConfirmWnd2 != (Object)null)
		{
			zbShopConfirmWnd2.RefreshCustom(shopType, data);
		}
	}

	public void ShowBar(EBarType barType, AwardDetail[] awardList)
	{
		zbBarWnd zbBarWnd2 = ShowWnd(EWndId.Bar, "prefab/wnd/vip/bar.prefab") as zbBarWnd;
		if ((Object)(object)zbBarWnd2 != (Object)null)
		{
			zbBarWnd2.RefreshCustom(barType, awardList);
		}
	}

	public void ShowBarRateNormal()
	{
		ShowWnd(EWndId.BarRateNormal, "prefab/wnd/vip/barRateNormal.prefab");
	}

	public void ShowBarRateSpecial()
	{
		ShowWnd(EWndId.BarRateSpecial, "prefab/wnd/vip/barRateSpecial.prefab");
	}

	public void ShowBarRateOperate()
	{
		ShowWnd(EWndId.BarRateOperate, "prefab/wnd/vip/barRateOperate.prefab");
	}

	public void ShowBarScore(LimitTimeHeroDetail timeHeroDetail)
	{
		zbBarScoreWnd zbBarScoreWnd2 = ShowWnd(EWndId.BarScore, "prefab/wnd/vip/barScore.prefab") as zbBarScoreWnd;
		if ((Object)(object)zbBarScoreWnd2 != (Object)null)
		{
			zbBarScoreWnd2.RefreshCustom(timeHeroDetail);
		}
	}

	public void ShowAuxResourceConvert(zbResourceConvertCsvRecord record, int count)
	{
		zbAuxResourceConvertWnd zbAuxResourceConvertWnd2 = ShowWnd(EWndId.AuxResourceConvert, "prefab/wnd/comm/auxResourceConvert.prefab") as zbAuxResourceConvertWnd;
		if ((Object)(object)zbAuxResourceConvertWnd2 != (Object)null)
		{
			zbAuxResourceConvertWnd2.RefreshCustom(record, count);
		}
	}

	public void ShowLevelUp()
	{
		ShowWnd(EWndId.LevelUp, "prefab/wnd/tip/levelUp.prefab");
	}

	public void ShowCommonLevelUp()
	{
		ShowWnd(EWndId.CommonLevelUp, "prefab/wnd/tip/commonLevelUp.prefab");
	}

	public void ShowFightValueUp(int lastFv)
	{
		zbFightValueUpWnd zbFightValueUpWnd2 = ShowWnd(EWndId.FightValueUp, "prefab/wnd/tip/fightValueUp.prefab") as zbFightValueUpWnd;
		if ((Object)(object)zbFightValueUpWnd2 != (Object)null)
		{
			zbFightValueUpWnd2.RefreshCustom(lastFv);
		}
	}

	public void ShowTipConnect(bool bNeedTimeOutClose = true, float effectDelay = 1.5f)
	{
		zbTipConnectWnd zbTipConnectWnd2 = ShowWnd(EWndId.TipConnect, "prefab/wnd/tip/tipconnect.prefab") as zbTipConnectWnd;
		if ((Object)(object)zbTipConnectWnd2 != (Object)null)
		{
			zbTipConnectWnd2.RefreshCustom(bNeedTimeOutClose, effectDelay);
		}
	}

	public void ShowTouchBlockWnd(float hideTm)
	{
		zbTouchBlockWnd zbTouchBlockWnd2 = ShowWnd(EWndId.TouchBlock, "prefab/wnd/tip/touchblock.prefab") as zbTouchBlockWnd;
		if ((Object)(object)zbTouchBlockWnd2 != (Object)null)
		{
			zbTouchBlockWnd2.RefreshCustom(hideTm);
		}
	}

	public void ShowConnectFaild()
	{
		ShowWnd(EWndId.ConnectFaildTip, "prefab/wnd/tip/wsConnectFaild.prefab");
	}

	public void ShowTipNewVersion()
	{
		ShowWnd(EWndId.TipNewVersion, "prefab/wnd/tip/tipnewversion.prefab");
	}

	public void ShowTip(EResourceType type, long id, long needCount = 0L)
	{
		if (type == EResourceType.Hero && !IsShow(EWndId.HeroDetail))
		{
			zbHeroCsvRecord recordByKey = zbDataConfig.Instance.heroCsvTable.GetRecordByKey((int)id);
			ShowHeroDetail(recordByKey, showleftRight: false);
			return;
		}
		List<EWndId> obj = new List<EWndId>
		{
			EWndId.TipCommon1,
			EWndId.TipCommon2,
			EWndId.TipCommon3,
			EWndId.TipCommon4,
			EWndId.TipCommon5
		};
		EWndId id2 = EWndId.TipCommon1;
		foreach (EWndId item in obj)
		{
			if (!Instance.IsShow(item))
			{
				id2 = item;
				break;
			}
		}
		zbTipCommonWnd zbTipCommonWnd2 = ShowWnd(id2, "prefab/wnd/tip/tipCommon.prefab") as zbTipCommonWnd;
		if ((Object)(object)zbTipCommonWnd2 != (Object)null)
		{
			zbTipCommonWnd2.RefreshCustom(type, id, needCount);
		}
	}

	public void ShowTipSkillType(int skillType, Vector2 pos)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		zbTipSkillTypeWnd zbTipSkillTypeWnd2 = ShowWnd(EWndId.TipSkillType, "prefab/wnd/tip/tipSkillType.prefab") as zbTipSkillTypeWnd;
		if ((Object)(object)zbTipSkillTypeWnd2 != (Object)null)
		{
			zbTipSkillTypeWnd2.RefreshCustom(skillType, pos);
		}
	}

	public void ShowTipSkill(int skillId, Vector2 pos)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		zbTipSkillWnd zbTipSkillWnd2 = ShowWnd(EWndId.TipSkill, "prefab/wnd/tip/tipSkill.prefab") as zbTipSkillWnd;
		if ((Object)(object)zbTipSkillWnd2 != (Object)null)
		{
			zbTipSkillWnd2.RefreshCustom(skillId, pos);
		}
	}

	public void ShowTipAttr(ESkillType skillType, WarRoomAttrDetail attrDetail, Vector2 pos)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		zbTipAttrWnd zbTipAttrWnd2 = ShowWnd(EWndId.TipAttr, "prefab/wnd/tip/tipAttr.prefab") as zbTipAttrWnd;
		if ((Object)(object)zbTipAttrWnd2 != (Object)null)
		{
			zbTipAttrWnd2.RefreshCustom(skillType, attrDetail, pos);
		}
	}

	public void ShowTipMod(string desc, Vector2 pos)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		zbTipModWnd zbTipModWnd2 = ShowWnd(EWndId.TipMod, "prefab/wnd/tip/tipMod.prefab") as zbTipModWnd;
		if ((Object)(object)zbTipModWnd2 != (Object)null)
		{
			zbTipModWnd2.RefreshCustom(desc, pos);
		}
	}

	public void ShowMoneyExchange(zbMoneyExchangeCsvRecord record)
	{
		zbMoneyExchangeWnd zbMoneyExchangeWnd2 = ShowWnd(EWndId.MoneyExchange, "prefab/wnd/moneyExchange.prefab") as zbMoneyExchangeWnd;
		if ((Object)(object)zbMoneyExchangeWnd2 != (Object)null)
		{
			zbMoneyExchangeWnd2.RefreshCustom(record);
		}
	}

	public void ShowChat()
	{
		ShowWnd(EWndId.Chat, "prefab/wnd/chat/chat.prefab");
	}

	public void ShowLegion()
	{
		ShowWnd(EWndId.Legion, "prefab/wnd/legion/legion.prefab");
	}

	public void ShowLegionChangeInfo()
	{
		ShowWnd(EWndId.LegionChangeInfo, "prefab/wnd/legion/legionChangeInfo.prefab");
	}

	public void ShowLegionMembers()
	{
		zbLegionMembersWnd zbLegionMembersWnd2 = ShowWnd(EWndId.LegionMembers, "prefab/wnd/legion/legionMembers.prefab") as zbLegionMembersWnd;
		if ((Object)(object)zbLegionMembersWnd2 != (Object)null)
		{
			zbLegionMembersWnd2.RefreshCustom();
		}
	}

	public void ShowLegionDinner()
	{
		ShowWnd(EWndId.LegionDinner, "prefab/wnd/legion/legionDinner.prefab");
	}

	public void ShowLegionDonate()
	{
		ShowWnd(EWndId.LegionDonate, "prefab/wnd/legion/legionDonate.prefab");
	}

	public void ShowLegionOther()
	{
		ShowWnd(EWndId.LegionOther, "prefab/wnd/legion/legionOther.prefab");
	}

	public void ShowLegionAuthority()
	{
		ShowWnd(EWndId.LegionAuthority, "prefab/wnd/legion/legionAuthority.prefab");
	}

	public void ShowLegionList()
	{
		ShowWnd(EWndId.LegionList, "prefab/wnd/legion/legionList.prefab");
	}

	public void ShowLegionLeave()
	{
		ShowWnd(EWndId.LegionLeave, "prefab/wnd/legion/legionLeaveConfirm.prefab");
	}

	public void ShowLegionCreate()
	{
		ShowWnd(EWndId.LegionCreate, "prefab/wnd/legion/legionCreate.prefab");
	}

	public void ShowLegionPosition(long playerId)
	{
		zbLegionPositionWnd zbLegionPositionWnd2 = ShowWnd(EWndId.LegionPosition, "prefab/wnd/legion/legionPosition.prefab") as zbLegionPositionWnd;
		if ((Object)(object)zbLegionPositionWnd2 != (Object)null)
		{
			zbLegionPositionWnd2.RefreshCustom(playerId);
		}
	}

	public void ShowLegionTip(LegionTipsInfo tipsInfo)
	{
		zbLegionTipWnd zbLegionTipWnd2 = ShowWnd(EWndId.LegionTip, "prefab/wnd/legion/legionTip.prefab") as zbLegionTipWnd;
		if (Object.op_Implicit((Object)(object)zbLegionTipWnd2))
		{
			zbLegionTipWnd2.RefreshCustom(tipsInfo);
		}
	}

	public void ShowLegionRecommend()
	{
		ShowWnd(EWndId.LegionRecommendList, "prefab/wnd/legion/legionRecommend.prefab");
	}

	public void ShowLegionNotice()
	{
		ShowWnd(EWndId.LegionNotice, "prefab/wnd/legion/legionNoticeChange.prefab");
	}

	public void ShowLegionDetail()
	{
		ShowWnd(EWndId.LegionDetail, "prefab/wnd/legion/legionDetail.prefab");
	}

	public void ShowLegionApplyLimited()
	{
		ShowWnd(EWndId.LegionLimited, "prefab/wnd/legion/legionApplyLimit.prefab");
	}

	public void ShowLegionPrivilege()
	{
		ShowWnd(EWndId.LegionPrivilege, "prefab/wnd/legion/legionPrivilege.prefab");
	}

	public void ShowLegionApply()
	{
		ShowWnd(EWndId.LegionApply, "prefab/wnd/legion/legionApply.prefab");
	}

	public void ShowLegionClearCd()
	{
		ShowWnd(EWndId.LegionClearCd, "prefab/wnd/legion/legionClearCd.prefab");
	}

	public void ShowLegionBoss()
	{
		ShowWnd(EWndId.LegionBoss, "prefab/wnd/legion/legionBoss.prefab");
	}

	public void ShowLegionBossRank(RankDetail[] rankList)
	{
		zbLegionBossRankWnd zbLegionBossRankWnd2 = ShowWnd(EWndId.LegionBossRank, "prefab/wnd/legion/legionBossRank.prefab") as zbLegionBossRankWnd;
		if ((Object)(object)zbLegionBossRankWnd2 != (Object)null)
		{
			zbLegionBossRankWnd2.RefreshCustom(rankList);
		}
	}

	public void ShowLegionBossAward()
	{
		ShowWnd(EWndId.LegionBossAward, "prefab/wnd/legion/legionBossAward.prefab");
	}

	public void ShowLegionBossRecord(LegionBossKillRecordDetail[] bossKillRecordDetails)
	{
		zbLegionBossRecordWnd zbLegionBossRecordWnd2 = ShowWnd(EWndId.LegionBossRecord, "prefab/wnd/legion/legionBossRecord.prefab") as zbLegionBossRecordWnd;
		if ((Object)(object)zbLegionBossRecordWnd2 != (Object)null)
		{
			zbLegionBossRecordWnd2.refreshCustom(bossKillRecordDetails);
		}
	}

	public void ShowLegionWarZhanling()
	{
		zbLegionWarZhanlingWnd zbLegionWarZhanlingWnd2 = ShowWnd(EWndId.LegionWarZhanling, "prefab/wnd/legionwar/legionWarZhanling.prefab") as zbLegionWarZhanlingWnd;
		if ((Object)(object)zbLegionWarZhanlingWnd2 != (Object)null)
		{
			zbLegionWarZhanlingWnd2.RefreshCustom();
		}
	}

	public void ShowLegionWarPreview()
	{
		ShowWnd(EWndId.LegionWarPreview, "prefab/wnd/legionWar/legionWarPreview.prefab");
	}

	public void ShowLegionWarApply()
	{
		ShowWnd(EWndId.LegionWarApply, "prefab/wnd/legionWar/legionWarApply.prefab");
	}

	public void ShowLegionWarSettle()
	{
		ShowWnd(EWndId.LegionWarSettle, "prefab/wnd/legionWar/legionWarSettle.prefab");
	}

	public void ShowLegionWarSettlePreview(ELegionWarForce eLegionWarForce, int redScoreCount, int blueScoreCount)
	{
		zbLegionWarSettlePreviewWnd zbLegionWarSettlePreviewWnd2 = ShowWnd(EWndId.LegionWarSettlePreview, "prefab/wnd/legionWar/legionWarSettlePreview.prefab") as zbLegionWarSettlePreviewWnd;
		if ((Object)(object)zbLegionWarSettlePreviewWnd2 != (Object)null)
		{
			zbLegionWarSettlePreviewWnd2.RefreshCustom(eLegionWarForce, redScoreCount, blueScoreCount);
		}
	}

	public void ShowLegionWarRank()
	{
		ShowWnd(EWndId.LegionWarRank, "prefab/wnd/legionWar/legionWarRank.prefab");
	}

	public void ShowLegionWarRankAward(int playerRank, int legionRank, int tabIndex = 0)
	{
		zbLegionWarRankAwardWnd zbLegionWarRankAwardWnd2 = ShowWnd(EWndId.LegionWarRankAward, "prefab/wnd/legionWar/legionWarRankAward.prefab") as zbLegionWarRankAwardWnd;
		if ((Object)(object)zbLegionWarRankAwardWnd2 != (Object)null)
		{
			zbLegionWarRankAwardWnd2.RefreshCustom(playerRank, legionRank, tabIndex);
		}
	}

	public void ShowLegionWarPointTip(LegionWarPointStateDetail legionWarPointInfo, Vector2 pos2)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		zbLegionWarPointTipWnd zbLegionWarPointTipWnd2 = ShowWnd(EWndId.LegionWarPointTip, "prefab/wnd/legionWar/legionWarPointTip.prefab") as zbLegionWarPointTipWnd;
		if ((Object)(object)zbLegionWarPointTipWnd2 != (Object)null)
		{
			zbLegionWarPointTipWnd2.RefreshCustom(legionWarPointInfo, pos2);
		}
	}

	public void ShowLegionWarMainTip(int PointId, Vector2 pos2)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		zbLegionWarMainTipWnd zbLegionWarMainTipWnd2 = ShowWnd(EWndId.LegionWarMainTip, "prefab/wnd/legionWar/legionWarMainTip.prefab") as zbLegionWarMainTipWnd;
		if ((Object)(object)zbLegionWarMainTipWnd2 != (Object)null)
		{
			zbLegionWarMainTipWnd2.RefreshCustom(PointId, pos2);
		}
	}

	public void ShowLegionWarEnter()
	{
		ShowWnd(EWndId.LegionWarEnter, "prefab/wnd/legionWar/legionWarEnter.prefab");
	}

	public void ShowLegionWarMod()
	{
		zbLegionWarModWnd zbLegionWarModWnd2 = ShowWnd(EWndId.LegionWarMod, "prefab/wnd/legionWar/legionWarMod.prefab") as zbLegionWarModWnd;
		if ((Object)(object)zbLegionWarModWnd2 != (Object)null)
		{
			zbLegionWarModWnd2.RefreshCustom(zbSelf.Instance.legionWarHandler.legionWarSelfTeamInfo);
		}
	}

	public void ShowLegionWarBeback()
	{
		ShowWnd(EWndId.LegionWarBeback, "prefab/wnd/legionWar/legionWarBeback.prefab");
	}

	public void ShowLegionWarTask()
	{
		ShowWnd(EWndId.LegionWarTask, "prefab/wnd/legionWar/legionWarTask.prefab");
	}

	public void ShowLegionWarBalance()
	{
		ShowWnd(EWndId.LegionWarBalance, "prefab/wnd/legionWar/legionWarBalance.prefab");
	}

	public void ShowLegionWarLog()
	{
		zbLegionWarLogWnd zbLegionWarLogWnd2 = ShowWnd(EWndId.LegionWarLog, "prefab/wnd/legionWar/legionWarLog.prefab") as zbLegionWarLogWnd;
		if ((Object)(object)zbLegionWarLogWnd2 != (Object)null)
		{
			zbLegionWarLogWnd2.RefreshCustom();
		}
	}

	public void ShowLegionWarEffect(LegionWarPlayerRankDetail[] list)
	{
		zbLegionWarEffectWnd zbLegionWarEffectWnd2 = ShowWnd(EWndId.LegionWarEffect, "prefab/wnd/legionWar/legionWarEffect.prefab") as zbLegionWarEffectWnd;
		if ((Object)(object)zbLegionWarEffectWnd2 != (Object)null)
		{
			zbLegionWarEffectWnd2.RefreshCustom(list);
		}
	}

	public zbBaseWnd ShowPeerage()
	{
		zbPeerageWnd obj = ShowWnd(EWndId.Peerage, "prefab/wnd/peerage/peerage.prefab") as zbPeerageWnd;
		_ = (Object)(object)obj != (Object)null;
		return obj;
	}

	public void ShowPeerageInfo(bool showPeerageup = false)
	{
		zbPeerageInfoWnd zbPeerageInfoWnd2 = ShowWnd(EWndId.PeerageInfo, "prefab/wnd/peerage/peerageInfo.prefab") as zbPeerageInfoWnd;
		if (((Object)(object)zbPeerageInfoWnd2 != (Object)null) & showPeerageup)
		{
			zbPeerageInfoWnd2.ShowPeerageUp();
			zbAudioMgr.Instance.PlayAudio("audio/ui/ui_award.ogg");
		}
		else
		{
			zbPeerageInfoWnd2.refreshCustom();
		}
	}

	public void ShowTask(int tab = 1)
	{
		zbPeerageWnd zbPeerageWnd2 = ShowPeerage() as zbPeerageWnd;
		if ((Object)(object)zbPeerageWnd2 != (Object)null)
		{
			zbPeerageWnd2.SelectTab(tab);
		}
	}

	public void ShowEndLessHorde()
	{
		ShowWnd(EWndId.EndlessHorde, "prefab/wnd/endlessHorde/endlessHorde.prefab");
	}

	public void ShowEndlessHordeFightForward(int selectNum = 1)
	{
		zbEndlessHordeFightForwardWnd zbEndlessHordeFightForwardWnd2 = ShowWnd(EWndId.EndlessHordeFightForward, "prefab/wnd/endlessHorde/endlessHordeFightForward.prefab") as zbEndlessHordeFightForwardWnd;
		if ((Object)(object)zbEndlessHordeFightForwardWnd2 != (Object)null)
		{
			zbEndlessHordeFightForwardWnd2.RefreshCustom(selectNum);
		}
	}

	public void ShowEndlessHordeFightForwardRestoreData(zbCombatTeam team, int _selectNum, int _selectMax, int _curRefreshNum, List<int> _selectMod)
	{
		zbEndlessHordeFightForwardWnd zbEndlessHordeFightForwardWnd2 = ShowWnd(EWndId.EndlessHordeFightForward, "prefab/wnd/endlessHorde/endlessHordeFightForward.prefab") as zbEndlessHordeFightForwardWnd;
		if ((Object)(object)zbEndlessHordeFightForwardWnd2 != (Object)null)
		{
			zbEndlessHordeFightForwardWnd2.RefreshRestoreData(team, _selectNum, _selectMax, _curRefreshNum, _selectMod);
		}
	}

	public void ShowPassportBuyLevel()
	{
		ShowWnd(EWndId.PassPortBuyLevel, "prefab/wnd/peerage/passportBuyLevel.prefab");
	}

	public void ShowSupportContact()
	{
		ShowWnd(EWndId.Support, "prefab/wnd/supportContact.prefab");
	}

	public void ShowGuideStatic(List<int> pageList)
	{
		ShowWnd(EWndId.GuideStatic, "prefab/wnd/guideStatic.prefab");
	}

	public void ShowGuideWeak(int _talkId, Action _onFinished = null, float labelTop = 740f, VisualElement _watchNode = null)
	{
		zbGuideWeakWnd zbGuideWeakWnd2 = ShowWnd(EWndId.GuideWeak, "prefab/wnd/comm/guideWeak.prefab") as zbGuideWeakWnd;
		if ((Object)(object)zbGuideWeakWnd2 != (Object)null)
		{
			zbGuideWeakWnd2.RefreshCustom(_talkId, _onFinished, labelTop, _watchNode);
		}
	}

	public void ShowGuidePrinter(int _talkId)
	{
		zbGuidePrinterWnd zbGuidePrinterWnd2 = ShowWnd(EWndId.GuidePrinter, "prefab/wnd/comm/guidePrinter.prefab") as zbGuidePrinterWnd;
		if ((Object)(object)zbGuidePrinterWnd2 != (Object)null)
		{
			zbGuidePrinterWnd2.RefreshPrinter(_talkId);
		}
	}

	public void ShowGuideFinger(VisualElement node, VisualElement targetNode = null)
	{
		zbGuideFinger zbGuideFinger2 = ShowWnd(EWndId.GuideFinger, "prefab/wnd/comm/guideFinger.prefab") as zbGuideFinger;
		if ((Object)(object)zbGuideFinger2 != (Object)null)
		{
			zbGuideFinger2.RefreshCustom(node, targetNode);
		}
	}

	public void ShowGuideWnd(VisualElement node, Action _onFinished = null)
	{
		zbGuideWnd zbGuideWnd2 = ShowWnd(EWndId.Guide, "prefab/wnd/comm/guide.prefab") as zbGuideWnd;
		if ((Object)(object)zbGuideWnd2 != (Object)null)
		{
			zbGuideWnd2.RefreshCustom(node, _onFinished);
		}
	}

	public void ShowBossUp()
	{
		zbBossUpWnd zbBossUpWnd2 = ShowWnd(EWndId.BossUp, "prefab/wnd/comm/bossUp.prefab") as zbBossUpWnd;
		if ((Object)(object)zbBossUpWnd2 != (Object)null)
		{
			zbBossUpWnd2.RefreshCustom();
		}
	}

	public void ShowCombatTest()
	{
		zbCombatTestWnd zbCombatTestWnd2 = ShowWnd(EWndId.CombatTest, "prefab/wnd/test/combatTest.prefab") as zbCombatTestWnd;
		if ((Object)(object)zbCombatTestWnd2 != (Object)null)
		{
			zbCombatTestWnd2.RefreshCustom();
		}
	}

	public void ShowNoviceTask7()
	{
		ShowWnd(EWndId.NoviceTask7, "prefab/wnd/activity/noviceTask7.prefab");
	}

	public void ShowNoviceTask14()
	{
		ShowWnd(EWndId.NoviceTask14, "prefab/wnd/activity/noviceTask14.prefab");
	}

	public void ShowOperate()
	{
		ShowWnd(EWndId.Operate, "prefab/wnd/operate/operate.prefab");
	}

	public void ShowRechargeDaily(string uuid, zbOperateCsvRecord record)
	{
		zbRechargeDailyWnd zbRechargeDailyWnd2 = ShowWnd(EWndId.RechargeDaily, "prefab/wnd/operate/rechargeDaily.prefab") as zbRechargeDailyWnd;
		if ((Object)(object)zbRechargeDailyWnd2 != (Object)null)
		{
			zbRechargeDailyWnd2.RefreshCustom(uuid, record);
		}
	}

	public void ShowRechargeDailySum(string uuid, zbOperateCsvRecord record)
	{
		zbRechargeDailySumWnd zbRechargeDailySumWnd2 = ShowWnd(EWndId.RechargeDailySum, "prefab/wnd/operate/RechargeDailySum.prefab") as zbRechargeDailySumWnd;
		if ((Object)(object)zbRechargeDailySumWnd2 != (Object)null)
		{
			zbRechargeDailySumWnd2.RefreshCustom(uuid, record);
		}
	}

	public void ShowRechargeSum(string uuid, zbOperateCsvRecord record)
	{
		zbRechargeSumWnd zbRechargeSumWnd2 = ShowWnd(EWndId.RechargeSum, "prefab/wnd/operate/RechargeSum.prefab") as zbRechargeSumWnd;
		if ((Object)(object)zbRechargeSumWnd2 != (Object)null)
		{
			zbRechargeSumWnd2.RefreshCustom(uuid, record);
		}
	}

	public void ShowRechargeGift(string uuid)
	{
		zbRechargeGiftWnd zbRechargeGiftWnd2 = ShowWnd(EWndId.RechargeGift, "prefab/wnd/operate/rechargeGift.prefab") as zbRechargeGiftWnd;
		if ((Object)(object)zbRechargeGiftWnd2 != (Object)null)
		{
			zbRechargeGiftWnd2.RefreshCustom(uuid);
		}
	}

	public void ShowConsumeDailySum(string uuid, zbOperateCsvRecord record)
	{
		zbConsumeDailySumWnd zbConsumeDailySumWnd2 = ShowWnd(EWndId.ConsumeDailySum, "prefab/wnd/operate/consumeDailySum.prefab") as zbConsumeDailySumWnd;
		if ((Object)(object)zbConsumeDailySumWnd2 != (Object)null)
		{
			zbConsumeDailySumWnd2.RefreshCustom(uuid, record);
		}
	}

	public void ShowConsumeSum(string uuid, zbOperateCsvRecord record)
	{
		zbConsumeSumWnd zbConsumeSumWnd2 = ShowWnd(EWndId.ConsumeSum, "prefab/wnd/operate/consumeSum.prefab") as zbConsumeSumWnd;
		if ((Object)(object)zbConsumeSumWnd2 != (Object)null)
		{
			zbConsumeSumWnd2.RefreshCustom(uuid, record);
		}
	}

	public void ShowBarDrawNormalSum(string uuid, zbOperateCsvRecord record)
	{
		zbBarDrawWnd zbBarDrawWnd2 = ShowWnd(EWndId.BarDrawNormal, "prefab/wnd/operate/barDrawNormal.prefab") as zbBarDrawWnd;
		if ((Object)(object)zbBarDrawWnd2 != (Object)null)
		{
			zbBarDrawWnd2.RefreshCustom(uuid, record);
		}
	}

	public void ShowJizi(JiZiDetail data)
	{
		zbJiziWnd zbJiziWnd2 = ShowWnd(EWndId.Jizi, "prefab/wnd/operate/jizi.prefab") as zbJiziWnd;
		if ((Object)(object)zbJiziWnd2 != (Object)null)
		{
			zbJiziWnd2.RefreshCustom(data);
		}
	}

	public void ShowJiziProbability(Dictionary<string, float>[] data)
	{
		zbJiziProbabilityWnd zbJiziProbabilityWnd2 = ShowWnd(EWndId.JiziProbability, "prefab/wnd/operate/jiziProbability.prefab") as zbJiziProbabilityWnd;
		if ((Object)(object)zbJiziProbabilityWnd2 != (Object)null)
		{
			zbJiziProbabilityWnd2.RefreshCustom(data);
		}
	}

	public void ShowLimitTimeRoulette(LimitTimeRouletteDetail data)
	{
		zbLimitTimeRouletteWnd zbLimitTimeRouletteWnd2 = ShowWnd(EWndId.LimitTimeRoulette, "prefab/wnd/operate/limitTimeRoulette.prefab") as zbLimitTimeRouletteWnd;
		if ((Object)(object)zbLimitTimeRouletteWnd2 != (Object)null)
		{
			zbLimitTimeRouletteWnd2.RefreshCustom(data, isFirst: true);
		}
	}

	public void ShowLimitTimeVendor(LimitTimeRouletteDetail data)
	{
		zbLimitTimeVendorWnd zbLimitTimeVendorWnd2 = ShowWnd(EWndId.LimitTimeVendor, "prefab/wnd/operate/limitTimeVendor.prefab") as zbLimitTimeVendorWnd;
		if ((Object)(object)zbLimitTimeVendorWnd2 != (Object)null)
		{
			zbLimitTimeVendorWnd2.RefreshCustom(data, isFirst: true);
		}
	}

	public void ShowLimitTimeTreasure(LimitTimeRouletteDetail limitTimeTreasure)
	{
		zbLimitTimeTreasureWnd zbLimitTimeTreasureWnd2 = ShowWnd(EWndId.LimitTimeTreasure, "prefab/wnd/operate/limitTimeTreasure.prefab") as zbLimitTimeTreasureWnd;
		if ((Object)(object)zbLimitTimeTreasureWnd2 != (Object)null)
		{
			zbLimitTimeTreasureWnd2.RefreshCustom(limitTimeTreasure, isFirst: true);
		}
	}

	public void ShowLimitTimeRoulettProbability(LimitTimeRouletteRandDetail[] limitTimeRouletteDetails, EOperateType type = EOperateType.LimitTimeRoulette)
	{
		zbLimitTimeRouletteProbabilityWnd zbLimitTimeRouletteProbabilityWnd2 = ShowWnd(EWndId.LimitTimeRouletteProbability, "prefab/wnd/operate/limitTimeRouletteProbability.prefab") as zbLimitTimeRouletteProbabilityWnd;
		if ((Object)(object)zbLimitTimeRouletteProbabilityWnd2 != (Object)null)
		{
			zbLimitTimeRouletteProbabilityWnd2.RefreshCustom(limitTimeRouletteDetails, type);
		}
	}

	public void ShowResourceExchange(ResourceExchangeDetail data)
	{
		zbResourceExchangeWnd zbResourceExchangeWnd2 = ShowWnd(EWndId.ResourceExchange, "prefab/wnd/operate/resourceExchange.prefab") as zbResourceExchangeWnd;
		if ((Object)(object)zbResourceExchangeWnd2 != (Object)null)
		{
			zbResourceExchangeWnd2.RefreshCustom(data);
		}
	}

	public void ShowPropExchange(PropExchangeDetail data)
	{
		zbPropExchangeWnd zbPropExchangeWnd2 = ShowWnd(EWndId.PropExchange, "prefab/wnd/operate/propExchange.prefab") as zbPropExchangeWnd;
		if ((Object)(object)zbPropExchangeWnd2 != (Object)null)
		{
			zbPropExchangeWnd2.RefreshCustom(data);
		}
	}

	public void ShowHolidayTask()
	{
		ShowWnd(EWndId.HolidayTask, "prefab/wnd/operate/holidayTask.prefab");
	}

	public void ShowPrayLantern(PrayLanternDetail data)
	{
		zbPrayLanternWnd zbPrayLanternWnd2 = ShowWnd(EWndId.PrayLantern, "prefab/wnd/operate/prayLantern.prefab") as zbPrayLanternWnd;
		if ((Object)(object)zbPrayLanternWnd2 != (Object)null)
		{
			zbPrayLanternWnd2.RefreshCustom(data);
		}
	}

	public void ShowPrayLanternUltimate(PrayLanternDetail data)
	{
		zbPrayLanternUltimateWnd zbPrayLanternUltimateWnd2 = ShowWnd(EWndId.PrayLanternUltimate, "prefab/wnd/operate/prayLanternUltimate.prefab") as zbPrayLanternUltimateWnd;
		if ((Object)(object)zbPrayLanternUltimateWnd2 != (Object)null)
		{
			zbPrayLanternUltimateWnd2.RefreshCustom(data);
		}
	}

	public void ShowLimitTimeShop(LimitTimeShopDetail data)
	{
		zbLimitTimeShopWnd zbLimitTimeShopWnd2 = ShowWnd(EWndId.LimitTimeShop, "prefab/wnd/operate/limitTimeShop.prefab") as zbLimitTimeShopWnd;
		if ((Object)(object)zbLimitTimeShopWnd2 != (Object)null)
		{
			zbLimitTimeShopWnd2.RefreshCustom(data);
		}
	}

	public void ShowThemeTask(ThemeTaskDetail data)
	{
		zbThemeTaskWnd zbThemeTaskWnd2 = ShowWnd(EWndId.ThemeTask, "prefab/wnd/operate/themeTask.prefab") as zbThemeTaskWnd;
		if ((Object)(object)zbThemeTaskWnd2 != (Object)null)
		{
			zbThemeTaskWnd2.RefreshCustom(data);
		}
	}

	public void ShowCipherBox(CipherBoxDetail cipherBoxDetail)
	{
		zbCipherBoxWnd zbCipherBoxWnd2 = ShowWnd(EWndId.CipherBox, "prefab/wnd/operate/cipherBox.prefab") as zbCipherBoxWnd;
		if ((Object)(object)zbCipherBoxWnd2 != (Object)null)
		{
			zbCipherBoxWnd2.RefreshCustom(cipherBoxDetail);
		}
	}

	public void ShowRestaurant(RestaurantDetail restaurantDetail)
	{
		if (restaurantDetail != null && restaurantDetail.ActivateAward == EBoolState.False)
		{
			ShowRestaurantActive();
			return;
		}
		zbRestaurantWnd zbRestaurantWnd2 = ShowWnd(EWndId.Restaurant, "prefab/wnd/operate/restaurant.prefab") as zbRestaurantWnd;
		if ((Object)(object)zbRestaurantWnd2 != (Object)null)
		{
			zbRestaurantWnd2.RefreshCustom(restaurantDetail, refreshFinger: true);
		}
	}

	public void ShowRestaurantActive()
	{
		ShowWnd(EWndId.RestaurantActive, "prefab/wnd/operate/restaurantActive.prefab");
	}

	public void ShowEndlessMine()
	{
		ShowWnd(EWndId.EndlessMine, "prefab/wnd/operate/endlessMine/endlessMine.prefab");
	}

	public void ShowEndlessMineActive()
	{
		ShowWnd(EWndId.EndlessMineActive, "prefab/wnd/operate/endlessMine/endlessMineActive.prefab");
	}

	public void ShowEndlessMineTarget()
	{
		ShowWnd(EWndId.EndlessMineTarget, "prefab/wnd/operate/endlessMine/endlessMineTarget.prefab");
	}

	public void ShowSnowVeil(SnowVeilDetail snowVeilDetail)
	{
		if (snowVeilDetail != null && snowVeilDetail.ActivateAward == EBoolState.False)
		{
			ShowSnowVeilActive();
			return;
		}
		zbSnowVeilWnd zbSnowVeilWnd2 = ShowWnd(EWndId.SnowVeil, "prefab/wnd/operate/snowVeil/snowVeil.prefab") as zbSnowVeilWnd;
		if ((Object)(object)zbSnowVeilWnd2 != (Object)null)
		{
			zbSnowVeilWnd2.RefreshCustom(snowVeilDetail);
		}
	}

	public void ShowSnowVeilActive()
	{
		ShowWnd(EWndId.SnowVeilActive, "prefab/wnd/operate/snowVeil/snowVeilActive.prefab");
	}

	public void ShowSnowVeilTurntable(SnowVeilDetail snowVeilDetail)
	{
		zbSnowVeilTurntableWnd zbSnowVeilTurntableWnd2 = ShowWnd(EWndId.SnowVeilTurntable, "prefab/wnd/operate/snowVeil/snowVeilTurntable.prefab") as zbSnowVeilTurntableWnd;
		if ((Object)(object)zbSnowVeilTurntableWnd2 != (Object)null)
		{
			zbSnowVeilTurntableWnd2.RefreshCustom(snowVeilDetail);
		}
	}

	public void ShowSnowVeilRolling(SnowVeilDetail snowVeilDetail)
	{
		zbSnowVeilRollingWnd zbSnowVeilRollingWnd2 = ShowWnd(EWndId.SnowVeilRolling, "prefab/wnd/operate/snowVeil/snowVeilRolling.prefab") as zbSnowVeilRollingWnd;
		if ((Object)(object)zbSnowVeilRollingWnd2 != (Object)null)
		{
			zbSnowVeilRollingWnd2.RefreshCustom(snowVeilDetail);
		}
	}

	public void ShowCipherBoxPreview(CipherBoxDetail cipherBoxDetail)
	{
		zbCipherBoxPreViewWnd zbCipherBoxPreViewWnd2 = ShowWnd(EWndId.CipherBoxPreview, "prefab/wnd/operate/cipherBoxPreview.prefab") as zbCipherBoxPreViewWnd;
		if ((Object)(object)zbCipherBoxPreViewWnd2 != (Object)null)
		{
			zbCipherBoxPreViewWnd2.RefreshCustom(cipherBoxDetail);
		}
	}

	public void ShowZhanLingKill(ZhanLingKillDetail zhanLingKillDetail)
	{
		zbZhanLingKillWnd zbZhanLingKillWnd2 = ShowWnd(EWndId.ZhanLingKill, "prefab/wnd/operate/zhanLingKill.prefab") as zbZhanLingKillWnd;
		if ((Object)(object)zbZhanLingKillWnd2 != (Object)null)
		{
			zbZhanLingKillWnd2.RefreshCustom(zhanLingKillDetail);
		}
	}

	public void ShowTactic()
	{
		_ = (Object)(object)(ShowWnd(EWndId.Tactic, "prefab/wnd/tactic/tactic.prefab") as zbTacticWnd) != (Object)null;
	}

	public void ShowTacticUpgrade()
	{
		_ = (Object)(object)(ShowWnd(EWndId.TacticUpgrade, "prefab/wnd/tactic/tacticUpgrade.prefab") as zbTacticUpgradeWnd) != (Object)null;
	}

	public void ShowTacticInfo()
	{
		_ = (Object)(object)(ShowWnd(EWndId.TacticInfo, "prefab/wnd/tactic/tacticInfo.prefab") as zbTacticInfoWnd) != (Object)null;
	}

	public void ShowTacticCardDetail()
	{
		_ = (Object)(object)(ShowWnd(EWndId.TacticCardDetail, "prefab/wnd/tactic/tacticCardDetail.prefab") as zbTacticCardDetailWnd) != (Object)null;
	}

	public void ShowDailySign()
	{
		ShowWnd(EWndId.DailySign, "prefab/wnd/operate/dailySign.prefab");
	}

	public void ShowGrowthGift(int GoodsId = 0)
	{
		zbGrowthGiftWnd zbGrowthGiftWnd2 = ShowWnd(EWndId.GrowthGift, "prefab/wnd/activity/growthgift.prefab") as zbGrowthGiftWnd;
		if ((Object)(object)zbGrowthGiftWnd2 != (Object)null)
		{
			zbGrowthGiftWnd2.RefreshCustom(GoodsId);
		}
	}

	public void ShowTiliBank()
	{
		ShowWnd(EWndId.TiliBank, "prefab/wnd/activity/tilibank.prefab");
	}

	public void ShowFirstRecharge(Action<bool> callback = null)
	{
		zbFirstRechargeWnd zbFirstRechargeWnd2 = ShowWnd(EWndId.NovicePack, "prefab/wnd/activity/firstRecharge.prefab") as zbFirstRechargeWnd;
		if ((Object)(object)zbFirstRechargeWnd2 != (Object)null)
		{
			zbFirstRechargeWnd2.RefreshCustom(callback);
		}
	}

	public void ShowFusionEnter()
	{
		ShowWnd(EWndId.FusionEnter, "prefab/wnd/operate/fusionenter.prefab");
	}

	public void ShowFusion()
	{
		ShowWnd(EWndId.Fusion, "prefab/wnd/operate/fusion.prefab");
	}

	public void ShowFusionAward()
	{
		ShowWnd(EWndId.FusionAward, "prefab/wnd/operate/fusionaward.prefab");
	}

	public void ShowFusionRank(RankDetail[] rankList)
	{
		zbFusionRankWnd zbFusionRankWnd2 = ShowWnd(EWndId.FusionRank, "prefab/wnd/operate/fusionRank.prefab") as zbFusionRankWnd;
		if (Object.op_Implicit((Object)(object)zbFusionRankWnd2))
		{
			zbFusionRankWnd2.RefreshCustom(rankList);
		}
	}

	public void ShowTimeCatcher(LimitTimeCatcherDetail limitTimeCatcherDetail)
	{
		zbTimeCatcher zbTimeCatcher2 = ShowWnd(EWndId.TimeCatcher, "prefab/wnd/operate/timecatcher.prefab") as zbTimeCatcher;
		if (Object.op_Implicit((Object)(object)zbTimeCatcher2))
		{
			zbTimeCatcher2.RefreshCustom(limitTimeCatcherDetail);
		}
	}

	public void ShowLoginSum(LoginSumDetail loginSumDetail)
	{
		zbLoginSumWnd zbLoginSumWnd2 = ShowWnd(EWndId.LoginSum, "prefab/wnd/operate/loginSum.prefab") as zbLoginSumWnd;
		if (loginSumDetail != null)
		{
			zbLoginSumWnd2.RefreshCustom(loginSumDetail);
		}
	}

	public void ShowDailyActive(string uuid, zbOperateCsvRecord record)
	{
		zbDailyActiveWnd zbDailyActiveWnd2 = ShowWnd(EWndId.DailyActive, "prefab/wnd/operate/dailyActive.prefab") as zbDailyActiveWnd;
		if ((Object)(object)zbDailyActiveWnd2 != (Object)null)
		{
			zbDailyActiveWnd2.RefreshCustom(uuid, record);
		}
	}

	public void ShowBarrageCmd(string uuid, zbOperateCsvRecord record)
	{
		zbBarrageCmdWnd zbBarrageCmdWnd2 = ShowWnd(EWndId.BarrageCmd, "prefab/wnd/operate/barrageCmd.prefab") as zbBarrageCmdWnd;
		if ((Object)(object)zbBarrageCmdWnd2 != (Object)null)
		{
			zbBarrageCmdWnd2.RefreshCustom(uuid, record);
		}
	}

	public void ShowInfiniteGift(InfiniteGiftDetail infiniteGiftDetail, Action<bool> callback = null)
	{
		zbInfiniteGiftWnd zbInfiniteGiftWnd2 = ShowWnd(EWndId.InfiniteGift, "prefab/wnd/operate/infinitegift.prefab") as zbInfiniteGiftWnd;
		if (Object.op_Implicit((Object)(object)zbInfiniteGiftWnd2))
		{
			zbInfiniteGiftWnd2.RefreshCustom(infiniteGiftDetail, callback);
		}
	}

	public void ShowScratchCard(ScratchCardDetail scratchCardDetail)
	{
		zbScratchCardWnd zbScratchCardWnd2 = ShowWnd(EWndId.ScratchCardWnd, "prefab/wnd/operate/scratchCard.prefab") as zbScratchCardWnd;
		if (Object.op_Implicit((Object)(object)zbScratchCardWnd2))
		{
			zbScratchCardWnd2.RefreshCustom(scratchCardDetail);
		}
	}

	public void ShowVoucherCostPop(float price, Action call = null)
	{
		zbVoucherCostPop zbVoucherCostPop2 = ShowWnd(EWndId.VouchercostPop, "prefab/wnd/tip/vouchercostpop.prefab") as zbVoucherCostPop;
		if (Object.op_Implicit((Object)(object)zbVoucherCostPop2))
		{
			zbVoucherCostPop2.RefreshCustom(price, call);
		}
	}

	public zbBaseWnd ShowWorld(WorldMapGridDetail gotoGridDetail = null, Action<bool> callback = null)
	{
		zbWorldWnd zbWorldWnd2 = ShowWnd(EWndId.World, "prefab/world/world.prefab") as zbWorldWnd;
		if (Object.op_Implicit((Object)(object)zbWorldWnd2))
		{
			zbWorldWnd2.m_isFirstIn = true;
			zbWorldWnd2.RefreshCustom(gotoGridDetail, callback);
		}
		return zbWorldWnd2;
	}

	public void ShowWorldLevel()
	{
		ShowWnd(EWndId.WorldLevel, "prefab/world/worldLevel.prefab");
	}

	public void ShowWorldDungeon(int gridId, int buildId)
	{
		zbWorldDungeonWnd zbWorldDungeonWnd2 = ShowWnd(EWndId.WorldDungeon, "prefab/world/worldDungeon.prefab") as zbWorldDungeonWnd;
		if ((Object)(object)zbWorldDungeonWnd2 != (Object)null)
		{
			zbWorldDungeonWnd2.RefreshCustom(gridId, buildId);
		}
	}

	public void ShowWorldRecon(WorldReconDetail reconDetail)
	{
		zbWorldReconWnd zbWorldReconWnd2 = ShowWnd(EWndId.WorldRecon, "prefab/world/worldRecon.prefab") as zbWorldReconWnd;
		if ((Object)(object)zbWorldReconWnd2 != (Object)null)
		{
			zbWorldReconWnd2.RefreshCustom(reconDetail);
		}
	}

	public void ShowWorldReconBuy()
	{
		ShowWnd(EWndId.WorldReconBuy, "prefab/world/worldReconBuy.prefab");
	}

	public void ShowWorldExploreCancel(WorldReconDetail reconDetail)
	{
		zbWorldExploreCancelWnd zbWorldExploreCancelWnd2 = ShowWnd(EWndId.WorldExploreCancel, "prefab/world/worldExploreCancel.prefab") as zbWorldExploreCancelWnd;
		if ((Object)(object)zbWorldExploreCancelWnd2 != (Object)null)
		{
			zbWorldExploreCancelWnd2.RefreshCustom(reconDetail);
		}
	}

	public void ShowWorldReconExplore(WorldMapGridDetail gridDetail)
	{
		zbWorldReconExploreWnd zbWorldReconExploreWnd2 = ShowWnd(EWndId.WorldReconExplore, "prefab/world/worldReconExplore.prefab") as zbWorldReconExploreWnd;
		if ((Object)(object)zbWorldReconExploreWnd2 != (Object)null)
		{
			zbWorldReconExploreWnd2.RefreshCustom(gridDetail);
		}
	}

	public void ShowWorldReconExplored(AwardDetail[] awardList)
	{
		zbWorldReconExploredWnd zbWorldReconExploredWnd2 = ShowWnd(EWndId.WorldReconExplored, "prefab/world/worldReconExplored.prefab") as zbWorldReconExploredWnd;
		if ((Object)(object)zbWorldReconExploredWnd2 != (Object)null)
		{
			zbWorldReconExploredWnd2.RefreshCustom(awardList);
		}
	}

	public void ShowWorldResourceCreate(WorldMapGridDetail gridDetail)
	{
		zbWorldResourceCreateWnd zbWorldResourceCreateWnd2 = ShowWnd(EWndId.WorldResourceCreate, "prefab/world/worldResourceCreate.prefab") as zbWorldResourceCreateWnd;
		if ((Object)(object)zbWorldResourceCreateWnd2 != (Object)null)
		{
			zbWorldResourceCreateWnd2.RefreshCustom(gridDetail);
		}
	}

	public void ShowWorldResourceCollect()
	{
		ShowWnd(EWndId.WorldResourceCollect, "prefab/world/worldResourceCollect.prefab");
	}

	public void ShowWorldResourceDetail(WorldMapGridDetail gridDetail)
	{
		zbWorldResourceDetailWnd zbWorldResourceDetailWnd2 = ShowWnd(EWndId.WorldResourceDetail, "prefab/world/worldResourceDetail.prefab") as zbWorldResourceDetailWnd;
		if ((Object)(object)zbWorldResourceDetailWnd2 != (Object)null)
		{
			zbWorldResourceDetailWnd2.RefreshCustom(gridDetail);
		}
	}

	public void ShowWorldRadar()
	{
		ShowWnd(EWndId.WorldRadar, "prefab/world/worldRadar.prefab");
	}

	public void ShowWorldEventBox(WorldEventDetail eventDetail)
	{
		zbWorldEventBoxWnd zbWorldEventBoxWnd2 = ShowWnd(EWndId.WorldEventBox, "prefab/world/worldEventBox.prefab") as zbWorldEventBoxWnd;
		if ((Object)(object)zbWorldEventBoxWnd2 != (Object)null)
		{
			zbWorldEventBoxWnd2.RefreshCustom(eventDetail);
		}
	}

	public void ShowWorldEventFight(WorldEventDetail eventDetail)
	{
		zbWorldEventFightWnd zbWorldEventFightWnd2 = ShowWnd(EWndId.WorldEventFight, "prefab/world/worldEventFight.prefab") as zbWorldEventFightWnd;
		if ((Object)(object)zbWorldEventFightWnd2 != (Object)null)
		{
			zbWorldEventFightWnd2.RefreshCustom(eventDetail);
		}
	}

	public void ShowWorldEventShop(WorldEventDetail eventDetail)
	{
		zbWorldEventShopWnd zbWorldEventShopWnd2 = ShowWnd(EWndId.WorldEventShop, "prefab/world/worldEventShop.prefab") as zbWorldEventShopWnd;
		if ((Object)(object)zbWorldEventShopWnd2 != (Object)null)
		{
			zbWorldEventShopWnd2.RefreshCustom(eventDetail);
		}
	}

	public void ShowWorldSurvivor(WorldEventDetail eventDetail)
	{
		zbWorldEventSurvivorWnd zbWorldEventSurvivorWnd2 = ShowWnd(EWndId.WorldEventSurvivor, "prefab/world/worldEventSurvivor.prefab") as zbWorldEventSurvivorWnd;
		if ((Object)(object)zbWorldEventSurvivorWnd2 != (Object)null)
		{
			zbWorldEventSurvivorWnd2.RefreshCustom(eventDetail);
		}
	}

	public void ShowWorldBoss()
	{
		ShowWnd(EWndId.WorldBoss, "prefab/wnd/worldBoss/worldBoss.prefab");
	}

	public void ShowWorldBossRank(RankDetail[] rankDetails)
	{
		zbWorldBossRankWnd zbWorldBossRankWnd2 = ShowWnd(EWndId.WorldBossRank, "prefab/wnd/worldBoss/worldBossRank.prefab") as zbWorldBossRankWnd;
		if ((Object)(object)zbWorldBossRankWnd2 != (Object)null)
		{
			zbWorldBossRankWnd2.RefreshCustom(rankDetails);
		}
	}

	public void ShowWorldBossLog(WorldBossHurtRecordDetail[] recordList)
	{
		zbWorldBossLogWnd zbWorldBossLogWnd2 = ShowWnd(EWndId.WorldBossLog, "prefab/wnd/worldBoss/worldBossLog.prefab") as zbWorldBossLogWnd;
		if ((Object)(object)zbWorldBossLogWnd2 != (Object)null)
		{
			zbWorldBossLogWnd2.RefreshCustom(recordList);
		}
	}

	public void ShowRule(string[] langKeys)
	{
		zbRuleWnd zbRuleWnd2 = ShowWnd(EWndId.WorldBossRule, "prefab/wnd/tip/ruleTip.prefab") as zbRuleWnd;
		if ((Object)(object)zbRuleWnd2 != (Object)null)
		{
			zbRuleWnd2.RefreshCustom(langKeys);
		}
	}

	public void ShowBuyCount(EBattleFuncType _battleFuncType, int param = 0)
	{
		zbBuyCountWnd zbBuyCountWnd2 = ShowWnd(EWndId.BuyCount, "prefab/wnd/comm/buyCount.prefab") as zbBuyCountWnd;
		if ((Object)(object)zbBuyCountWnd2 != (Object)null)
		{
			zbBuyCountWnd2.RefreshCustom(_battleFuncType, param);
		}
	}

	public void ShowWorldBossReborn()
	{
		zbWorldBossRebornWnd zbWorldBossRebornWnd2 = ShowWnd(EWndId.WorldBossRebron, "prefab/wnd/worldBoss/worldBossReborn.prefab") as zbWorldBossRebornWnd;
		if (Object.op_Implicit((Object)(object)zbWorldBossRebornWnd2))
		{
			zbWorldBossRebornWnd2.RefreshCustom();
		}
	}

	public void ShowWorldLair()
	{
		zbWorldLairWnd zbWorldLairWnd2 = ShowWnd(EWndId.WorldLair, "prefab/wnd/worldLairFight/worldLair.prefab") as zbWorldLairWnd;
		if (Object.op_Implicit((Object)(object)zbWorldLairWnd2))
		{
			zbWorldLairWnd2.RefreshCustom(needRefresh: true);
		}
	}

	public void ShowWorldLairFight(WorldLairDetail worldLairInfo, bool isHelp = false, RoleBaseInfo helpRoleInfo = null)
	{
		zbWorldLairFightWnd zbWorldLairFightWnd2 = ShowWnd(EWndId.WorldLairFight, "prefab/wnd/worldLairFight/worldLairFight.prefab") as zbWorldLairFightWnd;
		if (Object.op_Implicit((Object)(object)zbWorldLairFightWnd2))
		{
			zbWorldLairFightWnd2.RefreshCustom(worldLairInfo, isHelp, helpRoleInfo);
		}
	}

	public void ShowWorldLairList(List<WorldLairHelpInfo> worldLairInfos)
	{
		zbWorldLairListWnd zbWorldLairListWnd2 = ShowWnd(EWndId.WorldLairList, "prefab/wnd/worldLairFight/worldLairList.prefab") as zbWorldLairListWnd;
		if (Object.op_Implicit((Object)(object)zbWorldLairListWnd2))
		{
			zbWorldLairListWnd2.RefreshCustom(worldLairInfos);
		}
	}

	public void ShowWorldLairPassAward(int lairLv)
	{
		zbWorldLairPassAwardWnd zbWorldLairPassAwardWnd2 = ShowWnd(EWndId.WorldLairPassAward, "prefab/wnd/worldLairFight/worldLairPassAward.prefab") as zbWorldLairPassAwardWnd;
		if (Object.op_Implicit((Object)(object)zbWorldLairPassAwardWnd2))
		{
			zbWorldLairPassAwardWnd2.RefreshCustom(lairLv);
		}
	}

	public void ShowWorldLairRecord(WorldLairFightRecordDetail[] recordList)
	{
		zbWorldLairRecordWnd zbWorldLairRecordWnd2 = ShowWnd(EWndId.WorldLairRecord, "prefab/wnd/worldLairFight/worldLairRecord.prefab") as zbWorldLairRecordWnd;
		if (Object.op_Implicit((Object)(object)zbWorldLairRecordWnd2))
		{
			zbWorldLairRecordWnd2.RefreshCustom(recordList);
		}
	}

	public void ShowDungeon(zbDungeonHandler dungeonBaseHandler)
	{
		zbDungeonWnd zbDungeonWnd2 = ShowWnd(EWndId.Dungeon, "prefab/dungeon/dungeon.prefab") as zbDungeonWnd;
		if (Object.op_Implicit((Object)(object)zbDungeonWnd2))
		{
			zbDungeonWnd2.RefreshCustom(dungeonBaseHandler);
		}
	}

	public void ShowDungeonBalance(zbDungeonHandler dungeonBaseHandler, bool isActiveRetreat = false)
	{
		zbDungeonBalanceWnd zbDungeonBalanceWnd2 = ShowWnd(EWndId.DungeonBalance, "prefab/dungeon/dungeonBalance.prefab") as zbDungeonBalanceWnd;
		if (Object.op_Implicit((Object)(object)zbDungeonBalanceWnd2))
		{
			zbDungeonBalanceWnd2.RefreshCustom(dungeonBaseHandler);
		}
	}

	public void ShowDungeonMap(zbDungeonHandler dungeonBaseHandler)
	{
		zbDungeonMapWnd zbDungeonMapWnd2 = ShowWnd(EWndId.DungeonMap, "prefab/dungeon/dungeonMap.prefab") as zbDungeonMapWnd;
		if (Object.op_Implicit((Object)(object)zbDungeonMapWnd2))
		{
			zbDungeonMapWnd2.RefreshCustom(dungeonBaseHandler);
		}
	}

	public void ShowDungeonFormation(zbDungeonHandler dungeonBaseHandler, EDungeonType type, int param1, EDifficultyLevel param2 = EDifficultyLevel.EmptyValue)
	{
		zbDungeonFormationWnd zbDungeonFormationWnd2 = ShowWnd(EWndId.DungeonFormation, "prefab/dungeon/dungeonformation.prefab") as zbDungeonFormationWnd;
		if (Object.op_Implicit((Object)(object)zbDungeonFormationWnd2))
		{
			zbDungeonFormationWnd2.RefreshCustom(dungeonBaseHandler, type, param1, param2);
		}
	}

	public void ShowDungeonEventPopUp(DungeonTriggerEventInfo info)
	{
		zbDungeonEventPopUp zbDungeonEventPopUp2 = ShowWnd(EWndId.DungeonEventPopUp, "prefab/dungeon/dungeoneventpopup.prefab") as zbDungeonEventPopUp;
		if (Object.op_Implicit((Object)(object)zbDungeonEventPopUp2))
		{
			zbDungeonEventPopUp2.RefreshCustom(info);
		}
	}

	public void ShowDungeonModPopUp(DungeonTriggerEventInfo info, DungeonBaseInfo dungeonBaseInfo)
	{
		zbDungeonModWnd zbDungeonModWnd2 = ShowWnd(EWndId.DungeonMod, "prefab/dungeon/dungeonmod.prefab") as zbDungeonModWnd;
		if (Object.op_Implicit((Object)(object)zbDungeonModWnd2))
		{
			zbDungeonModWnd2.RefreshCustom(info, dungeonBaseInfo);
		}
	}

	public void ShowDungeonExit(zbDungeonHandler dungeonBaseHandler)
	{
		zbDungeonExitWnd zbDungeonExitWnd2 = ShowWnd(EWndId.DungeonExit, "prefab/dungeon/dungeonExit.prefab") as zbDungeonExitWnd;
		if (Object.op_Implicit((Object)(object)zbDungeonExitWnd2))
		{
			zbDungeonExitWnd2.RefreshCustom(dungeonBaseHandler);
		}
	}

	public void ShowDungeonFightPop(DungeonTriggerEventInfo info)
	{
		zbDungeonFightPop zbDungeonFightPop2 = ShowWnd(EWndId.DungeonFightPop, "prefab/dungeon/dungeonfightpop.prefab") as zbDungeonFightPop;
		if (Object.op_Implicit((Object)(object)zbDungeonFightPop2))
		{
			zbDungeonFightPop2.RefreshCustom(info);
		}
	}

	public void ShowDungeonLog(AwardDetail[] awardList, DungeonLogInfo dungeonLogInfo)
	{
		zbDungeonLogWnd zbDungeonLogWnd2 = ShowWnd(EWndId.DungeonLog, "prefab/dungeon/dungeonLog.prefab") as zbDungeonLogWnd;
		if (Object.op_Implicit((Object)(object)zbDungeonLogWnd2))
		{
			zbDungeonLogWnd2.RefreshCustom(awardList, dungeonLogInfo);
		}
	}

	public void ShowDungeonResume(bool isWorldDungeon = false)
	{
		zbDungeonResumeWnd zbDungeonResumeWnd2 = ShowWnd(EWndId.DungeonResume, "prefab/dungeon/dungeonResume.prefab") as zbDungeonResumeWnd;
		if (Object.op_Implicit((Object)(object)zbDungeonResumeWnd2))
		{
			zbDungeonResumeWnd2.RefreshCustom(isWorldDungeon);
		}
	}

	public void ShowPlayerInfo(RoleTipsInfo _roleTipsInfo)
	{
		zbPlayerInfoWnd zbPlayerInfoWnd2 = ShowWnd(EWndId.PlayerInfo, "prefab/wnd/playerInfo.prefab") as zbPlayerInfoWnd;
		if ((Object)(object)zbPlayerInfoWnd2 != (Object)null)
		{
			zbPlayerInfoWnd2.RefreshCustom(_roleTipsInfo);
		}
	}

	public void ShowPlayerInfoMiniTip(RoleMiniTipsInfo _roleMiniTipsInfo)
	{
		zbPlayerInfoMiniTipWnd zbPlayerInfoMiniTipWnd2 = ShowWnd(EWndId.PlayerInfoMiniTip, "prefab/wnd/playerInfoMiniTip.prefab") as zbPlayerInfoMiniTipWnd;
		if ((Object)(object)zbPlayerInfoMiniTipWnd2 != (Object)null)
		{
			zbPlayerInfoMiniTipWnd2.RefreshCustom(_roleMiniTipsInfo);
		}
	}

	public void ShowRank(int tabIndex, Action<bool> callback = null)
	{
		zbRankWnd zbRankWnd2 = ShowWnd(EWndId.Rank, "prefab/wnd/rank/rank.prefab") as zbRankWnd;
		if ((Object)(object)zbRankWnd2 != (Object)null)
		{
			zbRankWnd2.RefreshCustom(tabIndex, callback);
		}
	}

	public void ShowMainQuestRank(RankDetail[] rankList)
	{
		zbMainQuestRankWnd zbMainQuestRankWnd2 = ShowWnd(EWndId.MainQuestRank, "prefab/wnd/rank/mainQuestRank.prefab") as zbMainQuestRankWnd;
		if ((Object)(object)zbMainQuestRankWnd2 != (Object)null)
		{
			zbMainQuestRankWnd2.RefreshCustom(rankList);
		}
	}

	public void ShowOpenServerRank(OpsRankInfo opsRankInfo)
	{
		zbOpenServerRankWnd zbOpenServerRankWnd2 = ShowWnd(EWndId.OpenServerRank, "prefab/wnd/rank/openServerRank.prefab") as zbOpenServerRankWnd;
		if ((Object)(object)zbOpenServerRankWnd2 != (Object)null)
		{
			zbOpenServerRankWnd2.RefreshCustom(opsRankInfo);
		}
	}

	public void ShowOpenServerRankRule()
	{
		ShowWnd(EWndId.OpenServerRankRule, "prefab/wnd/rank/openServerRankRule.prefab");
	}

	public void ShowRankFirstReach(RankFirstReachDetail[] rankFirstReachDetails)
	{
		zbRankFirstReachWnd zbRankFirstReachWnd2 = ShowWnd(EWndId.RankFirstReach, "prefab/wnd/rank/rankFirstReach.prefab") as zbRankFirstReachWnd;
		if ((Object)(object)zbRankFirstReachWnd2 != (Object)null)
		{
			zbRankFirstReachWnd2.RefreshCustom(rankFirstReachDetails);
		}
	}

	public void ShowPlayerSuggest(PlayerSuggestDetail[] playerSuggestDetail)
	{
		zbPlayerSuggestWnd zbPlayerSuggestWnd2 = ShowWnd(EWndId.PlayerSuggest, "prefab/wnd/comm/playerSuggest.prefab") as zbPlayerSuggestWnd;
		if ((Object)(object)zbPlayerSuggestWnd2 != (Object)null)
		{
			zbPlayerSuggestWnd2.RefreshCustom(playerSuggestDetail);
		}
	}

	public void ShowPlayerSuggestDetail(RoleTipsInfo _roleTipsInfo, List<ModSkillInfo> modSkillInfos)
	{
		zbPlayerSuggestDetailWnd zbPlayerSuggestDetailWnd2 = ShowWnd(EWndId.PlayerSuggestDetail, "prefab/wnd/comm/playerSuggestDetail.prefab") as zbPlayerSuggestDetailWnd;
		if ((Object)(object)zbPlayerSuggestDetailWnd2 != (Object)null)
		{
			zbPlayerSuggestDetailWnd2.RefreshCustom(_roleTipsInfo, modSkillInfos);
		}
	}

	public void ShowPlayerTipsDetailWnd(RoleTipsDetail roleTipsDetail)
	{
		zbPlayerTipsDetailWnd zbPlayerTipsDetailWnd2 = ShowWnd(EWndId.PlayerTipsDetail, "prefab/wnd/comm/playerTipsDetail.prefab") as zbPlayerTipsDetailWnd;
		if ((Object)(object)zbPlayerTipsDetailWnd2 != (Object)null)
		{
			zbPlayerTipsDetailWnd2.RefreshCustom(roleTipsDetail);
		}
	}

	public void ShowFuncUnLockAwardWnd(EFuncType unLockType)
	{
		zbTipFuncUnLockAwardWnd zbTipFuncUnLockAwardWnd2 = ShowWnd(EWndId.FuncUnLockAward, "prefab/wnd/tip/tipFuncUnlockAward.prefab") as zbTipFuncUnLockAwardWnd;
		if ((Object)(object)zbTipFuncUnLockAwardWnd2 != (Object)null)
		{
			zbTipFuncUnLockAwardWnd2.RefreshCustom(unLockType);
		}
	}

	public void ShowPremiumCardPreviewWnd()
	{
		BubbleInfo("todo....");
	}

	public void ShowWeekCardWnd()
	{
		ShowWnd(EWndId.WeekCard, "prefab/comm/weekCard.prefab");
	}

	public void PushScrollBubble(string message)
	{
		m_scrollBubble.PushScrollBubble(message);
	}

	public void ShowMessageBox(string title, string message, Action<bool> callback, string lbBtnOk = "1", string lbCancel = "0", bool isTouchVis = true, bool showRoomIcon = false, HomelandRoomDetail roomData = null, bool isCloseVis = true)
	{
		zbMessageBox zbMessageBox2 = ShowWnd(EWndId.MessageBox, "prefab/wnd/tip/messageBox.prefab") as zbMessageBox;
		if (Object.op_Implicit((Object)(object)zbMessageBox2))
		{
			zbMessageBox2.RefreshCustom(title, message, callback, lbBtnOk, lbCancel, isTouchVis, showRoomIcon, roomData, isCloseVis);
		}
	}

	public void ShowMessageConfirm(string message, Action<bool> callback, bool showClostBtn = true)
	{
		zbMessageConfirmWnd zbMessageConfirmWnd2 = ShowWnd(EWndId.MessageConfirm, "prefab/wnd/tip/messageConfirm.prefab") as zbMessageConfirmWnd;
		if (Object.op_Implicit((Object)(object)zbMessageConfirmWnd2))
		{
			zbMessageConfirmWnd2.RefreshCustom(message, callback, showClostBtn);
		}
	}

	public void ShowVerificationCode(string title, string message, Action<bool> callback)
	{
		zbVerificationCodeWnd zbVerificationCodeWnd2 = ShowWnd(EWndId.VerificationCode, "prefab/wnd/tip/verificationCode.prefab") as zbVerificationCodeWnd;
		if (Object.op_Implicit((Object)(object)zbVerificationCodeWnd2))
		{
			zbVerificationCodeWnd2.RefreshCustom(title, message, callback);
		}
	}

	public void ShowSpeedBox()
	{
		zbSpeedBox zbSpeedBox2 = ShowWnd(EWndId.SpeedBox, "prefab/wnd/tip/speedBox.prefab") as zbSpeedBox;
		if (Object.op_Implicit((Object)(object)zbSpeedBox2))
		{
			zbSpeedBox2.RefreshCustom();
		}
	}

	public void ShowAppReview()
	{
		ShowWnd(EWndId.AppReview, "prefab/wnd/tip/appreview.prefab");
	}

	public zbBaseWnd ShowMecha(zbMechaCsvRecord record)
	{
		zbMechaWnd zbMechaWnd2 = ShowWnd(EWndId.Mecha, "prefab/wnd/mecha/mecha.prefab") as zbMechaWnd;
		if (Object.op_Implicit((Object)(object)zbMechaWnd2))
		{
			zbMechaWnd2.RefreshCustom(record);
		}
		return zbMechaWnd2;
	}

	public void ShowMechaDetail(zbMechaCsvRecord record)
	{
		zbMechaDetailWnd zbMechaDetailWnd2 = ShowWnd(EWndId.MechaDetail, "prefab/wnd/mecha/mechaDetail.prefab") as zbMechaDetailWnd;
		if (Object.op_Implicit((Object)(object)zbMechaDetailWnd2))
		{
			zbMechaDetailWnd2.RefreshCustom(record);
		}
	}

	public void ShowMechaSkillUpgrade(zbMechaCsvRecord mechaRecord, MechaSkillDetail skillDetail)
	{
		zbMechaSkillUpgradeWnd zbMechaSkillUpgradeWnd2 = ShowWnd(EWndId.MechaSkillUpgrade, "prefab/wnd/mecha/mechaSkillUpgrade.prefab") as zbMechaSkillUpgradeWnd;
		if (Object.op_Implicit((Object)(object)zbMechaSkillUpgradeWnd2))
		{
			zbMechaSkillUpgradeWnd2.RefreshCustom(mechaRecord, skillDetail);
		}
	}

	public void ShowMechaConvert(MechaDetail[] mechaDetails)
	{
		zbMechaConvertWnd zbMechaConvertWnd2 = ShowWnd(EWndId.MechaConvert, "prefab/wnd/mecha/mechaConvert.prefab") as zbMechaConvertWnd;
		if ((Object)(object)zbMechaConvertWnd2 != (Object)null)
		{
			zbMechaConvertWnd2.RefreshCustom(mechaDetails);
		}
	}

	public void ShowMechaGet(int mechaId)
	{
		zbMechaGetWnd zbMechaGetWnd2 = ShowWnd(EWndId.MechaGet, "prefab/wnd/mecha/mechaGet.prefab") as zbMechaGetWnd;
		if ((Object)(object)zbMechaGetWnd2 != (Object)null)
		{
			zbMechaGetWnd2.RefreshCustom(mechaId);
		}
	}

	public void ShowMechaStarSuccess(int mechaId)
	{
		zbMechaStarSuccessWnd zbMechaStarSuccessWnd2 = ShowWnd(EWndId.MechaStarSuccess, "prefab/wnd/mecha/mechaStarSuccess.prefab") as zbMechaStarSuccessWnd;
		if ((Object)(object)zbMechaStarSuccessWnd2 != (Object)null)
		{
			zbMechaStarSuccessWnd2.RefreshCustom(mechaId);
		}
	}

	public void ShowMechaReset(int mechaId, AwardDetail[] awardList)
	{
		zbMechaResetWnd zbMechaResetWnd2 = ShowWnd(EWndId.MechaReset, "prefab/wnd/mecha/mechaReset.prefab") as zbMechaResetWnd;
		if ((Object)(object)zbMechaResetWnd2 != (Object)null)
		{
			zbMechaResetWnd2.RefreshCustom(mechaId, awardList);
		}
	}

	public void ShowMechaSkillUpgradePreview(int mechaId)
	{
		zbMechaSkillUpgradePreviewWnd zbMechaSkillUpgradePreviewWnd2 = ShowWnd(EWndId.MechaSkillUpgradePreview, "prefab/wnd/mecha/mechaSkillUpgradePreview.prefab") as zbMechaSkillUpgradePreviewWnd;
		if ((Object)(object)zbMechaSkillUpgradePreviewWnd2 != (Object)null)
		{
			zbMechaSkillUpgradePreviewWnd2.RefreshCustom(mechaId);
		}
	}

	public void ShowJourneyMovie(int type)
	{
		zbJourneyMovieWnd zbJourneyMovieWnd2 = ShowWnd(EWndId.JourneyMovie, "prefab/wnd/journey/journeyMovie.prefab") as zbJourneyMovieWnd;
		if ((Object)(object)zbJourneyMovieWnd2 != (Object)null)
		{
			zbJourneyMovieWnd2.RefreshCustom(type);
		}
	}

	public void ShowJourneyOneMain()
	{
		ShowWnd(EWndId.JourneyOneMain, "prefab/wnd/journey/journeyOne/journeyOneMain.prefab");
	}

	public void ShowJourneyOneBar()
	{
		zbJourneyOneBarWnd zbJourneyOneBarWnd2 = ShowWnd(EWndId.JourneyOneBar, "prefab/wnd/journey/journeyOne/journeyOneBar.prefab") as zbJourneyOneBarWnd;
		if ((Object)(object)zbJourneyOneBarWnd2 != (Object)null)
		{
			zbJourneyOneBarWnd2.RefreshCustom();
		}
	}

	public void ShowJourneyOneBarRate(int journeyType = 1)
	{
		zbJourneyOneBarRateWnd zbJourneyOneBarRateWnd2 = ShowWnd(EWndId.JourneyOneBarRate, "prefab/wnd/journey/journeyOne/journeyOneBarRate.prefab") as zbJourneyOneBarRateWnd;
		if ((Object)(object)zbJourneyOneBarRateWnd2 != (Object)null)
		{
			zbJourneyOneBarRateWnd2.RefreshCustom(journeyType);
		}
	}

	public void ShowJourneyOneRechargeSum()
	{
		ShowWnd(EWndId.JourneyOneRechargeSum, "prefab/wnd/journey/journeyOne/journeyOneRechargeSum.prefab");
	}

	public void ShowJourneyOneXunLuo()
	{
		ShowWnd(EWndId.JourneyOneXunLuo, "prefab/wnd/journey/journeyOne/journeyOneXunLuo.prefab");
	}

	public void ShowJourneyOneZhanling()
	{
		zbJourneyOneZhanlingWnd zbJourneyOneZhanlingWnd2 = ShowWnd(EWndId.JourneyOneZhanling, "prefab/wnd/journey/journeyOne/journeyOneZhanling.prefab") as zbJourneyOneZhanlingWnd;
		if ((Object)(object)zbJourneyOneZhanlingWnd2 != (Object)null)
		{
			zbJourneyOneZhanlingWnd2.RefreshCustom();
		}
	}

	public void ShowJourneyOneShop()
	{
		zbJourneyOneShopWnd zbJourneyOneShopWnd2 = ShowWnd(EWndId.JourneyOneShop, "prefab/wnd/journey/journeyOne/journeyOneShop.prefab") as zbJourneyOneShopWnd;
		if ((Object)(object)zbJourneyOneShopWnd2 != (Object)null)
		{
			zbJourneyOneShopWnd2.RefreshCustom();
		}
	}

	public void ShowJourneyTwoMain()
	{
		ShowWnd(EWndId.JourneyTwoMain, "prefab/wnd/journey/journeyTwo/journeyTwoMain.prefab");
	}

	public void ShowJourneyTwoBar()
	{
		zbJourneyTwoBarWnd zbJourneyTwoBarWnd2 = ShowWnd(EWndId.JourneyTwoBar, "prefab/wnd/journey/journeyTwo/journeyTwoBar.prefab") as zbJourneyTwoBarWnd;
		if ((Object)(object)zbJourneyTwoBarWnd2 != (Object)null)
		{
			zbJourneyTwoBarWnd2.RefreshCustom();
		}
	}

	public void ShowJourneyTwoDiamondCost()
	{
		ShowWnd(EWndId.JourneyTwoDiamondCost, "prefab/wnd/journey/journeyTwo/journeyTwoDiamondCost.prefab");
	}

	public void ShowJourneyTwoExchange()
	{
		ShowWnd(EWndId.JourneyTwoExchange, "prefab/wnd/journey/journeyTwo/journeyTwoExchange.prefab");
	}

	public void ShowJourneyTwoActive()
	{
		ShowWnd(EWndId.JourneyTwoActive, "prefab/wnd/journey/journeyTwo/journeyTwoActive.prefab");
	}

	public void ShowJourneyTwoZhanLing()
	{
		ShowWnd(EWndId.JourneyTwoZhanLing, "prefab/wnd/journey/journeyTwo/journeyTwoZhanLing.prefab");
	}

	public void ShowJourneyThreeMain()
	{
		ShowWnd(EWndId.JourneyThreeMain, "prefab/wnd/journey/journeyThree/journeyThreeMain.prefab");
	}

	public void ShowJourneyThreeBar()
	{
		zbJourneyThreeBarWnd zbJourneyThreeBarWnd2 = ShowWnd(EWndId.JourneyThreeBar, "prefab/wnd/journey/journeyThree/journeyThreeBar.prefab") as zbJourneyThreeBarWnd;
		if ((Object)(object)zbJourneyThreeBarWnd2 != (Object)null)
		{
			zbJourneyThreeBarWnd2.RefreshCustom();
		}
	}

	public void ShowJourneyThreeDrawCount()
	{
		ShowWnd(EWndId.JourneyThreeDrawCount, "prefab/wnd/journey/journeyThree/journeyThreeDrawCount.prefab");
	}

	public void ShowJourneyThreeExchange()
	{
		ShowWnd(EWndId.JourneyThreeExchange, "prefab/wnd/journey/journeyThree/journeyThreeExchange.prefab");
	}

	public void ShowJourneyThreeSign()
	{
		ShowWnd(EWndId.JourneyThreeSign, "prefab/wnd/journey/journeyThree/journeyThreeSign.prefab");
	}

	public void ShowJourneyThreeZhanLing()
	{
		ShowWnd(EWndId.JourneyThreeZhanLing, "prefab/wnd/journey/journeyThree/journeyThreeZhanLing.prefab");
	}

	public void ShowPvpModSelect()
	{
		zbPvpModSelectWnd zbPvpModSelectWnd2 = ShowWnd(EWndId.PvpModSelect, "prefab/wnd/pvp/pvpModSelect.prefab") as zbPvpModSelectWnd;
		if ((Object)(object)zbPvpModSelectWnd2 != (Object)null)
		{
			zbPvpModSelectWnd2.RefreshCustom();
		}
	}

	public void ShowPvpModChange()
	{
		zbPvpModChangeWnd zbPvpModChangeWnd2 = ShowWnd(EWndId.PvpModChange, "prefab/wnd/pvp/pvpModChange.prefab") as zbPvpModChangeWnd;
		if ((Object)(object)zbPvpModChangeWnd2 != (Object)null)
		{
			zbPvpModChangeWnd2.RefreshCustom();
		}
	}

	public void ShowPvpBalance()
	{
		zbPvpBalanceWnd zbPvpBalanceWnd2 = ShowWnd(EWndId.PvpBalance, "prefab/wnd/pvp/pvpBalance.prefab") as zbPvpBalanceWnd;
		if ((Object)(object)zbPvpBalanceWnd2 != (Object)null)
		{
			zbPvpBalanceWnd2.RefreshCustom();
		}
	}

	public void ShowWall(int tabIndex)
	{
		zbWallWnd zbWallWnd2 = ShowWnd(EWndId.Wall, "prefab/wnd/wall/wall.prefab") as zbWallWnd;
		if ((Object)(object)zbWallWnd2 != (Object)null)
		{
			zbWallWnd2.RefreshCustom(tabIndex);
		}
	}

	public void ShowWallSkill(int wallId, bool isPreview = false)
	{
		zbWallSkillWnd zbWallSkillWnd2 = ShowWnd(EWndId.WallSkill, "prefab/wnd/wall/wallSkill.prefab") as zbWallSkillWnd;
		if ((Object)(object)zbWallSkillWnd2 != (Object)null)
		{
			zbWallSkillWnd2.RefreshCustom(wallId, isPreview);
		}
	}

	public void ShowWallStar(int wallId)
	{
		zbWallStarWnd zbWallStarWnd2 = ShowWnd(EWndId.WallStar, "prefab/wnd/wall/wallStar.prefab") as zbWallStarWnd;
		if ((Object)(object)zbWallStarWnd2 != (Object)null)
		{
			zbWallStarWnd2.RefreshCustom(wallId);
		}
	}

	public void ShowWallConvert(WallDetail[] wallDetails)
	{
		zbWallConvertWnd zbWallConvertWnd2 = ShowWnd(EWndId.WallConvert, "prefab/wnd/wall/wallConvert.prefab") as zbWallConvertWnd;
		if ((Object)(object)zbWallConvertWnd2 != (Object)null)
		{
			zbWallConvertWnd2.RefreshCustom(wallDetails);
		}
	}

	public void ShowWallGet(int wallId)
	{
		zbWallGetWnd zbWallGetWnd2 = ShowWnd(EWndId.WallGet, "prefab/wnd/wall/wallGet.prefab") as zbWallGetWnd;
		if ((Object)(object)zbWallGetWnd2 != (Object)null)
		{
			zbWallGetWnd2.RefreshCustom(wallId);
		}
	}

	public void ShowWallStarSuccess(int wallId)
	{
		zbWallStarSuccessWnd zbWallStarSuccessWnd2 = ShowWnd(EWndId.WallStarSuccess, "prefab/wnd/wall/wallStarSuccess.prefab") as zbWallStarSuccessWnd;
		if ((Object)(object)zbWallStarSuccessWnd2 != (Object)null)
		{
			zbWallStarSuccessWnd2.RefreshCustom(wallId);
		}
	}

	public void ShowLegendary()
	{
		zbLegendaryWnd zbLegendaryWnd2 = ShowWnd(EWndId.Legendary, "prefab/wnd/legendary/legendary.prefab") as zbLegendaryWnd;
		if ((Object)(object)zbLegendaryWnd2 != (Object)null)
		{
			zbLegendaryWnd2.RefreshCustom(isFirstIn: true);
		}
	}

	public void ShowLegendaryZhanLing()
	{
		ShowWnd(EWndId.LegendaryZhanLing, "prefab/wnd/legendary/legendaryZhanLing.prefab");
	}

	public zbBaseWnd ShowArenaMain()
	{
		zbArenaMainWnd zbArenaMainWnd2 = ShowWnd(EWndId.ArenaMain, "prefab/wnd/arena/arenaMain.prefab") as zbArenaMainWnd;
		if ((Object)(object)zbArenaMainWnd2 != (Object)null)
		{
			zbArenaMainWnd2.RefreshCustom(isFirstIn: true);
		}
		return zbArenaMainWnd2;
	}

	public void ShowArena()
	{
		ShowWnd(EWndId.Arena, "prefab/wnd/arena/arena.prefab");
	}

	public void ShowArenaRecordWnd(ArenaRecordDetail[] recordList)
	{
		zbArenaRecordWnd zbArenaRecordWnd2 = ShowWnd(EWndId.ArenaRecord, "prefab/wnd/arena/arenaRecord.prefab") as zbArenaRecordWnd;
		if ((Object)(object)zbArenaRecordWnd2 != (Object)null)
		{
			zbArenaRecordWnd2.RefreshCustom(recordList);
		}
	}

	public void ShowArenaEffect(RankDetail[] recordList)
	{
		zbArenaEffectWnd zbArenaEffectWnd2 = ShowWnd(EWndId.ArenaEffect, "prefab/wnd/arena/arenaEffect.prefab") as zbArenaEffectWnd;
		if ((Object)(object)zbArenaEffectWnd2 != (Object)null)
		{
			zbArenaEffectWnd2.RefreshCustom(recordList);
		}
	}

	public void ShowArenaDetail(EBattleFuncType battleFuncType, RoleBaseInfo roleBaseInfo, ArenaRankInfo arenaRankInfo = null, CrossLadderAchieveInfo achieveInfo = null, int peakRank = 0, TruckTransportDetail truckInfo = null)
	{
		zbArenaDetailWnd zbArenaDetailWnd2 = ShowWnd(EWndId.ArenaDetail, "prefab/wnd/arena/arenaDetail.prefab") as zbArenaDetailWnd;
		if ((Object)(object)zbArenaDetailWnd2 != (Object)null)
		{
			zbArenaDetailWnd2.RefreshCustom(battleFuncType, roleBaseInfo, arenaRankInfo, achieveInfo, peakRank, truckInfo);
		}
	}

	public void ShowArenaRank(ArenaRankListAck rankListAck)
	{
		zbArenaRankWnd zbArenaRankWnd2 = ShowWnd(EWndId.ArenaRank, "prefab/wnd/arena/arenaRank.prefab") as zbArenaRankWnd;
		if ((Object)(object)zbArenaRankWnd2 != (Object)null)
		{
			zbArenaRankWnd2.RefreshCustom(rankListAck);
		}
	}

	public void ShowArenaGift()
	{
		zbArenaGiftWnd zbArenaGiftWnd2 = ShowWnd(EWndId.ArenaGift, "prefab/wnd/arena/arenaGift.prefab") as zbArenaGiftWnd;
		if ((Object)(object)zbArenaGiftWnd2 != (Object)null)
		{
			zbArenaGiftWnd2.RefreshCustom(isFirstIn: true);
		}
	}

	public void ShowArenaBestRank(long diamondCount)
	{
		zbArenaBestRankWnd zbArenaBestRankWnd2 = ShowWnd(EWndId.ArenaBestRank, "prefab/wnd/arena/arenaBestRank.prefab") as zbArenaBestRankWnd;
		if ((Object)(object)zbArenaBestRankWnd2 != (Object)null)
		{
			zbArenaBestRankWnd2.RefreshCustom(diamondCount);
		}
	}

	public void ShowArenaRankAward()
	{
		zbArenaRankAwardWnd zbArenaRankAwardWnd2 = ShowWnd(EWndId.ArenaRankAward, "prefab/wnd/arena/arenaRankAward.prefab") as zbArenaRankAwardWnd;
		if ((Object)(object)zbArenaRankAwardWnd2 != (Object)null)
		{
			zbArenaRankAwardWnd2.RefreshCustom();
		}
	}

	public void ShowFoothold(int index)
	{
		zbFootholdWnd zbFootholdWnd2 = ShowWnd(EWndId.Foothold, "prefab/wnd/guard/foothold.prefab") as zbFootholdWnd;
		if ((Object)(object)zbFootholdWnd2 != (Object)null)
		{
			zbFootholdWnd2.RefreshCustom(index);
		}
	}

	public void ShowFootholdConvert(FootholdDetail[] footholdDetails)
	{
		zbFootholdConvertWnd zbFootholdConvertWnd2 = ShowWnd(EWndId.FootholdConvert, "prefab/wnd/guard/footholdConvert.prefab") as zbFootholdConvertWnd;
		if ((Object)(object)zbFootholdConvertWnd2 != (Object)null)
		{
			zbFootholdConvertWnd2.RefreshCustom(footholdDetails);
		}
	}

	public void ShowFootholdStar(int footholdId)
	{
		zbFootholdStarWnd zbFootholdStarWnd2 = ShowWnd(EWndId.FootholdStar, "prefab/wnd/guard/footholdStar.prefab") as zbFootholdStarWnd;
		if ((Object)(object)zbFootholdStarWnd2 != (Object)null)
		{
			zbFootholdStarWnd2.RefreshCustom(footholdId);
		}
	}

	public void ShowFootholdGet(int footholdId)
	{
		zbFootholdGetWnd zbFootholdGetWnd2 = ShowWnd(EWndId.FootholdGet, "prefab/wnd/guard/footholdGet.prefab") as zbFootholdGetWnd;
		if ((Object)(object)zbFootholdGetWnd2 != (Object)null)
		{
			zbFootholdGetWnd2.RefreshCustom(footholdId);
		}
	}

	public void ShowFootholdStarSuccess(int footholdId)
	{
		zbFootholdStarSuccessWnd zbFootholdStarSuccessWnd2 = ShowWnd(EWndId.FootholdStarSuccess, "prefab/wnd/guard/footholdStarSuccess.prefab") as zbFootholdStarSuccessWnd;
		if ((Object)(object)zbFootholdStarSuccessWnd2 != (Object)null)
		{
			zbFootholdStarSuccessWnd2.RefreshCustom(footholdId);
		}
	}

	public void ShowCrossLadder()
	{
		ShowWnd(EWndId.CrossLadder, "prefab/wnd/crossLadder/crossLadder.prefab");
	}

	public void ShowCrossLadderSeasonInit()
	{
		ShowWnd(EWndId.CrossLadderSeasonInit, "prefab/wnd/crossLadder/crossLadderSeasonInit.prefab");
	}

	public void ShowCrossLadderMatching()
	{
		ShowWnd(EWndId.CrossLadderMatching, "prefab/wnd/crossLadder/crossLadderMatching.prefab");
	}

	public void ShowCrossLadderMatchSuccess(CrossLadderPlayerInfo[] list)
	{
		zbCrossLadderMatchSuccessWnd zbCrossLadderMatchSuccessWnd2 = ShowWnd(EWndId.CrossLadderMatchSuccess, "prefab/wnd/crossLadder/crossLadderMatchSuccess.prefab") as zbCrossLadderMatchSuccessWnd;
		if ((Object)(object)zbCrossLadderMatchSuccessWnd2 != (Object)null)
		{
			zbCrossLadderMatchSuccessWnd2.RefreshCustom(list);
		}
	}

	public void ShowCrossLadderBalance()
	{
		ShowWnd(EWndId.CrossLadderBalance, "prefab/wnd/crossLadder/crossLadderBalance.prefab");
	}

	public void ShowCrossLadderUpgrade(float OverPct)
	{
		zbCrossLadderUpgradeWnd zbCrossLadderUpgradeWnd2 = ShowWnd(EWndId.CrossLadderUpgrade, "prefab/wnd/crossLadder/crossLadderUpgrade.prefab") as zbCrossLadderUpgradeWnd;
		if ((Object)(object)zbCrossLadderUpgradeWnd2 != (Object)null)
		{
			zbCrossLadderUpgradeWnd2.RefreshCustom(OverPct);
		}
	}

	public void ShowCrossLadderUpgradeFail()
	{
		ShowWnd(EWndId.CrossLadderUpgradeFail, "prefab/wnd/crossLadder/crossLadderUpgradeFail.prefab");
	}

	public zbBaseWnd ShowCrossLadderSettleAward()
	{
		return ShowWnd(EWndId.CrossLadderSettleAward, "prefab/wnd/crossLadder/crossLadderSettleAward.prefab");
	}

	public void ShowCrossLadderRecord(CrossLadderRecordDetail[] recordList)
	{
		zbCrossLadderRecordWnd zbCrossLadderRecordWnd2 = ShowWnd(EWndId.CrossLadderRecord, "prefab/wnd/crossLadder/crossLadderRecord.prefab") as zbCrossLadderRecordWnd;
		if ((Object)(object)zbCrossLadderRecordWnd2 != (Object)null)
		{
			zbCrossLadderRecordWnd2.RefreshCustom(recordList);
		}
	}

	public void ShowCrossLadderHelpCardWnd()
	{
		zbCrossLadderHelpCardWnd zbCrossLadderHelpCardWnd2 = ShowWnd(EWndId.CrossLadderHelpCard, "prefab/wnd/crossLadder/crossLadderHelpCard.prefab") as zbCrossLadderHelpCardWnd;
		if ((Object)(object)zbCrossLadderHelpCardWnd2 != (Object)null)
		{
			zbCrossLadderHelpCardWnd2.Refresh();
		}
	}

	public void ShowCrossLadderRank()
	{
		zbCrossLadderRankWnd zbCrossLadderRankWnd2 = ShowWnd(EWndId.CrossLadderRank, "prefab/wnd/crossLadder/crossLadderRank.prefab") as zbCrossLadderRankWnd;
		if (Object.op_Implicit((Object)(object)zbCrossLadderRankWnd2))
		{
			zbCrossLadderRankWnd2.RefreshCustom();
		}
	}

	public void ShowTreasure(int tabIndex)
	{
		zbTreasureWnd zbTreasureWnd2 = ShowWnd(EWndId.Treasure, "prefab/wnd/treasure/treasure.prefab") as zbTreasureWnd;
		if ((Object)(object)zbTreasureWnd2 != (Object)null)
		{
			zbTreasureWnd2.RefreshCustom(tabIndex);
		}
	}

	public void ShowTreasureDetail(zbTreasureCsvRecord treasureRecord)
	{
		zbTreasureDetailWnd zbTreasureDetailWnd2 = ShowWnd(EWndId.TreasureDetail, "prefab/wnd/treasure/treasureDetail.prefab") as zbTreasureDetailWnd;
		if ((Object)(object)zbTreasureDetailWnd2 != (Object)null)
		{
			zbTreasureDetailWnd2.RefreshCustom(treasureRecord);
		}
	}

	public void ShowTreasureUpgrade()
	{
		ShowWnd(EWndId.TreasureUpgrade, "prefab/wnd/treasure/treasureUpgrade.prefab");
	}

	public void ShowTreasureConvert(TreasureDetail[] treasureDetails)
	{
		zbTreasureConvertWnd zbTreasureConvertWnd2 = ShowWnd(EWndId.TreasureConvert, "prefab/wnd/treasure/treasureConvert.prefab") as zbTreasureConvertWnd;
		if ((Object)(object)zbTreasureConvertWnd2 != (Object)null)
		{
			zbTreasureConvertWnd2.RefreshCustom(treasureDetails);
		}
	}

	public void ShowTreasurePool()
	{
		ShowWnd(EWndId.TreasurePool, "prefab/wnd/treasure/treasurePool.prefab");
	}

	public void ShowTreasurePrivilege()
	{
		ShowWnd(EWndId.TreasurePrivilege, "prefab/wnd/treasure/treasurePrivilege.prefab");
	}

	public void ShowServant(ServantDetail detail = null)
	{
		zbServantWnd zbServantWnd2 = ShowWnd(EWndId.Servant, "prefab/wnd/servant/servant.prefab") as zbServantWnd;
		if ((Object)(object)zbServantWnd2 != (Object)null)
		{
			zbServantWnd2.RefreshCustom(detail);
		}
	}

	public void ShowServantSelect()
	{
		zbServantSelectWnd zbServantSelectWnd2 = ShowWnd(EWndId.ServantSelect, "prefab/wnd/servant/servantSelect.prefab") as zbServantSelectWnd;
		if ((Object)(object)zbServantSelectWnd2 != (Object)null)
		{
			zbServantSelectWnd2.RefreshCustom();
		}
	}

	public void ShowServantIncubate()
	{
		zbServantIncubateWnd zbServantIncubateWnd2 = ShowWnd(EWndId.ServantIncubate, "prefab/wnd/servant/servantIncubate.prefab") as zbServantIncubateWnd;
		if ((Object)(object)zbServantIncubateWnd2 != (Object)null)
		{
			zbServantIncubateWnd2.RefreshCustom();
		}
	}

	public void ShowServantGet(ServantDetail detail)
	{
		zbServantGetWnd zbServantGetWnd2 = ShowWnd(EWndId.ServantGet, "prefab/wnd/servant/servantGet.prefab") as zbServantGetWnd;
		if ((Object)(object)zbServantGetWnd2 != (Object)null)
		{
			zbServantGetWnd2.RefreshCustom(detail);
		}
	}

	public void ShowServantUpgradeQuality(ServantDetail detail)
	{
		zbServantUpgradeQualityWnd zbServantUpgradeQualityWnd2 = ShowWnd(EWndId.ServantUpgradeQuality, "prefab/wnd/servant/servantUpgradeQuality.prefab") as zbServantUpgradeQualityWnd;
		if ((Object)(object)zbServantUpgradeQualityWnd2 != (Object)null)
		{
			zbServantUpgradeQualityWnd2.RefreshCustom(detail);
		}
	}

	public void ShowServantSubSkillTip(string desc, Vector2 pos)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		zbServantSubSkillTipWnd zbServantSubSkillTipWnd2 = ShowWnd(EWndId.ServantSubSkillTip, "prefab/wnd/servant/servantSubSkillTip.prefab") as zbServantSubSkillTipWnd;
		if ((Object)(object)zbServantSubSkillTipWnd2 != (Object)null)
		{
			zbServantSubSkillTipWnd2.RefreshCustom(desc, pos);
		}
	}

	public void ShowServantPrivilege()
	{
		zbServantPrivilegeWnd zbServantPrivilegeWnd2 = ShowWnd(EWndId.ServantPrivilege, "prefab/wnd/servant/servantPrivilege.prefab") as zbServantPrivilegeWnd;
		if ((Object)(object)zbServantPrivilegeWnd2 != (Object)null)
		{
			zbServantPrivilegeWnd2.Refresh();
		}
	}

	public void ShowServantSubSkillHandBook()
	{
		ShowWnd(EWndId.ServantSubSkillHandBook, "prefab/wnd/servant/servantSubSkillHandBook.prefab");
	}

	public void ShowBossSubjectTask(BossSubjectDetail bossSubjectDetail)
	{
		zbBossSubjectTaskWnd zbBossSubjectTaskWnd2 = ShowWnd(EWndId.BossSubjectTask, "prefab/wnd/bossSubject/bossSubjectTask.prefab") as zbBossSubjectTaskWnd;
		if ((Object)(object)zbBossSubjectTaskWnd2 != (Object)null)
		{
			zbBossSubjectTaskWnd2.RefreshCustom(bossSubjectDetail);
		}
	}

	public void ShowBossSubjectMain(BossSubjectDetail bossSubjectDetail, int tabIndex = 0)
	{
		zbBossSubjectWnd zbBossSubjectWnd2 = ShowWnd(EWndId.BossSubjectMain, "prefab/wnd/bossSubject/bossSubjectMain.prefab") as zbBossSubjectWnd;
		if ((Object)(object)zbBossSubjectWnd2 != (Object)null)
		{
			zbBossSubjectWnd2.RefreshCustom(bossSubjectDetail, _fromOut: true, tabIndex);
		}
	}

	public void ShowBossSubjectPerfectPassTip(BossSubjectDetail _bossSubjectDetail, int toLevel)
	{
		zbBossSubjectPerfectPassTipWnd zbBossSubjectPerfectPassTipWnd2 = ShowWnd(EWndId.BossSubjectPerfectPassTip, "prefab/wnd/bossSubject/bossSubjectPerfectPassTip.prefab") as zbBossSubjectPerfectPassTipWnd;
		if ((Object)(object)zbBossSubjectPerfectPassTipWnd2 != (Object)null)
		{
			zbBossSubjectPerfectPassTipWnd2.RefreshCustom(_bossSubjectDetail, toLevel);
		}
	}

	public void ShowBossSubjectDiffTip(BossSubjectDetail _bossSubjectDetail, int toLevel)
	{
		zbBossSubjectDiffTipWnd zbBossSubjectDiffTipWnd2 = ShowWnd(EWndId.BossSubjectDiffTip, "prefab/wnd/bossSubject/bossSubjectDiffTip.prefab") as zbBossSubjectDiffTipWnd;
		if ((Object)(object)zbBossSubjectDiffTipWnd2 != (Object)null)
		{
			zbBossSubjectDiffTipWnd2.RefreshCustom(_bossSubjectDetail, toLevel);
		}
	}

	public void ShowWallSubject(WallSubjectDetail data, int tabIndex = 0)
	{
		zbWallSubjectWnd zbWallSubjectWnd2 = ShowWnd(EWndId.WallSubject, "prefab/wnd/wallSubject/wallSubject.prefab") as zbWallSubjectWnd;
		if ((Object)(object)zbWallSubjectWnd2 != (Object)null)
		{
			zbWallSubjectWnd2.RefreshCustom(data, tabIndex);
		}
	}

	public void ShowWallSubjectBar(WallSubjectDetail data)
	{
		zbWallSubjectBarWnd zbWallSubjectBarWnd2 = ShowWnd(EWndId.WallSubjectBar, "prefab/wnd/wallSubject/wallSubjectBar.prefab") as zbWallSubjectBarWnd;
		if ((Object)(object)zbWallSubjectBarWnd2 != (Object)null)
		{
			zbWallSubjectBarWnd2.RefreshCustom(data);
		}
	}

	public void ShowWallSubjectRelic(WallSubjectDetail data)
	{
		zbWallSubjectRelicWnd zbWallSubjectRelicWnd2 = ShowWnd(EWndId.WallSubjectRelic, "prefab/wnd/wallSubject/wallSubjectRelic.prefab") as zbWallSubjectRelicWnd;
		if ((Object)(object)zbWallSubjectRelicWnd2 != (Object)null)
		{
			zbWallSubjectRelicWnd2.RefreshCustom(data);
		}
	}

	public void ShowWallSubjectRelicTip(zbWallSubjectRelicCsvRecord record, bool isActive)
	{
		zbWallSubjectRelicTipWnd zbWallSubjectRelicTipWnd2 = ShowWnd(EWndId.WallSubjectRelicTip, "prefab/wnd/wallSubject/wallSubjectRelicTip.prefab") as zbWallSubjectRelicTipWnd;
		if ((Object)(object)zbWallSubjectRelicTipWnd2 != (Object)null)
		{
			zbWallSubjectRelicTipWnd2.RefreshCustom(record, isActive);
		}
	}

	public void ShowMechaSubject(MechaSubjectDetail data, int tabIndex = 0)
	{
		zbMechaSubjectWnd zbMechaSubjectWnd2 = ShowWnd(EWndId.MechaSubject, "prefab/wnd/mechaSubject/mechaSubject.prefab") as zbMechaSubjectWnd;
		if ((Object)(object)zbMechaSubjectWnd2 != (Object)null)
		{
			zbMechaSubjectWnd2.RefreshCustom(data, tabIndex);
		}
	}

	public void ShowMechaSubjectBar(MechaSubjectDetail data)
	{
		zbMechaSubjectBarWnd zbMechaSubjectBarWnd2 = ShowWnd(EWndId.MechaSubjectBar, "prefab/wnd/mechaSubject/mechaSubjectBar.prefab") as zbMechaSubjectBarWnd;
		if ((Object)(object)zbMechaSubjectBarWnd2 != (Object)null)
		{
			zbMechaSubjectBarWnd2.RefreshCustom(data);
		}
	}

	public void ShowMechaSubjectRelic(MechaSubjectDetail data)
	{
		zbMechaSubjectRelicWnd zbMechaSubjectRelicWnd2 = ShowWnd(EWndId.MechaSubjectRelic, "prefab/wnd/mechaSubject/mechaSubjectRelic.prefab") as zbMechaSubjectRelicWnd;
		if ((Object)(object)zbMechaSubjectRelicWnd2 != (Object)null)
		{
			zbMechaSubjectRelicWnd2.RefreshCustom(data);
		}
	}

	public void ShowMechaSubjectRelicTip(zbMechaSubjectRelicCsvRecord record, bool isActive)
	{
		zbMechaSubjectRelicTipWnd zbMechaSubjectRelicTipWnd2 = ShowWnd(EWndId.MechaSubjectRelicTip, "prefab/wnd/mechaSubject/mechaSubjectRelicTip.prefab") as zbMechaSubjectRelicTipWnd;
		if ((Object)(object)zbMechaSubjectRelicTipWnd2 != (Object)null)
		{
			zbMechaSubjectRelicTipWnd2.RefreshCustom(record, isActive);
		}
	}

	public void BubbleInfo(string msg)
	{
		zbBubbleWnd zbBubbleWnd2 = ShowBubble() as zbBubbleWnd;
		if ((Object)(object)zbBubbleWnd2 != (Object)null)
		{
			zbBubbleWnd2.BubbleInfo(msg);
		}
	}

	public void BubbleErrorCode(ErrorCodeAck ack)
	{
		zbBubbleWnd zbBubbleWnd2 = ShowBubble() as zbBubbleWnd;
		if ((Object)(object)zbBubbleWnd2 != (Object)null)
		{
			zbBubbleWnd2.BubbleErrorCode(ack.ErrorCode.ToString(), (int)ack.ErrorCode);
		}
	}

	public void BubbleErrorCodeId(EErrorCode ErrorCode)
	{
		zbBubbleWnd zbBubbleWnd2 = ShowBubble() as zbBubbleWnd;
		if ((Object)(object)zbBubbleWnd2 != (Object)null)
		{
			zbBubbleWnd2.BubbleErrorCode(ErrorCode.ToString(), (int)ErrorCode);
		}
	}

	public void ShowCrossPeak()
	{
		zbCrossPeakWnd zbCrossPeakWnd2 = ShowWnd(EWndId.CrossPeak, "prefab/wnd/crossPeak/crossPeak.prefab") as zbCrossPeakWnd;
		if ((Object)(object)zbCrossPeakWnd2 != (Object)null)
		{
			zbCrossPeakWnd2.RefreshCustom(byUIMgr: true);
		}
	}

	public void ShowCrossPeakRecord(CrossPeakRecordDetail[] recordList)
	{
		zbCrossPeakRecordWnd zbCrossPeakRecordWnd2 = ShowWnd(EWndId.CrossPeakRecord, "prefab/wnd/crossPeak/crossPeakRecord.prefab") as zbCrossPeakRecordWnd;
		if ((Object)(object)zbCrossPeakRecordWnd2 != (Object)null)
		{
			zbCrossPeakRecordWnd2.RefreshCustom(recordList);
		}
	}

	public void ShowCrossPeakRank()
	{
		zbCrossPeakRankWnd zbCrossPeakRankWnd2 = ShowWnd(EWndId.CrossPeakRank, "prefab/wnd/crossPeak/crossPeakRank.prefab") as zbCrossPeakRankWnd;
		if ((Object)(object)zbCrossPeakRankWnd2 != (Object)null)
		{
			zbCrossPeakRankWnd2.RefreshCustom();
		}
	}

	public void ShowCrossPeakBestRankWnd()
	{
		zbCrossPeakBestRankWnd zbCrossPeakBestRankWnd2 = ShowWnd(EWndId.CrossPeakBestRank, "prefab/wnd/crossPeak/crossPeakBestRank.prefab") as zbCrossPeakBestRankWnd;
		if ((Object)(object)zbCrossPeakBestRankWnd2 != (Object)null)
		{
			zbCrossPeakBestRankWnd2.RefreshCustom();
		}
	}

	public void ShowTruckMainWnd(int tabIndex, bool fromTask = false)
	{
		zbTruckMainWnd zbTruckMainWnd2 = ShowWnd(EWndId.TruckMain, "prefab/wnd/truck/truckMain.prefab") as zbTruckMainWnd;
		if ((Object)(object)zbTruckMainWnd2 != (Object)null)
		{
			zbTruckMainWnd2.RefreshCustom(tabIndex, fromEvent: false, fromTask);
		}
	}

	public void ShowTruckStartWnd(int _TruckId)
	{
		zbTruckStartWnd zbTruckStartWnd2 = ShowWnd(EWndId.TruckStart, "prefab/wnd/truck/truckStart.prefab") as zbTruckStartWnd;
		if ((Object)(object)zbTruckStartWnd2 != (Object)null)
		{
			zbTruckStartWnd2.RefreshCustom(_TruckId);
		}
	}

	public void ShowTruckRecord(int tabIndex)
	{
		zbTruckRecordWnd zbTruckRecordWnd2 = ShowWnd(EWndId.TruckRecord, "prefab/wnd/truck/truckRecord.prefab") as zbTruckRecordWnd;
		if ((Object)(object)zbTruckRecordWnd2 != (Object)null)
		{
			zbTruckRecordWnd2.RefreshCustom(tabIndex);
		}
	}

	public void ShowTruckHelpCardWnd()
	{
		zbTruckHelpCardWnd zbTruckHelpCardWnd2 = ShowWnd(EWndId.TruckHelpCard, "prefab/wnd/truck/truckHelpCard.prefab") as zbTruckHelpCardWnd;
		if ((Object)(object)zbTruckHelpCardWnd2 != (Object)null)
		{
			zbTruckHelpCardWnd2.Refresh();
		}
	}

	public void ShowTruckBalance(TruckTransportInfo _truckTransportInfo, bool _isRuning)
	{
		zbTruckBalanceWnd zbTruckBalanceWnd2 = ShowWnd(EWndId.TruckBalance, "prefab/wnd/truck/truckBalance.prefab") as zbTruckBalanceWnd;
		if ((Object)(object)zbTruckBalanceWnd2 != (Object)null)
		{
			zbTruckBalanceWnd2.RefreshCustom(_truckTransportInfo, _isRuning);
		}
	}

	public void ShowTruckWinLose()
	{
		zbTruckWinLoseWnd zbTruckWinLoseWnd2 = ShowWnd(EWndId.TruckWinLose, "prefab/wnd/truck/truckWinLose.prefab") as zbTruckWinLoseWnd;
		if ((Object)(object)zbTruckWinLoseWnd2 != (Object)null)
		{
			zbTruckWinLoseWnd2.RefreshCustom();
		}
	}

	public void ShowTrainSelectTip()
	{
		zbTrainSelectTipWnd zbTrainSelectTipWnd2 = ShowWnd(EWndId.TrainSelectTip, "prefab/wnd/truck/trainSelectTip.prefab") as zbTrainSelectTipWnd;
		if ((Object)(object)zbTrainSelectTipWnd2 != (Object)null)
		{
			zbTrainSelectTipWnd2.RefreshCustom();
		}
	}

	public void ShowTrainDriver()
	{
		zbTrainDriverWnd zbTrainDriverWnd2 = ShowWnd(EWndId.TrainSelect, "prefab/wnd/truck/trainDriver.prefab") as zbTrainDriverWnd;
		if ((Object)(object)zbTrainDriverWnd2 != (Object)null)
		{
			zbTrainDriverWnd2.RefreshCustom();
		}
	}

	public void ShowTrainTaker()
	{
		zbTrainTakerWnd zbTrainTakerWnd2 = ShowWnd(EWndId.TrainTaker, "prefab/wnd/truck/trainTaker.prefab") as zbTrainTakerWnd;
		if ((Object)(object)zbTrainTakerWnd2 != (Object)null)
		{
			zbTrainTakerWnd2.RefreshCustom();
		}
	}

	public void ShowTrainGoldTrain(zbTrainCsvRecord _record)
	{
		zbTrainGoldTrainWnd zbTrainGoldTrainWnd2 = ShowWnd(EWndId.TrainGoldTrain, "prefab/wnd/truck/trainGoldTrain.prefab") as zbTrainGoldTrainWnd;
		if ((Object)(object)zbTrainGoldTrainWnd2 != (Object)null)
		{
			zbTrainGoldTrainWnd2.RefreshCustom(_record);
		}
	}

	public void ShowTrainRob(TrainDetail trainDetail, bool isSelf)
	{
		zbTrainRobWnd zbTrainRobWnd2 = ShowWnd(EWndId.TrainRob, "prefab/wnd/truck/trainRob.prefab") as zbTrainRobWnd;
		if ((Object)(object)zbTrainRobWnd2 != (Object)null)
		{
			zbTrainRobWnd2.RefreshCustom(trainDetail, isSelf);
		}
	}

	public void ShowLegionWar()
	{
		zbLegionWarWnd zbLegionWarWnd2 = ShowWnd(EWndId.LegionWar, "prefab/wnd/legionWar/legionWar.prefab") as zbLegionWarWnd;
		if ((Object)(object)zbLegionWarWnd2 != (Object)null)
		{
			zbLegionWarWnd2.RefreshCustom(fromOut: true);
		}
	}

	public void ShowLegionWarWatch()
	{
		zbLegionWarWatchWnd zbLegionWarWatchWnd2 = ShowWnd(EWndId.LegionWarWatch, "prefab/wnd/legionWar/legionWarWatch/legionWarWatch.prefab") as zbLegionWarWatchWnd;
		if ((Object)(object)zbLegionWarWatchWnd2 != (Object)null)
		{
			zbLegionWarWatchWnd2.RefreshCustom(fromOut: true);
		}
	}

	public void ShowLegionWarFight()
	{
		zbLegionWarFightWnd zbLegionWarFightWnd2 = ShowWnd(EWndId.LegionWarFight, "prefab/wnd/legionWar/legionWarFight.prefab") as zbLegionWarFightWnd;
		if ((Object)(object)zbLegionWarFightWnd2 != (Object)null)
		{
			zbLegionWarFightWnd2.RefreshCustom();
		}
	}

	public void ShowFacilityHub(int tabIndex)
	{
		zbFacilityHubWnd zbFacilityHubWnd2 = ShowWnd(EWndId.FacilityHub, "prefab/wnd/facility/facilityHub.prefab") as zbFacilityHubWnd;
		if ((Object)(object)zbFacilityHubWnd2 != (Object)null)
		{
			zbFacilityHubWnd2.RefreshCustom(tabIndex);
		}
	}

	public void ShowFacilityDetail(FacilityBaseInfo facilityBaseInfo)
	{
		zbFacilityDetailWnd zbFacilityDetailWnd2 = ShowWnd(EWndId.FacilityDetail, "prefab/wnd/facility/facilityDetail.prefab") as zbFacilityDetailWnd;
		if ((Object)(object)zbFacilityDetailWnd2 != (Object)null)
		{
			zbFacilityDetailWnd2.RefreshCustom(facilityBaseInfo);
		}
	}

	public void ShowFacility()
	{
		zbFacilityWnd zbFacilityWnd2 = ShowWnd(EWndId.Facility, "prefab/wnd/facility/facility.prefab") as zbFacilityWnd;
		if ((Object)(object)zbFacilityWnd2 != (Object)null)
		{
			zbFacilityWnd2.RefreshCustom(isFirstIn: true);
		}
	}

	public void ShowFacilityEventChest(FacilityRoomDetail roomDetail)
	{
		zbFacilityEventChestWnd zbFacilityEventChestWnd2 = ShowWnd(EWndId.FacilityEventChest, "prefab/wnd/facility/facilityEventChest.prefab") as zbFacilityEventChestWnd;
		if ((Object)(object)zbFacilityEventChestWnd2 != (Object)null)
		{
			zbFacilityEventChestWnd2.RefreshCustom(roomDetail);
		}
	}

	public void ShowFacilityEventMonster(FacilityRoomDetail roomDetail, RoleBaseInfo roleBaseInfo, BattleInfo battleInfo = null)
	{
		zbFacilityEventMonsterWnd zbFacilityEventMonsterWnd2 = ShowWnd(EWndId.FacilityEventMonster, "prefab/wnd/facility/facilityEventMonster.prefab") as zbFacilityEventMonsterWnd;
		if ((Object)(object)zbFacilityEventMonsterWnd2 != (Object)null)
		{
			zbFacilityEventMonsterWnd2.RefreshCustom(roomDetail, roleBaseInfo, battleInfo);
		}
	}

	public void ShowFacilityEventShop(FacilityRoomDetail roomDetail, FacilityShopInfo shopInfo)
	{
		zbFacilityEventShopWnd zbFacilityEventShopWnd2 = ShowWnd(EWndId.FacilityEventShop, "prefab/wnd/facility/facilityEventShop.prefab") as zbFacilityEventShopWnd;
		if ((Object)(object)zbFacilityEventShopWnd2 != (Object)null)
		{
			zbFacilityEventShopWnd2.RefreshCustom(roomDetail, shopInfo);
		}
	}

	public void ShowFacilityEventRolling(FacilityRoomDetail roomDetail)
	{
		zbFacilityEventRollingWnd zbFacilityEventRollingWnd2 = ShowWnd(EWndId.FacilityEventRolling, "prefab/wnd/facility/facilityEventRolling.prefab") as zbFacilityEventRollingWnd;
		if ((Object)(object)zbFacilityEventRollingWnd2 != (Object)null)
		{
			zbFacilityEventRollingWnd2.RefreshCustom(roomDetail);
		}
	}

	public void ShowFacilityMessage(string title, string message, bool needTili, Action<bool> callback, long explorerExpire = 0L, AwardDetail award = null)
	{
		zbFacilityMessageWnd zbFacilityMessageWnd2 = ShowWnd(EWndId.FacilityMessage, "prefab/wnd/facility/facilityMessage.prefab") as zbFacilityMessageWnd;
		if ((Object)(object)zbFacilityMessageWnd2 != (Object)null)
		{
			zbFacilityMessageWnd2.RefreshCustom(title, message, needTili, callback, explorerExpire, award);
		}
	}

	public void ShowFacilityResult()
	{
		ShowWnd(EWndId.FacilityResult, "prefab/wnd/facility/facilityResult.prefab");
	}

	public void ShowTermDescTip(zbLangTermCsvRecord record, Vector2 pos)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		zbTermDescTipWnd zbTermDescTipWnd2 = ShowWnd(EWndId.TermDescTip, "prefab/wnd/tip/termDescTip.prefab") as zbTermDescTipWnd;
		if ((Object)(object)zbTermDescTipWnd2 != (Object)null)
		{
			zbTermDescTipWnd2.RefreshCustom(record, pos);
		}
	}

	public void ShowCardDetail()
	{
		zbCardDetailWnd zbCardDetailWnd2 = ShowWnd(EWndId.CardDetail, "prefab/wnd/carddetail/carddetail.prefab") as zbCardDetailWnd;
		if ((Object)(object)zbCardDetailWnd2 != (Object)null)
		{
			zbCardDetailWnd2.RefreshCustom();
		}
	}

	public void ShowStronger(Action<bool> callback)
	{
		zbStrongerWnd zbStrongerWnd2 = ShowWnd(EWndId.Stronger, "prefab/wnd/comm/stronger.prefab") as zbStrongerWnd;
		if ((Object)(object)zbStrongerWnd2 != (Object)null)
		{
			zbStrongerWnd2.RefreshCustom(callback);
		}
	}

	public void ShowSeasonWnd()
	{
		zbSeasonWnd zbSeasonWnd2 = ShowWnd(EWndId.Season, "prefab/season/season.prefab") as zbSeasonWnd;
		if ((Object)(object)zbSeasonWnd2 != (Object)null)
		{
			zbSeasonWnd2.RefreshCustom();
		}
	}

	public void ShowSeasonBuildDetail(zbSeasonBuild build)
	{
		zbSeasonBuildDetailWnd zbSeasonBuildDetailWnd2 = ShowWnd(EWndId.SeasonBuildDetail, "prefab/season/buildDetail.prefab") as zbSeasonBuildDetailWnd;
		if ((Object)(object)zbSeasonBuildDetailWnd2 != (Object)null)
		{
			zbSeasonBuildDetailWnd2.RefreshCustom(build);
		}
	}

	public void ShowSeasonCityWar()
	{
		zbSeasonCityWarWnd zbSeasonCityWarWnd2 = ShowWnd(EWndId.SeasonCityWar, "prefab/season/seasonCityWar.prefab") as zbSeasonCityWarWnd;
		if ((Object)(object)zbSeasonCityWarWnd2 != (Object)null)
		{
			zbSeasonCityWarWnd2.RefreshCustom();
		}
	}

	public void ShowSeasonAirdropClash()
	{
		ShowWnd(EWndId.SeasonAirdropClash, "prefab/season/seasonAirdropClash.prefab");
	}

	public void ShowSeasonAirdropList(ESeasonAirdropQuality quality)
	{
		zbSeasonAirdropListWnd zbSeasonAirdropListWnd2 = ShowWnd(EWndId.SeasonAirdropList, "prefab/season/seasonAirdropList.prefab") as zbSeasonAirdropListWnd;
		if ((Object)(object)zbSeasonAirdropListWnd2 != (Object)null)
		{
			zbSeasonAirdropListWnd2.RefreshCustom(quality);
		}
	}

	public void ShowSeasonAirdropDetail(SeasonAirdropInfo data)
	{
		zbSeasonAirdropDetailWnd zbSeasonAirdropDetailWnd2 = ShowWnd(EWndId.SeasonAirdropDetail, "prefab/season/seasonAirdropDetail.prefab") as zbSeasonAirdropDetailWnd;
		if ((Object)(object)zbSeasonAirdropDetailWnd2 != (Object)null)
		{
			zbSeasonAirdropDetailWnd2.RefreshCustom(data);
		}
	}

	public void ShowSeasonMyTerritory()
	{
		zbSeasonMyTerritoryWnd zbSeasonMyTerritoryWnd2 = ShowWnd(EWndId.SeasonMyTerritory, "prefab/season/seasonMyTerritory.prefab") as zbSeasonMyTerritoryWnd;
		if ((Object)(object)zbSeasonMyTerritoryWnd2 != (Object)null)
		{
			zbSeasonMyTerritoryWnd2.RefreshCustom();
		}
	}

	public void ShowSeasonTarget()
	{
		zbSeasonTargetWnd zbSeasonTargetWnd2 = ShowWnd(EWndId.SeasonTarget, "prefab/season/seasonTarget.prefab") as zbSeasonTargetWnd;
		if ((Object)(object)zbSeasonTargetWnd2 != (Object)null)
		{
			zbSeasonTargetWnd2.RefreshCustom();
		}
	}

	public void ShowSeasonResourceDetail(ResourceData resourceData, zbSeasonResBuild res)
	{
		zbSeasonResourceDetailWnd zbSeasonResourceDetailWnd2 = ShowWnd(EWndId.SeasonResourceDetail, "prefab/season/resourceDetail.prefab") as zbSeasonResourceDetailWnd;
		if ((Object)(object)zbSeasonResourceDetailWnd2 != (Object)null)
		{
			zbSeasonResourceDetailWnd2.RefreshCustom(resourceData, res);
		}
	}

	public void ShowSeasonRank(int index = 0)
	{
		zbSeasonRankWnd zbSeasonRankWnd2 = ShowWnd(EWndId.SeasonTargetRank, "prefab/season/seasonRank.prefab") as zbSeasonRankWnd;
		if ((Object)(object)zbSeasonRankWnd2 != (Object)null)
		{
			zbSeasonRankWnd2.RefreshCustom(index);
		}
	}

	public void ShowSeasonRankReward()
	{
		zbSeasonRankRewardWnd zbSeasonRankRewardWnd2 = ShowWnd(EWndId.SeasonRankReward, "prefab/season/seasonRankReward.prefab") as zbSeasonRankRewardWnd;
		if ((Object)(object)zbSeasonRankRewardWnd2 != (Object)null)
		{
			zbSeasonRankRewardWnd2.RefreshCustom();
		}
	}

	public void ShowSeasonForce(SeasonForceInfo info)
	{
		zbSeasonForceWnd zbSeasonForceWnd2 = ShowWnd(EWndId.SeasonForce, "prefab/season/seasonForce.prefab") as zbSeasonForceWnd;
		if ((Object)(object)zbSeasonForceWnd2 != (Object)null)
		{
			zbSeasonForceWnd2.RefreshCustom(info);
		}
	}

	public void ShowSeasonForceCity()
	{
		zbSeasonForceCityWnd zbSeasonForceCityWnd2 = ShowWnd(EWndId.SeasonForceCity, "prefab/season/seasonForceCity.prefab") as zbSeasonForceCityWnd;
		if ((Object)(object)zbSeasonForceCityWnd2 != (Object)null)
		{
			zbSeasonForceCityWnd2.RefreshCustom();
		}
	}

	public void ShowSeasonForceTech()
	{
		zbSeasonForceTechWnd zbSeasonForceTechWnd2 = ShowWnd(EWndId.SeasonForceTech, "prefab/season/seasonForceTech.prefab") as zbSeasonForceTechWnd;
		if ((Object)(object)zbSeasonForceTechWnd2 != (Object)null)
		{
			zbSeasonForceTechWnd2.RefreshCustom();
		}
	}

	public void ShowSeasonForceShop()
	{
		_ = (Object)(object)(ShowWnd(EWndId.SeasonForceShop, "prefab/season/seasonForce.prefab") as zbSeasonForceShopWnd) != (Object)null;
	}

	public void ShowSeasonForceMembers()
	{
		ShowWnd(EWndId.SeasonForceMembers, "prefab/season/seasonForceMembers.prefab");
	}

	public void ShowSeasonForceMemberManage(SeasonMemberInfo data)
	{
		zbSeasonForceMemberManageWnd zbSeasonForceMemberManageWnd2 = ShowWnd(EWndId.SeasonForceMemberManage, "prefab/season/seasonForceMemberManage.prefab") as zbSeasonForceMemberManageWnd;
		if ((Object)(object)zbSeasonForceMemberManageWnd2 != (Object)null)
		{
			zbSeasonForceMemberManageWnd2.RefreshCustom(data);
		}
	}

	public void ShowSeasonRes(SeasonResNodeInfo info)
	{
		zbSeasonResWnd zbSeasonResWnd2 = ShowWnd(EWndId.SeasonRes, "prefab/season/seasonRes.prefab") as zbSeasonResWnd;
		if ((Object)(object)zbSeasonResWnd2 != (Object)null)
		{
			zbSeasonResWnd2.RefreshCustom(info);
		}
	}

	public void ShowSeasonSearch()
	{
		zbSeasonSearchWnd zbSeasonSearchWnd2 = ShowWnd(EWndId.SeasonSearch, "prefab/season/seasonSearch.prefab") as zbSeasonSearchWnd;
		if ((Object)(object)zbSeasonSearchWnd2 != (Object)null)
		{
			zbSeasonSearchWnd2.RefreshCustom();
		}
	}

	public void ShowSeasonSettlement()
	{
		zbSeasonSettlementWnd zbSeasonSettlementWnd2 = ShowWnd(EWndId.SeasonSettlement, "prefab/season/seasonSettlement.prefab") as zbSeasonSettlementWnd;
		if ((Object)(object)zbSeasonSettlementWnd2 != (Object)null)
		{
			zbSeasonSettlementWnd2.RefreshCustom();
		}
	}

	public void ShowSeasonSettleAwardPreview()
	{
		zbSeasonSettleAwardPreviewWnd zbSeasonSettleAwardPreviewWnd2 = ShowWnd(EWndId.SeasonSettleAwardPreview, "prefab/season/seasonSettleAwardPreview.prefab") as zbSeasonSettleAwardPreviewWnd;
		if ((Object)(object)zbSeasonSettleAwardPreviewWnd2 != (Object)null)
		{
			zbSeasonSettleAwardPreviewWnd2.RefreshCustom();
		}
	}

	public void ShowIdentity()
	{
		ShowWnd(EWndId.Identity, "prefab/wnd/identity/identity.prefab");
	}

	public void ShowSeasonTask()
	{
		zbSeasonTaskWnd zbSeasonTaskWnd2 = ShowWnd(EWndId.SeasonTask, "prefab/season/seasonTask.prefab") as zbSeasonTaskWnd;
		if ((Object)(object)zbSeasonTaskWnd2 != (Object)null)
		{
			zbSeasonTaskWnd2.RefreshCustom();
		}
	}

	public void ShowSeasonRecharge()
	{
		zbSeasonRechargeWnd zbSeasonRechargeWnd2 = ShowWnd(EWndId.SeasonRecharge, "prefab/season/seasonRecharge.prefab") as zbSeasonRechargeWnd;
		if ((Object)(object)zbSeasonRechargeWnd2 != (Object)null)
		{
			zbSeasonRechargeWnd2.RefreshCustom();
		}
	}

	public void ShowSeasonCarHud(zbSeasonCar car)
	{
		zbSeasonCarHudWnd zbSeasonCarHudWnd2 = ShowWnd(EWndId.SeasonCarHud, "prefab/season/seasonCarHud.prefab") as zbSeasonCarHudWnd;
		if ((Object)(object)zbSeasonCarHudWnd2 != (Object)null)
		{
			zbSeasonCarHudWnd2.RefreshCustom(car);
		}
	}

	public void ShowSeasonGuard()
	{
		zbSeasonGuardInfoWnd zbSeasonGuardInfoWnd2 = ShowWnd(EWndId.SeasonGuard, "prefab/season/seasonGuardInfo.prefab") as zbSeasonGuardInfoWnd;
		if ((Object)(object)zbSeasonGuardInfoWnd2 != (Object)null)
		{
			zbSeasonGuardInfoWnd2.RefreshCustom();
		}
	}

	public void Update()
	{
		updateMusic();
		UpdateTouchEffect();
		CheckScreenSizeChange();
	}

	private void LateUpdate()
	{
		if (m_uiUnloadCounter >= 2)
		{
			m_uiUnloadCounter = 0;
			zbAssetLoader.Instance.ReleaseAssets();
		}
	}

	private void updateMusic()
	{
		if (IsShow(EWndId.MainLevel))
		{
			zbMusicStateMachine.Instance.SetState(EMusicState.Main);
		}
		else if (IsShow(EWndId.Combat))
		{
			zbMusicStateMachine.Instance.SetState(EMusicState.Combat);
		}
		else if (IsShow(EWndId.Login))
		{
			zbMusicStateMachine.Instance.SetState(EMusicState.Login);
		}
		else if (IsShow(EWndId.Dungeon))
		{
			zbMusicStateMachine.Instance.SetState(EMusicState.Dungeon);
		}
		else if (IsShow(EWndId.JourneyMovie))
		{
			zbMusicStateMachine.Instance.SetState(EMusicState.JourneyMovie);
		}
		else if (!IsShow(EWndId.Loading))
		{
			zbMusicStateMachine.Instance.SetState(EMusicState.Main);
		}
	}

	private void UpdateTouchEffect()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		m_lastTouchTime += Time.deltaTime;
		Vector2 val;
		if (Input.touchCount > 0)
		{
			Touch touch = Input.GetTouch(0);
			if ((int)((Touch)(ref touch)).phase == 0)
			{
				touch = Input.GetTouch(0);
				val = ((Touch)(ref touch)).position;
				goto IL_0051;
			}
		}
		if (Input.GetMouseButtonDown(0))
		{
			val = Vector2.op_Implicit(Input.mousePosition);
			goto IL_0051;
		}
		return;
		IL_0051:
		val.y = (float)Screen.height - val.y;
		OnShowTouchEffect(val);
	}

	private void OnShowTouchEffect(Vector2 screenPosition)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		if (m_touchCount >= m_effects.Count)
		{
			m_touchCount = 0;
		}
		SpineVisualElement val = m_effects[m_touchCount];
		if (val != null)
		{
			VisualElement rootTop = m_rootTop;
			IPanel panel = rootTop.panel;
			if (panel == null)
			{
				return;
			}
			Vector2 val2 = RuntimePanelUtils.ScreenToPanel(panel, screenPosition);
			Vector2 val3 = VisualElementExtensions.ChangeCoordinatesTo(panel.visualTree, rootTop, val2);
			((VisualElement)val).visible = true;
			((VisualElement)val).style.left = StyleLength.op_Implicit(val3.x);
			((VisualElement)val).style.top = StyleLength.op_Implicit(val3.y);
			val.AnimationState.SetAnimation(0, "animation", false);
		}
		m_touchCount++;
		OnPointerDown();
	}

	public void OnPointerDown()
	{
		m_lastTouchTime = 0f;
		if (zbGameStateMachine.Instance.GetStateId() == EGameState.Main && IsShow(EWndId.GuideFinger))
		{
			Instance.HideWnd(EWndId.GuideFinger);
		}
		if (IsShow(EWndId.Combat))
		{
			zbHeroQiang zbHeroQiang2 = (zbHeroQiang)zbCombatSystem.Instance.combatData.masterTeam.GetHeroBySkillType(ESkillType.Qiang);
			if ((Object)(object)zbHeroQiang2 != (Object)null)
			{
				zbHeroQiang2.SetTargetMonsterId(0);
			}
		}
	}

	public bool IsShow(EWndId id)
	{
		zbBaseWnd wndById = GetWndById(id);
		if ((Object)(object)wndById != (Object)null)
		{
			return wndById.IsShow();
		}
		return false;
	}

	public void DestroyWnd(EWndId id)
	{
		zbBaseWnd wndById = GetWndById(id);
		if (Object.op_Implicit((Object)(object)wndById) && Object.op_Implicit((Object)(object)wndById.GetPrefabObject()))
		{
			Object.Destroy((Object)(object)wndById.GetPrefabObject());
		}
	}

	public void OnNotifyDestroyWnd(EWndId id)
	{
		zbBaseWnd wndById = GetWndById(id);
		if (Object.op_Implicit((Object)(object)wndById))
		{
			wndById.GetRootElement().RemoveFromHierarchy();
			m_kDictWnd.Remove(id);
		}
		m_uiUnloadCounter++;
	}

	private void CheckScreenSizeChange()
	{
		if (zbIntervalTimer.Instance.IsOneSecondPassed() && (Screen.width != m_lastScreenWidht || Screen.height != m_lastScreenHeight))
		{
			m_lastScreenWidht = Screen.width;
			m_lastScreenHeight = Screen.height;
			OnScreenSizeChnaged();
		}
	}

	private void OnScreenSizeChnaged()
	{
		if (m_RootElement != null)
		{
			zbUIScaler.AdapationUI(m_Document);
		}
		foreach (KeyValuePair<EWndId, zbBaseWnd> item in m_kDictWnd)
		{
			item.Value.OnScreenSizeChnaged();
		}
	}

	public bool IsTopCascadWnd(EWndId id)
	{
		zbBaseWnd wndById = GetWndById(id);
		if ((Object)(object)wndById != (Object)null && wndById.IsCascadWnd())
		{
			return IsLastAddedElement(wndById.GetRootElement(), m_rootCascadWnd);
		}
		return false;
	}

	private bool IsLastAddedElement(VisualElement target, VisualElement parent)
	{
		List<VisualElement> list = parent.Children().ToList();
		if (list.Count > 0)
		{
			return list[list.Count - 1] == target;
		}
		return false;
	}
}
