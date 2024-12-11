/*
 * Copyright (C) RyuSOCKS
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License version 2,
 * as published by the Free Software Foundation.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see <https://www.gnu.org/licenses/>.
 */

using Microsoft.CodeAnalysis;
using System;
using System.Linq;

namespace RyuSocks.Generator.Packet
{
    internal class PacketFieldAttributeData
    {
        public int Offset { get; } = Default.Offset;
        public string OffsetMember { get; } = Default.OffsetMember;
        public int Length { get; }
        public string LengthMember { get; }
        public bool IsBigEndian { get; }
        public int MinLength { get; }
        public int MaxLength { get; }
        public string AssumeGeneratedEnumType { get; }
        public string ValidationMethod { get; }

        // ReSharper disable once SimplifyConditionalTernaryExpression
        public PacketFieldAttributeData(AttributeData attributeData)
        {
            if (attributeData.ConstructorArguments.Length == 0)
            {
                throw new InvalidOperationException($"Attribute constructor doesn't have arguments: {attributeData.AttributeClass?.Name}");
            }

            // Parse constructor arguments
            switch (attributeData.ConstructorArguments[0].Type!.Name)
            {
                case nameof(String):
                    OffsetMember = (string)attributeData.ConstructorArguments[0].Value!;
                    break;
                case nameof(Int32):
                    Offset = (int)attributeData.ConstructorArguments[0].Value!;
                    break;
                default:
                    throw new InvalidOperationException($"Attribute constructor argument type is not valid: {attributeData.ConstructorArguments[0].Type!.Name}");
            }

            // Parse named arguments
            TypedConstant lengthArg = attributeData.NamedArguments.SingleOrDefault(kvp => kvp.Key == nameof(Length)).Value;
            TypedConstant lengthMemberArg = attributeData.NamedArguments.SingleOrDefault(kvp => kvp.Key == nameof(LengthMember)).Value;
            TypedConstant isBigEndianArg = attributeData.NamedArguments.SingleOrDefault(kvp => kvp.Key == nameof(IsBigEndian)).Value;
            TypedConstant minLengthArg = attributeData.NamedArguments.SingleOrDefault(kvp => kvp.Key == nameof(MinLength)).Value;
            TypedConstant maxLengthArg = attributeData.NamedArguments.SingleOrDefault(kvp => kvp.Key == nameof(MaxLength)).Value;
            TypedConstant assumeGeneratedEnumTypeArg = attributeData.NamedArguments.SingleOrDefault(kvp => kvp.Key == nameof(AssumeGeneratedEnumType)).Value;
            TypedConstant validationMethodArg = attributeData.NamedArguments.SingleOrDefault(kvp => kvp.Key == nameof(ValidationMethod)).Value;

#pragma warning disable IDE0075 // Conditional expression can be simplified
            // Get the actual value of every named argument which was specified
            Length = !lengthArg.IsNull ? (int)lengthArg.Value! : Default.Length;
            LengthMember = !lengthMemberArg.IsNull ? (string)lengthMemberArg.Value! : Default.LengthMember;
            IsBigEndian = !isBigEndianArg.IsNull ? (bool)isBigEndianArg.Value! : Default.IsBigEndian;
            MinLength = !minLengthArg.IsNull ? (int)minLengthArg.Value! : Default.MinLength;
            MaxLength = !maxLengthArg.IsNull ? (int)maxLengthArg.Value! : Default.MaxLength;
            AssumeGeneratedEnumType = !assumeGeneratedEnumTypeArg.IsNull
                ? (string)assumeGeneratedEnumTypeArg.Value!
                : Default.AssumeGeneratedEnumType;
            ValidationMethod = !validationMethodArg.IsNull
                ? (string)validationMethodArg.Value!
                : Default.ValidationMethod;
#pragma warning restore IDE0075 // Conditional expression can be simplified
        }

        public static class Default
        {
            public const int Offset = -1;
            public const string OffsetMember = "";
            public const int Length = -1;
            public const string LengthMember = "";
            public const bool IsBigEndian = false;
            public const int MinLength = -1;
            public const int MaxLength = -1;
            public const string AssumeGeneratedEnumType = "";
            public const string ValidationMethod = "";

            public static string AsSourceString(string memberName)
            {
                switch (memberName)
                {
                    case nameof(Offset):
                        return Offset.ToString();
                    case nameof(OffsetMember):
                        return OffsetMember.Length > 0 ? $"\"{OffsetMember}\"" : "string.Empty";
                    case nameof(Length):
                        return Length.ToString();
                    case nameof(LengthMember):
                        return LengthMember.Length > 0 ? $"\"{LengthMember}\"" : "string.Empty";
                    case nameof(IsBigEndian):
                        return IsBigEndian.ToString().ToLower();
                    case nameof(MinLength):
                        return MinLength.ToString();
                    case nameof(MaxLength):
                        return MaxLength.ToString();
                    case nameof(AssumeGeneratedEnumType):
                        return AssumeGeneratedEnumType.Length > 0 ? $"\"{AssumeGeneratedEnumType}\"" : "string.Empty";
                    case nameof(ValidationMethod):
                        return ValidationMethod.Length > 0 ? $"\"{ValidationMethod}\"" : "string.Empty";
                    default:
                        throw new ArgumentException($"No member with name: {memberName}");
                }
            }
        }
    }
}
