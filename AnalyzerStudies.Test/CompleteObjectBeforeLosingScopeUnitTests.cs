// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

using System.Threading.Tasks;
using NUnit.Framework;

using VerifyCS = AnalyzerStudies.Test.CSharpAnalyzerVerifier<
	AnalyzerStudies.Test.CompleteTransactionScopeBeforeLosingScope>;

namespace AnalyzerStudies.Test;

[TestFixture]
public class CompleteObjectBeforeLosingScopeUnitTests
{
	//No diagnostics expected to show up
	[Test]
	public async Task NoCode_NoDiags()
	{
		var test = @"";

		await VerifyCS.VerifyAnalyzerAsync(test);
	}

	[Test]
	public async Task WithTarget_Completed_OK()
	{
		var test = @"
using System.Transactions;

namespace ConsoleApplication1
{
	class TypeName
	{
		void Method()
		{
			using (var scope = new TransactionScope())
			{
				scope.Complete();
			}
		}
	}
}";
		await VerifyCS.VerifyAnalyzerAsync(test);
	}


	[Test]
	public async Task WithTarget_NotCompleted_NG()
	{
		var test = """
using System.Transactions;

namespace ConsoleApplication1
{
	class TypeName
	{
		void Method()
		{
			using (var scope = new TransactionScope())
			{
			}
		}
	}
}
""";
		var expected = VerifyCS.Diagnostic(
			CompleteObjectBeforeLosingScope.CreateNotCompatibleRule("XA0001", "System.Transactions.TransactionScope", "Complete"))
			.WithSpan(9, 23, 9, 45)
			.WithArguments("new TransactionScope()");
		await VerifyCS.VerifyAnalyzerAsync(test, expected);
	}

	[Test]
	public async Task WithTarget_OnlySuccessCompleted_OK()
	{
		var test = """
using System;
using System.Transactions;

namespace ConsoleApplication1
{
	class TypeName
	{
		void Method()
		{
			using (var scope = new TransactionScope())
			{
				try
				{
					// throws exception
					Environment.GetEnvironmentVariable("NOT_EXIST").ToLower();
					scope.Complete();
				}
				catch
				{
					throw;
				}
			}
		}
	}
}
""";
		await VerifyCS.VerifyAnalyzerAsync(test);
	}

	// TODO: type / method resolution failure
}
