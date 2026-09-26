using System;
using UnityEngine;
using UnityEngine.UIElements;

public class zbServantTabDevelop
{
	private zbServantWnd m_wndParent;

	private VisualElement m_RootElement;

	private VisualElement[] m_tabLayers = (VisualElement[])(object)new VisualElement[4];

	private Button[] m_tabBtns = (Button[])(object)new Button[4];

	private VisualElement[] m_tabBtnsNotify = (VisualElement[])(object)new VisualElement[4];

	private zbServantDevelopTabUpradeSkill m_modulUpgradeSkill;

	private zbServantDevelopTabFlush m_modulFlush;

	private zbServantDevelopTabUpgradeAttr m_modulUpgradeAttr;

	private zbServantDevelopTabUpradeSubSkill m_modulUpgradeSubSkill;

	public int m_curIndex;

	public void OnEnable(zbServantWnd wndParent, VisualElement root)
	{
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		m_RootElement = root;
		for (int i = 0; i < 4; i++)
		{
			m_tabLayers[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "develop" + i, (string)null);
			m_tabBtns[i] = UQueryExtensions.Q<Button>(m_RootElement, "tab_btn" + i, (string)null);
			m_tabBtnsNotify[i] = zbUtil.CreateNotifyTag((VisualElement)(object)m_tabBtns[i], (Vector2?)new Vector2(0f, 0f), 1f, true);
			int tempIndex = i;
			m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_tabBtns[i], (Action<ClickEvent>)delegate
			{
				OnSelect(tempIndex);
			}, "", (TrickleDown)0);
		}
	}

	public void Refresh()
	{
		OnSelect(m_curIndex);
	}

	private void OnSelect(int index = 0)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		m_curIndex = index;
		for (int i = 0; i < 4; i++)
		{
			bool flag = index == i;
			m_tabLayers[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			((VisualElement)m_tabBtns[i]).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>($"ui/image/servant/btn_servant_{i}_{(flag ? '0' : '1')}.png"));
		}
		switch (m_curIndex)
		{
		case 0:
			if (m_modulUpgradeSkill == null)
			{
				m_modulUpgradeSkill = new zbServantDevelopTabUpradeSkill();
				m_modulUpgradeSkill.OnEnable(m_wndParent, m_tabLayers[index]);
			}
			m_modulUpgradeSkill.Refresh();
			break;
		case 1:
			if (m_modulUpgradeAttr == null)
			{
				m_modulUpgradeAttr = new zbServantDevelopTabUpgradeAttr();
				m_modulUpgradeAttr.OnEnable(m_wndParent, m_tabLayers[index]);
			}
			m_modulUpgradeAttr.Refresh();
			break;
		case 2:
			if (m_modulUpgradeSubSkill == null)
			{
				m_modulUpgradeSubSkill = new zbServantDevelopTabUpradeSubSkill();
				m_modulUpgradeSubSkill.OnEnable(m_wndParent, m_tabLayers[index]);
			}
			m_modulUpgradeSubSkill.Refresh();
			break;
		case 3:
			if (m_modulFlush == null)
			{
				m_modulFlush = new zbServantDevelopTabFlush();
				m_modulFlush.OnEnable(m_wndParent, m_tabLayers[index]);
			}
			m_modulFlush.Refresh();
			break;
		}
		RefreshNotify();
	}

	private void RefreshNotify()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		m_tabBtnsNotify[0].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_curIndex == 0 || !zbSelf.Instance.servantHandler.CanServantUpgradeMainSkill(m_wndParent.m_detail)) ? 1 : 0));
		m_tabBtnsNotify[1].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_curIndex == 1 || !zbSelf.Instance.servantHandler.CanServantUpgradeAttr(m_wndParent.m_detail)) ? 1 : 0));
		m_tabBtnsNotify[2].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_curIndex == 2 || !zbSelf.Instance.servantHandler.CanServantUpgradeSubSkill(m_wndParent.m_detail)) ? 1 : 0));
	}
}
