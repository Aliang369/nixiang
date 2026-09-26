using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateWallSubjectRouletteTargetAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateWallSubjectRouletteTargetAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ci50eXBlLk9wZXJhdGVXYWxsU3ViamVjdFJvdWxldHRlVGFyZ2V0QWNrLnBy" + "b3RvEgV0eXBlcxoWdHlwZS5Bd2FyZERldGFpbC5wcm90bxoVdHlwZS5EYXRh" + "Q2hhbmdlLnByb3RvGhx0eXBlLldhbGxTdWJqZWN0RGV0YWlsLnByb3RvIqYB" + "CiNPcGVyYXRlV2FsbFN1YmplY3RSb3VsZXR0ZVRhcmdldEFjaxIxCg9XYWxs" + "U3ViamVjdExpc3QYASADKAsyGC50eXBlcy5XYWxsU3ViamVjdERldGFpbBIl" + "CglBd2FyZExpc3QYAiADKAsyEi50eXBlcy5Bd2FyZERldGFpbBIlCgpEYXRh" + "Q2hhbmdlGAMgASgLMhEudHlwZXMuRGF0YUNoYW5nZUIuWgkuLztwcm90b2Oq" + "AgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeWallSubjectDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateWallSubjectRouletteTargetAck), (MessageParser)(object)OperateWallSubjectRouletteTargetAck.Parser, new string[3] { "WallSubjectList", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
