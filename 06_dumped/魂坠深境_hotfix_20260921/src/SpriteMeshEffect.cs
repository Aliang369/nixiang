using UnityEngine;
using UnityEngine.UI;

public class SpriteMeshEffect : BaseMeshEffect
{
	[SerializeField]
	public Image image;

	protected override void Awake()
	{
		base.Awake();
		image.material = Object.Instantiate(image.material);
	}

	public override void ModifyMesh(VertexHelper vh)
	{
		if (IsActive())
		{
			SetVertexUV1(vh, 0, Vector2.zero);
			SetVertexUV1(vh, 1, Vector2.up);
			SetVertexUV1(vh, 2, Vector2.one);
			SetVertexUV1(vh, 3, Vector2.right);
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
