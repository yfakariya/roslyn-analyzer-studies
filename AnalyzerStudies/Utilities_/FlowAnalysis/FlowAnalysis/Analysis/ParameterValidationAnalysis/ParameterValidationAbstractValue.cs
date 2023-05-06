﻿// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

namespace Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.ParameterValidationAnalysis
{
	/// <summary>
	/// Abstract validation value for <see cref="AbstractLocation"/>/<see cref="IOperation"/> tracked by <see cref="ParameterValidationAnalysis"/>.
	/// </summary>
	internal enum ParameterValidationAbstractValue
	{
		/// <summary>
		/// Analysis is not applicable for this location.
		/// </summary>
		NotApplicable,

		/// <summary>
		/// Location has not been validated.
		/// </summary>
		NotValidated,

		/// <summary>
		/// Location has been validated.
		/// </summary>
		Validated,

		/// <summary>
		/// Location may have been validated.
		/// </summary>
		MayBeValidated
	}
}
