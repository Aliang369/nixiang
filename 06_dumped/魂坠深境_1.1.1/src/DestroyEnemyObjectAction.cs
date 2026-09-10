using UnityEngine;

public class DestroyEnemyObjectAction : GameAction
{
	private readonly CharacterAI target;

	public DestroyEnemyObjectAction(CharacterAI target)
	{
		this.target = target;
	}

	protected override void Update()
	{
		if (target != null)
		{
			Object.Destroy(target.component.gameObject);
		}
		FinishAction();
	}
}
