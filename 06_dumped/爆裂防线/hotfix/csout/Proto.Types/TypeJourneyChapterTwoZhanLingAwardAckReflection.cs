using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeJourneyChapterTwoZhanLingAwardAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeJourneyChapterTwoZhanLingAwardAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cix0eXBlLkpvdXJuZXlDaGFwdGVyVHdvWmhhbkxpbmdBd2FyZEFjay5wcm90" + "bxIFdHlwZXMaFnR5cGUuQXdhcmREZXRhaWwucHJvdG8aFXR5cGUuRGF0YUNo" + "YW5nZS5wcm90bxoWdHlwZS5Kb3VybmV5SW5mby5wcm90byKaAQohSm91cm5l" + "eUNoYXB0ZXJUd29aaGFuTGluZ0F3YXJkQWNrEicKC0pvdXJuZXlJbmZvGAEg" + "ASgLMhIudHlwZXMuSm91cm5leUluZm8SJQoJQXdhcmRMaXN0GAIgAygLMhIu" + "dHlwZXMuQXdhcmREZXRhaWwSJQoKRGF0YUNoYW5nZRgDIAEoCzIRLnR5cGVz" + "LkRhdGFDaGFuZ2VCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdh" + "bWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeJourneyInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(JourneyChapterTwoZhanLingAwardAck), (MessageParser)(object)JourneyChapterTwoZhanLingAwardAck.Parser, new string[3] { "JourneyInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
