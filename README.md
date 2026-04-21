# ModelBindingDeepDive

## Purpose
This repository serves as a technical deep dive into the **ASP.NET Core Model Binding** system. It is designed to move beyond basic form-to-object mapping and explore the underlying mechanisms that govern how data is retrieved, converted, and validated from various HTTP request sources.

## Core Objectives
* **Internal Mechanics:** Understanding the role of `IModelBinder` and `IModelBinderProvider` in the request lifecycle.
* **Source Priority:** Exploring the default binding order (Form, Route, Query String) and how to explicitly override them using attributes (`[FromQuery]`, `[FromRoute]`, `[FromBody]`, etc.).
* **Complex Mapping:** Demonstrating the binding of complex types, collections, and dictionaries from flat request data.
* **Validation Logic:** Integrating `ModelState` validation and custom validation attributes to ensure data integrity before it reaches the controller.

## Key Features
- **Custom Model Binders:** Implementation of specialized binders for non-standard data formats or legacy integrations.
- **Value Providers:** Customizing how the framework sources data via `IValueProvider`.
- **Type Conversion:** Handling specific parsing logic for custom structs or unique data types.
- **Minimal APIs vs. Controllers:** Comparing binding behaviors and capabilities between traditional MVC controllers and modern Minimal API endpoints.

### Credits
Credit to **Frank Liu**. Check out his [video series](https://www.youtube.com/watch?v=F4dDe0SLjJM&list=PLgRlicSxjeMOXiYY7deqzO5qKdkg9wrqM&index=1&pp=iAQB) for the original walkthrough.