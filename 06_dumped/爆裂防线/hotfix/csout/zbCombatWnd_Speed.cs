using System;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbCombatWnd_Speed
{
	private zbCombatWnd m_wndParent;

	private VisualElement m_RootElement;

	private Button m_btnTouch;

	private Button[] m_btnSpeeds = (Button[])(object)new Button[3];

	private VisualElement m_spriteLock;

	public void OnEnable(zbCombatWnd wndParent, VisualElement root)
	{
		m_wndParent = wndParent;
		m_RootElement = root;
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnTouch, "", (TrickleDown)0);
		for (int i = 0; i < 3; i++)
		{
			m_btnSpeeds[i] = UQueryExtensions.Q<Button>(m_RootElement, "btn_speed" + i, (string)null);
			int curIndex = i;
			m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSpeeds[i], (Action<ClickEvent>)delegate(ClickEvent evt)
			{
				OnSpeeds(evt, curIndex);
			}, "", (TrickleDown)0);
		}
		m_spriteLock = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btnSpeeds[2], "bg_lock", (string)null);
		m_wndParent.m_gameEevent.RegisterEvent(EGameEvent.Event_SuccessOrder_Ack, OnEventUpdate);
	}

	public void SetVisible(bool vis)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_wndParent.m_btnSpeed).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(vis ? 1 : 0));
		m_RootElement.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!vis));
		if (vis)
		{
			RefreshSpeeds();
		}
	}

	public void RefreshSpeeds()
	{
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		ECombatSpeed speed = zbCombatSystem.Instance.combatSpeed.GetSpeed();
		for (int i = 0; i < 3; i++)
		{
			int num = i + 1;
			string arg = (("X" + num == speed.ToString()) ? "1" : "");
			string resName = $"ui/image/button/button_speed{num}{arg}.png";
			((VisualElement)m_btnSpeeds[i]).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resName));
		}
		m_spriteLock.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbSelf.Instance.vipHandler.CheckVipFuncLimited(EVipFuncType.HighSpeed, zbSelf.Instance.GetVipLevel()) > 0));
	}

	public void OnTouch()
	{
		SetVisible(vis: false);
	}

	private void OnSpeeds(ClickEvent evt, int index)
	{
		ECombatSpeed eCombatSpeed = ECombatSpeed.X1;
		switch (index)
		{
		case 2:
			if (zbSelf.Instance.vipHandler.CheckVipFuncLimited(EVipFuncType.HighSpeed, zbSelf.Instance.GetVipLevel()) <= 0)
			{
				zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.11492"), zbSelf.Instance.vipHandler.CheckVipFuncUnlock(EVipFuncType.HighSpeed)));
				return;
			}
			eCombatSpeed = ECombatSpeed.X3;
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10683"));
			break;
		case 1:
			eCombatSpeed = ECombatSpeed.X2;
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10682"));
			break;
		default:
			eCombatSpeed = ECombatSpeed.X1;
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10681"));
			break;
		}
		zbCombatSystem.Instance.combatSpeed.SetSpeed(eCombatSpeed);
		RefreshSpeeds();
		m_wndParent.refreshSpeed();
		SetVisible(vis: false);
		zbProgressEventHelper.Instance.SetProgressTrack(604);
	}

	private void OnEventUpdate(object data)
	{
		RefreshSpeeds();
	}
}
