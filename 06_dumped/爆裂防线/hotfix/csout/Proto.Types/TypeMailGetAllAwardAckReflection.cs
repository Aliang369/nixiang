using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeMailGetAllAwardAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeMailGetAllAwardAckReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch10eXBlLk1haWxHZXRBbGxBd2FyZEFjay5wcm90bxIFdHlwZXMaFnR5cGUu" + "QXdhcmREZXRhaWwucHJvdG8aFXR5cGUuRGF0YUNoYW5nZS5wcm90bxoVdHlw" + "ZS5NYWlsRGV0YWlsLnByb3RvIocBChJNYWlsR2V0QWxsQXdhcmRBY2sSIwoI" + "TWFpbExpc3QYASADKAsyES50eXBlcy5NYWlsRGV0YWlsEiUKCUF3YXJkTGlz" + "dBgCIAMoCzISLnR5cGVzLkF3YXJkRGV0YWlsEiUKCkRhdGFDaGFuZ2UYAyAB" + "KAsyES50eXBlcy5EYXRhQ2hhbmdlQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5" + "cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeMailDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(MailGetAllAwardAck), (MessageParser)(object)MailGetAllAwardAck.Parser, new string[3] { "MailList", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
