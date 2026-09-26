using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeHandbookModTypeTargetAwardAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeHandbookModTypeTargetAwardAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cih0eXBlLkhhbmRib29rTW9kVHlwZVRhcmdldEF3YXJkQWNrLnByb3RvEgV0" + "eXBlcxoWdHlwZS5Bd2FyZERldGFpbC5wcm90bxoVdHlwZS5EYXRhQ2hhbmdl" + "LnByb3RvGhd0eXBlLkhhbmRib29rSW5mby5wcm90byKYAQodSGFuZGJvb2tN" + "b2RUeXBlVGFyZ2V0QXdhcmRBY2sSKQoMSGFuZGJvb2tJbmZvGAEgASgLMhMu" + "dHlwZXMuSGFuZGJvb2tJbmZvEiUKCUF3YXJkTGlzdBgCIAMoCzISLnR5cGVz" + "LkF3YXJkRGV0YWlsEiUKCkRhdGFDaGFuZ2UYAyABKAsyES50eXBlcy5EYXRh" + "Q2hhbmdlQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFBy" + "b3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeHandbookInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(HandbookModTypeTargetAwardAck), (MessageParser)(object)HandbookModTypeTargetAwardAck.Parser, new string[3] { "HandbookInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
