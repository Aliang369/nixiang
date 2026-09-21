using System;

public class SecondConfirmInfo
{
	public string description;

	public string cancelChoiceText;

	public string confirmChoiceText;

	public bool confirmInteractable = true;

	public Action confirmCallback;

	public Action cancelCallback;
}
