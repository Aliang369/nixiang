using System;
using UnityEngine;

public class zbSeasonLineMgr : MonoBehaviour
{
	public class Line
	{
		public Vector3 start;

		public Vector3 end;

		public Color color;

		public float width;

		public bool active;

		public bool autoRemove;

		public float duration;

		public float timer;

		public zbSeasonCar car;
	}

	[SerializeField]
	private Material m_lineMat;

	private Mesh m_mesh;

	private MeshFilter m_filter;

	private MeshRenderer m_renderer;

	public Line[] m_lines;

	private int m_lineCount;

	private int m_capacity = 64;

	private Vector3[] m_verts;

	private int[] m_tris;

	private Color[] m_colors;

	private Vector2[] m_uvs;

	private Sprite m_carSprite0;

	private Sprite m_carSprite1;

	public static zbSeasonLineMgr Instance { get; private set; }

	private void Awake()
	{
		Instance = this;
		InitMesh();
	}

	private void OnDestroy()
	{
		if ((Object)(object)m_mesh != (Object)null)
		{
			Object.Destroy((Object)(object)m_mesh);
		}
	}

	private void InitMesh()
	{
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Expected O, but got Unknown
		m_carSprite0 = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/season/seasonCar_0.png");
		m_carSprite1 = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/season/seasonCar_1.png");
		m_filter = ((Component)this).gameObject.AddComponent<MeshFilter>();
		m_renderer = ((Component)this).gameObject.AddComponent<MeshRenderer>();
		((Renderer)m_renderer).material = m_lineMat;
		((Renderer)m_renderer).sortingLayerName = "Default";
		((Renderer)m_renderer).sortingOrder = 0;
		m_lines = new Line[m_capacity];
		for (int i = 0; i < m_capacity; i++)
		{
			m_lines[i] = CreateLineData(i);
		}
		m_verts = (Vector3[])(object)new Vector3[m_capacity * 4];
		m_tris = new int[m_capacity * 6];
		m_colors = (Color[])(object)new Color[m_capacity * 4];
		m_uvs = (Vector2[])(object)new Vector2[m_capacity * 4];
		GenIndices(0, m_capacity);
		m_mesh = new Mesh();
		m_mesh.MarkDynamic();
		m_filter.sharedMesh = m_mesh;
		m_mesh.vertices = m_verts;
		m_mesh.triangles = m_tris;
		m_mesh.colors = m_colors;
		m_mesh.uv = m_uvs;
	}

