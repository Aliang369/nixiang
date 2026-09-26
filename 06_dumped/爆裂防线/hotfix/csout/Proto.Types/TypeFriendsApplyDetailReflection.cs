using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeFriendsApplyDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeFriendsApplyDetailReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch10eXBlLkZyaWVuZHNBcHBseURldGFpbC5wcm90bxIFdHlwZXMaF3R5cGUu" + "Um9sZUJhc2VJbmZvLnByb3RvIlAKEkZyaWVuZHNBcHBseURldGFpbBIpCgxS" + "b2xlQmFzZUluZm8YASABKAsyEy50eXBlcy5Sb2xlQmFzZUluZm8SDwoHQXBw" + "bHlUbRgCIAEoA0IuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2Ft" + "ZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeRoleBaseInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(FriendsApplyDetail), (MessageParser)(object)FriendsApplyDetail.Parser, new string[2] { "RoleBaseInfo", "ApplyTm" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
