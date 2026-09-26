using UnityEngine.UIElements;

public class zbTacticUxml4Stars
{
	private TemplateContainer root;

	private VisualElement m_star1;

	private VisualElement m_star2;

	private VisualElement m_star3;

	private VisualElement m_star4;

	public zbTacticUxml4Stars(TemplateContainer item)
	{
		root = item;
		m_star1 = UQueryExtensions.Q<VisualElement>((VisualElement)(object)root, "star1", (string)null);
		m_star2 = UQueryExtensions.Q<VisualElement>((VisualElement)(object)root, "star2", (string)null);
		m_star3 = UQueryExtensions.Q<VisualElement>((VisualElement)(object)root, "star3", (string)null);
		m_star4 = UQueryExtensions.Q<VisualElement>((VisualElement)(object)root, "star4", (string)null);
	}
}
