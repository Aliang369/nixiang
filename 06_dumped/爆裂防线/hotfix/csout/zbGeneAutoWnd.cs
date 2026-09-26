using System;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbGeneAutoWnd : zbBaseWnd
{
	private Button[] m_btnPercents = (Button[])(object)new Button[3];

	private Label[] m_lbPercents = (Label[])(object)new Label[3];

	private VisualElement[] m_spriteOks = (VisualElement[])(object)new VisualElement[3];

	private Button m_btnConfirm;

	private Button m_btnClose;

	private ESkillType m_skillType;

	private Action<bool> m_callback;

	private int m_index;

	protected override void OnEnable()
	{
		base.OnEnable();
		for (int i = 0; i < 3; i++)
		{
			m_btnPercents[i] = UQueryExtensions.Q<Button>(m_RootElement, "btn_percent" + i, (string)null);
			m_lbPercents[i] = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnPercents[i], "lb_percent", (string)null);
			m_spriteOks[i] = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btnPercents[i], "sprite_ok", (string)null);
			int curIndex = i;
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnPercents[i], (Action<ClickEvent>)delegate
			{
				OnPercents(curIndex);
			}, "", (TrickleDown)0);
		}
		m_btnConfirm = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnConfirm, (Action)OnConfirm, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		AddDark();
	}

	public void RefreshCustom(ESkillType skillType, Action<bool> callback)
	{
		m_skillType = skillType;
		m_callback = callback;
		for (int i = 0; i < 3; i++)
		{
			((TextElement)m_lbPercents[i]).text = string.Format(zbUtil.Language("lang.10902"), GetCount(i)) + ": " + zbSelf.Instance.geneHandler.autoIndexs[i] * 100f + "%";
		}
		if (zbSelf.Instance.geneHandler.selAutoIndex > -1)
		{
			m_index = zbSelf.Instance.geneHandler.selAutoIndex;
		}
		else
		{
			m_index = -1;
		}
		RefreshSelect();
	}

	private int GetCount(int index)
	{
		float num = zbSelf.Instance.geneHandler.autoIndexs[index];
		for (int i = 0; i < zbSelf.Instance.geneHandler.geneShapeResonance.Length; i++)
		{
			if (Mathf.Abs(zbSelf.Instance.geneHandler.geneShapeResonance[i] - num) < 0.0001f)
			{
				return i + 1;
			}
		}
		return -1;
	}

	private void RefreshSelect()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 3; i++)
		{
			m_spriteOks[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_index != i) ? 1 : 0));
		}
		string resName = ((m_index == -1) ? "ui/image/button/btnGray_250x80.png" : "ui/image/button/btn_250_80_orange.png");
		((VisualElement)m_btnConfirm).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resName));
	}

	private void OnPercents(int index)
	{
		m_index = index;
		RefreshSelect();
	}

	private void OnConfirm()
	{
		if (m_index == -1)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10988"));
			return;
		}
		zbSelf.Instance.geneHandler.selAutoIndex = m_index;
		m_callback?.Invoke(obj: true);
		Hide();
		zbUIMgr.Instance.ShowGeneAutoModel(m_skillType, m_callback);
	}

	private void OnClose()
	{
		zbSelf.Instance.geneHandler.selAutoIndex = -1;
		m_callback?.Invoke(obj: true);
		Hide();
	}
}
