using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLegionLogListAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionLogListAckReflection()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cht0eXBlLkxlZ2lvbkxvZ0xpc3RBY2sucHJvdG8SBXR5cGVzGhR0eXBlLkxv" + "Z0RldGFpbC5wcm90byI1ChBMZWdpb25Mb2dMaXN0QWNrEiEKB0xvZ0xpc3QY" + "ASADKAsyEC50eXBlcy5Mb2dEZXRhaWxCLloJLi87cHJvdG9jqgILUHJvdG8u" + "VHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeLogDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionLogListAck), (MessageParser)(object)LegionLogListAck.Parser, new string[1] { "LogList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
