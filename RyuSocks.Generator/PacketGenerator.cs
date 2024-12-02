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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace RyuSocks.Generator
{
    // TODO: Write an analyzer for PacketFieldAttribute
    //         - make sure (Offset and OffsetMember) and (Length and LengthMember) aren't used at the same time
    //         - check whether the attribute is used on a property within a class/subclass that extends Packet
    // FIXME: Get accessibility modifiers of property accessors.
    // FIXME: Pay attention to the endianness, don't assume little endian.

    [Generator]
    public class PacketGenerator : IIncrementalGenerator
    {
        private const string Namespace = "RyuSocks.Packets";
        private const string AbstractClassName = "Packet";
        private const string PacketFieldAttributeName = "PacketFieldAttribute";
        private const string PacketBytesFieldName = "Bytes";

        private static readonly ImmutableDictionary<ActualType, ActualTypeInfo> _typeInfo = new Dictionary<ActualType, ActualTypeInfo>
        {
            { ActualType.Int16, new ActualTypeInfo(2, "BitConverter.ToInt16") },
            { ActualType.UInt16, new ActualTypeInfo(2, "BitConverter.ToUInt16") },
            { ActualType.Int32, new ActualTypeInfo(4, "BitConverter.ToInt32") },
            { ActualType.UInt32, new ActualTypeInfo(4, "BitConverter.ToUInt32") },
            { ActualType.Int64, new ActualTypeInfo(8, "BitConverter.ToInt64") },
            { ActualType.UInt64, new ActualTypeInfo(8, "BitConverter.ToUInt64") },
        }.ToImmutableDictionary();

#region SourceText
        private const string PacketFieldAttributeText = @"
using System;

namespace %NAMESPACE%
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false)]
    [System.Diagnostics.Conditional(""RyuSocks_PacketGenerator_DEBUG"")]
    public sealed class %ATTRIBUTE_NAME% : Attribute
    {
        private int offset = -1;
        private string offsetMember = string.Empty;

        /// <summary>
        /// Marks this property as a field of the packet.
        /// The containing class needs to extend <see cref=""%ABSTRACT_CLASS_NAME%""/>.
        /// </summary>
        /// <param name=""offset"">The offset of this field in the packet.</param>
        public %ATTRIBUTE_NAME%([System.ComponentModel.DataAnnotations.Range(0, int.MaxValue)] int offset)
        {
            this.offset = offset;
        }

        /// <summary>
        /// Marks this property as a field of the packet.
        /// The containing class needs to extend <see cref=""%ABSTRACT_CLASS_NAME%""/>.
        /// </summary>
        /// <param name=""offsetMember"">The name of the member which specifies the offset of this field in the packet.</param>
        public %ATTRIBUTE_NAME%(string offsetMember)
        {
            this.offsetMember = offsetMember;
        }

        /// <summary>
        /// The length of this field.
        /// Only required if it can't be determined from the property type.
        /// </summary>
        public int Length { get; set; } = 0;

        /// <summary>
        /// The name of the member which specifies the length of this field.
        /// Only required if it can't be determined from the property type.
        /// </summary>
        public string LengthMember { get; set; } = string.Empty;

        /// <summary>
        /// Whether this field is big endian.
        /// </summary>
        public bool IsBigEndian { get; set; } = false;

        public int Offset => offset;
        public string OffsetMember => offsetMember;
    }
}
";

        private const string AbstractPacketClassText = @"
using System;

namespace %NAMESPACE%
{
    public abstract partial class %CLASS_NAME%
    {
        /// <summary>
        /// The contents of the packet.
        /// </summary>
        public byte[] %BYTES_FIELD_NAME% { get; protected set; }

        /// <inheritdoc cref=""%BYTES_FIELD_NAME%""/>
        public Span<byte> AsSpan() => %BYTES_FIELD_NAME%;

        protected %CLASS_NAME%() { }
        
