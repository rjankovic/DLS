﻿#region License
/* **********************************************************************************
 * Copyright (c) Roman Ivantsov
 * This source code is subject to terms and conditions of the MIT License
 * for Irony. A copy of the license can be found in the License.txt file
 * at the root of this distribution. 
 * By using this source code in any fashion, you are agreeing to be bound by the terms of the 
 * MIT License.
 * You must not remove this notice from this software.
 * **********************************************************************************/
#endregion

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
#if !SILVERLIGHT
// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("44015759-db10-4a6f-8251-d1d18599b60f")]
[assembly: AssemblyTitle("Irony")]
[assembly: AssemblyDescription("Irony Main Assembly")]
[assembly: AssemblyConfiguration("")]
[assembly: SecurityRules(SecurityRuleSet.Level1, SkipVerificationInFullTrust = true)]
#else
[assembly: Guid("B83C8EBA-E4E5-4761-9C38-F662F56D63D7")]
[assembly: AssemblyTitle("Irony-SL")]
[assembly: AssemblyDescription("Irony for Silverlight")]
[assembly: AssemblyConfiguration("")]
#endif
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("Irony")]
[assembly: AssemblyCopyright("Copyright © 2011 Roman Ivantsov")]
[assembly: AssemblyTrademark("Irony")]
[assembly: AssemblyCulture("")]
[assembly: CLSCompliant(true)]

//Make the code security-transparent. more info here: http://msdn.microsoft.com/en-us/library/bb397858.aspx
[assembly: SecurityTransparent()]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]


// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
