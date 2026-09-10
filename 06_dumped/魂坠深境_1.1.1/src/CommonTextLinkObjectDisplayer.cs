using Common.UI;
using TMPro;
using UnityEngine;

public class CommonTextLinkObjectDisplayer : TextLinkObjectDisplayer
{
	private UniversalCommonInfoPanel universalCommonInfoPanel;

	private RectTransform pivotRectTransform;

	public CommonTextLinkObjectDisplayer(ApplicationStateInfo applicationState, TextMeshProUGUI tmpText, RectTransform pivotRectTransform = null)
	{
		base.tmpText = tmpText;
		if (pivotRectTransform == null)
		{
			pivotRectTransform = tmpText.GetComponent<RectTransform>();
		}
		applicationState.eventManager.UIHideAllInfoEvent.AddListener(base.OnHideAll);
		TextMeshProUGUIPointerEvent orAddComponent = tmpText.gameObject.GetOrAddComponent<TextMeshProUGUIPointerEvent>();
		orAddComponent.onClicked = base._OnTextClicked;
		orAddComponent.onEnter = base._OnTextEnter;
		orAddComponent.onExit = base._OnTextExit;
		this.pivotRectTransform = pivotRectTransform;
		if (applicationState.runningGameState != null)
		{
			universalCommonInfoPanel = applicationState.runningGameState.UIManager.universalCommonInfoPanel;
		}
		else if (applicationState.fortressState != null)
		{
			universalCommonInfoPanel = applicationState.fortressState.uiManager.universalCommonInfoPanel;
		}
	}

	public override void ShowInfo(object refLinkObject, CommonInfoData.ControlType controlType)
	{
		if ((object)universalCommonInfoPanel == null)
		{
			universalCommonInfoPanel = UIHelper.GetDefaultCommonInfoPanel();
		}
		if (universalCommonInfoPanel == null)
		{
			TraceLog.Error($"无法显示文本引用对象信息：UniversalCommonInfoPanel 为空，refLinkObject={refLinkObject}");
		}
		else
		{
			universalCommonInfoPanel.AppendInfo(new CommonInfoData(refLinkObject, controlType, pivotRectTransform, false));
		}
	}

	public override void HideInfo(CommonInfoData.ControlType controlType)
	{
		if ((object)universalCommonInfoPanel == null)
		{
			universalCommonInfoPanel = UIHelper.GetDefaultCommonInfoPanel();
		}
		if (!(universalCommonInfoPanel == null))
		{
			universalCommonInfoPanel.HideAll();
		}
	}
}
