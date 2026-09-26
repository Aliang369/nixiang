using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeFacilityRecommendAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeFacilityRecommendAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch90eXBlLkZhY2lsaXR5UmVjb21tZW5kQWNrLnByb3RvEgV0eXBlcxobdHlw" + "ZS5GYWNpbGl0eUJhc2VJbmZvLnByb3RvIkYKFEZhY2lsaXR5UmVjb21tZW5k" + "QWNrEi4KDVJlY29tbWVuZExpc3QYASADKAsyFy50eXBlcy5GYWNpbGl0eUJh" + "c2VJbmZvQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFBy" + "b3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeFacilityBaseInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(FacilityRecommendAck), (MessageParser)(object)FacilityRecommendAck.Parser, new string[1] { "RecommendList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
