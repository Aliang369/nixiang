using System;
using System.Collections;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbSnowVeilRollingWnd : zbBaseWnd
{
	private VisualElement[] m_bgScrolls = (VisualElement[])(object)new VisualElement[5];

	private zbItem[] m_items = new zbItem[5];

	private SpineVisualElement m_spine;

	private zbItem m_itemAward;

	private Button m_btnRolling;

	private Label m_lbFree;

	private Label m_lbDiamond;

	private Button m_btnGet;

	private Button m_btnSkip;

	private VisualElement m_spriteOk;

	private Button m_btnClose;

	private VisualElement m_notifyRolling;

	private VisualElement m_notifyGet;

	private const float ICON_HEIGHT = 128f;

	private const float WHEEL_RUN_TIME = 2.5f;

	private SnowVeilDetail m_data;

	private int[] m_costList;

	private int m_costCount;

	private int m_costDiamond;

	private int[] m_finalStates = new int[5];

	private float[] m_targetYs = new float[5];

	private int[] m_prevStates = new int[5];

	private bool[] m_needSpin = new bool[5];

	private bool[] m_allFinish = new bool[5];

	private Coroutine[] m_spinCoroutines = (Coroutine[])(object)new Coroutine[5];

	private bool m_isAllFinished;

	private bool m_skipAnim;

	private bool m_isRolling;

	protected override void OnEnable()
	{
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		for (int i = 0; i < 5; i++)
		{
			m_bgScrolls[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "scroll" + i, (string)null);
			m_items[i] = UQueryExtensions.Q<zbItem>(m_bgScrolls[i], "item", (string)null);
			m_items[i].OnEnable(this);
		}
		m_spine = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "spine", (string)null);
		m_itemAward = UQueryExtensions.Q<zbItem>(m_RootElement, "item_award", (string)null);
		m_itemAward.OnEnable(this);
		m_btnRolling = UQueryExtensions.Q<Button>(m_RootElement, "btn_rolling", (string)null);
		m_lbFree = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnRolling, "lb_free", (string)null);
		m_lbDiamond = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnRolling, "lb_diamond", (string)null);
		m_btnGet = UQueryExtensions.Q<Button>(m_RootElement, "btn_get", (string)null);
		m_btnSkip = UQueryExtensions.Q<Button>(m_RootElement, "btn_skip", (string)null);
		m_spriteOk = UQueryExtensions.Q<VisualElement>(m_RootElement, "sprite_ok", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRolling, (Action)OnRolling, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGet, (Action)OnGet, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSkip, (Action)OnSkip, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_notifyRolling = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnRolling, (Vector2?)new Vector2(35f, -10f), 1f, true);
		m_notifyGet = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnGet);
		m_gameEevent.RegisterEvent(EGameEvent.Event_SnowVeilRolling_Ack, OnRollingAck);
		AddDark();
		InitRecord();
	}

	private void InitRecord()
	{
		JObject record = zbDataConfig.Instance.snowVeilBaseJsonTable.GetRecord();
		m_costList = record["rolling"][(object)"costDiamond"].ToObject<int[]>();
		m_costCount = m_costList.Length;
	}

	public void RefreshCustom(SnowVeilDetail data)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_030d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0344: Unknown result type (might be due to invalid IL or missing references)
		//IL_0361: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_spine).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		if (data == null)
		{
			return;
		}
		m_data = data;
		RefreshSkip();
		int num = 0;
		for (int i = 0; i < 5; i++)
		{
			m_items[i].Refresh(new zbItemData(m_data.Rolling.SingleAward.Type, m_data.Rolling.SingleAward.Tid, m_data.Rolling.SingleAward.Count));
			m_finalStates[i] = m_data.Rolling.RollingState[i];
			if (m_finalStates[i] == 1)
			{
				m_finalStates[i] = 8;
			}
			if (m_finalStates[i] == 7 && m_data.Rolling.RollingCount != 0)
			{
				num++;
			}
			m_targetYs[i] = CalcTargetY(m_finalStates[i]);
			m_bgScrolls[i].style.top = StyleLength.op_Implicit(m_targetYs[i]);
		}
		if (num == 0)
		{
			((VisualElement)m_btnRolling).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_btnGet).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		else
		{
			((VisualElement)m_btnGet).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			if (num == 5)
			{
				((VisualElement)m_btnRolling).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				((TextElement)m_btnGet).text = zbUtil.Language("lang.10090");
				((VisualElement)m_spine).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_spine.AnimationState.SetAnimation(0, "in", false);
				m_spine.AnimationState.AddAnimation(0, "loop", true, 0f);
			}
			else
			{
				((VisualElement)m_btnRolling).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				((TextElement)m_btnGet).text = zbUtil.Language("lang.11180");
			}
		}
		m_itemAward.Refresh(new zbItemData(m_data.Rolling.SingleAward.Type, m_data.Rolling.SingleAward.Tid, m_data.Rolling.SingleAward.Count * num));
		((VisualElement)m_itemAward).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(num <= 0));
		if (m_data.Rolling.RollingCount >= m_costCount)
		{
			m_costDiamond = m_costList[m_costCount - 1];
		}
		else
		{
			m_costDiamond = m_costList[m_data.Rolling.RollingCount];
		}
		((VisualElement)m_lbFree).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_costDiamond != 0) ? 1 : 0));
		((VisualElement)m_lbDiamond).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_costDiamond <= 0));
		((TextElement)m_lbDiamond).text = m_costDiamond.ToString();
		m_notifyRolling.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_costDiamond != 0) ? 1 : 0));
		m_notifyGet.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(num != 5));
	}

	private void RefreshSkip()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		m_skipAnim = zbSetSave.Instance.GetSnowVeilRolling() == 1;
		m_spriteOk.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!m_skipAnim));
	}

	private void OnRolling()
	{
		if (m_data != null && !m_isRolling)
		{
			for (int i = 0; i < 5; i++)
			{
				m_prevStates[i] = m_finalStates[i];
			}
			if (!zbSelf.Instance.moneyHandler.HasEnoughMoney(2, m_costDiamond))
			{
				zbUIMgr.Instance.ShowTip(EResourceType.Money, 2L, m_costDiamond);
				return;
			}
			m_isRolling = true;
			zbWsMsgHandler.Instance.Operate_SnowVeilRolling_Req(m_data.BaseInfo.UUID);
		}
	}

	private void OnGet()
	{
		if (m_isRolling)
		{
			ForceFinishAll();
		}
		else if (m_data != null)
		{
			zbWsMsgHandler.Instance.Operate_SnowVeilRollingEnd_Req(m_data.BaseInfo.UUID);
		}
	}

	private void OnSkip()
	{
		m_skipAnim = !m_skipAnim;
		zbSetSave.Instance.SetSnowVeilRolling(m_skipAnim ? 1 : 0);
		RefreshSkip();
		if (m_skipAnim && !m_isAllFinished)
		{
			ForceFinishAll();
		}
	}

	private void OnClose()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		ForceFinishAll();
		m_isRolling = false;
		if (((VisualElement)m_btnGet).style.display == StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0))
		{
			OnGet();
		}
		Hide();
	}

	private float CalcTargetY(int num)
	{
		return -128f * (float)(num - 1) + 26f;
	}

	private void OnRollingAck(object data)
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		if (!(data is OperateSnowVeilRollingAck))
		{
			return;
		}
		SnowVeilDetail snowVeilByUUID = zbSelf.Instance.operateHandler.GetSnowVeilByUUID(m_data.BaseInfo.UUID);
		for (int i = 0; i < 5; i++)
		{
			int num = snowVeilByUUID.Rolling.RollingState[i];
			if (num == 1)
			{
				num = 8;
			}
			m_finalStates[i] = num;
			if (m_prevStates[i] == 7)
			{
				m_needSpin[i] = false;
				float num2 = CalcTargetY(num);
				m_bgScrolls[i].style.top = StyleLength.op_Implicit(num2);
				m_allFinish[i] = true;
			}
			else
			{
				m_needSpin[i] = true;
				m_allFinish[i] = false;
			}
		}
		StartRolling();
	}

	private void StartRolling()
	{
		if (!m_isRolling)
		{
			return;
		}
		m_isAllFinished = false;
		for (int i = 0; i < m_allFinish.Length; i++)
		{
			m_allFinish[i] = false;
		}
		if (m_skipAnim)
		{
			ForceFinishAll();
			return;
		}
		zbAudioMgr.Instance.PlayAudio("audio/ui/ui_wawajikaishi.ogg");
		for (int j = 0; j < 5; j++)
		{
			if (!m_needSpin[j])
			{
				m_allFinish[j] = true;
				continue;
			}
			if (m_spinCoroutines[j] != null)
			{
				((MonoBehaviour)this).StopCoroutine(m_spinCoroutines[j]);
			}
			m_spinCoroutines[j] = ((MonoBehaviour)this).StartCoroutine(SpinReel(j, (float)j * 0.15f));
		}
		CheckAllFinished();
	}

	private void ForceFinishAll()
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < m_spinCoroutines.Length; i++)
		{
			if (m_spinCoroutines[i] != null)
			{
				((MonoBehaviour)this).StopCoroutine(m_spinCoroutines[i]);
				m_spinCoroutines[i] = null;
			}
		}
		for (int j = 0; j < 5; j++)
		{
			float num = CalcTargetY(m_finalStates[j]);
			m_bgScrolls[j].style.top = StyleLength.op_Implicit(num);
			m_allFinish[j] = true;
		}
		if (!m_isAllFinished)
		{
			CheckAllFinished();
		}
		else
		{
			m_isRolling = false;
		}
	}

	private IEnumerator SpinReel(int index, float delay)
	{
		if (delay > 0f)
		{
			yield return (object)new WaitForSecondsRealtime(delay);
		}
		VisualElement content = m_bgScrolls[index];
		float speed = 0f;
		float runTime = 0f;
		float totalHeight = 768f;
		while (runTime < 2.5f)
		{
			runTime += Time.unscaledDeltaTime;
			speed = Mathf.Min(speed + 2000f * Time.unscaledDeltaTime, 2000f);
			float num = content.resolvedStyle.top + speed * Time.unscaledDeltaTime;
			if (num >= 0f)
			{
				num -= totalHeight;
			}
			content.style.top = StyleLength.op_Implicit(num);
			yield return null;
		}
		float targetY = CalcTargetY(m_finalStates[index]);
		while (true)
		{
			float top = content.resolvedStyle.top;
			if (Mathf.Abs(top - targetY) < 2f)
			{
				break;
			}
			speed = Mathf.Max(speed - 3000f * Time.unscaledDeltaTime, 200f);
			float num2 = top + speed * Time.unscaledDeltaTime;
			if (num2 >= targetY)
			{
				num2 = targetY;
			}
			if (num2 >= 0f)
			{
				num2 -= totalHeight;
			}
			content.style.top = StyleLength.op_Implicit(num2);
			yield return null;
		}
		content.style.top = StyleLength.op_Implicit(targetY);
		m_allFinish[index] = true;
		CheckAllFinished();
	}

	private void CheckAllFinished()
	{
		if (m_isAllFinished)
		{
			return;
		}
		for (int i = 0; i < m_allFinish.Length; i++)
		{
			if (!m_allFinish[i])
			{
				return;
			}
		}
		m_isAllFinished = true;
		m_isRolling = false;
		SnowVeilDetail snowVeilByUUID = zbSelf.Instance.operateHandler.GetSnowVeilByUUID(m_data.BaseInfo.UUID);
		RefreshCustom(snowVeilByUUID);
	}
}
