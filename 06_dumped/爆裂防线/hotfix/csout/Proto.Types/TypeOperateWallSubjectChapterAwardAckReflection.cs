using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateWallSubjectChapterAwardAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateWallSubjectChapterAwardAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cix0eXBlLk9wZXJhdGVXYWxsU3ViamVjdENoYXB0ZXJBd2FyZEFjay5wcm90" + "bxIFdHlwZXMaFnR5cGUuQXdhcmREZXRhaWwucHJvdG8aFXR5cGUuRGF0YUNo" + "YW5nZS5wcm90bxocdHlwZS5XYWxsU3ViamVjdERldGFpbC5wcm90byKkAQoh" + "T3BlcmF0ZVdhbGxTdWJqZWN0Q2hhcHRlckF3YXJkQWNrEjEKD1dhbGxTdWJq" + "ZWN0TGlzdBgBIAMoCzIYLnR5cGVzLldhbGxTdWJqZWN0RGV0YWlsEiUKCUF3" + "YXJkTGlzdBgCIAMoCzISLnR5cGVzLkF3YXJkRGV0YWlsEiUKCkRhdGFDaGFu" + "Z2UYAyABKAsyES50eXBlcy5EYXRhQ2hhbmdlQi5aCS4vO3Byb3RvY6oCC1By" + "b3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeWallSubjectDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateWallSubjectChapterAwardAck), (MessageParser)(object)OperateWallSubjectChapterAwardAck.Parser, new string[3] { "WallSubjectList", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
