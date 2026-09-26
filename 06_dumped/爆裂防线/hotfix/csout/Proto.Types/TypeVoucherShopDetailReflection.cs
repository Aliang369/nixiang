using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeVoucherShopDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeVoucherShopDetailReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLlZvdWNoZXJTaG9wRGV0YWlsLnByb3RvEgV0eXBlcxoTZW51bS5F" + "Q0NZVHlwZS5wcm90byJlChFWb3VjaGVyU2hvcERldGFpbBIPCgdHb29kc0lk" + "GAEgASgFEh8KA0NjeRgCIAEoDjISLkVDQ1lUeXBlLkVDQ1lUeXBlEg0KBVBy" + "aWNlGAMgASgCEg8KB1ZvdWNoZXIYBCABKAVCLloJLi87cHJvdG9jqgILUHJv" + "dG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumECCYTypeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(VoucherShopDetail), (MessageParser)(object)VoucherShopDetail.Parser, new string[4] { "GoodsId", "Ccy", "Price", "Voucher" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
