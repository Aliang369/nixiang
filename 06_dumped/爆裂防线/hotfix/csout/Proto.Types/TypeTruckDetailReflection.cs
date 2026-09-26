using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeTruckDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeTruckDetailReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChZ0eXBlLlRydWNrRGV0YWlsLnByb3RvEgV0eXBlcxofdHlwZS5UcnVja1Ry" + "YW5zcG9ydERldGFpbC5wcm90byJoCgtUcnVja0RldGFpbBIPCgdUcnVja0lk" + "GAEgASgFEjIKDVRyYW5zcG9ydEluZm8YAiABKAsyGy50eXBlcy5UcnVja1Ry" + "YW5zcG9ydERldGFpbBIUCgxSZWZyZXNoQ291bnQYAyABKAVCLloJLi87cHJv" + "dG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3Rv" + "Mw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeTruckTransportDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(TruckDetail), (MessageParser)(object)TruckDetail.Parser, new string[3] { "TruckId", "TransportInfo", "RefreshCount" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
