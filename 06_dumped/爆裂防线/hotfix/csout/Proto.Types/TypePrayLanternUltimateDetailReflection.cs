using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypePrayLanternUltimateDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypePrayLanternUltimateDetailReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiR0eXBlLlByYXlMYW50ZXJuVWx0aW1hdGVEZXRhaWwucHJvdG8SBXR5cGVz" + "GhdlbnVtLkVDb21tb25TdGF0ZS5wcm90bxoedHlwZS5VbHRpbWF0ZUF3YXJk" + "RGV0YWlsLnByb3RvIn0KGVByYXlMYW50ZXJuVWx0aW1hdGVEZXRhaWwSNQoR" + "VWx0aW1hdGVBd2FyZExpc3QYASADKAsyGi50eXBlcy5VbHRpbWF0ZUF3YXJk" + "RGV0YWlsEikKBVN0YXRlGAIgASgOMhouRUNvbW1vblN0YXRlLkVDb21tb25T" + "dGF0ZUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90" + "b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumECommonStateReflection.Descriptor,
			TypeUltimateAwardDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(PrayLanternUltimateDetail), (MessageParser)(object)PrayLanternUltimateDetail.Parser, new string[2] { "UltimateAwardList", "State" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
