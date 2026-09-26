using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeFuncShopDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeFuncShopDetailReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chl0eXBlLkZ1bmNTaG9wRGV0YWlsLnByb3RvEgV0eXBlcxoTZW51bS5FQ0NZ" + "VHlwZS5wcm90byJmCg5GdW5jU2hvcERldGFpbBIPCgdHb29kc0lkGAEgASgF" + "Eh8KA0NjeRgCIAEoDjISLkVDQ1lUeXBlLkVDQ1lUeXBlEg0KBVByaWNlGAMg" + "ASgCEhMKC0JvdWdodENvdW50GAQgASgFQi5aCS4vO3Byb3RvY6oCC1Byb3Rv" + "LlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumECCYTypeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(FuncShopDetail), (MessageParser)(object)FuncShopDetail.Parser, new string[4] { "GoodsId", "Ccy", "Price", "BoughtCount" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
