using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WrapperEnumerator : IEnumerator
{
	public class Info
	{
		public WrapperEnumerator enumerator;

		public Action<Info> onFinish;
	}

	public MonoBehaviour attachMono;

	private readonly IEnumerator enumerator;

	private Info info;

	private readonly Action<Exception, string> onException;

	private readonly WrapperEnumerator parent;

	private readonly string enumeratorName;

	private Coroutine unityCor;

	private object current;

	private int moveNextCount;

	private int yieldIndex;

	private string lastYieldDescription = "None";

	public bool Finished;

	public object Current
	{
		get
		{
			if (current is IEnumerator enumerator)
			{
				return new WrapperEnumerator(enumerator, onException, this);
			}
			return current;
		}
	}

	public WrapperEnumerator(IEnumerator enumerator, Action<Exception, string> onException = null, WrapperEnumerator parent = null)
	{
		this.enumerator = enumerator;
		this.onException = onException;
		this.parent = parent;
		enumeratorName = GetEnumeratorName(enumerator);
		Finished = false;
	}

	public bool MoveNext()
	{
		moveNextCount++;
		try
		{
			bool num = enumerator.MoveNext();
			if (!num)
			{
				OnFinish();
			}
			else
			{
				current = enumerator.Current;
				yieldIndex++;
				lastYieldDescription = DescribeYield(current);
			}
			return num;
		}
		catch (Exception arg)
		{
			onException?.Invoke(arg, BuildExceptionContext());
			return false;
		}
	}

	public void Reset()
	{
		enumerator.Reset();
	}

	public static WrapperEnumerator StartCoroutine(Func<IEnumerator> enumerator, MonoBehaviour attachMono, Info info = null)
	{
		if (info == null)
		{
			info = new Info();
		}
		info.enumerator = new WrapperEnumerator(enumerator(), delegate(Exception e, string context)
		{
			ReportCoroutineException(e, "嵌套协程错误-" + enumerator.GetType().Name + "-" + attachMono.GetType().Name + "\n" + context + "\n");
		});
		info.enumerator.info = info;
		info.enumerator.attachMono = attachMono;
		info.enumerator.unityCor = attachMono.StartCoroutine(info.enumerator);
		return info.enumerator;
	}

	public static WrapperEnumerator StartCoroutine(Func<Info, IEnumerator> enumerator, MonoBehaviour attachMono, Info info = null)
	{
		if (enumerator == null)
		{
			return null;
		}
		if (info == null)
		{
			info = new Info();
		}
		info.enumerator = new WrapperEnumerator(enumerator(info), delegate(Exception e, string context)
		{
			ReportCoroutineException(e, "嵌套协程错误-" + enumerator.GetType().Name + "-" + attachMono.GetType().Name + "\n" + context + "\n");
		});
		info.enumerator.info = info;
		info.enumerator.attachMono = attachMono;
		info.enumerator.unityCor = attachMono.StartCoroutine(info.enumerator);
		return info.enumerator;
	}

	public void Stop()
	{
		if (unityCor != null)
		{
			attachMono.StopCoroutine(unityCor);
		}
	}

	private void OnFinish()
	{
		Finished = true;
		try
		{
			info?.onFinish?.Invoke(info);
		}
		catch (Exception arg)
		{
			onException?.Invoke(arg, BuildExceptionContext());
		}
	}

	private string BuildExceptionContext()
	{
		return $"Coroutine={enumeratorName}, Block={GetCurrentBlockDescription()}, MoveNextCount={moveNextCount}, LastYield={lastYieldDescription}, Chain={BuildCoroutineChain()}";
	}

	private string GetCurrentBlockDescription()
	{
		if (yieldIndex == 0)
		{
			return "BeforeFirstYield";
		}
		return $"AfterYield#{yieldIndex}";
	}

	private string BuildCoroutineChain()
	{
		List<string> list = new List<string>();
		for (WrapperEnumerator wrapperEnumerator = this; wrapperEnumerator != null; wrapperEnumerator = wrapperEnumerator.parent)
		{
			list.Add(wrapperEnumerator.enumeratorName);
		}
		list.Reverse();
		return string.Join(" -> ", list);
	}

	private static string GetEnumeratorName(IEnumerator enumerator)
	{
		if (enumerator == null)
		{
			return "Null";
		}
		return enumerator.GetType().FullName;
	}

	private static string DescribeYield(object value)
	{
		if (value == null)
		{
			return "null";
		}
		return value.GetType().FullName;
	}

	private static void ReportCoroutineException(Exception e, string message)
	{
		TraceLog.Error(message);
		TraceLog.Exception(e, message, ThinkingAnalyticsExtends.ExceptionType.Coroutine);
	}

	public static IEnumerator WaitAll(List<WrapperEnumerator> enumerators)
	{
		yield return new WaitUntil(delegate
		{
			foreach (WrapperEnumerator enumerator2 in enumerators)
			{
				if (!enumerator2.Finished)
				{
					return false;
				}
			}
			return true;
		});
	}
}
