using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class zbWorldGridLineElement : VisualElement
{
	private class LineAnimationInfo
	{
		public zbWorldGridLine Line;

		public VisualElement Icon;

		public VisualElement EndCapDot;

		public int ReconId;

		public bool IsDrawing;

		public IVisualElementScheduledItem Handle;
	}

	private readonly List<LineAnimationInfo> m_animatedLines;

	private zbWorldItemMgr m_itemMgr;

	private readonly int m_cols;

	private readonly int m_rows;

	private float m_cellWidth;

	private float m_cellHeight;

	private float m_speed;

	private const float m_iconWidth = 50f;

	private const float m_iconHeight = 50f;

	public zbWorldGridLineElement(int cols, int rows, float cellWidth, float cellHeight, zbWorldItemMgr itemMgr)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		m_animatedLines = new List<LineAnimationInfo>();
		m_speed = 100f;
		((VisualElement)this)._002Ector();
		m_itemMgr = itemMgr;
		m_cols = cols;
		m_rows = rows;
		m_cellWidth = cellWidth;
		m_cellHeight = cellHeight;
		((VisualElement)this).style.width = StyleLength.op_Implicit((float)m_cols * m_cellWidth);
		((VisualElement)this).style.height = StyleLength.op_Implicit((float)m_rows * m_cellHeight);
		((VisualElement)this).style.position = StyleEnum<Position>.op_Implicit((Position)1);
		((VisualElement)this).style.left = StyleLength.op_Implicit(0f);
		((VisualElement)this).style.top = StyleLength.op_Implicit(0f);
		((VisualElement)this).pickingMode = (PickingMode)1;
	}

	public void ClearFinishedAnimations()
	{
		for (int num = m_animatedLines.Count - 1; num >= 0; num--)
		{
			LineAnimationInfo lineAnimationInfo = m_animatedLines[num];
			if (!lineAnimationInfo.IsDrawing)
			{
				if (lineAnimationInfo.Line != null && ((VisualElement)lineAnimationInfo.Line).parent != null)
				{
					((VisualElement)this).Remove((VisualElement)(object)lineAnimationInfo.Line);
				}
				if (lineAnimationInfo.Icon != null && lineAnimationInfo.Icon.parent != null)
				{
					((VisualElement)this).Remove(lineAnimationInfo.Icon);
				}
				if (lineAnimationInfo.EndCapDot != null && lineAnimationInfo.EndCapDot.parent != null)
				{
					((VisualElement)this).Remove(lineAnimationInfo.EndCapDot);
				}
				if (lineAnimationInfo.Handle != null)
				{
					lineAnimationInfo.Handle.Pause();
				}
				m_animatedLines.RemoveAt(num);
			}
		}
	}

	public void AddLine(int fromX, int fromY, int toX, int toY, Color color, float lineWidth)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = GridToLocal(fromX, fromY);
		Vector2 to = GridToLocal(toX, toY);
		zbWorldGridLine zbWorldGridLine2 = new zbWorldGridLine(val, to, color, lineWidth);
		((VisualElement)zbWorldGridLine2).style.width = ((VisualElement)this).style.width;
		((VisualElement)zbWorldGridLine2).style.height = ((VisualElement)this).style.height;
		((VisualElement)this).Add((VisualElement)(object)zbWorldGridLine2);
	}

	public void AnimateAddLine(int fromX, int fromY, int toX, int toY, Color color, float lineWidth, int reconId)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Expected O, but got Unknown
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		//IL_0280: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a2: Expected O, but got Unknown
		//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f0: Unknown result type (might be due to invalid IL or missing references)
		Vector2 from = GridToLocal(fromX, fromY);
		Vector2 val = GridToLocal(toX, toY);
		float num = Vector2.Distance(from, val);
		if (num <= 0f)
		{
			return;
		}
		m_speed = 100f;
		if (num >= 1400f)
		{
			m_speed = 300f;
		}
		else if (num >= 700f)
		{
			m_speed = 200f;
		}
		float duration = num / m_speed;
		zbWorldGridLine line = new zbWorldGridLine(from, val, color, lineWidth);
		((VisualElement)line).style.width = ((VisualElement)this).style.width;
		((VisualElement)line).style.height = ((VisualElement)this).style.height;
		line.SetProgressRange(0f, 1f);
		((VisualElement)this).Add((VisualElement)(object)line);
		float num2 = 25f;
		float num3 = 25f;
		VisualElement val2 = new VisualElement
		{
			pickingMode = (PickingMode)1
		};
		val2.style.position = StyleEnum<Position>.op_Implicit((Position)1);
		val2.style.left = StyleLength.op_Implicit(from.x - num2);
		val2.style.top = StyleLength.op_Implicit(from.y - num3);
		val2.style.width = StyleLength.op_Implicit(50f);
		val2.style.height = StyleLength.op_Implicit(50f);
		val2.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Texture2D>($"ui/image/world/world_team{reconId - 1}.png"));
		val2.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		VisualElement icon = val2;
		((VisualElement)this).Add(icon);
		VisualElement val3 = new VisualElement
		{
			pickingMode = (PickingMode)1
		};
		val3.style.position = StyleEnum<Position>.op_Implicit((Position)1);
		val3.style.width = StyleLength.op_Implicit(lineWidth + 4f);
		val3.style.height = StyleLength.op_Implicit(lineWidth + 4f);
		val3.style.backgroundColor = StyleColor.op_Implicit(color);
		val3.style.borderBottomLeftRadius = new StyleLength(Length.Percent(50f));
		val3.style.borderBottomRightRadius = new StyleLength(Length.Percent(50f));
		val3.style.borderTopLeftRadius = new StyleLength(Length.Percent(50f));
		val3.style.borderTopRightRadius = new StyleLength(Length.Percent(50f));
		val3.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		VisualElement val4 = val3;
		Vector2 val5 = line.EvaluatePath(1f);
		float num4 = (lineWidth + 4f) * 0.5f;
		val4.style.left = StyleLength.op_Implicit(val5.x - num4);
		val4.style.top = StyleLength.op_Implicit(val5.y - num4);
		((VisualElement)this).Add(val4);
		LineAnimationInfo animInfo = new LineAnimationInfo
		{
			Line = line,
			Icon = icon,
			EndCapDot = val4,
			ReconId = reconId,
			IsDrawing = true
		};
		m_animatedLines.Add(animInfo);
		float startTime = Time.unscaledTime;
		animInfo.Handle = ((VisualElement)this).schedule.Execute((Action)delegate
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			float num5 = Mathf.Clamp01((Time.unscaledTime - startTime) / duration);
			Vector2 val6 = line.EvaluatePath(num5);
			icon.style.translate = StyleTranslate.op_Implicit(new Translate(Length.op_Implicit(val6.x - from.x), Length.op_Implicit(val6.y - from.y), 0f));
			line.SetProgressRange(num5, 1f);
			if (num5 >= 1f)
			{
				animInfo.IsDrawing = false;
				ClearFinishedAnimations();
				m_itemMgr.RefreshLines();
				m_itemMgr.RefreshTeamList();
			}
		}).Every(0L);
	}

	public bool IsReconDrawing(int reconId)
	{
		foreach (LineAnimationInfo animatedLine in m_animatedLines)
		{
			if (animatedLine.ReconId == reconId && animatedLine.IsDrawing)
			{
				return true;
			}
		}
		return false;
	}

	private Vector2 GridToLocal(int x, int y)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		int num = x + m_cols / 2;
		int num2 = y + m_rows / 2;
		float num3 = (float)num * m_cellWidth + m_cellWidth / 2f;
		float num4 = (float)(m_rows - 1 - num2) * m_cellHeight + m_cellHeight / 2f;
		return new Vector2(num3, num4);
	}
}
