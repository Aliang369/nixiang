using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UI/Vertical Gradient Image", 12)]
public class VerticalGradientImage : Image
{
	[SerializeField]
	public Color topColor = Color.white;

	[SerializeField]
	public Color bottomColor = Color.white;

	private static readonly int TopColorId = Shader.PropertyToID("_TopColor");

	private static readonly int BottomColorId = Shader.PropertyToID("_BottomColor");

	private static Material defaultGradientMaterial;

	private Material runtimeMaterial;

	private Material runtimeMaterialSource;

	public override Material defaultMaterial
	{
		get
		{
			if (defaultGradientMaterial != null)
			{
				return defaultGradientMaterial;
			}
			Shader shader = Shader.Find("自定义/UI/VerticalGradientImage");
			if (shader == null)
			{
				return base.defaultMaterial;
			}
			defaultGradientMaterial = new Material(shader)
			{
				name = "VerticalGradientImage Default",
				hideFlags = HideFlags.HideAndDontSave
			};
			return defaultGradientMaterial;
		}
	}

	public override Material materialForRendering
	{
		get
		{
			Material material = base.materialForRendering;
			if (material == null || !material.HasProperty(TopColorId) || !material.HasProperty(BottomColorId))
			{
				return material;
			}
			if (runtimeMaterial == null || runtimeMaterialSource != material)
			{
				DestroyRuntimeMaterial();
				runtimeMaterialSource = material;
				runtimeMaterial = new Material(material)
				{
					name = material.name + " (VerticalGradientImage)",
					hideFlags = HideFlags.HideAndDontSave
				};
			}
			runtimeMaterial.SetColor(TopColorId, topColor);
			runtimeMaterial.SetColor(BottomColorId, bottomColor);
			return runtimeMaterial;
		}
	}

	public void SetTopColor(Color value)
	{
		if (!(topColor == value))
		{
			topColor = value;
			SetMaterialDirty();
		}
	}

	public void SetBottomColor(Color value)
	{
		if (!(bottomColor == value))
		{
			bottomColor = value;
			SetMaterialDirty();
		}
	}

	public void SetGradientColors(Color top, Color bottom)
	{
		if (!(topColor == top) || !(bottomColor == bottom))
		{
			topColor = top;
			bottomColor = bottom;
			SetMaterialDirty();
		}
	}

	protected override void OnDisable()
	{
		DestroyRuntimeMaterial();
		base.OnDisable();
	}

	protected override void OnPopulateMesh(VertexHelper toFill)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		base.OnPopulateMesh(toFill);
		Rect rect = base.rectTransform.rect;
		float height = rect.height;
		if (!(height <= 0f))
		{
			UIVertex vertex = default(UIVertex);
			for (int i = 0; i < toFill.currentVertCount; i++)
			{
				toFill.PopulateUIVertex(ref vertex, i);
				float y = Mathf.Clamp01((vertex.position.y - rect.yMin) / height);
				vertex.uv1 = new Vector2(vertex.uv1.x, y);
				toFill.SetUIVertex(vertex, i);
			}
		}
	}

	private void DestroyRuntimeMaterial()
	{
		if (!(runtimeMaterial == null))
		{
			if (Application.isPlaying)
			{
				Object.Destroy(runtimeMaterial);
			}
			else
			{
				Object.DestroyImmediate(runtimeMaterial);
			}
			runtimeMaterial = null;
			runtimeMaterialSource = null;
		}
	}
}
