using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(CanvasRenderer), typeof(TrailRenderer))]
[AddComponentMenu("UI/Effects/Extensions/UITrailRenderer")]
public class UITrailRenderer : MaskableGraphic
{
	private Camera cam;

	private Mesh Mesh;

	private TrailRenderer trail;

	public override Material defaultMaterial => trail.material;

	public override Texture mainTexture => trail.material.mainTexture;

	protected override void Awake()
	{
		base.Awake();
		if (Mesh == null)
		{
			Mesh = new Mesh();
		}
		if (cam == null)
		{
			cam = SceneInfoSingleton.instance?.UIManager?.gameVirtualCamera?.bindedCamera.finalCamera;
		}
		if (trail == null)
		{
			trail = GetComponent<TrailRenderer>();
			if (trail != null)
			{
				trail.alignment = LineAlignment.Local;
				trail.renderingLayerMask = 0u;
			}
		}
	}

	private void LateUpdate()
	{
		if (!(trail == null) && !(cam == null))
		{
			base.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
			SetAllDirty();
		}
	}

	private void OnDrawGizmos()
	{
		if (!(Mesh == null))
		{
			Vector3[] vertices = Mesh.vertices;
			for (int i = 0; i < vertices.Length; i++)
			{
				Gizmos.DrawSphere(vertices[i], 0.1f);
			}
		}
	}

	protected override void OnPopulateMesh(VertexHelper vh)
	{
		trail.BakeMesh(Mesh, cam);
		vh.Clear();
		if (!(Mesh == null))
		{
			Vector3[] vertices = Mesh.vertices;
			Vector2[] array = Mesh.uv;
			Color[] array2 = Mesh.colors;
			if (array.Length < vertices.Length)
			{
				array = new Vector2[vertices.Length];
			}
			if (array2.Length < vertices.Length)
			{
				array2 = new Color[vertices.Length];
			}
			_ = Matrix4x4.TRS(base.transform.position, Quaternion.identity, Vector3.one).inverse;
			for (int i = 0; i < vertices.Length; i++)
			{
				Vector3 point = vertices[i];
				point = base.transform.worldToLocalMatrix.MultiplyPoint(point);
				vh.AddVert(point, array2[i], array[i]);
			}
			int[] triangles = Mesh.triangles;
			for (int j = 0; j < triangles.Length; j += 3)
			{
				vh.AddTriangle(triangles[j], triangles[j + 1], triangles[j + 2]);
			}
		}
	}
}
