using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeAuxResourceQuickBuyReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeAuxResourceQuickBuyReqReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiF0eXBlLkF1eFJlc291cmNlUXVpY2tCdXlSZXEucHJvdG8SBXR5cGVzGhhl" + "bnVtLkVSZXNvdXJjZVR5cGUucHJvdG8iZQoWQXV4UmVzb3VyY2VRdWlja0J1" + "eVJlcRIqCgRUeXBlGAEgASgOMhwuRVJlc291cmNlVHlwZS5FUmVzb3VyY2VU" + "eXBlEgsKA1RpZBgCIAEoAxISCgpHcm91cENvdW50GAMgASgFQi5aCS4vO3By" + "b3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90" + "bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEResourceTypeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(AuxResourceQuickBuyReq), (MessageParser)(object)AuxResourceQuickBuyReq.Parser, new string[3] { "Type", "Tid", "GroupCount" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
