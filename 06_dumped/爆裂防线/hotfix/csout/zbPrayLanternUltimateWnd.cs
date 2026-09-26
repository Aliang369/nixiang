using System;
using Proto.Types;
using Spine.Unity;
using UnityEngine.UIElements;

public class zbPrayLanternUltimateWnd : zbBaseWnd
{
	private Button m_btnTouch;

	private VisualElement[] m_ultimates = (VisualElement[])(object)new VisualElement[3];

	private SpineVisualElement[] m_spines = (SpineVisualElement[])(object)new SpineVisualElement[3];

	private zbItem[] m_items = new zbItem[3];

	private Button[] m_btnSelects = (Button[])(object)new Button[3];

	private VisualElement[] m_spriteOks = (VisualElement[])(object)new VisualElement[3];

	private Button m_btnConfirm;

	private int m_selectIndex = -1;

	private PrayLanternDetail m_data;

	private UltimateAwardDetail m_ultimateAward;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnTouch, "", (TrickleDown)0);
		for (int i = 0; i < 3; i++)
		{
			m_ultimates[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "item" + i, (string)null);
			m_spines[i] = UQueryExtensions.Q<SpineVisualElement>(m_ultimates[i], "spine", (string)null);
			m_items[i] = UQueryExtensions.Q<zbItem>(m_ultimates[i], "item", (string)null);
			m_items[i].OnEnable(this);
			m_btnSelects[i] = UQueryExtensions.Q<Button>(m_ultimates[i], "btn_select", (string)null);
			m_spriteOks[i] = UQueryExtensions.Q<VisualElement>(m_ultimates[i], "sprite_ok", (string)null);
			int curIndex = i;
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSelects[curIndex], (Action<ClickEvent>)delegate(ClickEvent evt)
			{
				OnSelects(evt, curIndex);
			}, "", (TrickleDown)0);
		}
		m_btnConfirm = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnConfirm, (Action)OnConfirm, "", (TrickleDown)0);
		AddDark();
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Operate, OnEventUpdate);
	}

	public void RefreshCustom(PrayLanternDetail data)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		if (data == null)
		{
			return;
		}
		m_data = data;
		for (int i = 0; i < 3; i++)
		{
			m_ultimates[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		int num = 0;
		foreach (UltimateAwardDetail ultimateAward in data.Ultimate.UltimateAwardList)
		{
			if (num < m_ultimates.Length)
			{
				m_ultimates[num].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				zbItemData data2 = new zbItemData(ultimateAward.Award.Type, ultimateAward.Award.Tid, ultimateAward.Award.Count);
				m_items[num].Refresh(data2);
			}
			num++;
		}
		m_ultimateAward = zbSelf.Instance.operateHandler.GetUltimateAward(data.BaseInfo.UUID);
		if (m_ultimateAward != null)
		{
			m_selectIndex = m_ultimateAward.Index - 1;
		}
		else
		{
			m_selectIndex = -1;
		}
		RefreshSelect();
	}

	public void RefreshSelect()
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 3; i++)
		{
			if (i == m_selectIndex)
			{
				m_spines[i].AnimationState.SetAnimation(0, "sel", true);
			}
			else
			{
				m_spines[i].AnimationState.SetAnimation(0, "unsel", true);
			}
			m_spriteOks[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((i != m_selectIndex) ? 1 : 0));
		}
	}

	private void OnSelects(ClickEvent evt, int index)
	{
		m_selectIndex = index;
		RefreshSelect();
	}

	private void OnConfirm()
	{
		if (m_selectIndex < 0)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10378"));
		}
		else if (m_data != null)
		{
			zbWsMsgHandler.Instance.Operate_PrayLanternSelect_Req(m_data.BaseInfo.UUID, m_selectIndex + 1);
		}
	}

	private void OnTouch()
	{
		Hide();
	}

	private void OnEventUpdate(object data)
	{
		Hide();
	}
}
