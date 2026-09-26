using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeTrainTransportTakeRefreshAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeTrainTransportTakeRefreshAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cid0eXBlLlRyYWluVHJhbnNwb3J0VGFrZVJlZnJlc2hBY2sucHJvdG8SBXR5" + "cGVzGhV0eXBlLkRhdGFDaGFuZ2UucHJvdG8aFHR5cGUuVHJhaW5JbmZvLnBy" + "b3RvImoKHFRyYWluVHJhbnNwb3J0VGFrZVJlZnJlc2hBY2sSIwoJVHJhaW5J" + "bmZvGAEgASgLMhAudHlwZXMuVHJhaW5JbmZvEiUKCkRhdGFDaGFuZ2UYAiAB" + "KAsyES50eXBlcy5EYXRhQ2hhbmdlQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5" + "cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDataChangeReflection.Descriptor,
			TypeTrainInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(TrainTransportTakeRefreshAck), (MessageParser)(object)TrainTransportTakeRefreshAck.Parser, new string[2] { "TrainInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
