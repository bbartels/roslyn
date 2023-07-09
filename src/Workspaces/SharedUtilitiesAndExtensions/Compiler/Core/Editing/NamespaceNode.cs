// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

internal class NamespaceIdentifier
{
    public string Name { get; }

    public NamespaceIdentifier(string identifier)
        => Name = identifier is { Length: > 0 } ? identifier : throw new Exception($"Invalid namespace identifier: {identifier}");
}

internal record struct MatchResult(bool Success, int Depth);

internal abstract class NamespaceGroupNode
{
    public MatchResult Match(ReadOnlySpan<string> @namespace) => Match(@namespace, new(false, 0));
    internal abstract MatchResult Match(ReadOnlySpan<string> @namespace, MatchResult result);
    public abstract override string ToString();
}

internal class IntermediaryIdentifierNamespaceGroupNode(NamespaceIdentifier identifier, NamespaceGroupNode next) : NamespaceGroupNode
{
    public NamespaceIdentifier Identifier { get; } = identifier;
    public NamespaceGroupNode Next { get; } = next;

    internal override MatchResult Match(ReadOnlySpan<string> @namespace, MatchResult result) => @namespace switch
    {
        [string identifier, .. ReadOnlySpan<string> t] when identifier == Identifier.Name
            => Next.Match(t, result with { Depth = result.Depth + 1 }),
        _ => result with { Depth = result.Depth + 1, Success = false }
    };
    public override string ToString() => $"{Identifier.Name}.{Next}";
}

internal abstract class EndNamespaceGroupNode : NamespaceGroupNode { }

internal class EndIdentifierNamespaceGroupNode(NamespaceIdentifier identifier) : EndNamespaceGroupNode
{
    public NamespaceIdentifier Identifier { get; } = identifier;

    internal override MatchResult Match(ReadOnlySpan<string> @namespace, MatchResult result) => @namespace switch
    {
        [string identifier] when identifier == Identifier.Name => result with { Success = true, Depth = result.Depth + 1 },
        _ => result with { Success = false }
    };

    public override string ToString() => Identifier.Name;
}

internal class MatchAllNamespaceGroupNode(int rank) : EndNamespaceGroupNode
{
    public int Rank { get; } = rank;

    internal override MatchResult Match(ReadOnlySpan<string> @namespace, MatchResult result)
        => result with { Success = true, Depth = result.Depth };
    public override string ToString() => "*";
}
