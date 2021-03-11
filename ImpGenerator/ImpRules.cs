using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace DouglasDwyer.ImpGenerator
{
    public static class ImpRules
    {
        public static readonly DiagnosticDescriptor
            UnreliableMethodReturnTypeError =
                new DiagnosticDescriptor(
                    "IMP0001",
                    "Unreliable method return type",
                    "Method {0} is marked unreliable, but returns {1}. Unreliable methods must return void.",
                    "DouglasDwyer.Imp",
                    DiagnosticSeverity.Error,
                    true),
            UnreliableMethodReferenceParameterError =
                new DiagnosticDescriptor(
                    "IMP0002",
                    "Unreliable method parameter type",
                    "Method {0} is marked unreliable, but has reference-typed parameter {1}. Unreliable methods cannot take reference types as arguments.",
                    "DouglasDwyer.Imp",
                    DiagnosticSeverity.Error,
                    true),
            UnreliableMethodValueParameterError =
                new DiagnosticDescriptor(
                    "IMP0003",
                    "Unreliable method parameter type",
                    "Method {0} is marked unreliable, but has struct parameter {1} that contains reference-typed fields. Unreliable methods cannot take structs that contain reference types as arguments.",
                    "DouglasDwyer.Imp",
                    DiagnosticSeverity.Error,
                    true),
            CallingClientParameterTypeError =
                new DiagnosticDescriptor(
                    "IMP0004",
                    "CallingClient parameter type",
                    "Parameter {1} of method {0} is marked as calling client, but the parameter type does not inherit from DouglasDwyer.Imp.IImpClient.",
                    "DouglasDwyer.Imp",
                    DiagnosticSeverity.Error,
                    true),
            CallingClientParameterCountError =
                new DiagnosticDescriptor(
                    "IMP0005",
                    "CallingClient parameter count",
                    "Parameter {1} of method {0} is marked as calling client, but {0} already has a calling client parameter.",
                    "DouglasDwyer.Imp",
                    DiagnosticSeverity.Error,
                    true),
            NestedSharedClassError =
                new DiagnosticDescriptor(
                    "IMP0006",
                    "Nested shared class",
                    "Class {0} is marked as shared, but it is a nested member of class {1}. Shared classes cannot be nested.",
                    "DouglasDwyer.Imp",
                    DiagnosticSeverity.Error,
                    true),
            InvalidTypeNameError =
                new DiagnosticDescriptor(
                    "IMP0007",
                    "Invalid shared type name",
                    "Class {0} is marked as shared, but its shared interface name '{1}' is not a valid C# type name.",
                    "DouglasDwyer.Imp",
                    DiagnosticSeverity.Error,
                    true),
            PassByReferenceParameterWarning =
                new DiagnosticDescriptor(
                    "IMP0008",
                    "Shared method pass-by-reference parameter",
                    "Method {0} can be called remotely, but has pass-by-reference parameter {1}. Remote method invocation with in, out, and ref parameters is not supported, and will throw an exception.",
                    "DouglasDwyer.Imp",
                    DiagnosticSeverity.Warning,
                    true),
            PassByReferenceReturnTypeWarning =
                new DiagnosticDescriptor(
                    "IMP0009",
                    "Shared method pass-by-reference return type",
                    "Method {0} can be called remotely, but has a pass-by-reference return type. Remote method invocation with ref returns is not supported, and will throw an exception.",
                    "DouglasDwyer.Imp",
                    DiagnosticSeverity.Warning,
                    true);

    }
}
