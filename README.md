# OSK.Inputs
A set of core logic and implementations that are able to define the foundations of device input and their related input schemes for users. By using this library, you can set developer
defined controller definitions, schemes, and action maps using standard .NET dependency injection. This allows for separation from some specific game engines using this an abstraction library
to other implementations that may use well known input libraries, such as Unity3D, MonoGame, SDL2, and others, or even custom implementations.

# Design and Usage
The breakdown of the library follows:
 * `InputDefinition` - represents a top level input template or definition. This can be thought of as the name and other related settings for user input that is set per interaction type. For example, you may have a definition for vehicle, person, or other controls where user inputs will trigger different actions based on the type of input.
 * `InputControllerConfiguration` - this represents an actual controller of some sort and can contain one or more receivers and their input schemes. This can be specified as needed by developers when setting up their input definitions. This can be imagined as the physical xbox or playstation controller, wherein the inputs are contained
 * `InputReceiverConfiguration` - This is the actual input receiver that receives user input. This can be viewed as the mechanism directly receiving the input.
	* Note: in the scope of this library, controllers are meant to act as a container of a known input that can be used to display GUIs to a user and allow for easier developer handling for frontend interfaces. In most cases, a controller may very well only have the one receiver that receives the input but in some cases, a controller might take more than one receiver (i.e. keyboard and mouse as a single PC controller) so that when showing input schemes a developer can iterate through the list of receivers to show the user
 * `InputReceiverDescription` - This is defined on the definition and is the main way integrations are added to the library. Implementations will need to add their own receiver descriptions that will allow the library's receiver to be created using dependency injection
 * `InputScheme` - represents the action maps that are tied to specific input receivers on a controller.
 *  Inputs are broken into the following:
   * `IInput` - a central abstraction for all input types
   * `HardwareInput` - a base input that represents a physical button, joystick, or other real world hardware input
   * `VirtualInput` - a base input that represents a software based input, such as a `CombinationInput`
   * Note: each button input can be set with its own list of options that will allow for customization per inpu/receiver
 * `IInputSchemeRepository` - This is a repository that allows developers to set a custom adapter that can save/load input schemes and user selected active schemes. The default scheme repository is a no op and will only return empty/default responses.

When adding input definitions, input controllers and receivers, and their related input schemes, developers should be aware that the library will attempt to enforce all stated action keys are assigned to prevent unintentional mistakes when creating user input schemes.
Input Definitions can be set to allow or disallow custom user input schemes, but if the customer user schemes are allowed then developers will need to ensure that a valid `IInputSchemeRepository` is added to the dependency container. The provided implementation will not save or load
custom schemes and will always assume that there is only one input scheme availanble for a user.

After adding the package, input definitions, controllers, receivers, and schemes can be added similar to the following example:
```
 services.AddInputs(builder => {
    builder.AddInputDefinition("Vehicle", definition => {
        definition
            .AddAction("Fire", "Fires Main Gun")
            .AddAction("Reload", "Reloads the main Gun")

            // Allows adding custom input receivers not specified in defaults
            .AddInputController("Keyboard and Mouse", builder => {
                builder.UseMouse()
                builder.UseKeyboard()
            })
             .WithInputScheme("", isDefault: true, configurator => {

             });

             .AddPlayStationController
                .WithInputScheme("", isDefault: true, configurator => {

                })
        })
    });
```

With the dependency injection setup, the primary entry point will be the `IInputManager`, which is setup to allow saving custom input schemes as well as getting the list of input definitions
to show users on a settings page or similar. To start listening for inputs from the configuration, developers will want to get an `IInputHandler` from the input manager. The input manageer will
then provide an input handler that will listen for input across all input controllers and receivers that were set in the configuration. Additionally, the input handler will provide users a way to listen for
input controller changes should a user attempt to send input from a different controller than the original one they started with.

# Integrations
The library breaks out the inputs and their related receivers in a way that can allow for developers to only add packages for inputs and receivers that they are interested in for their application. Any new inputs being added
will need to use the `IInput` to be usable and any new input receivers will need to use the `IInputReceiver` interface as well as adding an `InputReceiverDescriptor` for the specific receiver.
The implementations for IInputReceiver are expected to contain a parameter for `InputReceiverConfiguration`. Additionally, input receivers are expected to listen to the cancellation tokens that are given and return an empty list
or the currently read list if the cancellation token is cancelled prior to completion of reading all inputs on an input receiver. If a cancellation exception is thrown, the default implementations will not catch it.

# Contributions and Issues
Any and all contributions are appreciated! Please be sure to follow the branch naming convention OSK-{issue number}-{deliminated}-{branch}-{name} as current workflows rely on it for automatic issue closure. Please submit issues for discussion and tracking using the github issue tracker.