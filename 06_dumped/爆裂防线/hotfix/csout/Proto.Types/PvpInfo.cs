using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class PvpInfo : IMessage<PvpInfo>, IMessage, IEquatable<PvpInfo>, IDeepCloneable<PvpInfo>, IBufferMessage
{
	private static readonly MessageParser<PvpInfo> _parser = new MessageParser<PvpInfo>((Func<PvpInfo>)(() => new PvpInfo()));

	private UnknownFieldSet _unknownFields;

	public const int ValidModInfoFieldNumber = 1;

	private PvpModInfo validModInfo_;

	public const int SetModInfoFieldNumber = 2;

	private PvpModInfo setModInfo_;

	public const int SetCountFieldNumber = 3;

	private int setCount_;

	public const int SetModListFieldNumber = 4;

	private static readonly FieldCodec<int> _repeated_setModList_codec = FieldCodec.ForInt32(34u);

	private readonly RepeatedField<int> setModList_ = new RepeatedField<int>();

	public const int RefreshCountFieldNumber = 5;

	private int refreshCount_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<PvpInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypePvpInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public PvpModInfo ValidModInfo
	{
		get
		{
			return validModInfo_;
		}
		set
		{
			validModInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public PvpModInfo SetModInfo
	{
		get
		{
			return setModInfo_;
		}
		set
		{
			setModInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int SetCount
	{
		get
		{
			return setCount_;
		}
		set
		{
			setCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<int> SetModList => setModList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int RefreshCount
	{
		get
		{
			return refreshCount_;
		}
		set
		{
			refreshCount_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public PvpInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public PvpInfo(PvpInfo other)
		: this()
	{
		validModInfo_ = ((other.validModInfo_ != null) ? other.validModInfo_.Clone() : null);
		setModInfo_ = ((other.setModInfo_ != null) ? other.setModInfo_.Clone() : null);
		setCount_ = other.setCount_;
		setModList_ = other.setModList_.Clone();
		refreshCount_ = other.refreshCount_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public PvpInfo Clone()
	{
		return new PvpInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as PvpInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(PvpInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(ValidModInfo, other.ValidModInfo))
		{
			return false;
		}
		if (!object.Equals(SetModInfo, other.SetModInfo))
		{
			return false;
		}
		if (SetCount != other.SetCount)
		{
			return false;
		}
		if (!setModList_.Equals(other.setModList_))
		{
			return false;
		}
		if (RefreshCount != other.RefreshCount)
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
		if (validModInfo_ != null)
		{
			num ^= ValidModInfo.GetHashCode();
		}
		if (setModInfo_ != null)
		{
			num ^= SetModInfo.GetHashCode();
		}
		if (SetCount != 0)
		{
			num ^= SetCount.GetHashCode();
		}
		num ^= ((object)setModList_).GetHashCode();
		if (RefreshCount != 0)
		{
			num ^= RefreshCount.GetHashCode();
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
		if (validModInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)ValidModInfo);
		}
		if (setModInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)SetModInfo);
		}
		if (SetCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(SetCount);
		}
		setModList_.WriteTo(ref output, _repeated_setModList_codec);
		if (RefreshCount != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt32(RefreshCount);
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
		if (validModInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)ValidModInfo);
		}
		if (setModInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)SetModInfo);
		}
		if (SetCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(SetCount);
		}
		num += setModList_.CalculateSize(_repeated_setModList_codec);
		if (RefreshCount != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(RefreshCount);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(PvpInfo other)
	{
		if (other == null)
		{
			return;
		}
		if (other.validModInfo_ != null)
		{
			if (validModInfo_ == null)
			{
				ValidModInfo = new PvpModInfo();
			}
			ValidModInfo.MergeFrom(other.ValidModInfo);
		}
		if (other.setModInfo_ != null)
		{
			if (setModInfo_ == null)
			{
				SetModInfo = new PvpModInfo();
			}
			SetModInfo.MergeFrom(other.SetModInfo);
		}
		if (other.SetCount != 0)
		{
			SetCount = other.SetCount;
		}
		setModList_.Add((IEnumerable<int>)other.setModList_);
		if (other.RefreshCount != 0)
		{
			RefreshCount = other.RefreshCount;
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
				if (validModInfo_ == null)
				{
					ValidModInfo = new PvpModInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)ValidModInfo);
				break;
			case 18u:
				if (setModInfo_ == null)
				{
					SetModInfo = new PvpModInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)SetModInfo);
				break;
			case 24u:
				SetCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
			case 34u:
				setModList_.AddEntriesFrom(ref input, _repeated_setModList_codec);
				break;
			case 40u:
				RefreshCount = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
