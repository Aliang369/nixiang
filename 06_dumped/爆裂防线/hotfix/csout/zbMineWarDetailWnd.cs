using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbMineWarDetailWnd : zbBaseWnd
{
	private Button m_btnTouch;

	public Button m_btnClose;

	[NonSerialized]
	private zbPlayerAvatarTarget renderTarget;

	private VisualElement foldIcon;

	private Label m_lbPlayerName;

	private Button m_btnPlayer;

	private VisualElement nodePlayer;

	private VisualElement nodedesc;

	private Label m_lbName;

	private Label m_lbDesc;

	private Label m_lbLeftTime;

	private Label m_lbLayBg;

	private Image m_descImg;

	private VisualElement[] m_nodeDetail = (VisualElement[])(object)new VisualElement[5];

	private TemplateContainer[] tb_skillMod = (TemplateContainer[])(object)new TemplateContainer[5];

	private zbPieProgress[] m_skillOutProgress = new zbPieProgress[5];

	private zbPieProgress[] m_skillInProgress = new zbPieProgress[5];

	private VisualElement[] m_skillIcon = (VisualElement[])(object)new VisualElement[5];

	private Label[] m_lbSkillLevel = (Label[])(object)new Label[5];

	private Button[] m_btnSkill = (Button[])(object)new Button[5];

	private List<SkillDetail> heroList = new List<SkillDetail>();

	private Button m_btnGetAward;

	private Button m_btnZhanling;

	private Button m_btnCuihui;

	private Button m_btnWork;

	private Button m_btnAdd;

	private VisualElement m_Tequan;

	private Button m_btnTequan;

	private Label m_lbSubTitle_2;

	private Label m_lbSubValue_1;

	private Label m_lbSubValue_2;

	private Button m_btnMoney_1;

	private Button m_btnMoney_2;

	private Label m_lbCostNum;

	private VisualElement iconWarning;

	private Label m_lbWarning;

	private MineWarPointDataAck mineWarPointDataAck;

	private CrossMinePointDataAck crossMinePointDataAck;

	private MineWarPointDetail mineWarPointDetail;

	private MineWarPointIdleInfo mineWarPointIdleInfo;

	private RoleBaseInfo roleBaseInfo;

	private zbMineNodeCsvRecord mineNodeCsvRecord;

	private int serverId;

	protected override void OnEnable()
	{
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Expected O, but got Unknown
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		foldIcon = UQueryExtensions.Q<VisualElement>(m_RootElement, "foldIcon", (string)null);
		m_lbPlayerName = UQueryExtensions.Q<Label>(m_RootElement, "lb_playername", (string)null);
		m_btnPlayer = UQueryExtensions.Q<Button>(m_RootElement, "btn_player", (string)null);
		nodePlayer = UQueryExtensions.Q<VisualElement>(m_RootElement, "nodePlayer", (string)null);
		nodedesc = UQueryExtensions.Q<VisualElement>(m_RootElement, "nodedesc", (string)null);
		renderTarget = ((Component)GameObject.Find("Renders").transform.Find("playerAvatarTarget")).GetComponent<zbPlayerAvatarTarget>();
		if (m_descImg == null)
		{
			m_descImg = new Image();
			((VisualElement)m_descImg).style.left = StyleLength.op_Implicit(0f);
			((VisualElement)m_descImg).style.top = StyleLength.op_Implicit(0f);
			((VisualElement)m_descImg).style.width = StyleLength.op_Implicit(240f);
			((VisualElement)m_descImg).style.height = StyleLength.op_Implicit(240f);
			((VisualElement)m_descImg).style.position = StyleEnum<Position>.op_Implicit((Position)1);
		}
		foldIcon.Add((VisualElement)(object)m_descImg);
		for (int i = 0; i < 5; i++)
		{
			tb_skillMod[i] = UQueryExtensions.Q<TemplateContainer>(m_RootElement, "skill" + (i + 1), (string)null);
			m_nodeDetail[i] = UQueryExtensions.Q<VisualElement>((VisualElement)(object)tb_skillMod[i], "nodeDetail", (string)null);
			m_skillOutProgress[i] = UQueryExtensions.Q<zbPieProgress>((VisualElement)(object)tb_skillMod[i], "outProgress", (string)null);
			m_skillOutProgress[i].progress = 100f;
			m_skillInProgress[i] = UQueryExtensions.Q<zbPieProgress>((VisualElement)(object)tb_skillMod[i], "inProgress", (string)null);
			m_skillInProgress[i].progress = 100f;
			m_skillIcon[i] = UQueryExtensions.Q<VisualElement>((VisualElement)(object)tb_skillMod[i], "skillIcon", (string)null);
			m_lbSkillLevel[i] = UQueryExtensions.Q<Label>((VisualElement)(object)tb_skillMod[i], "lb_skill", (string)null);
			m_btnSkill[i] = UQueryExtensions.Q<Button>((VisualElement)(object)tb_skillMod[i], "btn_skill", (string)null);
			int curIndex = i + 1;
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSkill[i], (Action<ClickEvent>)delegate(ClickEvent evt)
			{
				OnSkillClick(evt, curIndex);
			}, "", (TrickleDown)0);
		}
		m_lbSubTitle_2 = UQueryExtensions.Q<Label>(UQueryExtensions.Q<VisualElement>(m_RootElement, "node1", (string)null), "lb_key", (string)null);
		m_lbSubValue_1 = UQueryExtensions.Q<Label>(UQueryExtensions.Q<VisualElement>(m_RootElement, "node0", (string)null), "lb_value", (string)null);
		m_Tequan = UQueryExtensions.Q<VisualElement>(UQueryExtensions.Q<VisualElement>(m_RootElement, "node0", (string)null), "tequan_temp", (string)null);
		m_btnTequan = UQueryExtensions.Q<Button>(UQueryExtensions.Q<VisualElement>(m_RootElement, "node0", (string)null), "btn_tequam", (string)null);
		m_btnMoney_1 = UQueryExtensions.Q<Button>(UQueryExtensions.Q<VisualElement>(m_RootElement, "node0", (string)null), "btn_money", (string)null);
		m_lbSubValue_2 = UQueryExtensions.Q<Label>(UQueryExtensions.Q<VisualElement>(m_RootElement, "node1", (string)null), "lb_value", (string)null);
		m_btnMoney_2 = UQueryExtensions.Q<Button>(UQueryExtensions.Q<VisualElement>(m_RootElement, "node1", (string)null), "btn_money", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnMoney_1, (Action<ClickEvent>)OnIconMoney, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnMoney_2, (Action<ClickEvent>)OnIconMoney, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTequan, (Action<ClickEvent>)OnIconTequan, "", (TrickleDown)0);
		m_lbLayBg = UQueryExtensions.Q<Label>(m_RootElement, "lb_roomDesc", (string)null);
		m_lbLeftTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_leftTime", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_Desc", (string)null);
		m_btnGetAward = UQueryExtensions.Q<Button>(m_RootElement, "btn_get", (string)null);
		m_btnZhanling = UQueryExtensions.Q<Button>(m_RootElement, "btn_zhanling", (string)null);
		m_btnCuihui = UQueryExtensions.Q<Button>(m_RootElement, "btn_cuihui", (string)null);
		m_btnWork = UQueryExtensions.Q<Button>(m_RootElement, "btn_work", (string)null);
		m_lbCostNum = UQueryExtensions.Q<Label>(m_RootElement, "icon_engry", (string)null);
		m_btnAdd = UQueryExtensions.Q<Button>(m_RootElement, "btn_add", (string)null);
		iconWarning = UQueryExtensions.Q<VisualElement>(m_RootElement, "icon_warning", (string)null);
		m_lbWarning = UQueryExtensions.Q<Label>(m_RootElement, "lb_Warning", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGetAward, (Action<ClickEvent>)OnGetAward, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnZhanling, (Action<ClickEvent>)OnZhanling, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCuihui, (Action<ClickEvent>)OnCuihui, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnWork, (Action<ClickEvent>)OnWork, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnPlayer, (Action<ClickEvent>)OnPlayer, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAdd, (Action<ClickEvent>)OnBuy, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI(m_RootElement);
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnClickClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClickClose, "", (TrickleDown)0);
		AddDark();
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_MineWarBaseInfo, RefreshFightState);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_PvpTipsInfo, delegate
		{
			RefreshPvpTipsInfo();
		});
	}

	public void RefreshCustom(MineWarPointDataAck _mineWarPointDataAck, CrossMinePointDataAck _crossMinePointDataAck)
	{
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0325: Unknown result type (might be due to invalid IL or missing references)
		//IL_033b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		mineWarPointDataAck = _mineWarPointDataAck;
		crossMinePointDataAck = _crossMinePointDataAck;
		serverId = 0;
		if (mineWarPointDataAck != null)
		{
			mineWarPointDetail = mineWarPointDataAck.PointInfo;
			roleBaseInfo = mineWarPointDataAck.RoleBaseInfo;
			mineWarPointIdleInfo = mineWarPointDataAck.PointIdleInfo;
		}
		else
		{
			serverId = crossMinePointDataAck.ServerId;
			mineWarPointDetail = crossMinePointDataAck.PointInfo;
			roleBaseInfo = crossMinePointDataAck.RoleBaseInfo;
		}
		mineNodeCsvRecord = zbDataConfig.Instance.mineNodeCsvTable.GetRecordByKey(mineWarPointDetail.PointId);
		renderTarget.StartRender(m_descImg);
		RefreshSubNod();
		UpdatePerSecond();
		((TextElement)m_lbName).text = mineNodeCsvRecord.name;
		int layerByPoint = zbSelf.Instance.mineWarHandler.GetLayerByPoint(mineWarPointDetail.PointId);
		((VisualElement)m_lbLayBg).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/mineWar/laybg_" + layerByPoint + ".jpg"));
		((VisualElement)m_btnZhanling).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		((VisualElement)m_btnCuihui).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		((VisualElement)m_btnWork).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		RefreshFightState();
		if (mineWarPointDetail.PlayerId != 0)
		{
			renderTarget.RefreshAvatar(roleBaseInfo.AvatarId, roleBaseInfo.GunId);
			nodePlayer.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			nodedesc.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((TextElement)m_lbPlayerName).text = roleBaseInfo.RoleName;
			zbWsMsgHandler.Instance.Role_PvpTipsInfo_Req((int)roleBaseInfo.RoleId);
			RefreshPvpTipsInfo();
			if (mineWarPointDetail.PlayerId == zbSelf.Instance.GetRoleId())
			{
				((VisualElement)m_btnWork).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				return;
			}
			if (serverId > 0)
			{
				((VisualElement)m_btnCuihui).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			}
			else
			{
				((VisualElement)m_btnZhanling).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				((VisualElement)m_btnWork).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			}
			zbSelf.Instance.guardHandler.pvpTipsInfo = null;
		}
		else
		{
			nodePlayer.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			nodedesc.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			if (serverId > 0)
			{
				((TextElement)m_lbDesc).text = zbUtil.Language("lang.10960");
				((VisualElement)m_btnCuihui).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				((VisualElement)m_btnWork).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
			else
			{
				((TextElement)m_lbDesc).text = zbUtil.Language("lang.10961");
				((VisualElement)m_btnZhanling).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				((VisualElement)m_btnWork).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			}
		}
	}

	private void RefreshFightState(object data = null)
	{
		//IL_0294: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0382: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		int num = 1;
		int layerByPoint = zbSelf.Instance.mineWarHandler.GetLayerByPoint(mineWarPointDetail.PointId);
		if (zbSelf.Instance.mineWarHandler.currServerId == zbSelf.Instance.GetServerId())
		{
			num = 1;
			if (mineWarPointDetail.PlayerId == 0)
			{
				((VisualElement)m_btnZhanling).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_250_80_red.png"));
				if (layerByPoint >= zbSelf.Instance.mineWarHandler.selfLayerNum - 1)
				{
					((VisualElement)m_btnWork).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_250_80_orange.png"));
				}
				else
				{
					((VisualElement)m_btnWork).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_250_80_gray.png"));
				}
			}
			else if (mineWarPointDetail.PlayerId != zbSelf.Instance.GetRoleId())
			{
				if (layerByPoint >= zbSelf.Instance.mineWarHandler.selfLayerNum - 1)
				{
					((VisualElement)m_btnWork).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_250_80_orange.png"));
					if (mineWarPointDetail.DestroyTm > zbSelf.Instance.timeHandler.GetServerTime() || mineWarPointDetail.ProtectTm > zbSelf.Instance.timeHandler.GetServerTime() || mineWarPointDetail.LockedTm > zbSelf.Instance.timeHandler.GetServerTime())
					{
						((VisualElement)m_btnZhanling).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_250_80_gray.png"));
					}
					else
					{
						((VisualElement)m_btnZhanling).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_250_80_red.png"));
					}
				}
				else
				{
					((VisualElement)m_btnZhanling).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_250_80_gray.png"));
					((VisualElement)m_btnWork).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_250_80_gray.png"));
				}
			}
			else
			{
				((VisualElement)m_btnWork).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_250_80_orange.png"));
			}
		}
		else
		{
			num = zbSelf.Instance.mineWarHandler.GetCostNum(zbSelf.Instance.mineWarHandler.currServerId);
			if (layerByPoint != zbSelf.Instance.mineWarHandler.canCuihuiLayer)
			{
				((VisualElement)m_btnCuihui).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_250_80_gray.png"));
			}
			else if (mineWarPointDetail.DestroyTm > zbSelf.Instance.timeHandler.GetServerTime())
			{
				((VisualElement)m_btnCuihui).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_250_80_gray.png"));
			}
			else
			{
				((VisualElement)m_btnCuihui).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_250_80_red.png"));
			}
		}
		((TextElement)m_lbCostNum).enableRichText = true;
		((TextElement)m_lbCostNum).text = zbUtil.GetNeedHaveRichString(zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarSelfInfo.CombatNum, num);
		if (mineWarPointDetail.ProtectTm > zbSelf.Instance.timeHandler.GetServerTime() || mineWarPointDetail.LockedTm > zbSelf.Instance.timeHandler.GetServerTime())
		{
			iconWarning.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			if (mineWarPointDetail.ProtectTm > zbSelf.Instance.timeHandler.GetServerTime())
			{
				((TextElement)m_lbWarning).text = zbUtil.Language("lang.11027");
			}
			else if (mineWarPointDetail.LockedTm > zbSelf.Instance.timeHandler.GetServerTime())
			{
				((TextElement)m_lbWarning).text = zbUtil.Language("lang.11026");
			}
		}
		else
		{
			iconWarning.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
	}

	private void RefreshSubNod()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_btnGetAward).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		int num = mineNodeCsvRecord.resOut[2];
		if (mineWarPointDetail.PlayerId == zbSelf.Instance.GetRoleId())
		{
			int mineWarCollect = zbSelf.Instance.mineWarHandler.GetMineWarCollect();
			((TextElement)m_lbSubValue_1).text = ((float)num * (1f + (float)mineWarCollect / 100f)).ToString();
			if (zbServerData.sGameData.RechargeActiveInfo.TreasurePrivilegeInfo.Active == EBoolState.True)
			{
				m_Tequan.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				((VisualElement)m_lbSubValue_1).style.color = StyleColor.op_Implicit(new Color(0f, 0.4392157f, 0.03137255f, 1f));
			}
			else
			{
				m_Tequan.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				((VisualElement)m_lbSubValue_1).style.color = StyleColor.op_Implicit(new Color(0.7372549f, 20f / 51f, 0.08627451f, 1f));
			}
			((VisualElement)m_btnGetAward).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((TextElement)m_lbSubTitle_2).text = zbUtil.Language("lang.10282");
			int num2 = Extensions.Value<int>((IEnumerable<JToken>)zbDataConfig.Instance.mineBaseJsonTable.GetRecord()["resCapacity"]);
			if ((float)Math.Min(zbSelf.Instance.timeHandler.GetServerTime() - mineWarPointDetail.StartTm, mineWarPointDetail.FullTm) > 300f)
			{
				((VisualElement)m_btnGetAward).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_250_80_green.png"));
				((TextElement)m_lbSubValue_2).text = mineWarPointIdleInfo.SettleAward.Count + "/" + num * num2;
			}
			else
			{
				((VisualElement)m_btnGetAward).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_250_80_gray.png"));
				((TextElement)m_lbSubValue_2).text = "0/" + num * num2;
			}
		}
		else
		{
			m_Tequan.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((TextElement)m_lbSubValue_1).text = num.ToString();
			((VisualElement)m_lbSubValue_1).style.color = StyleColor.op_Implicit(new Color(0.7372549f, 20f / 51f, 0.08627451f, 1f));
			if (serverId > 0)
			{
				((TextElement)m_lbSubTitle_2).text = zbUtil.Language("lang.10971");
				((TextElement)m_lbSubValue_2).text = mineNodeCsvRecord.crossAwardFight[2].ToString();
			}
			else
			{
				((TextElement)m_lbSubTitle_2).text = zbUtil.Language("lang.10972");
				((TextElement)m_lbSubValue_2).text = mineNodeCsvRecord.awardFight[2].ToString();
			}
		}
	}

	private bool GetFightState()
	{
		if (zbSelf.Instance.timeHandler.GetServerTime() < zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarSelfInfo.NextCombatTm)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10983") + ":" + zbUtil.FormatTimeMMSS(zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarSelfInfo.NextCombatTm - zbSelf.Instance.timeHandler.GetServerTime()));
			return true;
		}
		int layerByPoint = zbSelf.Instance.mineWarHandler.GetLayerByPoint(mineWarPointDetail.PointId);
		if (zbSelf.Instance.mineWarHandler.currServerId == zbSelf.Instance.GetServerId())
		{
			if (zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarOpenInfo.CrossOpenTm <= zbSelf.Instance.timeHandler.GetServerTime() && zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarOpenInfo.NextCrossStart < zbSelf.Instance.timeHandler.GetServerTime() && zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarOpenInfo.NextCrossEnd > zbSelf.Instance.timeHandler.GetServerTime())
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.EErrorCode.32023"));
				return true;
			}
			if (mineWarPointDetail.PlayerId == 0)
			{
				return false;
			}
			if (mineWarPointDetail.PlayerId != zbSelf.Instance.GetRoleId())
			{
				if (layerByPoint < zbSelf.Instance.mineWarHandler.selfLayerNum - 1)
				{
					zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10969"));
					return true;
				}
				if (mineWarPointDetail.DestroyTm > zbSelf.Instance.timeHandler.GetServerTime())
				{
					zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10963"));
					return true;
				}
				if (mineWarPointDetail.ProtectTm > zbSelf.Instance.timeHandler.GetServerTime())
				{
					zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10966"));
					return true;
				}
				if (mineWarPointDetail.LockedTm > zbSelf.Instance.timeHandler.GetServerTime())
				{
					zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10964"));
					return true;
				}
			}
		}
		else
		{
			if (zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarOpenInfo.NextCrossStart >= zbSelf.Instance.timeHandler.GetServerTime() || zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarOpenInfo.NextCrossEnd <= zbSelf.Instance.timeHandler.GetServerTime())
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10990"));
				return true;
			}
			if (zbSelf.Instance.mineWarHandler.selfLayerNum > 4)
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.EErrorCode.32025"));
				return true;
			}
			if (layerByPoint != zbSelf.Instance.mineWarHandler.canCuihuiLayer)
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10968"));
				return true;
			}
			if (mineWarPointDetail.DestroyTm > zbSelf.Instance.timeHandler.GetServerTime())
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10963"));
				return true;
			}
			if (mineWarPointDetail.ProtectTm > zbSelf.Instance.timeHandler.GetServerTime())
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10966"));
				return true;
			}
			if (mineWarPointDetail.LockedTm > zbSelf.Instance.timeHandler.GetServerTime())
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10964"));
				return true;
			}
		}
		return false;
	}

	protected override void UpdatePerSecond()
	{
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		base.UpdatePerSecond();
		if (mineWarPointDetail.PlayerId != zbSelf.Instance.GetRoleId() && zbSelf.Instance.timeHandler.GetServerTime() < zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarSelfInfo.NextCombatTm)
		{
			if (((VisualElement)m_lbLeftTime).style.display != StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0))
			{
				((VisualElement)m_lbLeftTime).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			}
			((TextElement)m_lbLeftTime).text = zbUtil.Language("lang.10983") + ":" + zbUtil.FormatTimeMMSS(zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarSelfInfo.NextCombatTm - zbSelf.Instance.timeHandler.GetServerTime());
		}
		else if (((VisualElement)m_lbLeftTime).style.display != StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1))
		{
			((VisualElement)m_lbLeftTime).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
	}

	private void OnGetAward(ClickEvent evt)
	{
		if ((float)Math.Min(zbSelf.Instance.timeHandler.GetServerTime() - mineWarPointDetail.StartTm, mineWarPointDetail.FullTm) < 300f)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10391"));
			return;
		}
		zbWsMsgHandler.Instance.MineWar_IdleAward_Req(mineWarPointDetail.PointId);
		Hide();
	}

	private void OnBuy(ClickEvent evt = null)
	{
		if (zbSelf.Instance.mineWarHandler.GetBuyLimit() > zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarSelfInfo.BuyCombat)
		{
			zbUIMgr.Instance.ShowBuyCount(EBattleFuncType.MineWar);
		}
		else if (zbServerData.sGameData.RechargeActiveInfo.TreasurePrivilegeInfo.Active == EBoolState.False)
		{
			zbUIMgr.Instance.ShowTreasurePrivilege();
		}
		else
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10700"));
		}
	}

	private void OnZhanlingEmpty(bool isConfirm)
	{
		if (isConfirm)
		{
			zbWsMsgHandler.Instance.MineWar_EmptyPoint_Req(mineWarPointDetail.PointId);
		}
	}

	private void OnZhanling(ClickEvent evt)
	{
		if (GetFightState())
		{
			return;
		}
		if (zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarSelfInfo.CombatNum < 1)
		{
			OnBuy();
			return;
		}
		if (mineWarPointDetail.PlayerId == 0)
		{
			if (zbSelf.Instance.mineWarHandler.GetLayerByPoint(mineWarPointDetail.PointId) > zbSelf.Instance.mineWarHandler.selfLayerNum)
			{
				zbUIMgr.Instance.ShowMessageBox(zbUtil.Language("lang.10143"), zbUtil.Language("lang.11000"), OnZhanlingEmpty);
			}
			else
			{
				OnZhanlingEmpty(isConfirm: true);
			}
		}
		else if (zbSelf.Instance.mineWarHandler.currServerId == zbSelf.Instance.GetServerId())
		{
			zbWsMsgHandler.Instance.MineWar_PointData_Req(mineWarPointDetail.PointId, openEnemyWnd: true);
		}
		else
		{
			zbWsMsgHandler.Instance.CrossMine_PointData_Req(mineWarPointDetail.PointId, zbSelf.Instance.mineWarHandler.currServerId, openEnemyWnd: true);
		}
		Hide();
	}

	private void OnCuihui(ClickEvent evt)
	{
		if (GetFightState())
		{
			return;
		}
		if (mineWarPointDetail.PlayerId != 0)
		{
			if (serverId > 0)
			{
				int costNum = zbSelf.Instance.mineWarHandler.GetCostNum(zbSelf.Instance.mineWarHandler.currServerId);
				if (zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarSelfInfo.CombatNum < costNum)
				{
					OnBuy();
					return;
				}
				if (zbSelf.Instance.mineWarHandler.currServerId == zbSelf.Instance.GetServerId())
				{
					zbWsMsgHandler.Instance.MineWar_PointData_Req(mineWarPointDetail.PointId, openEnemyWnd: true);
				}
				else
				{
					zbWsMsgHandler.Instance.CrossMine_PointData_Req(mineWarPointDetail.PointId, zbSelf.Instance.mineWarHandler.currServerId, openEnemyWnd: true);
				}
			}
		}
		else if (serverId > 0)
		{
			int costNum2 = zbSelf.Instance.mineWarHandler.GetCostNum(zbSelf.Instance.mineWarHandler.currServerId);
			if (zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarSelfInfo.CombatNum < costNum2)
			{
				OnBuy();
				return;
			}
			zbWsMsgHandler.Instance.CrossMine_DestroyEmptyPoint_Req(mineWarPointDetail.PointId, serverId);
		}
		else
		{
			zbWsMsgHandler.Instance.MineWar_EmptyPoint_Req(mineWarPointDetail.PointId);
		}
		Hide();
	}

	private void OnWork(ClickEvent evt)
	{
		int layerByPoint = zbSelf.Instance.mineWarHandler.GetLayerByPoint(mineWarPointDetail.PointId);
		if (zbSelf.Instance.mineWarHandler.currServerId != zbSelf.Instance.GetServerId())
		{
			return;
		}
		if (zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarOpenInfo.CrossOpenTm <= zbSelf.Instance.timeHandler.GetServerTime() && zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarOpenInfo.NextCrossStart < zbSelf.Instance.timeHandler.GetServerTime() && zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarOpenInfo.NextCrossEnd > zbSelf.Instance.timeHandler.GetServerTime())
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.EErrorCode.32023"));
			return;
		}
		if (layerByPoint < zbSelf.Instance.mineWarHandler.selfLayerNum - 1)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10969"));
			return;
		}
		int num = 1;
		if (zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarSelfInfo.CombatNum < num)
		{
			OnBuy();
			return;
		}
		int num2 = mineNodeCsvRecord.saoDangAward[2];
		string title = zbUtil.Language("lang.10143");
		string message = string.Format(zbUtil.Language("lang.10989"), num, num2);
		zbUIMgr.Instance.ShowMessageBox(title, message, OnClickDelRoomConfirm);
	}

	private void OnClickDelRoomConfirm(bool isConfirm)
	{
		if (isConfirm)
		{
			zbWsMsgHandler.Instance.MineWar_SaoDang_Req(mineWarPointDetail.PointId);
		}
	}

	private void OnPlayer(ClickEvent evt)
	{
		if (mineWarPointDetail.PlayerId != zbSelf.Instance.GetRoleId())
		{
			zbWsMsgHandler.Instance.Role_TipsInfo_Req((int)roleBaseInfo.RoleId);
		}
	}

	private void OnIconMoney(ClickEvent evt)
	{
		zbUIMgr.Instance.ShowTip((EResourceType)mineNodeCsvRecord.resOut[0], mineNodeCsvRecord.resOut[1], 0L);
	}

	private void OnIconTequan(ClickEvent evt)
	{
		string msg = string.Format(zbUtil.Language("lang.11033"), 50 + "%");
		zbUIMgr.Instance.BubbleInfo(msg);
	}

	private void OnClickClose(ClickEvent evt)
	{
		renderTarget.StopRender();
		Hide();
	}

	public void RefreshPvpTipsInfo()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0306: Unknown result type (might be due to invalid IL or missing references)
		//IL_0332: Unknown result type (might be due to invalid IL or missing references)
		//IL_0349: Unknown result type (might be due to invalid IL or missing references)
		//IL_0353: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 5; i++)
		{
			m_nodeDetail[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		heroList.Clear();
		HashSet<int> hashSet = new HashSet<int>();
		if (zbSelf.Instance.guardHandler.pvpTipsInfo == null || zbSelf.Instance.guardHandler.pvpTipsInfo.PvpModInfo == null)
		{
			return;
		}
		List<zbModCsvRecord> source = (from r in (IEnumerable<int>)zbSelf.Instance.guardHandler.pvpTipsInfo.PvpModInfo.SelectList
			select zbDataConfig.Instance.modCsvTable.GetRecordByKey(r) into m
			where m != null
			select m).ToList();
		foreach (int mod in zbSelf.Instance.guardHandler.pvpTipsInfo.PvpModInfo.ModList)
		{
			foreach (SkillDetail skill in zbSelf.Instance.guardHandler.pvpTipsInfo.PvpModInfo.SkillList)
			{
				if (skill.SkillType + 20010 == (ESkillType)mod && hashSet.Add((int)skill.SkillType))
				{
					SkillDetail skillDetail = new SkillDetail();
					skillDetail.HeroTid = skill.HeroTid;
					skillDetail.SkillType = skill.SkillType;
					skillDetail.SkillId = skill.SkillId;
					heroList.Add(skillDetail);
				}
			}
		}
		foreach (int select in zbSelf.Instance.guardHandler.pvpTipsInfo.PvpModInfo.SelectList)
		{
			foreach (SkillDetail skill2 in zbSelf.Instance.guardHandler.pvpTipsInfo.PvpModInfo.SkillList)
			{
				if (skill2.SkillType + 20010 == (ESkillType)select && hashSet.Add((int)skill2.SkillType))
				{
					SkillDetail skillDetail2 = new SkillDetail();
					skillDetail2.HeroTid = skill2.HeroTid;
					skillDetail2.SkillType = skill2.SkillType;
					skillDetail2.SkillId = skill2.SkillId;
					heroList.Add(skillDetail2);
				}
			}
		}
		int num = 0;
		foreach (SkillDetail hero in heroList)
		{
			if (num >= 5)
			{
				break;
			}
			zbSkillCsvRecord recordByKey = zbDataConfig.Instance.skillCsvTable.GetRecordByKey(hero.SkillId);
			if (recordByKey != null)
			{
				((VisualElement)tb_skillMod[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_nodeDetail[num].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>(recordByKey.icon);
				m_skillIcon[num].style.backgroundImage = StyleBackground.op_Implicit(val);
				m_skillIcon[num].transform.scale = Vector3.one * 0.45f;
				ESkillType type = hero.SkillType;
				int num2 = source.Count((zbModCsvRecord mod) => mod.skillLvUp == 1 && Enumerable.Contains(mod.skillType, (int)type));
				((TextElement)m_lbSkillLevel[num]).text = num2.ToString();
				num++;
			}
		}
	}

	public void OnSkillClick(ClickEvent evt, int skillIndex)
	{
		if (zbSelf.Instance.guardHandler.pvpTipsInfo == null || zbSelf.Instance.guardHandler.pvpTipsInfo.PvpModInfo == null)
		{
			return;
		}
		List<int> list = new List<int>();
		foreach (int mod in zbSelf.Instance.guardHandler.pvpTipsInfo.PvpModInfo.ModList)
		{
			list.Add(mod);
		}
		foreach (int select in zbSelf.Instance.guardHandler.pvpTipsInfo.PvpModInfo.SelectList)
		{
			list.Add(select);
		}
		zbUIMgr.Instance.ShowSkillFightModWnd(0, heroList.ToArray(), list, isResumeFight: false);
	}
}
