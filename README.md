# OSK.Inputs
A set of core logic and implementations that are able to define the foundations of device input and their related input schemes for users. By using this library, you can set developer
defined controller definitions, schemes, and action maps using standard .NET dependency injection. This allows for separation from some specific game engines using this abstraction library
to other implementations that may use well known input libraries, such as Unity3D, MonoGame, SDL2, and others, or even custom implementations. Additionally, the library is also setup to support
local multiplayer across users and devices associated to a given game engine.

# Design and Usage
The core logic and implementations relies on a the following interfaces and components:
* `IInputManager`:  represents the fundamental access point to the library and its usage. This handles the `IApplicationInputUser`s and their respective devices and actions. Users can call the receive input function to retrieve inputs from users
* `IInputDeviceReader`: the core integration point for input systems utilizing the abstractions associated to this library. By using this interface, you can add new input devices to be available to users
* `InputSystemConfiguration`: the core configuration object that defines how the input manager and application users operate. This contains data related to the input definitions, schemes, and related actions that are triggered upon receiving input
* `InputDefinition`: the definition file that specifies what actions are available for triggering and the related input schemes associated to the definition
* `InputScheme`: the configuration of a set of input devices that are associated to a given input definition. All actions defined in the input definition must be mapped to inputs in the input scheme

To add the input library to a DI container, the `AddInputs` method should be utilized and configuration set using the `IInputSystemBuilder` configuration action

Input System Configuration and Validation
----
Input validation takes place during the startup phase of the DI container. The input system's input definitions, schemes, input devices, etc. are all checked, as much as possible, to validate the input system is correctly configured before being
used with the `IInputManager`. This validation is unskippable.

The input system configuration can be setup to allow for multiple local users as well as with the use of custom input schemes that can be defined by users. There is a default `IInputSchemeRepository` that allows for in-memory input scheme storage, 
but long term storage can be implemented by adding a custom `IInputSchemeRepository` implementation using the `UseInputSchemeRepository` method on the `IInputSystemBuilder`

Input Definitions and Schemes
----
An Input Definition is the template by which input schemes are created. An input definition can be thought of as a separation of actions that are available to a user. For example, a game application may allow users acces to a variety of vehicles and aircraft
which could require separate input definitions to perform the needed actions (i.e. moving a tank does not require actions for pitch, yaw, etc.). Input definitions can be set by a user at runtime to allow for definitions to switched as needed by an application 
 (i.e. user switches from tank to aircraft to infantry, etc.). Additionally, action events must be set with the associated actions, which are used to trigger developer code when an input is read from an input device.
 Some extensions are available in `InputDefinitionBuilderExtensions` to help configure input definitions and their actions. 

Input schemes must define a mapping for every action available on an input definition. This is to help prevent unexpected behavior with missing actions on schemes.

Input Devices and Controllers
----
The core concepts for how input is read is through individual `IInputDeviceReader`. An input device can be thought of as the actual device that receives input from a user, so this would be similar to a keyboard, mouse, xbox or playstation controllers, etc.
The `IInputDeviceReader` is a specific input device reader and this allows potential for different input device implementations to be used to read input on a per device basis. New input device readers can be configured using the
extensions found in `InputSystemBuilderExtensions`, with some common configurations being available on the extension API surface. These extensions expect that an implementation `IInputDeviceReader` is provided along with the associated `InputDevice` object.
The `InputDevice` defines the types of inputs the device utilizes. 

An Input Controller is defined as a set of input devices that should be allowed to share in a specific input scheme. For example, an input scheme might have an input controller that contains both a Keyboard and Mouse input device and thus all actions for an
input definition can be mapped to both the keyboard and mouse within the same scheme. Input controllers are handled internally within the library and are created automatically based on the input scheme configurations created during startup.

Inputs and Virtual Inputs
----
Inputs are utilized by `InputDevice`s during configuration so that `IInputDeviceReader`s know what to read when an input device is receiving input. Inputs are defined by `IInput` but the two primary implementations provided by the library
is `HardwareInput` and `VirtualInput`. Hardware input can be thought of as the physical button or sticks on a controller or keyboard, whereas virtual inputs can be thought of as an input defined by software. For example, combination inputs 
are software based inputs (Shift + 1 for '!'). Custom implementations of `IInput` can be defined as long as the `IInputDeviceReader` used with the input is able to understand how to interpret it.

Some virtual inputs are defined by the library and are available for use. These include:
* Combination Inputs
* Swipe Inputs are planned for addition in the future

Note: validation is performed on the input device reader to ensure that the input is valid for the device reader.


# Input Device Integrations
The primary interface to implement is the `IInputDeviceReader` as it is what is used to actually read an input from a device. This happens through a single input read method that is called during input reading of a user's assigned input devices.
The device reader should inform the library of the state of the input being requested through the input read context that is passed to the read method along with the actual input for processing. The input device reader implementation does not need
to be aware of virtual inputs as those are defined and processed by the internals of the library.

Note: Input Device readers are created by the library using the input reader type defined when using the input system builder. The readers are currently expected to be transiently generated by default through the current library implementation for 
`IInputReaderProvider`. Though it should be possible for consumers to define a custom implementation of `IInputReaderProvider` if it is added to the DI container prior to the core service collection extension being used.

# Contributions and Issues
Any and all contributions are appreciated! Please be sure to follow the branch naming convention OSK-{issue number}-{deliminated}-{branch}-{name} as current workflows rely on it for automatic issue closure. Please submit issues for discussion and tracking using the github issue tracker.