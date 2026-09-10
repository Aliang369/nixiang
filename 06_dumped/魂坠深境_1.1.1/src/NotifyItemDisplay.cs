using System.Collections;
using UnityEngine;

public class NotifyItemDisplay : GameAction
{
	private readonly bool delay;

	public Color shakeColor;

	public ItemDisplayBase targetDisplay;

	private readonly float time;

	private float timer;

	public NotifyItemDisplay(Item item, Color shakeColor, float time = 0.25f, bool delay = false)
	{
		targetDisplay = ItemDisplayBase.GetActiveItemSlotDisplay(item);
		this.shakeColor = shakeColor;
		this.time = time;
		this.delay = delay;
	}

	public NotifyItemDisplay(ItemDisplayBase display, Color shakeColor, float time = 0.25f, bool delay = false)
	{
		targetDisplay = display;
		this.shakeColor = shakeColor;
		this.time = time;
		this.delay = delay;
	}

	public IEnumerator notifyCor()
	{
		timer = 0f;
		targetDisplay.SetDirty();
		while (timer < Time.deltaTime)
		{
			timer += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		targetDisplay.SetDirty();
	}

	protected override void Update()
	{
		if (targetDisplay == null)
		{
			FinishAction();
		}
		else if (delay)
		{
			targetDisplay.SetDirty();
			timer += Time.deltaTime * base.actionTimeScale;
			if (timer > time)
			{
				targetDisplay.SetDirty();
				FinishAction();
			}
		}
		else
		{
			targetDisplay.StartCoroutine(notifyCor());
			FinishAction();
		}
	}
}
