using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class AdsCommonTargetDetail : IMessage<AdsCommonTargetDetail>, IMessage, IEquatable<AdsCommonTargetDetail>, IDeepCloneable<AdsCommonTargetDetail>, IBufferMessage
{
	private static readonly MessageParser<AdsCommonTargetDetail> _parser = new MessageParser<AdsCommonTargetDetail>((Func<AdsCommonTargetDetail>)(() => new AdsCommonTargetDetail()));

	private UnknownFieldSet _unknownFields;

	public const int TargetFieldNumber = 1;

	private int target_;

	public const int FreeStateFieldNumber = 2;

	private ECommonState freeState_;

	public const int FreeAwardFieldNumber = 3;

	private static readonly FieldCodec<AwardDetail> _repeated_freeAward_codec = FieldCodec.ForMessage<AwardDetail>(26u, AwardDetail.Parser);

	private readonly RepeatedField<AwardDetail> freeAward_ = new RepeatedField<AwardDetail>();

	public const int AdStateFieldNumber = 4;

	private ECommonState adState_;

	public const int AdAwardFieldNumber = 5;

	private static readonly FieldCodec<AwardDetail> _repeated_adAward_codec = FieldCodec.ForMessage<AwardDetail>(42u, AwardDetail.Parser);

	private readonly RepeatedField<AwardDetail> adAward_ = new RepeatedField<AwardDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<AdsCommonTargetDetail> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeAdsCommonTargetDetailReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Target
	{
		get
		{
			return target_;
		}
		set
		{
			target_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ECommonState FreeState
	{
		get
		{
			return freeState_;
		}
		set
		{
			freeState_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<AwardDetail> FreeAward => freeAward_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ECommonState AdState
	{
		get
		{
			return adState_;
		}
		set
		{
			adState_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<AwardDetail> AdAward => adAward_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public AdsCommonTargetDetail()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public AdsCommonTargetDetail(AdsCommonTargetDetail other)
		: this()
	{
		target_ = other.target_;
		freeState_ = other.freeState_;
		freeAward_ = other.freeAward_.Clone();
		adState_ = other.adState_;
		adAward_ = other.adAward_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public AdsCommonTargetDetail Clone()
	{
		return new AdsCommonTargetDetail(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as AdsCommonTargetDetail);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(AdsCommonTargetDetail other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Target != other.Target)
		{
			return false;
		}
		if (FreeState != other.FreeState)
		{
			return false;
		}
		if (!freeAward_.Equals(other.freeAward_))
		{
			return false;
		}
		if (AdState != other.AdState)
		{
			return false;
		}
		if (!adAward_.Equals(other.adAward_))
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
		if (Target != 0)
		{
			num ^= Target.GetHashCode();
		}
		if (FreeState != ECommonState.No)
		{
			num ^= FreeState.GetHashCode();
		}
		num ^= ((object)freeAward_).GetHashCode();
		if (AdState != ECommonState.No)
		{
			num ^= AdState.GetHashCode();
		}
		num ^= ((object)adAward_).GetHashCode();
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
		if (Target != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(Target);
		}
		if (FreeState != ECommonState.No)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteEnum((int)FreeState);
		}
		freeAward_.WriteTo(ref output, _repeated_freeAward_codec);
		if (AdState != ECommonState.No)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteEnum((int)AdState);
		}
		adAward_.WriteTo(ref output, _repeated_adAward_codec);
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
		if (Target != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Target);
		}
		if (FreeState != ECommonState.No)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)FreeState);
		}
		num += freeAward_.CalculateSize(_repeated_freeAward_codec);
		if (AdState != ECommonState.No)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)AdState);
		}
		num += adAward_.CalculateSize(_repeated_adAward_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(AdsCommonTargetDetail other)
	{
		if (other != null)
		{
			if (other.Target != 0)
			{
				Target = other.Target;
			}
			if (other.FreeState != ECommonState.No)
			{
				FreeState = other.FreeState;
			}
			freeAward_.Add((IEnumerable<AwardDetail>)other.freeAward_);
			if (other.AdState != ECommonState.No)
			{
				AdState = other.AdState;
			}
			adAward_.Add((IEnumerable<AwardDetail>)other.adAward_);
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
				Target = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				FreeState = (ECommonState)((ParseContext)(ref input)).ReadEnum();
				break;
			case 26u:
				freeAward_.AddEntriesFrom(ref input, _repeated_freeAward_codec);
				break;
			case 32u:
				AdState = (ECommonState)((ParseContext)(ref input)).ReadEnum();
				break;
			case 42u:
				adAward_.AddEntriesFrom(ref input, _repeated_adAward_codec);
				break;
			}
		}
	}
}
