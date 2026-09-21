using TMPro;
using UnityEngine;

public class TextMeshMaterialDisplay : MonoBehaviour
{
	[SerializeField]
	public TextMeshProUGUI textMesh;

	[SerializeField]
	public bool isUseCloneMaterial;

	[SerializeField]
	public bool updateMaterial;

	[SerializeField]
	public float _GlowInner;

	[SerializeField]
	public float _GlowOuter;

	private void Awake()
	{
		if (isUseCloneMaterial)
		{
			textMesh.fontSharedMaterial = new Material(textMesh.fontSharedMaterial);
		}
	}

	private void Update()
	{
		if (updateMaterial)
		{
			textMesh.fontSharedMaterial.SetFloat("_GlowInner", _GlowInner);
			textMesh.fontSharedMaterial.SetFloat("_GlowOuter", _GlowOuter);
		}
	}
}
