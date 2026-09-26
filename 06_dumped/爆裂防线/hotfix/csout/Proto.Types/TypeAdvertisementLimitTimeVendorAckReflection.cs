using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeAdvertisementLimitTimeVendorAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeAdvertisementLimitTimeVendorAckReflection()
	{
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Expected O, but got Unknown
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cip0eXBlLkFkdmVydGlzZW1lbnRMaW1pdFRpbWVWZW5kb3JBY2sucHJvdG8S" + "BXR5cGVzGhx0eXBlLkFkdmVydGlzZW1lbnRJbmZvLnByb3RvGhZ0eXBlLkF3" + "YXJkRGV0YWlsLnByb3RvGhV0eXBlLkRhdGFDaGFuZ2UucHJvdG8aInR5cGUu" + "TGltaXRUaW1lUm91bGV0dGVEZXRhaWwucHJvdG8i4QEKH0FkdmVydGlzZW1l" + "bnRMaW1pdFRpbWVWZW5kb3JBY2sSMwoRQWR2ZXJ0aXNlbWVudEluZm8YASAB" + "KAsyGC50eXBlcy5BZHZlcnRpc2VtZW50SW5mbxI7ChNMaW1pdFRpbWVWZW5k" + "b3JMaXN0GAIgAygLMh4udHlwZXMuTGltaXRUaW1lUm91bGV0dGVEZXRhaWwS" + "JQoJQXdhcmRMaXN0GAMgAygLMhIudHlwZXMuQXdhcmREZXRhaWwSJQoKRGF0" + "YUNoYW5nZRgEIAEoCzIRLnR5cGVzLkRhdGFDaGFuZ2VCLloJLi87cHJvdG9j" + "qgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			TypeAdvertisementInfoReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeLimitTimeRouletteDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(AdvertisementLimitTimeVendorAck), (MessageParser)(object)AdvertisementLimitTimeVendorAck.Parser, new string[4] { "AdvertisementInfo", "LimitTimeVendorList", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
