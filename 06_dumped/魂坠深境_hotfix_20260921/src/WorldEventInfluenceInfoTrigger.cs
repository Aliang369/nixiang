using System;
using Common.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class WorldEventInfluenceInfoTrigger : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler
{
	[NonSerialized]
	private Influence influence;

	[NonSerialized]
	private ICommonInfoPanel infoPanel;

	[NonSerialized]
	private RectTransform pivotRectTransform;

	public void Setup(Influence influence, ICommonInfoPanel infoPanel, RectTransform pivotRectTransform)
	{
		this.influence = influence;
		this.infoPanel = infoPanel;
		this.pivotRectTransform = pivotRectTransform;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (influence != null)
		{
			Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_click_general);
			infoPanel?.ShowCommonInfo(new CommonInfoData(influence, CommonInfoData.ControlType.Click, pivotRectTransform, false));
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (!PlatformHelper.IsMobile() && influence != null)
		{
			infoPanel?.ShowCommonInfo(new CommonInfoData(influence, CommonInfoData.ControlType.Hover, pivotRectTransform, false));
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (!PlatformHelper.IsMobile() && influence != null)
		{
			infoPanel?.HideCommonInfo(influence, CommonInfoData.ControlType.Hover);
		}
	}

	private void OnDisable()
	{
		if (influence != null)
		{
			infoPanel?.HideCommonInfo(influence, CommonInfoData.ControlType.Hover);
			infoPanel?.HideCommonInfo(influence, CommonInfoData.ControlType.Click);
		}
	}
}
