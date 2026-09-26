using System;
using System.Collections;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbSnowVeilWnd : zbBaseWnd
{
	private zbSnowVeilMap m_map = new zbSnowVeilMap();

	private zbHolidayEventView m_holidayEventView;

	private Button m_btnNextTarget;

	private VisualElement m_nodeTarget;

	private VisualElement m_bgTarget;

	private VisualElement m_bgGrand;

	private Label m_lbTime;

	private zbItem[] m_items = new zbItem[4];

	private VisualElement m_progress;

	private Label m_lbProgress;

	public VisualElement m_spriteScore;

	public SpineVisualElement m_spineProgress;

	private Button m_btnScore;

	private Button m_btnTarget;

	private VisualElement m_notifyTarget;

	private Button m_btnThemeTask;

	private VisualElement m_notifyThemeTask;

	private VisualElement m_addBg;

	private Label m_lbCount;

	private Button m_btnGo;

	private Button m_btnAdd;

	private Button m_btnBatch;

	private SpineVisualElement m_spine;

	public SpineVisualElement m_spineBig;

	private VisualElement m_nodeSpeed;

	private VisualElement m_spriteOk;

	private Button m_btnRule;

	private Button m_btnClose;

	public SnowVeilDetail m_data;

	private ThemeTaskDetail m_themeTaskDetail;

	public bool m_isGetPropsAck;

	private Coroutine m_goAckCoroutine;

	private bool m_isGoCooling;

	public bool m_isSpeed;

	public float m_speed = 1f;

	private readonly int[] BATCHCOUNT = new int[4] { 1, 2, 5, 10 };

	private int m_batchCount = 1;

	private long m_propCount;

	protected override void OnEnable()
	{
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		zbPlayerAvatar component = ((Component)m_GameObjectSceneRoot.transform.Find("playerAvatar")).GetComponent<zbPlayerAvatar>();
		Transform landsObject = m_GameObjectSceneRoot.transform.Find("lands");
		m_map.OnEnable(this, component, landsObject, m_RootElement);
		m_holidayEventView = new zbHolidayEventView();
		m_holidayEventView.OnEnable(this, UQueryExtensions.Q<VisualElement>(m_RootElement, "holidayEvent", (string)null));
		m_btnNextTarget = UQueryExtensions.Q<Button>(m_RootElement, "btn_nextTarget", (string)null);
		m_nodeTarget = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_target", (string)null);
		m_bgTarget = UQueryExtensions.Q<VisualElement>(m_nodeTarget, "bg_target", (string)null);
		m_bgGrand = UQueryExtensions.Q<VisualElement>(m_nodeTarget, "bg_grand", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>(m_nodeTarget, "lb_time", (string)null);
		for (int i = 0; i < 4; i++)
		{
			m_items[i] = UQueryExtensions.Q<zbItem>(m_nodeTarget, "item" + i, (string)null);
			m_items[i].OnEnable(this);
		}
		m_progress = UQueryExtensions.Q<VisualElement>(m_nodeTarget, "progressVal", (string)null);
		m_lbProgress = UQueryExtensions.Q<Label>(m_nodeTarget, "lb_progress", (string)null);
		m_spriteScore = UQueryExtensions.Q<VisualElement>(m_nodeTarget, "sprite_score", (string)null);
		m_spineProgress = UQueryExtensions.Q<SpineVisualElement>(m_nodeTarget, "spine_progress", (string)null);
		m_btnScore = UQueryExtensions.Q<Button>(m_nodeTarget, "btn_score", (string)null);
		m_btnTarget = UQueryExtensions.Q<Button>(m_nodeTarget, "btn_target", (string)null);
		m_notifyTarget = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnTarget, (Vector2?)new Vector2(3f, 5f), 1f, true);
		m_notifyTarget.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_btnThemeTask = UQueryExtensions.Q<Button>(m_RootElement, "btn_themeTask", (string)null);
		m_notifyThemeTask = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnThemeTask, (Vector2?)new Vector2(-3f, -3f), 1f, true);
		m_addBg = UQueryExtensions.Q<VisualElement>(m_RootElement, "addBg", (string)null);
		m_lbCount = UQueryExtensions.Q<Label>(m_addBg, "lb_count", (string)null);
		m_btnGo = UQueryExtensions.Q<Button>(m_addBg, "btn_go", (string)null);
		m_btnAdd = UQueryExtensions.Q<Button>(m_addBg, "btn_add", (string)null);
		m_btnBatch = UQueryExtensions.Q<Button>(m_addBg, "btn_batch", (string)null);
		m_spine = UQueryExtensions.Q<SpineVisualElement>(m_addBg, "spine", (string)null);
		m_spineBig = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "spineBig", (string)null);
		((VisualElement)m_spineBig).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_nodeSpeed = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_speed", (string)null);
		m_spriteOk = UQueryExtensions.Q<VisualElement>(m_nodeSpeed, "sprite_ok", (string)null);
		m_btnRule = UQueryExtensions.Q<Button>(m_RootElement, "btn_rule", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnNextTarget, (Action)OnNextTarget, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnScore, (Action)OnScore, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTarget, (Action)OnTarget, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnThemeTask, (Action)OnThemeTask, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGo, (Action)OnGo, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAdd, (Action)OnAdd, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBatch, (Action)OnBatch, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>(m_nodeSpeed, (Action)OnSpeed, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRule, (Action)OnRule, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		CreateMoneyPanel(new EMoneyType[1] { EMoneyType.Diamond });
		m_gameEevent.RegisterEvent(EGameEvent.Event_SnowVeilGo_Ack, OnSnowVeilGoAck);
		m_gameEevent.RegisterEvent(EGameEvent.Event_SnowVeilTarget_Ack, OnSnowVeilTargetAck);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Prop, delegate
		{
			RefreshCount();
		});
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_SnowVeilDetail, OnUpdateEvent);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Operate, OnUpdateEvent);
	}

	public void RefreshCustom(SnowVeilDetail detail)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		m_isGoCooling = false;
		zbSelf.Instance.holidayHandler.GetHolidayCount();
		if (detail != null)
		{
			m_holidayEventView.Refresh(detail.BaseInfo.PlanId);
		}
		m_themeTaskDetail = zbSelf.Instance.holidayHandler.GetThemeTaskDetail();
		((VisualElement)m_btnThemeTask).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_themeTaskDetail == null));
		m_notifyThemeTask.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_themeTaskDetail == null || !zbSelf.Instance.operateHandler.HasThemeTaskNotify(m_themeTaskDetail.BaseInfo.UUID)) ? 1 : 0));
		if (detail != null)
		{
			m_data = detail;
			RefreshSpeed();
			RefreshCount();
			RefreshTarget();
			RefreshTime();
			m_map.Refresh();
			RefreshBatch();
		}
	}

	private void RefreshSpeed()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		m_nodeSpeed.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_data.TargetDetail.Id < 7));
		m_isSpeed = zbSetSave.Instance.GetSnowVeilSpeed() == 1;
		m_speed = (m_isSpeed ? 2f : 1f);
		m_spriteOk.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!m_isSpeed));
	}

	private void RefreshCount()
	{
		m_propCount = zbSelf.Instance.itemHandler.GetCountByPropTid(m_data.Prop.PropId);
		((TextElement)m_lbCount).text = m_propCount.ToString();
	}

	public void RefreshTarget()
	{
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		if (m_isGetPropsAck || m_data == null)
		{
			return;
		}
		if (m_data.TargetNextGrand.CurrentIndex < m_data.TargetNextGrand.GrandIndex)
		{
			((VisualElement)m_btnNextTarget).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((TextElement)m_btnNextTarget).text = m_data.TargetNextGrand.CurrentIndex + "/" + m_data.TargetNextGrand.GrandIndex;
			m_bgTarget.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_bgGrand.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_lbTime).style.top = StyleLength.op_Implicit(-55f);
		}
		else
		{
			((VisualElement)m_btnNextTarget).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_bgTarget.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_bgGrand.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_lbTime).style.top = StyleLength.op_Implicit(-63f);
		}
		for (int i = 0; i < 4; i++)
		{
			((VisualElement)m_items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		int num = 0;
		foreach (AwardDetail award in m_data.TargetDetail.AwardList)
		{
			if (num < 4)
			{
				m_items[num].Refresh(new zbItemData(award.Type, award.Tid, award.Count));
				((VisualElement)m_items[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			}
			num++;
		}
		int totalScore = m_data.TotalScore;
		int costScore = m_data.TargetDetail.CostScore;
		((TextElement)m_lbProgress).text = $"{totalScore}/{costScore}";
		float num2 = Mathf.Clamp((float)totalScore / (float)costScore, 0f, 1f);
		m_progress.style.width = StyleLength.op_Implicit(new Length(num2 * 316f, (LengthUnit)0));
		((VisualElement)m_btnTarget).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((totalScore < costScore) ? 1 : 0));
	}

	private void OnNextTarget()
	{
		if (m_data != null)
		{
			int num = m_data.TargetNextGrand.GrandIndex - m_data.TargetNextGrand.CurrentIndex;
			zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.11184"), num));
		}
	}

	private void OnScore()
	{
		if (m_data != null)
		{
			int num = m_data.TargetDetail.CostScore - m_data.TotalScore;
			if (num > 0)
			{
				zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.11189"), num));
			}
		}
	}

	private void OnTarget()
	{
		if (m_data != null)
		{
			zbWsMsgHandler.Instance.Operate_SnowVeilTarget_Req(m_data.BaseInfo.UUID);
		}
	}

	private void OnThemeTask()
	{
		if (m_themeTaskDetail != null)
		{
			zbUIMgr.Instance.ShowThemeTask(m_themeTaskDetail);
		}
	}

	private void OnGo()
	{
		if (!m_isGoCooling && !m_isGetPropsAck && m_data != null)
		{
			if (!zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Prop, m_data.Prop.PropId, 1))
			{
				zbUIMgr.Instance.ShowTip(EResourceType.Prop, m_data.Prop.PropId, 0L);
				return;
			}
			zbWsMsgHandler.Instance.Operate_SnowVeilGo_Req(m_data.BaseInfo.UUID, m_batchCount);
			((MonoBehaviour)this).StartCoroutine(GoCooldown());
		}
	}

	private IEnumerator GoCooldown()
	{
		m_isGoCooling = true;
		((VisualElement)m_btnGo).SetEnabled(false);
		yield return (object)new WaitForSeconds(1f);
		m_isGoCooling = false;
		((VisualElement)m_btnGo).SetEnabled(true);
	}

	private void OnAdd()
	{
		if (m_data != null)
		{
			zbUIMgr.Instance.ShowTip(EResourceType.Prop, m_data.Prop.PropId, 0L);
		}
	}

	private void RefreshBatch()
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		if (m_batchCount > m_propCount)
		{
			m_batchCount = 1;
			for (int num = BATCHCOUNT.Length - 1; num >= 0; num--)
			{
				if (BATCHCOUNT[num] <= m_propCount)
				{
					m_batchCount = BATCHCOUNT[num];
					break;
				}
			}
		}
		((VisualElement)m_btnBatch).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		((VisualElement)m_btnBatch).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>($"ui/image/vip/bar/bar_score{m_batchCount}.png"));
	}

	private void OnBatch()
	{
		int num = Array.IndexOf(BATCHCOUNT, m_batchCount);
		if (num < 0)
		{
			num = 0;
		}
		for (int i = 1; i <= BATCHCOUNT.Length; i++)
		{
			int num2 = (num + i) % BATCHCOUNT.Length;
			int num3 = BATCHCOUNT[num2];
			if (num3 <= m_propCount)
			{
				m_batchCount = num3;
				break;
			}
		}
		RefreshBatch();
	}

	private void OnSpeed()
	{
		m_isSpeed = !m_isSpeed;
		zbSetSave.Instance.SetSnowVeilSpeed(m_isSpeed ? 1 : 0);
		RefreshSpeed();
	}

	private void OnRule()
	{
		string[] langKeys = new string[4] { "lang.11451", "lang.11452", "lang.11453", "lang.11454" };
		zbUIMgr.Instance.ShowRule(langKeys);
	}

	private void OnClose()
	{
		Hide();
	}

	private void RefreshTime()
	{
		if (m_data != null)
		{
			((TextElement)m_lbTime).text = zbUtil.FormatTimeSmart(m_data.BaseInfo.EndTm - zbSelf.Instance.timeHandler.GetServerTime());
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

	public void OnUpdateEvent(object obj)
	{
		SnowVeilDetail snowVeilByUUID = zbSelf.Instance.operateHandler.GetSnowVeilByUUID(m_data.BaseInfo.UUID);
		RefreshCustom(snowVeilByUUID);
	}

	private void OnSnowVeilGoAck(object data)
	{
		m_isGetPropsAck = true;
		if (m_goAckCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(m_goAckCoroutine);
			m_goAckCoroutine = null;
		}
		m_goAckCoroutine = ((MonoBehaviour)this).StartCoroutine(SpineAnimation());
	}

	private IEnumerator SpineAnimation()
	{
		zbAudioMgr.Instance.PlayAudio("audio/ui/ui_wawajikaishi.ogg");
		m_spineBig.AnimationState.TimeScale = m_speed;
		m_spine.AnimationState.TimeScale = m_speed;
		((VisualElement)m_spineBig).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_spineBig.AnimationState.SetAnimation(0, "act" + zbSelf.Instance.snowVeilHandler.m_goAckStep, false);
		m_spineBig.AnimationState.AddAnimation(0, "act" + zbSelf.Instance.snowVeilHandler.m_goAckStep + "_1", true, 0f);
		m_spine.AnimationState.SetAnimation(0, "act" + zbSelf.Instance.snowVeilHandler.m_goAckStep, false);
		m_spine.AnimationState.AddAnimation(0, "act" + zbSelf.Instance.snowVeilHandler.m_goAckStep + "_1", true, 0f);
		yield return (object)new WaitForSeconds(19f / 30f / m_speed);
		SnowVeilDetail snowVeilDetail = zbSelf.Instance.snowVeilHandler.GetSnowVeilDetail();
		RefreshCustom(snowVeilDetail);
	}

	private void OnSnowVeilTargetAck(object data)
	{
		SnowVeilDetail snowVeilDetail = zbSelf.Instance.snowVeilHandler.GetSnowVeilDetail();
		RefreshCustom(snowVeilDetail);
		_ = data is OperateSnowVeilTargetAck;
	}
}
