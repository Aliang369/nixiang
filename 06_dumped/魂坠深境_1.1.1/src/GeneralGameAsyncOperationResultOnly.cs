using System;
using System.Threading.Tasks;

public class GeneralGameAsyncOperationResultOnly<RESULT> : GameAsyncOperation
{
	private Func<Task<RESULT>> operation;

	private Task<RESULT> _task;

	public RESULT OpResult => _task.Result;

	protected override Task task => _task;

	protected override bool isNetworkTimeout
	{
		get
		{
			if (_task.IsCompleted)
			{
				return GameAsyncOperation.IsTimeoutException(_task.Exception);
			}
			return false;
		}
	}

	public GeneralGameAsyncOperationResultOnly(Func<Task<RESULT>> operation)
	{
		this.operation = operation;
		if (TraceLog.EnabledLog)
		{
			TraceLog.Debug("[GeneralGameAsyncOperationResultOnly] Create \n" + FormatOperation() + "\n");
		}
	}

	protected override void RunImpl()
	{
		if (TraceLog.EnabledLog)
		{
			TraceLog.Debug("[GeneralGameAsyncOperationResultOnly] Run \n" + FormatOperation() + "\n");
		}
		_task = operation();
	}

	protected override string FormatOperation()
	{
		return $"- option:{operation}\n- result: {typeof(RESULT)}";
	}
}
