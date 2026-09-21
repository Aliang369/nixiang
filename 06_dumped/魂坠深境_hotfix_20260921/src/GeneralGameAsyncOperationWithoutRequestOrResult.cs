using System;
using System.Linq;
using System.Threading.Tasks;
using ChillyRoom.Services.Core.Errors;

public class GeneralGameAsyncOperationWithoutRequestOrResult : GameAsyncOperation
{
	private Func<Task> operation;

	private Task _task;

	protected override Task task => _task;

	protected override bool isNetworkTimeout
	{
		get
		{
			if (_task.IsCompleted && _task.Exception != null)
			{
				return _task.Exception.Flatten().InnerExceptions.Any((Exception ex) => ex is ApiException<ErrorResponse> ex2 && ex2.StatusCode == 500);
			}
			return false;
		}
	}

	public GeneralGameAsyncOperationWithoutRequestOrResult(Func<Task> operation)
	{
		this.operation = operation;
		TraceLog.Debug("[GeneralGameAsyncOperationWithoutRequestOrResult] Create \n" + FormatOperation());
	}

	protected override void RunImpl()
	{
		TraceLog.Debug("[GeneralGameAsyncOperationWithoutRequestOrResult] Run \n" + FormatOperation());
		_task = operation();
	}

	protected override string FormatOperation()
	{
		return $"- option:{operation}\n";
	}
}
