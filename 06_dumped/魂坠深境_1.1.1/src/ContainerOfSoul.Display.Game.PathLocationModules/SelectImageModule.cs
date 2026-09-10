using System;
using System.Collections;
using UnityEngine;

namespace ContainerOfSoul.Display.Game.PathLocationModules;

public class SelectImageModule : PathLocationDisplay.DisplayModule
{
	[SerializeField]
	private Animator selectImageAnim;

	[NonSerialized]
	private bool showing;

	public override void Init()
	{
		AnimEventReceiver component = ((Component)(object)selectImageAnim).GetComponent<AnimEventReceiver>();
		component.onEventTrigger = (Action<object>)Delegate.Combine(component.onEventTrigger, new Action<object>(_OnEventTrigger));
	}

	public override void Setup()
	{
		((Component)(object)selectImageAnim).gameObject.SetActive(value: false);
		showing = false;
	}

	public IEnumerator OnSelect()
	{
		showing = true;
		((Component)(object)selectImageAnim).gameObject.SetActive(value: true);
		selectImageAnim.SetTrigger(AnimKeywords.show);
		yield return new WaitUntil(() => !showing);
	}

	private void _OnEventTrigger(object _)
	{
		((Component)(object)selectImageAnim).gameObject.SetActive(value: false);
		showing = false;
	}
}
