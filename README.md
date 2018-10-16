![Cauldron Logo](https://raw.githubusercontent.com/Capgemini/Cauldron/master/cauldron2.png)

# Cauldron C# Toolkit

## List of breaking changes
## Version 3.1.x
All Cauldron.Core namespaces were renamed to Cauldron.
### Packages that were merged or renamed
Old packages                               | Merged to
------------------------------------------ | ------------------
Capgemini.Cauldron.Core.Reflection<br/>Capgemini.Cauldron.Core.Disposable<br/>Capgemini.Cauldron.Activator | Capgemini.Cauldron.Activator
Capgemini.Cauldron.Core.Comparing<br/>Capgemini.Cauldron.Core.Extensions<br/>Capgemini.Cauldron.Core.Extensions.Compression<br/>Capgemini.Cauldron.Core.Extensions.Convertions<br/>Capgemini.Cauldron.Core.Extensions.IO<br/>Capgemini.Cauldron.Core.Formatters<br/>Capgemini.Cauldron.Core.Net | Capgemini.Cauldron
Capgemini.Cauldron.Core.Collections | Capgemini.Cauldron.Collections
Capgemini.Cauldron.Core.Randomizer | Capgemini.Cauldron.Randomizer
Capgemini.Cauldron.Core.Yaml | Capgemini.Cauldron.Yaml
Capgemini.Cauldron.Core.Interceptors | Capgemini.Cauldron.Interceptors
Capgemini.Cauldron.Core.Collections | Capgemini.Cauldron.Collections
Capgemini.Cauldron.Win32.WPF<br/>Capgemini.Cauldron.Win32.WPF.Interactivity<br/>Capgemini.Cauldron.Win32.WPF.Validation | Capgemini.Cauldron.Win32.WPF
Capgemini.Cauldron.Interceptors<br/>Capgemini.Cauldron.Win32.Interceptors | Capgemini.Cauldron.Interceptors


## Version 3.0.0
The interceptors (method, property, fields, constructor ...) from Cauldron.Interception.Fody were moved to Cauldron.BasicInterceptors and are now implemented as custom interceptors.
Custom interceptors are "scripts" that are compiled and run by Cauldron during the build of your project. For more information check the wiki.

## How to fix MSBUILD blocking a dll.
https://github.com/Microsoft/msbuild/issues/1709


## Documentation
### Wiki
https://github.com/Capgemini/Cauldron/wiki
### .NET Classic
https://Capgemini.github.io/Cauldron/win32/
### .NET Standard 2.0
https://Capgemini.github.io/Cauldron/netstandard/
### UWP
https://Capgemini.github.io/Cauldron/uwp/

## What you require to run this project in Visual Studio
- [Sandcastle Help File Builder](https://github.com/EWSoftware/SHFB/releases) (documentation)
- [CodeMaid](http://www.codemaid.net/) (only if you want to contribute)
- [dotnet-script](https://github.com/filipw/dotnet-script) (building and deployment)

## Nuget Packages
Assembly | Description   | NuGet
-------- | ------------- | ----------------
**Cauldron** | Provides usefull extension methods.<br/>      This assembly contains all funtions of the following packages:<br/>      Capgemini.Cauldron.Core.Comparing<br/>      Capgemini.Cauldron.Core.Extensions<br/>      Capgemini.Cauldron.Core.Extensions.Compression<br/>      Capgemini.Cauldron.Core.Extensions.Convertions<br/>      Capgemini.Cauldron.Core.Extensions.IO<br/>      Capgemini.Cauldron.Core.Formatters<br/>      Capgemini.Cauldron.Core.Net<br/><br/>      Breaking changes in 3.1.x - See https://github.com/Capgemini/Cauldron for more information.<br/> | [![NuGet](https://img.shields.io/nuget/v/Capgemini.Cauldron.svg)](https://www.nuget.org/packages/Capgemini.Cauldron/)
**Cauldron.Activator** | The activator is a simple and fast dependency injection framework. It is based on attributes and does not require any configuration files for configuration. It also supports using static methods as component constructor.<br/>      This version is not compatible with Assemblies weaved by versions older than 3.1.0 of the Cauldron.Activator.<br/>      Breaking changes in 3.1.x - See https://github.com/Capgemini/Cauldron for more information.<br/> | [![NuGet](https://img.shields.io/nuget/v/Capgemini.Cauldron.Activator.svg)](https://www.nuget.org/packages/Capgemini.Cauldron.Activator/)
**Cauldron.BasicInterceptors** | Custom interceptors for Cauldron.Interception.Fody that provides method, property, field and constructor interception. | [![NuGet](https://img.shields.io/nuget/v/Cauldron.BasicInterceptors.svg)](https://www.nuget.org/packages/Cauldron.BasicInterceptors/)
**Cauldron.Cecilator** | A Fody/Mono.Cecil wrapper that provides most basic IL code weaving helpers.<br/> | [![NuGet](https://img.shields.io/nuget/v/Cauldron.Cecilator.svg)](https://www.nuget.org/packages/Cauldron.Cecilator/)
**Cauldron.Collections** | Contains specialized Dictionary implementations, that are 4x faster than the .NET default Dictionary.<br/>      Breaking changes in 3.1.x - See https://github.com/Capgemini/Cauldron for more information.<br/> | [![NuGet](https://img.shields.io/nuget/v/Capgemini.Cauldron.Collections.svg)](https://www.nuget.org/packages/Capgemini.Cauldron.Collections/)
**Cauldron.Consoles** | Cauldron.Consoles is a Cauldron.Core based parameter parser which supports grouping of parameters in execution groups. It is also supports localization and has a nice parameter table :)<br/>      Breaking changes in 3.1.x - See https://github.com/Capgemini/Cauldron for more information.<br/> | [![NuGet](https://img.shields.io/nuget/v/Capgemini.Cauldron.Consoles.svg)](https://www.nuget.org/packages/Capgemini.Cauldron.Consoles/)
**Cauldron.Cryptography** | Contains typical implementations for AES, RSA and RSA-AES encryptions. It also contains extensions that helps working with SecureString.<br/>      Breaking changes in 3.1.x - See https://github.com/Capgemini/Cauldron for more information.<br/> | [![NuGet](https://img.shields.io/nuget/v/Capgemini.Cauldron.Cryptography.svg)](https://www.nuget.org/packages/Capgemini.Cauldron.Cryptography/)
**Cauldron.Interception.Fody** | Fody add-in that provides custom interception. Supports Net45, NetStandard2.0 and UWP.<br/>      Method, Property, Constructor and Field interceptors can be found in the package Cauldron.BasicInterceptors (https://www.nuget.org/packages/Cauldron.BasicInterceptors/)<br/> | [![NuGet](https://img.shields.io/nuget/v/Cauldron.Interception.Fody.svg)](https://www.nuget.org/packages/Cauldron.Interception.Fody/)
**Cauldron.Interceptors** | A collection of interceptor implementations.<br/>      Breaking changes in 3.1.x - See https://github.com/Capgemini/Cauldron for more information.<br/> | [![NuGet](https://img.shields.io/nuget/v/Capgemini.Cauldron.Interceptors.svg)](https://www.nuget.org/packages/Capgemini.Cauldron.Interceptors/)
**Cauldron.Localization** | A simple localization implementation that can work with different sources.<br/>      Breaking changes in 3.1.x - See https://github.com/Capgemini/Cauldron for more information.<br/> | [![NuGet](https://img.shields.io/nuget/v/Capgemini.Cauldron.Localization.svg)](https://www.nuget.org/packages/Capgemini.Cauldron.Localization/)
**Cauldron.Randomizer** | Provides a randomizer that is cryptographically secure.<br/>      Breaking changes in 3.1.x - See https://github.com/Capgemini/Cauldron for more information.<br/> | [![NuGet](https://img.shields.io/nuget/v/Capgemini.Cauldron.Randomizer.svg)](https://www.nuget.org/packages/Capgemini.Cauldron.Randomizer/)
**Cauldron.Win32.Impersonation** | Provides an extension for PrincipalContext to impersonate a user. | [![NuGet](https://img.shields.io/nuget/v/Capgemini.Cauldron.Win32.Impersonation.svg)](https://www.nuget.org/packages/Capgemini.Cauldron.Win32.Impersonation/)
**Cauldron.Win32.MonitorInfo** | Provides properties and methods for getting information about the monitor.<br/> | [![NuGet](https://img.shields.io/nuget/v/Capgemini.Cauldron.Win32.MonitorInfo.svg)](https://www.nuget.org/packages/Capgemini.Cauldron.Win32.MonitorInfo/)
**Cauldron.Win32.UserInformation** | Consolidates methods for getting user information (profile picture, email address, home directory ect.) to a single class.<br/>      Breaking changes in 3.1.x - See https://github.com/Capgemini/Cauldron for more information.<br/> | [![NuGet](https://img.shields.io/nuget/v/Capgemini.Cauldron.Win32.UserInformation.svg)](https://www.nuget.org/packages/Capgemini.Cauldron.Win32.UserInformation/)
**Cauldron.Win32.WindowsService** | Creating Windows Services is now a cake walk. This implementation can also handle localized service name and description. | [![NuGet](https://img.shields.io/nuget/v/Capgemini.Cauldron.Win32.WindowsService.svg)](https://www.nuget.org/packages/Capgemini.Cauldron.Win32.WindowsService/)
**Cauldron.Win32.WPF** | A simple MVVM framework that heavily uses IL-weaving based on Fody. | [![NuGet](https://img.shields.io/nuget/v/Capgemini.Cauldron.Win32.WPF.svg)](https://www.nuget.org/packages/Capgemini.Cauldron.Win32.WPF/)
**Cauldron.Win32.WPF.ParameterPassing** | Handles passing of parameters to running instance(s) of an application. | [![NuGet](https://img.shields.io/nuget/v/Capgemini.Cauldron.Win32.WPF.ParameterPassing.svg)](https://www.nuget.org/packages/Capgemini.Cauldron.Win32.WPF.ParameterPassing/)
**Cauldron.Win32.WPF.Theme.VSDark** | Visual Studio dark theme for Cauldron WPF | [![NuGet](https://img.shields.io/nuget/v/Capgemini.Cauldron.Win32.WPF.Theme.VSDark.svg)](https://www.nuget.org/packages/Capgemini.Cauldron.Win32.WPF.Theme.VSDark/)
**Cauldron.Win32.WPF.Theme.VSLight** | Visual Studio dark theme for Cauldron WPF | [![NuGet](https://img.shields.io/nuget/v/Capgemini.Cauldron.Win32.WPF.Theme.VSLight.svg)](https://www.nuget.org/packages/Capgemini.Cauldron.Win32.WPF.Theme.VSLight/)
**Cauldron.Yaml** | A YAML deserializer.<br/>      Breaking changes in 3.1.x - See https://github.com/Capgemini/Cauldron for more information.<br/> | [![NuGet](https://img.shields.io/nuget/v/Capgemini.Cauldron.Yaml.svg)](https://www.nuget.org/packages/Capgemini.Cauldron.Yaml/)

## Release Notes
### __3.2.1__
#### Change
- __Capgemini.Cauldron.Win32.WPF:__ _FindVisualParent<T> now considers sub-classes_
#### Added
- __Capgemini.Cauldron.Win32.WPF:__ _DataTrigger behaviour added_
- __Capgemini.Cauldron.Win32.WPF:__ _StoryboardBeginAnimationAction action added_
- __Capgemini.Cauldron.Win32.WPF:__ _VisibilityAnimation behaviour added_
- __Capgemini.Cauldron.Win32.WPF:__ _ColorHexToSolidColorBrush Json Converter added._
- __Capgemini.Cauldron:__ _Added MathEx methods: ValueOf for int, long and float._
- __Capgemini.Cauldron:__ _Directory Combine extensions added._
- __Capgemini.Cauldron:__ _Append extension added. Works like NET471 LINQ version._
- __Capgemini.Cauldron:__ _MinutesToTimeSpan Json Converter added._
#### Bugfix
- __Capgemini.Cauldron:__ _Several small fixes in the XML comments_
- __Capgemini.Cauldron:__ _Convertion extension 'As' can now convert value types correctly._
### __3.2.0__
#### Change
- __Capgemini.Cauldron.Interceptors:__ _Dependency to NLog removed_
- __Capgemini.Cauldron.Interceptors:__ _PerformanceLoggerAttribute removed_
- __Capgemini.Cauldron.Interceptors:__ _ExceptionLoggerAttribute removed_
- __Capgemini.Cauldron.Interceptors:__ _PropertyOnSetAttribute added_
- __Capgemini.Cauldron.Interceptors:__ _TimedCacheAttribute namespace changed to Cauldron.Core.Interceptors._
- __Capgemini.Cauldron.Interceptors:__ _InterceptorRule added to RegistryClassAttribute_
- __Capgemini.Cauldron.Interceptors:__ _Fody version 3.0.0 update and minor bug fixes._
- __Capgemini.Cauldron.Interceptors:__ _Now bringing its own interceptor_
- __Capgemini.Cauldron.Interceptors:__ _Assemblies for Net45, Net451, Net452, Net46, Net461 and Net462 added_
- __Capgemini.Cauldron.Interceptors:__ _Fody version 3.0.0 update and minor bug fixes._
- __Capgemini.Cauldron.Interceptors:__ _Assemblies for Net45, Net451, Net452, Net46, Net461 and Net462 added_
- __Capgemini.Cauldron.Interceptors:__ _Now signed_
- __Capgemini.Cauldron.Win32.UserInformation:__ _Dependencies cleaned_
- __Capgemini.Cauldron.Win32.UserInformation:__ _Now signed_
- __Capgemini.Cauldron.Win32.WindowsService:__ _Now signed_
- __Capgemini.Cauldron.Win32.Impersonation:__ _Assemblies for Net45, Net451, Net452, Net46, Net461 and Net462 added_
- __Capgemini.Cauldron.Win32.Impersonation:__ _Now signed_
- __Cauldron.Cecilator:__ _Nullable equality check fixed_
- __Capgemini.Cauldron.Win32.WPF.Theme.VSLight:__ _Now signed_
- __Capgemini.Cauldron.Collections:__ _Assemblies for Net45, Net451, Net452, Net46, Net461 and Net462 added_
- __Capgemini.Cauldron.Collections:__ _ConcurrentKeyedCollection removed with no replacement, because it is better to use ConcurrentDictionary._
- __Capgemini.Cauldron.Collections:__ _Extension ToKeyedCollection removed and replaced by ToFastKeyedCollection._
- __Capgemini.Cauldron.Collections:__ _KeyedCollectionEx removed and replaced by FastKeyedCollection._
- __Capgemini.Cauldron.Collections:__ _ConcurrentList removed and replaced by ConcurrentCollection._
- __Capgemini.Cauldron.Collections:__ _FastObservableCollection added._
- __Capgemini.Cauldron.Collections:__ _FastDictionary added._
- __Capgemini.Cauldron.Collections:__ _Now signed_
- __Capgemini.Cauldron.Collections:__ _All dependencies removed._
- __Capgemini.Cauldron.Win32.WPF.Theme.VSDark:__ _Now signed_
- __Capgemini.Cauldron.Win32.WPF.ParameterPassing:__ _Now signed_
- __Capgemini.Cauldron.Win32.MonitorInfo:__ _Now signed_
- __Capgemini.Cauldron.Win32.WPF:__ _Now signed_
#### Added
- __Capgemini.Cauldron.Interceptors:__ _Dependency to NLog added_
- __Capgemini.Cauldron.Interceptors:__ _PerformanceLoggerAttribute added_
- __Capgemini.Cauldron.Interceptors:__ _ExceptionLoggerAttribute added_
- __Capgemini.Cauldron.Interceptors:__ _ExecutionTimeAttribute added_
- __Capgemini.Cauldron.Interceptors:__ _PropertyOnSetAttribute added_
- __Capgemini.Cauldron.Interceptors:__ _RegistryAttribute added_
- __Capgemini.Cauldron.Interceptors:__ _contentFiles entry in nuget added._
#### Bugfix
- __Capgemini.Cauldron.Interceptors:__ _TimedCache weaver fixed for async methods_
- __Capgemini.Cauldron.Interceptors:__ _TimedCache weaver fixed for async methods part 2_
- __Cauldron.Cecilator:__ _Detection of resulting type in block container returing null, now fixed_
- __Cauldron.Cecilator:__ _Some minor bugs fixed_
- __Cauldron.Cecilator:__ _Method usage detection was broken which lead to unweaved methods._
- __Cauldron.Cecilator:__ _GetParametersArray was broken_
- __Cauldron.Cecilator:__ _GetParametersArray was still broken - not fixed_
- __Cauldron.Cecilator:__ _Multiple execution of OnEnter on async methods bug is fixed_
- __Capgemini.Cauldron.Win32.WPF.Theme.VSLight:__ _Styles not loaded - fixed_
- __Capgemini.Cauldron.Collections:__ _Bugs in TryGetValue in all collection type fixed_
- __Capgemini.Cauldron.Win32.WPF.Theme.VSDark:__ _Styles not loaded - fixed_
### __3.1.35 BETA__
#### Added
- __Capgemini.Cauldron:__ _PadOrCut extension method added. It allows to pad chars or cut the string to a specific length._
- __Capgemini.Cauldron:__ _Debug class added_
#### Change
- __Capgemini.Cauldron:__ _Concat<T>(this T[][] arrays) renamed to Flatten<T>(this T[][] arrays)_
- __Capgemini.Cauldron:__ _Assemblies for Net45, Net451, Net452, Net46, Net461 and Net462 added_
- __Capgemini.Cauldron:__ _Assembly for UWP added_
- __Capgemini.Cauldron:__ _Cauldron.Comparing merged with Cauldron.Core.Extension_
- __Capgemini.Cauldron:__ _All extension packages merged_
- __Capgemini.Cauldron:__ _Now signed_
### __3.1.26 BETA__
#### Added
- __Capgemini.Cauldron.Randomizer:__ _Added assemblies for NET45 and NET461_
#### Change
- __Capgemini.Cauldron.Randomizer:__ _Net461 DLLs removed because it causes a lot of issues on mixed solutions (NetCore and Net461)_
- __Capgemini.Cauldron.Randomizer:__ _Assemblies for Net45, Net451, Net452, Net46, Net461 and Net462 added_
- __Capgemini.Cauldron.Randomizer:__ _Assembly for UWP added_
- __Capgemini.Cauldron.Randomizer:__ _Now signed_
### __3.0.34__
#### Bugfix
- __Cauldron.Interception.Fody:__ _Multiple execution of OnEnter on async methods bug is fixed_
### __3.0.33 BETA__
#### Bugfix
- __Cauldron.BasicInterceptors:__ _Multiple execution of OnEnter on async methods bug is fixed_
- __Cauldron.Interception.Fody:__ _GetParametersArray was still broken - now fixed_
#### Change
- __Capgemini.Cauldron.Activator:__ _Fody dependency updated to 3.1.4_
### __3.0.32__
#### Bugfix
- __Cauldron.BasicInterceptors:__ _GetParametersArray was still broken - not fixed_
- __Capgemini.Cauldron.Activator:__ _RunSync was ambigious... It is supposed to be in Cauldron.Core.Extensions... Not in Activator._
### __3.0.31__
#### Change
- __Capgemini.Cauldron.Activator:__ _Fody dependency updated to 3.1.3_
#### Bugfix
- __Cauldron.Interception.Fody:__ _Scripting build bug that caused Cauldron not to rebuild changed scripts, solved_
### __3.0.30__
#### Change
- __Capgemini.Cauldron.Activator:__ _Cauldron Reflection merged with Cauldron.Activator_
- __Capgemini.Cauldron.Activator:__ _Cauldron Disposable merged with Cauldron.Activator_
- __Capgemini.Cauldron.Activator:__ _Now signed_
#### Bugfix
- __Cauldron.Interception.Fody:__ _GetParametersArray was broken_
### __3.0.29__
#### Bugfix
- __Cauldron.BasicInterceptors:__ _GetParametersArray was broken_
#### Change
- __Capgemini.Cauldron.Activator:__ _Speed tweak for the dictionary._
- __Capgemini.Cauldron.Activator:__ _FastDictionary added._
### __3.0.28__
#### Bugfix
- __Capgemini.Cauldron.Activator:__ _Method usage detection was broken which lead to unweaved methods._
### __3.0.26__
#### Change
- __Capgemini.Cauldron.Activator:__ _Conditional now possible - CallingType added to Resolver_
- __Capgemini.Cauldron.Activator:__ _Factory.Destroy optimized_
### __3.0.23__
#### Bugfix
- __Cauldron.BasicInterceptors:__ _Async method interception with multiple awaits fixed_
- __Cauldron.Interception.Fody:__ _Switches were not copied correctly in methods_
- __Cauldron.Cecilator:__ _Switches were not copied correctly in methods_
#### Added
- __Capgemini.Cauldron.Activator:__ _added 2 new properties to IFactoryTypeInfo: ChildType, IsEnumerable_
#### Change
- __Capgemini.Cauldron.Activator:__ _Performance tweaks for the Factory_
- __Capgemini.Cauldron.Activator:__ _More performance tweaks for the Factory_
### __3.0.21__
#### Change
- __Capgemini.Cauldron.Cryptography:__ _Now signed_
- __Capgemini.Cauldron.Cryptography:__ _All references removed_
#### Added
- __Capgemini.Cauldron.Cryptography:__ _UWP assembly added_
### __3.0.20__
#### Added
- __Cauldron.BasicInterceptors:__ _contentFiles entry in nuget added._
- __Capgemini.Cauldron.Win32.WindowsService:__ _contentFiles entry in nuget added._
- __Capgemini.Cauldron.Activator:__ _contentFiles entry in nuget added._
- __Cauldron.Interception.Fody:__ _Support for project "linked" interceptors._
- __Cauldron.Interception.Fody:__ _Searches project.assets.json file for interceptors_
- __Cauldron.Interception.Fody:__ _Searches csproj.nuget.g.props file for interceptors_
- __Capgemini.Cauldron.Win32.WPF:__ _contentFiles entry in nuget added._
#### Change
- __Capgemini.Cauldron.Activator:__ _More performance boost for the DI_
- __Cauldron.Cecilator:__ _Async helper coder optimized_
#### Bugfix
- __Capgemini.Cauldron.Activator:__ _Resolver was unable to resolve._
- __Cauldron.Cecilator:__ _"Member 'Cast' is declared in another module and needs to be imported" error fixed_
### __3.0.19__
#### Change
- __Capgemini.Cauldron.Activator:__ _Performance tuning_
- __Capgemini.Cauldron.Activator:__ _Inject attribute is now a custom interceptor_
- __Cauldron.Cecilator:__ _TypeSystem moved from BuilderType to the new class BuilderTypes_
- __Cauldron.Cecilator:__ _JsonIgnoreAttribute is now not added to Cauldron generated fields._
#### Added
- __Capgemini.Cauldron.Activator:__ _Component attribute options added._
- __Capgemini.Cauldron.Activator:__ _Injector properties added_
- __Capgemini.Cauldron.Activator:__ _Rebuilt event added_
#### Bugfix
- __Cauldron.Cecilator:__ _Multiple bugs regarding resolving of generics fixed._
- __Cauldron.Cecilator:__ _Async Methods parameters are "optimized" away on release mode. The weaver now adds the parameters._
- __Cauldron.Cecilator:__ _Async Methods "this" reference fixed._
### __3.0.18__
#### Added
- __Capgemini.Cauldron.Activator:__ _Closed generic support added._
#### Bugfix
- __Cauldron.Interception.Fody:__ _Bug that causes an exception while weaving in Net47 fixed._
### __3.0.17__
#### Change
- __Capgemini.Cauldron.Activator:__ _IFactoryResolver removed and replaced by Factory.Resolvers collection._
#### Added
- __Capgemini.Cauldron.Activator:__ _IFactoryExtension added._
#### Bugfix
- __Cauldron.Cecilator:__ _Bug fixed regarding weaving of async void methods._
- __Cauldron.Cecilator:__ _Return value for async methods is not correctly detected in some cases. fixed._
- __Cauldron.Cecilator:__ _Parameter array in async method not correctly weaved._
### __3.0.16__
#### Added
- __Cauldron.BasicInterceptors:__ _InterceptorOptionAttribute added._
- __Cauldron.Cecilator:__ _Added some support to more generic coding style._
#### Bugfix
- __Capgemini.Cauldron.Win32.WindowsService:__ _Bad nuget reference fixed_
### __3.0.14__
#### Bugfix
- __Cauldron.BasicInterceptors:__ _Due to changes in cecilator the method weaver has to be fixed also_
- __Cauldron.Cecilator:__ _Fixed bugs in weaving default values for generic parameters_
- __Cauldron.Cecilator:__ _Fixed bugs in weaving code into the Async State Machine MoveNext method_
- __Cauldron.Cecilator:__ _Fixed bugs in CopyMethod that causes the weaver to weave the wrong method in the call._
#### Added
- __Cauldron.BasicInterceptors:__ _Interceptors in abstract class for methods and properties are now possible._
### __3.0.10 BETA__
#### Bugfix
- __Capgemini.Cauldron.Activator:__ _Factory object cache performance tweaks_
### __3.0.7 BETA__
#### Change
- __Cauldron.BasicInterceptors:__ _Assemblies for Net45, Net451, Net452, Net46, Net461 and Net462 added_
- __Capgemini.Cauldron.Cryptography:__ _Assemblies for Net45, Net451, Net452, Net46, Net461 and Net462 added_
- __Capgemini.Cauldron.Win32.UserInformation:__ _Assemblies for Net45, Net451, Net452, Net46, Net461 and Net462 added_
- __Capgemini.Cauldron.Win32.WindowsService:__ _Assemblies for Net45, Net451, Net452, Net46, Net461 and Net462 added_
- __Capgemini.Cauldron.Activator:__ _Assemblies for Net45, Net451, Net452, Net46, Net461 and Net462 added_
- __Capgemini.Cauldron.Consoles:__ _Assemblies for Net45, Net451, Net452, Net46, Net461 and Net462 added_
- __Capgemini.Cauldron.Localization:__ _Assemblies for Net45, Net451, Net452, Net46, Net461 and Net462 added_
- __Capgemini.Cauldron.Win32.WPF.Theme.VSLight:__ _Assemblies for Net45, Net451, Net452, Net46, Net461 and Net462 added_
- __Capgemini.Cauldron.Win32.WPF.Theme.VSDark:__ _Assemblies for Net45, Net451, Net452, Net46, Net461 and Net462 added_
- __Capgemini.Cauldron.Win32.WPF.ParameterPassing:__ _Assemblies for Net45, Net451, Net452, Net46, Net461 and Net462 added_
- __Capgemini.Cauldron.Win32.MonitorInfo:__ _Assemblies for Net45, Net451, Net452, Net46, Net461 and Net462 added_
- __Capgemini.Cauldron.Win32.WPF:__ _Assemblies for Net45, Net451, Net452, Net46, Net461 and Net462 added_
### __3.0.5 BETA__
#### Bugfix
- __Capgemini.Cauldron.Activator:__ _A bug in cecilator that causes arrays parameter to be not correctly weaved fixed_
- __Cauldron.Interception.Fody:__ _Fixed bugs in weaving async methods_
- __Cauldron.Interception.Fody:__ _Fixed bugs in weaving methods with generic parameters_
- __Cauldron.Cecilator:__ _A bug in cecilator that causes arrays parameter to be not correctly weaved fixed_
- __Cauldron.Cecilator:__ _Fixed bugs in weaving async methods_
- __Cauldron.Cecilator:__ _Fixed bugs in weaving methods with generic parameters_
### __3.0.4 BETA__
#### Bugfix
- __Cauldron.Interception.Fody:__ _Properties with getter and setter but without backing field caused an Exception during weaving._
### __3.0.3 BETA__
#### Added
- __Cauldron.BasicInterceptors:__ _First version_
- __Cauldron.BasicInterceptors:__ _Dlls added for Net451, Net452, Net461 and Net462_
#### Change
- __Capgemini.Cauldron.Win32.WindowsService:__ _Fody version 3.0.0 update and minor bug fixes._
- __Capgemini.Cauldron.Activator:__ _Fody version 3.0.0 update and minor bug fixes._
- __Capgemini.Cauldron.Activator:__ _Now bringing its own interceptor_
- __Cauldron.Interception.Fody:__ _Basic interceptors moved to Cauldron.BasicInterceptors package_
- __Cauldron.Cecilator:__ _Fody version 3.0.0 update and minor bug fixes._
- __Capgemini.Cauldron.Localization:__ _Fody version 3.0.0 update and minor bug fixes._
- __Capgemini.Cauldron.Win32.WPF:__ _Fody version 3.0.0 update and minor bug fixes._
- __Capgemini.Cauldron.Win32.WPF:__ _Now bringing its own interceptor_
### __3.0.2 BETA__
#### Bugfix
- __Cauldron.Interception.Fody:__ _Resolving of generics throws an exception on certain cases._
- __Cauldron.Interception.Fody:__ _Custom interceptors now works correctly._
### __3.0.0 BETA__
#### Change
- __Cauldron.Interception.Fody:__ _Fody version 3.0.0 update and minor bug fixes._
- __Cauldron.Interception.Fody:__ _First version of the new Cecilator._
### __2.1.0__
#### Change
- __Cauldron.Interception.Fody:__ _All interceptor interfaces that were part of this package were moved to Cauldron.BasicInterceptors_
### __2.0.35 BETA__
#### Added
- __Capgemini.Cauldron.Win32.WPF.Theme.VSLight:__ _Beta release._
- __Capgemini.Cauldron.Win32.WPF.Theme.VSDark:__ _Beta release._
### __2.0.30 BETA__
#### Change
- __Capgemini.Cauldron.Activator:__ _ComponentConstructorAttribute now accepts internal ctors_
- __Cauldron.Interception.Fody:__ _DoNotInterceptAttribute removed... Replaced by the InterceptionRuleAttribute instead. See documentation for more details._
- __Cauldron.Interception.Fody:__ _Interceptor OnException return type changed to bool to be able to create interceptors that swallow exceptions._
- __Cauldron.Interception.Fody:__ _Getting rid of useless casts in created code._
- __Capgemini.Cauldron.Win32.WPF.ParameterPassing:__ _Change COPYDATASTRUCT data to unicode._
#### Added
- __Cauldron.Interception.Fody:__ _Support for properties without backing fields added._
- __Cauldron.Interception.Fody:__ _Support for assembly-wide attribute decorating added._
- __Cauldron.Interception.Fody:__ _Simple interceptors without try catch added._
#### Bugfix
- __Cauldron.Interception.Fody:__ _MethodInterceptors in Async methods are now weaved correctly._
- __Cauldron.Interception.Fody:__ _ModuleMain entry point interceptor fixed. Weaver throwed exception during weaving._
### __2.0.28__
#### Bugfix
- __Cauldron.Interception.Fody:__ _AssignMethodAttribute decorated field types were not imported._
#### Change
- __Cauldron.Interception.Fody:__ _{CtorArgument:} placeholder now accepts parameter names besides index._
- __Cauldron.Interception.Fody:__ _AssignMethodAttribute now falls back to 'is assignable' if the return type of a method does not match._
### __2.0.27__
#### Change
- __Cauldron.Interception.Fody:__ _AssignMethodAttribute now allowing parameters._
- __Cauldron.Interception.Fody:__ _AssignMethodAttribute can now throw an error if the associated method is not found._
- __Cauldron.Interception.Fody:__ _AssignMethodAttribute now supports constructor defined placeholders._
#### Bugfix
- __Cauldron.Interception.Fody:__ _Fixed a bug that caused some methods to return null if Code-Optimization is on._
### __2.0.26__
#### Added
- __Cauldron.Interception.Fody:__ _New interceptor extension added - AssignMethodAttribute_
### __2.0.25__
#### Bugfix
- __Cauldron.Interception.Fody:__ _NonSerializedAttribute was implemented as a custom attribute... Now correctly implemented._
#### Added
- __Cauldron.Interception.Fody:__ _System.Xml.Serialization.XmlIgnoreAttribute will be added to all Cauldron created properties if the type System.Xml.Serialization.XmlIgnoreAttribute exist._
- __Capgemini.Cauldron.Win32.WPF:__ _OnActivationProtocol added to ApplicationBase_
- __Capgemini.Cauldron.Win32.WPF:__ _RegisterUrlProtocols added to ApplicationBase_
#### Change
- __Capgemini.Cauldron.Win32.WPF.ParameterPassing:__ _Will now also invoke the callback delegate if there are no instances of the program running._
- __Capgemini.Cauldron.Win32.WPF:__ _ParameterPassing in ApplicationBase now uses Environment.Exit(0) after passing the params to other instances._
### __2.0.22__
#### Change
- __Capgemini.Cauldron.Win32.UserInformation:__ _Added a fallback if the user's account picture file was not found._
- __Cauldron.Interception.Fody:__ _Some info and error messages changed to be more informative._
- __Capgemini.Cauldron.Win32.WPF.ParameterPassing:__ _Now uses the NET461 assembly version of the randomizer._
#### Added
- __Cauldron.Interception.Fody:__ _Real support for Net45 added - The weaver itself still requires an installed Net461._
- __Cauldron.Interception.Fody:__ _NonSerializedAttribute or IgnoreDataMemberAttribute or JsonIgnoreAttribute added to all Cauldron created fields and properties (Only if these attributes are referenced in the project)._
#### Bugfix
- __Cauldron.Interception.Fody:__ _In some cases if the weaver tries to retrieved information from referenced assemblies, a BadImageFormatException occures. Fixed._
### __2.0.21__
#### Added
- __Capgemini.Cauldron.Win32.UserInformation:__ _WTSClientName added to User and CurrentUser. WTSClientName returns the Windows Terminal Session client name._
### __2.0.20__
#### Added
- __Cauldron.Interception.Fody:__ _Constructor interceptor added._
### __2.0.19__
#### Bugfix
- __Capgemini.Cauldron.Activator:__ _CreateFirst always returned null in some cases... This is fixed._
- __Cauldron.Interception.Fody:__ _A bug was in "cleaning" fixed that caused local variables to be removed although they were still in use._
- __Cauldron.Interception.Fody:__ _Fixed a bug that caused the weaver to weav types that does not exist and then throws exceptions._
- __Cauldron.Interception.Fody:__ _Sort of fixed the issue that jumping to errors on async methods did not work._
- __Capgemini.Cauldron.Localization:__ _Added a Contains method to Locale._
- __Capgemini.Cauldron.Win32.WPF:__ _Several issues with the dispatcher fixed._
- __Capgemini.Cauldron.Win32.WPF:__ _Navigator was not able to find the correct view in some cases._
- __Capgemini.Cauldron.Win32.WPF:__ _ObjectToVisibilityConverter logic was inverted._
- __Capgemini.Cauldron.Win32.WPF:__ _Bugs in MessageDialog that caused the strings not to be localized correctly fixed_
- __Capgemini.Cauldron.Win32.WPF:__ _Fixes an issue where the SplashScreen can be removed from MainView and causes the application to exit_
#### Added
- __Cauldron.Interception.Fody:__ _Allowing non auto-properties to be intercepted._
#### Change
- __Capgemini.Cauldron.Win32.WPF:__ _XAML resources loading order can now be changed by adding a dash and a numeric suffix to the file name. e.g. Style-01.xaml._
### __2.0.18__
#### Bugfix
- __Cauldron.Interception.Fody:__ _If PropertyChanged.Fody weaved before Cauldron then Cauldron has removed the implementation for the particular property. This is now fixed._
- __Cauldron.Interception.Fody:__ _Fixed a bug that caused try-catches to be malformed if the modified method was empty._
- __Cauldron.Interception.Fody:__ _Fixed the weaver for the IChangeAwareViewModel interface._
#### Added
- __Cauldron.Interception.Fody:__ _IPropertyInitializer added. This interface forces an interceptor to be loaded on type init instead of the first call._
### __2.0.16__
#### Bugfix
- __Cauldron.Interception.Fody:__ _Bug from yesterday regarding ComponentAttribute still occured on nested private classes. Now fixed._
#### Added
- __Cauldron.Interception.Fody:__ _Sequence points added._
- __Capgemini.Cauldron.Win32.WPF:__ _The ViewAttribute now accepts a string as view name/type._
- __Capgemini.Cauldron.Win32.WPF:__ _OnIsLoadingChanged added to ViewModelBase._
- __Capgemini.Cauldron.Win32.WPF:__ _Message unsubsribe added to dispose of ViewModelBase._
- __Capgemini.Cauldron.Win32.WPF:__ _RegisterChildren attribute added._
#### Change
- __Capgemini.Cauldron.Win32.WPF:__ _Renamed IMessageDialog.ShowException to IMessageDialog.ShowExceptionAsync_
### __2.0.13__
#### Added
- __Capgemini.Cauldron.Activator:__ _CreateFirst method added to the Factory. This will auto-pick the implementation with the highest priority if multiple implementations are available._
- __Cauldron.Interception.Fody:__ _Weaves an initializer for xaml resources to be able to auto-load them._
- __Capgemini.Cauldron.Win32.WPF:__ _Several information from ApplicationInfo added as static resources._
- __Capgemini.Cauldron.Win32.WPF:__ _IViewAware interface added._
- __Cauldron.Interception.Fody:__ _Useless method cache removed from weaver._
#### Change
- __Cauldron.Interception.Fody:__ _TimedCacheAttribute namespace change also reflected to weaver._
#### Bugfix
- __Cauldron.Interception.Fody:__ _The weaving of the auto-ComponentAttribute attributed types caused an error in some cases if the type is not public._
- __Cauldron.Interception.Fody:__ _Interceptors that uses ISyncRoot interface was not correctly weaved. This caused the object initialization to be weaved in the .cctor even though it is not static._
- __Capgemini.Cauldron.Win32.WPF:__ _Now picks the correct Window implementation._
- __Capgemini.Cauldron.Win32.WPF:__ _Bug regarding auto-selection of views in the navigator fixed._
- __Capgemini.Cauldron.Win32.WPF:__ _Bug in CauldronTemplateSelector regarding view orientation fixed._
- __Capgemini.Cauldron.Win32.WPF:__ _Fixed a bug that caused the application to crash if there are no IMultiValueConverter in the application._
### __2.0.8 BETA__
#### Change
- __Capgemini.Cauldron.Yaml:__ _Assemblies for Net45, Net451, Net452, Net46, Net461 and Net462 added_

### Old Release Notes
### 2.0.0 betas
- NetCore and Desktop libraries combined to NetStandard 2.0
- Cauldron.XAML renamed to Cauldron.Win32.WPF
- UWP libraries are combined together to a single library
- Win32 specific libraries has now the following nomenclature: Cauldron.Win32.*
- DispatcherEx is now injectable. It uses the new UnitTest aware Factory resolver to inject either the dummy dispatcher or the real deal. 
- ComparerUtils renamed to Comparer
### 1.2.8
- Breaking changes: Reorganisation of the XAML libraries
- Skinning added (Skins from first version of Cauldron re-added)
- The App class (which inherits from ApplicationBase) is now recognized as Splashscreen if it has assigned a view. The OnPreload method is only called if a view is assigned if the navigation-mode is not SinglePage.
- Bug fixes in Interception.Fody
  - Properties as constructors now works as expected
  - ComponentConstructor attributes are now no longer removed after weaving
- Locale class can now be injected without causing an error during Assembly verification
- Singleton<> base class component construtor attribute removed, because it is redundant and causes errors.
- NetCore versions of Cauldron.Interception, Cauldron.Core and Cauldron.Activator are now part of the NuGet package again.
- Password scoring now works a bit better
- Inline text for TextBlock fixed
- XAML.Validation is now awaitable
- Module OnLoad "interceptor" added
- Bug fixes - see Incidents
### 1.2.7 (1.2.0 to 1.2.6 betas)
- NetCore Dlls droped from package due to issues with Fody - This will be back as soon as NetCore and Fody works a lot better
- Types with the component attribute get a hard-coded CreateInstance method. The Factory will use this method to create an instance of the type. This should give the factory an instancing performance almost on par with the __new__ keyword.
- Types that inherits or implements the following classes or interfaces are considered as components and will also recieve a CreateInstance method: ResourceDictionary, IValueConverter, INotifyPropertyChanged, FrameworkElement
- ComponentConstructor Attribute
  - Now also accepts static Properties as Component constructor
  - New Property added: Priority; This is used by the Factory to order the result of CreateMany. 0 is lowest; uint.Max is highest.
- Breaking changes in Assemblies class. Some methods and properties were removed without replacement.
- Method and Property interceptor can now be used to intercept all methods and properties in a class. Excluded method can be attributed by the DoNotIntercept attribute.
- CreateObject<> renamed to CreateType, which fits more to what it does.
- Locale class redesigned for more performance
- Localization source implementations now requires a component attribute
- Basic implementation base classes added for ILocalizationSource
- XML serialization/deserialization in Serializer class replaced by much faster JSON.NET
- Breaking changes in Factory - The performance of the Factory was boosted with the following drawbacks
  - IFactoryInitializeComponent interface removed - Factory does not support this anymore
  - IFactoryExtension removed - The Factory is not extensible anymore :( ... To Resolve ambigousity the IFactoryResolver can be use instead.
  - This is the current performance of the factory <br/>![Factory performance](https://raw.githubusercontent.com/Capgemini/Cauldron/feature/images/factory-performance.PNG)
- Upgraded to the newest version of Fody
- Minor bug fixes
### 1.1.4
- Inject attribute default paramaters are now "params"
- Assemblies class now throws a better error message if an Assembly cannot be loaded
- Better error message in Inject attribute
- Errors in Cauldron.Interception.Fody copy method fixed [1](https://github.com/Capgemini/Cauldron/commit/8206509d7956cc0e47c38c12249c4b68e29cb162) [2](https://github.com/Capgemini/Cauldron/commit/a1fb4a03c43689388c22e58dd2555c4a79c9170c)
- TimedCache attribute key generation fixed.
bBlock now works- Bug fix for anonymous type to interface convertion
### 1.1.1
- Several minor bug fixes
- TimedCacheAttribute now supports async methods
- Unused variables are now removed from the method's local varible list
### 1.1.0
- Cauldron.Interception is now using Cecilator
- MethodOf, FieldOf, ChildOf removed
- New Interceptor added: TimedCacheAttribute - Caches Methods using MemoryCache
- Several Bug fixes - See issues section
### 1.0.8
- CreateObject moved to Cauldron.Core
- Performance boost to CreateInstance
- IEquatable<> interface added to User class
- Minor bugs fixed
- Bug fixed that caused Cauldron.Interception.dll to be referenced with copy local set to false.

### 1.0.7
- Bug fix in Cauldron.Interception.Fody regarding nested classes and generic classes and methods
- References of the Nuget packages updated

### 1.0.6
- Inject attribute from Cauldron.Injection moved to Cauldron.Activator
  - InjectAttribute is now based on Cauldron.Interception
- Cauldron property interceptors setters can deal with IEnumerables if target property implements the IEnumerable<> interface
- Experimental ChildTypeOf method added.
- Cauldron.Activator has now an extension that can create types from interfaces.
  - CreateObject extension removed from Cauldron.Dynamic
- Cauldron.Injection removed
- Fody add-in weaver bugs fixed
- Nuget packages fixed

### 1.0.5
- Reference to Fody
- Method, fields and property interceptor added
  - Try Catch Finally implementation
  - Method, property and field interceptors with SemaphoreSlim implementation
  - methodof and fieldof implementations in Cauldron.Core.Reflection
- Cauldron.IEnumerableExtensions removed

### 1.0.4
- .NET Standard 1.6 added to NuGet package
- Missing resources in UWP packages added

### 1.0.3
- Behaviour of As<> Extension changed. It will use implicit and explicit operators if casting did not work.
- string Replace(string,char[],char) extension method added.
- Examples added to the following methods
  - ExtensionsDirectoryServices.Impersonate
  - ConsoleUtils.WriteTable
  - AsyncHelper.NullGuard
- Extensions.IsDerivedFrom<T> removed
- Extension.LowerFirstCharacter optimized
- Cauldron.XAML.Interactivity.TextBoxHeader removed
- Reference to Cauldron.UWP.XAML.Potions in Cauldron.UWP.XAML removed
- Several minor bug fixes

### 1.0.2
- ByteSizeFormatter moved to Cauldron.Core.Formatters
- MetricUnitFormatter added -> key is metric -> .ToStringEx("metric") or "{0:metric}"
- ByteSizeFormatter key changed from B to byte -> .ToStringEx("byte") or "{0:byte}"
- ToStringEx extension method added
- Java property file reader / writer added
- NavigationFrame now always retrieve the View in the following order (on UWP and Desktop)
  - Defined in ViewAttribute
    - Variants such as Mobile, Desktop, Xbox, Iot, Landscape, Portrait
  - DataTemplate
    - Variants such as Mobile, Desktop, Xbox, Iot, Landscape, Portrait
  - Type (View Type) By Name
- New method added in ApplicationBase to be able to load additional assemblies before initializing XAML / WPF
- Several minor fixes
