using System;
using System.Collections;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbTreasureTabBar
{
	private zbTreasureWnd m_wndParent;

	private VisualElement m_rootElement;

	private Label m_lbName;

	private VisualElement m_nodeBottom;

	private VisualElement m_nodeRareHit;

	private Label m_lbRareHit;

	private Label m_lbMoenyName;

	private Button m_btnMoney;

	private Label m_lbTitle;

	private Button m_btnPool;

	private Button m_btnPrivilege;

	private SpineVisualElement m_spinePrivilege;

	private VisualElement m_notifyPrivilege;

	private EBoolState m_lastPrivilegeState = (EBoolState)(-1);

	public Button m_btnTreasure;

	private Label m_lbTreasure;

	private Label m_lbFreeTreasure;

	private Label m_lbCountTreasure;

	private Button m_btnTreasurePlus;

	private Label m_lbTreasurePlus;

	private Label m_lbFreeTreasurePlus;

	private Label m_lbCountTreasurePlus;

	private Coroutine m_barCoroutine;

	private bool m_skipAnimation;

	private bool m_animationPlaying;

	private int m_treasureCost;

	private int m_treasurePlusCost;

	private BarDetail m_treasureDetail;

	private BarDetail m_treasurePlusDetail;

	private EBarType m_reqBarType;

	private BarDrawAck m_drawAck;

	private bool m_isTargetReq;

	public void OnEnable(zbTreasureWnd wndParent, VisualElement root)
	{
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		m_rootElement = root;
		m_lbName = UQueryExtensions.Q<Label>(m_rootElement, "lb_name", (string)null);
		m_nodeBottom = UQueryExtensions.Q<VisualElement>(m_rootElement, "node_bottom", (string)null);
		m_nodeRareHit = UQueryExtensions.Q<VisualElement>(m_nodeBottom, "rareHit", (string)null);
		m_lbRareHit = UQueryExtensions.Q<Label>(m_nodeBottom, "lb_rareHit", (string)null);
		m_lbMoenyName = UQueryExtensions.Q<Label>(m_nodeBottom, "lb_moneyName", (string)null);
		m_btnMoney = UQueryExtensions.Q<Button>(m_nodeBottom, "btn_money", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_nodeBottom, "lb_title", (string)null);
		m_btnPool = UQueryExtensions.Q<Button>(m_nodeBottom, "btn_pool", (string)null);
		m_btnPrivilege = UQueryExtensions.Q<Button>(m_nodeBottom, "btn_privilege", (string)null);
		m_spinePrivilege = UQueryExtensions.Q<SpineVisualElement>((VisualElement)(object)m_btnPrivilege, "spine", (string)null);
		m_notifyPrivilege = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnPrivilege, (Vector2?)new Vector2(0f, 0f), 1f, true);
		m_btnTreasure = UQueryExtensions.Q<Button>(m_nodeBottom, "btn_treasure", (string)null);
		m_lbTreasure = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnTreasure, "lb_bar", (string)null);
		m_lbFreeTreasure = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnTreasure, "lb_free", (string)null);
		m_lbCountTreasure = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnTreasure, "lb_count", (string)null);
		m_btnTreasurePlus = UQueryExtensions.Q<Button>(m_nodeBottom, "btn_treasurePlus", (string)null);
		m_lbTreasurePlus = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnTreasurePlus, "lb_bar", (string)null);
		m_lbFreeTreasurePlus = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnTreasurePlus, "lb_free", (string)null);
		m_lbCountTreasurePlus = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnTreasurePlus, "lb_count", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnPool, (Action<ClickEvent>)OnPool, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnPrivilege, (Action<ClickEvent>)OnPrivilege, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTreasure, (Action)OnTreasure, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTreasurePlus, (Action)OnTreasurePlus, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_wndParent.m_btnTouch, (Action)OnTouch, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnMoney, (Action)OnShowMoney, "", (TrickleDown)0);
		m_wndParent.m_gameEevent.RegisterEvent(EGameEvent.Event_Aux_FreeGiftAward, delegate
		{
			RefreshTarget();
		});
		m_wndParent.m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Activity, delegate
		{
			RefreshTarget();
		});
		m_wndParent.m_gameEevent.RegisterEvent(EGameEvent.Event_Treasure_Bar_Draw_Ack, OnTreasureBarAck);
		InitRecord();
	}

	private void InitRecord()
	{
		m_treasureCost = zbSelf.Instance.barHandler.GetBarCost(EBarType.Treasure);
		m_treasurePlusCost = zbSelf.Instance.barHandler.GetBarCost(EBarType.TreasurePlus);
		((TextElement)m_lbCountTreasure).text = "x" + m_treasureCost;
		((TextElement)m_lbCountTreasurePlus).text = "x" + m_treasurePlusCost;
	}

	public void Refresh()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		m_barCoroutine = null;
		m_wndParent.m_barSpine.AnimationState.SetAnimation(0, "idle", true);
		((Component)m_wndParent.m_barChoukaSpine).gameObject.SetActive(false);
		((VisualElement)m_lbName).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_wndParent.m_oneObject.SetActive(false);
		((VisualElement)m_wndParent.m_btnTouch).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_nodeBottom.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_wndParent.SetUIVisible(vis: true);
		m_treasureDetail = zbSelf.Instance.barHandler.GetBarDetail(EBarType.Treasure);
		m_treasurePlusDetail = zbSelf.Instance.barHandler.GetBarDetail(EBarType.TreasurePlus);
		RefreshDetail();
		RefreshTarget();
	}

	private void RefreshDetail()
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		if (m_treasureDetail != null && m_treasurePlusDetail != null)
		{
			if (m_treasureDetail.DailyFree == EBoolState.True)
			{
				((VisualElement)m_lbFreeTreasure).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				((VisualElement)m_lbCountTreasure).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
			else
			{
				((VisualElement)m_lbFreeTreasure).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				((VisualElement)m_lbCountTreasure).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				bool flag = zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Money, 20, m_treasureCost);
				((VisualElement)m_lbCountTreasure).style.color = StyleColor.op_Implicit(flag ? Color.white : Color.red);
			}
			if (m_treasurePlusDetail.DailyFree == EBoolState.True)
			{
				((VisualElement)m_lbFreeTreasurePlus).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				((VisualElement)m_lbCountTreasurePlus).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				return;
			}
			((VisualElement)m_lbFreeTreasurePlus).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_lbCountTreasurePlus).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			bool flag2 = zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Money, 21, m_treasurePlusCost);
			((VisualElement)m_lbCountTreasurePlus).style.color = StyleColor.op_Implicit(flag2 ? Color.white : Color.red);
		}
	}

	private void RefreshTarget()
	{
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (zbServerData.sGameData.RechargeActiveInfo.TreasurePrivilegeInfo.Active == EBoolState.True)
		{
			m_nodeRareHit.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			string key = "<color=#FF0000>" + zbDataConfig.Instance.moneyCsvTable.GetRecordByKey(21).name + "</color>";
			int num = zbSelf.Instance.treasureHandler.targetNum - (zbServerData.sGameData.RechargeActiveInfo.TreasurePrivilegeInfo.Reach - zbServerData.sGameData.RechargeActiveInfo.TreasurePrivilegeInfo.Target);
			((TextElement)m_lbRareHit).text = string.Format(zbUtil.Language("lang.10952"), num);
			((TextElement)m_lbMoenyName).text = zbUtil.Language(key);
			if (num <= 0 && !m_isTargetReq)
			{
				m_isTargetReq = true;
				zbWsMsgHandler.Instance.RechargeActive_TreasurePrivilege_Req();
			}
		}
		else
		{
			m_nodeRareHit.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		RefreshPrivilege();
	}

	private void RefreshPrivilege()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		m_notifyPrivilege.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.treasureHandler.HasFreePackRedDot()));
		EBoolState active = zbServerData.sGameData.RechargeActiveInfo.TreasurePrivilegeInfo.Active;
		if (active != m_lastPrivilegeState)
		{
			m_lastPrivilegeState = active;
			if (active == EBoolState.True)
			{
				m_spinePrivilege.AnimationState.SetAnimation(0, "loop", true);
			}
			else
			{
				m_spinePrivilege.AnimationState.SetAnimation(0, "normal", false);
			}
		}
	}

	private void OnPool(ClickEvent evt)
	{
		zbUIMgr.Instance.ShowTreasurePool();
	}

	private void OnPrivilege(ClickEvent evt)
	{
		zbUIMgr.Instance.ShowTreasurePrivilege();
	}

	private void OnTreasure()
	{
		m_reqBarType = EBarType.Treasure;
		if (m_treasureDetail.DailyFree == EBoolState.True)
		{
			zbWsMsgHandler.Instance.Bar_Draw_Req(m_reqBarType, 1);
		}
		else if (!zbSelf.Instance.moneyHandler.HasEnoughMoney(20, m_treasureCost))
		{
			zbUIMgr.Instance.ShowTip(EResourceType.Money, 20L, m_treasureCost);
		}
		else
		{
			zbWsMsgHandler.Instance.Bar_Draw_Req(m_reqBarType, 1);
		}
	}

	private void OnTreasurePlus()
	{
		m_reqBarType = EBarType.TreasurePlus;
		if (m_treasurePlusDetail.DailyFree == EBoolState.True)
		{
			zbWsMsgHandler.Instance.Bar_Draw_Req(m_reqBarType, 1);
		}
		else if (!zbSelf.Instance.moneyHandler.HasEnoughMoney(21, m_treasurePlusCost))
		{
			zbUIMgr.Instance.ShowTip(EResourceType.Money, 21L, m_treasurePlusCost);
		}
		else
		{
			zbWsMsgHandler.Instance.Bar_Draw_Req(m_reqBarType, 1);
		}
	}

	public void OnTouch()
	{
		if (m_wndParent.m_currTab != 0)
		{
			if (m_animationPlaying)
			{
				m_skipAnimation = true;
				return;
			}
			m_isTargetReq = false;
			zbServerData.sGameData.BarInfo = m_drawAck.BarInfo;
			zbSelf.Instance.dataHandler.DataChange(m_drawAck.DataChange);
		}
	}

	private void OnTreasureBarAck(object obj)
	{
		if (!(obj is BarDrawAck barDrawAck))
		{
			return;
		}
		m_isTargetReq = true;
		m_drawAck = barDrawAck;
		if (m_barCoroutine != null)
		{
			((MonoBehaviour)m_wndParent).StopCoroutine(m_barCoroutine);
			m_barCoroutine = null;
		}
		if (barDrawAck.AwardList.Count > 0)
		{
			AwardDetail award = barDrawAck.AwardList[0];
			zbTreasureCsvRecord zbTreasureCsvRecord2 = zbDataConfig.Instance.treasureCsvTable.GetRecords().records.Values.FirstOrDefault((zbTreasureCsvRecord r) => r.propId == award.Tid);
			if (zbTreasureCsvRecord2 != null)
			{
				m_barCoroutine = ((MonoBehaviour)m_wndParent).StartCoroutine(SpineAnimation(zbTreasureCsvRecord2));
			}
		}
	}

	private IEnumerator SpineAnimation(zbTreasureCsvRecord record)
	{
		string[] animationNames = GetAnimatinoNames(record);
		m_skipAnimation = false;
		m_animationPlaying = true;
		m_nodeBottom.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		((VisualElement)m_wndParent.m_btnTouch).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_wndParent.SetUIVisible(vis: false);
		AudioSource m_audio = zbAudioMgr.Instance.PlayAudio("audio/ui/yixingchouka_1.ogg", loop: true);
		m_wndParent.m_barSpine.AnimationState.SetAnimation(0, animationNames[0], false);
		m_wndParent.m_barSpine.AnimationState.AddAnimation(0, animationNames[1], true, 0f);
		float waitTime1 = 2.3333333f;
		float elapsed = 0f;
		while (elapsed < waitTime1 && !m_skipAnimation)
		{
			elapsed += Time.deltaTime;
			yield return null;
		}
		if (m_skipAnimation)
		{
			zbAudioMgr.Instance.StopAudio(m_audio);
			zbAudioMgr.Instance.PlayAudio("audio/ui/yixingchouka_2.ogg");
			EndSpineAnimation(animationNames, record);
			yield break;
		}
		RefreshBarAckAward(record, immediate: false);
		zbAudioMgr.Instance.StopAudio(m_audio);
		zbAudioMgr.Instance.PlayAudio("audio/ui/yixingchouka_2.ogg");
		m_wndParent.m_barChoukaSpine.AnimationState.SetAnimation(0, animationNames[2], false);
		m_wndParent.m_barChoukaSpine.AnimationState.AddAnimation(0, animationNames[3], true, 0f);
		elapsed = 0f;
		float minShowTime = 2f;
		while (elapsed < minShowTime && !m_skipAnimation)
		{
			elapsed += Time.deltaTime;
			yield return null;
		}
		EndSpineAnimation(animationNames, record);
	}

	private void EndSpineAnimation(string[] animationNames, zbTreasureCsvRecord record)
	{
		if (m_animationPlaying)
		{
			m_animationPlaying = false;
			m_wndParent.m_barSpine.AnimationState.SetAnimation(0, animationNames[1], true);
			if (!((Component)m_wndParent.m_barChoukaSpine).gameObject.activeSelf)
			{
				RefreshBarAckAward(record, immediate: true);
			}
			m_wndParent.m_barChoukaSpine.AnimationState.SetAnimation(0, animationNames[3], true);
		}
	}

	private string[] GetAnimatinoNames(zbTreasureCsvRecord record)
	{
		string text = "purple";
		if (record.quality == 5)
		{
			text = "gold";
		}
		else if (record.quality == 6)
		{
			text = "red";
		}
		if (m_reqBarType != EBarType.Treasure)
		{
			return new string[4]
			{
				"act_2",
				"idle",
				"gold_in_" + text,
				"gold_loop_" + text
			};
		}
		return new string[4]
		{
			"act",
			"idle",
			"blue_in_" + text,
			"blue_loop_" + text
		};
	}

	private void OnShowMoney()
	{
		zbUIMgr.Instance.ShowTip(EResourceType.Money, 21L, 0L);
	}

	private void RefreshBarAckAward(zbTreasureCsvRecord record, bool immediate)
	{
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		zbAudioMgr.Instance.PlayAudio("audio/ui/rise_normal.ogg");
		((Component)m_wndParent.m_barChoukaSpine).gameObject.gameObject.SetActive(true);
		((VisualElement)m_lbName).schedule.Execute((Action)delegate
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			((VisualElement)m_lbName).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}).ExecuteLater((long)((!immediate) ? 633 : 0));
		m_wndParent.m_oneObject.SetActive(true);
		m_wndParent.m_spriteTreasure.sprite = zbAssetLoader.Instance.LoadSpriteAsset(record.icon);
		((VisualElement)m_lbName).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>($"ui/image/treasure/treasure_name{record.quality}.png"));
		((TextElement)m_lbName).text = record.name;
	}
}
