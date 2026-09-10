using System;
using System.Diagnostics;
using UnityEngine;

namespace OdinEx;

[AttributeUsage(AttributeTargets.All)]
[Conditional("UNITY_EDITOR")]
public class ExTableListAttribute : Attribute
{
	public bool AlwaysExpanded;

	public int CellPadding = 2;

	public int DefaultMinColumnWidth = 40;

	public bool DrawElementControler;

	public bool DrawScrollView = true;

	public bool hideColumnHeaders;

	public bool HideToolbar;

	public bool IsReadOnly;

	public int MaxScrollViewHeight;

	public int MinScrollViewHeight = 350;

	public int NumberOfItemsPerPage;

	public bool ShowIndexLabels;

	[SerializeField]
	[HideInInspector]
	private bool showPaging;

	[SerializeField]
	[HideInInspector]
	private bool showPagingHasValue;

	public int ScrollViewHeight
	{
		get
		{
			return Math.Min(MinScrollViewHeight, MaxScrollViewHeight);
		}
		set
		{
			MinScrollViewHeight = (MaxScrollViewHeight = value);
		}
	}

	public bool ShowPaging
	{
		get
		{
			return showPaging;
		}
		set
		{
			showPaging = value;
			showPagingHasValue = true;
		}
	}

	public bool ShowPagingHasValue => showPagingHasValue;
}
