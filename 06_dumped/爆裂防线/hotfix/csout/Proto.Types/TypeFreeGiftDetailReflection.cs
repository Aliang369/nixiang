using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeFreeGiftDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeFreeGiftDetailReflection()
	{
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Expected O, but got Unknown
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chl0eXBlLkZyZWVHaWZ0RGV0YWlsLnByb3RvEgV0eXBlcxoVZW51bS5FQm9v" + "bFN0YXRlLnByb3RvGhdlbnVtLkVSZWZyZXNoVHlwZS5wcm90bxoWdHlwZS5B" + "d2FyZERldGFpbC5wcm90byKdAQoORnJlZUdpZnREZXRhaWwSEAoIRnJlZVR5" + "cGUYASABKAUSLwoLUmVmcmVzaFR5cGUYAiABKA4yGi5FUmVmcmVzaFR5cGUu" + "RVJlZnJlc2hUeXBlEiEKBUF3YXJkGAMgASgLMhIudHlwZXMuQXdhcmREZXRh" + "aWwSJQoFU3RhdGUYBCABKA4yFi5FQm9vbFN0YXRlLkVCb29sU3RhdGVCLloJ" + "Li87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNi" + "BnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			EnumEBoolStateReflection.Descriptor,
			EnumERefreshTypeReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(FreeGiftDetail), (MessageParser)(object)FreeGiftDetail.Parser, new string[4] { "FreeType", "RefreshType", "Award", "State" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
