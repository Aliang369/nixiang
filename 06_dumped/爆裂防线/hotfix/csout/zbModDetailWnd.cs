using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class zbModDetailWnd : zbBaseWnd
{
	private VisualTreeAsset m_itemTA;

	private VisualTreeAsset m_itemTB;

	private Button m_btnTouch;

	private Button m_btnClose;

	private Label m_modName;

	private Label m_modDesc;

	private VisualElement m_spModIcon;

	private VisualElement m_require;

	private VisualElement m_conflict;

	private VisualElement m_hero;

	private ListView[] listViews = (ListView[])(object)new ListView[3];

	private Button[] m_btnWhys = (Button[])(object)new Button[3];

	private zbModHandbookCsvRecord m_modDetail;

	private List<int> requireMods = new List<int>();

	private List<int> conflictMods = new List<int>();

	private List<int[]> requireHeros = new List<int[]>();

	private zbTipElement m_tip;

	public zbTipElement zbTipElement => m_tip;

	protected override void OnEnable()
	{
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_itemTA = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/encyclopedia/tb_modDetailItem.uxml");
		m_itemTB = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/encyclopedia/tb_modDetailModItem.uxml");
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_modName = UQueryExtensions.Q<Label>(m_RootElement, "lb_modName", (string)null);
		m_modDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_modDesc", (string)null);
		m_spModIcon = UQueryExtensions.Q<VisualElement>(m_RootElement, "sp_modIcon", (string)null);
		m_require = UQueryExtensions.Q<VisualElement>(m_RootElement, "require", (string)null);
		m_conflict = UQueryExtensions.Q<VisualElement>(m_RootElement, "conflict", (string)null);
		m_hero = UQueryExtensions.Q<VisualElement>(m_RootElement, "hero", (string)null);
		m_btnWhys[0] = UQueryExtensions.Q<Button>(m_require, "btn_why", (string)null);
		m_btnWhys[1] = UQueryExtensions.Q<Button>(m_conflict, "btn_why", (string)null);
		m_btnWhys[2] = UQueryExtensions.Q<Button>(m_hero, "btn_why", (string)null);
		for (int i = 0; i < 3; i++)
		{
			int index = i;
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnWhys[i], (Action<ClickEvent>)delegate(ClickEvent evt)
			{
				string tipText = "";
				switch (index)
				{
				case 0:
					tipText = zbUtil.Language("lang.10836");
					break;
				case 1:
					tipText = zbUtil.Language("lang.10837");
					break;
				case 2:
					tipText = zbUtil.Language("lang.10838");
					break;
				}
				ShowTip(evt, tipText);
			}, "", (TrickleDown)0);
		}
		m_tip = UQueryExtensions.Q<zbTipElement>(m_RootElement, "tip", (string)null);
		m_tip.AutoHideDelay = 5f;
		Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/common/s9_tipBg.png");
		m_tip.BackgroundTexture = val.texture;
		m_tip.TipTextColor = Color.white;
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		InitListView();
		AddDark();
	}

	private void InitListView()
	{
		for (int i = 0; i < 3; i++)
		{
			listViews[i] = UQueryExtensions.Q<ListView>(m_RootElement, $"listView{i}", (string)null);
			UQueryExtensions.Q<ScrollView>((VisualElement)(object)listViews[i], (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
			UQueryExtensions.Q<ScrollView>((VisualElement)(object)listViews[i], (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
			UQueryExtensions.Q<ScrollView>((VisualElement)(object)listViews[i], (string)null, (string)null).mode = (ScrollViewMode)0;
			((BaseVerticalCollectionView)listViews[i]).fixedItemHeight = ((i == 1) ? 107 : 67);
			((BaseVerticalCollectionView)listViews[i]).virtualizationMethod = (CollectionVirtualizationMethod)0;
		}
	}

	public void RefreshCustom(zbModHandbookCsvRecord mod)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		m_modDetail = mod;
		if (m_modDetail != null)
		{
			((TextElement)m_modName).text = m_modDetail.name;
			zbLabelLinkUtil.BuildModLink(m_modDetail.desc, m_modDesc);
			m_spModIcon.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(m_modDetail.icon));
			for (int i = 0; i < 3; i++)
			{
				RefreshList(i);
			}
			if (m_tip != null)
			{
				m_tip.Hide();
			}
		}
	}

	private void RefreshList(int i)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		switch (i)
		{
		case 0:
			requireMods.Clear();
			m_require.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			if (m_modDetail != null && m_modDetail.requireMod != null && m_modDetail.requireMod.Length != 0)
			{
				requireMods = m_modDetail.requireMod.ToList();
				listViews[i].makeItem = () => (VisualElement)(object)new zbModDetailModItem(this, m_itemTB);
				listViews[i].bindItem = delegate(VisualElement element, int index)
				{
					(element as zbModDetailModItem).Init(requireMods[index], m_modDetail);
				};
				((BaseVerticalCollectionView)listViews[i]).itemsSource = null;
				((BaseVerticalCollectionView)listViews[i]).itemsSource = requireMods;
			}
			else
			{
				m_require.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
			break;
		case 1:
			requireHeros.Clear();
			m_hero.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			if (m_modDetail != null && m_modDetail.requireHero != null && m_modDetail.requireHero.Length != 0)
			{
				int[][] requireHero = m_modDetail.requireHero;
				foreach (int[] item in requireHero)
				{
					requireHeros.Add(item);
				}
				listViews[i].makeItem = () => (VisualElement)(object)new zbModDetailItem(this, m_itemTA);
				listViews[i].bindItem = delegate(VisualElement element, int index)
				{
					if (requireHeros[index] != null)
					{
						(element as zbModDetailItem).Init(requireHeros[index][0], m_modDetail, requireHeros[index][1]);
					}
				};
				((BaseVerticalCollectionView)listViews[i]).itemsSource = null;
				((BaseVerticalCollectionView)listViews[i]).itemsSource = requireHeros;
			}
			else
			{
				m_hero.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
			break;
		case 2:
			conflictMods.Clear();
			m_conflict.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			if (m_modDetail != null && m_modDetail.conflictMod != null && m_modDetail.conflictMod.Length != 0)
			{
				conflictMods = m_modDetail.conflictMod.ToList();
				listViews[i].makeItem = () => (VisualElement)(object)new zbModDetailModItem(this, m_itemTB);
				listViews[i].bindItem = delegate(VisualElement element, int index)
				{
					(element as zbModDetailModItem).Init(conflictMods[index], m_modDetail);
				};
				((BaseVerticalCollectionView)listViews[i]).itemsSource = null;
				((BaseVerticalCollectionView)listViews[i]).itemsSource = conflictMods;
			}
			else
			{
				m_conflict.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
			break;
		}
		((BaseVerticalCollectionView)listViews[i]).RefreshItems();
	}

	public void ShowTip(ClickEvent evt, string tipText)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		IEventHandler target = ((EventBase)evt).target;
		VisualElement val = (VisualElement)(object)((target is VisualElement) ? target : null);
		if (val != null)
		{
			Vector2 val2 = Vector2.op_Implicit(((PointerEventBase<ClickEvent>)(object)evt).localPosition);
			Vector2 val3 = VisualElementExtensions.LocalToWorld(val, val2);
			VisualElement val4 = UQueryExtensions.Q<VisualElement>(GetRootElement(), "root", (string)null);
			Vector2 val5 = VisualElementExtensions.WorldToLocal(val4, val3);
			Vector2 val6 = default(Vector2);
			((Vector2)(ref val6))._002Ector(val4.resolvedStyle.width * 0.01f, val4.resolvedStyle.height * 0.02f);
			val5 += val6;
			m_tip.ShowAt(val5, tipText, val4);
		}
	}

	private void OnClose(ClickEvent evt)
	{
		Hide();
	}
}
