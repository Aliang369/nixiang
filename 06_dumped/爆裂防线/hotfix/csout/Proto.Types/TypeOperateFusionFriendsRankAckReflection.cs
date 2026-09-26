using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateFusionFriendsRankAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateFusionFriendsRankAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiZ0eXBlLk9wZXJhdGVGdXNpb25GcmllbmRzUmFua0Fjay5wcm90bxIFdHlw" + "ZXMaFXR5cGUuUmFua0RldGFpbC5wcm90byJCChtPcGVyYXRlRnVzaW9uRnJp" + "ZW5kc1JhbmtBY2sSIwoIUmFua0xpc3QYASADKAsyES50eXBlcy5SYW5rRGV0" + "YWlsQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3Rv" + "XFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeRankDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateFusionFriendsRankAck), (MessageParser)(object)OperateFusionFriendsRankAck.Parser, new string[1] { "RankList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
