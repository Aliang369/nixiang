using UnityEngine;

public class zbMonsterSprite
{
	public zbMonster monster;

	private SpriteRenderer m_spriteMonster;

	private GameObject m_spriteMonsterGO;

	private Color m_normalColor;

	private Color m_redColor;

	private float m_lerpTime;

	private float m_lerpTimeKeep;

	private int m_Lerping;

	public void Init(zbMonster obj)
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		monster = obj;
		m_spriteMonster = ((Component)((Component)monster).transform.Find("subObject").Find("spriteMonster")).GetComponent<SpriteRenderer>();
		m_spriteMonsterGO = ((Component)m_spriteMonster).gameObject;
		m_spriteMonsterGO.SetActive(false);
		if ((Object)(object)m_spriteMonster != (Object)null)
		{
			m_normalColor = m_spriteMonster.color;
		}
	}

	public void SetMonsterSprite()
	{
		if (monster.monsterAvatarRecord.bigIcon != "")
		{
			Sprite val = zbAssetLoader.Instance.LoadCombatSpriteAsset(monster.monsterAvatarRecord.bigIcon);
			if ((Object)(object)val != (Object)null)
			{
				m_spriteMonster.sprite = val;
				SetMonsterSpriteActive(bActive: true);
			}
		}
	}

	public void SetMonsterSpriteActive(bool bActive)
	{
		if ((Object)(object)m_spriteMonster != (Object)null)
		{
			m_spriteMonsterGO.SetActive(bActive);
		}
		if ((Object)(object)monster != (Object)null)
		{
			monster.isUsingSprite = bActive;
		}
	}

	public void ResetColor()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)m_spriteMonster != (Object)null)
		{
			m_spriteMonster.color = m_normalColor;
		}
		m_Lerping = 0;
		m_lerpTimeKeep = 0f;
	}

	public void LerpChangeColor()
	{
		if (m_Lerping == 0)
		{
			m_Lerping = 1;
			m_lerpTimeKeep = 0f;
		}
	}

	public void Update()
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)m_spriteMonster == (Object)null || m_Lerping == 0)
		{
			return;
		}
		if (m_Lerping == 1)
		{
			m_lerpTimeKeep += Time.deltaTime;
			float num = Mathf.Clamp01(m_lerpTimeKeep / m_lerpTime);
			m_spriteMonster.color = Color.Lerp(m_normalColor, m_redColor, num);
			if (num >= 1f)
			{
				m_lerpTimeKeep = 0f;
				m_Lerping = 2;
			}
		}
		else if (m_Lerping == 2)
		{
			m_lerpTimeKeep += Time.deltaTime;
			float num2 = Mathf.Clamp01(m_lerpTimeKeep / m_lerpTime);
			m_spriteMonster.color = Color.Lerp(m_redColor, m_normalColor, num2);
			if (num2 >= 1f)
			{
				m_Lerping = 0;
			}
		}
	}

	public zbMonsterSprite()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		m_redColor = Color.red;
		m_lerpTime = 0.2f;
		base._002Ector();
	}
}
