using UnityEngine;

public class AnimEndEvent : MonoBehaviour
{
	[SerializeField]
	private Animator animator;

	public void OnEnd()
	{
		((Behaviour)(object)animator).enabled = false;
	}

	private string GetPath()
	{
		string text = base.gameObject.name;
		Transform parent = base.gameObject.transform.parent;
		while (parent != null)
		{
			text = parent.name + "/" + text;
			parent = parent.parent;
		}
		return text;
	}
}
