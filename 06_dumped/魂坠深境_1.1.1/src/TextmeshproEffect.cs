using DG.Tweening;
using TMPro;
using UnityEngine;

public class TextmeshproEffect : MonoBehaviour
{
	public class Config
	{
		[SerializeField]
		public Color32 color;

		[SerializeField]
		public Ease ease;

		[SerializeField]
		public float time;
	}

	public class EffectParam
	{
		public static EffectParam _instance;

		public Float param;

		public static EffectParam instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new EffectParam();
				}
				return _instance;
			}
		}

		public EffectParam()
		{
			param = 0f;
			Sequence sequence = DOTween.Sequence();
			Config textMeshProEffectConfig = ScriptableObjectSingleton<VFXDataPreset>.instance.textMeshProEffectConfig;
			sequence.SetEase(textMeshProEffectConfig.ease);
			sequence.SetLoops(-1);
			sequence.Append(param.DOTo(1f, textMeshProEffectConfig.time));
			sequence.Append(param.DOTo(0f, textMeshProEffectConfig.time));
			sequence.Play();
		}

		public Color32 GetColor(Color32 color32)
		{
			Color32 color33 = ScriptableObjectSingleton<VFXDataPreset>.instance.textMeshProEffectConfig.color;
			return new Color32((byte)Mathf.Lerp((int)color32.r, (int)color33.r, param), (byte)Mathf.Lerp((int)color32.g, (int)color33.g, param), (byte)Mathf.Lerp((int)color32.b, (int)color33.b, param), (byte)Mathf.Lerp((int)color32.a, (int)color33.a, param));
		}
	}

	public TMP_Text textComponent;

	public Vector2 rollStrength = new Vector2(5f, 5f);

	public float rollSpeed = 10f;

	public Vector2 jitterStrength = new Vector2(5f, 5f);

	private void Update()
	{
		if (string.IsNullOrEmpty(textComponent.text))
		{
			return;
		}
		textComponent.ForceMeshUpdate();
		for (int i = 0; i < textComponent.textInfo.linkCount; i++)
		{
			if (textComponent.textInfo.linkInfo.Length <= i)
			{
				continue;
			}
			TMP_LinkInfo tMP_LinkInfo = textComponent.textInfo.linkInfo[i];
			int num = ((tMP_LinkInfo.linkTextLength == 0) ? (textComponent.textInfo.characterInfo.Length - tMP_LinkInfo.linkTextfirstCharacterIndex) : tMP_LinkInfo.linkTextLength);
			if (tMP_LinkInfo.GetLinkID() == "roll")
			{
				for (int j = tMP_LinkInfo.linkTextfirstCharacterIndex; j < tMP_LinkInfo.linkTextfirstCharacterIndex + num; j++)
				{
					TMP_CharacterInfo tMP_CharacterInfo = textComponent.textInfo.characterInfo[j];
					int materialReferenceIndex = tMP_CharacterInfo.materialReferenceIndex;
					Vector3[] vertices = textComponent.textInfo.meshInfo[materialReferenceIndex].vertices;
					for (int k = 0; k < 4; k++)
					{
						if (tMP_CharacterInfo.character != ' ')
						{
							int num2 = tMP_CharacterInfo.vertexIndex + k;
							if (num2 < vertices.Length)
							{
								Vector3 vector = new Vector2(Mathf.Sin(Time.realtimeSinceStartup * rollSpeed + (float)j) * rollStrength.x, Mathf.Cos(Time.realtimeSinceStartup * rollSpeed + (float)j) * rollStrength.y);
								vertices[num2] += vector;
							}
						}
					}
				}
			}
			else if (tMP_LinkInfo.GetLinkID() == "jitter")
			{
				for (int l = tMP_LinkInfo.linkTextfirstCharacterIndex; l < tMP_LinkInfo.linkTextfirstCharacterIndex + num; l++)
				{
					TMP_CharacterInfo tMP_CharacterInfo2 = textComponent.textInfo.characterInfo[l];
					int materialReferenceIndex2 = tMP_CharacterInfo2.materialReferenceIndex;
					Vector3[] vertices2 = textComponent.textInfo.meshInfo[materialReferenceIndex2].vertices;
					Vector3 vector2 = new Vector2(Random.Range(-1f, 1f) * jitterStrength.x, Random.Range(-1f, 1f) * jitterStrength.y);
					for (int m = 0; m < 4; m++)
					{
						if (tMP_CharacterInfo2.character != ' ')
						{
							int num3 = tMP_CharacterInfo2.vertexIndex + m;
							if (num3 < vertices2.Length)
							{
								vertices2[num3] += vector2;
							}
						}
					}
				}
			}
			else
			{
				if (!tMP_LinkInfo.GetLinkID().StartsWith("fade"))
				{
					continue;
				}
				for (int n = 0; n < textComponent.textInfo.linkInfo.Length; n++)
				{
					if (!(textComponent.textInfo.linkInfo[n].GetLinkID() == "end" + tMP_LinkInfo.GetLinkID()))
					{
						continue;
					}
					for (int num4 = tMP_LinkInfo.linkTextfirstCharacterIndex; num4 < textComponent.textInfo.linkInfo[n].linkTextfirstCharacterIndex; num4++)
					{
						TMP_CharacterInfo tMP_CharacterInfo3 = textComponent.textInfo.characterInfo[num4];
						int materialReferenceIndex3 = tMP_CharacterInfo3.materialReferenceIndex;
						Color32[] colors = textComponent.textInfo.meshInfo[materialReferenceIndex3].colors32;
						for (int num5 = 0; num5 < 4; num5++)
						{
							char character = tMP_CharacterInfo3.character;
							if (character != ' ' && character != '\n')
							{
								int num6 = tMP_CharacterInfo3.vertexIndex + num5;
								if (num6 <= colors.Length)
								{
									colors[num6] = EffectParam.instance.GetColor(colors[num6]);
								}
							}
						}
					}
					break;
				}
			}
		}
		textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
	}

	private void OnEnable()
	{
		textComponent = GetComponent<TextMeshProUGUI>();
	}

	public void ResetTMP()
	{
		textComponent.ClearMesh();
		textComponent.textInfo.Clear();
		textComponent.textInfo.ClearAllMeshInfo();
		textComponent.textInfo.ClearLineInfo();
	}
}
