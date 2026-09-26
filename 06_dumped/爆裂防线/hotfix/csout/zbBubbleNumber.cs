using UnityEngine;
using UnityEngine.UIElements;

public class zbBubbleNumber
{
	private zbCombatData m_combatData;

	private VisualElement m_rootParent;

	private VisualElement m_rootThis;

	private Label m_lbNum;

	private Vector3 targetPosition;

	internal int activeIndex = -1;

	private Vector3 moveDirection;

	private static readonly float moveSpeed = 4f;

	private float violentUpSizFactor = 1f;

	private float scaleUpTime = 0.1f;

	private float showTime = 0.75f;

	private float scaleDownTime = 0.125f;

	private float m_scaleUpTimeAccum;

	private float m_showTimeAccum;

	private float m_scaleDownTimeAccum;

	private Vector3 m_objPos;

	private float m_objScale;

	private float m_objOpacity;

	private static readonly float bubble_width_half = 100f;

	private static readonly float bubble_height_half = 20f;

	private VisualElement m_nodeCenter;

	public BubbleDisplayData bubbleDisplay = new BubbleDisplayData();

	public void Init(VisualElement rootParent)
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		m_rootParent = rootParent;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/combat/bubbleNumber.uxml");
		m_rootThis = (VisualElement)(object)val.Instantiate();
		m_rootParent.Add(m_rootThis);
		m_nodeCenter = UQueryExtensions.Q<VisualElement>(m_rootThis, "nodeCenter", (string)null);
		m_lbNum = UQueryExtensions.Q<Label>(m_rootThis, "lbNum", (string)null);
		((VisualElement)m_lbNum).style.width = StyleLength.op_Implicit(new Length(200f, (LengthUnit)0));
		((VisualElement)m_lbNum).style.whiteSpace = StyleEnum<WhiteSpace>.op_Implicit((WhiteSpace)1);
		((VisualElement)m_lbNum).style.unityTextAlign = new StyleEnum<TextAnchor>((TextAnchor)4);
		m_scaleUpTimeAccum = 0f;
		m_showTimeAccum = 0f;
		m_scaleDownTimeAccum = 0f;
		zbFontReplacer.Instance.ReplaceFontInUI(m_rootThis);
	}

	public void SetActive(bool active)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		m_rootThis.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!active));
		m_scaleUpTimeAccum = 0f;
		m_showTimeAccum = 0f;
		m_scaleDownTimeAccum = 0f;
	}

	public bool IsActive()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		return m_rootThis.style.display == StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
	}

	public Color GetTextColor(ECombatBubble bubbleType)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		switch (bubbleType)
		{
		case ECombatBubble.WallNormalDecHp:
		case ECombatBubble.Unhit:
		case ECombatBubble.Immune:
		case ECombatBubble.Block:
			return Color.yellow;
		case ECombatBubble.MonsterAddHp:
		case ECombatBubble.WallAddHp:
			return Color.green;
		case ECombatBubble.MonsterNormalViolentDecHp:
		case ECombatBubble.InstantKill:
			return Color.red;
		default:
			return Color.white;
		}
	}

	private float GetTextViolentSize(ECombatBubble bubbleType)
	{
		if (bubbleType == ECombatBubble.MonsterNormalViolentDecHp)
		{
			return 1.5f;
		}
		return 1f;
	}

	public void Show(zbCombatData combatData, Vector3 bubblePosition, ECombatBubble bubbleType, long num)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		m_combatData = combatData;
		m_objPos = bubblePosition;
		m_objScale = 1f;
		m_objOpacity = 1f;
		violentUpSizFactor = GetTextViolentSize(bubbleType);
		Color textColor = GetTextColor(bubbleType);
		if (((VisualElement)m_lbNum).resolvedStyle.color != textColor)
		{
			((VisualElement)m_lbNum).style.color = StyleColor.op_Implicit(textColor);
		}
		string text;
		int num2;
		switch (bubbleType)
		{
		case ECombatBubble.Unhit:
			text = zbUtil.Language("lang.10010");
			num2 = 24;
			showTime = 0.2f;
			break;
		case ECombatBubble.Immune:
			text = zbUtil.Language("lang.10231");
			num2 = 24;
			showTime = 0.2f;
			break;
		case ECombatBubble.Block:
			text = zbUtil.Language("lang.11463");
			num2 = 24;
			showTime = 0.2f;
			break;
		case ECombatBubble.InstantKill:
			text = zbUtil.Language("lang.11498");
			num2 = 28;
			showTime = 0.35f;
			break;
		default:
			num2 = 28;
			showTime = 0.2f;
			text = ((num >= 0) ? zbUtil.ConvertToTBMK(num) : ("-" + zbUtil.ConvertToTBMK(-num)));
			break;
		}
		bubbleDisplay.BubbleType = bubbleType;
		bubbleDisplay.CachedText = text;
		bubbleDisplay.Cachedcolor = textColor;
		if (((VisualElement)m_lbNum).style.fontSize != StyleLength.op_Implicit((float)num2))
		{
			((VisualElement)m_lbNum).style.fontSize = StyleLength.op_Implicit((float)num2);
		}
		if (((TextElement)m_lbNum).text != text)
		{
			((TextElement)m_lbNum).text = text;
		}
		moveDirection = Vector2.op_Implicit(BubbleDirectionHelper.GetRandomDirectionalVector());
		targetPosition = m_objPos + moveDirection * 0.4f;
		UpdateBubblePos();
		UpdateBubblScaleOpacity();
		SafeBringToFront(m_rootThis);
	}

	public void Update()
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if (m_scaleUpTimeAccum < scaleUpTime)
		{
			m_objPos = Vector2.op_Implicit(Vector2.MoveTowards(Vector2.op_Implicit(m_objPos), Vector2.op_Implicit(targetPosition), moveSpeed * Time.deltaTime));
			UpdateBubblePos();
			m_scaleUpTimeAccum += Time.deltaTime;
			return;
		}
		if (m_showTimeAccum < showTime)
		{
			m_showTimeAccum += Time.deltaTime;
			return;
		}
		float objScale = m_objScale;
		if (m_scaleDownTimeAccum < scaleDownTime)
		{
			float num = Mathf.Clamp01(m_scaleDownTimeAccum / scaleDownTime);
			m_objScale = Mathf.Lerp(objScale, 0.35f, num);
			m_objOpacity = Mathf.Lerp(1f, 0f, num);
			UpdateBubblScaleOpacity();
			m_scaleDownTimeAccum += Time.deltaTime;
		}
		else
		{
			m_combatData.combatBubble.ReturnBubble(this);
		}
	}

	private void UpdateBubblePos()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = WorldToToolKit(m_objPos);
		Vector2 rootBubbleWorldBoundPos = m_combatData.combatBubble.m_bubblePool.m_rootBubbleWorldBoundPos;
		Vector2 val2 = val - rootBubbleWorldBoundPos;
		m_nodeCenter.transform.position = new Vector3(val2.x - bubble_width_half, val2.y - bubble_height_half, 0f);
	}

	private void UpdateBubblScaleOpacity()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		float num = m_objScale * violentUpSizFactor;
		m_nodeCenter.transform.scale = new Vector3(num, num, 1f);
		m_rootThis.style.opacity = StyleFloat.op_Implicit(m_objOpacity);
	}

	public Vector2 WorldToToolKit(Vector3 worldPos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(worldPos.x * m_combatData.combatBubble.m_bubblePool.m_worldToPanelScale.x + m_combatData.combatBubble.m_bubblePool.m_worldToPanelOffset.x, worldPos.y * m_combatData.combatBubble.m_bubblePool.m_worldToPanelScale.y + m_combatData.combatBubble.m_bubblePool.m_worldToPanelOffset.y);
	}

	public static void SafeBringToFront(VisualElement element)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		VisualElement parent = element.parent;
		if (parent.childCount != 0)
		{
			Hierarchy hierarchy = parent.hierarchy;
			if (((Hierarchy)(ref hierarchy))[parent.childCount - 1] != element)
			{
				element.BringToFront();
			}
		}
	}

	public void ForceAccelerateFadeOut()
	{
		m_scaleUpTimeAccum = scaleUpTime;
		if (m_showTimeAccum < showTime)
		{
			m_showTimeAccum = Mathf.Max(m_showTimeAccum, showTime * 0.8f);
		}
	}
}
