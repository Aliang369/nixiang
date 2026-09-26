using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class VipInfo : IMessage<VipInfo>, IMessage, IEquatable<VipInfo>, IDeepCloneable<VipInfo>, IBufferMessage
{
	private static readonly MessageParser<VipInfo> _parser = new MessageParser<VipInfo>((Func<VipInfo>)(() => new VipInfo()));

	private UnknownFieldSet _unknownFields;

	public const int VipExpInfoFieldNumber = 1;

	private VipExpInfo vipExpInfo_;

	public const int VipFreeExpFieldNumber = 2;

	private VipFreeExpInfo vipFreeExp_;

	public const int VipLevelPackFieldNumber = 3;

	private static readonly FieldCodec<int> _repeated_vipLevelPack_codec = FieldCodec.ForInt32(26u);

	private readonly RepeatedField<int> vipLevelPack_ = new RepeatedField<int>();

	public const int DailyFreeFieldNumber = 4;

	private int dailyFree_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<VipInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeVipInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public VipExpInfo VipExpInfo
	{
		get
		{
			return vipExpInfo_;
		}
		set
		{
			vipExpInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public VipFreeExpInfo VipFreeExp
	{
		get
		{
			return vipFreeExp_;
		}
		set
		{
			vipFreeExp_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<int> VipLevelPack => vipLevelPack_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int DailyFree
	{
		get
		{
			return dailyFree_;
		}
		set
		{
			dailyFree_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public VipInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public VipInfo(VipInfo other)
		: this()
	{
		vipExpInfo_ = ((other.vipExpInfo_ != null) ? other.vipExpInfo_.Clone() : null);
		vipFreeExp_ = ((other.vipFreeExp_ != null) ? other.vipFreeExp_.Clone() : null);
		vipLevelPack_ = other.vipLevelPack_.Clone();
		dailyFree_ = other.dailyFree_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public VipInfo Clone()
	{
		return new VipInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as VipInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(VipInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(VipExpInfo, other.VipExpInfo))
		{
			return false;
		}
		if (!object.Equals(VipFreeExp, other.VipFreeExp))
		{
			return false;
		}
		if (!vipLevelPack_.Equals(other.vipLevelPack_))
		{
			return false;
		}
		if (DailyFree != other.DailyFree)
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
		if (vipExpInfo_ != null)
		{
			num ^= VipExpInfo.GetHashCode();
		}
		if (vipFreeExp_ != null)
		{
			num ^= VipFreeExp.GetHashCode();
		}
		num ^= ((object)vipLevelPack_).GetHashCode();
		if (DailyFree != 0)
		{
			num ^= DailyFree.GetHashCode();
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
		if (vipExpInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)VipExpInfo);
		}
		if (vipFreeExp_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)VipFreeExp);
		}
		vipLevelPack_.WriteTo(ref output, _repeated_vipLevelPack_codec);
		if (DailyFree != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(DailyFree);
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
		if (vipExpInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)VipExpInfo);
		}
		if (vipFreeExp_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)VipFreeExp);
		}
		num += vipLevelPack_.CalculateSize(_repeated_vipLevelPack_codec);
		if (DailyFree != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(DailyFree);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(VipInfo other)
	{
		if (other == null)
		{
			return;
		}
		if (other.vipExpInfo_ != null)
		{
			if (vipExpInfo_ == null)
			{
				VipExpInfo = new VipExpInfo();
			}
			VipExpInfo.MergeFrom(other.VipExpInfo);
		}
		if (other.vipFreeExp_ != null)
		{
			if (vipFreeExp_ == null)
			{
				VipFreeExp = new VipFreeExpInfo();
			}
			VipFreeExp.MergeFrom(other.VipFreeExp);
		}
		vipLevelPack_.Add((IEnumerable<int>)other.vipLevelPack_);
		if (other.DailyFree != 0)
		{
			DailyFree = other.DailyFree;
		}
		_unknownFields = UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
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
			case 10u:
				if (vipExpInfo_ == null)
				{
					VipExpInfo = new VipExpInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)VipExpInfo);
				break;
			case 18u:
				if (vipFreeExp_ == null)
				{
					VipFreeExp = new VipFreeExpInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)VipFreeExp);
				break;
			case 24u:
			case 26u:
				vipLevelPack_.AddEntriesFrom(ref input, _repeated_vipLevelPack_codec);
				break;
			case 32u:
				DailyFree = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
