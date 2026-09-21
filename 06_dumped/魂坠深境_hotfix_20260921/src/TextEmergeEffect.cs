using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class TextEmergeEffect : MonoBehaviour
{
	public struct WordData
	{
		public char word;

		public Color color;

		public int lineNumber;
	}

	[Serializable]
	public struct EffectConfig
	{
		[SerializeField]
		public float emergeTimePerWord;

		[SerializeField]
		public float waitTime;

		[SerializeField]
		public float accleratedTime;
	}

	[SerializeField]
	public TextMeshProUGUI text;

	[NonSerialized]
	private bool accelerated;

	[NonSerialized]
	private Coroutine coroutine;

	[NonSerialized]
	private bool finish;

	[NonSerialized]
	public float timeScale = 1f;

	private void Update()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (!Input.GetMouseButtonDown(0))
		{
			if (Input.touchCount <= 0)
			{
				return;
			}
			Touch touch = Input.GetTouch(0);
			if ((int)((Touch)(ref touch)).phase != 0)
			{
				return;
			}
		}
		accelerated = true;
	}

	public void StartEmergeText(string emergeText, Action onFinish = null)
	{
		StartEmergeText(emergeText, ScriptableObjectSingleton<VFXDataPreset>.instance.textEmergeEffect, onFinish);
	}

	public void StartEmergeText(string emergeText, EffectConfig config, Action onFinish = null)
	{
		text.text = emergeText;
		StartEmergeText(config, onFinish);
	}

	public void StartEmergeText(Action onFinish = null)
	{
		StartEmergeText(ScriptableObjectSingleton<VFXDataPreset>.instance.textEmergeEffect, onFinish);
	}

	public void StartEmergeText(EffectConfig config, Action onFinish = null)
	{
		if (!base.gameObject.activeInHierarchy)
		{
			onFinish?.Invoke();
			return;
		}
		if (coroutine != null)
		{
			StopCoroutine(coroutine);
		}
		coroutine = StartCoroutine(EmergeText(config, onFinish));
	}

	public void Finish()
	{
		finish = true;
	}

	private IEnumerator EmergeText(EffectConfig config, Action onFinish = null)
	{
		text.ForceMeshUpdate();
		text.maxVisibleCharacters = 0;
		accelerated = false;
		finish = false;
		string originText = text.text;
		if (originText == null)
		{
			yield break;
		}
		int currentStringIndex = 0;
		WordData[] wordDatas = GetWordData();
		int currentWordIndex = 0;
		float timeSinceCurrentWorld = 0f;
		float waitTime = config.waitTime;
		float emergeTime = config.emergeTimePerWord;
		float accleratedTime = config.accleratedTime / (float)wordDatas.Length;
		while (currentStringIndex <= originText.Length && currentWordIndex < wordDatas.Length)
		{
			if (finish)
			{
				text.text = originText;
				break;
			}
			if (accelerated)
			{
				waitTime = 0f;
				emergeTime = accleratedTime;
				accelerated = false;
			}
			yield return null;
			timeSinceCurrentWorld += Time.deltaTime * timeScale;
			while (timeSinceCurrentWorld >= emergeTime && currentStringIndex <= originText.Length && currentWordIndex < wordDatas.Length)
			{
				timeSinceCurrentWorld -= emergeTime;
				currentWordIndex++;
				currentStringIndex++;
				text.maxVisibleCharacters = currentWordIndex;
				if (waitTime != 0f && originText.Substring(currentStringIndex).StartsWith("</noparse>"))
				{
					yield return new WaitForSeconds(waitTime);
				}
				bool flag = false;
				while (currentStringIndex < originText.Length)
				{
					if (originText[currentStringIndex] == '<')
					{
						flag = true;
						currentStringIndex++;
						if (originText.Length < currentStringIndex)
						{
							Debug.LogError("文本配置错误: " + originText);
							text.maxVisibleCharacters = 99999;
							onFinish?.Invoke();
							yield break;
						}
						continue;
					}
					if (originText[currentStringIndex] == '>')
					{
						flag = false;
						currentStringIndex++;
						if (originText.Length < currentStringIndex)
						{
							Debug.LogError("文本配置错误: " + originText);
							text.maxVisibleCharacters = 99999;
							onFinish?.Invoke();
							yield break;
						}
						continue;
					}
					if (!flag)
					{
						break;
					}
					currentStringIndex++;
					if (originText.Length < currentStringIndex)
					{
						Debug.LogError("文本配置错误: " + originText);
						text.maxVisibleCharacters = 99999;
						onFinish?.Invoke();
						yield break;
					}
				}
			}
		}
		text.maxVisibleCharacters = 99999;
		onFinish?.Invoke();
	}

	private WordData[] GetWordData()
	{
		int characterCount = text.textInfo.characterCount;
		WordData[] array = new WordData[characterCount];
		TMP_CharacterInfo[] characterInfo = text.textInfo.characterInfo;
		for (int i = 0; i < characterCount; i++)
		{
			array[i] = new WordData
			{
				color = characterInfo[i].color,
				word = characterInfo[i].character,
				lineNumber = characterInfo[i].lineNumber
			};
		}
		return array;
	}
}
