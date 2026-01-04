# Concept and Architecture

The design utilizes the following core data structures:
 - Input System Configuration
   - Device Specifications
   - Input Definitions
     - Input Actions
     - Input Schemes
       - Device Maps
   - Input Proccer Configuration
   - Join Policy

In addition to the main configuration, there are input notifications that will be sent, based on varying occurences in the input system.

The **input system configuration** is considered the **source of truth** for the input system. All information that is used to run and interpret the inputs by a user is provided by this configuration data.

There is an extensions project available to help construct this configuration file using dependency injection or a factory method when creating an integration point in a game engine or other application.

***Note: Input configurations will be validated prior to their use as the application is started. It will throw an input validation exception when debugging if there is an issue discovered with the configuration.***

# Configurations

## Device Specifications

A device specification defines what a 'device' is for the input system. This will consist of a `InputDeviceIdentifier` and a collection of `IInput`s. The device identifier is meant to define the type of controller rather than the specific name. It consists of a **device family** and a **device type**. Device family is for the specific brand or family of device, for example Xbox for xbox controllers, while the device type represents the type of input device, so GamePad for console input devices. Using this, a list of supported devices can be defined for the input system to use.

## Device Combinations

A device combination is the core mechanism the input system will determine device support. Device combinations are made up of `InputDeviceIdentifier`s and represent a collection of devices that are meant to be treated as a single controller. In some cases, like game pads, this will likely only be a combination of one, but in others, like Keyboard and Mouse, there will be two or more identifiers for the combination.

To help make creating this collection of combinations easieer, the collection will be created for the consumer based on the list of schemes and devices used for those schemes. Instead of needing to worry about setting up the correct devices, it is only required to configure them on the input shceme as desired and the configuration file will derive the combinations on its own.

## Input Definitions

Input definitions represent a main logical mapping for a set of input schemes. This will consist of the input actions and any schemes that the definition suppports. Input Actions will contain the needed data to describe, configure, and run a software defined action when the input is triggered. Input schemes represent the mappings between the input devices and the input actions that will be triggered.

An input definition represents a **template** of input. For example, you might have a set of input schemes for handling driving vehicles, flying aircraft, or handling ships. Instead of trying to create a single complex definition to support all of those configurations, it would be easier to define an input definition for each and simply switch the user's active definition. This keeps definitions as simple as they can be for the specific game mode, style of gameplay, etc. that is needed at runtime for the user.

>[!NOTE]
>**Definition Defaults**
>The current input system will allow marking more than one input definition as a default, if this happens the system will pick the first definition that it finds when needing a default definition

## Input Action

An input action is the focal point for the application to run and execute functions based on user input. It is given a unique name, desription, and other configuration for the application to process user input. These actions can be defined via a custom action object or by using a strongly typed service from dependency injection, if using the configuration extensions.

## Input Scheme

An input scheme is a specific collection of devices and input maps that tie back to the input actions on the definition. Input schemes are expected to encompass all the actions defined on the definition it relates to or users may not be able to fully utilize the desired definition as there will be missing actions. Input schemes are to have a **unique name** per definition, and can be configured as a default scheme for first choice if a scheme preference is not designated by a user. Additionally, the scheme can only consist of a single map per device, so all action maps that are defined for a device must be configured there.

>[!NOTE]
>**Scheme Defaults**
>The current input system will allow marking more than one input scheme as a default, if this happens the system will pick the first scheme that it finds when needing a default scheme

>[!NOTE]
>**Scheme Configuration**
>Given how input schemes, and the related custom schemes, are currently configured they will only be applied to a single definition.
>***This means if you want to create a default scheme for all input definitions, it must be done on a per definition level.***

A default input scheme repository is available to store scheme preferrences in memory, but this will not be kept across application start/stops and does ***does not support*** custom schemes. It is not injected into the dependency chain by default, so it must be set by the application either using a custom repository or using the default one provided.

### Device Maps

Device maps refer to the device and input ids that are used to trigger the actions. Input ids must be associated with the device or there will be issues when attempting to handle the mappings between an input and action to trigger.

### Custom Schemes

A custom scheme is a scheme that is configured by the user. It is meant to be stored and retrieved from long term, persistent storage and applied to the input system configuration at runtime. It is similar to an input scheme but is given a definition name so that it be applied to the correct definition.

## Input Processor and Join Configurations

In addition to the main configuration file, there are extra configurations for processing input and the join policy. Processing inputs requires distinguishing input actions in the abscence of integrations calling the code. For example, if an eventing system is designed to trigger on input events in an engine, it may only send Start and End phases for the input, without any indication of an active state. For these reasons, the library is configured to allow developers to specify a window of time to delay an active phase input before transitioning to active, on behalf of the integration. There is also a similar window of time that can be specified before the input system will consider an interaction 'fully ended' - that is, a consecutive input start immediately after an end can be interpreted as a 'Tap'.

For the join policy, there are three primary settings, `max users`, `pairing behavior`, `user join behavior`. This determines how the core input system on how it will handle new device activations or users. 

***Max users*** will be used to limit the total number of users in the input system. If a device pairing has been made and another is being created that could result in a new user, max users will force the device to one of the current users if the limit has been reached.

***Pairing behavior*** is used to determine if the input system should handle new device inputs automatically on the applications behalf. For example, if a game has been started and an input pressed, should the input system attempt to give the device to a user or should it be ignored? If set to manual pairing, the device will not trigger any pairing attempt or new user creations and will be expected to be performed by the application using the input manager.

***User join behavior*** determines if a new user can be created, if a device pairing might require it. For example, if a new input is detected on a device and no user exists to use it, the input system can create that user for the application's behalf. If this is set to manual, it is expected that the application will perform any needed user creation with the user manager.

## Input Notifications

There a variety of notifications that can be triggered and sent from the input system. From unrecognized devices, device pairings, disconnections/reconnections, user scheme changes etc. there are numrous events that can happen within the input system. To this end, there is an `InputSystemNotifier` object available on the input system that provides a means to listen for and respond to these notifications as needed