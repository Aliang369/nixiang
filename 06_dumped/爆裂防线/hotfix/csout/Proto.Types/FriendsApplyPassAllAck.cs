using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class FriendsApplyPassAllAck : IMessage<FriendsApplyPassAllAck>, IMessage, IEquatable<FriendsApplyPassAllAck>, IDeepCloneable<FriendsApplyPassAllAck>, IBufferMessage
{
	private static readonly MessageParser<FriendsApplyPassAllAck> _parser = new MessageParser<FriendsApplyPassAllAck>((Func<FriendsApplyPassAllAck>)(() => new FriendsApplyPassAllAck()));

	private UnknownFieldSet _unknownFields;

	public const int FriendsListFieldNumber = 1;

	private static readonly FieldCodec<FriendsDetail> _repeated_friendsList_codec = FieldCodec.ForMessage<FriendsDetail>(10u, FriendsDetail.Parser);

	private readonly RepeatedField<FriendsDetail> friendsList_ = new RepeatedField<FriendsDetail>();

	public const int ApplyListFieldNumber = 2;

	private static readonly FieldCodec<FriendsApplyDetail> _repeated_applyList_codec = FieldCodec.ForMessage<FriendsApplyDetail>(18u, FriendsApplyDetail.Parser);

	private readonly RepeatedField<FriendsApplyDetail> applyList_ = new RepeatedField<FriendsApplyDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<FriendsApplyPassAllAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeFriendsApplyPassAllAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<FriendsDetail> FriendsList => friendsList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<FriendsApplyDetail> ApplyList => applyList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FriendsApplyPassAllAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FriendsApplyPassAllAck(FriendsApplyPassAllAck other)
		: this()
	{
		friendsList_ = other.friendsList_.Clone();
		applyList_ = other.applyList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public FriendsApplyPassAllAck Clone()
	{
		return new FriendsApplyPassAllAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as FriendsApplyPassAllAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(FriendsApplyPassAllAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!friendsList_.Equals(other.friendsList_))
		{
			return false;
		}
		if (!applyList_.Equals(other.applyList_))
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
		num ^= ((object)friendsList_).GetHashCode();
		num ^= ((object)applyList_).GetHashCode();
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
		friendsList_.WriteTo(ref output, _repeated_friendsList_codec);
		applyList_.WriteTo(ref output, _repeated_applyList_codec);
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
		num += friendsList_.CalculateSize(_repeated_friendsList_codec);
		num += applyList_.CalculateSize(_repeated_applyList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(FriendsApplyPassAllAck other)
	{
		if (other != null)
		{
			friendsList_.Add((IEnumerable<FriendsDetail>)other.friendsList_);
			applyList_.Add((IEnumerable<FriendsApplyDetail>)other.applyList_);
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
			case 10u:
				friendsList_.AddEntriesFrom(ref input, _repeated_friendsList_codec);
				break;
			case 18u:
				applyList_.AddEntriesFrom(ref input, _repeated_applyList_codec);
				break;
			}
		}
	}
}
