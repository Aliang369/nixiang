using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeCrossMinePointDataAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeCrossMinePointDataAckReflection()
	{
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Expected O, but got Unknown
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLkNyb3NzTWluZVBvaW50RGF0YUFjay5wcm90bxIFdHlwZXMaGnR5" + "cGUuTWluZVdhckJhc2VJbmZvLnByb3RvGh10eXBlLk1pbmVXYXJQb2ludERl" + "dGFpbC5wcm90bxoXdHlwZS5Sb2xlQmFzZUluZm8ucHJvdG8iswEKFUNyb3Nz" + "TWluZVBvaW50RGF0YUFjaxIQCghTZXJ2ZXJJZBgBIAEoBRIsCglQb2ludElu" + "Zm8YAiABKAsyGS50eXBlcy5NaW5lV2FyUG9pbnREZXRhaWwSKQoMUm9sZUJh" + "c2VJbmZvGAMgASgLMhMudHlwZXMuUm9sZUJhc2VJbmZvEi8KD01pbmVXYXJC" + "YXNlSW5mbxgEIAEoCzIWLnR5cGVzLk1pbmVXYXJCYXNlSW5mb0IuWgkuLztw" + "cm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJv" + "dG8z"), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeMineWarBaseInfoReflection.Descriptor,
			TypeMineWarPointDetailReflection.Descriptor,
			TypeRoleBaseInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(CrossMinePointDataAck), (MessageParser)(object)CrossMinePointDataAck.Parser, new string[4] { "ServerId", "PointInfo", "RoleBaseInfo", "MineWarBaseInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
