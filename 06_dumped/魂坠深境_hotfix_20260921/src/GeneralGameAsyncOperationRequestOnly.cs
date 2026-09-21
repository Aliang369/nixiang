using System;
using System.Linq;
using System.Threading.Tasks;
using ChillyRoom.Services.Core.Errors;

public class GeneralGameAsyncOperationRequestOnly<REQUEST> : GameAsyncOperation
{
	private Func<REQUEST, Task> operation;

	private REQUEST request;

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

	public GeneralGameAsyncOperationRequestOnly(REQUEST request, Func<REQUEST, Task> operation)
	{
		this.operation = operation;
		this.request = request;
		if (TraceLog.EnabledLog)
		{
			TraceLog.Debug("[GeneralGameAsyncOperationRequestOnly] Create \n" + FormatOperation());
		}
	}

	protected override void RunImpl()
	{
		if (TraceLog.EnabledLog)
		{
			TraceLog.Debug("[GeneralGameAsyncOperationRequestOnly] Run \n" + FormatOperation());
		}
		_task = operation(request);
	}

	protected override string FormatOperation()
	{
		return $"- option:{operation}\n- request: {typeof(REQUEST)}";
	}
}
