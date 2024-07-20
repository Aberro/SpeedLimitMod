using System;
using System.Diagnostics.CodeAnalysis;
using Unity.Entities;
using Colossal.Serialization.Entities;

namespace SpeedLimitEditor.Components;

public struct CustomSpeed :
	IComponentData,
	IQueryTypeParameter,
	IEquatable<CustomSpeed>,
	ISerializable
{
	public float m_Value;

	[SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator", Justification = "For Equals method we want to do precise comparison")]
	public bool Equals(CustomSpeed other)
	{
		return this.m_Value == other.m_Value;
	}

	public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
	{
		writer.Write(this.m_Value);
	}

	public void Deserialize<TReader>(TReader reader) where TReader : IReader
	{
		reader.Read(out this.m_Value);
	}
}
