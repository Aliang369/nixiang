using UnityEngine.UIElements;

public class zbTacticUxmlFilter
{
	private TemplateContainer m_container;

	private VisualElement m_root;

	private Button m_btnFilter;

	private VisualElement m_panelFilter;

	private VisualElement m_bgTouch;

	public zbTacticUxmlFilter(TemplateContainer item)
	{
		m_container = item;
		m_root = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_container, "root", (string)null);
		m_btnFilter = UQueryExtensions.Q<Button>((VisualElement)(object)m_container, "btn_filter", (string)null);
		m_panelFilter = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_container, "panel_filter", (string)null);
		m_bgTouch = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_container, "bg_touch", (string)null);
		((CallbackEventHandler)m_btnFilter).RegisterCallback<ClickEvent>((EventCallback<ClickEvent>)OnFilter, (TrickleDown)0);
		((CallbackEventHandler)m_bgTouch).RegisterCallback<ClickEvent>((EventCallback<ClickEvent>)OnBackgroundTouched, (TrickleDown)0);
	}

	public void OnEnable()
	{
		InitPanelFilter();
	}

	private void OnFilter(ClickEvent evt)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		zbUIMgr.Instance.BubbleInfo("点击了filter按钮");
		if (StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0) == m_panelFilter.style.display)
		{
			m_root.style.width = StyleLength.op_Implicit(90f);
			m_root.style.height = StyleLength.op_Implicit(90f);
			m_panelFilter.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_bgTouch.style.width = StyleLength.op_Implicit(0f);
			m_bgTouch.style.height = StyleLength.op_Implicit(0f);
		}
		else
		{
			m_root.style.width = StyleLength.op_Implicit(600f);
			m_root.style.height = StyleLength.op_Implicit(400f);
			m_panelFilter.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_bgTouch.style.width = StyleLength.op_Implicit(720f);
			m_bgTouch.style.height = StyleLength.op_Implicit(1680f);
		}
	}

	private void OnBackgroundTouched(ClickEvent evt)
	{
		InitPanelFilter();
	}

	private void InitPanelFilter()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		m_root.style.width = StyleLength.op_Implicit(90f);
		m_root.style.height = StyleLength.op_Implicit(90f);
		m_panelFilter.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_bgTouch.style.width = StyleLength.op_Implicit(0f);
		m_bgTouch.style.height = StyleLength.op_Implicit(0f);
	}
}
