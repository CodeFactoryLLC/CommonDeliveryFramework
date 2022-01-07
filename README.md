# CommonDeliveryFramework
Framework library that provides foundational blocks of code for common application delivery needs. This supports .net standard and .net core / 5 and later.  The final output is published as a Nuget Package.  The original purpose of this framework is for any project of any type to make use of - it has special applications for anyone who is authoring a CodeFactory automation template.

## New to CodeFactory?
In the simplest terms, CodeFactory is a real time software factory that is triggered from inside Visual Studio during the design and construction of software. CodeFactory allows for development staff to automate repetitive development tasks that take up developer’s time.

Please see the following link for further information and guidance about the [CodeFactory Runtime](https://github.com/CodeFactoryLLC/CodeFactory) or the [CodeFactory SDK](https://www.nuget.org/packages/CodeFactorySDK/).

## Core purpose of the project
This project defines a set of commonly used artifacts that all modern-day applications make use of.  Currently this focuses on logging and exception handling.  By providing a set of common building blocks for logging leveraging the Microsoft.Logging namespaces while still allowing the use of 3rd party frameworks like nLog or xUnit

For Exception handling the framework defines a large selection of Types:
- Communication Exception
- Configuration Exception
- Data Exception
- External Access Exception
- Logic Exception
- Security Exception
- Timeout Exception
- Unhandled Exception
- Validation Exception

All of these different types are sub-classes of Managed Exception which wraps up and understands how to convert to the standards compliant Problem type that can safely be sent across service boundaries.

**Check out the [CDF Product Roadmap](https://github.com/CodeFactoryLLC/CommonDeliveryFramework/wiki) for planned features on the upcoming release.**

**CDF v2-beta is now available to [download](https://github.com/CodeFactoryLLC/CommonDeliveryFramework/tree/Automation-Beta)**
