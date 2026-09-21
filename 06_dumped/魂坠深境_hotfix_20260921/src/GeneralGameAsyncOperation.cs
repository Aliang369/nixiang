using System;
using System.Linq;
using System.Threading.Tasks;
using ChillyRoom.Services.Core.Errors;

public class GeneralGameAsyncOperation<REQUEST, RESULT> : GameAsyncOperation
{
	private Func<REQUEST, Task<RESULT>> operation;

	private REQUEST request;

	private Task<RESULT> _task;

	public RESULT OpResult => _task.Result;

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

	public GeneralGameAsyncOperation(REQUEST request, Func<REQUEST, Task<RESULT>> operation)
	{
		this.operation = operation;
		this.request = request;
		if (TraceLog.EnabledLog)
		{
			TraceLog.Debug("[GeneralGameAsyncOperation] Create \n" + FormatOperation());
		}
	}

	protected override void RunImpl()
	{
		if (TraceLog.EnabledLog)
		{
			TraceLog.Debug("[GeneralGameAsyncOperation] Run \n" + FormatOperation());
		}
		_task = operation(request);
	}

	protected override string FormatOperation()
	{
		return $"- option:{operation}\n- request: {typeof(REQUEST)} \n- result: {typeof(RESULT)}";
	}
}
