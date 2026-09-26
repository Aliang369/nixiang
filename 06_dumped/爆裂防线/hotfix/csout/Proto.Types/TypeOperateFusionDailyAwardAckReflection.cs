using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateFusionDailyAwardAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateFusionDailyAwardAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiV0eXBlLk9wZXJhdGVGdXNpb25EYWlseUF3YXJkQWNrLnByb3RvEgV0eXBl" + "cxoWdHlwZS5Bd2FyZERldGFpbC5wcm90bxoVdHlwZS5EYXRhQ2hhbmdlLnBy" + "b3RvGhV0eXBlLkZ1c2lvbkluZm8ucHJvdG8ikQEKGk9wZXJhdGVGdXNpb25E" + "YWlseUF3YXJkQWNrEiUKCkZ1c2lvbkluZm8YASABKAsyES50eXBlcy5GdXNp" + "b25JbmZvEiUKCUF3YXJkTGlzdBgCIAMoCzISLnR5cGVzLkF3YXJkRGV0YWls" + "EiUKCkRhdGFDaGFuZ2UYAyABKAsyES50eXBlcy5EYXRhQ2hhbmdlQi5aCS4v" + "O3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZw" + "cm90bzM="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeFusionInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateFusionDailyAwardAck), (MessageParser)(object)OperateFusionDailyAwardAck.Parser, new string[3] { "FusionInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
