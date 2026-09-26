using System;
using UnityEngine;
using UnityEngine.UIElements;

public class zbPieProgressTex : VisualElement
{
	public class UxmlFactory : UxmlFactory<zbPieProgressTex, UxmlTraits>
	{
	}

	public class UxmlTraits : UxmlTraits
	{
		private readonly UxmlStringAttributeDescription m_sprite;

		private readonly UxmlColorAttributeDescription m_color;

		private readonly UxmlFloatAttributeDescription m_progress;

		private readonly UxmlIntAttributeDescription m_smoothSegments;

		public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			((UxmlTraits)this).Init(ve, bag, cc);
			zbPieProgressTex zbPieProgressTex2 = (zbPieProgressTex)(object)ve;
			zbPieProgressTex2._color = ((TypedUxmlAttributeDescription<Color>)(object)m_color).GetValueFromBag(bag, cc);
			zbPieProgressTex2._progress = ((TypedUxmlAttributeDescription<float>)(object)m_progress).GetValueFromBag(bag, cc);
			zbPieProgressTex2._smoothSegments = ((TypedUxmlAttributeDescription<int>)(object)m_smoothSegments).GetValueFromBag(bag, cc);
			string valueFromBag = ((TypedUxmlAttributeDescription<string>)(object)m_sprite).GetValueFromBag(bag, cc);
			if (!string.IsNullOrEmpty(valueFromBag))
			{
				zbPieProgressTex2._pendingSpriteUrl = valueFromBag;
				Sprite val = TryResolveSprite(valueFromBag);
				if ((Object)(object)val != (Object)null)
				{
					zbPieProgressTex2._sprite = val;
				}
			}
		}

