using UnityEngine;
using UnityEngine.UIElements;

public class zbPvpBalanceDamage
{
	private zbBaseWnd m_wndParent;

	private VisualElement m_rootElement;

	private zbPvpBalanceDamageSub m_damagePanelLeft;

	private zbPvpBalanceDamageSub m_damagePanelRight;

	public void OnEnable(zbBaseWnd wndParent, VisualElement root)
	{
		m_wndParent = wndParent;
		m_rootElement = root;
		VisualElement root2 = UQueryExtensions.Q<VisualElement>(m_rootElement, "leftDamage", (string)null);
		VisualElement root3 = UQueryExtensions.Q<VisualElement>(m_rootElement, "rightDamage", (string)null);
		m_damagePanelLeft = new zbPvpBalanceDamageSub();
		m_damagePanelRight = new zbPvpBalanceDamageSub();
		m_damagePanelLeft.OnEnable(m_wndParent, root2);
		m_damagePanelRight.OnEnable(m_wndParent, root3);
	}

	public void Refresh()
	{
		m_damagePanelLeft.Refresh(zbCombatSystem.Instance.combatData);
		if (zbCombatSystem.Instance.combatData2 != null)
		{
			m_damagePanelRight.Refresh(zbCombatSystem.Instance.combatData2);
		}
	}

	public void SetBackgroundImage(Sprite bg)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		m_rootElement.style.backgroundImage = new StyleBackground(bg);
	}
}
