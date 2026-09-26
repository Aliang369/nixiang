using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateHolidayTaskBuyLevelAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateHolidayTaskBuyLevelAckReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cih0eXBlLk9wZXJhdGVIb2xpZGF5VGFza0J1eUxldmVsQWNrLnByb3RvEgV0" + "eXBlcxoVdHlwZS5EYXRhQ2hhbmdlLnByb3RvGhx0eXBlLkhvbGlkYXlUYXNr" + "RGV0YWlsLnByb3RvInkKHU9wZXJhdGVIb2xpZGF5VGFza0J1eUxldmVsQWNr" + "EjEKD0hvbGlkYXlUYXNrTGlzdBgBIAMoCzIYLnR5cGVzLkhvbGlkYXlUYXNr" + "RGV0YWlsEiUKCkRhdGFDaGFuZ2UYAiABKAsyES50eXBlcy5EYXRhQ2hhbmdl" + "Qi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5" + "cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDataChangeReflection.Descriptor,
			TypeHolidayTaskDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateHolidayTaskBuyLevelAck), (MessageParser)(object)OperateHolidayTaskBuyLevelAck.Parser, new string[2] { "HolidayTaskList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
