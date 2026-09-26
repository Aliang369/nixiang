using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbMonsterHudProgress
{
	private zbCombatData m_combatData;

	private zbMonster m_monster;

	private VisualElement m_rootParent;

	private VisualElement m_rootThis;

	private VisualElement m_progress1;

	private VisualElement m_progress2;

	private Label m_lbLevelName;

	private VisualElement m_progressShieldBg;

	private VisualElement m_progressShieldPb;

	private VisualElement m_progressBreakBg;

	private VisualElement m_progressBreakPb;

	private Texture2D iconTexture1;

	private Texture2D iconTexture2;

	private VisualElement m_nodeCenter;

	private static readonly float maxWidth = 160f;

	private const float kHudHeight = 16f;

	private int m_lastHpIndex = -1;

	private float m_lastPosMonsterX = float.NaN;

	private float m_lastPosMonsterY = float.NaN;

	private float m_lastPosBodyScale = float.NaN;

	private static StyleBackground s_progressBg1;

	private static StyleBackground s_progressBg2;

	private static bool s_progressBg1Inited;

	private static bool s_progressBg2Inited;

	private const string kSkipPosCounterKey = "hud跳过位置更新次数";

	public void Init(VisualElement rootParent)
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		m_rootParent = rootParent;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/combat/monsterHud.uxml");
		m_rootThis = (VisualElement)(object)val.Instantiate();
		m_rootParent.Add(m_rootThis);
		m_nodeCenter = UQueryExtensions.Q<VisualElement>(m_rootThis, "nodeCenter", (string)null);
		m_nodeCenter.style.left = StyleLength.op_Implicit(0f);
		m_nodeCenter.style.top = StyleLength.op_Implicit(0f);
		m_progress1 = UQueryExtensions.Q<VisualElement>(m_rootThis, "progress1", (string)null);
		m_progress2 = UQueryExtensions.Q<VisualElement>(m_rootThis, "progress2", (string)null);
		m_lbLevelName = UQueryExtensions.Q<Label>(m_rootThis, "lbg_title", (string)null);
		((TextElement)m_lbLevelName).text = "";
		m_progressShieldBg = UQueryExtensions.Q<VisualElement>(m_rootThis, "progressShield_bg", (string)null);
		m_progressShieldPb = UQueryExtensions.Q<VisualElement>(m_rootThis, "progressShield", (string)null);
		m_progressBreakBg = UQueryExtensions.Q<VisualElement>(m_rootThis, "progressBreak_bg", (string)null);
		m_progressBreakPb = UQueryExtensions.Q<VisualElement>(m_rootThis, "progressBreak", (string)null);
		iconTexture1 = zbAssetLoader.Instance.LoadResouce<Texture2D>("ui/image/progress/progress160x16_1.jpg");
		iconTexture2 = zbAssetLoader.Instance.LoadResouce<Texture2D>("ui/image/progress/progress160x16_2.jpg");
		if (!s_progressBg1Inited && (Object)(object)iconTexture1 != (Object)null)
		{
			s_progressBg1 = new StyleBackground(Background.FromTexture2D(iconTexture1));
			s_progressBg1Inited = true;
		}
		if (!s_progressBg2Inited && (Object)(object)iconTexture2 != (Object)null)
		{
			s_progressBg2 = new StyleBackground(Background.FromTexture2D(iconTexture2));
			s_progressBg2Inited = true;
		}
		zbFontReplacer.Instance.ReplaceFontInUI(m_rootThis);
		SetActive(active: false);
	}

	public void SetActive(bool active)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		m_nodeCenter.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!active));
		m_rootThis.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!active));
	}

	public bool IsActive()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		return m_rootThis.style.display == StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
	}

	public void Show(zbCombatData combatData, zbMonster owner)
	{
		m_combatData = combatData;
		m_monster = owner;
		SetActive(active: true);
		m_lastHpIndex = -1;
		m_lastPosMonsterX = float.NaN;
		m_lastPosMonsterY = float.NaN;
		m_lastPosBodyScale = float.NaN;
		UpdateHudPos();
	}

	public void Update()
	{
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		if (!IsActive() || zbCombatSystem.Instance.IsPaused())
		{
			return;
		}
		if (m_monster.isCanSelected())
		{
			zbUtil.SetDisplayIfChanged(m_nodeCenter, (DisplayStyle)0);
		}
		else
		{
			zbUtil.SetDisplayIfChanged(m_nodeCenter, (DisplayStyle)1);
		}
		float num = (float)(long)m_monster.maxHp / 10f;
		int num2 = Mathf.CeilToInt((float)(long)m_monster.curHp / num);
		if (num2 != m_lastHpIndex)
		{
			m_lastHpIndex = num2;
			StyleBackground backgroundImage = ((num2 % 2 == 0) ? s_progressBg1 : s_progressBg2);
			StyleBackground backgroundImage2 = ((num2 % 2 == 0) ? s_progressBg2 : s_progressBg1);
			if (m_progress1 != null)
			{
				m_progress1.style.backgroundImage = backgroundImage;
			}
			if (m_progress2 != null)
			{
				m_progress2.style.backgroundImage = backgroundImage2;
			}
			zbUtil.SetWidthIfChanged(m_progress1, (num2 != 1) ? maxWidth : 0f);
			if (zbCombatSystem.Instance.GetBattleType() != EBattleFuncType.WorldBoss || !m_monster.IsBoss())
			{
				((TextElement)m_lbLevelName).text = "X" + num2;
			}
		}
		float num3 = ((float)(long)m_monster.curHp - num * (float)(num2 - 1)) / num;
		zbUtil.SetWidthIfChanged(m_progress2, num3 * maxWidth);
		UpdateHudPos();
		UpdateShield();
		zbUtil.SetDisplayIfChanged(m_progressBreakBg, (DisplayStyle)(!m_monster.beBreakBoss));
		if (m_progressBreakBg.style.display == StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0))
		{
			zbUtil.SetWidthIfChanged(m_progressBreakPb, (float)m_monster.breakCount / (float)m_monster.breakTotal * 160f);
		}
	}

	private void UpdateHudPos()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)m_monster).transform.position;
		float x = position.x;
		float y = position.y;
		float attrValueByType = m_monster.GetAttrValueByType(EAttrMonsterType.BodyScale);
		if (x == m_lastPosMonsterX && y == m_lastPosMonsterY && attrValueByType == m_lastPosBodyScale)
		{
			zbPerfLog.Inc("hud跳过位置更新次数", 1L);
			return;
		}
		m_lastPosMonsterX = x;
		m_lastPosMonsterY = y;
		m_lastPosBodyScale = attrValueByType;
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(x, y + (float)m_monster.monsterAvatarRecord.boxHeight * attrValueByType * 1f / 100f, 0f);
		if (zbCombatSystem.Instance.isPVPBattleType)
		{
			if (m_combatData.dataIndex == 1)
			{
				((Vector3)(ref val)).Set((val.x - m_combatData.nodeRootPositonX) * 0.8f, (val.y - m_combatData.nodeRootPositonY + 8.4f) * 0.8f - 8.4f, val.z);
			}
			else
			{
				((Vector3)(ref val)).Set(0f - (val.x - m_combatData.nodeRootPositonX) * 0.8f, 0f - ((val.y - m_combatData.nodeRootPositonY + 8.4f) * 0.8f - 8.4f) + 1f, val.z);
			}
		}
		else
		{
			((Vector3)(ref val)).Set(val.x - m_combatData.nodeRootPositonX, val.y - m_combatData.nodeRootPositonY, val.z);
		}
		Vector2 val2 = RuntimePanelUtils.CameraTransformWorldToPanel(m_nodeCenter.panel, val, zbCameraHelper.Instance.mainCamera);
		Vector2 rootHudWorldBoundPos = m_combatData.combatHud.m_hudPool.m_rootHudWorldBoundPos;
		Vector2 val3 = val2 - rootHudWorldBoundPos;
		m_nodeCenter.style.translate = StyleTranslate.op_Implicit(new Translate(Length.op_Implicit(val3.x - maxWidth * 0.5f), Length.op_Implicit(val3.y - 8f), 0f));
	}

	private void UpdateShield()
	{
		if ((long)m_monster.shieldHp > 0)
		{
			zbUtil.SetDisplayIfChanged(m_progressShieldBg, (DisplayStyle)0);
			zbUtil.SetWidthIfChanged(m_progressShieldPb, (float)(long)m_monster.shieldHp / (float)m_monster.maxShieldHp * 160f);
		}
		else
		{
			zbUtil.SetDisplayIfChanged(m_progressShieldBg, (DisplayStyle)1);
		}
	}
}
