using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class zbCombatWnd_Legendary
{
	private zbCombatWnd m_wndParent;

	private VisualElement m_RootElement;

	private Label m_lbLegendary;

	private Button m_btnLegendary;

	private Coroutine m_autoHideCoroutine;

	public void OnEnable(zbCombatWnd wndParent, VisualElement root)
	{
		m_wndParent = wndParent;
		m_RootElement = root;
		m_lbLegendary = UQueryExtensions.Q<Label>(m_RootElement, "lb_legendary", (string)null);
		m_btnLegendary = UQueryExtensions.Q<Button>(m_RootElement, "btn_legendary", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnLegendary, (Action)OnLegendary, "", (TrickleDown)0);
	}

	public void Refresh()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		int legendaryEffectId = GetLegendaryEffectId();
		if (legendaryEffectId <= 0)
		{
			m_RootElement.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			return;
		}
		zbLegendaryEffectCsvRecord recordByKey = zbDataConfig.Instance.legendaryEffectCsvTable.GetRecordByKey(legendaryEffectId);
		if (recordByKey == null)
		{
			m_RootElement.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			return;
		}
		m_RootElement.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		((TextElement)m_lbLegendary).text = recordByKey.desc;
		OnLegendary();
	}

	private int GetLegendaryEffectId()
	{
		return zbCombatSystem.Instance.battleInfo.MonsterInfo.ExtraInfo.LegendaryEffect;
	}

	public void OnLegendary()
	{
		if (m_autoHideCoroutine != null)
		{
			((MonoBehaviour)m_wndParent).StopCoroutine(m_autoHideCoroutine);
		}
		m_autoHideCoroutine = ((MonoBehaviour)m_wndParent).StartCoroutine(AutoHide());
	}

	private IEnumerator AutoHide()
	{
		((VisualElement)m_lbLegendary).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		yield return (object)new WaitForSeconds(3f);
		((VisualElement)m_lbLegendary).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
	}
}
