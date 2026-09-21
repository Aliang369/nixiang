using System;
using UnityEngine;

public class MenuController : MonoBehaviour
{
	public static Action OnLoadEnd;

	private void Start()
	{
		OnLoadEnd?.Invoke();
	}
}
