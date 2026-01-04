# Release 0.x Summary

The original **0.x** versions of the library handled input processing and device reading primarily through a **polling mechanism**. 

An integration project was also developed for **Unity3D**, which can be found [here](https://github.com/OpenSourceKingdom/OSK.Unity3D.Inputs). Please note that while this project is functional for most use cases, it contains known bugs and has not been fully vetted against all input types or hardware configurations.

### ⚠️ Status: Deprecated
The 0.x versions are now **deprecated** and are no longer supported. It is strongly recommend upgrading to the latest version to take advantage of the new architecture, improved stability, and current feature set.

## Upgrading to v1.x

It's unfortunate, but there are vast breaking changes in the design and concept of the library from 0.x to 1.x; for these reasons, the process to integrate and consume the library has changed both via semantics, extensions, and integrations to support in the supported applications.

Startup files will need to be adjusted to accomodate the differences in the extensions for the dependency injection - this can performed by using the extensions project `OSK.Extensions.Inputs.Configuration` and using that to handle any updates needed for configuring via DI.
- The original mechanism for reading input was highly tied to polling and did not work well with eventing or supporting more than one mechanism for reading inputs, as such the core logic for this was removed from the library and integrations will need be adjusted to accomodate for the new design and concepts of the library. You can check out the docs for more information on this.