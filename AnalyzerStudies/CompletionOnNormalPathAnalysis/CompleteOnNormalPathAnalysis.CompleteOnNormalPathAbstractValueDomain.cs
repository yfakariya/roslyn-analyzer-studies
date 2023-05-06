// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;

namespace AnalyzerStudies.CompletionOnNormalPathAnalysis;

partial class CompleteOnNormalPathAnalysis
{
	/// <summary>
	/// Abstract value domain for <see cref="CompleteOnNormalPathAnalysis"/> to merge and compare <see cref="CompleteOnNormalPathAbstractValue"/> values.
	/// </summary>
	private sealed class CompleteOnNormalPathAbstractValueDomain : AbstractValueDomain<CompleteOnNormalPathAbstractValue>
	{
		public static CompleteOnNormalPathAbstractValueDomain Default = new();

		private readonly SetAbstractDomain<IOperation> _completionOperationsDomain = SetAbstractDomain<IOperation>.Default;


		public override CompleteOnNormalPathAbstractValue UnknownOrMayBeValue => CompleteOnNormalPathAbstractValue.Unknown;

		public override CompleteOnNormalPathAbstractValue Bottom => CompleteOnNormalPathAbstractValue.NonTarget;

		public override int Compare(CompleteOnNormalPathAbstractValue oldValue, CompleteOnNormalPathAbstractValue newValue, bool assertMonotonicity)
		{
			if (ReferenceEquals(oldValue, newValue))
			{
				return 0;
			}

			if (oldValue.Kind == newValue.Kind)
			{
				return _completionOperationsDomain.Compare(oldValue.CompletingOrEscapingOperations, newValue.CompletingOrEscapingOperations);
			}
			else if (oldValue.Kind < newValue.Kind ||
				newValue.Kind == CompleteOnNormalPathAbstractValueKind.Invalid ||
				newValue.Kind == CompleteOnNormalPathAbstractValueKind.Completed)
			{
				return -1;
			}
			else
			{
				FireNonMonotonicAssertIfNeeded(assertMonotonicity);
				return 1;
			}
		}

		public override CompleteOnNormalPathAbstractValue Merge(CompleteOnNormalPathAbstractValue value1, CompleteOnNormalPathAbstractValue value2)
		{
			if (value1 == null)
			{
				return value2;
			}
			else if (value2 == null)
			{
				return value1;
			}
			else if (value1.Kind == CompleteOnNormalPathAbstractValueKind.Invalid)
			{
				return value2;
			}
			else if (value2.Kind == CompleteOnNormalPathAbstractValueKind.Invalid)
			{
				return value1;
			}
			else if (value1.Kind == CompleteOnNormalPathAbstractValueKind.NonTarget || value2.Kind == CompleteOnNormalPathAbstractValueKind.NonTarget)
			{
				return CompleteOnNormalPathAbstractValue.NonTarget;
			}
			else if (value1.Kind == CompleteOnNormalPathAbstractValueKind.Unknown || value2.Kind == CompleteOnNormalPathAbstractValueKind.Unknown)
			{
				return CompleteOnNormalPathAbstractValue.Unknown;
			}
			else if (value1.Kind == CompleteOnNormalPathAbstractValueKind.NotCompleted && value2.Kind == CompleteOnNormalPathAbstractValueKind.NotCompleted)
			{
				return CompleteOnNormalPathAbstractValue.NotCompleted;
			}

			var mergedCompletionOperations = _completionOperationsDomain.Merge(value1.CompletingOrEscapingOperations, value2.CompletingOrEscapingOperations);
			Debug.Assert(!mergedCompletionOperations.IsEmpty);
			return new CompleteOnNormalPathAbstractValue(mergedCompletionOperations, GetMergedKind());

			// Local functions.
			CompleteOnNormalPathAbstractValueKind GetMergedKind()
			{
				Debug.Assert(!value1.CompletingOrEscapingOperations.IsEmpty || !value2.CompletingOrEscapingOperations.IsEmpty);

				if (value1.Kind == value2.Kind)
				{
					return value1.Kind;
				}
				else if (value1.Kind == CompleteOnNormalPathAbstractValueKind.MaybeCompleted ||
					value2.Kind == CompleteOnNormalPathAbstractValueKind.MaybeCompleted)
				{
					return CompleteOnNormalPathAbstractValueKind.MaybeCompleted;
				}

				switch (value1.Kind)
				{
					case CompleteOnNormalPathAbstractValueKind.NotCompleted:
					{
						switch (value2.Kind)
						{
							case CompleteOnNormalPathAbstractValueKind.Escaped:
							case CompleteOnNormalPathAbstractValueKind.NotCompletedOrEscaped:
							{
								return CompleteOnNormalPathAbstractValueKind.NotCompletedOrEscaped;
							}
							case CompleteOnNormalPathAbstractValueKind.Completed:
							{
								return CompleteOnNormalPathAbstractValueKind.MaybeCompleted;
							}
						}

						break;
					}
					case CompleteOnNormalPathAbstractValueKind.Escaped:
					{
						switch (value2.Kind)
						{
							case CompleteOnNormalPathAbstractValueKind.NotCompleted:
							case CompleteOnNormalPathAbstractValueKind.NotCompletedOrEscaped:
							{
								return CompleteOnNormalPathAbstractValueKind.NotCompletedOrEscaped;
							}
							case CompleteOnNormalPathAbstractValueKind.Completed:
							{
								return CompleteOnNormalPathAbstractValueKind.Completed;
							}
						}

						break;
					}
					case CompleteOnNormalPathAbstractValueKind.NotCompletedOrEscaped:
					{
						switch (value2.Kind)
						{
							case CompleteOnNormalPathAbstractValueKind.NotCompleted:
							case CompleteOnNormalPathAbstractValueKind.Escaped:
							{
								return CompleteOnNormalPathAbstractValueKind.NotCompletedOrEscaped;
							}
							case CompleteOnNormalPathAbstractValueKind.Completed:
							{
								return CompleteOnNormalPathAbstractValueKind.MaybeCompleted;
							}
						}

						break;
					}
					case CompleteOnNormalPathAbstractValueKind.Completed:
					{
						switch (value2.Kind)
						{
							case CompleteOnNormalPathAbstractValueKind.Escaped:
							{
								return CompleteOnNormalPathAbstractValueKind.Completed;
							}
							case CompleteOnNormalPathAbstractValueKind.NotCompleted:
							case CompleteOnNormalPathAbstractValueKind.NotCompletedOrEscaped:
							{
								return CompleteOnNormalPathAbstractValueKind.MaybeCompleted;
							}
						}

						break;
					}
				}

				Debug.Fail($"Unhandled completion value kind merge: {value1.Kind} and {value2.Kind}");
				return CompleteOnNormalPathAbstractValueKind.MaybeCompleted;
			}
		}
	}
}
