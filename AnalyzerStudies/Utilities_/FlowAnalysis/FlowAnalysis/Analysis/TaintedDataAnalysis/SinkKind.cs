// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

namespace Analyzer.Utilities.FlowAnalysis.Analysis.TaintedDataAnalysis
{
	public enum SinkKind
	{
		Sql,
		Dll,
		InformationDisclosure,
		Xss,
		FilePathInjection,
		ProcessCommand,
		Regex,
		Ldap,
		Redirect,
		XPath,
		Xml,
		Xaml,
		ZipSlip,
		HardcodedEncryptionKey,
		HardcodedCertificate,
	}
}
