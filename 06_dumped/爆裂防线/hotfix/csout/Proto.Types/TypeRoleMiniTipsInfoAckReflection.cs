using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeRoleMiniTipsInfoAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeRoleMiniTipsInfoAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLlJvbGVNaW5pVGlwc0luZm9BY2sucHJvdG8SBXR5cGVzGht0eXBl" + "LlJvbGVNaW5pVGlwc0luZm8ucHJvdG8iRAoTUm9sZU1pbmlUaXBzSW5mb0Fj" + "axItCgxNaW5pVGlwc0luZm8YASABKAsyFy50eXBlcy5Sb2xlTWluaVRpcHNJ" + "bmZvQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3Rv" + "XFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeRoleMiniTipsInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(RoleMiniTipsInfoAck), (MessageParser)(object)RoleMiniTipsInfoAck.Parser, new string[1] { "MiniTipsInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
