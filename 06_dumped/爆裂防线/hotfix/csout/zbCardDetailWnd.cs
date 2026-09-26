using System;
using UnityEngine;
using UnityEngine.UIElements;

public class zbCardDetailWnd : zbBaseWnd
{
	private zbCardSpineOption m_cardSpineOption;

	private Image m_descImg;

	private VisualElement m_card;

	private VisualElement m_cardInfo;

	private Button m_btnClose;

	private VisualElement m_touch;

	private Vector2 m_pointerStartPos;

	private bool m_isPointerDown;

	private bool m_isSwiping;

	private const int m_textureW = 600;

	private const int m_textureH = 900;

	private const float SWIPE_THRESHOLD = 10f;

	protected override void OnEnable()
	{
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Expected O, but got Unknown
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_cardSpineOption = Object.FindFirstObjectByType<zbCardSpineOption>();
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_cardInfo = UQueryExtensions.Q<VisualElement>(m_RootElement, "cardinfo", (string)null);
		m_touch = UQueryExtensions.Q<VisualElement>(m_RootElement, "touch", (string)null);
		m_uiEvent.RegisterCallback<PointerDownEvent>(m_touch, (Action<PointerDownEvent>)OnPointerDown, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<PointerMoveEvent>(m_touch, (Action<PointerMoveEvent>)OnPointerMove, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<PointerUpEvent>(m_touch, (Action<PointerUpEvent>)OnPointerUp, "", (TrickleDown)0);
		m_card = UQueryExtensions.Q<VisualElement>(m_RootElement, "card", (string)null);
		if (m_descImg == null)
		{
			m_descImg = new Image();
			((VisualElement)m_descImg).style.width = StyleLength.op_Implicit(600f);
			((VisualElement)m_descImg).style.height = StyleLength.op_Implicit(900f);
			((VisualElement)m_descImg).style.position = StyleEnum<Position>.op_Implicit((Position)1);
			((VisualElement)m_descImg).style.left = new StyleLength(new Length(50f, (LengthUnit)1));
			((VisualElement)m_descImg).style.top = new StyleLength(new Length(50f, (LengthUnit)1));
			((VisualElement)m_descImg).style.translate = StyleTranslate.op_Implicit(new Translate(new Length(-50f, (LengthUnit)1), new Length(-50f, (LengthUnit)1)));
			m_card.Add((VisualElement)(object)m_descImg);
		}
	}

	public void RefreshCustom()
	{
		if (Object.op_Implicit((Object)(object)m_cardSpineOption))
		{
			m_cardSpineOption.SetRenderSpine(600, 900);
		}
	}

	private void OnPointerDown(PointerDownEvent evt)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		m_pointerStartPos = Vector2.op_Implicit(((PointerEventBase<PointerDownEvent>)(object)evt).position);
		m_isPointerDown = true;
		m_isSwiping = false;
		((EventBase)evt).StopPropagation();
	}

	private void OnPointerMove(PointerMoveEvent evt)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		if (m_isPointerDown)
		{
			Vector2 delta = Vector2.op_Implicit(((PointerEventBase<PointerMoveEvent>)(object)evt).position) - m_pointerStartPos;
			if (!m_isSwiping && ((Vector2)(ref delta)).sqrMagnitude > 100f)
			{
				m_isSwiping = true;
			}
			if (m_isSwiping)
			{
				m_cardInfo.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				m_card.style.scale = new StyleScale(Vector2.op_Implicit(new Vector3(1.2f, 1.2f, 1f)));
				m_cardSpineOption?.OnSwipe(delta);
			}
			((EventBase)evt).StopPropagation();
		}
	}

	private void OnPointerUp(PointerUpEvent evt)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		m_card.style.scale = new StyleScale(Vector2.op_Implicit(new Vector3(1f, 1f, 1f)));
		m_cardInfo.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		if (!m_isSwiping)
		{
			OnTouch();
		}
		m_cardSpineOption?.OnSwipeEnd();
		m_isPointerDown = false;
		m_isSwiping = false;
		((EventBase)evt).StopPropagation();
	}

	private void OnTouch()
	{
	}

	private void OnClose()
	{
		m_cardSpineOption.StopRender();
		Hide();
	}

	protected override void UpdateAtShow()
	{
		base.UpdateAtShow();
		if (m_descImg != null && !((Object)(object)m_cardSpineOption == (Object)null) && (Object)(object)m_cardSpineOption.GetRenderTexture() != (Object)null)
		{
			m_descImg.image = (Texture)(object)m_cardSpineOption.GetRenderTexture();
		}
	}
}
