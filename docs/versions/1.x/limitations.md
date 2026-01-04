# Limitations

The library is currently in development and doesn't provide an exhaustive list of features and capability. As time goes on the limitations will be reduced, but this section is meant to highlight ***some*** of the known limitations.

* Direct Input Reading is not supported by this library. It is necessary to create an implementation project for the system using this. This library handles user interactions and tracks them for input processing and management but **not the actual input reading, eventing, etc. for the inputs**.
* The only currently planned support is for Godot v4 along with a suite of software for C# development. A Unity3D implementation exists for the 0.x version but it had some bugs that might make it unviable. Unless a developer decides to make a quick implementation or fix up the current Unity3D project, it will be some time before that is updated to support the latest version of the library
* *The only inputs supported*, at the moment, are ***digital, analog, and pointer*** input types. That is that motion and headset devices are not fully supported, though ***support should be able to be added using a device specification that maps the motion/etc. to an analog or similar input***. But, this will only be a 'hack' way of doing it. Library support will need to be added at some point.

>[!NOTE]
> Development on the library is continuing and support and adjustments for the above will be mitigated at some point, time allowing, or if additional developers from the community are able to help out with such implementations or updates