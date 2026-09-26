using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeLimitVipHeroRandDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLimitVipHeroRandDetailReflection()
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiF0eXBlLkxpbWl0VmlwSGVyb1JhbmREZXRhaWwucHJvdG8SBXR5cGVzGhdl" + "bnVtLkVDb21tb25TdGF0ZS5wcm90bxoWdHlwZS5Bd2FyZERldGFpbC5wcm90" + "byJ6ChZMaW1pdFZpcEhlcm9SYW5kRGV0YWlsEg0KBUluZGV4GAEgASgFEigK" + "BFJhcmUYAiABKA4yGi5FQ29tbW9uU3RhdGUuRUNvbW1vblN0YXRlEicKC0F3" + "YXJkRGV0YWlsGAMgASgLMhIudHlwZXMuQXdhcmREZXRhaWxCLloJLi87cHJv" + "dG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3Rv" + "Mw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumECommonStateReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LimitVipHeroRandDetail), (MessageParser)(object)LimitVipHeroRandDetail.Parser, new string[3] { "Index", "Rare", "AwardDetail" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
