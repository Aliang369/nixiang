using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeMineWarOpenInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeMineWarOpenInfoReflection()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chp0eXBlLk1pbmVXYXJPcGVuSW5mby5wcm90bxIFdHlwZXMiVAoPTWluZVdh" + "ck9wZW5JbmZvEhMKC0Nyb3NzT3BlblRtGAEgASgDEhYKDk5leHRDcm9zc1N0" + "YXJ0GAIgASgDEhQKDE5leHRDcm9zc0VuZBgDIAEoA0IuWgkuLztwcm90b2Oq" + "AgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(MineWarOpenInfo), (MessageParser)(object)MineWarOpenInfo.Parser, new string[3] { "CrossOpenTm", "NextCrossStart", "NextCrossEnd" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
