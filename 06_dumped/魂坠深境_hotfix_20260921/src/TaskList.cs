using System.Collections.Generic;
using System.Threading.Tasks;

public class TaskList
{
	public List<Task> tasks;

	public void Attach(Task task)
	{
		task.Dispose();
		tasks.Add(task);
	}
}
