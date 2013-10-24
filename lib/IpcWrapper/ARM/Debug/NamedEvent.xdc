<?xml version="1.0"?><doc>
<members>
<member name="T:IpcWrapper.NamedEvent" decl="false" source="c:\users\administrator\documents\visual studio 2012\projects\hanasu\lib\ipcwrapper\namedevent.h" line="12">
<summary>
Represents a Win32 named event object that can be used to signal across foreground / background processes
</summary>
</member>
<member name="M:IpcWrapper.NamedEvent.TryOpen(System.String,IpcWrapper.NamedEvent*)" decl="true" source="c:\users\administrator\documents\visual studio 2012\projects\hanasu\lib\ipcwrapper\namedevent.h" line="18">
<summary>
Attempts to open an existing event, if it already exists
</summary>
<param name="name">The name of the event</param>
<param name="namedEvent">Returns the event, if opened</param>
<returns>True if the event was opened; false otherwise</returns>
</member>
<member name="M:IpcWrapper.NamedEvent.#ctor(System.String)" decl="true" source="c:\users\administrator\documents\visual studio 2012\projects\hanasu\lib\ipcwrapper\namedevent.h" line="26">
<summary>
Opens an existing named event. The event must already exist
</summary>
<param name="name">The name of the event to open</param>
</member>
<member name="M:IpcWrapper.NamedEvent.#ctor(System.String,System.Boolean)" decl="true" source="c:\users\administrator\documents\visual studio 2012\projects\hanasu\lib\ipcwrapper\namedevent.h" line="32">
<summary>
Creates a new named event, or opens it if it already exists
</summary>
<param name="name">The name of the event to open</param>
<param name="autoReset">True if this is an auto-reset event; false otherwise</param>
<remarks>Use the PreviouslyCreated property to tell if this was previously created or not</remarks>
</member>
<member name="M:IpcWrapper.NamedEvent.Set" decl="true" source="c:\users\administrator\documents\visual studio 2012\projects\hanasu\lib\ipcwrapper\namedevent.h" line="40">
<summary>
Signals the event. If it is an auto-reset event, will automatically be reset
</summary>
</member>
<member name="M:IpcWrapper.NamedEvent.Reset" decl="true" source="c:\users\administrator\documents\visual studio 2012\projects\hanasu\lib\ipcwrapper\namedevent.h" line="45">
<summary>
Resets the event. Only useful if this is a manual-reset event
</summary>
</member>
<member name="M:IpcWrapper.NamedEvent.WaitAsync" decl="true" source="c:\users\administrator\documents\visual studio 2012\projects\hanasu\lib\ipcwrapper\namedevent.h" line="50">
<summary>
Waits asynchronously for the event to become signalled
</summary>
<returns>Success when the event is signalled</returns>
</member>
<member name="M:IpcWrapper.NamedEvent.WaitAsync(Windows.Foundation.TimeSpan)" decl="true" source="c:\users\administrator\documents\visual studio 2012\projects\hanasu\lib\ipcwrapper\namedevent.h" line="56">
<summary>
Waits asynchronously for the event to become signalled, or until a timeout is reached
</summary>
<param name="timeout">The length of time to wait before giving up</param>
<returns>Success when the event is signalled, or Timeout if the timeout expires</returns>
</member>
<member name="P:IpcWrapper.NamedEvent.PreviouslyCreated" decl="false" source="c:\users\administrator\documents\visual studio 2012\projects\hanasu\lib\ipcwrapper\namedevent.h" line="63">
<summary>
Whether or not the event was created by someone else
</summary>
</member>
<member name="P:IpcWrapper.NamedEvent.Name" decl="false" source="c:\users\administrator\documents\visual studio 2012\projects\hanasu\lib\ipcwrapper\namedevent.h" line="68">
<summary>
The name of the event
</summary>
</member>
</members>
</doc>