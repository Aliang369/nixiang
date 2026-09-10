using System;

namespace OdinEx;

public class TableColumnLabel : Attribute
{
	public string Text;

	public TableColumnLabel(string text)
	{
		Text = text;
	}
}
