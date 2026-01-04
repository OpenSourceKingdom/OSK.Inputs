# OSK.Inputs

## Project Focus

Input management across multiple game engines can vary depending on how the editors are setup to handle the mappings and related core functions that drives input actions. This library is mostly meant to be used as the intermediary between applications and the engines that read the inputs - i.e. Unity3D, Godot, or others. As of now, the intent is not to build a completely new input reading system, it's to create a focal point to managing default and custom input schemes, multiple input templates (or definitions), and allowing a way to better present these data structures to users utilizing the application.

The library contains logic to support dependency injection in a standard .NET application but also contains a factory method to support potential configuration via a game editor.

The ultimate goal is to create a single source for managing inputs and their input actions and getting this information to be displayed and managed by end users that can be easily shared across C# style engines. This should hopefully make development easier and faster as the shared logic will be consistent even if the way engines interpret inputs does not.

### Supported Engines
* **Legacy:** Unity3D was mostly supported for basic input functionality using the 0.x integration ([View Legacy Project](https://github.com/OpenSourceKingdom/OSK.Unity3D.Inputs)). This should be considered deprecated at this time. While the unity project can be updated to support the newest version of the library, there are no plans yet to do so.
* **Upcoming:** There will be support for **Godot 4.x** using a new set of repositories alongside this one. This support should be made available sometime in 2026

---

## 📖 Documentation
For further information about the library, usage, etc., please visit the **[Docs Page](https://opensourcekingdom.github.io/OSK.Inputs/)** (or check the `/docs` folder).

# Contributions and Issues
Any and all contributions are appreciated! Please be sure to follow the branch naming convention OSK-{issue number}-{deliminated}-{branch}-{name} as current workflows rely on it for automatic issue closure. Please submit issues for discussion and tracking using the github issue tracker.