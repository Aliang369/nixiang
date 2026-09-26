using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeTrainInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeTrainInfoReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChR0eXBlLlRyYWluSW5mby5wcm90bxIFdHlwZXMaFnR5cGUuVHJhaW5EZXRh" + "aWwucHJvdG8aGXR5cGUuVHJhaW5NYXRjaEluZm8ucHJvdG8ilwEKCVRyYWlu" + "SW5mbxISCgpVbmxvY2tMaXN0GAEgAygFEiUKCVNlbGZUcmFpbhgCIAEoCzIS" + "LnR5cGVzLlRyYWluRGV0YWlsEiUKCVdhaXRUcmFpbhgDIAEoCzISLnR5cGVz" + "LlRyYWluRGV0YWlsEigKCU1hdGNoSW5mbxgEIAEoCzIVLnR5cGVzLlRyYWlu" + "TWF0Y2hJbmZvQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1l" + "XFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeTrainDetailReflection.Descriptor,
			TypeTrainMatchInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(TrainInfo), (MessageParser)(object)TrainInfo.Parser, new string[4] { "UnlockList", "SelfTrain", "WaitTrain", "MatchInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
