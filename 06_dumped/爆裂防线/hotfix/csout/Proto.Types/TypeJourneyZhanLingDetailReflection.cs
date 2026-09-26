using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeJourneyZhanLingDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeJourneyZhanLingDetailReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLkpvdXJuZXlaaGFuTGluZ0RldGFpbC5wcm90bxIFdHlwZXMaJnR5" + "cGUuSm91cm5leVpoYW5MaW5nQWN0aXZlRGV0YWlsLnByb3RvIn8KFUpvdXJu" + "ZXlaaGFuTGluZ0RldGFpbBINCgVMZXZlbBgBIAEoBRINCgVSZWFjaBgCIAEo" + "BRIQCghMb3dBd2FyZBgDIAEoBRI2CgpBY3RpdmVMaXN0GAQgAygLMiIudHlw" + "ZXMuSm91cm5leVpoYW5MaW5nQWN0aXZlRGV0YWlsQi5aCS4vO3Byb3RvY6oC" + "C1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeJourneyZhanLingActiveDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(JourneyZhanLingDetail), (MessageParser)(object)JourneyZhanLingDetail.Parser, new string[4] { "Level", "Reach", "LowAward", "ActiveList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
