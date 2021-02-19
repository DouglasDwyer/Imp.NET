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
                    true);

    }
}
