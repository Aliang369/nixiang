using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeFeatureEndlessHordeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeFeatureEndlessHordeReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLkZlYXR1cmVFbmRsZXNzSG9yZGUucHJvdG8SBXR5cGVzGht0eXBl" + "LkVuZGxlc3NIb3JkZUluZm8ucHJvdG8iWgoTRmVhdHVyZUVuZGxlc3NIb3Jk" + "ZRIxChBFbmRsZXNzSG9yZGVJbmZvGAEgASgLMhcudHlwZXMuRW5kbGVzc0hv" + "cmRlSW5mbxIQCghTZWxmUmFuaxgCIAEoBUIuWgkuLztwcm90b2OqAgtQcm90" + "by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeEndlessHordeInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(FeatureEndlessHorde), (MessageParser)(object)FeatureEndlessHorde.Parser, new string[2] { "EndlessHordeInfo", "SelfRank" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
