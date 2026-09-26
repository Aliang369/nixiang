using UnityEngine;

public class zbSeasonBackground : MonoBehaviour
{
	[SerializeField]
	private Material bgMaterial;

	private void Start()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		float num = (float)zbSeasonMapDefine.gridCount * zbSeasonMapDefine.cellSize;
		float num2 = zbSeasonMapDefine.cellSize * 0.5f;
		float num3 = num * 0.5f + num2;
		Mesh val = new Mesh
		{
			name = "SeaSon_Background"
		};
		val.vertices = (Vector3[])(object)new Vector3[4]
		{
			new Vector3(0f - num3, 0f - num3, 0f),
			new Vector3(num3, 0f - num3, 0f),
			new Vector3(0f - num3, num3, 0f),
			new Vector3(num3, num3, 0f)
		};
		val.uv = (Vector2[])(object)new Vector2[4]
		{
			new Vector2(0f, 0f),
			new Vector2((float)zbSeasonMapDefine.gridCount, 0f),
			new Vector2(0f, (float)zbSeasonMapDefine.gridCount),
			new Vector2((float)zbSeasonMapDefine.gridCount, (float)zbSeasonMapDefine.gridCount)
		};
		val.triangles = new int[6] { 0, 2, 1, 2, 3, 1 };
		val.RecalculateBounds();
		((Component)this).GetComponent<MeshFilter>().sharedMesh = val;
		((Renderer)((Component)this).GetComponent<MeshRenderer>()).material = bgMaterial;
	}
}
