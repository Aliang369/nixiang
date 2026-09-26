using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class zbHomeEditRoomAnim
{
	public zbHomeEditWnd wndParent;

	public zbHomeEditRoom proxyRoom;

	public VisualElement roomRootElement;

	public VisualElement roomFgElement;

	public VisualElement roomFrameElement;

	private VisualElement m_spriteColor;

	private VisualElement m_spriteIcon;

	private Label m_lbLevel;

	private Label m_lbName;

	private Coroutine m_animCoroutine;

	public zbHomeEditRoomAnim(zbHomeEditWnd _wndParent)
	{
		wndParent = _wndParent;
	}

	public void OnEnable()
	{
		roomRootElement = UQueryExtensions.Q(wndParent.GetRootElement(), "room_anim", (string)null);
		roomFgElement = UQueryExtensions.Q(roomRootElement, "room_fg", (string)null);
		roomFrameElement = UQueryExtensions.Q(roomRootElement, "room_frame", (string)null);
		m_spriteColor = UQueryExtensions.Q<VisualElement>(roomRootElement, "color", (string)null);
		m_spriteIcon = UQueryExtensions.Q<VisualElement>(roomRootElement, "icon", (string)null);
		m_lbLevel = UQueryExtensions.Q<Label>(roomRootElement, "lb_level", (string)null);
		m_lbName = UQueryExtensions.Q<Label>(roomRootElement, "lb_name", (string)null);
		SetVisible(visible: false);
	}

	public void MoveToBagAnim(zbHomeEditRoom _proxyRoom)
	{
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		proxyRoom = _proxyRoom;
		if (proxyRoom.roomData != null)
		{
			((TextElement)m_lbLevel).text = "Lv." + proxyRoom.roomData.Level;
			zbHomeRoomTypeCsvRecord recordByKey = zbDataConfig.Instance.homeRoomTypeCsvTable.GetRecordByKey(proxyRoom.roomData.TypeId);
			if (recordByKey != null)
			{
				m_spriteColor.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((recordByKey.type != 3 || proxyRoom.roomData.Level <= 0) ? 1 : 0));
				((TextElement)m_lbName).text = recordByKey.name;
				Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>(recordByKey.iconEdit);
				if ((Object)(object)val != (Object)null)
				{
					roomFgElement.style.backgroundImage = StyleBackground.op_Implicit(val);
				}
				Texture2D val2 = zbAssetLoader.Instance.LoadResouce<Texture2D>(recordByKey.iconType);
				if ((Object)(object)val2 != (Object)null)
				{
					m_spriteIcon.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val2));
					m_spriteIcon.MarkDirtyRepaint();
				}
			}
			SetVisible(visible: true);
		}
		roomRootElement.transform.scale = wndParent.zoomContent.transform.scale;
		Vector2 val3 = VisualElementExtensions.LocalToWorld(proxyRoom.roomRootElement, Vector2.zero);
		Vector2 start = VisualElementExtensions.WorldToLocal(roomRootElement.parent, val3);
		Vector2 val4 = VisualElementExtensions.LocalToWorld((VisualElement)(object)wndParent.m_btnBag, Vector2.zero);
		Vector2 end = VisualElementExtensions.WorldToLocal(roomRootElement.parent, val4);
		PlayerMoveAnimation(start, end, wndParent.zoomContent.transform.scale, 0.4f);
	}

	private void PlayerMoveAnimation(Vector2 start, Vector2 end, Vector3 startScale, float duration)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (m_animCoroutine != null)
		{
			((MonoBehaviour)wndParent).StopCoroutine(m_animCoroutine);
		}
		m_animCoroutine = ((MonoBehaviour)wndParent).StartCoroutine(MoveAnimRoutine(start, end, startScale, duration));
	}

	private IEnumerator MoveAnimRoutine(Vector2 start, Vector2 end, Vector3 startScale, float duration)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		float elapsed = 0f;
		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			float num = Mathf.Clamp01(elapsed / duration);
			num = num * num * (3f - 2f * num);
			Vector2 val = Vector2.Lerp(start, end, num);
			roomRootElement.style.left = StyleLength.op_Implicit(val.x);
			roomRootElement.style.top = StyleLength.op_Implicit(val.y);
			float num2 = Mathf.Lerp(startScale.x, 0.5f, num);
			roomRootElement.transform.scale = new Vector3(num2, num2, 1f);
			float num3 = Mathf.Lerp(1f, 0f, num);
			roomRootElement.style.opacity = StyleFloat.op_Implicit(num3);
			yield return null;
		}
		roomRootElement.style.left = StyleLength.op_Implicit(end.x);
		roomRootElement.style.top = StyleLength.op_Implicit(end.y);
		roomRootElement.style.opacity = StyleFloat.op_Implicit(1f);
		SetVisible(visible: false);
		m_animCoroutine = null;
	}

	public void SetVisible(bool visible)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (visible)
		{
			roomRootElement.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		else
		{
			roomRootElement.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
	}
}
