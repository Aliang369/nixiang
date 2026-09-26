using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeHomelandFloorDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeHomelandFloorDetailReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLkhvbWVsYW5kRmxvb3JEZXRhaWwucHJvdG8SBXR5cGVzGhdlbnVt" + "LkVBY3RpdmVTdGF0ZS5wcm90byJPChNIb21lbGFuZEZsb29yRGV0YWlsEg0K" + "BUZsb29yGAEgASgFEikKBVN0YXRlGAIgASgOMhouRUFjdGl2ZVN0YXRlLkVB" + "Y3RpdmVTdGF0ZUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2Ft" + "ZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEActiveStateReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(HomelandFloorDetail), (MessageParser)(object)HomelandFloorDetail.Parser, new string[2] { "Floor", "State" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
