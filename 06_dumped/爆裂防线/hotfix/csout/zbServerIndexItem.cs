using System;
using UnityEngine;
using UnityEngine.UIElements;

public class zbServerIndexItem : VisualElement
{
	private Label m_lblTile;

	private Button m_btnTouch;

	public VisualElement m_bg;

	private zbServerListWnd m_parent;

	public zbServerIndexData m_data;

	private Texture2D m_selected_button;

	private Texture2D m_unselected_button;

	public zbServerIndexItem(VisualTreeAsset tAsset, zbServerListWnd parent)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)this)._002Ector();
		((VisualElement)this).style.backgroundColor = new StyleColor(Color.clear);
		((VisualElement)this).Add((VisualElement)(object)tAsset.Instantiate());
		m_parent = parent;
		m_lblTile = UQueryExtensions.Q<Label>((VisualElement)(object)this, "title", (string)null);
		m_bg = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "touch", (string)null);
		m_parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnTouch, "", (TrickleDown)0);
		m_selected_button = zbAssetLoader.Instance.LoadResouce<Texture2D>("ui/image/button/btn_186_82_yellow.png");
		m_unselected_button = zbAssetLoader.Instance.LoadResouce<Texture2D>("ui/image/button/btn_186_82_gray.png");
	}

	public void Init(zbServerIndexData data)
	{
		m_data = data;
		if (m_data.m_bLastTab)
		{
			((TextElement)m_lblTile).text = zbUtil.Language("lang.10046");
		}
		else if (m_data.m_rem > 0)
		{
			((TextElement)m_lblTile).text = $"S{(m_data.m_Index - 1) * m_parent.rowNum + 1}--S{(m_data.m_Index - 1) * m_parent.rowNum + m_data.m_rem}";
		}
		else
		{
			((TextElement)m_lblTile).text = $"S{(m_data.m_Index - 1) * m_parent.rowNum + 1}--S{m_data.m_Index * m_parent.rowNum}";
		}
		zbUtil.RemoveClassNames((VisualElement)(object)this, "unity-collection-view__item", "unity-list-view__item");
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void OnTouch(ClickEvent evt)
	{
		if ((Object)(object)m_parent != (Object)null)
		{
			if (m_data.m_bLastTab)
			{
				m_parent.OnLastLoginServer();
			}
			else
			{
				m_parent.OnServerSel(m_data.m_Index);
			}
		}
	}

	public void OnSelected(int index)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (m_data.m_Index == index)
		{
			m_bg.style.backgroundImage = new StyleBackground(m_selected_button);
		}
		else
		{
			m_bg.style.backgroundImage = new StyleBackground(m_unselected_button);
		}
	}
}
