using System;
using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbDungeonMapCanvas : VisualElement
{
	public class UxmlFactory : UxmlFactory<zbDungeonMapCanvas>
	{
	}

	private struct LineQuad
	{
		public Vector2 a;

		public Vector2 b;

		public Vector2 c;

		public Vector2 d;

		public Color color;
	}

	private struct CircleDraw
	{
		public Vector2 center;

		public float radius;

		public Color color;
	}

	public const float CellSpacing = 30f;

	private const float PointSize = 12f;

	internal static readonly bool kUseMeshDirect;

	private const int CircleSegments = 32;

	private const int MaxVertsPerMesh = 65000;

	private const int MaxQuadsPerMesh = 16000;

	public Dictionary<Vector2Int, DungeonMapCellDetail> m_cellMap;

	private Vector2Int? m_playerPosition;

	private Color exploredColor;

	private Color visibleColor;

	private Color playerColor;

	private RenderTexture m_RT;

	private Material m_Mat;

	private Texture2D m_Tex2D;

	private static Vector2 m_contentOffset;

	private readonly List<LineQuad> m_lines;

	private readonly List<CircleDraw> m_circles;

	private float MapHeight => ((VisualElement)this).resolvedStyle.height;

	public bool IsMoving => false;

	public Vector2Int GetCurrentPlayerGrid()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		return (Vector2Int)(((_003F?)m_playerPosition) ?? new Vector2Int(-1, -1));
	}

	public bool CanMoveTo(Vector2Int newPos)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if (m_cellMap != null)
		{
			return m_cellMap.ContainsKey(newPos);
		}
		return false;
	}

	public void MovePlayerTo(Vector2Int newPos)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		Vector2Int? playerPosition = m_playerPosition;
		if ((!playerPosition.HasValue || (playerPosition.HasValue && !(playerPosition.GetValueOrDefault() == newPos))) && CanMoveTo(newPos))
		{
			m_playerPosition = newPos;
			RequestRender();
		}
	}

	public zbDungeonMapCanvas()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		exploredColor = Color.yellow;
		visibleColor = Color.gray;
		playerColor = Color.blue;
		m_lines = new List<LineQuad>();
		m_circles = new List<CircleDraw>();
		((VisualElement)this)._002Ector();
		((VisualElement)this).pickingMode = (PickingMode)1;
		((VisualElement)this).style.flexShrink = StyleFloat.op_Implicit(1f);
		((VisualElement)this).style.alignSelf = StyleEnum<Align>.op_Implicit((Align)2);
		((VisualElement)this).style.justifyContent = StyleEnum<Justify>.op_Implicit((Justify)1);
		((VisualElement)this).style.alignItems = StyleEnum<Align>.op_Implicit((Align)2);
		((CallbackEventHandler)this).RegisterCallback<AttachToPanelEvent>((EventCallback<AttachToPanelEvent>)OnAttached, (TrickleDown)0);
		((CallbackEventHandler)this).RegisterCallback<DetachFromPanelEvent>((EventCallback<DetachFromPanelEvent>)OnDetached, (TrickleDown)0);
		((CallbackEventHandler)this).RegisterCallback<GeometryChangedEvent>((EventCallback<GeometryChangedEvent>)OnGeometryChanged, (TrickleDown)0);
		if (kUseMeshDirect)
		{
			((VisualElement)this).generateVisualContent = (Action<MeshGenerationContext>)Delegate.Combine(((VisualElement)this).generateVisualContent, new Action<MeshGenerationContext>(OnGenerateVisualContent));
		}
	}

	private void RequestRender()
	{
		if (kUseMeshDirect)
		{
			((VisualElement)this).MarkDirtyRepaint();
		}
		else
		{
			RenderFallback();
		}
	}

	private void OnAttached(AttachToPanelEvent evt)
	{
		if (!kUseMeshDirect)
		{
			((VisualElement)this).schedule.Execute((Action)delegate
			{
				RenderFallback();
			});
		}
	}

	private void OnDetached(DetachFromPanelEvent evt)
	{
		DisposeResources();
	}

	private void OnGeometryChanged(GeometryChangedEvent evt)
	{
		RequestRender();
	}

	public void SetMap(Dictionary<Vector2Int, DungeonMapCellDetail> fullCellMap, Vector2Int? playerPos)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		m_cellMap = new Dictionary<Vector2Int, DungeonMapCellDetail>();
		foreach (KeyValuePair<Vector2Int, DungeonMapCellDetail> item in fullCellMap)
		{
			DungeonMapCellDetail value = item.Value;
			if (value.FunType == EDungeonMapCellFunType.Elevator || value.FunType == EDungeonMapCellFunType.Spawn)
			{
				m_cellMap[item.Key] = value;
			}
			else if (value.FunType == EDungeonMapCellFunType.Room && (value.State == EDungeonMapCellState.Visible || value.State == EDungeonMapCellState.LeftOpen || value.State == EDungeonMapCellState.RightOpen || value.State == EDungeonMapCellState.AllOpen))
			{
				m_cellMap[item.Key] = value;
			}
		}
		m_playerPosition = playerPos;
		RequestRender();
	}

	public void SetContentOffset(Vector2 offset)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		m_contentOffset = offset;
		RequestRender();
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
		if (!float.IsNaN(width) && !float.IsNaN(height) && !(width <= 1f) && !(height <= 1f) && m_cellMap != null)
		{
			CollectPrimitives();
			WriteLinesToMesh(ctx);
			WriteCirclesToMesh(ctx);
		}
	}

	private void WriteLinesToMesh(MeshGenerationContext ctx)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		if (m_lines.Count == 0)
		{
			return;
		}
		for (int i = 0; i < m_lines.Count; i += 16000)
		{
			int num = Mathf.Min(16000, m_lines.Count - i);
			MeshWriteData val = ctx.Allocate(num * 4, num * 6, (Texture)null);
			Vertex nextVertex = new Vertex
			{
				uv = Vector2.zero
			};
			ushort num2 = 0;
			for (int j = 0; j < num; j++)
			{
				LineQuad lineQuad = m_lines[i + j];
				nextVertex.tint = Color32.op_Implicit(lineQuad.color);
				nextVertex.position = new Vector3(lineQuad.a.x, lineQuad.a.y, Vertex.nearZ);
				val.SetNextVertex(nextVertex);
				nextVertex.position = new Vector3(lineQuad.b.x, lineQuad.b.y, Vertex.nearZ);
				val.SetNextVertex(nextVertex);
				nextVertex.position = new Vector3(lineQuad.c.x, lineQuad.c.y, Vertex.nearZ);
				val.SetNextVertex(nextVertex);
				nextVertex.position = new Vector3(lineQuad.d.x, lineQuad.d.y, Vertex.nearZ);
				val.SetNextVertex(nextVertex);
				val.SetNextIndex(num2);
				val.SetNextIndex((ushort)(num2 + 1));
				val.SetNextIndex((ushort)(num2 + 2));
				val.SetNextIndex((ushort)(num2 + 1));
				val.SetNextIndex((ushort)(num2 + 3));
				val.SetNextIndex((ushort)(num2 + 2));
				num2 += 4;
			}
		}
	}

	private void WriteCirclesToMesh(MeshGenerationContext ctx)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		if (m_circles.Count == 0)
		{
			return;
		}
		int num = 34;
		int num2 = Mathf.Max(1, 65000 / num);
		for (int i = 0; i < m_circles.Count; i += num2)
		{
			int num3 = Mathf.Min(num2, m_circles.Count - i);
			MeshWriteData val = ctx.Allocate(num3 * num, num3 * 32 * 3, (Texture)null);
			Vertex nextVertex = new Vertex
			{
				uv = Vector2.zero
			};
			ushort num4 = 0;
			for (int j = 0; j < num3; j++)
			{
				CircleDraw circleDraw = m_circles[i + j];
				nextVertex.tint = Color32.op_Implicit(circleDraw.color);
				nextVertex.position = new Vector3(circleDraw.center.x, circleDraw.center.y, Vertex.nearZ);
				val.SetNextVertex(nextVertex);
				for (int k = 0; k <= 32; k++)
				{
					float num5 = (float)Math.PI * 2f * (float)k / 32f;
					nextVertex.position = new Vector3(circleDraw.center.x + Mathf.Cos(num5) * circleDraw.radius, circleDraw.center.y + Mathf.Sin(num5) * circleDraw.radius, Vertex.nearZ);
					val.SetNextVertex(nextVertex);
				}
				for (int l = 0; l < 32; l++)
				{
					val.SetNextIndex(num4);
					val.SetNextIndex((ushort)(num4 + 1 + l));
					val.SetNextIndex((ushort)(num4 + 2 + l));
				}
				num4 += (ushort)num;
			}
		}
	}

	private void CollectPrimitives()
	{
		m_lines.Clear();
		m_circles.Clear();
		CollectConnections();
		CollectCells();
		CollectPlayer();
	}

	private void AddCircle(Vector2 center, float radius, Color color)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		m_circles.Add(new CircleDraw
		{
			center = center,
			radius = radius,
			color = color
		});
	}

	private void AddLine(Vector2 start, Vector2 end, Color color, float lineWidth)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = end - start;
		float magnitude = ((Vector2)(ref val)).magnitude;
		if (!(magnitude < 0.001f))
		{
			Vector2 val2 = new Vector2(0f - val.y, val.x) / magnitude * (lineWidth * 0.5f);
			m_lines.Add(new LineQuad
			{
				a = start + val2,
				b = start - val2,
				c = end + val2,
				d = end - val2,
				color = color
			});
		}
	}

	private void CollectConnections()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		CalculateContentOffset(m_cellMap);
		Vector2Int val = default(Vector2Int);
		Vector2Int val2 = default(Vector2Int);
		Vector2Int val3 = default(Vector2Int);
		foreach (KeyValuePair<Vector2Int, DungeonMapCellDetail> item in m_cellMap)
		{
			Vector2Int key = item.Key;
			DungeonMapCellDetail value = item.Value;
			if (!IsExploredOrVisible(value))
			{
				continue;
			}
			int cellWidth = GetCellWidth(value);
			((Vector2Int)(ref val))._002Ector(((Vector2Int)(ref key)).x + cellWidth, ((Vector2Int)(ref key)).y);
			if (m_cellMap.TryGetValue(val, out var value2) && IsExploredOrVisible(value2) && CanConnect(value, value2, out var isExploredLine))
			{
				Vector2 start = GetCellMiddleGridCenter(key, value) + m_contentOffset;
				Vector2 end = GetCellMiddleGridCenter(val, value2) + m_contentOffset;
				AddLine(start, end, isExploredLine ? exploredColor : visibleColor, 2f);
			}
			if (value.FunType == EDungeonMapCellFunType.Elevator)
			{
				((Vector2Int)(ref val2))._002Ector(((Vector2Int)(ref key)).x, ((Vector2Int)(ref key)).y - 1);
				((Vector2Int)(ref val3))._002Ector(((Vector2Int)(ref key)).x, ((Vector2Int)(ref key)).y + 1);
				if (m_cellMap.TryGetValue(val2, out var value3) && value3.FunType == EDungeonMapCellFunType.Elevator && IsExplored(value3) && IsExplored(value))
				{
					Vector2 start2 = GetCellCenter(key, value) + m_contentOffset;
					Vector2 end2 = GetCellCenter(val2, value3) + m_contentOffset;
					AddDashedLine(start2, end2, Color.red);
				}
				if (m_cellMap.TryGetValue(val3, out var value4) && value4.FunType == EDungeonMapCellFunType.Elevator && IsExplored(value4) && IsExplored(value))
				{
					Vector2 start3 = GetCellCenter(key, value) + m_contentOffset;
					Vector2 end3 = GetCellCenter(val3, value4) + m_contentOffset;
					AddDashedLine(start3, end3, Color.red);
				}
			}
		}
	}

	private void AddDashedLine(Vector2 start, Vector2 end, Color color, float lineWidth = 2f, float dashLength = 5f, float gapLength = 3f)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = end - start;
		Vector2 normalized = ((Vector2)(ref val)).normalized;
		float num = Vector2.Distance(start, end);
		if (!(num < 0.001f))
		{
			for (float num2 = 0f; num2 < num; num2 += dashLength + gapLength)
			{
				float num3 = Mathf.Min(num2 + dashLength, num);
				Vector2 start2 = start + normalized * num2;
				Vector2 end2 = start + normalized * num3;
				AddLine(start2, end2, color, lineWidth);
			}
		}
	}

	private void CollectCells()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		Vector2Int val = default(Vector2Int);
		foreach (KeyValuePair<Vector2Int, DungeonMapCellDetail> item in m_cellMap)
		{
			Vector2Int key = item.Key;
			DungeonMapCellDetail value = item.Value;
			if (ShouldDisplay(value))
			{
				int cellWidth = GetCellWidth(value);
				Color cellColor = GetCellColor(value, isPlayer: false);
				for (int i = 0; i < cellWidth; i++)
				{
					((Vector2Int)(ref val))._002Ector(((Vector2Int)(ref key)).x + i, ((Vector2Int)(ref key)).y);
					Vector2 center = new Vector2(((float)((Vector2Int)(ref val)).x + 0.5f) * 30f, MapHeight - (float)((Vector2Int)(ref val)).y * 30f) + m_contentOffset;
					AddCircle(center, 6f, cellColor);
				}
			}
		}
	}

	private void CollectPlayer()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		if (m_playerPosition.HasValue && m_cellMap.TryGetValue(m_playerPosition.Value, out var value))
		{
			int cellWidth = GetCellWidth(value);
			Vector2Int val = default(Vector2Int);
			for (int i = 0; i < cellWidth; i++)
			{
				Vector2Int value2 = m_playerPosition.Value;
				int num = ((Vector2Int)(ref value2)).x + i;
				value2 = m_playerPosition.Value;
				((Vector2Int)(ref val))._002Ector(num, ((Vector2Int)(ref value2)).y);
				Vector2 center = new Vector2(((float)((Vector2Int)(ref val)).x + 0.5f) * 30f, MapHeight - (float)((Vector2Int)(ref val)).y * 30f) + m_contentOffset;
				AddCircle(center, 6f, playerColor);
			}
		}
	}

	private void RenderFallback()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Expected O, but got Unknown
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Expected O, but got Unknown
		//IL_0476: Unknown result type (might be due to invalid IL or missing references)
		//IL_0480: Expected O, but got Unknown
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_027f: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_032d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0339: Unknown result type (might be due to invalid IL or missing references)
		//IL_034f: Unknown result type (might be due to invalid IL or missing references)
		//IL_036b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0391: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ad: Unknown result type (might be due to invalid IL or missing references)
		Rect layout = ((VisualElement)this).layout;
		float width = ((Rect)(ref layout)).width;
		layout = ((VisualElement)this).layout;
		float height = ((Rect)(ref layout)).height;
		if (float.IsNaN(width) || float.IsNaN(height) || width <= 1f || height <= 1f || m_cellMap == null)
		{
			return;
		}
		int num = Mathf.Max(2, Mathf.CeilToInt(width));
		int num2 = Mathf.Max(2, Mathf.CeilToInt(height));
		if ((Object)(object)m_RT == (Object)null || ((Texture)m_RT).width != num || ((Texture)m_RT).height != num2)
		{
			if ((Object)(object)m_RT != (Object)null)
			{
				m_RT.Release();
			}
			m_RT = new RenderTexture(num, num2, 0, (RenderTextureFormat)0);
			((Texture)m_RT).filterMode = (FilterMode)1;
		}
		if ((Object)(object)m_Mat == (Object)null)
		{
			Shader val = Shader.Find("Hidden/Internal-Colored");
			if ((Object)(object)val == (Object)null)
			{
				val = Shader.Find("UI/Default");
			}
			if ((Object)(object)val == (Object)null)
			{
				val = Shader.Find("Sprites/Default");
			}
			if ((Object)(object)val != (Object)null)
			{
				m_Mat = new Material(val);
			}
		}
		if ((Object)(object)m_Mat == (Object)null)
		{
			return;
		}
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = m_RT;
		GL.Clear(true, true, new Color(0f, 0f, 0f, 0f));
		GL.PushMatrix();
		GL.LoadPixelMatrix(0f, width, height, 0f);
		CollectPrimitives();
		if (m_lines.Count > 0)
		{
			m_Mat.SetPass(0);
			GL.Begin(4);
			foreach (LineQuad line in m_lines)
			{
				GL.Color(line.color);
				GL.Vertex3(line.a.x, line.a.y, 0f);
				GL.Vertex3(line.b.x, line.b.y, 0f);
				GL.Vertex3(line.c.x, line.c.y, 0f);
				GL.Vertex3(line.b.x, line.b.y, 0f);
				GL.Vertex3(line.d.x, line.d.y, 0f);
				GL.Vertex3(line.c.x, line.c.y, 0f);
			}
			GL.End();
		}
		if (m_circles.Count > 0)
		{
			m_Mat.SetPass(0);
			GL.Begin(4);
			foreach (CircleDraw circle in m_circles)
			{
				GL.Color(circle.color);
				for (int i = 0; i < 32; i++)
				{
					float num3 = (float)Math.PI * 2f * (float)i / 32f;
					float num4 = (float)Math.PI * 2f * (float)(i + 1) / 32f;
					GL.Vertex3(circle.center.x, circle.center.y, 0f);
					GL.Vertex3(circle.center.x + Mathf.Cos(num3) * circle.radius, circle.center.y + Mathf.Sin(num3) * circle.radius, 0f);
					GL.Vertex3(circle.center.x + Mathf.Cos(num4) * circle.radius, circle.center.y + Mathf.Sin(num4) * circle.radius, 0f);
				}
			}
			GL.End();
		}
		GL.PopMatrix();
		if ((Object)(object)m_Tex2D == (Object)null || ((Texture)m_Tex2D).width != ((Texture)m_RT).width || ((Texture)m_Tex2D).height != ((Texture)m_RT).height)
		{
			if ((Object)(object)m_Tex2D != (Object)null)
			{
				Object.Destroy((Object)(object)m_Tex2D);
			}
			m_Tex2D = new Texture2D(((Texture)m_RT).width, ((Texture)m_RT).height, (TextureFormat)5, false);
		}
		m_Tex2D.ReadPixels(new Rect(0f, 0f, (float)((Texture)m_RT).width, (float)((Texture)m_RT).height), 0, 0);
		m_Tex2D.Apply();
		RenderTexture.active = active;
		((VisualElement)this).style.backgroundImage = new StyleBackground(Background.FromTexture2D(m_Tex2D));
	}

	private void DisposeResources()
	{
		if ((Object)(object)m_RT != (Object)null)
		{
			m_RT.Release();
			m_RT = null;
		}
		if ((Object)(object)m_Mat != (Object)null)
		{
			Object.Destroy((Object)(object)m_Mat);
			m_Mat = null;
		}
		if ((Object)(object)m_Tex2D != (Object)null)
		{
			Object.Destroy((Object)(object)m_Tex2D);
			m_Tex2D = null;
		}
	}

	private Vector2 GetCellMiddleGridCenter(Vector2Int pos, DungeonMapCellDetail cell)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		int num = GetCellWidth(cell) / 2;
		Vector2Int val = default(Vector2Int);
		((Vector2Int)(ref val))._002Ector(((Vector2Int)(ref pos)).x + num, ((Vector2Int)(ref pos)).y);
		return new Vector2(((float)((Vector2Int)(ref val)).x + 0.5f) * 30f, MapHeight - (float)((Vector2Int)(ref val)).y * 30f);
	}

	private void CalculateContentOffset(Dictionary<Vector2Int, DungeonMapCellDetail> cellMap)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		int num = int.MaxValue;
		int num2 = int.MinValue;
		int num3 = int.MaxValue;
		int num4 = int.MinValue;
		foreach (KeyValuePair<Vector2Int, DungeonMapCellDetail> item in cellMap)
		{
			DungeonMapCellDetail value = item.Value;
			int cellWidth = GetCellWidth(value);
			int num5 = num;
			Vector2Int key = item.Key;
			num = Mathf.Min(num5, ((Vector2Int)(ref key)).x);
			int num6 = num2;
			key = item.Key;
			num2 = Mathf.Max(num6, ((Vector2Int)(ref key)).x + cellWidth - 1);
			int num7 = num3;
			key = item.Key;
			num3 = Mathf.Min(num7, ((Vector2Int)(ref key)).y);
			int num8 = num4;
			key = item.Key;
			num4 = Mathf.Max(num8, ((Vector2Int)(ref key)).y);
		}
		float num9 = (float)(num2 - num + 1) * 30f;
		float num10 = (float)(num4 - num3 + 1) * 30f;
		float width = ((VisualElement)this).resolvedStyle.width;
		float height = ((VisualElement)this).resolvedStyle.height;
		m_contentOffset = new Vector2((width - num9) * 0.5f - (float)num * 30f, (height - num10) * 0.5f - (float)num3 * 30f);
	}

	private int GetCellWidth(DungeonMapCellDetail cell)
	{
		return Mathf.Clamp(cell.GridList?.Count ?? 1, 1, 3);
	}

	private bool ShouldDisplay(DungeonMapCellDetail cell)
	{
		if (cell.FunType == EDungeonMapCellFunType.Elevator || cell.FunType == EDungeonMapCellFunType.Spawn)
		{
			return true;
		}
		if (cell.State == EDungeonMapCellState.Visible)
		{
			return true;
		}
		return IsExplored(cell);
	}

	private bool IsExplored(DungeonMapCellDetail cell)
	{
		return cell.State != EDungeonMapCellState.Visible;
	}

	private bool IsExploredOrVisible(DungeonMapCellDetail cell)
	{
		if (cell.FunType != EDungeonMapCellFunType.Elevator && cell.FunType != EDungeonMapCellFunType.Spawn && cell.State != EDungeonMapCellState.Visible)
		{
			return IsExplored(cell);
		}
		return true;
	}

	private bool IsVisibleOnly(DungeonMapCellDetail cell)
	{
		return cell.State == EDungeonMapCellState.Visible;
	}

	public Vector2 GetCellCenter(Vector2Int pos, DungeonMapCellDetail cell)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		float num = cell.GridList?.Count ?? 1;
		return new Vector2(((float)((Vector2Int)(ref pos)).x + num / 2f) * 30f, MapHeight - (float)((Vector2Int)(ref pos)).y * 30f);
	}

	private Color GetCellColor(DungeonMapCellDetail cell, bool isPlayer)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (isPlayer)
		{
			return playerColor;
		}
		if (cell.FunType == EDungeonMapCellFunType.Elevator || cell.FunType == EDungeonMapCellFunType.Spawn)
		{
			return exploredColor;
		}
		if (cell.FunType == EDungeonMapCellFunType.Room)
		{
			if (cell.State == EDungeonMapCellState.Visible)
			{
				return visibleColor;
			}
			return exploredColor;
		}
		return exploredColor;
	}

	private bool CanConnect(DungeonMapCellDetail left, DungeonMapCellDetail right, out bool isExploredLine)
	{
		isExploredLine = false;
		if (left == null || right == null)
		{
			return false;
		}
		bool flag = IsExplored(left);
		bool flag2 = IsExplored(right);
		bool flag3 = left.FunType == EDungeonMapCellFunType.Elevator;
		bool flag4 = right.FunType == EDungeonMapCellFunType.Elevator;
		bool flag5 = left.State == EDungeonMapCellState.RightOpen || left.State == EDungeonMapCellState.AllOpen;
		bool flag6 = right.State == EDungeonMapCellState.LeftOpen || right.State == EDungeonMapCellState.AllOpen;
		if (left.State == EDungeonMapCellState.Visible || right.State == EDungeonMapCellState.Visible)
		{
			isExploredLine = false;
			return true;
		}
		if (flag & flag2 & flag5 & flag6)
		{
			isExploredLine = true;
			return true;
		}
		if (flag3 & flag6 & flag2)
		{
			isExploredLine = true;
			return true;
		}
		if (flag4 & flag5 & flag)
		{
			isExploredLine = true;
			return true;
		}
		return false;
	}

	static zbDungeonMapCanvas()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		kUseMeshDirect = true;
		m_contentOffset = Vector2.zero;
	}
}
