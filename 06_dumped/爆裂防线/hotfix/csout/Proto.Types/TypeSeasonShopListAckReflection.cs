using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeSeasonShopListAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSeasonShopListAckReflection()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLlNlYXNvblNob3BMaXN0QWNrLnByb3RvEgV0eXBlcxoadHlwZS5T" + "ZWFzb25TaG9wR29vZHMucHJvdG8iYQoRU2Vhc29uU2hvcExpc3RBY2sSDQoF" + "TWVyaXQYASABKAUSEgoKU2Vhc29uQ29pbhgCIAEoBRIpCglHb29kc0xpc3QY" + "AyADKAsyFi50eXBlcy5TZWFzb25TaG9wR29vZHNCLloJLi87cHJvdG9jqgIL" + "UHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeSeasonShopGoodsReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SeasonShopListAck), (MessageParser)(object)SeasonShopListAck.Parser, new string[3] { "Merit", "SeasonCoin", "GoodsList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
