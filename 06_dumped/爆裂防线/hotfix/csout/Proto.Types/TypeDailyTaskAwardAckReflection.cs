using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeDailyTaskAwardAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeDailyTaskAwardAckReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLkRhaWx5VGFza0F3YXJkQWNrLnByb3RvEgV0eXBlcxoWdHlwZS5B" + "d2FyZERldGFpbC5wcm90bxoYdHlwZS5EYWlseVRhc2tJbmZvLnByb3RvGhV0" + "eXBlLkRhdGFDaGFuZ2UucHJvdG8ijgEKEURhaWx5VGFza0F3YXJkQWNrEisK" + "DURhaWx5VGFza0luZm8YASABKAsyFC50eXBlcy5EYWlseVRhc2tJbmZvEiUK" + "CUF3YXJkTGlzdBgCIAMoCzISLnR5cGVzLkF3YXJkRGV0YWlsEiUKCkRhdGFD" + "aGFuZ2UYAyABKAsyES50eXBlcy5EYXRhQ2hhbmdlQi5aCS4vO3Byb3RvY6oC" + "C1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDailyTaskInfoReflection.Descriptor,
			TypeDataChangeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(DailyTaskAwardAck), (MessageParser)(object)DailyTaskAwardAck.Parser, new string[3] { "DailyTaskInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
