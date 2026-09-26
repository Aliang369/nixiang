using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeGemComposePreviewAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeGemComposePreviewAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch90eXBlLkdlbUNvbXBvc2VQcmV2aWV3QWNrLnByb3RvEgV0eXBlcxoZdHlw" + "ZS5SZXNvdXJjZURldGFpbC5wcm90byJCChRHZW1Db21wb3NlUHJldmlld0Fj" + "axIqCgtQcmV2aWV3TGlzdBgBIAMoCzIVLnR5cGVzLlJlc291cmNlRGV0YWls" + "Qi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5" + "cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeResourceDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(GemComposePreviewAck), (MessageParser)(object)GemComposePreviewAck.Parser, new string[1] { "PreviewList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
