using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbArenaMainWnd : zbBaseWnd
{
	private Transform m_transformTruck;

	private zbPlayerAvatar m_avatarArena;

	private zbPlayerAvatar m_avatarCrossLadder;

	private Label m_lbAvatarArena;

	private Label m_lbAvatarCrossLadder;

	private zbJourneyOneCitizen[] m_citizens = new zbJourneyOneCitizen[4];

	private SpriteRenderer m_spriteFoothold;

	private zbPlayerAvatar m_spineAvatar;

	public zbArenaBottom m_bottomPanel;

	private Button m_btnArena;

	[NonSerialized]
	public Label m_lbArenaName;

	private Button m_btnShop;

	private VisualElement m_spriteShop;

	private Button m_btnCrossLadder;

	private VisualElement m_spriteCrossLadder;

	public Label m_lbCrossLadder;

	private Label m_lbCrossLadderTime;

	private Button m_btnMineWar;

	private VisualElement m_spriteMineWar;

	public Label m_lbMineWarName;

	public Label m_lbMineWarTime;

	private Button m_btnTruck;

	private VisualElement m_spriteTruckBubble;

	private zbTopBar m_topBar;

	private zbTopLeftPlayerAvatar m_playerAvatar;

	private VisualElement m_notifyArena;

	private VisualElement m_notifyCrossLadder;

	private VisualElement m_notifyMineWar;

	private VisualElement m_notifyTruck;

	private bool mineWarCrossRed;

	private bool lastMineWarCrossRed;

	private bool openCrossPeak = true;

	protected override void OnEnable()
	{
		//IL_0382: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0424: Unknown result type (might be due to invalid IL or missing references)
		//IL_043e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0444: Invalid comparison between Unknown and I4
		base.OnEnable();
		m_transformTruck = ((Component)m_GameObjectSceneRoot.transform.Find("truck")).GetComponent<Transform>();
		m_avatarArena = ((Component)m_GameObjectSceneRoot.transform.Find("avatarArena")).GetComponent<zbPlayerAvatar>();
		m_avatarCrossLadder = ((Component)m_GameObjectSceneRoot.transform.Find("avatarCrossLadder")).GetComponent<zbPlayerAvatar>();
		m_lbAvatarArena = UQueryExtensions.Q<Label>(m_RootElement, "lb_avatarArena", (string)null);
		m_lbAvatarCrossLadder = UQueryExtensions.Q<Label>(m_RootElement, "lb_avatarCrossLadder", (string)null);
		for (int i = 0; i < 4; i++)
		{
			m_citizens[i] = ((Component)m_GameObjectSceneRoot.transform.Find("citizen" + i)).GetComponent<zbJourneyOneCitizen>();
		}
		m_spriteFoothold = ((Component)m_GameObjectSceneRoot.transform.Find("bottomBg").Find("foothold")).GetComponent<SpriteRenderer>();
		m_spineAvatar = ((Component)((Component)m_spriteFoothold).transform.Find("avatar")).GetComponent<zbPlayerAvatar>();
		m_bottomPanel = new zbArenaBottom();
		m_bottomPanel = UQueryExtensions.Q<zbArenaBottom>(m_RootElement, "arenaBottom", (string)null);
		m_bottomPanel.OnEnable(this, m_spriteFoothold, m_spineAvatar);
		m_btnArena = UQueryExtensions.Q<Button>(m_RootElement, "btn_arena", (string)null);
		m_lbArenaName = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnArena, "lb_name", (string)null);
		m_btnShop = UQueryExtensions.Q<Button>(m_RootElement, "btn_shop", (string)null);
		m_spriteShop = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btnShop, "sprite", (string)null);
		m_btnCrossLadder = UQueryExtensions.Q<Button>(m_RootElement, "btn_crossLadder", (string)null);
		m_spriteCrossLadder = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btnCrossLadder, "sprite", (string)null);
		m_lbCrossLadder = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnCrossLadder, "lb_name", (string)null);
		m_lbCrossLadderTime = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnCrossLadder, "lb_time", (string)null);
		m_btnMineWar = UQueryExtensions.Q<Button>(m_RootElement, "btn_mineWar", (string)null);
		m_spriteMineWar = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btnMineWar, "sprite", (string)null);
		m_lbMineWarTime = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnMineWar, "lb_time", (string)null);
		m_lbMineWarName = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnMineWar, "lb_name", (string)null);
		m_btnTruck = UQueryExtensions.Q<Button>(m_RootElement, "btn_truck", (string)null);
		m_spriteTruckBubble = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btnTruck, "truck_bubble", (string)null);
		m_topBar = new zbTopBar();
		m_topBar.OnEnable(this);
		m_playerAvatar = new zbTopLeftPlayerAvatar();
		m_playerAvatar.OnEnable(this);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnArena, (Action)OnArena, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnShop, (Action)OnShop, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCrossLadder, (Action)OnCrossLadder, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnMineWar, (Action)OnMineWar, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTruck, (Action)OnTruck, "", (TrickleDown)0);
		m_notifyArena = zbUtil.CreateNotifyTag((VisualElement)(object)m_lbArenaName, (Vector2?)new Vector2(15f, -10f), 1f, true);
		m_notifyCrossLadder = zbUtil.CreateNotifyTag((VisualElement)(object)UQueryExtensions.Q<Label>((VisualElement)(object)m_btnCrossLadder, "lb_name", (string)null), (Vector2?)new Vector2(40f, -10f), 1f, true);
		m_notifyMineWar = zbUtil.CreateNotifyTag((VisualElement)(object)UQueryExtensions.Q<Label>((VisualElement)(object)m_btnMineWar, "lb_name", (string)null), (Vector2?)new Vector2(40f, -10f), 1f, true);
		m_notifyTruck = zbUtil.CreateNotifyTag((VisualElement)(object)UQueryExtensions.Q<Label>((VisualElement)(object)m_btnTruck, "lb_name", (string)null), (Vector2?)new Vector2(40f, -10f), 1f, true);
		if ((int)Application.platform == 8)
		{
			CreateMoneyPanel(new EMoneyType[1] { EMoneyType.ArenaCoin });
		}
		else
		{
			CreateMoneyPanel(new EMoneyType[2]
			{
				EMoneyType.ArenaCoin,
				EMoneyType.Diamond
			});
		}
		CreateMainBottom(5);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_FootholdInfo, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_ArenaRankTop, RefreshTop);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_RoleBaseInfo, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_RoleCommonInfo, OnEventUpdate);
	}

	public void RefreshCustom(bool isFirstIn = false)
	{
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0331: Unknown result type (might be due to invalid IL or missing references)
		//IL_035c: Unknown result type (might be due to invalid IL or missing references)
		m_topBar.Refresh();
		m_playerAvatar.Refresh();
		if (isFirstIn)
		{
			RefreshTop();
			RefreshCitizens();
		}
		zbWsMsgHandler.Instance.Rank_Top_Req();
		m_bottomPanel.Refresh();
		if (zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Shop4))
		{
			m_spriteShop.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/arena/arena_main_lock.png"));
		}
		else
		{
			m_spriteShop.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/arena/arena_main_shop.png"));
		}
		if (zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.CrossLadder))
		{
			m_spriteCrossLadder.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/arena/arena_main_lock.png"));
			((VisualElement)m_lbCrossLadderTime).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((TextElement)m_lbCrossLadder).text = zbUtil.Language("lang.10894");
		}
		else
		{
			if (zbSelf.Instance.crossLadderHandler.IsCrossLadderOpen())
			{
				zbWsMsgHandler.Instance.CrossPeak_Top_Req();
			}
			m_spriteCrossLadder.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/arena/arena_main_crossLadder.png"));
			((VisualElement)m_lbCrossLadderTime).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((TextElement)m_lbCrossLadder).text = zbUtil.Language("lang.10894");
			if (openCrossPeak && zbServerData.sGameData.CrossLadderInfo.AchieveInfo.LadderId >= 26)
			{
				long num = zbGameSave.Instance.GetLong("OpenCrossPeak_" + zbSelf.Instance.GetRoleId(), 0L);
				if (num > 0 && zbUtil.IsCurrentMonth(num))
				{
					((TextElement)m_lbCrossLadder).text = zbUtil.Language("lang.11088");
				}
			}
			RefreshCrossLadderTime();
		}
		if (zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.MineWar))
		{
			m_spriteMineWar.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/arena/arena_main_lock.png"));
			((VisualElement)m_lbMineWarTime).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		else
		{
			m_spriteMineWar.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/arena/arena_main_mineWar.png"));
			RefreshMineWarTime();
		}
		if (zbSelf.Instance.journeyTwoHandler.IsEnd())
		{
			((Component)m_transformTruck).gameObject.SetActive(true);
			((VisualElement)m_btnTruck).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		else
		{
			((Component)m_transformTruck).gameObject.SetActive(false);
			((VisualElement)m_btnTruck).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		m_notifyArena.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.arenaHandler.HasRedPoint()));
		m_notifyCrossLadder.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.crossLadderHandler.HasRedPoint()));
		m_notifyMineWar.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((!mineWarCrossRed && !zbSelf.Instance.mineWarHandler.HasRedPoint()) ? 1 : 0));
		m_notifyTruck.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.truckHandler.HasRedPoint()));
		UpdatePerSecond();
		RefreshBtns();
	}

	private void RefreshBtns()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_btnArena).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Arena) ? 1 : 0));
		((VisualElement)m_btnCrossLadder).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.CrossLadder) ? 1 : 0));
		((VisualElement)m_btnMineWar).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.MineWar) ? 1 : 0));
	}

	private void RefreshTop(object obj = null)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		RankDetail rankDetail = zbSelf.Instance.arenaHandler.rankDetail;
		((Component)m_avatarArena).gameObject.SetActive(rankDetail != null);
		((VisualElement)m_lbAvatarArena).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(rankDetail == null));
		if (rankDetail != null)
		{
			m_avatarArena.RefreshCustom(rankDetail.RoleBaseInfo.AvatarId, rankDetail.RoleBaseInfo.GunId);
			((TextElement)m_lbAvatarArena).text = rankDetail.RoleBaseInfo.RoleName;
			zbSelf.Instance.suitGunHandler.SetSuperSuitEnterAnimation(m_avatarArena);
		}
		RoleBaseInfo peakTop = zbSelf.Instance.crossPeakHandler.peakTop;
		((Component)m_avatarCrossLadder).gameObject.SetActive(peakTop != null);
		((VisualElement)m_lbAvatarCrossLadder).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(peakTop == null));
		if (peakTop != null)
		{
			m_avatarCrossLadder.RefreshCustom(peakTop.AvatarId, peakTop.GunId);
			((TextElement)m_lbAvatarCrossLadder).text = peakTop.RoleName;
			zbSelf.Instance.suitGunHandler.SetSuperSuitEnterAnimation(m_avatarCrossLadder);
		}
	}

	private void RefreshCitizens()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Expected O, but got Unknown
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		Vector3[] array = (Vector3[])(object)new Vector3[4]
		{
			new Vector3(-1f, -1f, 0f),
			new Vector3(-1.3f, -2.3f, 0f),
			new Vector3(-2.2f, -3.8f, 0f),
			new Vector3(-1.5f, -3.9f, 0f)
		};
		Vector3[] array2 = (Vector3[])(object)new Vector3[4]
		{
			new Vector3(-0.3f, 0.2f, 0f),
			new Vector3(-0.8f, -1.6f, 0f),
			new Vector3(-1f, -2.8f, 0f),
			new Vector3(-0.4f, -4.5f, 0f)
		};
		int[] array3 = (from x in ((IEnumerable<JToken>)(JArray)zbDataConfig.Instance.gameBaseJsonTable.GetRecord()["arenaCitizen"]).OrderBy((JToken x) => Rand.value).Take(4)
			select (int)x).ToArray();
		for (int num = 0; num < 4; num++)
		{
			m_citizens[num].Refresh(array3[num], array[num], array2[num]);
		}
	}

	private void OnArena()
	{
		zbLinkHelper.OpenArena();
	}

	private void OnShop()
	{
		zbLinkHelper.OpenShop(3);
	}

	private void OnCrossLadder()
	{
		if (openCrossPeak && zbServerData.sGameData.CrossLadderInfo.AchieveInfo.LadderId >= 26)
		{
			if (zbSelf.Instance.crossLadderHandler.GetNextOpenLeftTime() > 0)
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10943"));
				return;
			}
			long num = zbGameSave.Instance.GetLong("OpenCrossPeak_" + zbSelf.Instance.GetRoleId(), 0L);
			if (num > 0 && zbUtil.IsCurrentMonth(num))
			{
				zbWsMsgHandler.Instance.CrossPeak_Data_Req(openWnd: true);
				return;
			}
		}
		zbLinkHelper.OpenCrossLadder();
	}

	private void OnMineWar()
	{
		zbLinkHelper.OpenMineWar();
		mineWarCrossRed = false;
	}

	private void OnTruck()
	{
		zbLinkHelper.OpenTruck();
	}

	private void RefreshCrossLadderTime()
	{
		if (!zbSelf.Instance.crossLadderHandler.IsCrossLadderOpen() && zbSelf.Instance.timeHandler.GetServerTime() < zbServerData.sGameData?.CrossLadderInfo?.OpenInfo.StartTm)
		{
			((TextElement)m_lbCrossLadderTime).text = zbUtil.Language("lang.10942") + ": " + zbUtil.FormatTimeSmart(zbServerData.sGameData.CrossLadderInfo.OpenInfo.StartTm - zbSelf.Instance.timeHandler.GetServerTime());
		}
		else if (zbSelf.Instance.crossLadderHandler.IsCrossLadderOpen())
		{
			((TextElement)m_lbCrossLadderTime).text = zbUtil.Language("lang.10941") + ": " + zbUtil.FormatTimeSmart(zbServerData.sGameData.CrossLadderInfo.OpenInfo.EndTm - zbSelf.Instance.timeHandler.GetServerTime());
		}
		else if (zbSelf.Instance.crossLadderHandler.GetNextOpenLeftTime() > 0)
		{
			((TextElement)m_lbCrossLadderTime).text = zbUtil.Language("lang.10942") + ": " + zbUtil.FormatTimeSmart(zbSelf.Instance.crossLadderHandler.GetNextOpenLeftTime());
		}
		else
		{
			zbWsMsgHandler.Instance.CrossLadder_Data_Req();
		}
	}

	private void RefreshMineWarTime()
	{
		if (!zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.MineWar) && zbServerData.sGameData.MineWarBaseInfo != null)
		{
			if (zbServerData.sGameData.MineWarBaseInfo.MineWarOpenInfo.NextCrossStart < zbSelf.Instance.timeHandler.GetServerTime() && zbServerData.sGameData.MineWarBaseInfo.MineWarOpenInfo.NextCrossEnd > zbSelf.Instance.timeHandler.GetServerTime())
			{
				zbUtil.SetDisplayIfChanged((VisualElement)(object)m_lbMineWarTime, (DisplayStyle)0);
				((TextElement)m_lbMineWarTime).text = zbUtil.Language("lang.10997") + ": " + zbUtil.FormatTimeSmart(zbServerData.sGameData.MineWarBaseInfo.MineWarOpenInfo.NextCrossEnd - zbSelf.Instance.timeHandler.GetServerTime());
				if (!lastMineWarCrossRed)
				{
					mineWarCrossRed = true;
					lastMineWarCrossRed = true;
				}
			}
			else
			{
				zbUtil.SetDisplayIfChanged((VisualElement)(object)m_lbMineWarTime, (DisplayStyle)1);
			}
		}
		else
		{
			zbUtil.SetDisplayIfChanged((VisualElement)(object)m_lbMineWarTime, (DisplayStyle)1);
		}
	}

	private void RefreshTruckTime()
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		if (!zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Truck) && zbServerData.sGameData.TruckSelfInfo != null)
		{
			if (zbSelf.Instance.truckHandler.CanTruckGetAward())
			{
				zbUtil.SetDisplayIfChanged(m_spriteTruckBubble, (DisplayStyle)0);
				Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/truck/truck_bubble.png");
				m_spriteTruckBubble.style.backgroundImage = new StyleBackground(val);
			}
			else if (zbSelf.Instance.truckHandler.HaveTruckTransport())
			{
				zbUtil.SetDisplayIfChanged(m_spriteTruckBubble, (DisplayStyle)0);
				Sprite val2 = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/truck/truck_bubbleempty.png");
				m_spriteTruckBubble.style.backgroundImage = new StyleBackground(val2);
			}
			else if (zbSelf.Instance.truckHandler.HaveTruckRuning())
			{
				zbUtil.SetDisplayIfChanged(m_spriteTruckBubble, (DisplayStyle)0);
				Sprite val3 = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/truck/truck_bubbletime.png");
				m_spriteTruckBubble.style.backgroundImage = new StyleBackground(val3);
			}
			else
			{
				zbUtil.SetDisplayIfChanged(m_spriteTruckBubble, (DisplayStyle)1);
			}
		}
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		RefreshCrossLadderTime();
		RefreshMineWarTime();
		RefreshTruckTime();
	}

	protected override void UpdateAtShow()
	{
		base.UpdateAtShow();
		m_topBar.UpdateRedPoint();
		m_playerAvatar.UpdateRedPoint();
	}

	public void OnShowEditTeamGuide()
	{
		zbUIMgr.Instance.ShowGuideFinger((VisualElement)(object)m_bottomPanel.m_btnEdit);
	}

	protected override void RefreshWhenAutoVisible()
	{
		base.RefreshWhenAutoVisible();
		RefreshCustom();
	}

	protected override void RefreshWhenAutoVisibleCaseNoScene()
	{
		base.RefreshWhenAutoVisibleCaseNoScene();
		RefreshCustom();
	}

	private void OnEventUpdate(object data)
	{
		RefreshCustom();
	}
}
