using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DebugTextInfo : MonoBehaviour
{
	[SerializeField]
	public Text text;

	[NonSerialized]
	public UnityAction onUpdateTextInfo;

	private void Update()
	{
		onUpdateTextInfo?.Invoke();
	}

	public void SetTextUpdater(UnityAction onUpdate)
	{
		onUpdateTextInfo = onUpdate;
	}
}
