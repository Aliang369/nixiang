using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeServantAttrDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeServantAttrDetailReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLlNlcnZhbnRBdHRyRGV0YWlsLnByb3RvEgV0eXBlcxoTZW51bS5F" + "UXVhbGl0eS5wcm90byJIChFTZXJ2YW50QXR0ckRldGFpbBIOCgZBdHRySWQY" + "ASABKAUSIwoHUXVhbGl0eRgCIAEoDjISLkVRdWFsaXR5LkVRdWFsaXR5Qi5a" + "CS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVz" + "YgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEQualityReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ServantAttrDetail), (MessageParser)(object)ServantAttrDetail.Parser, new string[2] { "AttrId", "Quality" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
