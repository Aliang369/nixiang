using System;
using UnityEngine;

namespace Common.UI;

public abstract class MonoDisplayModule : MonoBehaviour
{
	[NonSerialized]
	private RectTransform _rectTransform;

	[NonSerialized]
	public UIBase parent;

	public ApplicationStateInfo applicationState => Bootstrap.applicationState;

	public FortressStateInfo fortressState => applicationState.fortressState;

	public GameStateInfo gameState => applicationState.runningGameState;

	public RectTransform rectTransform => _rectTransform ?? (_rectTransform = base.transform as RectTransform);

	public UIManager manager => parent.manager;

	public virtual void Init()
	{
	}

	public virtual void Dispose()
	{
	}

	public virtual void UpdateDisplay()
	{
	}

	public virtual void Setup()
	{
	}

	public virtual void Unset()
	{
	}

	public virtual void OnConfigContext()
	{
	}
}
