using System;
using UnityEngine;
using UnityEngine.UIElements;

public class zbTipElement : VisualElement
{
	public class UxmlFactory : UxmlFactory<zbTipElement>
	{
	}

	private VisualElement m_tipBg;

	private Label m_lbtip;

	private IVisualElementScheduledItem m_hideSchedule;

	private bool isFlipped;

	[SerializeField]
	private float autoHideDelay;

	public bool IsVisible { get; private set; }

	public float AutoHideDelay
	{
		get
		{
			return autoHideDelay;
		}
		set
		{
			autoHideDelay = Mathf.Max(0.5f, value);
		}
	}

	public Texture2D BackgroundTexture
	{
		get
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			Background backgroundImage = m_tipBg.resolvedStyle.backgroundImage;
			return ((Background)(ref backgroundImage)).texture;
		}
		set
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)value != (Object)null)
			{
				m_tipBg.style.backgroundImage = new StyleBackground(value);
			}
		}
	}

	public Color TipTextColor
	{
		set
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			((VisualElement)m_lbtip).style.color = new StyleColor(value);
		}
	}

	public zbTipElement()
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		autoHideDelay = 3f;
		((VisualElement)this)._002Ector();
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/tip/tb_tip.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_tipBg = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg", (string)null);
		m_lbtip = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_tipDesc", (string)null);
		((VisualElement)this).style.position = StyleEnum<Position>.op_Implicit((Position)1);
		((VisualElement)this).style.overflow = StyleEnum<Overflow>.op_Implicit((Overflow)0);
		zbFontReplacer.Instance?.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void ShowAt(Vector2 localPos, string content, VisualElement container = null)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		if (m_lbtip != null)
		{
			((TextElement)m_lbtip).text = content;
			((VisualElement)this).style.left = StyleLength.op_Implicit(localPos.x);
			((VisualElement)this).style.top = StyleLength.op_Implicit(localPos.y);
			((VisualElement)this).style.opacity = StyleFloat.op_Implicit(0f);
			((VisualElement)this).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			IsVisible = true;
			((VisualElement)this).schedule.Execute((Action)delegate
			{
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				AdjustPosition(localPos, container);
			}).StartingIn(16L);
			if (m_hideSchedule != null)
			{
				m_hideSchedule.Pause();
				m_hideSchedule = null;
			}
			m_hideSchedule = ((VisualElement)this).schedule.Execute((Action)delegate
			{
				Hide();
			}).StartingIn((long)(autoHideDelay * 1000f));
		}
	}

	public void Show(string content)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		ShowAt(new Vector2(0f, 0f), content);
	}

	public void Hide()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)this).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		IsVisible = false;
		if (m_hideSchedule != null)
		{
			m_hideSchedule.Pause();
			m_hideSchedule = null;
		}
	}

	private void AdjustPosition(Vector2 screenPos, VisualElement container)
	{
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		if (container != null)
		{
			Rect layout = ((VisualElement)this).layout;
			float width = ((Rect)(ref layout)).width;
			layout = ((VisualElement)this).layout;
			float height = ((Rect)(ref layout)).height;
			layout = container.layout;
			float width2 = ((Rect)(ref layout)).width;
			layout = container.layout;
			float height2 = ((Rect)(ref layout)).height;
			float num = screenPos.x;
			float num2 = screenPos.y;
			if (screenPos.x + width > width2)
			{
				num = screenPos.x - width;
				if (!isFlipped)
				{
					SetFlip(flip: true);
				}
			}
			else if (isFlipped)
			{
				SetFlip(flip: false);
			}
			if (screenPos.y + height > height2)
			{
				num2 = screenPos.y - height;
			}
			num = Mathf.Clamp(num, 0f, width2 - width);
			num2 = Mathf.Clamp(num2, 0f, height2 - height);
			((VisualElement)this).style.left = StyleLength.op_Implicit(num);
			((VisualElement)this).style.top = StyleLength.op_Implicit(num2);
			((VisualElement)this).style.translate = new StyleTranslate(new Translate(new Length(0f, (LengthUnit)0), new Length(0f, (LengthUnit)0)));
		}
		else
		{
			((VisualElement)this).style.left = StyleLength.op_Implicit(screenPos.x);
			((VisualElement)this).style.top = StyleLength.op_Implicit(screenPos.y);
			((VisualElement)this).style.translate = new StyleTranslate(new Translate(new Length(0f, (LengthUnit)0), new Length(0f, (LengthUnit)0)));
		}
		((VisualElement)this).style.opacity = StyleFloat.op_Implicit(1f);
	}

	private void SetFlip(bool flip)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		float num = ((!flip) ? 1 : (-1));
		m_tipBg.style.scale = StyleScale.op_Implicit(new Scale(new Vector3(num, 1f, 1f)));
		((VisualElement)m_lbtip).style.scale = StyleScale.op_Implicit(new Scale(new Vector3(num, 1f, 1f)));
		isFlipped = flip;
	}
}
