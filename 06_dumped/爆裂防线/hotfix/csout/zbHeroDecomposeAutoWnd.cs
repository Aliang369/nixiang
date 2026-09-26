using System;
using System.Collections.Generic;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbHeroDecomposeAutoWnd : zbBaseWnd
{
	private Button[] m_btnRaritys = (Button[])(object)new Button[2];

	private VisualElement[] m_spriteOks = (VisualElement[])(object)new VisualElement[2];

	private Button m_btnCancel;

	private Button m_btnConfirm;

	private Button m_btnClose;

	protected override void OnEnable()
	{
		base.OnEnable();
		for (int i = 0; i < 2; i++)
		{
			m_btnRaritys[i] = UQueryExtensions.Q<Button>(m_RootElement, "btn_rarity" + i, (string)null);
			m_spriteOks[i] = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btnRaritys[i], "sprite_ok", (string)null);
			int curIndex = i;
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRaritys[i], (Action<ClickEvent>)delegate
			{
				OnRaritys(curIndex);
			}, "", (TrickleDown)0);
		}
		m_btnCancel = UQueryExtensions.Q<Button>(m_RootElement, "btn_cancel", (string)null);
		m_btnConfirm = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCancel, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnConfirm, (Action)OnConfirm, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		AddDark();
	}

	public override void Refresh()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		m_spriteOks[0].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_spriteOks[1].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
	}

	private void OnRaritys(int index)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		bool flag = m_spriteOks[index].style.display == StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_spriteOks[index].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag ? 1 : 0));
	}

	private void OnConfirm()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		List<int> list = new List<int>();
		for (int i = 0; i < 2; i++)
		{
			if (m_spriteOks[i].style.display == StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0))
			{
				list.Add(i + 1);
			}
		}
		List<ResourceDetail> heroFragListByRarity = zbSelf.Instance.heroHandler.GetHeroFragListByRarity(list);
		List<ResourceDetail> list2 = new List<ResourceDetail>();
		foreach (ResourceDetail item in heroFragListByRarity)
		{
			if (list2.Count < 6)
			{
				list2.Add(item);
			}
		}
		if (list2.Count <= 0)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10241"));
		}
		else
		{
			zbHeroDecomposeWnd zbHeroDecomposeWnd2 = (zbHeroDecomposeWnd)zbUIMgr.Instance.GetWndById(EWndId.HeroDecompose);
			if ((Object)(object)zbHeroDecomposeWnd2 != (Object)null)
			{
				zbHeroDecomposeWnd2.decomposeList.Clear();
				foreach (ResourceDetail item2 in list2)
				{
					zbHeroDecomposeWnd2.decomposeList.Add(item2);
				}
				zbHeroDecomposeWnd2.RefreshDecomposeList();
			}
		}
		OnClose();
	}

	private void OnClose()
	{
		Hide();
	}
}
