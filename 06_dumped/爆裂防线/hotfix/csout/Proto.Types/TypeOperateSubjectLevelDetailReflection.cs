using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeOperateSubjectLevelDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateSubjectLevelDetailReflection()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiR0eXBlLk9wZXJhdGVTdWJqZWN0TGV2ZWxEZXRhaWwucHJvdG8SBXR5cGVz" + "Gh1lbnVtLkVCYXR0bGVSZXN1bHRTY29yZS5wcm90byJoChlPcGVyYXRlU3Vi" + "amVjdExldmVsRGV0YWlsEg8KB0xldmVsSWQYASABKAUSOgoKTGV2ZWxTY29y" + "ZRgCIAEoDjImLkVCYXR0bGVSZXN1bHRTY29yZS5FQmF0dGxlUmVzdWx0U2Nv" + "cmVCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9c" + "VHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEBattleResultScoreReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateSubjectLevelDetail), (MessageParser)(object)OperateSubjectLevelDetail.Parser, new string[2] { "LevelId", "LevelScore" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
