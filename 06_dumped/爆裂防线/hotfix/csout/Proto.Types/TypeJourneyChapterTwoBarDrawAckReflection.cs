using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeJourneyChapterTwoBarDrawAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeJourneyChapterTwoBarDrawAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiZ0eXBlLkpvdXJuZXlDaGFwdGVyVHdvQmFyRHJhd0Fjay5wcm90bxIFdHlw" + "ZXMaFnR5cGUuQXdhcmREZXRhaWwucHJvdG8aFXR5cGUuRGF0YUNoYW5nZS5w" + "cm90bxoWdHlwZS5Kb3VybmV5SW5mby5wcm90byKUAQobSm91cm5leUNoYXB0" + "ZXJUd29CYXJEcmF3QWNrEicKC0pvdXJuZXlJbmZvGAEgASgLMhIudHlwZXMu" + "Sm91cm5leUluZm8SJQoJQXdhcmRMaXN0GAIgAygLMhIudHlwZXMuQXdhcmRE" + "ZXRhaWwSJQoKRGF0YUNoYW5nZRgDIAEoCzIRLnR5cGVzLkRhdGFDaGFuZ2VC" + "LloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlw" + "ZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeJourneyInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(JourneyChapterTwoBarDrawAck), (MessageParser)(object)JourneyChapterTwoBarDrawAck.Parser, new string[3] { "JourneyInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
