using System.Collections;

namespace Common.UI;

public interface IShowAsync
{
	IEnumerator ShowAnim()
	{
		yield break;
	}

	void OnShowStart()
	{
	}

	void OnShowEnd()
	{
	}

	IEnumerator ShowUIAsync()
	{
		OnShowStart();
		yield return ShowAnim();
		OnShowEnd();
	}
}
