using UnityEngine.UIElements;

public class zbTacticUxmlCard
{
	private TemplateContainer root;

	private zbTacticUxml4Stars m_uxml4Stars;

	public zbTacticUxmlCard(TemplateContainer item)
	{
		root = item;
		TemplateContainer item2 = UQueryExtensions.Q<TemplateContainer>((VisualElement)(object)root, "uxml_4stars", (string)null);
		m_uxml4Stars = new zbTacticUxml4Stars(item2);
	}
}
