using System;
using UnityEngine;
using UnityEngine.UI;

public class CompleteModule : UIDisplayModule
{
	public class Data
	{
		public Func<bool> completed;
	}

	[SerializeField]
	private Image image;

	[NonSerialized]
	private Data data;

	public override void Setup()
	{
		data = parent.GetAppendData<Data>();
	}

	public override void UpdateDisplay()
	{
		image.gameObject.SetActive(value: false);
		Data obj = data;
		if (obj != null && obj.completed?.Invoke() == true)
		{
			image.gameObject.SetActive(value: true);
		}
	}

	public override void Unset()
	{
		data = null;
	}

	public static (object, object) GetAppendData(Func<bool> completed)
	{
		return (typeof(Data), new Data
		{
			completed = completed
		});
	}
}
