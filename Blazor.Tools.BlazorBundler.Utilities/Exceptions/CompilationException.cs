using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Blazor.Tools.BlazorBundler.Utilities.Exceptions
{
    public class CompilationException : Exception
    {
        public IEnumerable<Diagnostic> Diagnostics { get; }

        public CompilationException(IEnumerable<Diagnostic> diagnostics)
            : base("Compilation failed.")
        {
            Diagnostics = diagnostics;
        }

        public CompilationException(IEnumerable<Diagnostic> diagnostics, string message)
            : base(message)
        {
            Diagnostics = diagnostics;
        }

        public CompilationException(IEnumerable<Diagnostic> diagnostics, string message, Exception innerException)
            : base(message, innerException)
        {
            Diagnostics = diagnostics;
        }
    }

}
