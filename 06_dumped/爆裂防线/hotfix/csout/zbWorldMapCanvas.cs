using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class zbWorldMapCanvas : VisualElement
{
	public class UxmlFactory : UxmlFactory<zbWorldMapCanvas>
	{
	}

	public enum MapCellState
	{
		Unlocked,
		ToExplore,
		Locked
	}

	private struct Quad
	{
		public Rect rect;

		public Color color;
	}

	private const int CellSize = 140;

	private MapCellState[,] m_map;

	private Texture2D m_texLocked;

	private Texture2D m_texToExplore;

	private float lineThickness;

	private Color lineColor;

	internal static readonly bool kUseMeshDirect = true;

	private const int MaxQuadsPerMesh = 16000;

	private RenderTexture m_RT;

	private Material m_Mat;

	private Texture2D m_Tex2D;

	private readonly List<Quad> m_lockedQuads;

	private readonly List<Quad> m_toExploreQuads;

	private readonly List<Quad> m_solidQuads;

	public zbWorldMapCanvas()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		lineThickness = 10f;
		lineColor = Color.yellow;
		m_lockedQuads = new List<Quad>();
		m_toExploreQuads = new List<Quad>();
		m_solidQuads = new List<Quad>();
		((VisualElement)this)._002Ector();
		m_map = new MapCellState[31, 31];
		for (int i = 0; i < 31; i++)
		{
			for (int j = 0; j < 31; j++)
			{
				if (j >= 10 && j <= 20 && i >= 10 && i <= 20)
				{
					m_map[j, i] = MapCellState.Unlocked;
				}
				else if ((j + i) % 2 == 0)
				{
					m_map[j, i] = MapCellState.ToExplore;
				}
				else
				{
					m_map[j, i] = MapCellState.Locked;
				}
			}
		}
		m_map[5, 5] = MapCellState.Unlocked;
		m_map[6, 5] = MapCellState.Unlocked;
		m_map[6, 6] = MapCellState.Unlocked;
		m_texLocked = zbAssetLoader.Instance.LoadResouce<Texture2D>("ui/image/world/cellLock.png");
		m_texToExplore = zbAssetLoader.Instance.LoadResouce<Texture2D>("ui/image/world/cellExplore.png");
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
			if (m_toExploreQuads.Count > 0 && (Object)(object)m_texToExplore != (Object)null)
			{
				WriteTexturedQuads(ctx, (Texture)(object)m_texToExplore, m_toExploreQuads);
			}
			if (m_solidQuads.Count > 0)
			{
				WriteSolidQuads(ctx, m_solidQuads);
			}
		}
	}

	private void CollectQuads()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		m_lockedQuads.Clear();
		m_toExploreQuads.Clear();
		m_solidQuads.Clear();
		Color white = Color.white;
		Rect rect = default(Rect);
		for (int i = 0; i < m_map.GetLength(1); i++)
		{
			for (int j = 0; j < m_map.GetLength(0); j++)
			{
				MapCellState mapCellState = m_map[j, i];
				((Rect)(ref rect))._002Ector((float)(j * 140), (float)(i * 140), 140f, 140f);
				switch (mapCellState)
				{
				case MapCellState.Locked:
					if ((Object)(object)m_texLocked != (Object)null)
					{
						m_lockedQuads.Add(new Quad
						{
							rect = rect,
							color = white
						});
					}
					break;
				case MapCellState.ToExplore:
					if ((Object)(object)m_texToExplore != (Object)null)
					{
						m_toExploreQuads.Add(new Quad
						{
							rect = rect,
							color = white
						});
					}
					break;
				}
				if (mapCellState == MapCellState.Unlocked)
				{
					CollectCellEdges(j, i, rect);
					CollectCornerPatches(j, i);
				}
			}
		}
	}

	private void AddSolid(Rect rect)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		m_solidQuads.Add(new Quad
		{
			rect = rect,
			color = lineColor
		});
	}

	private void CollectCellEdges(int x, int y, Rect rect)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		if (!IsUnlocked(x, y + 1))
		{
			AddSolid(new Rect(((Rect)(ref rect)).x, ((Rect)(ref rect)).y + ((Rect)(ref rect)).height - lineThickness, ((Rect)(ref rect)).width, lineThickness));
		}
		if (!IsUnlocked(x, y - 1))
		{
			AddSolid(new Rect(((Rect)(ref rect)).x, ((Rect)(ref rect)).y, ((Rect)(ref rect)).width, lineThickness));
		}
		if (!IsUnlocked(x - 1, y))
		{
			AddSolid(new Rect(((Rect)(ref rect)).x, ((Rect)(ref rect)).y, lineThickness, ((Rect)(ref rect)).height));
		}
		if (!IsUnlocked(x + 1, y))
		{
			AddSolid(new Rect(((Rect)(ref rect)).x + ((Rect)(ref rect)).width - lineThickness, ((Rect)(ref rect)).y, lineThickness, ((Rect)(ref rect)).height));
		}
	}

	private void CollectCornerPatches(int x, int y)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		float num = x * 140;
		float num2 = y * 140;
		if (IsUnlocked(x - 1, y) && IsUnlocked(x, y - 1) && !IsUnlocked(x - 1, y - 1))
		{
			AddSolid(new Rect(num, num2, lineThickness, lineThickness));
		}
		if (IsUnlocked(x + 1, y) && IsUnlocked(x, y - 1) && !IsUnlocked(x + 1, y - 1))
		{
			AddSolid(new Rect(num + 140f - lineThickness, num2, lineThickness, lineThickness));
		}
		if (IsUnlocked(x - 1, y) && IsUnlocked(x, y + 1) && !IsUnlocked(x - 1, y + 1))
		{
			AddSolid(new Rect(num, num2 + 140f - lineThickness, lineThickness, lineThickness));
		}
		if (IsUnlocked(x + 1, y) && IsUnlocked(x, y + 1) && !IsUnlocked(x + 1, y + 1))
		{
			AddSolid(new Rect(num + 140f - lineThickness, num2 + 140f - lineThickness, lineThickness, lineThickness));
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
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
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
			Vertex nextVertex = new Vertex
			{
				tint = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue)
			};
			ushort num2 = 0;
			for (int j = 0; j < num; j++)
			{
				Quad quad = quads[i + j];
				float xMin = ((Rect)(ref quad.rect)).xMin;
				float yMin = ((Rect)(ref quad.rect)).yMin;
				float xMax = ((Rect)(ref quad.rect)).xMax;
				float yMax = ((Rect)(ref quad.rect)).yMax;
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

	private void RenderFallback()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Expected O, but got Unknown
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Expected O, but got Unknown
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Expected O, but got Unknown
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		Rect layout = ((VisualElement)this).layout;
		float width = ((Rect)(ref layout)).width;
		layout = ((VisualElement)this).layout;
		float height = ((Rect)(ref layout)).height;
		if (float.IsNaN(width) || float.IsNaN(height) || width <= 1f || height <= 1f)
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
		GL.Clear(true, true, new Color(0f, 0f, 0f, 0f));
		GL.PushMatrix();
		GL.LoadPixelMatrix(0f, width, height, 0f);
		CollectQuads();
		DrawTexturedQuadsGL((Texture)(object)m_texLocked, m_lockedQuads);
		DrawTexturedQuadsGL((Texture)(object)m_texToExplore, m_toExploreQuads);
		DrawSolidQuadsGL();
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

	private void DrawSolidQuadsGL()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		if (m_solidQuads.Count == 0)
		{
			return;
		}
		m_Mat.mainTexture = null;
		m_Mat.SetPass(0);
		GL.Begin(4);
		foreach (Quad solidQuad in m_solidQuads)
		{
			Rect rect = solidQuad.rect;
			GL.Color(solidQuad.color);
			GL.Vertex3(((Rect)(ref rect)).x, ((Rect)(ref rect)).y, 0f);
			GL.Vertex3(((Rect)(ref rect)).xMax, ((Rect)(ref rect)).y, 0f);
			GL.Vertex3(((Rect)(ref rect)).xMax, ((Rect)(ref rect)).yMax, 0f);
			GL.Vertex3(((Rect)(ref rect)).x, ((Rect)(ref rect)).y, 0f);
			GL.Vertex3(((Rect)(ref rect)).xMax, ((Rect)(ref rect)).yMax, 0f);
			GL.Vertex3(((Rect)(ref rect)).x, ((Rect)(ref rect)).yMax, 0f);
		}
		GL.End();
	}

	private void DrawTexturedQuadsGL(Texture texture, List<Quad> quads)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		if (quads.Count == 0 || (Object)(object)texture == (Object)null)
		{
			return;
		}
		m_Mat.mainTexture = texture;
		m_Mat.SetPass(0);
		GL.Begin(4);
		GL.Color(Color.white);
		foreach (Quad quad in quads)
		{
			Rect rect = quad.rect;
			GL.TexCoord2(1f, 0f);
			GL.Vertex3(((Rect)(ref rect)).x, ((Rect)(ref rect)).y, 0f);
			GL.TexCoord2(0f, 0f);
			GL.Vertex3(((Rect)(ref rect)).xMax, ((Rect)(ref rect)).y, 0f);
			GL.TexCoord2(0f, 1f);
			GL.Vertex3(((Rect)(ref rect)).xMax, ((Rect)(ref rect)).yMax, 0f);
			GL.TexCoord2(1f, 0f);
			GL.Vertex3(((Rect)(ref rect)).x, ((Rect)(ref rect)).y, 0f);
			GL.TexCoord2(0f, 1f);
			GL.Vertex3(((Rect)(ref rect)).xMax, ((Rect)(ref rect)).yMax, 0f);
			GL.TexCoord2(1f, 1f);
			GL.Vertex3(((Rect)(ref rect)).x, ((Rect)(ref rect)).yMax, 0f);
		}
		GL.End();
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

	private bool IsUnlocked(int x, int y)
	{
		if (x >= 0 && y >= 0 && x < m_map.GetLength(0) && y < m_map.GetLength(1))
		{
			return m_map[x, y] == MapCellState.Unlocked;
		}
		return false;
	}

	public void Refresh()
	{
		RequestRender();
	}
}
