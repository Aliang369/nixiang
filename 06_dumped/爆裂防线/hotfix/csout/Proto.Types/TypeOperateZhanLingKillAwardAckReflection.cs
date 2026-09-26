using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateZhanLingKillAwardAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateZhanLingKillAwardAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiZ0eXBlLk9wZXJhdGVaaGFuTGluZ0tpbGxBd2FyZEFjay5wcm90bxIFdHlw" + "ZXMaFnR5cGUuQXdhcmREZXRhaWwucHJvdG8aFXR5cGUuRGF0YUNoYW5nZS5w" + "cm90bxoddHlwZS5aaGFuTGluZ0tpbGxEZXRhaWwucHJvdG8ioAEKG09wZXJh" + "dGVaaGFuTGluZ0tpbGxBd2FyZEFjaxIzChBaaGFuTGluZ0tpbGxMaXN0GAEg" + "AygLMhkudHlwZXMuWmhhbkxpbmdLaWxsRGV0YWlsEiUKCUF3YXJkTGlzdBgC" + "IAMoCzISLnR5cGVzLkF3YXJkRGV0YWlsEiUKCkRhdGFDaGFuZ2UYAyABKAsy" + "ES50eXBlcy5EYXRhQ2hhbmdlQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVz" + "ygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeZhanLingKillDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateZhanLingKillAwardAck), (MessageParser)(object)OperateZhanLingKillAwardAck.Parser, new string[3] { "ZhanLingKillList", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
