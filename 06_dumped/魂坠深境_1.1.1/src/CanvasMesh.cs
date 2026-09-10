using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class CanvasMesh : Graphic
{
	public Mesh Mesh;

	protected override void OnPopulateMesh(VertexHelper vh)
	{
		vh.Clear();
		if (!(Mesh == null))
		{
			Vector3[] vertices = Mesh.vertices;
			Vector2[] array = Mesh.uv;
			if (array.Length < vertices.Length)
			{
				array = new Vector2[vertices.Length];
			}
			Vector2 vector = Mesh.bounds.min;
			Vector2 vector2 = Mesh.bounds.size;
			for (int i = 0; i < vertices.Length; i++)
			{
				Vector2 vector3 = vertices[i];
				vector3.x = (vector3.x - vector.x) / vector2.x;
				vector3.y = (vector3.y - vector.y) / vector2.y;
				vector3 = Vector2.Scale(vector3 - base.rectTransform.pivot, base.rectTransform.rect.size);
				vh.AddVert(vector3, color, array[i]);
			}
			int[] triangles = Mesh.triangles;
			for (int j = 0; j < triangles.Length; j += 3)
			{
				vh.AddTriangle(triangles[j], triangles[j + 1], triangles[j + 2]);
			}
		}
	}

	public Vector3 TransformVertex(Vector3 vertex)
	{
		Vector2 vector = default(Vector2);
		vector.x = (vertex.x - Mesh.bounds.min.x) / Mesh.bounds.size.x;
		vector.y = (vertex.y - Mesh.bounds.min.y) / Mesh.bounds.size.y;
		vector = Vector2.Scale(vector - base.rectTransform.pivot, base.rectTransform.rect.size);
		return base.transform.TransformPoint(vector);
	}

	public Vector3 InverseTransformVertex(Vector3 vertex)
	{
		Vector2 a = base.transform.InverseTransformPoint(vertex);
		a.x /= base.rectTransform.rect.size.x;
		a.y /= base.rectTransform.rect.size.y;
		a += base.rectTransform.pivot;
		a = Vector2.Scale(a, Mesh.bounds.size);
		a.x += Mesh.bounds.min.x;
		a.y += Mesh.bounds.min.y;
		return a;
	}
}
