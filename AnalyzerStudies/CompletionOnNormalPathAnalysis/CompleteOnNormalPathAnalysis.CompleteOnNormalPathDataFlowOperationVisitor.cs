// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.PointsToAnalysis;
using Microsoft.CodeAnalysis.Operations;
using CompleteOnNormalPathAnalysisData =
	Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.DictionaryAnalysisData<
		Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.AbstractLocation,
		AnalyzerStudies.CompletionOnNormalPathAnalysis.CompleteOnNormalPathAbstractValue>;
using CompleteOnNormalPathAnalysisDomain =
	Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.MapAbstractDomain<
		Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.AbstractLocation,
		AnalyzerStudies.CompletionOnNormalPathAnalysis.CompleteOnNormalPathAbstractValue>;

namespace AnalyzerStudies.CompletionOnNormalPathAnalysis;

partial class CompleteOnNormalPathAnalysis
{
	/// <summary>
	/// Operation visitor to flow the target values across a given statement in a basic block.
	/// </summary>
	private sealed class CompleteOnNormalPathDataFlowOperationVisitor :
		AbstractLocationDataFlowOperationVisitor<
			CompleteOnNormalPathAnalysisData,
			CompleteOnNormalPathAnalysisContext,
			CompleteOnNormalPathAnalysisResult,
			CompleteOnNormalPathAbstractValue>
	{
		internal static readonly CompleteOnNormalPathAnalysisDomain CompleteOnNormalPathAnalysisDomainInstance =
			new(CompleteOnNormalPathAbstractValueDomain.Default);

		private readonly Dictionary<IFieldSymbol, PointsToAbstractValue>? _trackedInstanceFieldLocations;

		public bool CompletionOwnershipTransferAtConstructor => DataFlowAnalysisContext.CompletionOwnershipTransferAtConstructor;

		public bool CompletionOwnershipTransferAtMethodCall => DataFlowAnalysisContext.CompletionOwnershipTransferAtMethodCall;

		public ImmutableDictionary<IFieldSymbol, PointsToAbstractValue> TrackedInstanceFieldPointsToMap
		{
			get
			{
				if (_trackedInstanceFieldLocations == null)
				{
					throw new InvalidOperationException();
				}

				return _trackedInstanceFieldLocations.ToImmutableDictionary();
			}
		}

		public CompleteOnNormalPathDataFlowOperationVisitor(
			CompleteOnNormalPathAnalysisContext analysisContext
		) : base(analysisContext)
		{
			Debug.Assert(analysisContext.PointsToAnalysisResult != null);

			if (analysisContext.TrackInstanceFields)
			{
				_trackedInstanceFieldLocations = new Dictionary<IFieldSymbol, PointsToAbstractValue>();
			}
		}

		protected override CompleteOnNormalPathAbstractValue GetAbstractDefaultValue(ITypeSymbol type)
			=> CompleteOnNormalPathAbstractValue.NonTarget;

		protected override CompleteOnNormalPathAbstractValue GetAbstractValue(AbstractLocation location)
			=> CurrentAnalysisData.TryGetValue(location, out var value) ?
				value :
				ValueDomain.UnknownOrMayBeValue;

		protected override bool HasAnyAbstractValue(CompleteOnNormalPathAnalysisData data)
			=> data.Count > 0;

		protected override void SetAbstractValue(AbstractLocation location, CompleteOnNormalPathAbstractValue value)
		{
			if (!location.IsNull &&
				location.LocationType is not null &&
				// TODO: ValueType support?
				(!location.LocationType.IsValueType || location.LocationType.IsRefLikeType) &&
				IsTarget(location.LocationType))
			{
				CurrentAnalysisData[location] = value;
			}
		}

		protected override void StopTrackingAbstractValue(AbstractLocation location)
			=> CurrentAnalysisData.Remove(location);

		protected override void ResetCurrentAnalysisData()
			=> ResetAnalysisData(CurrentAnalysisData);

		protected override CompleteOnNormalPathAbstractValue HandleInstanceCreation(IOperation creation, PointsToAbstractValue instanceLocation, CompleteOnNormalPathAbstractValue defaultValue)
		{
			if (ExecutingExceptionPathsAnalysisPostPass)
			{
				base.HandlePossibleThrowingOperation(creation);
			}

			defaultValue = CompleteOnNormalPathAbstractValue.NonTarget;
			var instanceType = creation.Type;

			if (!IsTarget(instanceType) ||
				!IsCurrentBlockReachable())
			{
				return defaultValue;
			}

			SetAbstractValue(instanceLocation, CompleteOnNormalPathAbstractValue.NotCompleted);
			return CompleteOnNormalPathAbstractValue.NotCompleted;
		}

		private void HandleCompletionOperation(IOperation completionOperation, IOperation? completedInstance)
		{
			if (completedInstance == null || !IsTarget(completedInstance.Type))
			{
				return;
			}

			var instanceLocation = GetPointsToAbstractValue(completedInstance);
			foreach (var location in instanceLocation.Locations)
			{
				if (CurrentAnalysisData.TryGetValue(location, out var currentCompletionValue))
				{
					var completionValue = currentCompletionValue.WithNewCompletionOperation(completionOperation);
					SetAbstractValue(location, completionValue);
				}
			}
		}

		private void HandlePossibleInvalidatingOperation(IOperation invalidatedInstance)
		{
			var instanceLocation = GetPointsToAbstractValue(invalidatedInstance);
			foreach (var location in instanceLocation.Locations)
			{
				if (CurrentAnalysisData.TryGetValue(location, out var currentCompletionValue) &&
					currentCompletionValue.Kind != CompleteOnNormalPathAbstractValueKind.NonTarget)
				{
					SetAbstractValue(location, CompleteOnNormalPathAbstractValue.Invalid);
				}
			}
		}

		private void HandlePossibleEscapingOperation(IOperation escapingOperation, ImmutableHashSet<AbstractLocation> escapedLocations)
		{
			foreach (var escapedLocation in escapedLocations)
			{
				if (CurrentAnalysisData.TryGetValue(escapedLocation, out var currentCompletionValue) &&
					currentCompletionValue.Kind != CompleteOnNormalPathAbstractValueKind.Unknown)
				{
					var newCompletionValue = currentCompletionValue.WithNewEscapingOperation(escapingOperation);
					SetAbstractValue(escapedLocation, newCompletionValue);
				}
			}
		}

		protected override void SetAbstractValueForArrayElementInitializer(IArrayCreationOperation arrayCreation, ImmutableArray<AbstractIndex> indices, ITypeSymbol elementType, IOperation initializer, CompleteOnNormalPathAbstractValue value)
		{
			// Escaping from array element assignment is handled in PointsTo analysis.
			// We do not need to do anything here.
		}

		protected override void SetAbstractValueForAssignment(IOperation target, IOperation? assignedValueOperation, CompleteOnNormalPathAbstractValue assignedValue, bool mayBeAssignment = false)
		{
			// Assignments should automatically transfer PointsTo value.
			// We do not need to do anything here.
		}

		protected override void SetAbstractValueForTupleElementAssignment(AnalysisEntity tupleElementEntity, IOperation assignedValueOperation, CompleteOnNormalPathAbstractValue assignedValue)
		{
			// Assigning to tuple elements should automatically transfer PointsTo value.
			// We do not need to do anything here.
		}

		protected override void SetValueForParameterPointsToLocationOnEntry(IParameterSymbol parameter, PointsToAbstractValue pointsToAbstractValue)
		{
			// Unlike I[Async]Disposable, we always handle ownership transfer when configured.
			if (CompletionOwnershipTransferAtConstructor && parameter.ContainingSymbol.IsConstructor())
			{
				SetAbstractValue(pointsToAbstractValue, CompleteOnNormalPathAbstractValue.NotCompleted);
			}
		}

		protected override void EscapeValueForParameterPointsToLocationOnExit(IParameterSymbol parameter, AnalysisEntity analysisEntity, ImmutableHashSet<AbstractLocation> escapedLocations)
		{
			Debug.Assert(!escapedLocations.IsEmpty);
			Debug.Assert(parameter.RefKind != RefKind.None);
			var escapedTargetLocations =
				escapedLocations.Where(l => IsTarget(l.LocationType));
			SetAbstractValue(escapedTargetLocations, ValueDomain.UnknownOrMayBeValue);
		}

		protected override CompleteOnNormalPathAbstractValue ComputeAnalysisValueForEscapedRefOrOutArgument(IArgumentOperation operation, CompleteOnNormalPathAbstractValue defaultValue)
		{
			Debug.Assert(operation.Parameter.RefKind is RefKind.Ref or RefKind.Out);

			// Special case: don't flag "out" arguments for "bool TryGetXXX(..., out value)" invocations.
			if (operation.Parent is IInvocationOperation invocation &&
				invocation.TargetMethod.ReturnType.SpecialType == SpecialType.System_Boolean &&
				invocation.TargetMethod.Name.StartsWith("TryGet", StringComparison.Ordinal) &&
				invocation.Arguments[^1] == operation)
			{
				return CompleteOnNormalPathAbstractValue.NotCompleted;
			}

			return base.ComputeAnalysisValueForEscapedRefOrOutArgument(operation, defaultValue);
		}

		protected override CompleteOnNormalPathAnalysisData MergeAnalysisData(CompleteOnNormalPathAnalysisData value1, CompleteOnNormalPathAnalysisData value2)
			=> CompleteOnNormalPathAnalysisDomainInstance.Merge(value1, value2);

		protected override void UpdateValuesForAnalysisData(CompleteOnNormalPathAnalysisData targetAnalysisData)
			=> UpdateValuesForAnalysisData(targetAnalysisData, CurrentAnalysisData);

		protected override CompleteOnNormalPathAnalysisData GetClonedAnalysisData(CompleteOnNormalPathAnalysisData analysisData)
			=> GetClonedAnalysisDataHelper(CurrentAnalysisData);

		public override CompleteOnNormalPathAnalysisData GetEmptyAnalysisData()
			=> GetEmptyAnalysisDataHelper();

		protected override CompleteOnNormalPathAnalysisData GetExitBlockOutputData(CompleteOnNormalPathAnalysisResult analysisResult)
			=> GetClonedAnalysisDataHelper(analysisResult.ExitBlockOutput.Data);

		protected override void ApplyMissingCurrentAnalysisDataForUnhandledExceptionData(CompleteOnNormalPathAnalysisData dataAtException, ThrownExceptionInfo throwBranchWithExceptionType)
			=> ApplyMissingCurrentAnalysisDataForUnhandledExceptionData(dataAtException, CurrentAnalysisData);

		protected override bool Equals(CompleteOnNormalPathAnalysisData value1, CompleteOnNormalPathAnalysisData value2)
				=> EqualsHelper(value1, value2);

		#region Visitor methods

		public override CompleteOnNormalPathAbstractValue DefaultVisit(IOperation operation, object? argument)
		{
			_ = base.DefaultVisit(operation, argument);
			return CompleteOnNormalPathAbstractValue.NonTarget;
		}

		public override CompleteOnNormalPathAbstractValue Visit(IOperation operation, object? argument)
		{
			var value = base.Visit(operation, argument);
			HandlePossibleEscapingOperation(operation, GetEscapedLocations(operation));

			return value;
		}

		// FIXME: Is it needed?
		protected override void HandlePossibleThrowingOperation(IOperation operation)
		{
			// Handle possible throwing operation.
			// Note that we handle the cases for object creation and method invocation
			// separately as these also lead to NotCompleted allocations, but
			// they should not be considered as part of current state when possible exception occurs.
			if (operation != null &&
				operation.Kind != OperationKind.ObjectCreation &&
				(operation is not IInvocationOperation invocation ||
				   invocation.TargetMethod.IsLambdaOrLocalFunctionOrDelegate()))
			{
				base.HandlePossibleThrowingOperation(operation);
			}
		}

		private bool IsFactoryMethodOfTarget(IMethodSymbol method)
			=> method.IsStatic &&
			   SymbolEqualityComparer.Default.Equals(method.ReturnType, DataFlowAnalysisContext.TargetType);

		public override CompleteOnNormalPathAbstractValue VisitInvocation_NonLambdaOrDelegateOrLocalFunction(
				IMethodSymbol method,
				IOperation? visitedInstance,
				ImmutableArray<IArgumentOperation> visitedArguments,
				bool invokedAsDelegate,
				IOperation originalOperation,
				CompleteOnNormalPathAbstractValue defaultValue
		)
		{
			var value = base.VisitInvocation_NonLambdaOrDelegateOrLocalFunction(method, visitedInstance,
				visitedArguments, invokedAsDelegate, originalOperation, defaultValue);

			if (SymbolEqualityComparer.Default.Equals(method, DataFlowAnalysisContext.CompleteMethod))
			{
				HandleCompletionOperation(originalOperation, visitedInstance);
				return value;
			}

			// Catches things like static calls to File.Open() and Create()
			if (IsFactoryMethodOfTarget(method))
			{
				var instanceLocation = GetPointsToAbstractValue(originalOperation);
				return HandleInstanceCreation(originalOperation, instanceLocation, value);
			}

			return value;
		}

		protected override void ApplyInterproceduralAnalysisResult(
			CompleteOnNormalPathAnalysisData resultData,
			bool isLambdaOrLocalFunction,
			bool hasDelegateTypeArgument,
			CompleteOnNormalPathAnalysisResult analysisResult)
		{
			base.ApplyInterproceduralAnalysisResult(resultData, isLambdaOrLocalFunction, hasDelegateTypeArgument, analysisResult);

			// Apply the tracked instance field locations from interprocedural analysis.
			if (_trackedInstanceFieldLocations != null &&
				analysisResult.TrackedInstanceFieldPointsToMap != null)
			{
				foreach (var (field, pointsToValue) in analysisResult.TrackedInstanceFieldPointsToMap)
				{
					if (!_trackedInstanceFieldLocations.ContainsKey(field))
					{
						_trackedInstanceFieldLocations.Add(field, pointsToValue);
					}
				}
			}
		}

		protected override void PostProcessArgument(IArgumentOperation operation, bool isEscaped)
		{
			base.PostProcessArgument(operation, isEscaped);

			if (isEscaped)
			{
				// Discover if a target object is being passed into the creation method for this new target object
				// and if the new target object assumes ownership of that passed in target object.
				if (IsTargetOwnershipTransfer())
				{
					var pointsToValue = GetPointsToAbstractValue(operation.Value);
					HandlePossibleEscapingOperation(operation, pointsToValue.Locations);
					return;
				}
				else if (FlowBranchConditionKind == ControlFlowConditionKind.WhenFalse &&
					operation.Parameter.RefKind == RefKind.Out &&
					operation.Parent is IInvocationOperation invocation &&
					invocation.TargetMethod.ReturnType.SpecialType == SpecialType.System_Boolean)
				{
					// Case 1:
					//      // Assume 'obj' is not a valid object on the 'else' path.
					//      if (TryXXX(out T obj))
					//      {
					//          obj.Complete();
					//      }
					//
					//      return;

					// Case 2:
					//      if (!TryXXX(out T obj))
					//      {
					//          return; // Assume 'obj' is not a valid object on this path.
					//      }
					//
					//      obj.Complete();

					HandlePossibleInvalidatingOperation(operation);
					return;
				}
			}

			// Ref or out argument values from callee might be escaped by assigning to field.
			if (operation.Parameter.RefKind is RefKind.Out or RefKind.Ref)
			{
				HandlePossibleEscapingOperation(operation, GetEscapedLocations(operation));
			}

			return;

			// Local functions.
			bool IsTargetOwnershipTransfer()
			{
				if (operation.Parameter.RefKind == RefKind.Out)
				{
					// Out arguments are always owned by the caller.
					return false;
				}

				return operation.Parent switch
				{
					IObjectCreationOperation => CompletionOwnershipTransferAtConstructor,
					IInvocationOperation invocation => CompletionOwnershipTransferAtMethodCall ||
						IsFactoryMethodOfTarget(invocation.TargetMethod),

					_ => false,
				};
			}
		}

		public override CompleteOnNormalPathAbstractValue VisitFieldReference(IFieldReferenceOperation operation, object? argument)
		{
			var value = base.VisitFieldReference(operation, argument);
			if (_trackedInstanceFieldLocations != null &&
				!operation.Field.IsStatic &&
				operation.Instance?.Kind == OperationKind.InstanceReference)
			{
				var pointsToAbstractValue = GetPointsToAbstractValue(operation);
				if (pointsToAbstractValue.Kind == PointsToAbstractValueKind.KnownLocations &&
					pointsToAbstractValue.Locations.Count == 1)
				{
					var location = pointsToAbstractValue.Locations.Single();
					if (location.IsAnalysisEntityDefaultLocation)
					{
						if (!_trackedInstanceFieldLocations.TryGetValue(operation.Field, out _))
						{
							// First field reference on any control flow path.
							// Create a default instance to represent the object referenced by the field at start of the method and
							// check if the instance has NotDisposed state, indicating it is a disposable field that must be tracked.
							if (HandleInstanceCreation(operation, pointsToAbstractValue, defaultValue: CompleteOnNormalPathAbstractValue.NonTarget) == CompleteOnNormalPathAbstractValue.NotCompleted)
							{
								_trackedInstanceFieldLocations.Add(operation.Field, pointsToAbstractValue);
							}
						}
						else if (!CurrentAnalysisData.ContainsKey(location))
						{
							// This field has already started being tracked on a different control flow path.
							// Process the default instance creation on this control flow path as well.
							var completedState = HandleInstanceCreation(operation, pointsToAbstractValue, CompleteOnNormalPathAbstractValue.NonTarget);
							Debug.Assert(completedState == CompleteOnNormalPathAbstractValue.NotCompleted);
						}
					}
				}
			}

			return value;
		}

		public override CompleteOnNormalPathAbstractValue VisitBinaryOperatorCore(IBinaryOperation operation, object? argument)
		{
			var value = base.VisitBinaryOperatorCore(operation, argument);

			// Handle null-check for a target symbol on a control flow branch.
			//     var x = flag ? new T() : null;
			//     if (x == null)
			//     {
			//         // Target allocation above cannot exist on this code path.
			//     }
			//

			// if (x == null)
			// {
			//      // This code path
			// }
			var isNullEqualsOnWhenTrue = FlowBranchConditionKind == ControlFlowConditionKind.WhenTrue &&
				(operation.OperatorKind == BinaryOperatorKind.Equals || operation.OperatorKind == BinaryOperatorKind.ObjectValueEquals);

			// if (x != null) { ... }
			// else
			// {
			//      // This code path
			// }
			var isNullNotEqualsOnWhenFalse = FlowBranchConditionKind == ControlFlowConditionKind.WhenFalse &&
				(operation.OperatorKind == BinaryOperatorKind.NotEquals || operation.OperatorKind == BinaryOperatorKind.ObjectValueNotEquals);

			if (isNullEqualsOnWhenTrue || isNullNotEqualsOnWhenFalse)
			{
				if (GetNullAbstractValue(operation.RightOperand) == NullAbstractValue.Null)
				{
					// if (x == null)
					HandlePossibleInvalidatingOperation(operation.LeftOperand);
				}
				else if (GetNullAbstractValue(operation.LeftOperand) == NullAbstractValue.Null)
				{
					// if (null == x)
					HandlePossibleInvalidatingOperation(operation.RightOperand);
				}
			}

			return value;
		}

		public override CompleteOnNormalPathAbstractValue VisitIsNull(IIsNullOperation operation, object? argument)
		{
			var value = base.VisitIsNull(operation, argument);

			// Handle null-check for a target symbol on a control flow branch.
			// See comments in VisitBinaryOperatorCore override above for further details.
			if (FlowBranchConditionKind == ControlFlowConditionKind.WhenTrue)
			{
				HandlePossibleInvalidatingOperation(operation.Operand);
			}

			return value;
		}

		#endregion Visitor methods

		private bool IsTarget([NotNullWhen(returnValue: true)] ITypeSymbol? type)
			=> CompleteOnNormalPathAnalysisHelper.IsTarget(type, DataFlowAnalysisContext.TargetType);
	}
}
