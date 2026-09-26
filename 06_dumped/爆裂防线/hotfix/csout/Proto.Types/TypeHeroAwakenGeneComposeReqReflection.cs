using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeHeroAwakenGeneComposeReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeHeroAwakenGeneComposeReqReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiN0eXBlLkhlcm9Bd2FrZW5HZW5lQ29tcG9zZVJlcS5wcm90bxIFdHlwZXMa" + "FWVudW0uRVNraWxsVHlwZS5wcm90byJmChhIZXJvQXdha2VuR2VuZUNvbXBv" + "c2VSZXESKQoJU2tpbGxUeXBlGAEgASgOMhYuRVNraWxsVHlwZS5FU2tpbGxU" + "eXBlEg0KBUluZGV4GAIgASgFEhAKCEdlbmVMaXN0GAMgAygFQi5aCS4vO3By" + "b3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90" + "bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumESkillTypeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(HeroAwakenGeneComposeReq), (MessageParser)(object)HeroAwakenGeneComposeReq.Parser, new string[3] { "SkillType", "Index", "GeneList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
