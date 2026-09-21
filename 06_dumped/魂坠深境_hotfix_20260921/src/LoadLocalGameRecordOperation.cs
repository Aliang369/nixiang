using System;
using System.Linq;
using System.Threading.Tasks;
using ChillyRoom.Services.Core.Errors;

public class LoadLocalGameRecordOperation : GameAsyncOperation
{
	private string accountId;

	private string gameId;

	private Task<GameRecord> _task;

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

	public GameRecord RecordResult => _task?.Result;

	public LoadLocalGameRecordOperation(string gameId)
	{
		accountId = Bootstrap.instance.CurrentAccountID;
		this.gameId = gameId;
		TraceLog.Debug("[LoadLocalGameRecordOperation] Create \n" + FormatOperation());
	}

	protected override void RunImpl()
	{
		TraceLog.Debug("[LoadLocalGameRecordOperation] Run \n" + FormatOperation());
		_task = SavingUtil.LoadGameRecordAsync(accountId, gameId);
	}

	protected override string FormatOperation()
	{
		return "- accountId:" + accountId + "\n- gameId:" + gameId + "\n";
	}
}
