using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateAdsDirectAwardAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateAdsDirectAwardAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiN0eXBlLk9wZXJhdGVBZHNEaXJlY3RBd2FyZEFjay5wcm90bxIFdHlwZXMa" + "GnR5cGUuQWRzRGlyZWN0RGV0YWlsLnByb3RvGhZ0eXBlLkF3YXJkRGV0YWls" + "LnByb3RvGhV0eXBlLkRhdGFDaGFuZ2UucHJvdG8ilwEKGE9wZXJhdGVBZHNE" + "aXJlY3RBd2FyZEFjaxItCg1BZHNEaXJlY3RMaXN0GAEgAygLMhYudHlwZXMu" + "QWRzRGlyZWN0RGV0YWlsEiUKCUF3YXJkTGlzdBgCIAMoCzISLnR5cGVzLkF3" + "YXJkRGV0YWlsEiUKCkRhdGFDaGFuZ2UYAyABKAsyES50eXBlcy5EYXRhQ2hh" + "bmdlQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3Rv" + "XFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAdsDirectDetailReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateAdsDirectAwardAck), (MessageParser)(object)OperateAdsDirectAwardAck.Parser, new string[3] { "AdsDirectList", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
