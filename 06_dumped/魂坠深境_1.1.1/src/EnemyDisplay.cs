using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDisplay : MonoBehaviour
{
	[NonSerialized]
	public List<IEnemyDisplayModule> displayModules;

	private void Awake()
	{
		displayModules = new List<IEnemyDisplayModule>();
		IEnemyDisplayModule[] components = GetComponents<IEnemyDisplayModule>();
		foreach (IEnemyDisplayModule enemyDisplayModule in components)
		{
			displayModules.Add(enemyDisplayModule);
			enemyDisplayModule.Initialize(this);
		}
	}

	public void UpdateContent()
	{
		foreach (IEnemyDisplayModule displayModule in displayModules)
		{
			displayModule.UpdateContent();
		}
	}
}
