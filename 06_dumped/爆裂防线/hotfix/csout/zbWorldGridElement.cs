using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class zbWorldGridElement : VisualElement
{
	public enum MapCellState
	{
		Locked,
		ToExplore,
		Unlocked
	}

	private struct GridCellState
	{
		public float alpha;

		public bool showBorder;

		public MapCellState state;
	}

	private class BlinkTask
	{
		public float originalAlpha;

		public float elapsed;

		public float duration;

		public float freq;
	}

	private struct Quad
	{
		public Rect rect;

		public Color color;
	}

	private const int MaxQuadsPerMesh = 16000;

	private int m_cols;

	private int m_rows;

	private float m_cellWidth;

	private float m_cellHeight;

	private Color m_lineColor;

	private float m_lineWidth;

	private GridCellState[] m_cellStates;

	private Dictionary<int, BlinkTask> m_blinkTasks;

	private bool m_isBlinking;

	private IVisualElementScheduledItem m_blinkScheduledItem;

	private readonly List<int> m_cachedBlinkKeys;

	private Texture2D m_texLocked;

	private Texture2D m_texToExplore;

	private Color m_borderColor;

	private float m_borderThickness;

	private readonly List<Quad> m_lockedQuads;

	private readonly List<Quad> m_exploreQuads;

	private readonly List<Quad> m_solidQuads;

	public zbWorldGridElement(int cols, int rows, float cellWidth, float cellHeight)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		m_cellWidth = 140f;
		m_cellHeight = 140f;
		m_lineColor = new Color(Color.white.r, Color.white.g, Color.white.b, 0.1f);
		m_lineWidth = 3f;
		m_blinkTasks = new Dictionary<int, BlinkTask>();
		m_cachedBlinkKeys = new List<int>();
		m_borderColor = new Color(Color.white.r, Color.white.g, Color.white.b, 0.6f);
		m_borderThickness = 5f;
		m_lockedQuads = new List<Quad>();
		m_exploreQuads = new List<Quad>();
		m_solidQuads = new List<Quad>();
		((VisualElement)this)._002Ector();
		m_cols = cols;
		m_rows = rows;
		m_cellWidth = cellWidth;
		m_cellHeight = cellHeight;
		m_texLocked = zbAssetLoader.Instance.LoadResouce<Texture2D>("ui/image/world/cellLock.png");
		m_texToExplore = zbAssetLoader.Instance.LoadResouce<Texture2D>("ui/image/world/cellExplore.png");
		InitGrid();
		((VisualElement)this).generateVisualContent = (Action<MeshGenerationContext>)Delegate.Combine(((VisualElement)this).generateVisualContent, new Action<MeshGenerationContext>(OnGenerateVisualContent));
		((CallbackEventHandler)this).RegisterCallback<GeometryChangedEvent>((EventCallback<GeometryChangedEvent>)delegate
		{
			((VisualElement)this).MarkDirtyRepaint();
		}, (TrickleDown)0);
		((VisualElement)this).style.width = StyleLength.op_Implicit((float)m_cols * m_cellWidth);
		((VisualElement)this).style.height = StyleLength.op_Implicit((float)m_rows * m_cellHeight);
		((VisualElement)this).style.left = StyleLength.op_Implicit(0f);
		((VisualElement)this).style.top = StyleLength.op_Implicit(0f);
		((VisualElement)this).style.position = StyleEnum<Position>.op_Implicit((Position)1);
	}

	private void InitGrid()
	{
		m_cellStates = new GridCellState[m_rows * m_cols];
		for (int i = 0; i < m_cellStates.Length; i++)
		{
			m_cellStates[i].alpha = 1f;
			m_cellStates[i].showBorder = false;
			m_cellStates[i].state = MapCellState.Locked;
		}
	}

	private int GetGridIndex(int x, int y)
	{
		int num = x + m_cols / 2;
		int num2 = m_rows / 2 - y;
		if (num < 0 || num >= m_cols || num2 < 0 || num2 >= m_rows)
		{
			return 0;
		}
		return num2 * m_cols + num;
	}

	public void SetCellState(int x, int y, MapCellState state, bool showBorder = false)
	{
		int gridIndex = GetGridIndex(x, y);
		m_cellStates[gridIndex].state = state;
		m_cellStates[gridIndex].showBorder = showBorder;
		m_cellStates[gridIndex].alpha = 1f;
	}

	public void BlinkCellAlpha(int x, int y, int blinkCount = 6, float freq = 5f)
	{
		int gridIndex = GetGridIndex(x, y);
		if (m_blinkTasks.ContainsKey(gridIndex))
		{
			return;
		}
		float duration = (float)blinkCount / freq;
		m_blinkTasks[gridIndex] = new BlinkTask
		{
			originalAlpha = m_cellStates[gridIndex].alpha,
			elapsed = 0f,
			duration = duration,
			freq = freq
		};
		if (!m_isBlinking)
		{
			m_isBlinking = true;
			IVisualElementScheduledItem blinkScheduledItem = m_blinkScheduledItem;
			if (blinkScheduledItem != null)
			{
				blinkScheduledItem.Pause();
			}
			m_blinkScheduledItem = ((VisualElement)this).schedule.Execute((Action)BlinkUpdate).Every(0L);
		}
	}

	private void BlinkUpdate()
	{
		if (m_blinkTasks.Count == 0)
		{
			m_isBlinking = false;
			IVisualElementScheduledItem blinkScheduledItem = m_blinkScheduledItem;
			if (blinkScheduledItem != null)
			{
				blinkScheduledItem.Pause();
			}
			m_blinkScheduledItem = null;
			return;
		}
		float deltaTime = Time.deltaTime;
		m_cachedBlinkKeys.Clear();
		m_cachedBlinkKeys.AddRange(m_blinkTasks.Keys);
		foreach (int cachedBlinkKey in m_cachedBlinkKeys)
		{
			BlinkTask blinkTask = m_blinkTasks[cachedBlinkKey];
			blinkTask.elapsed += deltaTime;
			if (blinkTask.elapsed >= blinkTask.duration)
			{
				m_cellStates[cachedBlinkKey].alpha = blinkTask.originalAlpha;
				m_blinkTasks.Remove(cachedBlinkKey);
			}
			else
			{
				float num = 0.8f * Mathf.Sin(blinkTask.elapsed * blinkTask.freq * (float)Math.PI * 2f);
				m_cellStates[cachedBlinkKey].alpha = Mathf.Clamp01(blinkTask.originalAlpha + num);
			}
		}
		((VisualElement)this).MarkDirtyRepaint();
	}

	public void StartDraw()
	{
		((VisualElement)this).MarkDirtyRepaint();
	}

	private void OnGenerateVisualContent(MeshGenerationContext ctx)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		Rect layout = ((VisualElement)this).layout;
		float width = ((Rect)(ref layout)).width;
		layout = ((VisualElement)this).layout;
		float height = ((Rect)(ref layout)).height;
		if (!float.IsNaN(width) && !float.IsNaN(height) && !(width <= 1f) && !(height <= 1f))
		{
			CollectQuads();
			if (m_lockedQuads.Count > 0 && (Object)(object)m_texLocked != (Object)null)
			{
				WriteTexturedQuads(ctx, (Texture)(object)m_texLocked, m_lockedQuads);
			}
			if (m_exploreQuads.Count > 0 && (Object)(object)m_texToExplore != (Object)null)
			{
				WriteTexturedQuads(ctx, (Texture)(object)m_texToExplore, m_exploreQuads);
			}
			if (m_solidQuads.Count > 0)
			{
				WriteSolidQuads(ctx, m_solidQuads);
			}
		}
	}

	private void AddQuad(List<Quad> list, Rect rect, Color color)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		list.Add(new Quad
		{
			rect = rect,
			color = color
		});
	}

	private void CollectQuads()
	{
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		m_lockedQuads.Clear();
		m_exploreQuads.Clear();
		m_solidQuads.Clear();
		Color color = default(Color);
		((Color)(ref color))._002Ector(31f / 51f, 31f / 51f, 31f / 51f, 0f);
		Rect rect = default(Rect);
		for (int i = 0; i < m_rows; i++)
		{
			float num = (float)i * m_cellHeight;
			for (int j = 0; j < m_cols; j++)
			{
				int num2 = i * m_cols + j;
				GridCellState gridCellState = m_cellStates[num2];
				float num3 = (float)j * m_cellWidth;
				((Rect)(ref rect))._002Ector(num3, num, m_cellWidth, m_cellHeight);
				if (gridCellState.state == MapCellState.Locked)
				{
					if ((Object)(object)m_texLocked != (Object)null)
					{
						AddQuad(m_lockedQuads, rect, new Color(1f, 1f, 1f, gridCellState.alpha));
					}
				}
				else if (gridCellState.state == MapCellState.ToExplore && (Object)(object)m_texToExplore != (Object)null)
				{
					AddQuad(m_exploreQuads, rect, new Color(1f, 1f, 1f, gridCellState.alpha));
				}
				if (gridCellState.alpha < 1f)
				{
					color.a = 1f - gridCellState.alpha;
					AddQuad(m_solidQuads, rect, color);
				}
				if (gridCellState.state != MapCellState.Locked)
				{
					CollectCellBorders(j, i, num3, num);
					CollectCornerPatches(j, i, num3, num);
				}
			}
		}
		CollectGridLines();
	}

	private void CollectCellBorders(int col, int row, float x, float y)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		if (IsLockedCell(col, row - 1))
		{
			AddQuad(m_solidQuads, new Rect(x, y, m_cellWidth, m_borderThickness), m_borderColor);
		}
		if (IsLockedCell(col, row + 1))
		{
			AddQuad(m_solidQuads, new Rect(x, y + m_cellHeight - m_borderThickness, m_cellWidth, m_borderThickness), m_borderColor);
		}
		if (IsLockedCell(col - 1, row))
		{
			AddQuad(m_solidQuads, new Rect(x, y, m_borderThickness, m_cellHeight), m_borderColor);
		}
		if (IsLockedCell(col + 1, row))
		{
			AddQuad(m_solidQuads, new Rect(x + m_cellWidth - m_borderThickness, y, m_borderThickness, m_cellHeight), m_borderColor);
		}
	}

	private void CollectCornerPatches(int col, int row, float x, float y)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		if (IsExplorable(col - 1, row) && IsExplorable(col, row + 1) && IsLockedCell(col - 1, row + 1))
		{
			AddQuad(m_solidQuads, new Rect(x, y + m_cellHeight - m_borderThickness, m_borderThickness, m_borderThickness), m_borderColor);
		}
		if (IsExplorable(col + 1, row) && IsExplorable(col, row + 1) && IsLockedCell(col + 1, row + 1))
		{
			AddQuad(m_solidQuads, new Rect(x + m_cellWidth - m_borderThickness, y + m_cellHeight - m_borderThickness, m_borderThickness, m_borderThickness), m_borderColor);
		}
		if (IsExplorable(col - 1, row) && IsExplorable(col, row - 1) && IsLockedCell(col - 1, row - 1))
		{
			AddQuad(m_solidQuads, new Rect(x, y, m_borderThickness, m_borderThickness), m_borderColor);
		}
		if (IsExplorable(col + 1, row) && IsExplorable(col, row - 1) && IsLockedCell(col + 1, row - 1))
		{
			AddQuad(m_solidQuads, new Rect(x + m_cellWidth - m_borderThickness, y, m_borderThickness, m_borderThickness), m_borderColor);
		}
	}

	private void CollectGridLines()
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		float num = m_cellWidth * (float)m_cols;
		float num2 = m_cellHeight * (float)m_rows;
		float num3 = ((m_lineWidth % 2f == 1f) ? 0.5f : 0f);
		for (int i = 0; i <= m_rows; i++)
		{
			float num4 = (float)i * m_cellHeight + num3;
			AddQuad(m_solidQuads, new Rect(0f, num4 - 0.5f, num, 1f), m_lineColor);
		}
		for (int j = 0; j <= m_cols; j++)
		{
			float num5 = (float)j * m_cellWidth + num3;
			AddQuad(m_solidQuads, new Rect(num5 - 0.5f, 0f, 1f, num2), m_lineColor);
		}
	}

	private void WriteTexturedQuads(MeshGenerationContext ctx, Texture texture, List<Quad> quads)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < quads.Count; i += 16000)
		{
			int num = Mathf.Min(16000, quads.Count - i);
			MeshWriteData val = ctx.Allocate(num * 4, num * 6, texture);
			Rect uvRegion = val.uvRegion;
			Vector2 min = ((Rect)(ref uvRegion)).min;
			Vector2 size = ((Rect)(ref uvRegion)).size;
			Vector2 uv = new Vector2(1f, 0f) * size + min;
			Vector2 uv2 = new Vector2(0f, 0f) * size + min;
			Vector2 uv3 = new Vector2(0f, 1f) * size + min;
			Vector2 uv4 = new Vector2(1f, 1f) * size + min;
			Vertex nextVertex = default(Vertex);
			ushort num2 = 0;
			for (int j = 0; j < num; j++)
			{
				Quad quad = quads[i + j];
				float xMin = ((Rect)(ref quad.rect)).xMin;
				float yMin = ((Rect)(ref quad.rect)).yMin;
				float xMax = ((Rect)(ref quad.rect)).xMax;
				float yMax = ((Rect)(ref quad.rect)).yMax;
				nextVertex.tint = Color32.op_Implicit(quad.color);
				nextVertex.position = new Vector3(xMin, yMin, Vertex.nearZ);
				nextVertex.uv = uv;
				val.SetNextVertex(nextVertex);
				nextVertex.position = new Vector3(xMax, yMin, Vertex.nearZ);
				nextVertex.uv = uv2;
				val.SetNextVertex(nextVertex);
				nextVertex.position = new Vector3(xMax, yMax, Vertex.nearZ);
				nextVertex.uv = uv3;
				val.SetNextVertex(nextVertex);
				nextVertex.position = new Vector3(xMin, yMax, Vertex.nearZ);
				nextVertex.uv = uv4;
				val.SetNextVertex(nextVertex);
				val.SetNextIndex(num2);
				val.SetNextIndex((ushort)(num2 + 1));
				val.SetNextIndex((ushort)(num2 + 2));
				val.SetNextIndex(num2);
				val.SetNextIndex((ushort)(num2 + 2));
				val.SetNextIndex((ushort)(num2 + 3));
				num2 += 4;
			}
		}
	}

	private void WriteSolidQuads(MeshGenerationContext ctx, List<Quad> quads)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < quads.Count; i += 16000)
		{
			int num = Mathf.Min(16000, quads.Count - i);
			MeshWriteData val = ctx.Allocate(num * 4, num * 6, (Texture)null);
			Vertex nextVertex = new Vertex
			{
				uv = Vector2.zero
			};
			ushort num2 = 0;
			for (int j = 0; j < num; j++)
			{
				Quad quad = quads[i + j];
				float xMin = ((Rect)(ref quad.rect)).xMin;
				float yMin = ((Rect)(ref quad.rect)).yMin;
				float xMax = ((Rect)(ref quad.rect)).xMax;
				float yMax = ((Rect)(ref quad.rect)).yMax;
				nextVertex.tint = Color32.op_Implicit(quad.color);
				nextVertex.position = new Vector3(xMin, yMin, Vertex.nearZ);
				val.SetNextVertex(nextVertex);
				nextVertex.position = new Vector3(xMax, yMin, Vertex.nearZ);
				val.SetNextVertex(nextVertex);
				nextVertex.position = new Vector3(xMax, yMax, Vertex.nearZ);
				val.SetNextVertex(nextVertex);
				nextVertex.position = new Vector3(xMin, yMax, Vertex.nearZ);
				val.SetNextVertex(nextVertex);
				val.SetNextIndex(num2);
				val.SetNextIndex((ushort)(num2 + 1));
				val.SetNextIndex((ushort)(num2 + 2));
				val.SetNextIndex(num2);
				val.SetNextIndex((ushort)(num2 + 2));
				val.SetNextIndex((ushort)(num2 + 3));
				num2 += 4;
			}
		}
	}

	private bool IsLockedCell(int col, int row)
	{
		if (col < 0 || col >= m_cols || row < 0 || row >= m_rows)
		{
			return true;
		}
		int num = row * m_cols + col;
		return m_cellStates[num].state == MapCellState.Locked;
	}

	private bool IsExplorable(int col, int row)
	{
		if (col < 0 || col >= m_cols || row < 0 || row >= m_rows)
		{
			return false;
		}
		int num = row * m_cols + col;
		MapCellState state = m_cellStates[num].state;
		if (state != MapCellState.Unlocked)
		{
			return state == MapCellState.ToExplore;
		}
		return true;
	}

	public Vector2 GetCellVectByPosition(Vector2 pos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		int num = Mathf.FloorToInt(pos.x / m_cellWidth);
		int num2 = Mathf.FloorToInt(pos.y / m_cellHeight);
		int num3 = num - m_cols / 2;
		int num4 = m_rows / 2 - num2;
		return new Vector2((float)num3, (float)num4);
	}

	public Vector2? PickElementAt(Vector2 mousePosition)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		int num = Mathf.FloorToInt(mousePosition.x / m_cellWidth);
		int num2 = Mathf.FloorToInt(mousePosition.y / m_cellHeight);
		if (num < 0 || num >= m_cols || num2 < 0 || num2 >= m_rows)
		{
			return null;
		}
		int num3 = num - m_cols / 2;
		int num4 = m_rows / 2 - num2;
		return new Vector2((float)num3, (float)num4);
	}
}
