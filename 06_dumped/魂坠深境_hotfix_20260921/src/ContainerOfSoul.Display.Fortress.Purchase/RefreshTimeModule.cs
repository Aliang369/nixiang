using System;
using Common.UI;
using TMPro;
using UnityEngine;

namespace ContainerOfSoul.Display.Fortress.Purchase;

public class RefreshTimeModule : MonoDisplayModule, IFrameUpdate
{
	[SerializeField]
	private TextMeshProUGUI refreshTimeText;

	private bool displayEnabled = true;

	private PurchaseCell purchaseCell => parent as PurchaseCell;

	private PurchaseInfo purchase => purchaseCell?.setupData;

	private GameObject refreshTimeArea => refreshTimeText?.transform.parent?.gameObject;

	public GameObject DisplayArea => refreshTimeArea;

	public void FrameUpdate()
	{
		if (!displayEnabled || purchase == null || refreshTimeText == null)
		{
			SetAreaActive(active: false);
			return;
		}
		DateTime? resetEndTime = purchase.GetResetEndTime();
		if (!resetEndTime.HasValue)
		{
			SetAreaActive(active: false);
			return;
		}
		TimeSpan span = resetEndTime.Value - DateTime.UtcNow;
		if (span.TotalSeconds <= 0.0)
		{
			SetAreaActive(active: false);
			return;
		}
		SetAreaActive(active: true);
		refreshTimeText.gameObject.SetActive(value: true);
		refreshTimeText.text = TextDisplayHelper.GetTimeSpanText(span);
	}

	public void SetDisplayEnabled(bool value)
	{
		displayEnabled = value;
		if (!displayEnabled)
		{
			SetAreaActive(active: false);
		}
	}

	private void SetAreaActive(bool active)
	{
		if (refreshTimeArea != null)
		{
			refreshTimeArea.SetActive(active);
		}
		else if (refreshTimeText != null)
		{
			refreshTimeText.gameObject.SetActive(active);
		}
	}
}
