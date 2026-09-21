using Common.UI;
using TMPro;

public class ItemTextLinkObjectDisplayer : TextLinkObjectDisplayer
{
	public ItemInfoReferenceModule infoReferenceModule;

	public MainItemInfoDisplayBase mainItemInfoDisplay;

	public ItemTextLinkObjectDisplayer(ApplicationStateInfo applicationState, TextMeshProUGUI tmpText)
	{
		base.tmpText = tmpText;
		TextMeshProUGUIPointerEvent orAddComponent = tmpText.gameObject.GetOrAddComponent<TextMeshProUGUIPointerEvent>();
		orAddComponent.onClicked = base._OnTextClicked;
		orAddComponent.onEnter = base._OnTextEnter;
		orAddComponent.onExit = base._OnTextExit;
		applicationState.eventManager.UIHideAllInfoEvent.AddListener(base.OnHideAll);
		infoReferenceModule = tmpText.GetComponentInParent<ItemInfoDisplayBase>(includeInactive: true).GetComponent<ItemInfoReferenceModule>();
		mainItemInfoDisplay = tmpText.GetComponentInParent<MainItemInfoDisplayBase>(includeInactive: true);
	}

	public override void ShowInfo(object refLinkObject, CommonInfoData.ControlType controlType)
	{
		HideInfo(controlType);
		infoReferenceModule.SetupRefObjectDisplay(mainItemInfoDisplay, refLinkObject);
	}

	public override void HideInfo(CommonInfoData.ControlType controlType)
	{
		infoReferenceModule.UnsetItemReference(mainItemInfoDisplay);
		infoReferenceModule.HideAllSideReference();
		infoReferenceModule.UnsetCommonReference();
	}
}
