using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypePassportInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypePassportInfoReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chd0eXBlLlBhc3Nwb3J0SW5mby5wcm90bxIFdHlwZXMaG3R5cGUuUGFzc3Bv" + "cnRCYXNlSW5mby5wcm90bxocdHlwZS5QYXNzcG9ydExldmVsSW5mby5wcm90" + "byJmCgxQYXNzcG9ydEluZm8SKQoIQmFzZUluZm8YASABKAsyFy50eXBlcy5Q" + "YXNzcG9ydEJhc2VJbmZvEisKCUxldmVsSW5mbxgCIAEoCzIYLnR5cGVzLlBh" + "c3Nwb3J0TGV2ZWxJbmZvQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygIS" + "WmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypePassportBaseInfoReflection.Descriptor,
			TypePassportLevelInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(PassportInfo), (MessageParser)(object)PassportInfo.Parser, new string[2] { "BaseInfo", "LevelInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
