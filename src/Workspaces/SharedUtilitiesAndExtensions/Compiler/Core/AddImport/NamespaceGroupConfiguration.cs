// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Microsoft.CodeAnalysis.AddImport;

internal class NamespaceGroupConfiguration(IEnumerable<NamespaceGroupNode> groups)
{
    public static readonly NamespaceGroupNode Default = new MatchAllNamespaceGroupNode(1);
    public static readonly NamespaceGroupConfiguration DefaultConfig = new(new[] { new MatchAllNamespaceGroupNode(1) });

    public IReadOnlyCollection<NamespaceGroupNode> Groups { get; } = EnsureContainsCatchAll(groups);

    private static IReadOnlyCollection<NamespaceGroupNode> EnsureContainsCatchAll(IEnumerable<NamespaceGroupNode> groups)
        => (groups.Any(x => x is MatchAllNamespaceGroupNode) ? groups : groups.Append(Default)).ToList();

    public NamespaceGroupNode Match(ReadOnlySpan<string> input)
    {

        MatchResult? result = null;
        NamespaceGroupNode? mostSpecificGroup = null;

        foreach (var group in Groups)
        {
            if (group.Match(input) is { Success: true } match && match.Depth > (result?.Depth ?? -1))
            {
                result = match;
                mostSpecificGroup = group;
            }
        }

        return mostSpecificGroup ?? throw new Exception("This should not happen");
    }

    public override string ToString() => string.Join(" ; ", Groups.Select(x => x.ToString()));
}
