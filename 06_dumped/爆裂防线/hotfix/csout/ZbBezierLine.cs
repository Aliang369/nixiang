using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ZbBezierLine : VisualElement
{
	public Color lineColor;

	public float lineWidth;

	internal static readonly bool kUseMeshDirect = true;

	private const int MaxQuadsPerMesh = 16000;

	private const int SampleCount = 64;

	private RenderTexture m_RT;

	private Material m_Mat;

	private Texture2D m_Tex2D;

	private readonly List<Vector2> m_points;

	public List<Vector2> controlPoints { get; set; }

	public ZbBezierLine()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		controlPoints = new List<Vector2>();
		lineColor = Color.red;
		lineWidth = 2f;
		m_points = new List<Vector2>();
		((VisualElement)this)._002Ector();
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

	public void Refresh()
	{
		RequestRender();
	}

	private void OnGenerateVisualContent(MeshGenerationContext ctx)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		Rect layout = ((VisualElement)this).layout;
		float width = ((Rect)(ref layout)).width;
		layout = ((VisualElement)this).layout;
		float height = ((Rect)(ref layout)).height;
		if (float.IsNaN(width) || float.IsNaN(height) || width <= 1f || height <= 1f || !SampleCurve())
		{
			return;
		}
		int num = m_points.Count - 1;
		if (num <= 0)
		{
			return;
		}
		for (int i = 0; i < num; i += 16000)
		{
			int num2 = Mathf.Min(16000, num - i);
			MeshWriteData val = ctx.Allocate(num2 * 4, num2 * 6, (Texture)null);
			Vertex nextVertex = new Vertex
			{
				uv = Vector2.zero,
				tint = Color32.op_Implicit(lineColor)
			};
			ushort num3 = 0;
			for (int j = 0; j < num2; j++)
			{
				Vector2 p = m_points[i + j];
				Vector2 p2 = m_points[i + j + 1];
				GetSegmentQuad(p, p2, out var a, out var b, out var c, out var d);
				nextVertex.position = new Vector3(a.x, a.y, Vertex.nearZ);
				val.SetNextVertex(nextVertex);
				nextVertex.position = new Vector3(b.x, b.y, Vertex.nearZ);
				val.SetNextVertex(nextVertex);
				nextVertex.position = new Vector3(c.x, c.y, Vertex.nearZ);
				val.SetNextVertex(nextVertex);
				nextVertex.position = new Vector3(d.x, d.y, Vertex.nearZ);
				val.SetNextVertex(nextVertex);
				val.SetNextIndex(num3);
				val.SetNextIndex((ushort)(num3 + 1));
				val.SetNextIndex((ushort)(num3 + 2));
				val.SetNextIndex((ushort)(num3 + 1));
				val.SetNextIndex((ushort)(num3 + 3));
				val.SetNextIndex((ushort)(num3 + 2));
				num3 += 4;
			}
		}
	}

	private bool SampleCurve()
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		m_points.Clear();
		if (controlPoints == null || controlPoints.Count < 2)
		{
			return false;
		}
		if (controlPoints.Count == 2)
		{
			for (int i = 0; i <= 64; i++)
			{
				float num = (float)i / 64f;
				m_points.Add(Vector2.Lerp(controlPoints[0], controlPoints[1], num));
			}
		}
		else if (controlPoints.Count == 3)
		{
			for (int j = 0; j <= 64; j++)
			{
				float t = (float)j / 64f;
				m_points.Add(QuadraticBezier(controlPoints[0], controlPoints[1], controlPoints[2], t));
			}
		}
		else if (controlPoints.Count == 4)
		{
			for (int k = 0; k <= 64; k++)
			{
				float t2 = (float)k / 64f;
				m_points.Add(CubicBezier(controlPoints[0], controlPoints[1], controlPoints[2], controlPoints[3], t2));
			}
		}
		else
		{
			for (int l = 0; l + 3 < controlPoints.Count; l += 3)
			{
				for (int m = 0; m <= 64; m++)
				{
					float t3 = (float)m / 64f;
					m_points.Add(CubicBezier(controlPoints[l], controlPoints[l + 1], controlPoints[l + 2], controlPoints[l + 3], t3));
				}
			}
		}
		return m_points.Count >= 2;
	}

	private void GetSegmentQuad(Vector2 p1, Vector2 p2, out Vector2 a, out Vector2 b, out Vector2 c, out Vector2 d)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
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
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = p2 - p1;
		float magnitude = ((Vector2)(ref val)).magnitude;
		Vector2 val2 = ((!(magnitude < 0.001f)) ? (new Vector2(0f - val.y, val.x) / magnitude * (lineWidth * 0.5f)) : (Vector2.right * (lineWidth * 0.5f)));
		a = p1 + val2;
		b = p1 - val2;
		c = p2 + val2;
		d = p2 - val2;
	}

	private void RenderFallback()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Expected O, but got Unknown
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Expected O, but got Unknown
		//IL_0303: Unknown result type (might be due to invalid IL or missing references)
		//IL_030d: Expected O, but got Unknown
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0335: Unknown result type (might be due to invalid IL or missing references)
		//IL_035f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0364: Unknown result type (might be due to invalid IL or missing references)
		if (controlPoints == null || controlPoints.Count < 2)
		{
			return;
		}
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
		if (SampleCurve())
		{
			m_Mat.SetPass(0);
			GL.Begin(4);
			GL.Color(lineColor);
			for (int i = 0; i < m_points.Count - 1; i++)
			{
				GetSegmentQuad(m_points[i], m_points[i + 1], out var a, out var b, out var c, out var d);
				GL.Vertex3(a.x, a.y, 0f);
				GL.Vertex3(b.x, b.y, 0f);
				GL.Vertex3(c.x, c.y, 0f);
				GL.Vertex3(b.x, b.y, 0f);
				GL.Vertex3(d.x, d.y, 0f);
				GL.Vertex3(c.x, c.y, 0f);
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

	private Vector2 QuadraticBezier(Vector2 p0, Vector2 p1, Vector2 p2, float t)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		float num = 1f - t;
		return num * num * p0 + 2f * num * t * p1 + t * t * p2;
	}

	private Vector2 CubicBezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		float num = 1f - t;
		return num * num * num * p0 + 3f * num * num * t * p1 + 3f * num * t * t * p2 + t * t * t * p3;
	}
}
