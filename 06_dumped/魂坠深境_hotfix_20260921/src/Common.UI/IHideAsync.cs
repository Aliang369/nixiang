using System.Collections;

namespace Common.UI;

public interface IHideAsync
{
	IEnumerator HideAnim()
	{
		yield break;
	}

	void OnHideStart()
	{
	}

	void OnHideEnd()
	{
	}

	IEnumerator HideUIAsync()
	{
		OnHideStart();
		yield return HideAnim();
		OnHideEnd();
	}
}
