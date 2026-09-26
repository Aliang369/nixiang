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

public class zbSnowVeilTurntableWnd : zbBaseWnd
{
	private zbItem[] m_items = new zbItem[8];

	private VisualElement m_spritePoint;

	private SpineVisualElement m_spine;

	private Button m_btnTurn;

	private Label m_lbFree;

	private Label m_lbDiamond;

	private Button m_btnSkip;

	private VisualElement m_spriteOk;

	private Button m_btnClose;

	private Button m_btnClose2;

	private SnowVeilDetail m_data;

	private int[] m_costList;

	private int m_costCount;

	private int m_costDiamond;

	private float m_preAngle;

	private Coroutine m_turnCoroutine;

	private bool m_skipAnim;

	private OperateSnowVeilTurntableAck m_ack;

	private bool m_isTurning;

	private static float startReqTime;

	protected override void OnEnable()
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		for (int i = 0; i < 8; i++)
		{
			m_items[i] = UQueryExtensions.Q<zbItem>(m_RootElement, "item" + i, (string)null);
			m_items[i].OnEnable(this);
		}
		m_spritePoint = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_point", (string)null);
		m_spine = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "spine", (string)null);
		((VisualElement)m_spine).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_btnTurn = UQueryExtensions.Q<Button>(m_RootElement, "btn_turn", (string)null);
		m_lbFree = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnTurn, "lb_free", (string)null);
		m_lbDiamond = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnTurn, "lb_diamond", (string)null);
		m_btnSkip = UQueryExtensions.Q<Button>(m_RootElement, "btn_skip", (string)null);
		m_spriteOk = UQueryExtensions.Q<VisualElement>(m_RootElement, "sprite_ok", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_btnClose2 = UQueryExtensions.Q<Button>(m_RootElement, "btn_close2", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTurn, (Action)OnTurn, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSkip, (Action)OnSkip, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose2, (Action)OnClose, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_SnowVeilTurntable_Ack, OnAck);
		AddDark();
		InitRecord();
	}

	private void InitRecord()
	{
		JObject record = zbDataConfig.Instance.snowVeilBaseJsonTable.GetRecord();
		m_costList = record["turntable"][(object)"cost"].ToObject<int[]>();
		m_costCount = m_costList.Length;
	}

	public void RefreshCustom(SnowVeilDetail data)
	{
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		if (data == null)
		{
			return;
		}
		m_data = data;
		RefreshSkip();
		bool flag = true;
		foreach (SnowVeilTurntableSlotDetail slot in m_data.Turntable.SlotList)
		{
			m_items[slot.Index - 1].Refresh(new zbItemData(slot.Award.Type, slot.Award.Tid, slot.Award.Count));
			m_items[slot.Index - 1].SetGettedVisible(slot.State == EBoolState.True);
			if (slot.State == EBoolState.False)
			{
				flag = false;
			}
		}
		((VisualElement)m_btnTurn).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag ? 1 : 0));
		((VisualElement)m_btnClose2).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		if (m_data.Turntable.RunCount >= m_costCount)
		{
			m_costDiamond = m_costList[m_costCount - 1];
		}
		else
		{
			m_costDiamond = m_costList[m_data.Turntable.RunCount];
		}
		if (m_costDiamond > 0)
		{
			((VisualElement)m_lbFree).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_lbDiamond).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((TextElement)m_lbDiamond).text = m_costDiamond.ToString();
		}
		else
		{
			((VisualElement)m_lbFree).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_lbDiamond).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		if (m_turnCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(m_turnCoroutine);
		}
		m_spritePoint.style.rotate = StyleRotate.op_Implicit(new Rotate(Angle.op_Implicit(m_preAngle)));
	}

	private void RefreshSkip()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		m_skipAnim = zbSetSave.Instance.GetSnowVeilTurntable() == 1;
		m_spriteOk.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!m_skipAnim));
	}

	private void OnTurn()
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		if (m_data != null && !m_isTurning)
		{
			if (!zbSelf.Instance.moneyHandler.HasEnoughMoney(2, m_costDiamond))
			{
				zbUIMgr.Instance.ShowTip(EResourceType.Money, 2L, m_costDiamond);
				return;
			}
			m_isTurning = true;
			startReqTime = Time.time;
			((VisualElement)m_spine).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			zbWsMsgHandler.Instance.Operate_SnowVeilTurntable_Req(m_data.BaseInfo.UUID);
		}
	}

	private void OnSkip()
	{
		m_skipAnim = !m_skipAnim;
		zbSetSave.Instance.SetSnowVeilTurntable(m_skipAnim ? 1 : 0);
		RefreshSkip();
		if (m_skipAnim && m_turnCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(m_turnCoroutine);
			m_turnCoroutine = null;
			if (m_ack != null)
			{
				PlayTurntableResultDirect(m_ack);
			}
		}
	}

	private void OnClose()
	{
		if (!m_isTurning)
		{
			Hide();
		}
	}

	private void OnAck(object data)
	{
		if (data is OperateSnowVeilTurntableAck ack)
		{
			m_ack = ack;
			if (m_skipAnim)
			{
				PlayTurntableResultDirect(ack);
			}
			else
			{
				m_turnCoroutine = ((MonoBehaviour)this).StartCoroutine(PlayTurntableAnimation(ack));
			}
		}
	}

	private void PlayTurntableResultDirect(OperateSnowVeilTurntableAck ack)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		float num = (float)(ack.Index - 1) * 45f + 22.5f;
		m_spritePoint.style.rotate = new StyleRotate(new Rotate(Angle.Degrees(num)));
		m_preAngle = num;
		((VisualElement)m_spine).style.rotate = new StyleRotate(new Rotate(Angle.Degrees(num - 22.5f)));
		((VisualElement)m_spine).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_spine.AnimationState.SetAnimation(0, "loop", true);
		zbServerData.sGameData.OperateInfo.SnowVeilList.Clear();
		zbServerData.sGameData.OperateInfo.SnowVeilList.AddRange((IEnumerable<SnowVeilDetail>)ack.SnowVeilList);
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_SnowVeilDetail);
		SnowVeilDetail snowVeilByUUID = zbSelf.Instance.operateHandler.GetSnowVeilByUUID(m_data.BaseInfo.UUID);
		RefreshCustom(snowVeilByUUID);
		zbSelf.Instance.dataHandler.DataChange(ack.DataChange, ((IEnumerable<AwardDetail>)ack.AwardList).ToArray());
		m_isTurning = false;
	}

	private IEnumerator PlayTurntableAnimation(OperateSnowVeilTurntableAck ack)
	{
		zbAudioMgr.Instance.PlayAudio("audio/ui/ui_wawajikaishi.ogg");
		int index = ack.Index;
		float targetAngle = (float)(index - 1) * 45f + 22.5f;
		float num = Mathf.Repeat(targetAngle - m_preAngle, 360f);
		float totalAngle = 1440f + num;
		float duration = 1.2f;
		float elapsed = 0f;
		float startAngle = m_preAngle;
		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			float num2 = EaseInOut(elapsed / duration);
			float num3 = startAngle + totalAngle * num2;
			m_spritePoint.style.rotate = new StyleRotate(new Rotate(Angle.Degrees(num3)));
			m_preAngle = num3 % 360f;
			yield return null;
		}
		m_spritePoint.style.rotate = new StyleRotate(new Rotate(Angle.Degrees(targetAngle)));
		m_preAngle = targetAngle;
		((VisualElement)m_spine).style.rotate = new StyleRotate(new Rotate(Angle.Degrees(targetAngle - 22.5f)));
		((VisualElement)m_spine).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_spine.AnimationState.SetAnimation(0, "in", false);
		m_spine.AnimationState.AddAnimation(0, "loop", true, 0f);
		zbServerData.sGameData.OperateInfo.SnowVeilList.Clear();
		zbServerData.sGameData.OperateInfo.SnowVeilList.AddRange((IEnumerable<SnowVeilDetail>)ack.SnowVeilList);
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_SnowVeilDetail);
		SnowVeilDetail snowVeilByUUID = zbSelf.Instance.operateHandler.GetSnowVeilByUUID(m_data.BaseInfo.UUID);
		RefreshCustom(snowVeilByUUID);
		zbSelf.Instance.dataHandler.DataChange(ack.DataChange, ((IEnumerable<AwardDetail>)ack.AwardList).ToArray());
		m_turnCoroutine = null;
		m_isTurning = false;
	}

	private float EaseInOut(float t)
	{
		if (!(t < 0.5f))
		{
			return 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
		}
		return 2f * t * t;
	}

	protected override void UpdatePerSecond()
	{
		if (m_isTurning && Time.time - startReqTime >= 3f)
		{
			m_isTurning = false;
		}
	}
}
