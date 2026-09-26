using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeAdvertisementChapterTwoBarAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeAdvertisementChapterTwoBarAckReflection()
	{
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Expected O, but got Unknown
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cih0eXBlLkFkdmVydGlzZW1lbnRDaGFwdGVyVHdvQmFyQWNrLnByb3RvEgV0" + "eXBlcxocdHlwZS5BZHZlcnRpc2VtZW50SW5mby5wcm90bxoWdHlwZS5Bd2Fy" + "ZERldGFpbC5wcm90bxoVdHlwZS5EYXRhQ2hhbmdlLnByb3RvGhZ0eXBlLkpv" + "dXJuZXlJbmZvLnByb3RvIssBCh1BZHZlcnRpc2VtZW50Q2hhcHRlclR3b0Jh" + "ckFjaxIzChFBZHZlcnRpc2VtZW50SW5mbxgBIAEoCzIYLnR5cGVzLkFkdmVy" + "dGlzZW1lbnRJbmZvEicKC0pvdXJuZXlJbmZvGAIgASgLMhIudHlwZXMuSm91" + "cm5leUluZm8SJQoJQXdhcmRMaXN0GAMgAygLMhIudHlwZXMuQXdhcmREZXRh" + "aWwSJQoKRGF0YUNoYW5nZRgEIAEoCzIRLnR5cGVzLkRhdGFDaGFuZ2VCLloJ" + "Li87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNi" + "BnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			TypeAdvertisementInfoReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeJourneyInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(AdvertisementChapterTwoBarAck), (MessageParser)(object)AdvertisementChapterTwoBarAck.Parser, new string[4] { "AdvertisementInfo", "JourneyInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
