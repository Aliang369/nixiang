using TMPro;
using UnityEngine;

public class MatterHintPanel : CommonInfoPanelBase
{
	[SerializeField]
	private RectTransform matterDetailBox;

	[SerializeField]
	private TextMeshProUGUI matterDetailName;

	[SerializeField]
	private TextMeshProUGUI matterDetailDescription;

	protected override void Init()
	{
		base.Init();
		matterDetailBox.gameObject.SetActive(value: false);
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.AddListener(_OnEmptyAreaClicked);
	}

	protected override void Dispose()
	{
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.RemoveListener(_OnEmptyAreaClicked);
	}

	protected override void UpdateInfo()
	{
		if (currentCommonInfoData != null && currentCommonInfoData.target is MatterInfo matterInfo)
		{
			matterDetailBox.gameObject.SetActive(value: true);
			if (currentCommonInfoData.pivotRectTransform != null)
			{
				base.transform.position = currentCommonInfoData.pivotRectTransform.position;
			}
			matterDetailName.text = matterInfo.matterDef.NameWithColor;
			matterDetailDescription.text = matterInfo.matterDef.description;
		}
		else
		{
			matterDetailBox.gameObject.SetActive(value: false);
		}
	}

	private void _OnEmptyAreaClicked()
	{
		HideAllCommonInfo();
	}
}
