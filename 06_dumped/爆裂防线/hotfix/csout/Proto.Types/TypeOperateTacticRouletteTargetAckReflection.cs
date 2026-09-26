using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateTacticRouletteTargetAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateTacticRouletteTargetAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cil0eXBlLk9wZXJhdGVUYWN0aWNSb3VsZXR0ZVRhcmdldEFjay5wcm90bxIF" + "dHlwZXMaFnR5cGUuQXdhcmREZXRhaWwucHJvdG8aFXR5cGUuRGF0YUNoYW5n" + "ZS5wcm90bxofdHlwZS5UYWN0aWNSb3VsZXR0ZURldGFpbC5wcm90byKnAQoe" + "T3BlcmF0ZVRhY3RpY1JvdWxldHRlVGFyZ2V0QWNrEjcKElRhY3RpY1JvdWxl" + "dHRlTGlzdBgBIAMoCzIbLnR5cGVzLlRhY3RpY1JvdWxldHRlRGV0YWlsEiUK" + "CUF3YXJkTGlzdBgCIAMoCzISLnR5cGVzLkF3YXJkRGV0YWlsEiUKCkRhdGFD" + "aGFuZ2UYAyABKAsyES50eXBlcy5EYXRhQ2hhbmdlQi5aCS4vO3Byb3RvY6oC" + "C1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeTacticRouletteDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateTacticRouletteTargetAck), (MessageParser)(object)OperateTacticRouletteTargetAck.Parser, new string[3] { "TacticRouletteList", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
