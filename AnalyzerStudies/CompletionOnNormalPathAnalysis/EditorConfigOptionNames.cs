// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

// Merge members with original namespace
namespace Analyzer.Utilities;

internal static partial class EditorConfigOptionNames
{
	/// <summary>
	/// Boolean option to configure if passing a completable object as a constructor argument should be considered
	/// as a completion ownership transfer, primarily for XXnnnn (CompleteObjectsBeforeLosingScope).
	/// </summary>
	public const string CompletionOwnershipTransferAtConstructor = "completion_ownership_transfer_at_constructor";

	/// <summary>
	/// Boolean option to configure if passing a completable object as an argument to a method invocation should be considered
	/// as a completion ownership transfer to the caller, primarily for XXnnnn (CompleteObjectsBeforeLosingScope)
	/// </summary>
	public const string CompletionOwnershipTransferAtMethodCall = "completion_ownership_transfer_at_method_call";
}
