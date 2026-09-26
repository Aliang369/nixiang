using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class zbWorldGridLine : VisualElement
{
	private Vector2 m_from;

	private Vector2 m_to;

	private Color m_color;

	private float m_width;

	private float m_startProgress;

	private float m_endProgress;

	private readonly List<Vector2> m_cachedFullPath;

	private float m_len1;

	private float m_len2;

	private float m_len3;

	private float m_totalLen;

	private Vector2 m_end1;

	private Vector2 m_mid;

	private Vector2 m_start2;

	private bool m_isStraight;

	public zbWorldGridLine(Vector2 from, Vector2 to, Color color, float width)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		m_color = Color.red;
		m_width = 4f;
		m_endProgress = 1f;
		m_cachedFullPath = new List<Vector2>(64);
		((VisualElement)this)._002Ector();
		m_from = from;
		m_to = to;
		m_color = color;
		m_width = width;
		((VisualElement)this).pickingMode = (PickingMode)1;
		((VisualElement)this).style.position = StyleEnum<Position>.op_Implicit((Position)1);
		((VisualElement)this).style.left = StyleLength.op_Implicit(0f);
		((VisualElement)this).style.top = StyleLength.op_Implicit(0f);
		((VisualElement)this).generateVisualContent = (Action<MeshGenerationContext>)Delegate.Combine(((VisualElement)this).generateVisualContent, new Action<MeshGenerationContext>(OnGenerateVisualContent));
	}

	public void SetProgressRange(float startT, float endT)
	{
		float num = Mathf.Clamp01(startT);
		float num2 = Mathf.Clamp01(endT);
		if (!Mathf.Approximately(num, m_startProgress) || !Mathf.Approximately(num2, m_endProgress))
		{
			m_startProgress = num;
			m_endProgress = num2;
			((VisualElement)this).MarkDirtyRepaint();
		}
	}

	private void EnsurePathCache()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		if (!m_isStraight && (!(m_len1 > 0f) || !(m_totalLen > 0f)))
		{
			Vector2 val = m_from;
			Vector2 to = m_to;
			if (Mathf.Approximately(val.x, to.x) || Mathf.Approximately(val.y, to.y))
			{
				m_isStraight = true;
				return;
			}
			float num = ((val.x > to.x) ? 70f : (-70f));
			float num2 = ((val.y > to.y) ? (-70f) : 70f);
			m_mid = new Vector2(to.x, val.y);
			m_end1 = new Vector2(m_mid.x + num, m_mid.y);
			m_start2 = new Vector2(m_mid.x, m_mid.y + num2);
			m_len1 = Vector2.Distance(val, m_end1);
			m_len2 = BezierApproxLength(m_end1, m_mid, m_start2);
			m_len3 = Vector2.Distance(m_start2, to);
			m_totalLen = m_len1 + m_len2 + m_len3;
		}
	}

	private void OnGenerateVisualContent(MeshGenerationContext ctx)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		if (m_startProgress >= m_endProgress)
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
		EnsurePathCache();
		m_cachedFullPath.Clear();
		CollectVisiblePoints(m_cachedFullPath);
		int count = m_cachedFullPath.Count;
		if (count < 2)
		{
			return;
		}
		int num = 0;
		Vector2 val;
		for (int i = 0; i < count - 1; i++)
		{
			val = m_cachedFullPath[i + 1] - m_cachedFullPath[i];
			if (((Vector2)(ref val)).sqrMagnitude > 1.0000001E-06f)
			{
				num++;
			}
		}
		if (num == 0)
		{
			return;
		}
		MeshWriteData val2 = ctx.Allocate(num * 4, num * 6, (Texture)null);
		float num2 = m_width * 0.5f;
		Vertex nextVertex = new Vertex
		{
			tint = Color32.op_Implicit(m_color),
			uv = Vector2.zero
		};
		int num3 = 0;
		for (int j = 0; j < count - 1; j++)
		{
			Vector2 val3 = m_cachedFullPath[j];
			Vector2 val4 = m_cachedFullPath[j + 1];
			Vector2 val5 = val4 - val3;
			if (!(((Vector2)(ref val5)).sqrMagnitude <= 1.0000001E-06f))
			{
				val = new Vector2(0f - val5.y, val5.x);
				Vector2 val6 = ((Vector2)(ref val)).normalized * num2;
				nextVertex.position = new Vector3(val3.x + val6.x, val3.y + val6.y, 0f);
				val2.SetNextVertex(nextVertex);
				nextVertex.position = new Vector3(val3.x - val6.x, val3.y - val6.y, 0f);
				val2.SetNextVertex(nextVertex);
				nextVertex.position = new Vector3(val4.x + val6.x, val4.y + val6.y, 0f);
				val2.SetNextVertex(nextVertex);
				nextVertex.position = new Vector3(val4.x - val6.x, val4.y - val6.y, 0f);
				val2.SetNextVertex(nextVertex);
				ushort num4 = (ushort)num3;
				val2.SetNextIndex(num4);
				val2.SetNextIndex((ushort)(num4 + 1));
				val2.SetNextIndex((ushort)(num4 + 2));
				val2.SetNextIndex((ushort)(num4 + 1));
				val2.SetNextIndex((ushort)(num4 + 3));
				val2.SetNextIndex((ushort)(num4 + 2));
				num3 += 4;
			}
		}
	}

	private void CollectVisiblePoints(List<Vector2> points)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = m_from;
		Vector2 to = m_to;
		if (m_isStraight)
		{
			points.Add(Vector2.Lerp(val, to, m_startProgress));
			points.Add(Vector2.Lerp(val, to, m_endProgress));
			return;
		}
		float num = m_totalLen * m_startProgress;
		float num2 = m_totalLen * m_endProgress;
		points.Add(EvaluatePath(m_startProgress));
		if (num < m_len1)
		{
			float num3 = Mathf.Min(num2, m_len1);
			if (num3 > num)
			{
				points.Add(Vector2.Lerp(val, m_end1, num3 / m_len1));
			}
		}
		if (num2 > m_len1 && num < m_len1 + m_len2)
		{
			float num4 = Mathf.Max(0f, num - m_len1);
			float num5 = Mathf.Min(num2 - m_len1, m_len2);
			float num6 = num4 / m_len2;
			float num7 = num5 / m_len2;
			int num8 = Mathf.Max(2, (int)((num5 - num4) / 5f));
			for (int i = 0; i <= num8; i++)
			{
				float t = num6 + (num7 - num6) * ((float)i / (float)num8);
				points.Add(QuadraticBezier(m_end1, m_mid, m_start2, t));
			}
		}
		if (num2 > m_len1 + m_len2 && num < m_totalLen)
		{
			float num9 = num2 - m_len1 - m_len2;
			points.Add(Vector2.Lerp(m_start2, to, num9 / m_len3));
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

	private float BezierApproxLength(Vector2 p0, Vector2 p1, Vector2 p2, int steps = 10)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		Vector2 val = p0;
		for (int i = 1; i <= steps; i++)
		{
			float t = (float)i / (float)steps;
			Vector2 val2 = QuadraticBezier(p0, p1, p2, t);
			num += Vector2.Distance(val, val2);
			val = val2;
		}
		return num;
	}

	public Vector2 EvaluatePath(float t)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = m_from;
		Vector2 to = m_to;
		EnsurePathCache();
		if (m_isStraight)
		{
			return Vector2.Lerp(val, to, t);
		}
		float num = m_totalLen * t;
		if (num <= m_len1)
		{
			return Vector2.Lerp(val, m_end1, num / m_len1);
		}
		num -= m_len1;
		if (num <= m_len2)
		{
			return QuadraticBezier(m_end1, m_mid, m_start2, num / m_len2);
		}
		num -= m_len2;
		return Vector2.Lerp(m_start2, to, num / m_len3);
	}
}
