using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEItemTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEItemTypeReflection()
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChRlbnVtLkVJdGVtVHlwZS5wcm90bxIJRUl0ZW1UeXBlKoMCCglFSXRlbVR5" + "cGUSDgoKRW1wdHlWYWx1ZRAAEhAKDFJvbGVOYW1lQ2FyZBABEgwKCEVxdWlw" + "RXhwEAISDAoIU2tpbGxFeHAQAxIMCghNYXRlcmlhbBAEEg4KCkRlZmVuc2VF" + "eHAQBRIPCgtNb25zdGVySXRlbRAGEgsKB0dlbUl0ZW0QBxIVChFIZXJvQXdh" + "a2VuVXBncmFkZRAIEhUKEUhlcm9Bd2FrZW5BZHZhbmNlEAkSCgoGQm94S2V5" + "EAoSDgoKTWVjaGFTa2lsbBALEg4KCk1lY2hhTGV2ZWwQDBIOCgpIZXJvRm9s" + "bG93EA0SEgoORmFjaWxpdHlDcmVhdGUQDkIuWgkuLztwcm90b2OqAgtQcm90" + "by5FbnVtc8oCElpiZ2FtZVxQcm90b1xFbnVtc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EItemType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
