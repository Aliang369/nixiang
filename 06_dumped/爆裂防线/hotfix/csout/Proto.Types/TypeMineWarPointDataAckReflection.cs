using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeMineWarPointDataAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeMineWarPointDataAckReflection()
	{
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Expected O, but got Unknown
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLk1pbmVXYXJQb2ludERhdGFBY2sucHJvdG8SBXR5cGVzGhp0eXBl" + "Lk1pbmVXYXJCYXNlSW5mby5wcm90bxoddHlwZS5NaW5lV2FyUG9pbnREZXRh" + "aWwucHJvdG8aH3R5cGUuTWluZVdhclBvaW50SWRsZUluZm8ucHJvdG8aF3R5" + "cGUuUm9sZUJhc2VJbmZvLnByb3RvItMBChNNaW5lV2FyUG9pbnREYXRhQWNr" + "Ei8KD01pbmVXYXJCYXNlSW5mbxgBIAEoCzIWLnR5cGVzLk1pbmVXYXJCYXNl" + "SW5mbxIsCglQb2ludEluZm8YAiABKAsyGS50eXBlcy5NaW5lV2FyUG9pbnRE" + "ZXRhaWwSMgoNUG9pbnRJZGxlSW5mbxgDIAEoCzIbLnR5cGVzLk1pbmVXYXJQ" + "b2ludElkbGVJbmZvEikKDFJvbGVCYXNlSW5mbxgEIAEoCzITLnR5cGVzLlJv" + "bGVCYXNlSW5mb0IuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2Ft" + "ZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			TypeMineWarBaseInfoReflection.Descriptor,
			TypeMineWarPointDetailReflection.Descriptor,
			TypeMineWarPointIdleInfoReflection.Descriptor,
			TypeRoleBaseInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(MineWarPointDataAck), (MessageParser)(object)MineWarPointDataAck.Parser, new string[4] { "MineWarBaseInfo", "PointInfo", "PointIdleInfo", "RoleBaseInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
