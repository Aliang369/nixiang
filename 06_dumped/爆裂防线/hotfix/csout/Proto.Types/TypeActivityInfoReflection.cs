using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeActivityInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeActivityInfoReflection()
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Expected O, but got Unknown
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chd0eXBlLkFjdGl2aXR5SW5mby5wcm90bxIFdHlwZXMaFXR5cGUuQnViYmxl" + "R2lmdC5wcm90bxoUdHlwZS5EYWlseVNpZ24ucHJvdG8aFnR5cGUuTm92aWNl" + "TG9naW4ucHJvdG8aE3R5cGUuVGlsaUJhbmsucHJvdG8ipgEKDEFjdGl2aXR5" + "SW5mbxInCgtOb3ZpY2VMb2dpbhgBIAEoCzISLnR5cGVzLk5vdmljZUxvZ2lu" + "EiMKCURhaWx5U2lnbhgCIAEoCzIQLnR5cGVzLkRhaWx5U2lnbhIhCghUaWxp" + "QmFuaxgDIAEoCzIPLnR5cGVzLlRpbGlCYW5rEiUKCkJ1YmJsZUdpZnQYBCAB" + "KAsyES50eXBlcy5CdWJibGVHaWZ0Qi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5" + "cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			TypeBubbleGiftReflection.Descriptor,
			TypeDailySignReflection.Descriptor,
			TypeNoviceLoginReflection.Descriptor,
			TypeTiliBankReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ActivityInfo), (MessageParser)(object)ActivityInfo.Parser, new string[4] { "NoviceLogin", "DailySign", "TiliBank", "BubbleGift" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
