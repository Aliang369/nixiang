using System.Threading.Tasks;
using UnityEngine;

public class WaitTask : CustomYieldInstruction
{
	private readonly Task task;

	public override bool keepWaiting
	{
		get
		{
			if (task == null)
			{
				return false;
			}
			return !task.IsCompleted;
		}
	}

	public WaitTask(Task task)
	{
		this.task = task;
	}
}
