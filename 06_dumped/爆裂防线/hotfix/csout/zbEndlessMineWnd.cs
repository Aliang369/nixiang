using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbEndlessMineWnd : zbBaseWnd
{
	public Dictionary<EPropType, int> PropTypeIds = new Dictionary<EPropType, int>
	{
		{
			EPropType.Drill,
			10004
		},
		{
			EPropType.Pickaxe,
			10003
		},
		{
			EPropType.Bomb,
			10005
		},
		{
			EPropType.Ore,
			10002
		}
	};

	private zbSpineScene m_spineScene;

	private zbEndlessMineHero[] m_heros = new zbEndlessMineHero[3];

	private Label m_lbTime;

	private zbHolidayEventView m_holidayEventView;

	private Button m_btnNextExchange;

	private VisualElement m_nodeExchange;

	private zbItem[] m_items = new zbItem[4];

	private VisualElement m_progress;

	private Label m_lbProgress;

	public VisualElement m_spriteOre;

	public SpineVisualElement m_spineProgress;

	private Button m_btnExchange;

	private VisualElement m_notifyExchange;

	private Button m_btnThemeTask;

	private VisualElement m_notifyThemeTask;

	private Button m_btnTarget;

	private zbItem m_itemTarget;

	private Label m_lbTarget;

	private VisualElement m_notifyTarget;

	private Label m_lbDepth;

	private VisualElement m_nodeFinger;

	private zbEndlessMineMap m_map = new zbEndlessMineMap();

	private Button m_btnPickaxe;

	private Button m_btnDrill;

	private Button m_btnBomb;

	private Label m_lbPickaxe;

	private Label m_lbDrill;

	private Label m_lbBomb;

	private Button m_btnBag;

	private Button m_btnClose;

	private Sprite m_selectedIcon;

	private Sprite m_unSelectedIcon;

	public EPropType m_selectProp = EPropType.Pickaxe;

	private bool m_isFirstIn = true;

	private ThemeTaskDetail m_themeTaskDetail;

	public bool m_isGetPropsAck;

	private string m_curLoopAnim = "";

	private OperateTargetDetail m_targetDetail;

	protected override void OnEnable()
	{
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_selectedIcon = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/operate/endlessMine/endlessMine_frameSel.png");
		m_unSelectedIcon = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/operate/endlessMine/endlessMine_frame.png");
		m_spineScene = ((Component)m_GameObjectSceneRoot.transform.Find("spineScene")).GetComponent<zbSpineScene>();
		for (int i = 0; i < 3; i++)
		{
			m_heros[i] = ((Component)m_GameObjectSceneRoot.transform.Find("hero" + i)).GetComponent<zbEndlessMineHero>();
		}
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_holidayEventView = new zbHolidayEventView();
		m_holidayEventView.OnEnable(this, UQueryExtensions.Q<VisualElement>(m_RootElement, "holidayEvent", (string)null));
		m_btnNextExchange = UQueryExtensions.Q<Button>(m_RootElement, "btn_nextExchange", (string)null);
		m_nodeExchange = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_exchange", (string)null);
		for (int j = 0; j < 4; j++)
		{
			m_items[j] = UQueryExtensions.Q<zbItem>(m_nodeExchange, "item" + j, (string)null);
			m_items[j].OnEnable(this);
		}
		m_progress = UQueryExtensions.Q<VisualElement>(m_nodeExchange, "progressVal", (string)null);
		m_lbProgress = UQueryExtensions.Q<Label>(m_nodeExchange, "lb_progress", (string)null);
		m_spriteOre = UQueryExtensions.Q<VisualElement>(m_nodeExchange, "sprite_ore", (string)null);
		m_spineProgress = UQueryExtensions.Q<SpineVisualElement>(m_nodeExchange, "spine_progress", (string)null);
		m_btnExchange = UQueryExtensions.Q<Button>(m_nodeExchange, "btn_exchange", (string)null);
		m_notifyExchange = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnExchange, (Vector2?)new Vector2(-2f, 0f), 1f, true);
		m_notifyExchange.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_btnThemeTask = UQueryExtensions.Q<Button>(m_RootElement, "btn_themeTask", (string)null);
		m_notifyThemeTask = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnThemeTask, (Vector2?)new Vector2(-3f, -3f), 1f, true);
		m_btnTarget = UQueryExtensions.Q<Button>(m_RootElement, "btn_target", (string)null);
		m_itemTarget = UQueryExtensions.Q<zbItem>((VisualElement)(object)m_btnTarget, "item_target", (string)null);
		m_itemTarget.OnEnable(this);
		m_lbTarget = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnTarget, "lb_target", (string)null);
		m_notifyTarget = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnTarget, (Vector2?)new Vector2(-5f, -5f), 1f, true);
		m_lbDepth = UQueryExtensions.Q<Label>(m_RootElement, "lb_depth", (string)null);
		m_nodeFinger = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_finger", (string)null);
		m_map.OnEnable(this, m_RootElement);
		m_btnPickaxe = UQueryExtensions.Q<Button>(m_RootElement, "btn_pickaxe", (string)null);
		m_lbPickaxe = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnPickaxe, "lb_pickaxe", (string)null);
		m_btnDrill = UQueryExtensions.Q<Button>(m_RootElement, "btn_drill", (string)null);
		m_lbDrill = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnDrill, "lb_drill", (string)null);
		m_btnBomb = UQueryExtensions.Q<Button>(m_RootElement, "btn_bomb", (string)null);
		m_lbBomb = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnBomb, "lb_bomb", (string)null);
		m_btnBag = UQueryExtensions.Q<Button>(m_RootElement, "btn_bag", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnNextExchange, (Action)OnNextExchange, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnExchange, (Action)OnExchange, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnThemeTask, (Action)OnThemeTask, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTarget, (Action)OnTarget, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnPickaxe, (Action<ClickEvent>)delegate
		{
			OnProps(EPropType.Pickaxe);
		}, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnDrill, (Action<ClickEvent>)delegate
		{
			OnProps(EPropType.Drill);
		}, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBomb, (Action<ClickEvent>)delegate
		{
			OnProps(EPropType.Bomb);
		}, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBag, (Action)OnBag, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		CreateMoneyPanel(new EMoneyType[1] { EMoneyType.Diamond });
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_EndlessMineInfo, delegate
		{
			Refresh();
		});
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Operate, delegate
		{
			Refresh();
		});
		m_gameEevent.RegisterEvent(EGameEvent.Event_EndlessMine_GetProps_Ack, OnGetPropsAck);
		m_gameEevent.RegisterEvent(EGameEvent.Event_EndlessMine_ExchangeAward_Ack, OnEndlessMineExchangeAwardAck);
	}

	private void InitHerosWalk()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		m_heros[0].Refresh(10210, new Vector3(-2f, 4.25f, -6f), 0.8f);
		m_heros[1].Refresh(11110, new Vector3(1.3f, 3.25f, -6f), 0.5f);
		m_heros[2].Refresh(10710, new Vector3(0.3f, 0.9f, -6f), 0.9f, 0.2f);
	}

	public override void Refresh()
	{
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		zbSelf.Instance.holidayHandler.GetHolidayCount();
		EndlessMineDetail endlessMineDetail = zbSelf.Instance.endlessMineHandler.GetEndlessMineDetail();
		if (endlessMineDetail != null)
		{
			m_holidayEventView.Refresh(endlessMineDetail.BaseInfo.PlanId);
		}
		m_themeTaskDetail = zbSelf.Instance.holidayHandler.GetThemeTaskDetail();
		m_notifyThemeTask.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_themeTaskDetail == null || !zbSelf.Instance.operateHandler.HasThemeTaskNotify(m_themeTaskDetail.BaseInfo.UUID)) ? 1 : 0));
		if (zbSelf.Instance.endlessMineHandler.endlessMineInfo != null)
		{
			((TextElement)m_lbDepth).text = $"{zbSelf.Instance.endlessMineHandler.endlessMineInfo.EndlessMineBaseInfo.Depth}m";
			RefreshTarget();
			RefreshExchange();
			RefreshProps();
			RefreshSelectProp(EPropType.Pickaxe);
			RefreshTime();
			m_map.Refresh();
			if (m_isFirstIn)
			{
				m_isFirstIn = false;
				InitHerosWalk();
			}
			if (zbSelf.Instance.newTagHandler.IsNewEndlessMine((int)zbSelf.Instance.endlessMineHandler.endlessMineInfo.EndlessMineBaseInfo.StartTm) && zbSelf.Instance.endlessMineHandler.GetEndlessMinePropCount(PropTypeIds[EPropType.Pickaxe]) > 0 && zbSelf.Instance.endlessMineHandler.endlessMineInfo.EndlessMineBaseInfo.Depth == 0)
			{
				zbSelf.Instance.newTagHandler.AddNewEndlessMine((int)zbSelf.Instance.endlessMineHandler.endlessMineInfo.EndlessMineBaseInfo.StartTm);
				zbUIMgr.Instance.ShowGuideFinger(m_nodeFinger);
			}
		}
	}

	public void RefreshTarget()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		m_notifyTarget.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.endlessMineHandler.HasTargetNotify()));
		m_targetDetail = zbSelf.Instance.endlessMineHandler.GetTargetDetail();
		if (m_targetDetail != null)
		{
			((TextElement)m_lbTarget).text = $"{zbSelf.Instance.endlessMineHandler.endlessMineInfo.EndlessMineBaseInfo.Depth}/{m_targetDetail.Target}";
			AwardDetail awardDetail = m_targetDetail.AwardList[0];
			m_itemTarget.Refresh(new zbItemData(awardDetail.Type, awardDetail.Tid, awardDetail.Count), isNameVis: false, isCountVis: true, isTouchVis: false);
		}
	}

	public void RefreshExchange()
	{
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_0267: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		if (m_isGetPropsAck)
		{
			return;
		}
		string text;
		if (zbSelf.Instance.endlessMineHandler.endlessMineInfo.EndlessMineBaseInfo.ExchangeNextGrand.CurrentIndex < zbSelf.Instance.endlessMineHandler.endlessMineInfo.EndlessMineBaseInfo.ExchangeNextGrand.GrandIndex)
		{
			((VisualElement)m_btnNextExchange).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((TextElement)m_btnNextExchange).text = zbSelf.Instance.endlessMineHandler.endlessMineInfo.EndlessMineBaseInfo.ExchangeNextGrand.CurrentIndex + "/" + zbSelf.Instance.endlessMineHandler.endlessMineInfo.EndlessMineBaseInfo.ExchangeNextGrand.GrandIndex;
			text = "normal_loop";
		}
		else
		{
			((VisualElement)m_btnNextExchange).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			text = "gold_loop";
		}
		if (m_curLoopAnim != text)
		{
			m_curLoopAnim = text;
			m_spineScene.SetAnimation(m_curLoopAnim, isLoop: true);
		}
		for (int i = 0; i < 4; i++)
		{
			((VisualElement)m_items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		int num = 0;
		foreach (AwardDetail award in zbSelf.Instance.endlessMineHandler.endlessMineInfo.EndlessMineBaseInfo.ExchangeDetail.AwardList)
		{
			if (num < 4)
			{
				m_items[num].Refresh(new zbItemData(award.Type, award.Tid, award.Count));
				((VisualElement)m_items[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			}
			num++;
		}
		int num2 = 0;
		EndlessMinePropDetail endlessMinePropDetail = zbSelf.Instance.endlessMineHandler.GetEndlessMinePropDetail(PropTypeIds[EPropType.Ore]);
		if (endlessMinePropDetail != null)
		{
			num2 = endlessMinePropDetail.Count;
		}
		int cost = zbSelf.Instance.endlessMineHandler.endlessMineInfo.EndlessMineBaseInfo.ExchangeDetail.Cost;
		((TextElement)m_lbProgress).text = $"{num2}/{cost}";
		float num3 = Mathf.Clamp((float)num2 / (float)cost, 0f, 1f);
		m_progress.style.width = StyleLength.op_Implicit(new Length(num3 * 206f, (LengthUnit)0));
		((VisualElement)m_btnExchange).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((num2 < cost) ? 1 : 0));
	}

	private void RefreshProps()
	{
		((TextElement)m_lbPickaxe).text = zbSelf.Instance.endlessMineHandler.GetEndlessMinePropCount(PropTypeIds[EPropType.Pickaxe]).ToString();
		((TextElement)m_lbDrill).text = zbSelf.Instance.endlessMineHandler.GetEndlessMinePropCount(PropTypeIds[EPropType.Drill]).ToString();
		((TextElement)m_lbBomb).text = zbSelf.Instance.endlessMineHandler.GetEndlessMinePropCount(PropTypeIds[EPropType.Bomb]).ToString();
	}

	private void RefreshSelectProp(EPropType type)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_btnPickaxe).style.backgroundImage = new StyleBackground((type == EPropType.Pickaxe) ? m_selectedIcon : m_unSelectedIcon);
		((VisualElement)m_btnDrill).style.backgroundImage = new StyleBackground((type == EPropType.Drill) ? m_selectedIcon : m_unSelectedIcon);
		((VisualElement)m_btnBomb).style.backgroundImage = new StyleBackground((type == EPropType.Bomb) ? m_selectedIcon : m_unSelectedIcon);
		m_selectProp = type;
		m_map.SetProp(PropTypeIds[type]);
	}

	private void OnProps(EPropType type)
	{
		EndlessMinePropDetail endlessMinePropDetail = zbSelf.Instance.endlessMineHandler.GetEndlessMinePropDetail(PropTypeIds[type]);
		if (endlessMinePropDetail != null)
		{
			if (endlessMinePropDetail.Count > 0 && m_selectProp != type)
			{
				RefreshSelectProp(type);
			}
			else
			{
				zbUIMgr.Instance.ShowTip(EResourceType.Prop, PropTypeIds[type], 0L);
			}
		}
	}

	private void OnNextExchange()
	{
		int num = zbSelf.Instance.endlessMineHandler.endlessMineInfo.EndlessMineBaseInfo.ExchangeNextGrand.GrandIndex - zbSelf.Instance.endlessMineHandler.endlessMineInfo.EndlessMineBaseInfo.ExchangeNextGrand.CurrentIndex;
		zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.11166"), num));
	}

	private void OnExchange()
	{
		if (!m_isGetPropsAck)
		{
			zbWsMsgHandler.Instance.EndlessMine_ExchangeAward_Req();
		}
	}

	private void OnThemeTask()
	{
		if (m_themeTaskDetail != null)
		{
			zbUIMgr.Instance.ShowThemeTask(m_themeTaskDetail);
		}
	}

	private void OnTarget()
	{
		if (zbSelf.Instance.endlessMineHandler.HasTargetNotify() && m_targetDetail != null)
		{
			zbWsMsgHandler.Instance.EndlessMine_TargetAward_Req(m_targetDetail.Index);
		}
		else
		{
			zbUIMgr.Instance.ShowEndlessMineTarget();
		}
	}

	private void OnBag()
	{
		zbUIMgr.Instance.ShowBag(2);
	}

	private void OnClose()
	{
		Hide();
	}

	private void RefreshTime()
	{
		if (zbSelf.Instance.endlessMineHandler.GetEndlessMineDetail() != null)
		{
			((TextElement)m_lbTime).text = zbUtil.FormatTimeSmart(zbServerData.sGameData.OperateInfo.EndlessMineInfo.BaseInfo.EndTm - zbSelf.Instance.timeHandler.GetServerTime());
		}
	}

	protected override void UpdateAtShow()
	{
		base.UpdateAtShow();
		m_holidayEventView.Update();
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		RefreshTime();
	}

	protected override void RefreshWhenAutoVisible()
	{
		base.RefreshWhenAutoVisible();
		zbWsMsgHandler.Instance.EndlessMine_Data_Req();
	}

	protected override void RefreshWhenAutoVisibleCaseNoScene()
	{
		base.RefreshWhenAutoVisibleCaseNoScene();
		zbWsMsgHandler.Instance.EndlessMine_Data_Req();
	}

	private void OnGetPropsAck(object data)
	{
		m_isGetPropsAck = true;
		m_map.ShowPropsUseEffect(data);
	}

	private void OnEndlessMineExchangeAwardAck(object data)
	{
		if (data is EndlessMineExchangeAwardAck ack)
		{
			((MonoBehaviour)this).StartCoroutine(PlayTargetAnimation(ack));
		}
	}

	private IEnumerator PlayTargetAnimation(EndlessMineExchangeAwardAck ack)
	{
		zbUIMgr.Instance.ShowModel();
		m_nodeExchange.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		if (m_curLoopAnim == "normal_loop")
		{
			m_spineScene.SetAnimation("normal_out", isLoop: false);
		}
		else
		{
			m_spineScene.SetAnimation("gold_out", isLoop: false);
		}
		yield return (object)new WaitForSeconds(5f / 6f);
		if (ack.EndlessMineBaseInfo.ExchangeNextGrand.GrandIndex == ack.EndlessMineBaseInfo.ExchangeNextGrand.CurrentIndex)
		{
			m_spineScene.PlaySequenceAnimation("gold_in", "gold_loop");
			m_curLoopAnim = "gold_loop";
		}
		else
		{
			((SkeletonRenderer)m_spineScene.skeletonAnimation).skeleton.SetToSetupPose();
			m_spineScene.skeletonAnimation.AnimationState.ClearTracks();
			m_spineScene.PlaySequenceAnimation("normal_in", "normal_loop");
			m_curLoopAnim = "normal_loop";
		}
		zbSelf.Instance.dataHandler.DataChange(ack.DataChange, ((IEnumerable<AwardDetail>)ack.AwardList).ToArray());
		yield return (object)new WaitForSeconds(1f);
		zbSelf.Instance.endlessMineHandler.UpdateEndlessMineBaseInfo(ack.EndlessMineBaseInfo);
		m_nodeExchange.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		zbUIMgr.Instance.HideModel();
	}
}
