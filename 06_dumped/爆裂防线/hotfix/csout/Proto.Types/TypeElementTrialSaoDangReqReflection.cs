using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeElementTrialSaoDangReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeElementTrialSaoDangReqReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiF0eXBlLkVsZW1lbnRUcmlhbFNhb0RhbmdSZXEucHJvdG8SBXR5cGVzGhtl" + "bnVtLkVTa2lsbEF0dGFja1R5cGUucHJvdG8iTwoWRWxlbWVudFRyaWFsU2Fv" + "RGFuZ1JlcRI1CglUcmlhbFR5cGUYASABKA4yIi5FU2tpbGxBdHRhY2tUeXBl" + "LkVTa2lsbEF0dGFja1R5cGVCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPK" + "AhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumESkillAttackTypeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ElementTrialSaoDangReq), (MessageParser)(object)ElementTrialSaoDangReq.Parser, new string[1] { "TrialType" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
