using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeBubbleGiftReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeBubbleGiftReflection()
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Expected O, but got Unknown
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChV0eXBlLkJ1YmJsZUdpZnQucHJvdG8SBXR5cGVzGhVlbnVtLkVCb29sU3Rh" + "dGUucHJvdG8aIHR5cGUuQnViYmxlR2lmdEdvb2RzRGV0YWlsLnByb3RvIrAB" + "CgpCdWJibGVHaWZ0EiYKBklzT3BlbhgBIAEoDjIWLkVCb29sU3RhdGUuRUJv" + "b2xTdGF0ZRISCgpEYWlseUNvdW50GAIgASgFEhEKCVJhbmRDb3VudBgDIAEo" + "BRIRCglXYWl0U3RhcnQYBCABKAMSDwoHV2FpdEVuZBgFIAEoAxIvCglHb29k" + "c0l0ZW0YBiABKAsyHC50eXBlcy5CdWJibGVHaWZ0R29vZHNEZXRhaWxCLloJ" + "Li87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNi" + "BnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumEBoolStateReflection.Descriptor,
			TypeBubbleGiftGoodsDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(BubbleGift), (MessageParser)(object)BubbleGift.Parser, new string[6] { "IsOpen", "DailyCount", "RandCount", "WaitStart", "WaitEnd", "GoodsItem" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
