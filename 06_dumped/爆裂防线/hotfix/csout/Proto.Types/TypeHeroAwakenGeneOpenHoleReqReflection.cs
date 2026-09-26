using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeHeroAwakenGeneOpenHoleReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeHeroAwakenGeneOpenHoleReqReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiR0eXBlLkhlcm9Bd2FrZW5HZW5lT3BlbkhvbGVSZXEucHJvdG8SBXR5cGVz" + "GhVlbnVtLkVTa2lsbFR5cGUucHJvdG8iRgoZSGVyb0F3YWtlbkdlbmVPcGVu" + "SG9sZVJlcRIpCglTa2lsbFR5cGUYASABKA4yFi5FU2tpbGxUeXBlLkVTa2ls" + "bFR5cGVCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJv" + "dG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumESkillTypeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(HeroAwakenGeneOpenHoleReq), (MessageParser)(object)HeroAwakenGeneOpenHoleReq.Parser, new string[1] { "SkillType" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