		internal static Sprite TryResolveSprite(string url)
		{
			if (string.IsNullOrEmpty(url))
			{
				return null;
			}
			if (SpriteUrlResolver != null)
			{
				return SpriteUrlResolver(url);
			}
			return null;
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
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
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
			//IL_007f: Expected O, but got Unknown
			//IL_0084: Expected O, but got Unknown
			UxmlStringAttributeDescription val = new UxmlStringAttributeDescription
			{
				name = "sprite"
			};
			((TypedUxmlAttributeDescription<string>)val).defaultValue = "";
			m_sprite = val;
			UxmlColorAttributeDescription val2 = new UxmlColorAttributeDescription
			{
				name = "color"
			};
			((TypedUxmlAttributeDescription<Color>)val2).defaultValue = Color.white;
			m_color = val2;
			UxmlFloatAttributeDescription val3 = new UxmlFloatAttributeDescription
			{
				name = "progress"
			};
			((TypedUxmlAttributeDescription<float>)val3).defaultValue = 1f;
			m_progress = val3;
			UxmlIntAttributeDescription val4 = new UxmlIntAttributeDescription
			{
				name = "smooth-segments"
			};
			((TypedUxmlAttributeDescription<int>)val4).defaultValue = 128;
			m_smoothSegments = val4;
			((UxmlTraits)this)._002Ector();
		}
	}

	public static Func<string, Sprite> SpriteUrlResolver;

	private string _pendingSpriteUrl;

	private Sprite _sprite;

	private Color _color;

	private float _progress;

	private int _smoothSegments;

	internal static readonly bool kUseMeshDirect = true;

	private RenderTexture m_RT;

	private Material m_Mat;

	private Texture2D m_Tex2D;

	private float m_lastRenderedProgress;

	private Background m_cachedBackground;

	public string PendingSpriteUrl
	{
		get
		{
			return _pendingSpriteUrl;
		}
		set
		{
			_pendingSpriteUrl = value;
			if (!string.IsNullOrEmpty(value))
			{
				Sprite val = UxmlTraits.TryResolveSprite(value);
				if ((Object)(object)val != (Object)null)
				{
					_sprite = val;
				}
			}
		}
	}

	public Sprite sprite
	{
		get
		{
			return _sprite;
		}
		set
		{
			_sprite = value;
			RequestRender();
		}
	}

	public Color color
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _color;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_color = value;
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
			float num = Mathf.Clamp01(value);
			if (!Mathf.Approximately(num, _progress))
			{
				_progress = num;
				if (!(Mathf.Abs(_progress - m_lastRenderedProgress) < 0.005f) || !(_progress > 0f) || !(_progress < 1f))
				{
					RequestRender();
				}
			}
		}
	}

	public int smoothSegments
	{
		get
		{
			return _smoothSegments;
		}
		set
		{
			_smoothSegments = Mathf.Max(8, value);
			RequestRender();
		}
	}

	public zbPieProgressTex()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		_color = Color.white;
		_progress = 1f;
		_smoothSegments = 128;
		m_lastRenderedProgress = -1f;
		((VisualElement)this)._002Ector();
		((CallbackEventHandler)this).RegisterCallback<AttachToPanelEvent>((EventCallback<AttachToPanelEvent>)OnAttached, (TrickleDown)0);
		((CallbackEventHandler)this).RegisterCallback<DetachFromPanelEvent>((EventCallback<DetachFromPanelEvent>)OnDetached, (TrickleDown)0);
		((CallbackEventHandler)this).RegisterCallback<GeometryChangedEvent>((EventCallback<GeometryChangedEvent>)OnGeometryChanged, (TrickleDown)0);
		((VisualElement)this).pickingMode = (PickingMode)1;
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
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		Rect layout = ((VisualElement)this).layout;
		float width = ((Rect)(ref layout)).width;
		layout = ((VisualElement)this).layout;
		float height = ((Rect)(ref layout)).height;
		if (!float.IsNaN(width) && !float.IsNaN(height) && !(width <= 1f) && !(height <= 1f) && !(_progress <= 0f))
		{
			Texture2D val = (((Object)(object)_sprite != (Object)null) ? _sprite.texture : Texture2D.whiteTexture);
			Vector2 val2 = default(Vector2);
			((Vector2)(ref val2))._002Ector(width / 2f, height / 2f);
			float num = Mathf.Min(width, height) / 2f;
			float num2 = (float)Math.PI * 2f * _progress;
			int num3 = Mathf.Max(12, Mathf.RoundToInt((float)_smoothSegments * _progress));
			MeshWriteData val3 = ctx.Allocate(num3 + 2, num3 * 3, (Texture)(object)val);
			Rect uvRegion = val3.uvRegion;
			float xMin = ((Rect)(ref uvRegion)).xMin;
			float yMin = ((Rect)(ref uvRegion)).yMin;
			float width2 = ((Rect)(ref uvRegion)).width;
			float height2 = ((Rect)(ref uvRegion)).height;
			Vertex nextVertex = new Vertex
			{
				tint = Color32.op_Implicit(_color)
			};
			nextVertex.position = new Vector3(val2.x, val2.y, 0f);
			nextVertex.uv = new Vector2(xMin + 0.5f * width2, yMin + 0.5f * height2);
			val3.SetNextVertex(nextVertex);
			float num4 = num2 / (float)num3;
			float num5 = -(float)Math.PI / 2f;
			for (int i = 0; i <= num3; i++)
			{
				float num6 = num5 + (float)i * num4;
				float num7 = Mathf.Cos(num6);
				float num8 = Mathf.Sin(num6);
				nextVertex.position = new Vector3(val2.x + num7 * num, val2.y + num8 * num, 0f);
				nextVertex.uv = new Vector2(xMin + (0.5f + num7 * 0.5f) * width2, yMin + (0.5f - num8 * 0.5f) * height2);
				val3.SetNextVertex(nextVertex);
			}
			for (int j = 0; j < num3; j++)
			{
				ushort num9 = (ushort)(1 + j);
				val3.SetNextIndex((ushort)0);
				val3.SetNextIndex(num9);
				val3.SetNextIndex((ushort)(num9 + 1));
			}
			m_lastRenderedProgress = _progress;
		}
	}

	private void RenderFallback()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Expected O, but got Unknown
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Expected O, but got Unknown
		//IL_039a: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a4: Expected O, but got Unknown
		//IL_03a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0421: Unknown result type (might be due to invalid IL or missing references)
		//IL_0426: Unknown result type (might be due to invalid IL or missing references)
		//IL_0437: Unknown result type (might be due to invalid IL or missing references)
		//IL_043c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0410: Unknown result type (might be due to invalid IL or missing references)
		//IL_0415: Unknown result type (might be due to invalid IL or missing references)
		Rect layout = ((VisualElement)this).layout;
		float width = ((Rect)(ref layout)).width;
		layout = ((VisualElement)this).layout;
		float height = ((Rect)(ref layout)).height;
		if (float.IsNaN(width) || float.IsNaN(height) || width <= 1f || height <= 1f || _progress <= 0f)
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
		Texture2D mainTexture = (((Object)(object)_sprite != (Object)null) ? _sprite.texture : Texture2D.whiteTexture);
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
		m_Mat.mainTexture = (Texture)(object)mainTexture;
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = m_RT;
		GL.Clear(true, true, new Color(0f, 0f, 0f, 0f));
		GL.PushMatrix();
		GL.LoadPixelMatrix(0f, width, height, 0f);
		Vector2 val2 = default(Vector2);
		((Vector2)(ref val2))._002Ector(width / 2f, height / 2f);
		float num3 = Mathf.Min(width, height) / 2f;
		float num4 = (float)Math.PI * 2f * _progress;
		int num5 = Mathf.Max(12, Mathf.RoundToInt((float)_smoothSegments * _progress));
		float num6 = num4 / (float)num5;
		float num7 = -(float)Math.PI / 2f;
		m_Mat.SetPass(0);
		GL.Begin(4);
		for (int i = 0; i < num5; i++)
		{
			float num8 = num7 + (float)i * num6;
			float num9 = num7 + (float)(i + 1) * num6;
			float num10 = Mathf.Cos(num8);
			float num11 = Mathf.Sin(num8);
			float num12 = Mathf.Cos(num9);
			float num13 = Mathf.Sin(num9);
			GL.Color(_color);
			GL.TexCoord2(0.5f, 0.5f);
			GL.Vertex3(val2.x, val2.y, 0f);
			GL.TexCoord2(0.5f + num10 * 0.5f, 0.5f - num11 * 0.5f);
			GL.Vertex3(val2.x + num10 * num3, val2.y + num11 * num3, 0f);
			GL.TexCoord2(0.5f + num12 * 0.5f, 0.5f - num13 * 0.5f);
			GL.Vertex3(val2.x + num12 * num3, val2.y + num13 * num3, 0f);
		}
		GL.End();
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
