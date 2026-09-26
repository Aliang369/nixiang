using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeMailReadAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeMailReadAckReflection()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChZ0eXBlLk1haWxSZWFkQWNrLnByb3RvEgV0eXBlcxoWdHlwZS5NYWlsQ29u" + "dGVudC5wcm90bxoVdHlwZS5NYWlsRGV0YWlsLnByb3RvIlMKC01haWxSZWFk" + "QWNrEh8KBE1haWwYASABKAsyES50eXBlcy5NYWlsRGV0YWlsEiMKB0NvbnRl" + "bnQYAiABKAsyEi50eXBlcy5NYWlsQ29udGVudEIuWgkuLztwcm90b2OqAgtQ" + "cm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeMailContentReflection.Descriptor,
			TypeMailDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(MailReadAck), (MessageParser)(object)MailReadAck.Parser, new string[2] { "Mail", "Content" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
