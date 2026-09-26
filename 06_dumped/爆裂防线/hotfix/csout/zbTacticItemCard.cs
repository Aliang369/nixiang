using UnityEngine.UIElements;

public class zbTacticItemCard : zbViewBaseItem
{
	private zbTacticWnd m_wndParent;

	private Button m_btnTouch;

	private VisualElement m_cardNameIcon;

	private Label m_cardNameText;

	private zbTacticUxmlCard m_uxmlCard;

	public zbTacticItemCard(zbTacticWnd wndParent)
	{
		m_wndParent = wndParent;
		VisualElement val = (VisualElement)(object)zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/tactic/tb_tacticItemCard.uxml").Instantiate();
		((VisualElement)this).Add(val);
		m_btnTouch = UQueryExtensions.Q<Button>(val, "btn_touch", (string)null);
		((CallbackEventHandler)m_btnTouch).RegisterCallback<ClickEvent>((EventCallback<ClickEvent>)OnTouch, (TrickleDown)0);
		m_cardNameIcon = UQueryExtensions.Q<VisualElement>(UQueryExtensions.Q<VisualElement>(val, "card_name", (string)null), "icon", (string)null);
		m_cardNameText = UQueryExtensions.Q<Label>(UQueryExtensions.Q<VisualElement>(val, "card_name", (string)null), "text", (string)null);
		TemplateContainer item = UQueryExtensions.Q<TemplateContainer>(val, "uxml_card", (string)null);
		m_uxmlCard = new zbTacticUxmlCard(item);
	}

	private void OnTouch(ClickEvent evt)
	{
		m_wndParent.OnGridItemCardSelected(this);
	}

	public void Refresh(zbTacticCsvRecord record)
	{
		zbModCsvRecord recordByKey = zbDataConfig.Instance.modCsvTable.GetRecordByKey(record.modId);
		((TextElement)m_cardNameText).text = recordByKey.name;
	}
}
