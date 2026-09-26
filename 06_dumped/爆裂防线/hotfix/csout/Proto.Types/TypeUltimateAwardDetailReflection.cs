using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeUltimateAwardDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeUltimateAwardDetailReflection()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLlVsdGltYXRlQXdhcmREZXRhaWwucHJvdG8SBXR5cGVzGhZ0eXBl" + "LkF3YXJkRGV0YWlsLnByb3RvIlkKE1VsdGltYXRlQXdhcmREZXRhaWwSDQoF" + "SW5kZXgYASABKAUSEAoIU2VsZWN0ZWQYAiABKAUSIQoFQXdhcmQYAyABKAsy" + "Ei50eXBlcy5Bd2FyZERldGFpbEIuWgkuLztwcm90b2OqAgtQcm90by5UeXBl" + "c8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeAwardDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(UltimateAwardDetail), (MessageParser)(object)UltimateAwardDetail.Parser, new string[3] { "Index", "Selected", "Award" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
