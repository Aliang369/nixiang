using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeJourneyChapterThreeExchangeAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeJourneyChapterThreeExchangeAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cil0eXBlLkpvdXJuZXlDaGFwdGVyVGhyZWVFeGNoYW5nZUFjay5wcm90bxIF" + "dHlwZXMaFnR5cGUuQXdhcmREZXRhaWwucHJvdG8aFXR5cGUuRGF0YUNoYW5n" + "ZS5wcm90bxoWdHlwZS5Kb3VybmV5SW5mby5wcm90byKXAQoeSm91cm5leUNo" + "YXB0ZXJUaHJlZUV4Y2hhbmdlQWNrEicKC0pvdXJuZXlJbmZvGAEgASgLMhIu" + "dHlwZXMuSm91cm5leUluZm8SJQoJQXdhcmRMaXN0GAIgAygLMhIudHlwZXMu" + "QXdhcmREZXRhaWwSJQoKRGF0YUNoYW5nZRgDIAEoCzIRLnR5cGVzLkRhdGFD" + "aGFuZ2VCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJv" + "dG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeJourneyInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(JourneyChapterThreeExchangeAck), (MessageParser)(object)JourneyChapterThreeExchangeAck.Parser, new string[3] { "JourneyInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
