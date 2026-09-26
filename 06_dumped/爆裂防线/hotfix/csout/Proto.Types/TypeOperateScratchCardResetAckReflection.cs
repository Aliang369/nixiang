using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateScratchCardResetAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateScratchCardResetAckReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiV0eXBlLk9wZXJhdGVTY3JhdGNoQ2FyZFJlc2V0QWNrLnByb3RvEgV0eXBl" + "cxoVdHlwZS5EYXRhQ2hhbmdlLnByb3RvGhx0eXBlLlNjcmF0Y2hDYXJkRGV0" + "YWlsLnByb3RvInYKGk9wZXJhdGVTY3JhdGNoQ2FyZFJlc2V0QWNrEjEKD1Nj" + "cmF0Y2hDYXJkTGlzdBgBIAMoCzIYLnR5cGVzLlNjcmF0Y2hDYXJkRGV0YWls" + "EiUKCkRhdGFDaGFuZ2UYAiABKAsyES50eXBlcy5EYXRhQ2hhbmdlQi5aCS4v" + "O3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZw" + "cm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDataChangeReflection.Descriptor,
			TypeScratchCardDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateScratchCardResetAck), (MessageParser)(object)OperateScratchCardResetAck.Parser, new string[2] { "ScratchCardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
