using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypePvpModInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypePvpModInfoReflection()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChV0eXBlLlB2cE1vZEluZm8ucHJvdG8SBXR5cGVzGhZ0eXBlLlNraWxsRGV0" + "YWlsLnByb3RvIlgKClB2cE1vZEluZm8SJQoJU2tpbGxMaXN0GAEgAygLMhIu" + "dHlwZXMuU2tpbGxEZXRhaWwSDwoHTW9kTGlzdBgCIAMoBRISCgpTZWxlY3RM" + "aXN0GAMgAygFQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1l" + "XFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeSkillDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(PvpModInfo), (MessageParser)(object)PvpModInfo.Parser, new string[3] { "SkillList", "ModList", "SelectList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
