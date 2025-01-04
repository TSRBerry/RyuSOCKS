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
    // NOTE: The default values should be the same as the ones specified in PacketFieldAttribute.
    //       See: PacketGenerator.cs
    internal class PacketFieldAttributeData
    {
        public int Offset { get; } = -1;
        public string OffsetMember { get; } = string.Empty;
        public int Length { get; }
        public string LengthMember { get; }
        public bool IsBigEndian { get; }
        public int MinLength { get; }
        public int MaxLength { get; }
        public string AssumeGeneratedEnumType { get; }
        public string ValidationMethod { get; }

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

            // Get the actual value of every named argument which was specified
            Length = !lengthArg.IsNull ? (int)lengthArg.Value! : -1;
            LengthMember = !lengthMemberArg.IsNull ? (string)lengthMemberArg.Value! : string.Empty;
            IsBigEndian = !isBigEndianArg.IsNull && (bool)isBigEndianArg.Value!;
            MinLength = !minLengthArg.IsNull ? (int)minLengthArg.Value! : -1;
            MaxLength = !maxLengthArg.IsNull ? (int)maxLengthArg.Value! : -1;
            AssumeGeneratedEnumType = !assumeGeneratedEnumTypeArg.IsNull
                ? (string)assumeGeneratedEnumTypeArg.Value!
                : string.Empty;
            ValidationMethod = !validationMethodArg.IsNull
                ? (string)validationMethodArg.Value!
                : string.Empty;
        }
    }
}
