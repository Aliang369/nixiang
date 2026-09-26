using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeModHandbookInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeModHandbookInfoReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chp0eXBlLk1vZEhhbmRib29rSW5mby5wcm90bxIFdHlwZXMaHHR5cGUuTW9k" + "SGFuZGJvb2tEZXRhaWwucHJvdG8aHHR5cGUuTW9kSGFuZGJvb2tUYXJnZXQu" + "cHJvdG8aIHR5cGUuTW9kSGFuZGJvb2tUeXBlVGFyZ2V0LnByb3RvIqABCg9N" + "b2RIYW5kYm9va0luZm8SKQoHTW9kTGlzdBgBIAMoCzIYLnR5cGVzLk1vZEhh" + "bmRib29rRGV0YWlsEjQKDlR5cGVUYXJnZXRMaXN0GAIgAygLMhwudHlwZXMu" + "TW9kSGFuZGJvb2tUeXBlVGFyZ2V0EiwKClRhcmdldEluZm8YAyABKAsyGC50" + "eXBlcy5Nb2RIYW5kYm9va1RhcmdldEIuWgkuLztwcm90b2OqAgtQcm90by5U" + "eXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeModHandbookDetailReflection.Descriptor,
			TypeModHandbookTargetReflection.Descriptor,
			TypeModHandbookTypeTargetReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ModHandbookInfo), (MessageParser)(object)ModHandbookInfo.Parser, new string[3] { "ModList", "TypeTargetList", "TargetInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
