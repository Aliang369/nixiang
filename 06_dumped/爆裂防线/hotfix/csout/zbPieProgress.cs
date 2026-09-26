using System;
using UnityEngine;
using UnityEngine.UIElements;

public class zbPieProgress : VisualElement
{
	public class UxmlFactory : UxmlFactory<zbPieProgress, UxmlTraits>
	{
	}

	public class UxmlTraits : UxmlTraits
	{
		private readonly UxmlFloatAttributeDescription m_radius;

		private readonly UxmlFloatAttributeDescription m_progress;

		private readonly UxmlColorAttributeDescription m_fanColor;

		public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			((UxmlTraits)this).Init(ve, bag, cc);
			zbPieProgress obj = (zbPieProgress)(object)ve;
			obj._radius = ((TypedUxmlAttributeDescription<float>)(object)m_radius).GetValueFromBag(bag, cc);
			obj._progress = ((TypedUxmlAttributeDescription<float>)(object)m_progress).GetValueFromBag(bag, cc);
			obj._fanColor = ((TypedUxmlAttributeDescription<Color>)(object)m_fanColor).GetValueFromBag(bag, cc);
		}

		public UxmlTraits()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Expected O, but got Unknown
			//IL_0021: Expected O, but got Unknown
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Expected O, but got Unknown
			//IL_0042: Expected O, but got Unknown
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Expected O, but got Unknown
			//IL_0063: Expected O, but got Unknown
			UxmlFloatAttributeDescription val = new UxmlFloatAttributeDescription
			{
				name = "_radius"
			};
			((TypedUxmlAttributeDescription<float>)val).defaultValue = 100f;
			m_radius = val;
			UxmlFloatAttributeDescription val2 = new UxmlFloatAttributeDescription
			{
				name = "_progress"
			};
			((TypedUxmlAttributeDescription<float>)val2).defaultValue = 50f;
			m_progress = val2;
			UxmlColorAttributeDescription val3 = new UxmlColorAttributeDescription
			{
				name = "_fan-color"
			};
			((TypedUxmlAttributeDescription<Color>)val3).defaultValue = Color.green;
			m_fanColor = val3;
			((UxmlTraits)this)._002Ector();
		}
	}

	private float _radius;

	private float _progress;

	private Color _fanColor;

	internal static readonly bool kUseMeshDirect = true;

	private float m_lastRenderedProgress;

	private Background m_cachedBackground;

	private VisualElement m_Chart;

	private RenderTexture m_RT;

	private Material m_Mat;

	private Texture2D m_Tex2D;

	private const float kRenderThreshold = 2f;

	private const string kCounterRendered = "pie环渲染次数";

	public float radius
	{
		get
		{
			return _radius;
		}
		set
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			_radius = value;
			if (m_Chart != null)
			{
				m_Chart.style.height = StyleLength.op_Implicit(diameter);
				m_Chart.style.width = StyleLength.op_Implicit(diameter);
			}
			RequestRender();
		}
	}

	public float diameter => _radius * 2f;

	public float progress
	{
		get
		{
			return _progress;
		}
		set
		{
			float num = Mathf.Clamp(value, 0f, 100f);
			if (!Mathf.Approximately(num, _progress))
			{
				_progress = num;
				if (!(Mathf.Abs(_progress - m_lastRenderedProgress) < 2f) || !(_progress > 0f) || !(_progress < 100f))
				{
					RequestRender();
				}
			}
		}
	}

	public Color fanColor
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _fanColor;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_fanColor = value;
			RequestRender();
		}
	}

	public zbPieProgress()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		_radius = 100f;
		_progress = 50f;
		_fanColor = Color.green;
		m_lastRenderedProgress = -1f;
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

	private void DisposeResources()
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
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
		m_cachedBackground = Background.op_Implicit((Texture2D)null);
	}

	private void OnGenerateVisualContent(MeshGenerationContext ctx)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		Rect layout = ((VisualElement)this).layout;
		float width = ((Rect)(ref layout)).width;
		layout = ((VisualElement)this).layout;
		float height = ((Rect)(ref layout)).height;
		if (!float.IsNaN(width) && !float.IsNaN(height) && !(width <= 1f) && !(height <= 1f) && !(_progress >= 100f))
		{
			Vector2 val = default(Vector2);
			((Vector2)(ref val))._002Ector(width / 2f, height / 2f);
			float num = 360f * (_progress / 100f);
			float num2 = (-90f + num) * ((float)Math.PI / 180f);
			float num3 = (4.712389f - num2) / 64f;
			MeshWriteData val2 = ctx.Allocate(66, 192, (Texture)null);
			Vertex nextVertex = new Vertex
			{
				tint = Color32.op_Implicit(_fanColor),
				uv = Vector2.zero,
				position = new Vector3(val.x, val.y, 0f)
			};
			val2.SetNextVertex(nextVertex);
			for (int i = 0; i <= 64; i++)
			{
				float num4 = num2 + (float)i * num3;
				nextVertex.position = new Vector3(val.x + Mathf.Cos(num4) * _radius, val.y + Mathf.Sin(num4) * _radius, 0f);
				val2.SetNextVertex(nextVertex);
			}
			for (int j = 0; j < 64; j++)
			{
				ushort num5 = (ushort)(1 + j);
				val2.SetNextIndex((ushort)0);
				val2.SetNextIndex(num5);
				val2.SetNextIndex((ushort)(num5 + 1));
			}
			m_lastRenderedProgress = _progress;
			zbPerfLog.Inc("pie环渲染次数", 1L);
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
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Expected O, but got Unknown
		//IL_030b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0315: Expected O, but got Unknown
		//IL_0327: Unknown result type (might be due to invalid IL or missing references)
		//IL_032c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_0359: Unknown result type (might be due to invalid IL or missing references)
		//IL_0378: Unknown result type (might be due to invalid IL or missing references)
		//IL_037e: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0391: Unknown result type (might be due to invalid IL or missing references)
		//IL_0396: Unknown result type (might be due to invalid IL or missing references)
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
		if (_progress < 100f)
		{
			Vector2 val2 = default(Vector2);
			((Vector2)(ref val2))._002Ector(width / 2f, height / 2f);
			float num3 = _radius;
			float num4 = 360f * (_progress / 100f);
			float num5 = (-90f + num4) * ((float)Math.PI / 180f);
			m_Mat.SetPass(0);
			GL.Begin(4);
			int num6 = 64;
			float num7 = (4.712389f - num5) / (float)num6;
			for (int i = 0; i < num6; i++)
			{
				float num8 = num5 + (float)i * num7;
				float num9 = num5 + (float)(i + 1) * num7;
				GL.Color(_fanColor);
				GL.Vertex3(val2.x, val2.y, 0f);
				GL.Vertex3(val2.x + Mathf.Cos(num8) * num3, val2.y + Mathf.Sin(num8) * num3, 0f);
				GL.Vertex3(val2.x + Mathf.Cos(num9) * num3, val2.y + Mathf.Sin(num9) * num3, 0f);
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
			m_Tex2D = new Texture2D(((Texture)m_RT).width, ((Texture)m_RT).height, (TextureFormat)4, false);
			((Object)m_Tex2D).name = "zbPieProgressTex";
			m_cachedBackground = Background.op_Implicit((Texture2D)null);
		}
		m_Tex2D.ReadPixels(new Rect(0f, 0f, (float)((Texture)m_RT).width, (float)((Texture)m_RT).height), 0, 0);
		m_Tex2D.Apply();
		RenderTexture.active = active;
		if (m_cachedBackground == Background.op_Implicit((Texture2D)null))
		{
			m_cachedBackground = Background.FromTexture2D(m_Tex2D);
		}
		((VisualElement)this).style.backgroundImage = new StyleBackground(Background.op_Implicit((Texture2D)null));
		((VisualElement)this).style.backgroundImage = new StyleBackground(m_cachedBackground);
		m_lastRenderedProgress = _progress;
		zbPerfLog.Inc("pie环渲染次数", 1L);
	}
}
