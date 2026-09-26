using System;
using System.Collections;
using System.Collections.Generic;
using Proto.Types;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbBarrageCmdWnd : zbBaseWnd
{
	private Button m_btnTouch;

	private SpineVisualElement m_spine;

	private VisualElement m_bg;

	private Button m_btnClose;

	private VisualElement barrageContainer;

	private zbItem m_item;

	private Label m_lbCount;

	private Button m_btnGet;

	private VisualElement m_notify;

	private BarrageCmdDetail m_data;

	private zbOperateCsvRecord m_record;

	private float showTime;

	private bool selfAction;

	private float barrageInterval = 1.5f;

	private List<BarrageItem> barrageList = new List<BarrageItem>();

	private Queue<BarrageItem> barragePool = new Queue<BarrageItem>();

	private int maxPoolSize = 50;

	private bool m_isClosing;

	protected override void Awake()
	{
		base.Awake();
		m_item = new zbItem();
	}

	protected override void OnEnable()
	{
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_spine = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "spine", (string)null);
		m_bg = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		barrageContainer = UQueryExtensions.Q<VisualElement>(m_RootElement, "BarrageContainer", (string)null);
		m_item = UQueryExtensions.Q<zbItem>(m_RootElement, "item", (string)null);
		m_item.OnEnable(this);
		m_lbCount = UQueryExtensions.Q<Label>(m_RootElement, "lb_count", (string)null);
		m_btnGet = UQueryExtensions.Q<Button>(m_RootElement, "btn_get", (string)null);
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnGet, (Vector2?)new Vector2(-3f, -5f), 1f, true);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGet, (Action<ClickEvent>)OnGet, "", (TrickleDown)0);
		AddDark();
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Operate, OnEventUpdate);
	}

	public void RefreshCustom(string uuid, zbOperateCsvRecord record)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		m_isClosing = false;
		m_data = zbSelf.Instance.operateHandler.GetRedBagInfoByUUID(uuid);
		m_record = record;
		RefreshView();
		m_bg.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_spine.AnimationState.SetAnimation(0, "in", false);
		m_spine.AnimationState.AddAnimation(0, "loop", true, 0f);
		((MonoBehaviour)this).StartCoroutine(DelayShowBg());
	}

	private IEnumerator DelayShowBg()
	{
		yield return (object)new WaitForSeconds(0.4f);
		m_bg.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
	}

	private void OnEventUpdate(object data)
	{
		RefreshCustom(m_data.BaseInfo.UUID, m_record);
	}

	private void RefreshView()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)m_lbCount).text = string.Format("{0}: {1}", zbUtil.Language("lang.10159"), m_data.LimitNum - m_data.Count);
		if (m_data.LimitNum <= m_data.Count)
		{
			((TextElement)m_btnGet).text = zbUtil.Language("lang.10148");
			m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		else
		{
			((TextElement)m_btnGet).text = zbUtil.Language("lang.10090");
			m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		zbItemData data = new zbItemData(m_data.AwardList[0].Type, m_data.AwardList[0].Tid, m_data.AwardList[0].Count);
		m_item.Refresh(data);
		SpawnRandomBarrage(zbUtil.Language("lang.10995"));
		SpawnRandomBarrage(zbUtil.Language("lang.10996"));
	}

	private void SpawnRandomBarrage(string content, bool isSelf = false)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected O, but got Unknown
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		BarrageItem barrageItem;
		if (barragePool.Count > 0)
		{
			barrageItem = barragePool.Dequeue();
			((TextElement)barrageItem.label).text = content;
		}
		else
		{
			Label val = new Label(content);
			((VisualElement)val).pickingMode = (PickingMode)1;
			((VisualElement)val).style.position = StyleEnum<Position>.op_Implicit((Position)1);
			((VisualElement)val).style.whiteSpace = StyleEnum<WhiteSpace>.op_Implicit((WhiteSpace)1);
			((VisualElement)val).style.unityFontStyleAndWeight = StyleEnum<FontStyle>.op_Implicit((FontStyle)1);
			barrageItem = new BarrageItem
			{
				label = val
			};
		}
		((VisualElement)barrageItem.label).style.color = StyleColor.op_Implicit(isSelf ? Color.green : Color.white);
		((VisualElement)barrageItem.label).style.fontSize = StyleLength.op_Implicit((float)(isSelf ? 28 : 24));
		float posX = ((barrageContainer.resolvedStyle.width > 0f) ? barrageContainer.resolvedStyle.width : 720f);
		int max = Mathf.FloorToInt(((barrageContainer.resolvedStyle.height > 0f) ? barrageContainer.resolvedStyle.height : 400f) / 30f);
		float posY = Rand.Range(0, max) * 30 + 5;
		barrageItem.posX = posX;
		barrageItem.posY = posY;
		barrageItem.speed = Rand.Range(100f, 200f);
		((VisualElement)barrageItem.label).style.left = StyleLength.op_Implicit(new Length(barrageItem.posX, (LengthUnit)0));
		((VisualElement)barrageItem.label).style.top = StyleLength.op_Implicit(new Length(barrageItem.posY, (LengthUnit)0));
		barrageContainer.Add((VisualElement)(object)barrageItem.label);
		barrageList.Add(barrageItem);
	}

	protected override void UpdateAtShow()
	{
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		float deltaTime = Time.deltaTime;
		showTime += deltaTime;
		if (showTime >= barrageInterval)
		{
			showTime = 0f;
			if (selfAction)
			{
				RefreshSelf();
				selfAction = false;
			}
			else if (m_data?.LangList != null && m_data.LangList.Count > 0)
			{
				int num = Rand.Range(0, m_data.LangList.Count);
				BarrageCmdLangDetail barrageCmdLangDetail = m_data.LangList[num];
				SpawnRandomBarrage(barrageCmdLangDetail.Content);
			}
		}
		_ = barrageContainer.resolvedStyle.width;
		for (int num2 = barrageList.Count - 1; num2 >= 0; num2--)
		{
			BarrageItem barrageItem = barrageList[num2];
			barrageItem.posX -= barrageItem.speed * deltaTime;
			if (barrageItem.posX < 0f - ((VisualElement)barrageItem.label).resolvedStyle.width)
			{
				if (barrageContainer.Contains((VisualElement)(object)barrageItem.label))
				{
					barrageContainer.Remove((VisualElement)(object)barrageItem.label);
				}
				barrageList.RemoveAt(num2);
				if (barragePool.Count < maxPoolSize)
				{
					((TextElement)barrageItem.label).text = "";
					barragePool.Enqueue(barrageItem);
				}
			}
			else
			{
				((VisualElement)barrageItem.label).style.left = StyleLength.op_Implicit(new Length(barrageItem.posX, (LengthUnit)0));
			}
		}
	}

	private void OnGet(ClickEvent evt)
	{
		if (m_data.LimitNum <= m_data.Count)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10160"));
			return;
		}
		selfAction = true;
		zbWsMsgHandler.Instance.Operate_BarrageCmd_Req(m_data.BaseInfo.UUID, zbLanguage.Instance.GetLanguageType());
	}

	private void RefreshSelf()
	{
		string text = "";
		foreach (BarrageCmdLangDetail lang in m_data.LangList)
		{
			if (lang.Lang == zbLanguage.Instance.GetLanguageType())
			{
				text = lang.Content;
				break;
			}
		}
		if (string.IsNullOrEmpty(text) && m_data.LangList.Count > 0)
		{
			text = m_data.LangList[0].Content;
		}
		if (!string.IsNullOrEmpty(text))
		{
			SpawnRandomBarrage(text, isSelf: true);
		}
	}

	private void OnClose(ClickEvent evt)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		if (!m_isClosing)
		{
			m_isClosing = true;
			m_bg.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_spine.AnimationState.ClearTrack(0);
			m_spine.AnimationState.SetAnimation(0, "out", false).Complete += new TrackEntryDelegate(OnOutAnimationComplete);
		}
	}

	private void OnOutAnimationComplete(TrackEntry trackEntry)
	{
		if (trackEntry.Animation.Name == "out")
		{
			Hide();
		}
	}
}
