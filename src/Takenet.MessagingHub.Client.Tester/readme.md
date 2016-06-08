# Messaging Hub Application Tester

This module can be used to test Messaging Hub Applications

## How it works?

To test your Messaging Hub Application using this module, do as follows:
- Create a new test project, using the test framework you like;
- Install the package `Takenet.MessagingHub.Client.Tester`
- Create a TestClass class inheriting from `Takenet.MessagingHub.Client.Tester.TestClass<TServiceProvider>`, like in [this example](https://github.com/rafaelromao/mtc2016/blob/master/MTC2016.Tests/TestClass.cs).

  Using this class, you can:
   - Register a service provider to configure mocking types that will be injected into your application during the tests.
   - Configure OneTimeSetup and OneTimeTeardown methods, to run respectively before and after each test class is executed.

- Create your test classes inheriting from your TestClass, like in [this example](https://github.com/rafaelromao/mtc2016/blob/master/MTC2016.Tests/QuestionsTests.cs)

## Example

To see a complete example of an application being tested using this module, see this [link](https://github.com/rafaelromao/mtc2016)

