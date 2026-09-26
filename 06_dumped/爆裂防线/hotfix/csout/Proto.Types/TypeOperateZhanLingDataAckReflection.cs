using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateZhanLingDataAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateZhanLingDataAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiF0eXBlLk9wZXJhdGVaaGFuTGluZ0RhdGFBY2sucHJvdG8SBXR5cGVzGh10" + "eXBlLlpoYW5MaW5nS2lsbERldGFpbC5wcm90byJNChZPcGVyYXRlWmhhbkxp" + "bmdEYXRhQWNrEjMKEFpoYW5MaW5nS2lsbExpc3QYASADKAsyGS50eXBlcy5a" + "aGFuTGluZ0tpbGxEZXRhaWxCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPK" + "AhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeZhanLingKillDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateZhanLingDataAck), (MessageParser)(object)OperateZhanLingDataAck.Parser, new string[1] { "ZhanLingKillList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
