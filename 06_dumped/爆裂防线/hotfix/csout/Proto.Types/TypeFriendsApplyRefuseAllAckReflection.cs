using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeFriendsApplyRefuseAllAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeFriendsApplyRefuseAllAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiN0eXBlLkZyaWVuZHNBcHBseVJlZnVzZUFsbEFjay5wcm90bxIFdHlwZXMa" + "HXR5cGUuRnJpZW5kc0FwcGx5RGV0YWlsLnByb3RvIkgKGEZyaWVuZHNBcHBs" + "eVJlZnVzZUFsbEFjaxIsCglBcHBseUxpc3QYASADKAsyGS50eXBlcy5Gcmll" + "bmRzQXBwbHlEZXRhaWxCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJa" + "YmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeFriendsApplyDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(FriendsApplyRefuseAllAck), (MessageParser)(object)FriendsApplyRefuseAllAck.Parser, new string[1] { "ApplyList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
