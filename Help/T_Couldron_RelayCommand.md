# RelayCommand Class
 _**\[This is preliminary documentation and is subject to change.\]**_

Implements the <a href="T_Couldron_IRelayCommand">IRelayCommand</a> interface


## Inheritance Hierarchy
System.Object<br />&nbsp;&nbsp;Couldron.RelayCommand<br />
**Namespace:**&nbsp;<a href="N_Couldron">Couldron</a><br />**Assembly:**&nbsp;Couldron (in Couldron.dll) Version: 1.0.0.0 (1.0.0.0)

## Syntax

**C#**<br />
``` C#
public class RelayCommand : IRelayCommand, 
	ICommand, INotifyPropertyChanged
```

The RelayCommand type exposes the following members.


## Constructors
&nbsp;<table><tr><th></th><th>Name</th><th>Description</th></tr><tr><td>![Public method](media/pubmethod.gif "Public method")</td><td><a href="M_Couldron_RelayCommand__ctor">RelayCommand(Action)</a></td><td>
Initializes a new instance of RelayCommand class</td></tr><tr><td>![Public method](media/pubmethod.gif "Public method")</td><td><a href="M_Couldron_RelayCommand__ctor_1">RelayCommand(Action, Func(Boolean))</a></td><td>
Initializes a new instance of RelayCommand class</td></tr></table>&nbsp;
<a href="#relaycommand-class">Back to Top</a>

## Properties
&nbsp;<table><tr><th></th><th>Name</th><th>Description</th></tr><tr><td>![Public property](media/pubproperty.gif "Public property")</td><td><a href="P_Couldron_RelayCommand_IsEnabled">IsEnabled</a></td><td>
Gets or sets a value that indicates if the assiociated control is disabled or enabled</td></tr></table>&nbsp;
<a href="#relaycommand-class">Back to Top</a>

## Methods
&nbsp;<table><tr><th></th><th>Name</th><th>Description</th></tr><tr><td>![Public method](media/pubmethod.gif "Public method")</td><td><a href="M_Couldron_RelayCommand_CanExecute">CanExecute</a></td><td>
Defines the method that determines whether the command can execute in its current state.</td></tr><tr><td>![Public method](media/pubmethod.gif "Public method")</td><td>Equals</td><td> (Inherited from Object.)</td></tr><tr><td>![Public method](media/pubmethod.gif "Public method")</td><td><a href="M_Couldron_RelayCommand_Execute">Execute</a></td><td>
Defines the method to be called when the command is invoked.</td></tr><tr><td>![Protected method](media/protmethod.gif "Protected method")</td><td>Finalize</td><td> (Inherited from Object.)</td></tr><tr><td>![Public method](media/pubmethod.gif "Public method")</td><td>GetHashCode</td><td> (Inherited from Object.)</td></tr><tr><td>![Public method](media/pubmethod.gif "Public method")</td><td>GetType</td><td> (Inherited from Object.)</td></tr><tr><td>![Protected method](media/protmethod.gif "Protected method")</td><td>MemberwiseClone</td><td> (Inherited from Object.)</td></tr><tr><td>![Protected method](media/protmethod.gif "Protected method")</td><td><a href="M_Couldron_RelayCommand_OnCanExecuteChanged">OnCanExecuteChanged</a></td><td>
Occures if the <a href="M_Couldron_RelayCommand_RefreshCanExecute">RefreshCanExecute()</a> method has been invoked</td></tr><tr><td>![Public method](media/pubmethod.gif "Public method")</td><td><a href="M_Couldron_RelayCommand_RefreshCanExecute">RefreshCanExecute</a></td><td>
Triggers the CanExecuteChanged event and forces the control to refresh the execution state</td></tr><tr><td>![Public method](media/pubmethod.gif "Public method")</td><td>ToString</td><td> (Inherited from Object.)</td></tr></table>&nbsp;
<a href="#relaycommand-class">Back to Top</a>

## Events
&nbsp;<table><tr><th></th><th>Name</th><th>Description</th></tr><tr><td>![Public event](media/pubevent.gif "Public event")</td><td><a href="E_Couldron_RelayCommand_CanExecuteChanged">CanExecuteChanged</a></td><td>
Occurs when changes occur that affect whether or not the command should execute.</td></tr><tr><td>![Public event](media/pubevent.gif "Public event")</td><td><a href="E_Couldron_RelayCommand_PropertyChanged">PropertyChanged</a></td><td>
Occurs when a property value changes.</td></tr></table>&nbsp;
<a href="#relaycommand-class">Back to Top</a>

## Extension Methods
&nbsp;<table><tr><th></th><th>Name</th><th>Description</th></tr><tr><td>![Public Extension Method](media/pubextension.gif "Public Extension Method")</td><td><a href="M_Couldron_Extensions_CastTo__1">CastTo(T)</a></td><td>
Performs certain types of conversions between compatible reference types or nullable types 

 Returns null if convertion was not successfull
 (Defined by <a href="T_Couldron_Extensions">Extensions</a>.)</td></tr><tr><td>![Public Extension Method](media/pubextension.gif "Public Extension Method")</td><td><a href="M_Couldron_Extensions_DisposeAll">DisposeAll</a></td><td>
Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources. 

 This will dispose an object if it implements the IDisposable interface. 

 If the object is a FrameworkElement it will try to find known diposable attached properties. 

 It will also dispose the the DataContext content.
 (Defined by <a href="T_Couldron_Extensions">Extensions</a>.)</td></tr><tr><td>![Public Extension Method](media/pubextension.gif "Public Extension Method")</td><td><a href="M_Couldron_ExtensionConvertions_ToBool">ToBool</a></td><td>
Tries to convert an Object to a Boolean
 (Defined by <a href="T_Couldron_ExtensionConvertions">ExtensionConvertions</a>.)</td></tr><tr><td>![Public Extension Method](media/pubextension.gif "Public Extension Method")</td><td><a href="M_Couldron_ExtensionConvertions_ToDouble">ToDouble</a></td><td>
Tries to convert a Object to an Double
 (Defined by <a href="T_Couldron_ExtensionConvertions">ExtensionConvertions</a>.)</td></tr><tr><td>![Public Extension Method](media/pubextension.gif "Public Extension Method")</td><td><a href="M_Couldron_ExtensionConvertions_ToInteger">ToInteger</a></td><td>
Tries to convert a Object to an Int32
 (Defined by <a href="T_Couldron_ExtensionConvertions">ExtensionConvertions</a>.)</td></tr><tr><td>![Public Extension Method](media/pubextension.gif "Public Extension Method")</td><td><a href="M_Couldron_ExtensionConvertions_ToString2">ToString2</a></td><td>
Returns a string that represents the current object. 

 If the object is null a Empty will be returned
 (Defined by <a href="T_Couldron_ExtensionConvertions">ExtensionConvertions</a>.)</td></tr></table>&nbsp;
<a href="#relaycommand-class">Back to Top</a>

## See Also


#### Reference
<a href="N_Couldron">Couldron Namespace</a><br />