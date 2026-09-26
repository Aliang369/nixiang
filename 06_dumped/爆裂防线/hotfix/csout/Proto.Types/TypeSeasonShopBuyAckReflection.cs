using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeSeasonShopBuyAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSeasonShopBuyAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cht0eXBlLlNlYXNvblNob3BCdXlBY2sucHJvdG8SBXR5cGVzImYKEFNlYXNv" + "blNob3BCdXlBY2sSDgoGU2hvcElkGAEgASgFEg0KBUNvdW50GAIgASgFEg0K" + "BU1lcml0GAMgASgFEhEKCU1vbmV5VHlwZRgEIAEoBRIRCglNb25leUxlZnQY" + "BSABKAVCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJv" + "dG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SeasonShopBuyAck), (MessageParser)(object)SeasonShopBuyAck.Parser, new string[5] { "ShopId", "Count", "Merit", "MoneyType", "MoneyLeft" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
