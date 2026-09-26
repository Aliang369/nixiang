using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeNoviceLoginDayDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeNoviceLoginDayDetailReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch90eXBlLk5vdmljZUxvZ2luRGF5RGV0YWlsLnByb3RvEgV0eXBlcxoXZW51" + "bS5FQ29tbW9uU3RhdGUucHJvdG8iTgoUTm92aWNlTG9naW5EYXlEZXRhaWwS" + "CwoDRGF5GAEgASgFEikKBVN0YXRlGAIgASgOMhouRUNvbW1vblN0YXRlLkVD" + "b21tb25TdGF0ZUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2Ft" + "ZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { EnumECommonStateReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(NoviceLoginDayDetail), (MessageParser)(object)NoviceLoginDayDetail.Parser, new string[2] { "Day", "State" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
