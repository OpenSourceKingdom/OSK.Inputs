# Usage and Intregation

There are two paths of development for this library, either as an application consumer or as an integration. These two usage styles will differ in what is needed to handle the needed path for the developer's use.

## Consumer Usage

A consumer of the library will only need to perform two core processes to utilize the input system: configuring the input system and adding the `OSK.Inputs` library to the dependency chain. There is an optional ability to inject an application `IInputSchemeRepository` which can be used to handle application specific needs, like saving or loading custom schemes for the user.

A configuration file must be valid in order for it to be used as unexpected configuration may result in the input system not working as intended with user input interactions or application actions not being triggered. It's for this reason that calling an `IInputSystem` to initialize will result in the validation of the input system's configuration file that is provided.

 The `IInputSystem` should be managed by an ***integration*** path, the consumer for a project that has an implementation should only need to worry about initializing the needed input system configuration. Fo

***Note: If a game engine or other application does not have an implementation available for use, please consider adding one and sharing it. You can add a reference to it on the Application Support page.***

### Configuring Input Actions and mappings

Input actions can be configured either directly on the definition or by using a <T> type service on the configuration extensions. Actions must be given a unique name and an action executor, which is the function that will be triggered when the input is interacted with. This function will be given an `InputEventContext` which contains various data information about the input interaction. There is extra data related to pointer information that is not always needed for functions and is not calculated for all event actions that are triggered - i.e. **calculating pointer details is off by default** and future motion or other input types might be as well. This is because the pointer information tracks the history of the pointer by several frames, for every pointer on the screen, and will attempt to calculate as accurate of motion information as possible (i.e. velocity of the mouse, etc.). Due to potential performance costs in calculating this, albeit potentially negligible, and the fact that most functions won't need pointer information to process an event, sending this data is off by default. This can be turned on by setting the related property to include pointer information on the action that needs it.

> [!IMPORTANT]
> **Input Context Pointer Details**
> **Pointer information** being sent to the event context will be **for the specific user**, and is not sent for all users. 
>
> If two or more users have been assigned pointer inputs, each will only receive pointer details relevant to them. It **cannot be assumed** that the pointer details include all pointers present on the screen. 
> 
> This distinction helps reduce issues where pointer data might be incorrectly processed by a user who doesn't own it.

### Configuration Extensions

Creating configurations might be a bit cumbersome, so a helper project `OSK.Extensions.Inputs.Configuration` exists to make this as easy as possible. The project provides means to inject configuration via standard dependency injection into a startup file, but there is a factory method available to create the configuration via the same builders the extensions use to help integrate with potential Editor UI driven configuration.

In addition to the standard mechanism for creating action maps, the extensions project allows a developer to create actions maps out of a strongly typed class using dependency injection from the service container hosting the core input system. The extensions project will create an action for each method in the service, if it follows a project specific format. If using the extensions project, a function is considered a ***viable*** input action if it follows the following format:
 - The function is a void return type
 - The function takes a single parameter for the `InputEventContext`

Any other method will be ignored, so accidentally adding methods that do not meet the requirements will not cause any issues with the action injection into the configuration. The name of the method will become the input action's name and therefore method names must be unique, even across overloads, at this time. Additionally, using the standard mechanism will inherently turn off pointer information for the action.

If there is a desire to customize the name beyond the method or to turn other action map features, like pointer information, then you will want to use the `InputActionMapAttribute` that is available from the project. It's purpose is to allow the customization for the input action properties at the method level.

When using the input device map builder, it will require using device specifications that inherit from the `InputDeviceSpecification<T>` to allow for input protection on the input map, to ensure the inputs belong to the correct device specification.

>[!TIP]
> ** Input Device Specification Support Made Easy **
>The extensions project will also inject the device specification into the input system configuration on behalf of the user, so if using the configuration builders it is not necessary to add devices manually - simply add the schemes you want to use and it will work

### Input Processing

The input system can pause input processing by calling the `ToggleInputProcessing` method on the input system. This will cause the system to ignore inputs received during this time frame.

## Integrations

Integrations should manage two core requirements for the input system to function: reading and mapping inputs to the library and running the input system across frames. So, the integration will need to handle creating a manager to call the `IInputSystem` object and run the `Update` method, which will allow processing input data across frames.

Additionally, there should be an implementation that takes the `IInputProcessor` as a dependency and then can be used to trigger the needed events by polling, eventing, or other desired implementation.