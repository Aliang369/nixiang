using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeBagVoucherCouponActiveAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeBagVoucherCouponActiveAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiR0eXBlLkJhZ1ZvdWNoZXJDb3Vwb25BY3RpdmVBY2sucHJvdG8SBXR5cGVz" + "GhV0eXBlLkRhdGFDaGFuZ2UucHJvdG8aGnR5cGUuVm91Y2hlclNob3BJbmZv" + "LnByb3RvInMKGUJhZ1ZvdWNoZXJDb3Vwb25BY3RpdmVBY2sSLwoPVm91Y2hl" + "clNob3BJbmZvGAEgASgLMhYudHlwZXMuVm91Y2hlclNob3BJbmZvEiUKCkRh" + "dGFDaGFuZ2UYAiABKAsyES50eXBlcy5EYXRhQ2hhbmdlQi5aCS4vO3Byb3Rv" + "Y6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDataChangeReflection.Descriptor,
			TypeVoucherShopInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(BagVoucherCouponActiveAck), (MessageParser)(object)BagVoucherCouponActiveAck.Parser, new string[2] { "VoucherShopInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
