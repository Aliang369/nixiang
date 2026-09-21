using System;

public class ExceptionTraceManager : GlobalGameManager
{
	public ExceptionTraceManager()
	{
		AppDomain.CurrentDomain.UnhandledException += _OnUnhandledException;
	}

	public override void Init(ApplicationStateInfo applicationState)
	{
	}

	public override void Dispose()
	{
		AppDomain.CurrentDomain.UnhandledException -= _OnUnhandledException;
	}

	private void _OnUnhandledException(object sender, UnhandledExceptionEventArgs args)
	{
		if (args.ExceptionObject is Exception ex)
		{
			ThinkingAnalyticsExtends.TraceException(ex.Message, ex.StackTrace, ThinkingAnalyticsExtends.ExceptionType.UnHandled);
		}
	}
}