	private Line CreateLineData(int index)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		Line line = new Line();
		GameObject val = new GameObject($"SeasonCar_{index}");
		val.transform.SetParent(((Component)this).transform, false);
		zbSeasonCar zbSeasonCar2 = val.AddComponent<zbSeasonCar>();
		zbSeasonCar2.Init(index, m_carSprite0, ((Renderer)m_renderer).sortingLayerName, ((Renderer)m_renderer).sortingOrder + 10);
		val.SetActive(false);
		line.car = zbSeasonCar2;
		return line;
	}

	private void GenIndices(int startLine, int count)
	{
		for (int i = startLine; i < startLine + count; i++)
		{
			int num = i * 6;
			int num2 = i * 4;
			m_tris[num] = num2;
			m_tris[num + 1] = num2 + 1;
			m_tris[num + 2] = num2 + 2;
			m_tris[num + 3] = num2;
			m_tris[num + 4] = num2 + 2;
			m_tris[num + 5] = num2 + 3;
		}
	}

	private void EnsureCapacity(int needLines)
	{
		if (needLines > m_capacity)
		{
			int num;
			for (num = m_capacity; num < needLines; num *= 2)
			{
			}
			Array.Resize(ref m_lines, num);
			for (int i = m_capacity; i < num; i++)
			{
				m_lines[i] = CreateLineData(i);
			}
			Array.Resize(ref m_verts, num * 4);
			Array.Resize(ref m_tris, num * 6);
			Array.Resize(ref m_colors, num * 4);
			Array.Resize(ref m_uvs, num * 4);
			GenIndices(m_capacity, num - m_capacity);
			m_capacity = num;
			m_mesh.Clear();
			m_mesh.vertices = m_verts;
			m_mesh.triangles = m_tris;
			m_mesh.colors = m_colors;
			m_mesh.uv = m_uvs;
		}
	}

	private void UpdateCarDirection(Line line, Vector3 from, Vector3 to)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = to - from;
		if (((Vector3)(ref val)).sqrMagnitude < 0.0001f)
		{
			return;
		}
		float num = Mathf.Atan2(val.y, val.x) * 57.29578f;
		SpriteRenderer component = ((Component)line.car).GetComponent<SpriteRenderer>();
		if (!((Object)(object)component == (Object)null))
		{
			component.flipY = false;
			float num2 = 0f;
			if (num >= 0f && num < 90f)
			{
				component.sprite = m_carSprite1;
				component.flipX = false;
				num2 = num - 30f;
			}
			else if (num >= 90f && num <= 180f)
			{
				component.sprite = m_carSprite1;
				component.flipX = true;
				num2 = num - 150f;
			}
			else if (num < 0f && num >= -90f)
			{
				component.sprite = m_carSprite0;
				component.flipX = false;
				num2 = num - -30f;
			}
			else
			{
				component.sprite = m_carSprite0;
				component.flipX = true;
				num2 = num - -150f;
			}
			((Component)line.car).transform.rotation = Quaternion.Euler(0f, 0f, num2);
		}
	}

	private void LateUpdate()
	{
		//IL_03ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0404: Unknown result type (might be due to invalid IL or missing references)
		//IL_040e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0413: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0250: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_027f: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0294: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_030a: Unknown result type (might be due to invalid IL or missing references)
		//IL_030f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0325: Unknown result type (might be due to invalid IL or missing references)
		//IL_032a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0366: Unknown result type (might be due to invalid IL or missing references)
		//IL_036d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0374: Unknown result type (might be due to invalid IL or missing references)
		//IL_0381: Unknown result type (might be due to invalid IL or missing references)
		//IL_038b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0392: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a3: Unknown result type (might be due to invalid IL or missing references)
		float num = 0.2f;
		for (int i = 0; i < m_capacity; i++)
		{
			Line line = m_lines[i];
			if (line == null)
			{
				continue;
			}
			if (i >= m_lineCount || !line.active)
			{
				ClearQuadVertices(i);
				if ((Object)(object)line.car != (Object)null && ((Component)line.car).gameObject.activeSelf)
				{
					((Component)line.car).gameObject.SetActive(false);
				}
				continue;
			}
			bool flag = false;
			_ = line.start;
			_ = line.end;
			_ = line.start;
			Vector3 start = line.start;
			_ = line.end;
			Vector3 end = line.end;
			Vector3 val = start;
			Vector3 val2 = end;
			if (line.duration > 0f)
			{
				line.timer += Time.deltaTime;
				float num2 = Mathf.Clamp01(line.timer / line.duration);
				val = Vector3.Lerp(start, end, num2);
				if (num2 >= 1f)
				{
					flag = true;
				}
			}
			if (flag)
			{
				line.active = false;
				ClearQuadVertices(i);
				if ((Object)(object)line.car != (Object)null && ((Component)line.car).gameObject.activeSelf)
				{
					ClearCar(line);
				}
				continue;
			}
			Vector3 val3 = val2 - val;
			float magnitude = ((Vector3)(ref val3)).magnitude;
			if (magnitude < 0.001f)
			{
				ClearQuadVertices(i);
				if ((Object)(object)line.car != (Object)null && ((Component)line.car).gameObject.activeSelf)
				{
					ClearCar(line);
				}
				continue;
			}
			val3 /= magnitude;
			Vector3 val4 = val - val3 * num;
			if (Vector3.Dot(val2 - val4, val3) <= 0f)
			{
				ClearQuadVertices(i);
			}
			else
			{
				Vector3 val5 = new Vector3(0f - val3.y, val3.x, 0f) * line.width * 0.5f;
				int num3 = i * 4;
				m_verts[num3] = val4 + val5;
				m_verts[num3 + 1] = val4 - val5;
				m_verts[num3 + 2] = val2 - val5;
				m_verts[num3 + 3] = val2 + val5;
				m_colors[num3] = line.color;
				m_colors[num3 + 1] = line.color;
				m_colors[num3 + 2] = line.color;
				m_colors[num3 + 3] = line.color;
				float num4 = Vector3.Distance(val4, val2) / 0.5f;
				m_uvs[num3] = new Vector2(0f, 1f);
				m_uvs[num3 + 1] = new Vector2(0f, 0f);
				m_uvs[num3 + 2] = new Vector2(num4, 0f);
				m_uvs[num3 + 3] = new Vector2(num4, 1f);
			}
			if ((Object)(object)line.car != (Object)null)
			{
				if (!((Component)line.car).gameObject.activeSelf)
				{
					((Component)line.car).gameObject.SetActive(true);
				}
				line.car.SetPosition(new Vector3(val.x, val.y, val.z - 0.1f));
				Mathf.Atan2(val3.y, val3.x);
				UpdateCarDirection(line, start, end);
			}
		}
		m_mesh.vertices = m_verts;
		m_mesh.uv = m_uvs;
		m_mesh.colors = m_colors;
		m_mesh.UploadMeshData(false);
		m_mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 10000f);
	}

	private void ClearQuadVertices(int lineIndex)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		int num = lineIndex * 4;
		m_verts[num] = (m_verts[num + 1] = (m_verts[num + 2] = (m_verts[num + 3] = Vector3.zero)));
		m_uvs[num] = (m_uvs[num + 1] = (m_uvs[num + 2] = (m_uvs[num + 3] = Vector2.zero)));
		m_colors[num] = (m_colors[num + 1] = (m_colors[num + 2] = (m_colors[num + 3] = default(Color))));
	}

	public int AddLine(Vector3 start, Vector3 end, Color color, float width, float duration, float startProgress = 0f)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		int num = -1;
		for (int i = 0; i < m_lineCount; i++)
		{
			if (!m_lines[i].active)
			{
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			EnsureCapacity(m_lineCount + 1);
			num = m_lineCount;
			m_lineCount++;
		}
		Line obj = m_lines[num];
		obj.start = start;
		obj.end = end;
		obj.color = color;
		obj.width = width;
		obj.duration = duration;
		obj.timer = Mathf.Clamp01(startProgress) * duration;
		obj.autoRemove = true;
		obj.active = true;
		return num;
	}

	public void SpeedUpLineProportional(int lineId, float newDuration)
	{
		if (lineId >= 0 && lineId < m_lineCount)
		{
			Line line = m_lines[lineId];
			if (line.active && !(line.duration <= 0f))
			{
				float num = Mathf.Clamp01(line.timer / line.duration);
				line.duration = newDuration;
				line.timer = num * newDuration;
			}
		}
	}

	private void ClearCar(Line line)
	{
		zbSeasonCarHudWnd zbSeasonCarHudWnd2 = zbUIMgr.Instance.GetWndById(EWndId.SeasonCarHud) as zbSeasonCarHudWnd;
		if ((Object)(object)zbSeasonCarHudWnd2 != (Object)null)
		{
			zbSeasonCarHudWnd2.Hide();
		}
		((Component)line.car).gameObject.SetActive(false);
	}

	public void RemoveLine(int lineId)
	{
		if (lineId >= 0 && lineId < m_lineCount)
		{
			m_lines[lineId].active = false;
			if ((Object)(object)m_lines[lineId].car != (Object)null)
			{
				((Component)m_lines[lineId].car).gameObject.SetActive(false);
			}
		}
	}

	public void ClearAll()
	{
		for (int i = 0; i < m_lineCount; i++)
		{
			m_lines[i].active = false;
			if ((Object)(object)m_lines[i].car != (Object)null)
			{
				((Component)m_lines[i].car).gameObject.SetActive(false);
			}
		}
		m_lineCount = 0;
	}
}
