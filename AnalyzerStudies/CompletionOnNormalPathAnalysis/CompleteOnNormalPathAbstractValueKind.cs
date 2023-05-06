// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;

namespace AnalyzerStudies.CompletionOnNormalPathAnalysis;

/// <summary>
/// Abstract completion-on-normal-path value for <see cref="AbstractLocation"/>/<see cref="IOperation"/> tracked by <see cref="CompleteOnNormalPathAnalysis"/>.
/// </summary>
public enum CompleteOnNormalPathAbstractValueKind
{
	/// <summary>
	/// Indicates locations that are not target type.
	/// </summary>
	NonTarget,

	/// <summary>
	/// Indicates a value for target locations that are not feasible on the given program path.
	/// For example,
	/// <code>
	///     var x = flag ? new Target() : null;
	///     if (x == null)
	///     {
	///         // Target allocation above cannot exist on this code path.
	///     }
	/// </code>
	/// </summary>
	Invalid,

	/// <summary>
	/// Indicates target locations that are not completed.
	/// </summary>
	NotCompleted,

	/// <summary>
	/// Indicates target locations that have escaped the declaring method's scope.
	/// For example, a target allocation assigned to a field/property or
	/// escaped as a return value for a function, or assigned to a ref or out parameter, etc.
	/// </summary>
	Escaped,

	/// <summary>
	/// Indicates target locations that are either not completed or escaped.
	/// </summary>
	NotCompletedOrEscaped,

	/// <summary>
	/// Indicates target locations that are completed.
	/// </summary>
	Completed,

	/// <summary>
	/// Indicates target locations that may be completed on some program path(s).
	/// </summary>
	MaybeCompleted,

	/// <summary>
	/// Indicates target locations whose completion state is unknown.
	/// </summary>
	Unknown,
}
