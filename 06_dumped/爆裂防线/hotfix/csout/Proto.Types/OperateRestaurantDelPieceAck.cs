using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class OperateRestaurantDelPieceAck : IMessage<OperateRestaurantDelPieceAck>, IMessage, IEquatable<OperateRestaurantDelPieceAck>, IDeepCloneable<OperateRestaurantDelPieceAck>, IBufferMessage
{
	private static readonly MessageParser<OperateRestaurantDelPieceAck> _parser = new MessageParser<OperateRestaurantDelPieceAck>((Func<OperateRestaurantDelPieceAck>)(() => new OperateRestaurantDelPieceAck()));

	private UnknownFieldSet _unknownFields;

	public const int RestaurantListFieldNumber = 1;

	private static readonly FieldCodec<RestaurantDetail> _repeated_restaurantList_codec = FieldCodec.ForMessage<RestaurantDetail>(10u, RestaurantDetail.Parser);

	private readonly RepeatedField<RestaurantDetail> restaurantList_ = new RepeatedField<RestaurantDetail>();

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<OperateRestaurantDelPieceAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeOperateRestaurantDelPieceAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RepeatedField<RestaurantDetail> RestaurantList => restaurantList_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateRestaurantDelPieceAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateRestaurantDelPieceAck(OperateRestaurantDelPieceAck other)
		: this()
	{
		restaurantList_ = other.restaurantList_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OperateRestaurantDelPieceAck Clone()
	{
		return new OperateRestaurantDelPieceAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as OperateRestaurantDelPieceAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(OperateRestaurantDelPieceAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!restaurantList_.Equals(other.restaurantList_))
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
		num ^= ((object)restaurantList_).GetHashCode();
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
		restaurantList_.WriteTo(ref output, _repeated_restaurantList_codec);
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
		num += restaurantList_.CalculateSize(_repeated_restaurantList_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(OperateRestaurantDelPieceAck other)
	{
		if (other != null)
		{
			restaurantList_.Add((IEnumerable<RestaurantDetail>)other.restaurantList_);
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
			if (num != 10)
			{
				_unknownFields = UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
			}
			else
			{
				restaurantList_.AddEntriesFrom(ref input, _repeated_restaurantList_codec);
			}
		}
	}
}
