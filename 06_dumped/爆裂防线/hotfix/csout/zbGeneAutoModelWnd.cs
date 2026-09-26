using System;
using System.Collections;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbGeneAutoModelWnd : zbBaseWnd
{
	private Button m_btnTouch;

	private ESkillType m_skillType;

	private bool m_isClosing;

	private Action<bool> m_callback;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		AddDark();
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Gene, OnEventUpdate);
	}

	public void RefreshCustom(ESkillType skillType, Action<bool> callback)
	{
		m_skillType = skillType;
		m_callback = callback;
		m_isClosing = false;
		RefreshDetail();
	}

	private IEnumerator CoRefreshDetail()
	{
		yield return (object)new WaitForSeconds(0.5f);
		if (!m_isClosing)
		{
			RefreshDetail();
		}
	}

	private void RefreshDetail()
	{
		GeneResonanceResult geneResonanceResult = zbSelf.Instance.geneHandler.GetGeneResonanceResult(m_skillType);
		if (geneResonanceResult.MaxPotentialCount > 0 && zbSelf.Instance.geneHandler.geneShapeResonance[geneResonanceResult.MaxPotentialCount - 1] >= zbSelf.Instance.geneHandler.autoIndexs[zbSelf.Instance.geneHandler.selAutoIndex])
		{
			zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.10976"), zbSelf.Instance.geneHandler.geneShapeResonance[geneResonanceResult.MaxPotentialCount - 1] * 100f + "%"));
			OnClose();
		}
		else if (!zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Money, 19, zbSelf.Instance.geneHandler.geneRefreshCost))
		{
			zbUIMgr.Instance.ShowTip(EResourceType.Money, 19L, zbSelf.Instance.geneHandler.geneRefreshCost);
			OnClose();
		}
		else
		{
			zbWsMsgHandler.Instance.HeroAwaken_GeneRefreshHole_Req(m_skillType);
		}
	}

	private void OnClose()
	{
		if (!m_isClosing)
		{
			m_isClosing = true;
			zbSelf.Instance.geneHandler.selAutoIndex = -1;
			m_callback?.Invoke(obj: true);
			Hide();
		}
	}

	private void OnEventUpdate(object obj)
	{
		((MonoBehaviour)this).StartCoroutine(CoRefreshDetail());
	}
}
