using System;
using System.Collections.Generic;

namespace Common.UI;

public class UIStack
{
	private Stack<UIBase> stack = new Stack<UIBase>();

	public Action<UIBase> showUI;

	public Action<UIBase> hideUI;

	public void Show(UIBase uiBase, UIBase current)
	{
		stack.Push(current);
		hideUI?.Invoke(current);
		showUI?.Invoke(uiBase);
	}

	public void Hide(UIBase uiBase)
	{
		hideUI?.Invoke(uiBase);
		if (stack.Count > 0)
		{
			showUI?.Invoke(stack.Pop());
		}
	}
}
