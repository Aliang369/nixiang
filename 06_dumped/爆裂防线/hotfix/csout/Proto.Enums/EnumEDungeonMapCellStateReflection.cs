using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEDungeonMapCellStateReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEDungeonMapCellStateReflection()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch9lbnVtLkVEdW5nZW9uTWFwQ2VsbFN0YXRlLnByb3RvEhRFRHVuZ2Vvbk1h" + "cENlbGxTdGF0ZSpqChRFRHVuZ2Vvbk1hcENlbGxTdGF0ZRINCglJbnZpc2li" + "bGUQABILCgdWaXNpYmxlEAESDAoIQWN0aXZhdGUQAhIMCghMZWZ0T3BlbhAD" + "Eg0KCVJpZ2h0T3BlbhAEEgsKB0FsbE9wZW4QBUIuWgkuLztwcm90b2OqAgtQ" + "cm90by5FbnVtc8oCElpiZ2FtZVxQcm90b1xFbnVtc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EDungeonMapCellState) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
