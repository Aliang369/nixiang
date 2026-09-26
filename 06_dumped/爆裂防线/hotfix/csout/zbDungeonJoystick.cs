using UnityEngine;
using UnityEngine.UIElements;

public class zbDungeonJoystick
{
	private zbDungeonWnd m_wndParent;

	private VisualElement m_joystickContainer;

	private VisualElement m_joystickbg;

	private VisualElement m_joystickThumb;

	private bool m_bDrag;

	private Vector2 m_touchDown;

	private Vector2 m_joystickInput;

	private Vector3 m_joystickInitPos;

	public zbDungeonJoystick()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		m_joystickInput = Vector2.zero;
		m_joystickInitPos = Vector3.zero;
		base._002Ector();
	}

	public void Init(zbDungeonWnd _wndParent)
	{
		m_wndParent = _wndParent;
	}

	public Vector2 GetJoystickInput()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return m_joystickInput;
	}

	public void OnEnable()
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		VisualElement rootElement = m_wndParent.GetRootElement();
		m_joystickContainer = UQueryExtensions.Q<VisualElement>(rootElement, "joystick", (string)null);
		m_joystickbg = UQueryExtensions.Q<VisualElement>(m_joystickContainer, "joystick_bg", (string)null);
		m_joystickThumb = UQueryExtensions.Q<VisualElement>(m_joystickContainer, "joystick_thumb", (string)null);
		m_joystickInitPos = m_joystickContainer.transform.position;
	}

	public void OnPointerDown(PointerDownEvent evt)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		m_bDrag = true;
		m_touchDown = Vector2.op_Implicit(((PointerEventBase<PointerDownEvent>)(object)evt).localPosition);
		float width = m_joystickContainer.resolvedStyle.width;
		m_joystickContainer.transform.position = new Vector3(((PointerEventBase<PointerDownEvent>)(object)evt).localPosition.x - width / 2f, ((PointerEventBase<PointerDownEvent>)(object)evt).localPosition.y - width / 2f, ((PointerEventBase<PointerDownEvent>)(object)evt).localPosition.z);
	}

	public void OnPointerMove(PointerMoveEvent evt)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		UpdateThumbPosition(Vector2.op_Implicit(((PointerEventBase<PointerMoveEvent>)(object)evt).localPosition));
	}

	public void OnPointerUp(PointerUpEvent evt)
	{
		m_bDrag = false;
		ResetJoystick();
	}

	public void SetVisible(bool vis)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		m_joystickContainer.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!vis));
	}

	private void UpdateThumbPosition(Vector2 posTouch)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		if (m_bDrag)
		{
			Vector2 val = posTouch - m_touchDown;
			float num = m_joystickbg.resolvedStyle.width / 2f;
			Vector2 val3;
			Vector2 val2 = (val3 = Vector2.ClampMagnitude(val, num));
			m_joystickThumb.transform.position = new Vector3(val3.x, val3.y, m_joystickThumb.transform.position.z);
			Vector2 input = val2 / num;
			ApplyJoyStickInput(input);
		}
	}

	public void ResetJoystick()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		m_bDrag = false;
		m_joystickContainer.transform.position = m_joystickInitPos;
		m_joystickThumb.transform.position = Vector2.op_Implicit(Vector2.zero);
		m_joystickInput = Vector2.zero;
	}

	private void ApplyJoyStickInput(Vector2 input)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		if (Mathf.Abs(input.x) < float.Epsilon && Mathf.Abs(input.y) < float.Epsilon)
		{
			m_joystickInput = Vector2.op_Implicit(Vector3.zero);
		}
		else if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
		{
			if (input.x > 0f)
			{
				m_joystickInput = Vector2.op_Implicit(Vector3.right);
			}
			else
			{
				m_joystickInput = Vector2.op_Implicit(Vector3.left);
			}
		}
		else if (input.y > 0f)
		{
			m_joystickInput = Vector2.op_Implicit(Vector3.down);
		}
		else
		{
			m_joystickInput = Vector2.op_Implicit(Vector3.up);
		}
	}
}
