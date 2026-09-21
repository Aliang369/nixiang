using System;
using UnityEngine;

namespace OdinEx;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class TextAreaPropAttribute : PropertyAttribute
{
	public readonly int boxWidth;

	public readonly int maxLines;

	public readonly int minLines;

	public bool richText;

	public readonly int deltaHeight;

	public TextAreaPropAttribute()
	{
		minLines = 3;
		maxLines = 3;
	}

	public TextAreaPropAttribute(int minLines, int maxLines, bool richText = false, int deltaHeight = 0)
	{
		this.minLines = minLines;
		this.maxLines = maxLines;
		this.richText = richText;
		this.deltaHeight = deltaHeight;
	}

	public TextAreaPropAttribute(int minLines, int maxLines, int boxWidth)
	{
		this.minLines = minLines;
		this.maxLines = maxLines;
		this.boxWidth = boxWidth;
		richText = false;
	}
}
