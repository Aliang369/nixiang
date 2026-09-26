using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeHeroAwakenGeneSetReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeHeroAwakenGeneSetReqReflection()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch90eXBlLkhlcm9Bd2FrZW5HZW5lU2V0UmVxLnByb3RvEgV0eXBlcxoVZW51" + "bS5FU2tpbGxUeXBlLnByb3RvImEKFEhlcm9Bd2FrZW5HZW5lU2V0UmVxEikK" + "CVNraWxsVHlwZRgBIAEoDjIWLkVTa2lsbFR5cGUuRVNraWxsVHlwZRINCgVJ" + "bmRleBgCIAEoBRIPCgdHZW5lVGlkGAMgASgFQi5aCS4vO3Byb3RvY6oCC1By" + "b3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumESkillTypeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(HeroAwakenGeneSetReq), (MessageParser)(object)HeroAwakenGeneSetReq.Parser, new string[3] { "SkillType", "Index", "GeneTid" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
