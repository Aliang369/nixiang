using UnityEngine;

public abstract class BootSetter<T> : MonoBehaviour where T : class
{
	private static BootSetter<T> instance;

	protected T bootTarget;

	public virtual void Setup(T t)
	{
		instance = this;
		bootTarget = t;
	}

	public static void BootAttach(GameObject prefabSource, T bootTarget)
	{
		BootSetter<T> bootSetter = Object.FindObjectOfType<BootSetter<T>>();
		if (bootSetter != null)
		{
			Object.Destroy(bootSetter.gameObject);
		}
		bootSetter = Object.Instantiate(prefabSource).GetComponent<BootSetter<T>>();
		bootSetter.Setup(bootTarget);
	}

	public static T GetCurrentBootTarget()
	{
		BootSetter<T> bootSetter = instance;
		if ((object)bootSetter == null)
		{
			return null;
		}
		return bootSetter.bootTarget;
	}
}
