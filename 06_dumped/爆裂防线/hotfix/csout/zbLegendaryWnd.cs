using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbLegendaryWnd : zbBaseWnd
{
	private zbPlayerAvatar m_spineAvatar0;

	private zbPlayerAvatar m_spineAvatar1;

	private VisualElement m_nodeTop;

	private Label m_lbLevel;

	private VisualElement m_doubleAward;

	private zbItem[] m_items = new zbItem[5];

	private VisualElement m_bgDesc;

	private Label m_lbDesc;

	private Button m_btnFreeGift;

	private VisualElement m_spriteFreeGift;

	private Label m_lbFreeGift;

	private VisualElement ve_joinplay;

	public Button m_btnJoinPlayer;

	private Button m_btnJoinClose;

	private Label m_lbSelfName;

	private Label m_lbPlayerName;

	public VisualElement ve_joinplayGuide;

	private Label[] m_lbCosts = (Label[])(object)new Label[2];

	private Button m_btnFight;

	public Button m_btnZhanLing;

	private Button m_btnWeekCard;

	private Button m_btnClose;

	private VisualElement m_notifyFreeGift;

	private VisualElement m_notifyFight;

	private VisualElement m_notifyZhanLing;

	private VisualElement m_notifyWeekCard;

	private zbLegendaryLevelCsvRecord m_record;

	private RoleBaseInfo m_roleBaseInfo;

	private Button m_btnAuto;

	private VisualElement m_spriteOk;

	private int[][] m_positions = new int[3][]
	{
		new int[2] { -330, 370 },
		new int[2] { -110, 475 },
		new int[2] { -320, 540 }
	};

	protected override void OnEnable()
	{
		//IL_03da: Unknown result type (might be due to invalid IL or missing references)
		//IL_0405: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04eb: Invalid comparison between Unknown and I4
		//IL_04fd: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_spineAvatar0 = zbUtil.FindGameObjectRecursive(((Component)this).transform, "playerAvatar0").GetComponent<zbPlayerAvatar>();
		m_spineAvatar1 = zbUtil.FindGameObjectRecursive(((Component)this).transform, "playerAvatar1").GetComponent<zbPlayerAvatar>();
		m_nodeTop = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_top", (string)null);
		m_lbLevel = UQueryExtensions.Q<Label>(m_RootElement, "lb_level", (string)null);
		m_doubleAward = UQueryExtensions.Q<VisualElement>(m_RootElement, "double_award", (string)null);
		for (int i = 0; i < 5; i++)
		{
			m_items[i] = UQueryExtensions.Q<zbItem>(m_RootElement, "item" + i, (string)null);
			m_items[i].OnEnable(this);
		}
		m_bgDesc = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_desc", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_btnFreeGift = UQueryExtensions.Q<Button>(m_RootElement, "btn_freeGift", (string)null);
		m_spriteFreeGift = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_freeGift", (string)null);
		m_lbFreeGift = UQueryExtensions.Q<Label>(m_RootElement, "lb_freeGift", (string)null);
		ve_joinplay = UQueryExtensions.Q<VisualElement>(m_RootElement, "ve_joinplay", (string)null);
		m_btnJoinPlayer = UQueryExtensions.Q<Button>(m_RootElement, "btn_joinplay", (string)null);
		m_btnJoinClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_joinclose", (string)null);
		m_lbSelfName = UQueryExtensions.Q<Label>(m_RootElement, "lb_selfName", (string)null);
		m_lbPlayerName = UQueryExtensions.Q<Label>(m_RootElement, "lb_playerName", (string)null);
		ve_joinplayGuide = UQueryExtensions.Q<VisualElement>(m_RootElement, "ve_joinplayGuide", (string)null);
		m_btnAuto = UQueryExtensions.Q<Button>(m_RootElement, "btn_auto", (string)null);
		m_spriteOk = UQueryExtensions.Q<VisualElement>(m_RootElement, "sprite_ok", (string)null);
		for (int j = 0; j < 2; j++)
		{
			m_lbCosts[j] = UQueryExtensions.Q<Label>(m_RootElement, "lb_cost" + j, (string)null);
			int currentIndex = j;
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_lbCosts[j], (Action<ClickEvent>)delegate(ClickEvent evt)
			{
				OnCosts(evt, currentIndex);
			}, "", (TrickleDown)0);
		}
		m_btnFight = UQueryExtensions.Q<Button>(m_RootElement, "btn_fight", (string)null);
		m_btnZhanLing = UQueryExtensions.Q<Button>(m_RootElement, "btn_zhanLing", (string)null);
		m_btnWeekCard = UQueryExtensions.Q<Button>(m_RootElement, "btn_weekCard", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnFreeGift, (Action<ClickEvent>)OnFreeGift, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnJoinPlayer, (Action<ClickEvent>)OnJoinPlayer, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnJoinClose, (Action<ClickEvent>)OnJoinClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnFight, (Action<ClickEvent>)OnFight, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnZhanLing, (Action<ClickEvent>)OnZhanLing, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnWeekCard, (Action)OnWeekCard, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAuto, (Action)OnAuto, "", (TrickleDown)0);
		m_notifyFreeGift = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnFreeGift, (Vector2?)new Vector2(10f, -6f), 1f, true);
		m_notifyFight = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnFight, (Vector2?)new Vector2(5f, 5f), 1f, true);
		m_notifyZhanLing = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnZhanLing);
		m_notifyWeekCard = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnWeekCard);
		CreateMoneyPanel(new EMoneyType[2]
		{
			EMoneyType.Tili,
			EMoneyType.Legendary
		});
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_LegendaryInfo, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Money, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_SuccessOrder_Ack, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Activity, OnEventUpdate);
		if ((int)Application.platform == 8)
		{
			m_nodeTop.style.top = StyleLength.op_Implicit(20f);
		}
	}

	public void RefreshCustom(bool isFirstIn)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_030b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0336: Unknown result type (might be due to invalid IL or missing references)
		m_doubleAward.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbSelf.Instance.vipHandler.GetDoubleAwardFunType() != EFuncType.Legendary));
		if (!zbGuide.Instance.GetGuideBool(EGuideStep.guide_step_Legendary_2) && zbSelf.Instance.funHandler.IsNeedGuide(EFuncType.Legendary))
		{
			zbGuide.Instance.SetGuideBool(EGuideStep.guide_step_Legendary_2);
			zbGuide.Instance.ExecGuide(EGuideStep.guide_step_Legendary_2);
		}
		m_spineAvatar0.RefreshCustom(zbSelf.Instance.GetAvatarId(), zbSelf.Instance.GetGunId());
		((TextElement)m_lbSelfName).text = zbSelf.Instance.GetRoleName();
		if (isFirstIn)
		{
			m_roleBaseInfo = null;
		}
		RefreshDetail();
		RefreshAuto();
		int num = 0;
		zbItemData[] costList = zbSelf.Instance.legendaryHandler.m_costList;
		foreach (zbItemData zbItemData2 in costList)
		{
			if (num < m_lbCosts.Length)
			{
				((VisualElement)m_lbCosts[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				stResorceIconInfo resorceIconPath = zbResourceHelper.GetResorceIconPath(zbItemData2.type, zbItemData2.id);
				((VisualElement)m_lbCosts[num]).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resorceIconPath.strIconPath));
				((TextElement)m_lbCosts[num]).text = "x" + zbItemData2.count;
				if (!zbSelf.Instance.dataHandler.HasResourceEnough(zbItemData2.type, (int)zbItemData2.id, (int)zbItemData2.count))
				{
					((VisualElement)m_lbCosts[num]).style.color = StyleColor.op_Implicit(Color.red);
				}
				else
				{
					((VisualElement)m_lbCosts[num]).style.color = StyleColor.op_Implicit(Color.white);
				}
			}
			num++;
		}
		FreeGiftDetail freeGiftDetail = ((IEnumerable<FreeGiftDetail>)zbServerData.sGameData.FreeGiftList).FirstOrDefault((FreeGiftDetail r) => r.FreeType == 6);
		if (freeGiftDetail != null)
		{
			stResorceIconInfo resorceIconPath2 = zbResourceHelper.GetResorceIconPath(freeGiftDetail.Award.Type, freeGiftDetail.Award.Tid);
			m_spriteFreeGift.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resorceIconPath2.strIconPath));
			((TextElement)m_lbFreeGift).text = "x" + freeGiftDetail.Award.Count;
		}
		((VisualElement)m_btnFreeGift).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.legendaryHandler.HasFreeGiftNotify()));
		m_notifyFreeGift.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.legendaryHandler.HasFreeGiftNotify()));
		m_notifyFight.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.legendaryHandler.HasFightNotify()));
		m_notifyZhanLing.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.legendaryHandler.HasZhanLingNotify()));
		m_notifyWeekCard.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.vipHandler.HasVipCardNotifyByTab(EVipCardTab.Week)));
	}

	private void RefreshDetail()
	{
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		RefreshJoinPlayer();
		m_record = zbDataConfig.Instance.legendaryLevelCsvTable.GetRecordByKey(zbServerData.sGameData.LegendaryInfo.NextLevel);
		((TextElement)m_lbLevel).text = zbResourceHelper.GetLegendaryLevelName(zbServerData.sGameData.LegendaryInfo.NextLevel);
		RefreshAwards();
		zbLegendaryEffectCsvRecord recordByKey = zbDataConfig.Instance.legendaryEffectCsvTable.GetRecordByKey(zbServerData.sGameData.LegendaryInfo.EffectId);
		if (recordByKey != null)
		{
			int[] array = m_positions[recordByKey.position - 1];
			if (array != null)
			{
				m_bgDesc.style.left = StyleLength.op_Implicit((float)array[0]);
				m_bgDesc.style.top = StyleLength.op_Implicit((float)array[1]);
			}
			((TextElement)m_lbDesc).text = recordByKey.desc;
		}
	}

	private void RefreshAwards()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 5; i++)
		{
			((VisualElement)m_items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		int num = 0;
		int[][] array = m_record?.awardShow;
		foreach (int[] array2 in array)
		{
			if (num < m_items.Length)
			{
				int num2 = ((zbSelf.Instance.vipHandler.GetDoubleAwardFunType() != EFuncType.Legendary || (array2[0] == 1 && array2[1] == 5)) ? 1 : 2);
				zbItemData data = new zbItemData((EResourceType)array2[0], array2[1], array2[2] * num2);
				((VisualElement)m_items[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_items[num].Refresh(data);
			}
			num++;
		}
		VisualElement[] items = (VisualElement[])(object)m_items;
		zbUtil.CenterNodeItems(items);
	}

	private void RefreshAuto()
	{
		zbSelf.Instance.combatHandler.SetAutoOKDisplay(EBattleFuncType.Legendary, m_spriteOk);
	}

	private void OnAuto()
	{
		if (zbSelf.Instance.vipHandler.CheckVipFuncLimited(EVipFuncType.AutoFight, zbSelf.Instance.GetVipLevel()) <= 0)
		{
			zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.11492"), zbSelf.Instance.vipHandler.CheckVipFuncUnlock(EVipFuncType.AutoFight)) + "【" + zbUtil.Language("lang.11494") + "】");
			return;
		}
		zbSelf.Instance.combatHandler.SetAutoSelectMod(EBattleFuncType.Legendary);
		RefreshAuto();
	}

	private void RefreshJoinPlayer()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if (m_roleBaseInfo != null)
		{
			ve_joinplay.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_btnJoinClose).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((Component)m_spineAvatar1).gameObject.SetActive(true);
			m_spineAvatar1.RefreshCustom(m_roleBaseInfo.AvatarId, m_roleBaseInfo.GunId);
			((TextElement)m_lbPlayerName).text = m_roleBaseInfo.RoleName;
		}
		else
		{
			ve_joinplay.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_btnJoinClose).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((Component)m_spineAvatar1).gameObject.SetActive(false);
			((TextElement)m_lbPlayerName).text = "";
		}
	}

	public void OnCosts(ClickEvent evt, int index)
	{
		zbItemData zbItemData2 = zbSelf.Instance.legendaryHandler.m_costList[index];
		zbUIMgr.Instance.ShowTip(zbItemData2.type, zbItemData2.id, zbItemData2.count);
	}

	public void OnFreeGift(ClickEvent evt)
	{
		if (zbSelf.Instance.legendaryHandler.HasFreeGiftNotify())
		{
			zbWsMsgHandler.Instance.Aux_FreeGiftAward_Req(6);
		}
	}

	public void OnJoinPlayer(ClickEvent evt)
	{
		zbUIMgr.Instance.ShowMainQuestJoin(0, EFuncType.Legendary);
	}

	public void OnJoinClose(ClickEvent evt)
	{
		m_roleBaseInfo = null;
		RefreshJoinPlayer();
	}

	public void SetJoinPlayer(RoleBaseInfo info)
	{
		m_roleBaseInfo = info;
		RefreshJoinPlayer();
	}

	private void OnFight(ClickEvent evt)
	{
		zbItemData[] costList = zbSelf.Instance.legendaryHandler.m_costList;
		foreach (zbItemData zbItemData2 in costList)
		{
			if (!zbSelf.Instance.dataHandler.HasResourceEnough(zbItemData2.type, (int)zbItemData2.id, (int)zbItemData2.count))
			{
				zbUIMgr.Instance.ShowTip(zbItemData2.type, (int)zbItemData2.id, (int)zbItemData2.count);
				return;
			}
		}
		if (zbServerData.sGameData.MainQuestInfo.MainLevel < m_record.mainLevel)
		{
			zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.10616"), m_record.mainLevel));
			return;
		}
		long num = ((m_roleBaseInfo != null) ? m_roleBaseInfo.RoleId : 0);
		zbSelf.Instance.legendaryHandler.playerId = num;
		if (num == 0L)
		{
			zbUIMgr.Instance.ShowMessageBox(zbUtil.Language("lang.10143"), zbUtil.Language("lang.10662"), delegate(bool isOK)
			{
				if (isOK)
				{
					zbWsMsgHandler.Instance.Legendary_FightStart_Req(0L);
				}
			}, zbUtil.Language("lang.10206"), zbUtil.Language("lang.10287"));
		}
		else
		{
			zbWsMsgHandler.Instance.Legendary_FightStart_Req(num);
		}
	}

	private void OnZhanLing(ClickEvent evt)
	{
		zbUIMgr.Instance.ShowLegendaryZhanLing();
	}

	private void OnWeekCard()
	{
		zbUIMgr.Instance.ShowWeekCardWnd();
	}

	public void OnClose(ClickEvent evt)
	{
		Hide();
	}

	private void OnEventUpdate(object data)
	{
		RefreshCustom(isFirstIn: false);
	}
}
