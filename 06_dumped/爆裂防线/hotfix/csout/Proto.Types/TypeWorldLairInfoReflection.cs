using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeWorldLairInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWorldLairInfoReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chh0eXBlLldvcmxkTGFpckluZm8ucHJvdG8SBXR5cGVzGhx0eXBlLldvcmxk" + "TGFpckJhc2VJbmZvLnByb3RvGhp0eXBlLldvcmxkTGFpckRldGFpbC5wcm90" + "byJyCg1Xb3JsZExhaXJJbmZvEjMKEVdvcmxkTGFpckJhc2VJbmZvGAEgASgL" + "MhgudHlwZXMuV29ybGRMYWlyQmFzZUluZm8SLAoMU2VsZkxhaXJJbmZvGAIg" + "ASgLMhYudHlwZXMuV29ybGRMYWlyRGV0YWlsQi5aCS4vO3Byb3RvY6oCC1By" + "b3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeWorldLairBaseInfoReflection.Descriptor,
			TypeWorldLairDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WorldLairInfo), (MessageParser)(object)WorldLairInfo.Parser, new string[2] { "WorldLairBaseInfo", "SelfLairInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
