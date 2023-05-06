// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

using System.Collections.Immutable;
using System.Diagnostics;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;

namespace AnalyzerStudies.CompletionOnNormalPathAnalysis;

/// <summary>
/// Abstract complete-on-normal-path data tracked by <see cref="CompleteOnNormalPathAnalysis"/>.
/// It contains the set of <see cref="IOperation"/>s that complete an associated target <see cref="AbstractLocation"/> and
/// the completion <see cref="Kind"/>.
/// </summary>
public sealed class CompleteOnNormalPathAbstractValue : CacheBasedEquatable<CompleteOnNormalPathAbstractValue>
{
	public static readonly CompleteOnNormalPathAbstractValue NonTarget = new(CompleteOnNormalPathAbstractValueKind.NonTarget);

	public static readonly CompleteOnNormalPathAbstractValue Invalid = new(CompleteOnNormalPathAbstractValueKind.Invalid);

	public static readonly CompleteOnNormalPathAbstractValue NotCompleted = new(CompleteOnNormalPathAbstractValueKind.NotCompleted);

	public static readonly CompleteOnNormalPathAbstractValue Unknown = new(CompleteOnNormalPathAbstractValueKind.Unknown);

	public ImmutableHashSet<IOperation> CompletingOrEscapingOperations { get; }

	public CompleteOnNormalPathAbstractValueKind Kind { get; }

	private CompleteOnNormalPathAbstractValue(CompleteOnNormalPathAbstractValueKind kind)
		: this(ImmutableHashSet<IOperation>.Empty, kind)
	{
	}

	internal CompleteOnNormalPathAbstractValue(
		ImmutableHashSet<IOperation> completingOrEscapingOperations,
		CompleteOnNormalPathAbstractValueKind kind
	)
	{
		VerifyArguments(completingOrEscapingOperations, kind);
		CompletingOrEscapingOperations = completingOrEscapingOperations;
		Kind = kind;
	}

	internal CompleteOnNormalPathAbstractValue WithNewCompletionOperation(IOperation completingOperation)
	{
		Debug.Assert(Kind != CompleteOnNormalPathAbstractValueKind.NonTarget);

		return
			new CompleteOnNormalPathAbstractValue(
				CompletingOrEscapingOperations.Add(completingOperation),
				CompleteOnNormalPathAbstractValueKind.Completed
			);
	}

	internal CompleteOnNormalPathAbstractValue WithNewEscapingOperation(IOperation escapingOperation)
	{
		Debug.Assert(Kind != CompleteOnNormalPathAbstractValueKind.NonTarget);
		Debug.Assert(Kind != CompleteOnNormalPathAbstractValueKind.Unknown);

		return
			new CompleteOnNormalPathAbstractValue(
				ImmutableHashSet.Create(escapingOperation),
				 CompleteOnNormalPathAbstractValueKind.Escaped
			);
	}

	[Conditional("DEBUG")]
	private static void VerifyArguments(
		ImmutableHashSet<IOperation> completingOrEscapingOperations, CompleteOnNormalPathAbstractValueKind kind)
	{
		switch (kind)
		{
			case CompleteOnNormalPathAbstractValueKind.NonTarget:
			case CompleteOnNormalPathAbstractValueKind.NotCompleted:
			case CompleteOnNormalPathAbstractValueKind.Invalid:
			case CompleteOnNormalPathAbstractValueKind.Unknown:
			{
				Debug.Assert(completingOrEscapingOperations.IsEmpty);
				break;
			}
			case CompleteOnNormalPathAbstractValueKind.Escaped:
			case CompleteOnNormalPathAbstractValueKind.Completed:
			case CompleteOnNormalPathAbstractValueKind.MaybeCompleted:
			{
				Debug.Assert(!completingOrEscapingOperations.IsEmpty);
				break;
			}
		}
	}
	protected override void ComputeHashCodeParts(ref RoslynHashCode hashCode)
	{
		hashCode.Add(HashUtilities.Combine(CompletingOrEscapingOperations));
		hashCode.Add(Kind.GetHashCode());
	}

	protected override bool ComputeEqualsByHashCodeParts(CacheBasedEquatable<CompleteOnNormalPathAbstractValue> obj)
	{
		var other = obj as CompleteOnNormalPathAbstractValue;
		return
			other is not null &&
			Kind == other.Kind &&
			CompletingOrEscapingOperations.SetEquals(other.CompletingOrEscapingOperations);
	}
}
