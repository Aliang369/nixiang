using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeFeatureWorldLairReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeFeatureWorldLairReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cht0eXBlLkZlYXR1cmVXb3JsZExhaXIucHJvdG8SBXR5cGVzGhh0eXBlLldv" + "cmxkTGFpckluZm8ucHJvdG8iUgoQRmVhdHVyZVdvcmxkTGFpchIrCg1Xb3Js" + "ZExhaXJJbmZvGAEgASgLMhQudHlwZXMuV29ybGRMYWlySW5mbxIRCglIZWxw" + "Q291bnQYAiABKAVCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdh" + "bWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeWorldLairInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(FeatureWorldLair), (MessageParser)(object)FeatureWorldLair.Parser, new string[2] { "WorldLairInfo", "HelpCount" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
