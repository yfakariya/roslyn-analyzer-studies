// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

using System.Diagnostics;

namespace Microsoft.CodeAnalysis.FlowAnalysis
{
	internal static class ControlFlowConditionKindExtensions
	{
		public static ControlFlowConditionKind Negate(this ControlFlowConditionKind controlFlowConditionKind)
		{
			switch (controlFlowConditionKind)
			{
				case ControlFlowConditionKind.WhenFalse:
					return ControlFlowConditionKind.WhenTrue;

				case ControlFlowConditionKind.WhenTrue:
					return ControlFlowConditionKind.WhenFalse;

				default:
					Debug.Fail($"Unsupported conditional kind: '{controlFlowConditionKind}'");
					return controlFlowConditionKind;
			}
		}
	}
}
