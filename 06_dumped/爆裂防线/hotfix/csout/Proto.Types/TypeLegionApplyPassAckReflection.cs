using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLegionApplyPassAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionApplyPassAckReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch10eXBlLkxlZ2lvbkFwcGx5UGFzc0Fjay5wcm90bxIFdHlwZXMaGXR5cGUu" + "TGVnaW9uQmFzZUluZm8ucHJvdG8iVQoSTGVnaW9uQXBwbHlQYXNzQWNrEhAK" + "CFBsYXllcklkGAEgASgFEi0KDkxlZ2lvbkJhc2VJbmZvGAIgASgLMhUudHlw" + "ZXMuTGVnaW9uQmFzZUluZm9CLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPK" + "AhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeLegionBaseInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionApplyPassAck), (MessageParser)(object)LegionApplyPassAck.Parser, new string[2] { "PlayerId", "LegionBaseInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
