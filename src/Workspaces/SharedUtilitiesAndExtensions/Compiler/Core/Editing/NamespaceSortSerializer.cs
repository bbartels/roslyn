// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Immutable;
using Roslyn.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.AddImport;

namespace Microsoft.CodeAnalysis.Editing;

internal class NamespaceSortConfigParser
{
    private const char Separator = ';';
    private const char MatchAll = '*';
    private const char Navigation = '.';
    private static Regex TrimWhitespaces() => new Regex(@"\s+");

    private static NamespaceGroupNode ParseGroup(ReadOnlySpan<char> groupString) => groupString switch
    {
        [MatchAll, .. ReadOnlySpan<char>] => new MatchAllNamespaceGroupNode(1),
        [.. ReadOnlySpan<char> t] when t.IndexOf(Navigation) is int index and >= 0
            => new IntermediaryIdentifierNamespaceGroupNode(new(t[..index].ToString()), ParseGroup(t[(index + 1)..])),
        [.. ReadOnlySpan<char> t] => new EndIdentifierNamespaceGroupNode(new(t.ToString()))
    };

    private static string SanitizeInput(string input) => TrimWhitespaces().Replace(input, string.Empty);

    public static Optional<NamespaceGroupConfiguration> ParseNamespaceSortOrder(string sortOrder)
        => new NamespaceGroupConfiguration(SanitizeInput(sortOrder).Split(Separator).Select(x => ParseGroup(x.AsSpan())));

    public static string SerializeNamespaceSortOrder(NamespaceGroupConfiguration sortOrder) => sortOrder.ToString();
}
