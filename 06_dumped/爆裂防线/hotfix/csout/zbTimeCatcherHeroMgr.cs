using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbTimeCatcherHeroMgr
{
	private zbTimeCatcher m_timeCatcher;

	private VisualElement m_nodeHero;

	private Vector2 m_itemSize;

	private float m_scrollSpeed;

	private float[] m_positions;

	private float m_gap;

	private float m_nodeHeroW;

	public bool isCatcherHero;

	private int m_itemCount;

	public zbTimeCatcherHeroMgr(zbTimeCatcher timeCatcher)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		m_itemSize = new Vector2(124f, 230f);
		m_scrollSpeed = 80f;
		m_gap = 50f;
		m_nodeHeroW = 670f;
		base._002Ector();
		m_timeCatcher = timeCatcher;
	}

	public void OnEnable(VisualElement nodeHero)
	{
		m_nodeHero = nodeHero;
	}

	private zbTimeCatcherHero CreateNewHeroElement(int heroId, LimitTimeCatcherDetail detail)
	{
		zbCatcherHeroInfo heroInfo = new zbCatcherHeroInfo
		{
			heroId = heroId,
			fragPerCoin = detail.FragPerCoin,
			coinCount = detail.CoinCount
		};
		zbTimeCatcherHero obj = new zbTimeCatcherHero(m_timeCatcher);
		obj.Init(heroInfo);
		return obj;
	}

	private void RefresHeroInfo(LimitTimeCatcherDetail limitTimeCatcherDetail)
	{
		for (int i = 0; i < m_itemCount; i++)
		{
			if (i < limitTimeCatcherDetail.HeroList.Count)
			{
				zbCatcherHeroInfo zbCatcherHeroInfo2 = new zbCatcherHeroInfo();
				zbCatcherHeroInfo2.heroId = limitTimeCatcherDetail.HeroList[i];
				zbCatcherHeroInfo2.fragPerCoin = limitTimeCatcherDetail.FragPerCoin;
				zbCatcherHeroInfo2.coinCount = limitTimeCatcherDetail.CoinCount;
				if (m_nodeHero[i] is zbTimeCatcherHero zbTimeCatcherHero2)
				{
					zbTimeCatcherHero2.Init(zbCatcherHeroInfo2);
				}
			}
		}
	}

	public void CreatehHero(LimitTimeCatcherDetail limitTimeCatcherDetail)
	{
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		int count = limitTimeCatcherDetail.HeroList.Count;
		if (count == m_itemCount)
		{
			RefresHeroInfo(limitTimeCatcherDetail);
			return;
		}
		m_itemCount = count;
		m_positions = new float[count];
		for (int i = 0; i < count; i++)
		{
			zbCatcherHeroInfo zbCatcherHeroInfo2 = new zbCatcherHeroInfo();
			zbCatcherHeroInfo2.heroId = limitTimeCatcherDetail.HeroList[i];
			zbCatcherHeroInfo2.fragPerCoin = limitTimeCatcherDetail.FragPerCoin;
			zbCatcherHeroInfo2.coinCount = limitTimeCatcherDetail.CoinCount;
			if (i < m_nodeHero.childCount)
			{
				if (m_nodeHero[i] is zbTimeCatcherHero zbTimeCatcherHero2)
				{
					zbTimeCatcherHero2.Init(zbCatcherHeroInfo2);
					continue;
				}
				m_nodeHero.RemoveAt(i);
				zbTimeCatcherHero zbTimeCatcherHero3 = CreateNewHeroElement(zbCatcherHeroInfo2.heroId, limitTimeCatcherDetail);
				zbTimeCatcherHero3.RefreshHeroDetail();
				m_nodeHero.Insert(i, (VisualElement)(object)zbTimeCatcherHero3);
			}
			else
			{
				zbTimeCatcherHero zbTimeCatcherHero3 = CreateNewHeroElement(zbCatcherHeroInfo2.heroId, limitTimeCatcherDetail);
				zbTimeCatcherHero3.RefreshHeroDetail();
				m_nodeHero.Add((VisualElement)(object)zbTimeCatcherHero3);
			}
		}
		while (m_nodeHero.childCount > count)
		{
			m_nodeHero.RemoveAt(m_nodeHero.childCount - 1);
		}
		float num = m_nodeHeroW;
		for (int j = 0; j < m_nodeHero.childCount; j++)
		{
			m_positions[j] = num;
			m_nodeHero[j].style.left = StyleLength.op_Implicit(num);
			num += m_itemSize.x + m_gap;
		}
	}

	private float GetMaxRight()
	{
		float num = float.MinValue;
		float[] positions = m_positions;
		for (int i = 0; i < positions.Length; i++)
		{
			float num2 = positions[i] + m_itemSize.x + m_gap;
			if (num2 > num)
			{
				num = num2;
			}
		}
		return num;
	}

	public void Update()
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		float num = m_scrollSpeed * Time.deltaTime;
		for (int i = 0; i < m_nodeHero.childCount; i++)
		{
			m_positions[i] -= num;
			if (m_positions[i] + m_itemSize.x < 0f)
			{
				m_positions[i] = GetMaxRight();
			}
			m_nodeHero[i].style.left = StyleLength.op_Implicit(m_positions[i]);
		}
		if (!isCatcherHero)
		{
			CheckColiisionWithCatcher();
		}
	}

	private void CheckColiisionWithCatcher()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		if (m_nodeHero == null || m_timeCatcher.GetCatcherRect() == null)
		{
			return;
		}
		foreach (VisualElement item in m_nodeHero.Children())
		{
			if (item is zbTimeCatcherHero zbTimeCatcherHero2)
			{
				Rect worldBound = zbTimeCatcherHero2.heroRect.worldBound;
				Rect worldBound2 = m_timeCatcher.GetCatcherRect().worldBound;
				if (((Rect)(ref worldBound)).Overlaps(worldBound2))
				{
					isCatcherHero = true;
					m_timeCatcher.OnCatcherHero(zbTimeCatcherHero2.catcherHeroInfo.heroId);
					zbTimeCatcherHero2.SetHeroVisible(visible: false);
					m_timeCatcher.PlayCatchEffect(zbTimeCatcherHero2);
					break;
				}
			}
		}
	}
}
