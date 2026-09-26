using System.Collections;
using UnityEngine;

public class zbSpriteColorChange
{
	private MonoBehaviour m_mono;

	private Coroutine m_myCoroutine;

	public zbSpriteColorChange(MonoBehaviour mono)
	{
		m_mono = mono;
	}

	public void OnEnable()
	{
	}

	public void OnDisable()
	{
		RemoveOldCoroutine();
	}

	private void RemoveOldCoroutine()
	{
		if (m_myCoroutine != null)
		{
			m_mono.StopCoroutine(m_myCoroutine);
			m_myCoroutine = null;
		}
	}

	public void LerpChangeColor()
	{
		if (Object.op_Implicit((Object)(object)m_mono))
		{
			RemoveOldCoroutine();
			SpriteRenderer[] componentsInChildren = ((Component)m_mono).gameObject.GetComponentsInChildren<SpriteRenderer>();
			m_myCoroutine = m_mono.StartCoroutine(ChangeColor(componentsInChildren));
		}
	}

	private IEnumerator ChangeColor(SpriteRenderer[] spriteRenderers)
	{
		Color startColor = Color.white;
		Color targetColor = Color.red;
		float duration = 0.5f;
		float timeElapsed = 0f;
		SpriteRenderer[] array;
		while (timeElapsed < duration)
		{
			array = spriteRenderers;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].color = Color.Lerp(startColor, targetColor, timeElapsed / duration);
			}
			timeElapsed += Time.deltaTime;
			yield return null;
		}
		array = spriteRenderers;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].color = targetColor;
		}
		timeElapsed = 0f;
		while (timeElapsed < duration)
		{
			array = spriteRenderers;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].color = Color.Lerp(targetColor, startColor, timeElapsed / duration);
			}
			timeElapsed += Time.deltaTime;
			yield return null;
		}
		array = spriteRenderers;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].color = startColor;
		}
	}
}
