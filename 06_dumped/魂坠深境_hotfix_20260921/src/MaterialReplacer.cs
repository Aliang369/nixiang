using UnityEngine;
using UnityEngine.UI;

public class MaterialReplacer : MonoBehaviour
{
	private void Awake()
	{
		Image component = base.gameObject.GetComponent<Image>();
		if (!(component == null))
		{
			component.material = Object.Instantiate(component.material);
		}
	}
}
