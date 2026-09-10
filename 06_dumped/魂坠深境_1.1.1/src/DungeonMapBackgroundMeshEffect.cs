using System;
using UnityEngine;
using UnityEngine.UI;

public class DungeonMapBackgroundMeshEffect : BaseMeshEffect
{
	[SerializeField]
	private Image image;

	[SerializeField]
	private Material originMaterial;

	[NonSerialized]
	private Vector2[] verts;

	public void Init()
	{
		image.material = UnityEngine.Object.Instantiate(originMaterial);
		verts = new Vector2[4]
		{
			Vector2.zero,
			Vector2.up,
			Vector2.one,
			Vector2.right
		};
	}

	public void SetFactor(float start, float end)
	{
		verts[0].x = start;
		verts[1].x = start;
		verts[2].x = end;
		verts[3].x = end;
	}

	public Material GetMaterial()
	{
		return image.material;
	}

	public override void ModifyMesh(VertexHelper vh)
	{
		if (IsActive())
		{
			SetVertexUV1(vh, 0, verts[0]);
			SetVertexUV1(vh, 1, verts[1]);
			SetVertexUV1(vh, 2, verts[2]);
			SetVertexUV1(vh, 3, verts[3]);
		}
	}

	private void SetVertexUV1(VertexHelper vh, int index, Vector2 vu1)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		UIVertex vertex = default(UIVertex);
		vh.PopulateUIVertex(ref vertex, index);
		vertex.uv1 = vu1;
		vh.SetUIVertex(vertex, index);
	}
}
