using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateSnowVeilTargetDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateSnowVeilTargetDetailReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiZ0eXBlLk9wZXJhdGVTbm93VmVpbFRhcmdldERldGFpbC5wcm90bxIFdHlw" + "ZXMaFnR5cGUuQXdhcmREZXRhaWwucHJvdG8iYwobT3BlcmF0ZVNub3dWZWls" + "VGFyZ2V0RGV0YWlsEgoKAklkGAEgASgFEhEKCUNvc3RTY29yZRgCIAEoBRIl" + "CglBd2FyZExpc3QYAyADKAsyEi50eXBlcy5Bd2FyZERldGFpbEIuWgkuLztw" + "cm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJv" + "dG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeAwardDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateSnowVeilTargetDetail), (MessageParser)(object)OperateSnowVeilTargetDetail.Parser, new string[3] { "Id", "CostScore", "AwardList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
