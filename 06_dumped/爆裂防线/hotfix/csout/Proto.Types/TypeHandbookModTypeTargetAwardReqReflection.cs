using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeHandbookModTypeTargetAwardReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeHandbookModTypeTargetAwardReqReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cih0eXBlLkhhbmRib29rTW9kVHlwZVRhcmdldEF3YXJkUmVxLnByb3RvEgV0" + "eXBlcxoVZW51bS5FU2tpbGxUeXBlLnByb3RvIkoKHUhhbmRib29rTW9kVHlw" + "ZVRhcmdldEF3YXJkUmVxEikKCVNraWxsVHlwZRgBIAEoDjIWLkVTa2lsbFR5" + "cGUuRVNraWxsVHlwZUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpi" + "Z2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { EnumESkillTypeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(HandbookModTypeTargetAwardReq), (MessageParser)(object)HandbookModTypeTargetAwardReq.Parser, new string[1] { "SkillType" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
