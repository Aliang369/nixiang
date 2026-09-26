using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeJourneyChapterTwoZhanLingAwardReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeJourneyChapterTwoZhanLingAwardReqReflection()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cix0eXBlLkpvdXJuZXlDaGFwdGVyVHdvWmhhbkxpbmdBd2FyZFJlcS5wcm90" + "bxIFdHlwZXMiMgohSm91cm5leUNoYXB0ZXJUd29aaGFuTGluZ0F3YXJkUmVx" + "Eg0KBUxldmVsGAEgASgFQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygIS" + "WmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(JourneyChapterTwoZhanLingAwardReq), (MessageParser)(object)JourneyChapterTwoZhanLingAwardReq.Parser, new string[1] { "Level" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