        protected %CLASS_NAME%(byte[] bytes)
        {
            %BYTES_FIELD_NAME% = bytes;
        }
    }
}
";
#endregion

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Register the attribute source
            context.RegisterPostInitializationOutput(static postContext => postContext.AddSource(
                    $"{PacketFieldAttributeName}.g.cs",
                    PacketFieldAttributeText
                        .Replace("%NAMESPACE%", Namespace)
                        .Replace("%ATTRIBUTE_NAME%", PacketFieldAttributeName)
                        .Replace("%ABSTRACT_CLASS_NAME%", AbstractClassName)
                )
            );
            // Register the abstract packet class source
            context.RegisterPostInitializationOutput(static postContext => postContext.AddSource(
                    $"{AbstractClassName}.g.cs",
                    AbstractPacketClassText
                        .Replace("%NAMESPACE%", Namespace)
                        .Replace("%CLASS_NAME%", AbstractClassName)
                        .Replace("%BYTES_FIELD_NAME%", PacketBytesFieldName)
                )
            );

            // Create an incremental value provider using the generated attribute
            var packetFieldProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
                $"{Namespace}.{PacketFieldAttributeName}",
                IsValidPacketFieldProperty,
                TransformPacketFieldProperty
            );

            // TODO: Figure out how to group by a field/property, so code can be generated for all classes in parallel,
            //       while every class gets only one generated file.
            //       Not sure if that's actually desirable.
            //       Looking for something similar to LinQ GroupBy
            context.RegisterSourceOutput(packetFieldProvider, ProduceSourceCode);
        }

        /// <summary>
        /// Check whether <paramref name="syntaxNode"/> is a valid packet field property.
        /// </summary>
        /// <param name="syntaxNode">The syntax node to check.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns><c>True</c> if <paramref name="syntaxNode"/> is a valid packet field property.</returns>
        private static bool IsValidPacketFieldProperty(SyntaxNode syntaxNode, CancellationToken cancellationToken) =>
            syntaxNode is PropertyDeclarationSyntax { Parent: ClassDeclarationSyntax { BaseList.Types.Count: > 0 } classDeclaration } propertyDeclaration
            && propertyDeclaration.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.PartialKeyword))
            && classDeclaration.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.PartialKeyword));

        private static PacketFieldModel TransformPacketFieldProperty(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
        {
            var propertySymbol = (context.TargetSymbol as IPropertySymbol)!;
            var attributeData = context.Attributes[0];
            var classSymbol = context.TargetSymbol.ContainingSymbol;

            if (attributeData.ConstructorArguments.Length == 0)
            {
                throw new InvalidOperationException($"Attribute constructor doesn't have arguments: {attributeData.AttributeClass?.Name}");
            }

            // Parse attribute data

            int offset = -1;
            string offsetMember = string.Empty;

            switch (attributeData.ConstructorArguments[0].Type!.Name)
            {
                case "String":
                    offsetMember = (string)attributeData.ConstructorArguments[0].Value!;
                    break;
                case "Int32":
                    offset = (int)attributeData.ConstructorArguments[0].Value!;
                    break;
                default:
                    throw new InvalidOperationException($"Attribute constructor argument type is not valid: {attributeData.ConstructorArguments[0].Type!.Name}");
            }

            TypedConstant lengthArg = attributeData.NamedArguments.SingleOrDefault(kvp => kvp.Key == "Length").Value;
            TypedConstant lengthMemberArg = attributeData.NamedArguments.SingleOrDefault(kvp => kvp.Key == "LengthMember").Value;
            TypedConstant isBigEndianArg = attributeData.NamedArguments.SingleOrDefault(kvp => kvp.Key == "IsBigEndian").Value;
            int length = !lengthArg.IsNull ? (int)lengthArg.Value! : -1;
            string lengthMember = !lengthMemberArg.IsNull ? (string)lengthMemberArg.Value! : string.Empty;
            bool isBigEndian = !isBigEndianArg.IsNull && (bool)isBigEndianArg.Value!;

            // Get information about the property type

            ITypeSymbol actualTypeSymbol = propertySymbol.Type;
            bool isArray = propertySymbol.Type.TypeKind == TypeKind.Array;
            bool isEnum = propertySymbol.Type.TypeKind == TypeKind.Enum;
            bool isStruct = IsActualStruct(propertySymbol.Type);
            if (!Enum.TryParse(actualTypeSymbol.Name, out ActualType actualType))
            {
                actualType = ActualType.NamedType;
            }

            if (isArray)
            {
                actualTypeSymbol = (propertySymbol.Type as IArrayTypeSymbol)!.ElementType;
                isEnum = actualTypeSymbol.TypeKind == TypeKind.Enum;
                isStruct = IsActualStruct(actualTypeSymbol);

                if (!isEnum && !isStruct && actualTypeSymbol.TypeKind != TypeKind.Class)
                {
                    actualType = (ActualType)Enum.Parse(typeof(ActualType), actualTypeSymbol.Name, true);
                }
            }

            if (isEnum)
            {
                actualType = (ActualType)Enum.Parse(typeof(ActualType), (actualTypeSymbol as INamedTypeSymbol)!.EnumUnderlyingType!.Name, true);
            }

            // Get imports
            // NOTE: This is only necessary because property types could be generated by other source generators,
            //       which aren't available at this point.

            // TODO: Add something like "AssumeEnum" to PacketFieldAttribute
            //       which could be used when propertySymbol.Type.TypeKind == TypeKind.Error

            CompilationUnitSyntax rootNode = context.TargetNode.SyntaxTree.GetCompilationUnitRoot();
            string[] imports = rootNode.Usings.ToString().Split('\n');
            for (int i = 0; i < imports.Length; i++)
            {
                imports[i] = imports[i].Trim();
            }

            // Create model from data
            return new PacketFieldModel(
                offset,
                offsetMember,
                length,
                lengthMember,
                isBigEndian,
                new FieldTypeModel(
                    propertySymbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    isArray,
                    isEnum,
                    isStruct,
                    actualType
                ),
                propertySymbol.Name,
                propertySymbol.DeclaredAccessibility,
                classSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                imports
            );
        }

#region Methods to generate accessors
        private static void GenerateSimpleStringAccessor(CodeBuilder source, PacketFieldModel packetField, bool isGetter)
        {
            // FIXME: This implementation assumes strings are always ASCII
            if (isGetter)
            {
                if (packetField.FieldType.IsEnum)
                {
                    source.AppendLine($"Enum.Parse<{packetField.FieldType.Name}>(Encoding.ASCII.GetString({PacketBytesFieldName}, {GetFieldOffset(packetField)}, {GetFieldLength(packetField)}), true);");
                    return;
                }

                source.AppendLine($"return Encoding.ASCII.GetString({PacketBytesFieldName}, {GetFieldOffset(packetField)}, {GetFieldLength(packetField)});");
            }
            else
            {
                string valueParameter = packetField.FieldType.IsEnum
                    ? $"Enum.GetName<{packetField.FieldType.Name}>(value)"
                    : "value";

                if (packetField.Length <= 0)
                {
                    if (packetField.FieldType.IsEnum)
                    {
                        source.AppendLine($"string valueString = {valueParameter};");
                        valueParameter = "valueString";
                    }

                    // TODO: Add type cast if necessary
                    source.AppendLine($"this.{packetField.LengthMember} = {valueParameter}.Length;");
                    source.AppendLine($"Encoding.ASCII.GetBytes({valueParameter}, {PacketBytesFieldName}.AsSpan({GetFieldOffset(packetField)}, this.{packetField.LengthMember}));");
                }
                else
                {
                    source.AppendLine($"Encoding.ASCII.GetBytes({valueParameter}, {PacketBytesFieldName}.AsSpan({GetFieldOffset(packetField)}, {GetFieldLength(packetField)}));");
                }
            }
        }

        private static void GenerateSimpleAccessor(CodeBuilder source, PacketFieldModel packetField, bool isGetter)
        {
            string maybeCast = packetField.FieldType.IsEnum ? $"({packetField.FieldType.Name})" : string.Empty;

            switch (packetField.FieldType.ActualType)
            {
                case ActualType.Byte:
                    if (isGetter)
                    {
                        source.AppendLine($"return {maybeCast}{PacketBytesFieldName}[{GetFieldOffset(packetField)}];");
                    }
                    else
                    {
                        string maybeCastValue = packetField.FieldType.IsEnum ? "(byte)" : string.Empty;
                        source.AppendLine($"{PacketBytesFieldName}[{GetFieldOffset(packetField)}] = {maybeCastValue}value;");
                    }
                    break;
                case ActualType.SByte:
                    if (isGetter)
                    {
                        source.AppendLine($"return {maybeCast}(sbyte){PacketBytesFieldName}[{GetFieldOffset(packetField)}];");
                    }
                    else
                    {
                        source.AppendLine($"{PacketBytesFieldName}[{GetFieldOffset(packetField)}] = (byte)value;");
                    }
                    break;
                case ActualType.Int16:
                case ActualType.UInt16:
                case ActualType.Int32:
                case ActualType.UInt32:
                case ActualType.Int64:
                case ActualType.UInt64:
                    var typeInfo = _typeInfo[packetField.FieldType.ActualType];
                    if (isGetter)
                    {
                        if (packetField.IsBigEndian)
                        {
                            source.AppendLine($"Span<byte> valueSpan = {PacketBytesFieldName}.AsSpan({GetFieldOffset(packetField)}, {typeInfo.Length});");
                            source.AppendLine("valueSpan.Reverse();");
                            source.AppendLine($"return {maybeCast}{typeInfo.ConverterMethodName}(valueSpan);");
                        }
                        else
                        {
                            source.AppendLine($"return {maybeCast}{typeInfo.ConverterMethodName}({PacketBytesFieldName}.AsSpan({GetFieldOffset(packetField)}, {typeInfo.Length}));");
                        }
                    }
                    else
                    {
                        string valueParameter = packetField.FieldType.IsEnum
                            ? $"({TypeStringFromActualType(packetField.FieldType.ActualType)})value"
                            : "value";

                        if (packetField.IsBigEndian)
                        {
                            source.AppendLine($"byte[] valueBytes = BitConverter.GetBytes({valueParameter});");
                            source.AppendLine("Array.Reverse(valueBytes);");
                            source.AppendLine($"valueBytes.CopyTo({PacketBytesFieldName}.AsSpan({GetFieldOffset(packetField)}, {typeInfo.Length}));");
                        }
                        else
                        {
                            source.AppendLine($"BitConverter.GetBytes({valueParameter}).CopyTo({PacketBytesFieldName}.AsSpan({GetFieldOffset(packetField)}, {typeInfo.Length}));");
                        }
                    }
                    break;
                case ActualType.NamedType:
                    // Only deal with strings here
                    GenerateSimpleStringAccessor(source, packetField, isGetter);
                    break;
                default:
                    throw new InvalidOperationException($"Unable to generate simple accessor for type: {packetField.FieldType.Name}({packetField.FieldType.ActualType})");
            }

        }

        // TODO: Find a way to deal with classes and structs properly
        //       This probably requires a model change
        //       For classes:
        //         - getter: Require a specific constructor?
        //         - setter: Require a specific method/property to get access to a Span<byte> or byte[]?
        private static void GenerateAccessor(CodeBuilder source, PacketFieldModel packetField, bool isGetter)
        {
            if (packetField.FieldType.IsArray)
            {
                // TODO: Deal with arrays
                //       Arrays could contain structs, enums or classes
                source.AppendLine("// TODO: arrays");
                return;
            }

            if (packetField.FieldType.IsStruct)
            {
                // TODO: Deal with structs
                source.AppendLine("// TODO: structs");
                return;
            }

            // Deal with simple types: enums, strings and integral numeric types
            if (packetField.FieldType is { ActualType: ActualType.NamedType, Name: "String" }
                || packetField.FieldType.ActualType != ActualType.NamedType)
            {
                GenerateSimpleAccessor(source, packetField, isGetter);
                return;
            }

            if (packetField.FieldType.ActualType == ActualType.NamedType)
            {
                // TODO: Deal with classes
                source.AppendLine("// TODO: classes");
            }
        }
#endregion

        private static void ProduceSourceCode(SourceProductionContext context, PacketFieldModel packetField)
        {
            string namespaceName = string.Empty;
            string className = packetField.ClassName;

            if (className.Contains("."))
            {
                namespaceName = className.Substring(0, className.LastIndexOf(".", StringComparison.Ordinal));
                className = className.Substring(className.LastIndexOf(".", StringComparison.Ordinal) + 1);
            }

            if (namespaceName.StartsWith("global::"))
            {
                namespaceName = namespaceName.Substring(8);
            }

            // Begin building the generated source
            CodeBuilder source = new();
            string[] requiredImports = ["using System;", "using System.Text;"];

            // Add imports

            foreach (var importString in packetField.Imports)
            {
                source.AppendLine(importString);
            }

            foreach (var requiredImportString in requiredImports)
            {
                if (!packetField.Imports.Contains(requiredImportString))
                {
                    source.AppendLine(requiredImportString);
                }
            }

            source.AppendLine();

            // Add namespace if necessary

            if (namespaceName.Length > 0)
            {
                source.EnterScope($"namespace {namespaceName}");
            }

            // Add class and property

            source.EnterScope($"partial class {className}");
            source.EnterScope($"{GetAccessModifierString(packetField.PropertyAccessModifier)} partial {packetField.FieldType.Name} {packetField.PropertyName}");

            // Add code for getter
            source.EnterScope("get");
            GenerateAccessor(source, packetField, true);
            source.LeaveScope();

            // Add code for setter
            source.EnterScope("set");
            GenerateAccessor(source, packetField, false);
            source.LeaveScope();

            // Leave property and class scope
            source.LeaveScope();
            source.LeaveScope();

            // Leave namespace scope if necessary
            if (namespaceName.Length > 0)
            {
                source.LeaveScope();
            }

            // Add the generated source
            string fullyQualifiedName = namespaceName.Length > 0 ? $"{namespaceName}.{className}" : className;
            context.AddSource($"{fullyQualifiedName}.{packetField.PropertyName}.g.cs", source.ToString());
        }

        private static bool IsActualStruct(ITypeSymbol typeSymbol)
        {
            if (typeSymbol.TypeKind != TypeKind.Struct)
            {
                return false;
            }

            return typeSymbol.SpecialType is < SpecialType.System_Boolean or > SpecialType.System_String;
        }

        private enum ActualType
        {
            NamedType,
            SByte,
            Byte,
            Int16,
            UInt16,
            Int32,
            UInt32,
            Int64,
            UInt64,
        }

        private static string TypeStringFromActualType(ActualType actualType)
        {
            return actualType switch
            {
                ActualType.SByte => "sbyte",
                ActualType.Byte => "byte",
                ActualType.Int16 => "short",
                ActualType.UInt16 => "ushort",
                ActualType.Int32 => "int",
                ActualType.UInt32 => "uint",
                ActualType.Int64 => "long",
                ActualType.UInt64 => "ulong",
                _ => throw new InvalidOperationException($"Couldn't get type string from {nameof(ActualType)}: {actualType}"),
            };
        }
        
        private record struct ActualTypeInfo(int Length, string ConverterMethodName);

        private record struct FieldTypeModel(
            string Name,
            bool IsArray,
            bool IsEnum,
            bool IsStruct,
            ActualType ActualType
        );

        private record struct PacketFieldModel(
            int Offset,
            string OffsetMember,
            int Length,
            string LengthMember,
            bool IsBigEndian,
            FieldTypeModel FieldType,
            string PropertyName,
            Accessibility PropertyAccessModifier,
            string ClassName,
            string[] Imports
        );

        private static string GetAccessModifierString(Accessibility accessModifier)
        {
            return accessModifier switch
            {
                Accessibility.NotApplicable => string.Empty,
                Accessibility.Private => "private",
                Accessibility.ProtectedAndInternal => "internal protected",
                Accessibility.Protected => "protected",
                Accessibility.Internal => "internal",
                Accessibility.Public => "public",
                _ => throw new InvalidOperationException($"Couldn't get access modifier string for: {accessModifier}"),
            };
        }

        private static string GetFieldOffset(PacketFieldModel model)
        {
            if (model.Offset >= 0)
            {
                return model.Offset.ToString();
            }

            if (model.OffsetMember.Length == 0)
            {
                throw new InvalidOperationException($"No {nameof(model.OffsetMember)} found.");
            }

            return $"this.{model.OffsetMember}";
        }

        private static string GetFieldLength(PacketFieldModel model)
        {
            if (model.Length >= 0)
            {
                return model.Length.ToString();
            }

            if (model.LengthMember.Length == 0)
            {
                throw new InvalidOperationException($"No {nameof(model.LengthMember)} found.");
            }

            return $"this.{model.LengthMember}";
        }
    }
}
