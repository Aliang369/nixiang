using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbMiniMapCanvas : VisualElement
{
	public class UxmlFactory : UxmlFactory<zbMiniMapCanvas>
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

	private Texture2D m_texPlayer;

	private Texture2D m_texBoss;

	private Texture2D m_texEntry;

	private Texture2D m_texUnknown;

	private Texture2D m_texNormal;

	private readonly float m_iconSize;

	private readonly float m_gridSize;

	private readonly float m_lineWidth;

	public Dictionary<int, zbDungeonCellTypeCsvRecord> dungeonCellTypeRecord;

	[NonSerialized]
	public readonly float m_gridSpacingX;

	[NonSerialized]
	public readonly float m_gridSpacingY;

	private bool m_renderMod;

	private readonly Color sGridColorExplored;

	private readonly Color sGridColorNotExplore;

	private readonly Color sConnLineColorExplored;

	private readonly Color sConnLineColorNotExplore;

	private readonly Color sGridElevator;

	private readonly Color sDashLineColor;

	private Dictionary<(int, int), MapGridDetail> m_gridMap;

	private int m_gridMapMaxY;

	private int m_gridMapMaxX;

	[NonSerialized]
	public Vector2 m_playerPos;

	private zbMiniMapStateMachine m_fsm;

	[NonSerialized]
	public int nCurPlayerGridX;

	[NonSerialized]
	public int nCurPlayerGridY;

	internal static readonly bool kUseMeshDirect = true;

	private const int CircleSegments = 24;

	private const int MaxVertsPerMesh = 65000;

	private const int MaxQuadsPerMesh = 16000;

	private RenderTexture m_RT;

	private Material m_Mat;

	private Texture2D m_Tex2D;

	private readonly List<LineQuad> m_lines;

	private readonly List<CircleDraw> m_circles;

	private readonly List<Rect> m_unknownIcons;

	private readonly List<Rect> m_bossIcons;

	private readonly List<Rect> m_entryIcons;

	private readonly List<Rect> m_playerIcons;

	public zbMiniMapCanvas()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		m_iconSize = 32f;
		m_gridSize = 8f;
		m_lineWidth = 2f;
		m_gridSpacingX = 64f;
		m_gridSpacingY = 64f;
		m_renderMod = true;
		sGridColorExplored = new Color(1f, 1f, 0.3f, 1f);
		sGridColorNotExplore = new Color(0.5f, 0.5f, 0.5f, 1f);
		sConnLineColorExplored = Color.yellow;
		sConnLineColorNotExplore = new Color(0.35686275f, 24f / 85f, 23f / 85f, 1f);
		sGridElevator = new Color(1f, 1f, 0.3f, 1f);
		sDashLineColor = Color.red;
		m_gridMap = new Dictionary<(int, int), MapGridDetail>();
		m_playerPos = Vector2.zero;
		m_lines = new List<LineQuad>();
		m_circles = new List<CircleDraw>();
		m_unknownIcons = new List<Rect>();
		m_bossIcons = new List<Rect>();
		m_entryIcons = new List<Rect>();
		m_playerIcons = new List<Rect>();
		((VisualElement)this)._002Ector();
		((VisualElement)this).pickingMode = (PickingMode)1;
		((CallbackEventHandler)this).RegisterCallback<AttachToPanelEvent>((EventCallback<AttachToPanelEvent>)OnAttachToPanel, (TrickleDown)0);
		((CallbackEventHandler)this).RegisterCallback<DetachFromPanelEvent>((EventCallback<DetachFromPanelEvent>)OnDetachFromPanel, (TrickleDown)0);
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

	private void OnAttachToPanel(AttachToPanelEvent evt)
	{
		if (!kUseMeshDirect)
		{
			RenderFallback();
		}
	}

	private void OnDetachFromPanel(DetachFromPanelEvent evt)
	{
		DisposeResources();
	}

	private void OnGeometryChanged(GeometryChangedEvent evt)
	{
		RequestRender();
	}

	public void OnEnable()
	{
		nCurPlayerGridX = 0;
		nCurPlayerGridY = 0;
		m_texEntry = zbAssetLoader.Instance.LoadResouce<Texture2D>("ui/image/dungeon/minimap_entry.png");
		m_texPlayer = zbAssetLoader.Instance.LoadResouce<Texture2D>("ui/image/dungeon/minimap_player.png");
		m_texBoss = zbAssetLoader.Instance.LoadResouce<Texture2D>("ui/image/dungeon/minimap_boss.png");
		m_texUnknown = zbAssetLoader.Instance.LoadResouce<Texture2D>("ui/image/dungeon/minimap_unknown.png");
		m_texNormal = Texture2D.whiteTexture;
		dungeonCellTypeRecord = zbDataConfig.Instance.dungeonCellTypeCsvTable.GetRecords().records;
		m_fsm = new zbMiniMapStateMachine();
		m_fsm.Init(this);
	}

	public void ConvertToMiniMap(DungeonMapInfo mapInfo)
	{
		m_gridMap.Clear();
		foreach (DungeonMapCellDetail cell in mapInfo.CellList)
		{
			int x = cell.X;
			int y = cell.Y;
			int num = 0;
			int count = cell.GridList.Count;
			bool hasLeftDoor = cell.State == EDungeonMapCellState.LeftOpen || cell.State == EDungeonMapCellState.AllOpen;
			bool hasRightDoor = cell.State == EDungeonMapCellState.RightOpen || cell.State == EDungeonMapCellState.AllOpen;
			bool hasBoss = false;
			foreach (DungeonMapGridDetail grid in cell.GridList)
			{
				if (grid.EventType == EDungeonEventType.BossFight)
				{
					hasBoss = true;
					break;
				}
			}
			foreach (DungeonMapGridDetail grid2 in cell.GridList)
			{
				MapGridDetail value = new MapGridDetail
				{
					X = x + num - 1,
					Y = y - 1,
					State = ((cell.State != EDungeonMapCellState.Visible) ? ((cell.State != EDungeonMapCellState.Invisible) ? EGridState.Explored : EGridState.Invisible) : EGridState.NotExplore)
				};
				switch (count)
				{
				case 1:
					value.HasLeftDoor = hasLeftDoor;
					break;
				case 2:
					value.HasLeftDoor = num == 1;
					if (num == 0)
					{
						value.HasLeftDoor = hasLeftDoor;
					}
					break;
				case 3:
					value.HasLeftDoor = num == 1 || num == 2;
					if (num == 0)
					{
						value.HasLeftDoor = hasLeftDoor;
					}
					break;
				}
				switch (count)
				{
				case 1:
					value.HasRightDoor = hasRightDoor;
					break;
				case 2:
					value.HasRightDoor = num == 0;
					if (num == 1)
					{
						value.HasRightDoor = hasRightDoor;
					}
					break;
				case 3:
					value.HasRightDoor = num == 0 || num == 1;
					if (num == 2)
					{
						value.HasRightDoor = hasRightDoor;
					}
					break;
				}
				value.HasBoss = hasBoss;
				value.HasPendingEvent = grid2.State == EActiveState.Pending && grid2.EventId > 0 && grid2.EventType != EDungeonEventType.EmptyValue;
				value.IsElevator = cell.FunType == EDungeonMapCellFunType.Elevator;
				value.IsEntry = num == 0 && cell.FunType == EDungeonMapCellFunType.Spawn;
				value.CellType = cell.CellType;
				m_gridMap[(value.X, value.Y)] = value;
				num++;
			}
		}
		if (m_gridMap.Count > 0)
		{
			m_gridMapMaxY = m_gridMap.Values.Max((MapGridDetail g) => g.Y);
			m_gridMapMaxX = m_gridMap.Values.Where((MapGridDetail g) => g.State == EGridState.Explored).Max((MapGridDetail g) => g.X);
		}
	}

	public void RefreshCurGridPlayerPos()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		m_playerPos = GetGridPos(nCurPlayerGridX, nCurPlayerGridY);
		RequestRender();
	}

	public StyleScale GetBigMapScale()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		if (m_gridMapMaxX <= 2)
		{
			return new StyleScale(new Scale(new Vector2(1.2f, 1.2f)));
		}
		if (m_gridMapMaxX <= 4)
		{
			return new StyleScale(new Scale(new Vector2(1.1f, 1.1f)));
		}
		if (m_gridMapMaxX <= 6)
		{
			return new StyleScale(new Scale(new Vector2(1f, 1f)));
		}
		if (m_gridMapMaxX <= 8)
		{
			return new StyleScale(new Scale(new Vector2(0.9f, 0.9f)));
		}
		if (m_gridMapMaxX <= 10)
		{
			return new StyleScale(new Scale(new Vector2(0.82f, 0.82f)));
		}
		if (m_gridMapMaxX <= 12)
		{
			return new StyleScale(new Scale(new Vector2(0.72f, 0.72f)));
		}
		if (m_gridMapMaxX <= 14)
		{
			return new StyleScale(new Scale(new Vector2(0.6f, 0.6f)));
		}
		if (m_gridMapMaxX <= 16)
		{
			return new StyleScale(new Scale(new Vector2(0.5f, 0.5f)));
		}
		return new StyleScale(new Scale(new Vector2(0.4f, 0.4f)));
	}

	public (StyleLength width, StyleLength height) GetMiniMapSize()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		if (m_gridMap == null || m_gridMap.Count == 0)
		{
			return (width: new StyleLength((StyleKeyword)1), height: new StyleLength((StyleKeyword)1));
		}
		StyleScale bigMapScale = GetBigMapScale();
		Scale value = ((StyleScale)(ref bigMapScale)).value;
		Vector2.op_Implicit(((Scale)(ref value)).value);
		float num = (float)(m_gridMapMaxX + 1) * m_gridSpacingX + m_iconSize;
		float num2 = (float)(m_gridMapMaxY + 1) * m_gridSpacingY + m_iconSize;
		float num3 = num * 1f;
		float num4 = num2 * 1f;
		return (width: new StyleLength(num3), height: new StyleLength(num4));
	}

	public Vector2 GetGridPos(int GridX, int GridY)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2((float)GridX * m_gridSpacingX, (float)(m_gridMapMaxY - GridY) * m_gridSpacingY);
	}

	public void SetPlayerPos(Vector2 pos)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		m_playerPos = pos;
		RequestRender();
	}

	public void SetRenderMod(bool byPlayerPos)
	{
		m_renderMod = byPlayerPos;
	}

	private void OnGenerateVisualContent(MeshGenerationContext ctx)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		Rect contentRect = ((VisualElement)this).contentRect;
		float width = ((Rect)(ref contentRect)).width;
		contentRect = ((VisualElement)this).contentRect;
		float height = ((Rect)(ref contentRect)).height;
		if (!float.IsNaN(width) && !float.IsNaN(height) && !(width <= 1f) && !(height <= 1f) && m_gridMap != null && m_gridMap.Count != 0 && !((Object)(object)m_texNormal == (Object)null))
		{
			CollectPrimitives(width, height);
			WriteLinesToMesh(ctx);
			WriteCirclesToMesh(ctx);
			if (m_unknownIcons.Count > 0 && (Object)(object)m_texUnknown != (Object)null)
			{
				WriteTexturedRects(ctx, (Texture)(object)m_texUnknown, m_unknownIcons);
			}
			if (m_bossIcons.Count > 0 && (Object)(object)m_texBoss != (Object)null)
			{
				WriteTexturedRects(ctx, (Texture)(object)m_texBoss, m_bossIcons);
			}
			if (m_entryIcons.Count > 0 && (Object)(object)m_texEntry != (Object)null)
			{
				WriteTexturedRects(ctx, (Texture)(object)m_texEntry, m_entryIcons);
			}
			if (m_playerIcons.Count > 0 && (Object)(object)m_texPlayer != (Object)null)
			{
				WriteTexturedRects(ctx, (Texture)(object)m_texPlayer, m_playerIcons);
			}
		}
	}

	private void WriteLinesToMesh(MeshGenerationContext ctx)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
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
		int num = 26;
		int num2 = Mathf.Max(1, 65000 / num);
		for (int i = 0; i < m_circles.Count; i += num2)
		{
			int num3 = Mathf.Min(num2, m_circles.Count - i);
			MeshWriteData val = ctx.Allocate(num3 * num, num3 * 24 * 3, (Texture)null);
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
				for (int k = 0; k <= 24; k++)
				{
					float num5 = (float)Math.PI * 2f * (float)k / 24f;
					nextVertex.position = new Vector3(circleDraw.center.x + Mathf.Cos(num5) * circleDraw.radius, circleDraw.center.y + Mathf.Sin(num5) * circleDraw.radius, Vertex.nearZ);
					val.SetNextVertex(nextVertex);
				}
				for (int l = 0; l < 24; l++)
				{
					val.SetNextIndex(num4);
					val.SetNextIndex((ushort)(num4 + 1 + l));
					val.SetNextIndex((ushort)(num4 + 2 + l));
				}
				num4 += (ushort)num;
			}
		}
	}

	private void WriteTexturedRects(MeshGenerationContext ctx, Texture texture, List<Rect> rects)
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
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < rects.Count; i += 16000)
		{
			int num = Mathf.Min(16000, rects.Count - i);
			MeshWriteData val = ctx.Allocate(num * 4, num * 6, texture);
			Rect uvRegion = val.uvRegion;
			Vector2 min = ((Rect)(ref uvRegion)).min;
			Vector2 size = ((Rect)(ref uvRegion)).size;
			Vector2 uv = new Vector2(0f, 1f) * size + min;
			Vector2 uv2 = new Vector2(1f, 1f) * size + min;
			Vector2 uv3 = new Vector2(1f, 0f) * size + min;
			Vector2 uv4 = new Vector2(0f, 0f) * size + min;
			Vertex nextVertex = new Vertex
			{
				tint = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue)
			};
			ushort num2 = 0;
			for (int j = 0; j < num; j++)
			{
				Rect val2 = rects[i + j];
				float xMin = ((Rect)(ref val2)).xMin;
				float yMin = ((Rect)(ref val2)).yMin;
				float xMax = ((Rect)(ref val2)).xMax;
				float yMax = ((Rect)(ref val2)).yMax;
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

	private void CollectPrimitives(float viewportWidth, float viewportHeight)
	{
		m_lines.Clear();
		m_circles.Clear();
		m_unknownIcons.Clear();
		m_bossIcons.Clear();
		m_entryIcons.Clear();
		m_playerIcons.Clear();
		int gridMapMaxY = m_gridMapMaxY;
		float offsetX = (m_renderMod ? (viewportWidth / 2f - m_playerPos.x) : 0f);
		float offsetY = (m_renderMod ? (viewportHeight / 2f - m_playerPos.y) : 0f);
		CollectConnections(gridMapMaxY, offsetX, offsetY);
		CollectBackgroundCells(gridMapMaxY, offsetX, offsetY);
		CollectIcons(gridMapMaxY, offsetX, offsetY);
		CollectPlayerIcon(offsetX, offsetY);
	}

	private void AddLine(Vector2 start, Vector2 end, float lineWidth, Color color)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
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

	private void AddDashedLine(Vector2 start, Vector2 end, float lineWidth, Color color)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = end - start;
		float magnitude = ((Vector2)(ref val)).magnitude;
		if (!(magnitude < 0.001f))
		{
			Vector2 val2 = val / magnitude;
			float num = 4f;
			float num2 = 2f;
			float num3 = num + num2;
			int num4 = Mathf.FloorToInt(magnitude / num3);
			Vector2 val3 = (magnitude - (float)num4 * num3 + num2) / 2f * val2;
			Vector2 val4 = start + val3;
			for (int i = 0; i < num4; i++)
			{
				Vector2 val5 = val4;
				Vector2 end2 = val5 + val2 * num;
				AddLine(val5, end2, lineWidth, color);
				val4 += val2 * num3;
			}
		}
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

	private void CollectConnections(int maxY, float offsetX, float offsetY)
	{
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_032d: Unknown result type (might be due to invalid IL or missing references)
		//IL_032e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0337: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = default(Vector2);
		Vector2 end = default(Vector2);
		Vector2 end2 = default(Vector2);
		Vector2 start = default(Vector2);
		foreach (MapGridDetail value4 in m_gridMap.Values)
		{
			if (value4.State == EGridState.Invisible)
			{
				continue;
			}
			float num = maxY - value4.Y;
			((Vector2)(ref val))._002Ector((float)value4.X * m_gridSpacingX + m_gridSize / 2f + offsetX, num * m_gridSpacingY + m_gridSize / 2f + offsetY);
			EGridState state;
			if (m_gridMap.TryGetValue((value4.X + 1, value4.Y), out var value))
			{
				state = value4.State;
				if (state == EGridState.NotExplore || state == EGridState.Invisible)
				{
					state = value.State;
					if (state == EGridState.NotExplore || state == EGridState.Invisible)
					{
						goto IL_0176;
					}
				}
				if ((value4.HasRightDoor && value.HasLeftDoor) || (value4.State == EGridState.Explored && (value.State == EGridState.NotExplore || value.IsElevator) && CheckHasDoorByRecord(value4.CellType, hasLeft: false)))
				{
					float num2 = maxY - value.Y;
					((Vector2)(ref end))._002Ector((float)(value4.X + 1) * m_gridSpacingX + m_gridSize / 2f + offsetX, num2 * m_gridSpacingY + m_gridSize / 2f + offsetY);
					Color connectionColor = GetConnectionColor(value4.State, value.State);
					AddLine(val, end, m_lineWidth, connectionColor);
				}
			}
			goto IL_0176;
			IL_027d:
			if (!m_gridMap.TryGetValue((value4.X, value4.Y + 1), out var value2))
			{
				continue;
			}
			state = value4.State;
			if (state == EGridState.NotExplore || state == EGridState.Invisible)
			{
				state = value2.State;
				if (state == EGridState.NotExplore || state == EGridState.Invisible)
				{
					continue;
				}
			}
			if (value4.IsElevator && value2.IsElevator)
			{
				float num3 = maxY - value2.Y;
				((Vector2)(ref end2))._002Ector((float)value4.X * m_gridSpacingX + m_gridSize / 2f + offsetX, num3 * m_gridSpacingY + m_gridSize / 2f + offsetY);
				AddDashedLine(val, end2, m_lineWidth, sDashLineColor);
			}
			continue;
			IL_0176:
			if (m_gridMap.TryGetValue((value4.X - 1, value4.Y), out var value3))
			{
				state = value4.State;
				if (state == EGridState.NotExplore || state == EGridState.Invisible)
				{
					state = value3.State;
					if (state == EGridState.NotExplore || state == EGridState.Invisible)
					{
						goto IL_027d;
					}
				}
				if ((value4.HasLeftDoor && value3.HasRightDoor) || (value4.State == EGridState.Explored && (value3.State == EGridState.NotExplore || value3.IsElevator) && CheckHasDoorByRecord(value4.CellType, hasLeft: true)))
				{
					float num4 = maxY - value3.Y;
					((Vector2)(ref start))._002Ector((float)(value4.X - 1) * m_gridSpacingX + m_gridSize / 2f + offsetX, num4 * m_gridSpacingY + m_gridSize / 2f + offsetY);
					Color connectionColor2 = GetConnectionColor(value4.State, value3.State);
					AddLine(start, val, m_lineWidth, connectionColor2);
				}
			}
			goto IL_027d;
		}
	}

	private void CollectBackgroundCells(int maxY, float offsetX, float offsetY)
	{
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		foreach (MapGridDetail value3 in m_gridMap.Values)
		{
			if (value3.State != EGridState.Invisible && (!m_gridMap.TryGetValue((value3.X + 1, value3.Y), out var value) || (value3.State != EGridState.NotExplore && value3.State != EGridState.Invisible) || (value.State != EGridState.NotExplore && value.State != EGridState.Invisible)) && (!m_gridMap.TryGetValue((value3.X - 1, value3.Y), out var value2) || (value3.State != EGridState.NotExplore && value3.State != EGridState.Invisible) || (value2.State != EGridState.NotExplore && value2.State != EGridState.Invisible)))
			{
				float num = maxY - value3.Y;
				Vector2 val = new Vector2((float)value3.X * m_gridSpacingX + offsetX, num * m_gridSpacingY + offsetY);
				Color color = GetGridColorByState(value3.State);
				if (value3.State == EGridState.Explored && value3.HasPendingEvent)
				{
					color = Color.red;
				}
				Vector2 center = val + new Vector2(m_gridSize / 2f, m_gridSize / 2f);
				AddCircle(center, m_gridSize / 2f, color);
			}
		}
	}

	private void CollectIcons(int maxY, float offsetX, float offsetY)
	{
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		Vector2 gridPos = default(Vector2);
		foreach (MapGridDetail value5 in m_gridMap.Values)
		{
			if (value5.State == EGridState.Invisible)
			{
				continue;
			}
			bool flag = true;
			bool flag2 = false;
			bool flag3 = false;
			flag2 = !m_gridMap.TryGetValue((value5.X - 1, value5.Y), out var value) || value.State == EGridState.NotExplore || value.State == EGridState.Invisible || (value.State == EGridState.Explored && !CheckHasDoorByRecord(value.CellType, hasLeft: false));
			flag3 = !m_gridMap.TryGetValue((value5.X + 1, value5.Y), out var value2) || value2.State == EGridState.NotExplore || value2.State == EGridState.Invisible || (value2.State == EGridState.Explored && !CheckHasDoorByRecord(value2.CellType, hasLeft: true));
			if (flag2 & flag3)
			{
				flag = false;
			}
			if (value5.IsElevator && !flag)
			{
				bool flag4 = m_gridMap.TryGetValue((value5.X, value5.Y + 1), out var value3) && value3.IsElevator;
				bool flag5 = m_gridMap.TryGetValue((value5.X, value5.Y - 1), out var value4) && value4.IsElevator;
				flag = ((value5.State == EGridState.NotExplore && ((flag4 && value3.State == EGridState.Explored) || (flag5 && value4.State == EGridState.Explored))) ? true : false);
			}
			if (flag)
			{
				float num = maxY - value5.Y;
				((Vector2)(ref gridPos))._002Ector((float)value5.X * m_gridSpacingX + offsetX, num * m_gridSpacingY + offsetY);
				Vector2 centeredPos = GetCenteredPos(gridPos, m_iconSize, m_iconSize);
				if (value5.State == EGridState.NotExplore && (Object)(object)m_texUnknown != (Object)null && !value5.HasBoss)
				{
					AddIcon(m_unknownIcons, centeredPos);
				}
				else if (value5.HasBoss && (Object)(object)m_texBoss != (Object)null)
				{
					AddIcon(m_bossIcons, centeredPos);
				}
				else if (value5.IsEntry && (Object)(object)m_texEntry != (Object)null)
				{
					AddIcon(m_entryIcons, centeredPos);
				}
			}
		}
	}

	private void AddIcon(List<Rect> list, Vector2 iconPos)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		list.Add(new Rect(iconPos.x, iconPos.y, m_iconSize, m_iconSize));
	}

	private void CollectPlayerIcon(float offsetX, float offsetY)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)m_texPlayer == (Object)null))
		{
			Vector2 gridPos = default(Vector2);
			((Vector2)(ref gridPos))._002Ector(m_playerPos.x + offsetX, m_playerPos.y + offsetY);
			Vector2 centeredPos = GetCenteredPos(gridPos, m_iconSize, m_iconSize);
			AddIcon(m_playerIcons, centeredPos);
		}
	}

	private Color GetGridColorByState(EGridState state)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		return (Color)(state switch
		{
			EGridState.Explored => sGridColorExplored, 
			EGridState.NotExplore => sGridColorNotExplore, 
			EGridState.Invisible => sGridColorNotExplore, 
			_ => sGridColorNotExplore, 
		});
	}

	private Color GetConnectionColor(EGridState a, EGridState b)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (a == EGridState.Explored && b == EGridState.Explored)
		{
			return sConnLineColorExplored;
		}
		return sConnLineColorNotExplore;
	}

	private Vector2 GetCenteredPos(Vector2 gridPos, float iconWidth, float iconHeight)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		float num = (m_gridSize - iconWidth) / 2f;
		float num2 = (m_gridSize - iconHeight) / 2f;
		return gridPos + new Vector2(num, num2);
	}

	public void Update(int x, int y)
	{
		nCurPlayerGridX = x;
		nCurPlayerGridY = y;
		m_fsm.Update();
	}

	public bool CheckHasDoorByRecord(int type, bool hasLeft)
	{
		zbDungeonCellTypeCsvRecord zbDungeonCellTypeCsvRecord2 = dungeonCellTypeRecord[type];
		if (zbDungeonCellTypeCsvRecord2 != null)
		{
			if (hasLeft)
			{
				return zbDungeonCellTypeCsvRecord2.leftDoor > 0;
			}
			return zbDungeonCellTypeCsvRecord2.rightDoor > 0;
		}
		return false;
	}

	private void RenderFallback()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Expected O, but got Unknown
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Expected O, but got Unknown
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Expected O, but got Unknown
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
		Rect contentRect = ((VisualElement)this).contentRect;
		float width = ((Rect)(ref contentRect)).width;
		contentRect = ((VisualElement)this).contentRect;
		float height = ((Rect)(ref contentRect)).height;
		if (float.IsNaN(width) || float.IsNaN(height) || width <= 1f || height <= 1f || m_gridMap == null || m_gridMap.Count == 0 || (Object)(object)m_texNormal == (Object)null)
		{
			return;
		}
		if ((Object)(object)m_RT == (Object)null || ((Texture)m_RT).width != (int)width || ((Texture)m_RT).height != (int)height)
		{
			if ((Object)(object)m_RT != (Object)null)
			{
				m_RT.Release();
				Object.DestroyImmediate((Object)(object)m_RT);
			}
			m_RT = new RenderTexture((int)width, (int)height, 0, (RenderTextureFormat)0);
			((Texture)m_RT).filterMode = (FilterMode)1;
		}
		if ((Object)(object)m_Mat == (Object)null)
		{
			Shader val = Shader.Find("Sprites/Default");
			if ((Object)(object)val == (Object)null)
			{
				val = Shader.Find("UI/Default");
			}
			if ((Object)(object)val == (Object)null)
			{
				val = Shader.Find("Hidden/Internal-Colored");
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
		GL.Clear(false, true, new Color(0f, 0f, 0f, 0f));
		GL.PushMatrix();
		GL.LoadPixelMatrix(0f, width, height, 0f);
		CollectPrimitives(width, height);
		DrawAllLinesGL();
		DrawAllCirclesGL();
		DrawIconsGL(m_unknownIcons, m_texUnknown);
		DrawIconsGL(m_bossIcons, m_texBoss);
		DrawIconsGL(m_entryIcons, m_texEntry);
		DrawIconsGL(m_playerIcons, m_texPlayer);
		GL.PopMatrix();
		if ((Object)(object)m_Tex2D == (Object)null || ((Texture)m_Tex2D).width != ((Texture)m_RT).width || ((Texture)m_Tex2D).height != ((Texture)m_RT).height)
		{
			if ((Object)(object)m_Tex2D != (Object)null)
			{
				Object.DestroyImmediate((Object)(object)m_Tex2D);
			}
			m_Tex2D = new Texture2D(((Texture)m_RT).width, ((Texture)m_RT).height, (TextureFormat)5, false);
		}
		m_Tex2D.ReadPixels(new Rect(0f, 0f, (float)((Texture)m_RT).width, (float)((Texture)m_RT).height), 0, 0);
		m_Tex2D.Apply();
		RenderTexture.active = active;
		((VisualElement)this).style.backgroundImage = new StyleBackground(Background.FromTexture2D(m_Tex2D));
	}

	private void DrawAllLinesGL()
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		if (m_lines.Count == 0)
		{
			return;
		}
		m_Mat.mainTexture = (Texture)(object)m_texNormal;
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

	private void DrawAllCirclesGL()
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		if (m_circles.Count == 0)
		{
			return;
		}
		m_Mat.mainTexture = (Texture)(object)m_texNormal;
		m_Mat.SetPass(0);
		GL.Begin(4);
		foreach (CircleDraw circle in m_circles)
		{
			GL.Color(circle.color);
			for (int i = 0; i < 24; i++)
			{
				float num = (float)Math.PI * 2f * (float)i / 24f;
				float num2 = (float)Math.PI * 2f * (float)(i + 1) / 24f;
				GL.Vertex3(circle.center.x, circle.center.y, 0f);
				GL.Vertex3(circle.center.x + Mathf.Cos(num) * circle.radius, circle.center.y + Mathf.Sin(num) * circle.radius, 0f);
				GL.Vertex3(circle.center.x + Mathf.Cos(num2) * circle.radius, circle.center.y + Mathf.Sin(num2) * circle.radius, 0f);
			}
		}
		GL.End();
	}

	private void DrawIconsGL(List<Rect> rects, Texture2D tex)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		if (rects.Count == 0 || (Object)(object)tex == (Object)null)
		{
			return;
		}
		m_Mat.mainTexture = (Texture)(object)tex;
		m_Mat.SetPass(0);
		GL.Begin(4);
		GL.Color(Color.white);
		foreach (Rect rect in rects)
		{
			Rect current = rect;
			float xMin = ((Rect)(ref current)).xMin;
			float yMin = ((Rect)(ref current)).yMin;
			float xMax = ((Rect)(ref current)).xMax;
			float yMax = ((Rect)(ref current)).yMax;
			GL.TexCoord2(0f, 1f);
			GL.Vertex3(xMin, yMin, 0f);
			GL.TexCoord2(1f, 1f);
			GL.Vertex3(xMax, yMin, 0f);
			GL.TexCoord2(1f, 0f);
			GL.Vertex3(xMax, yMax, 0f);
			GL.TexCoord2(1f, 0f);
			GL.Vertex3(xMax, yMax, 0f);
			GL.TexCoord2(0f, 0f);
			GL.Vertex3(xMin, yMax, 0f);
			GL.TexCoord2(0f, 1f);
			GL.Vertex3(xMin, yMin, 0f);
		}
		GL.End();
	}

	private void DisposeResources()
	{
		if ((Object)(object)m_RT != (Object)null)
		{
			m_RT.Release();
			Object.DestroyImmediate((Object)(object)m_RT);
			m_RT = null;
		}
		if ((Object)(object)m_Mat != (Object)null)
		{
			Object.DestroyImmediate((Object)(object)m_Mat);
			m_Mat = null;
		}
		if ((Object)(object)m_Tex2D != (Object)null)
		{
			Object.DestroyImmediate((Object)(object)m_Tex2D);
			m_Tex2D = null;
		}
	}
}
