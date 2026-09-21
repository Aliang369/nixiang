using System.Collections.Generic;
using System.Linq;
using Common.UI;

public static class ScrollRectGridLayoutGroupExtends
{
	public static void UpdateSequenceDisplay<TDisplay, TElement>(List<TElement> elements, CellUIPrefabPool<TDisplay, TElement> pool, ScrollRectGridLayoutGroup layoutGroup) where TDisplay : CellUI<TElement>
	{
		if (SequenceEqual(layoutGroup.displayElements.Select((ScrollRectGridLayoutGroup.DisplayElement d) => (TElement)d.data).ToList(), elements))
		{
			return;
		}
		pool.DespawnAll();
		layoutGroup.ClearAllDisplayElement();
		foreach (TElement item in elements)
		{
			TDisplay display = null;
			layoutGroup.AddDisplayElement(new ScrollRectGridLayoutGroup.DisplayElement
			{
				data = item,
				onShow = delegate
				{
					display = pool.Spawn(item);
					display.SetDirty();
					return display.gameObject;
				},
				onHide = delegate
				{
					pool.Despawn(display);
				}
			});
		}
		layoutGroup.RefreshLayout();
	}

	public static void UpdateSequenceDisplay<TDisplay, TElement>(this CellUIPrefabPool<TDisplay, TElement> pool, List<TElement> elements, ScrollRectGridLayoutGroup layoutGroup) where TDisplay : CellUI<TElement>
	{
		UpdateSequenceDisplay(elements, pool, layoutGroup);
	}

	public static void UpdateSequenceDisplay<TDisplay, TElement, TSetupData>(this CellUIPrefabPool<TDisplay, TSetupData> pool, List<TElement> elements, ScrollRectGridLayoutGroup layoutGroup, CellUIPrefabPool<TDisplay, TSetupData>.IDataConvertor<TElement> convertor) where TDisplay : CellUI<TSetupData>
	{
		if (SequenceEqual(layoutGroup.displayElements.Select((ScrollRectGridLayoutGroup.DisplayElement d) => (TElement)d.data).ToList(), elements))
		{
			return;
		}
		pool.DespawnAll();
		layoutGroup.ClearAllDisplayElement();
		foreach (TElement item in elements)
		{
			TDisplay display = null;
			layoutGroup.AddDisplayElement(new ScrollRectGridLayoutGroup.DisplayElement
			{
				data = item,
				onShow = delegate
				{
					display = pool.Spawn(convertor.ToSetupData(item));
					display.SetDirty();
					return display.gameObject;
				},
				onHide = delegate
				{
					pool.Despawn(display);
				}
			});
		}
		layoutGroup.RefreshLayout();
	}

	public static bool SequenceEqual<T>(List<T> list, List<T> list2)
	{
		if (list.IsNullOrEmpty() && list2.IsNullOrEmpty())
		{
			return true;
		}
		if (list.Count != list2.Count)
		{
			return false;
		}
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] != null || list2[i] != null)
			{
				if (list[i] == null || list2[i] == null)
				{
					return false;
				}
				if (!list[i].Equals(list2[i]))
				{
					return false;
				}
			}
		}
		return true;
	}
}
