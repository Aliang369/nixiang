using UnityEngine;

public class zbCombatDefenseHp : MonoBehaviour
{
	private zbCombatData m_combatData;

	private ESortLayerType m_sortType = ESortLayerType.TeamHp;

	private SpriteRenderer m_spriteRenderer;

	private SpriteRenderer m_shieldRenderer;

	private Transform m_progressBg;

	private Transform m_progressShieldBg;

	private float progressRatio;

	private float progressShieldRatio;

	private Vector2 kResultPos;

	private const float FullWidth = 4.92f;

	private float m_invMaxHp = 1f;

	private float m_invMaxShieldHp = 1f;

	private float m_lastProgressRatio = -1f;

	private float m_lastShieldRatio = -1f;

	private void OnEnable()
	{
		zbSortMgr.Instance.InitSortOrder(m_sortType, ((Component)this).gameObject);
		m_progressBg = ((Component)this).transform.Find("progressBg");
		m_spriteRenderer = ((Component)((Component)this).transform.Find("progressBg").Find("progressFg")).gameObject.GetComponent<SpriteRenderer>();
		m_progressShieldBg = ((Component)this).transform.Find("progressShieldBg");
		m_shieldRenderer = ((Component)((Component)this).transform.Find("progressShieldBg").Find("progressShield")).gameObject.GetComponent<SpriteRenderer>();
		m_lastProgressRatio = -1f;
		m_lastShieldRatio = -1f;
		SetProgress(m_shieldRenderer, 0f);
	}

	public void InitData(zbCombatData combatData)
	{
		m_combatData = combatData;
		if ((long)m_combatData.defense.maxHp > 0)
		{
			m_invMaxHp = 1f / (float)(long)m_combatData.defense.maxHp;
		}
		if ((long)m_combatData.defense.maxShieldHp > 0)
		{
			m_invMaxShieldHp = 1f / (float)(long)m_combatData.defense.maxShieldHp;
		}
	}

	public void SetProgress(SpriteRenderer sr, float percent)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		float num = 4.92f * percent;
		Vector2 size = sr.size;
		size.x = num;
		sr.size = size;
		Vector3 localPosition = ((Component)sr).transform.localPosition;
		localPosition.x = (num - 4.92f) * 0.5f;
		((Component)sr).transform.localPosition = localPosition;
	}

	public Vector2 GetProgressPosition()
	{
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		if (m_combatData.dataIndex == 2)
		{
			num = 1f;
		}
		float x;
		if ((long)m_combatData.defense.shieldHp > 0)
		{
			x = m_progressShieldBg.position.x - 2.46f + progressShieldRatio * 4.92f;
			kResultPos.x = x;
			kResultPos.y = m_progressShieldBg.position.y + num;
			return kResultPos;
		}
		x = m_progressBg.position.x - 2.46f + progressRatio * 4.92f;
		kResultPos.x = x;
		kResultPos.y = m_progressBg.position.y + num;
		return kResultPos;
	}

	private void Update()
	{
		progressRatio = (float)(long)m_combatData.defense.curHp * m_invMaxHp;
		if (progressRatio < 0f)
		{
			progressRatio = 0f;
		}
		else if (progressRatio > 1f)
		{
			progressRatio = 1f;
		}
		if (progressRatio != m_lastProgressRatio)
		{
			m_lastProgressRatio = progressRatio;
			SetProgress(m_spriteRenderer, progressRatio);
		}
		if ((long)m_combatData.defense.maxShieldHp > 0)
		{
			progressShieldRatio = (float)(long)m_combatData.defense.shieldHp * m_invMaxShieldHp;
			if (progressShieldRatio < 0f)
			{
				progressShieldRatio = 0f;
			}
			else if (progressShieldRatio > 1f)
			{
				progressShieldRatio = 1f;
			}
			if (progressShieldRatio != m_lastShieldRatio)
			{
				m_lastShieldRatio = progressShieldRatio;
				SetProgress(m_shieldRenderer, progressShieldRatio);
			}
			if (progressShieldRatio <= 0f)
			{
				if (((Component)m_progressShieldBg).gameObject.activeSelf)
				{
					((Component)m_progressShieldBg).gameObject.SetActive(false);
				}
			}
			else if (!((Component)m_progressShieldBg).gameObject.activeSelf)
			{
				((Component)m_progressShieldBg).gameObject.SetActive(true);
			}
		}
		else if (((Component)m_progressShieldBg).gameObject.activeSelf)
		{
			((Component)m_progressShieldBg).gameObject.SetActive(false);
		}
	}
}
