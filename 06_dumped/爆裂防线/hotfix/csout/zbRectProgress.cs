using System;
using UnityEngine;
using UnityEngine.UIElements;

public class zbRectProgress : VisualElement
{
	public class UxmlFactory : UxmlFactory<zbRectProgress, UxmlTraits>
	{
	}

	public class UxmlTraits : UxmlTraits
	{
		private readonly UxmlFloatAttributeDescription m_width;

		private readonly UxmlFloatAttributeDescription m_height;

		private readonly UxmlFloatAttributeDescription m_progress;

		private readonly UxmlColorAttributeDescription m_fillColor;

		private readonly UxmlColorAttributeDescription m_borderColor;

		private readonly UxmlBoolAttributeDescription m_fromBottom;

		public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			((UxmlTraits)this).Init(ve, bag, cc);
			zbRectProgress obj = (zbRectProgress)(object)ve;
			obj._width = ((TypedUxmlAttributeDescription<float>)(object)m_width).GetValueFromBag(bag, cc);
			obj._height = ((TypedUxmlAttributeDescription<float>)(object)m_height).GetValueFromBag(bag, cc);
			obj._progress = ((TypedUxmlAttributeDescription<float>)(object)m_progress).GetValueFromBag(bag, cc);
			obj._fillColor = ((TypedUxmlAttributeDescription<Color>)(object)m_fillColor).GetValueFromBag(bag, cc);
			obj._borderColor = ((TypedUxmlAttributeDescription<Color>)(object)m_borderColor).GetValueFromBag(bag, cc);
			obj._fromBottom = ((TypedUxmlAttributeDescription<bool>)(object)m_fromBottom).GetValueFromBag(bag, cc);
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
			//IL_005e: Expected O, but got Unknown
			//IL_0063: Expected O, but got Unknown
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Expected O, but got Unknown
			//IL_0084: Expected O, but got Unknown
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Expected O, but got Unknown
			//IL_00a5: Expected O, but got Unknown
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Expected O, but got Unknown
			//IL_00c2: Expected O, but got Unknown
			UxmlFloatAttributeDescription val = new UxmlFloatAttributeDescription
			{
				name = "_width"
			};
			((TypedUxmlAttributeDescription<float>)val).defaultValue = 200f;
			m_width = val;
			UxmlFloatAttributeDescription val2 = new UxmlFloatAttributeDescription
			{
				name = "_height"
			};
			((TypedUxmlAttributeDescription<float>)val2).defaultValue = 30f;
			m_height = val2;
			UxmlFloatAttributeDescription val3 = new UxmlFloatAttributeDescription
			{
				name = "_progress"
			};
			((TypedUxmlAttributeDescription<float>)val3).defaultValue = 0f;
			m_progress = val3;
			UxmlColorAttributeDescription val4 = new UxmlColorAttributeDescription
			{
				name = "_fill-color"
			};
			((TypedUxmlAttributeDescription<Color>)val4).defaultValue = Color.green;
			m_fillColor = val4;
			UxmlColorAttributeDescription val5 = new UxmlColorAttributeDescription
			{
				name = "_border-color"
			};
			((TypedUxmlAttributeDescription<Color>)val5).defaultValue = Color.white;
			m_borderColor = val5;
			UxmlBoolAttributeDescription val6 = new UxmlBoolAttributeDescription
			{
				name = "_from-bottom"
			};
			((TypedUxmlAttributeDescription<bool>)val6).defaultValue = false;
			m_fromBottom = val6;
			((UxmlTraits)this)._002Ector();
		}
	}

	private float _width;

	private float _height;

	private float _progress;

	private Color _fillColor;

	private Color _borderColor;

	private bool _fromBottom;

	internal static readonly bool kUseMeshDirect = true;

	private RenderTexture m_RT;

	private Material m_Mat;

	private Texture2D m_Tex2D;

	private float m_lastRenderedProgress;

	private Background m_cachedBackground;

	public float width
	{
		get
		{
			return _width;
		}
		set
		{
			_width = value;
			RequestRender();
		}
	}

	public float height
	{
		get
		{
			return _height;
		}
		set
		{
			_height = value;
			RequestRender();
		}
	}

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
				if (!(Mathf.Abs(_progress - m_lastRenderedProgress) < 1f) || !(_progress > 0f) || !(_progress < 100f))
				{
					RequestRender();
				}
			}
		}
	}

	public Color fillColor
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _fillColor;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_fillColor = value;
			RequestRender();
		}
	}

	public Color borderColor
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _borderColor;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_borderColor = value;
			RequestRender();
		}
	}

	public bool fromBottom
	{
		get
		{
			return _fromBottom;
		}
		set
		{
			_fromBottom = value;
			RequestRender();
		}
	}

	public zbRectProgress()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		_width = 200f;
		_height = 30f;
		_fillColor = Color.green;
		_borderColor = Color.white;
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
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		Rect layout = ((VisualElement)this).layout;
		float num = ((Rect)(ref layout)).width;
		layout = ((VisualElement)this).layout;
		float num2 = ((Rect)(ref layout)).height;
		if (!float.IsNaN(num) && !float.IsNaN(num2) && !(num <= 1f) && !(num2 <= 1f))
		{
			bool flag = _progress > 0f && _progress < 100f;
			MeshWriteData val = ctx.Allocate(flag ? 20 : 16, flag ? 30 : 24, (Texture)null);
			Vertex val2 = new Vertex
			{
				uv = Vector2.zero
			};
			val2.tint = Color32.op_Implicit(_borderColor);
			AppendBorderLine(val, val2, new Vector3(0f, 0f, 0f), new Vector3(_width, 0f, 0f), 0);
			AppendBorderLine(val, val2, new Vector3(0f, _height, 0f), new Vector3(_width, _height, 0f), 4);
			AppendBorderLine(val, val2, new Vector3(0f, 0f, 0f), new Vector3(0f, _height, 0f), 8);
			AppendBorderLine(val, val2, new Vector3(_width, 0f, 0f), new Vector3(_width, _height, 0f), 12);
			if (flag)
			{
				float num3 = _height * (_progress / 100f);
				float num4 = (_fromBottom ? (_height - num3) : 0f);
				float num5 = (_fromBottom ? _height : num3);
				val2.tint = Color32.op_Implicit(_fillColor);
				val2.position = new Vector3(0f, num4, 0f);
				val.SetNextVertex(val2);
				val2.position = new Vector3(_width, num4, 0f);
				val.SetNextVertex(val2);
				val2.position = new Vector3(0f, num5, 0f);
				val.SetNextVertex(val2);
				val2.position = new Vector3(_width, num5, 0f);
				val.SetNextVertex(val2);
				ushort num6 = 16;
				val.SetNextIndex(num6);
				val.SetNextIndex((ushort)(num6 + 1));
				val.SetNextIndex((ushort)(num6 + 2));
				val.SetNextIndex((ushort)(num6 + 1));
				val.SetNextIndex((ushort)(num6 + 3));
				val.SetNextIndex((ushort)(num6 + 2));
			}
			m_lastRenderedProgress = _progress;
		}
	}

	private static void AppendBorderLine(MeshWriteData mesh, Vertex vertex, Vector3 p1, Vector3 p2, int baseIndex)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = p2 - p1;
		Vector3 val2 = new Vector3(0f - val.y, val.x, 0f);
		Vector3 val3 = ((Vector3)(ref val2)).normalized * 0.5f;
		ushort num = (ushort)baseIndex;
		vertex.position = p1 + val3;
		mesh.SetNextVertex(vertex);
		vertex.position = p1 - val3;
		mesh.SetNextVertex(vertex);
		vertex.position = p2 + val3;
		mesh.SetNextVertex(vertex);
		vertex.position = p2 - val3;
		mesh.SetNextVertex(vertex);
		mesh.SetNextIndex(num);
		mesh.SetNextIndex((ushort)(num + 1));
		mesh.SetNextIndex((ushort)(num + 2));
		mesh.SetNextIndex((ushort)(num + 1));
		mesh.SetNextIndex((ushort)(num + 3));
		mesh.SetNextIndex((ushort)(num + 2));
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
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Expected O, but got Unknown
		//IL_036a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0374: Expected O, but got Unknown
		//IL_0376: Unknown result type (might be due to invalid IL or missing references)
		//IL_037b: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0405: Unknown result type (might be due to invalid IL or missing references)
		//IL_040a: Unknown result type (might be due to invalid IL or missing references)
		//IL_03de: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
		Rect layout = ((VisualElement)this).layout;
		float num = ((Rect)(ref layout)).width;
		layout = ((VisualElement)this).layout;
		float num2 = ((Rect)(ref layout)).height;
		if (float.IsNaN(num) || float.IsNaN(num2) || num <= 1f || num2 <= 1f)
		{
			return;
		}
		int num3 = Mathf.Max(2, Mathf.CeilToInt(num));
		int num4 = Mathf.Max(2, Mathf.CeilToInt(num2));
		if ((Object)(object)m_RT == (Object)null || ((Texture)m_RT).width != num3 || ((Texture)m_RT).height != num4)
		{
			if ((Object)(object)m_RT != (Object)null)
			{
				m_RT.Release();
			}
			m_RT = new RenderTexture(num3, num4, 0, (RenderTextureFormat)0);
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
		GL.LoadPixelMatrix(0f, num, num2, 0f);
		m_Mat.SetPass(0);
		GL.Begin(1);
		GL.Color(_borderColor);
		GL.Vertex3(0f, 0f, 0f);
		GL.Vertex3(_width, 0f, 0f);
		GL.Vertex3(0f, _height, 0f);
		GL.Vertex3(_width, _height, 0f);
		GL.Vertex3(0f, 0f, 0f);
		GL.Vertex3(0f, _height, 0f);
		GL.Vertex3(_width, 0f, 0f);
		GL.Vertex3(_width, _height, 0f);
		GL.End();
		if (_progress > 0f && _progress < 100f)
		{
			float num5 = _height * (_progress / 100f);
			float num6 = (_fromBottom ? (_height - num5) : 0f);
			float num7 = (_fromBottom ? _height : num5);
			GL.Begin(7);
			GL.Color(_fillColor);
			GL.Vertex3(0f, num6, 0f);
			GL.Vertex3(_width, num6, 0f);
			GL.Vertex3(_width, num7, 0f);
			GL.Vertex3(0f, num7, 0f);
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
	}
}
