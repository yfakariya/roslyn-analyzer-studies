// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

#if !CODEANALYSIS_V3_OR_BETTER
namespace Microsoft.CodeAnalysis
{
    using System.Collections.Generic;

    internal sealed class SymbolEqualityComparer : IEqualityComparer<ISymbol?>
    {
        private SymbolEqualityComparer()
        {
        }

        public bool Equals(ISymbol? x, ISymbol? y)
            => x is null
                ? y is null
                : x.Equals(y);

        public int GetHashCode(ISymbol? obj)
            => obj?.GetHashCode() ?? 0;

        public static SymbolEqualityComparer Default { get; } = new();
    }
}
#endif
