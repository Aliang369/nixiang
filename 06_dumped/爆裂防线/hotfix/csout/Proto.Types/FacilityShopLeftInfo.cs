using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class FacilityShopLeftInfo : IMessage<FacilityShopLeftInfo>, IMessage, IEquatable<FacilityShopLeftInfo>, IDeepCloneable<FacilityShopLeftInfo>, IBufferMessage
{
	private static readonly MessageParser<FacilityShopLeftInfo> _parser = new MessageParser<FacilityShopLeftInfo>((Func<FacilityShopLeftInfo>)(() => new FacilityShopLeftInfo()));

	private UnknownFieldSet _unknownFields;

	public const int SlotCountFieldNumber = 1;

	private int slotCount_;

	public const int TotalLeftFieldNumber = 2;

	private int totalLeft_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<FacilityShopLeftInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeFacilityShopLeftInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int SlotCount
	{
		get
		{
			return slotCount_;
		}
		set
		{
			slotCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int TotalLeft
	{
		get
		{
			return totalLeft_;
		}
		set
		{
			totalLeft_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FacilityShopLeftInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FacilityShopLeftInfo(FacilityShopLeftInfo other)
		: this()
	{
		slotCount_ = other.slotCount_;
		totalLeft_ = other.totalLeft_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FacilityShopLeftInfo Clone()
	{
		return new FacilityShopLeftInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as FacilityShopLeftInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(FacilityShopLeftInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (SlotCount != other.SlotCount)
		{
			return false;
		}
		if (TotalLeft != other.TotalLeft)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override int GetHashCode()
	{
		int num = 1;
		if (SlotCount != 0)
		{
			num ^= SlotCount.GetHashCode();
		}
		if (TotalLeft != 0)
		{
			num ^= TotalLeft.GetHashCode();
		}
		if (_unknownFields != null)
		{
			num ^= ((object)_unknownFields).GetHashCode();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override string ToString()
	{
		return JsonFormatter.ToDiagnosticString((IMessage)(object)this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void WriteTo(CodedOutputStream output)
	{
		output.WriteRawMessage((IMessage)(object)this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	void IBufferMessage.InternalWriteTo(ref WriteContext output)
	{
		if (SlotCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(SlotCount);
		}
		if (TotalLeft != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt32(TotalLeft);
		}
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(ref output);
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int CalculateSize()
	{
		int num = 0;
		if (SlotCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(SlotCount);
		}
		if (TotalLeft != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(TotalLeft);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(FacilityShopLeftInfo other)
	{
		if (other != null)
		{
			if (other.SlotCount != 0)
			{
				SlotCount = other.SlotCount;
			}
			if (other.TotalLeft != 0)
			{
				TotalLeft = other.TotalLeft;
			}
			_unknownFields = UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(CodedInputStream input)
	{
		input.ReadRawMessage((IMessage)(object)this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	void IBufferMessage.InternalMergeFrom(ref ParseContext input)
	{
		uint num;
		while ((num = ((ParseContext)(ref input)).ReadTag()) != 0)
		{
			switch (num)
			{
			default:
				_unknownFields = UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
				break;
			case 8u:
				SlotCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				TotalLeft = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
