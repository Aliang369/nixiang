using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEFriendActionCollectStateReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEFriendActionCollectStateReflection()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiRlbnVtLkVGcmllbmRBY3Rpb25Db2xsZWN0U3RhdGUucHJvdG8SGUVGcmll" + "bmRBY3Rpb25Db2xsZWN0U3RhdGUqUQoZRUZyaWVuZEFjdGlvbkNvbGxlY3RT" + "dGF0ZRIMCghOb3RFeGlzdBAAEgsKB1BlbmRpbmcQARINCglDb2xsZWN0ZWQQ" + "AhIKCgZDYW5ub3QQA0IuWgkuLztwcm90b2OqAgtQcm90by5FbnVtc8oCElpi" + "Z2FtZVxQcm90b1xFbnVtc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EFriendActionCollectState) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
