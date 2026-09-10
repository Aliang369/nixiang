using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ImageFrameAnimation : MonoBehaviour
{
	public float time = 1f;

	public bool loop;

	public bool inverse;

	public List<Sprite> frames;

	[NonSerialized]
	private Image _image;

	private Image image
	{
		get
		{
			if (_image == null)
			{
				_image = GetComponent<Image>();
			}
			return _image;
		}
	}

	public bool IsPlaying { get; private set; }

	public void Play()
	{
		if (!IsPlaying)
		{
			StartCoroutine(PlayCor());
		}
	}

	private IEnumerator PlayCor()
	{
		IsPlaying = true;
		do
		{
			float t = 0f;
			while (true)
			{
				t += Time.deltaTime;
				int num = (int)(t / time * (float)frames.Count);
				if (num < frames.Count)
				{
					image.sprite = (inverse ? frames[frames.Count - 1 - num] : frames[num]);
				}
				else
				{
					if (loop)
					{
						break;
					}
					IsPlaying = false;
				}
				if (!IsPlaying)
				{
					break;
				}
				yield return null;
			}
		}
		while (IsPlaying);
	}

	public void Stop()
	{
		IsPlaying = false;
	}
}
