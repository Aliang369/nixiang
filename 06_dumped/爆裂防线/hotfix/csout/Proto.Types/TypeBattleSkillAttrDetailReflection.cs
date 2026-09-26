using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeBattleSkillAttrDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeBattleSkillAttrDetailReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLkJhdHRsZVNraWxsQXR0ckRldGFpbC5wcm90bxIFdHlwZXMaGWVu" + "dW0uRUF0dHJTa2lsbFR5cGUucHJvdG8iVAoVQmF0dGxlU2tpbGxBdHRyRGV0" + "YWlsEiwKBFR5cGUYASABKA4yHi5FQXR0clNraWxsVHlwZS5FQXR0clNraWxs" + "VHlwZRINCgVWYWx1ZRgCIAEoAkIuWgkuLztwcm90b2OqAgtQcm90by5UeXBl" + "c8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEAttrSkillTypeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(BattleSkillAttrDetail), (MessageParser)(object)BattleSkillAttrDetail.Parser, new string[2] { "Type", "Value" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
