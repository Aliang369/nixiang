using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeMineWarBaseInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeMineWarBaseInfoReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chp0eXBlLk1pbmVXYXJCYXNlSW5mby5wcm90bxIFdHlwZXMaGnR5cGUuTWlu" + "ZVdhck9wZW5JbmZvLnByb3RvGhp0eXBlLk1pbmVXYXJTZWxmSW5mby5wcm90" + "byJzCg9NaW5lV2FyQmFzZUluZm8SLwoPTWluZVdhck9wZW5JbmZvGAEgASgL" + "MhYudHlwZXMuTWluZVdhck9wZW5JbmZvEi8KD01pbmVXYXJTZWxmSW5mbxgC" + "IAEoCzIWLnR5cGVzLk1pbmVXYXJTZWxmSW5mb0IuWgkuLztwcm90b2OqAgtQ" + "cm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeMineWarOpenInfoReflection.Descriptor,
			TypeMineWarSelfInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(MineWarBaseInfo), (MessageParser)(object)MineWarBaseInfo.Parser, new string[2] { "MineWarOpenInfo", "MineWarSelfInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
