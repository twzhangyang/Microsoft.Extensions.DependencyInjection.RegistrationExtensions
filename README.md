# Microsoft.Extensions.DependencyInjection.RegistrationExtensions
A registration extension enable you register components in ASP.NET Core like you are using Windsor Castle

As a fan of windsor castle, I would like registering components to Container in ASP.NET Core by Castle's way, But 
ASP.NET Core's DI is enough for us, A few lines of code were created enable you using API like Castle.

```
container.Register(Classes.FromAssembly().BaseOn<IOptions>().WithDefaultInterface());
container.Register(Classes.FromAssembly().BaseOn<IOptions>().WithBaseClass());
container.Register(Classes.FromAssembly().BaseOn<IOptions>().WithSelf());
container.Register(Classes.FromAssembly().BaseOn<IOptions>().WithDefaultInterfaceAndSelf());
container.Register(Classes.FromAssembly().BaseOn<IOptions>().WithBaseClassAndSelf());
```
All the extensions support open generic type:
```
container.Register(Classes.FromAssembly().BaseOn<List<>>().WithDefaultInterface());
```

### Registering components to the container
Before using DI, you need to tell the relationship between the container components, for example:

container.Register<IAService, AService>();
So when you use constructor injection, you tell the constructor to inject an instance of the IAService type, and the container will create an instance of AService based on the relationship you registered earlier.
It seems that everything is very simple, but in the real application is not so simple. Imagine that in a project, there are thousands of components, how to maintain the relationship between these thousands of components?
A slightly improved strategy is to classify components:

```
private void RegisterApplicationServices(Container container)
{    
    container.Register<IAApplicationService, AApplicationService>();
    container.Register<IBApplicationService, BApplicationService>();
    //…
}
private void RegisterDomainServices(Container container)
{
    container.Register<IADomainService, ADomainService>();
    container.Register<IBDomainService, BDomainService>();
    //…
}
private void RegisterOtherServices(Container container)
{
    container.Register<IDataTimeSource, DataTimeSource>();
    container.Register<IUserFetcher, UserFetcher>();
    //…
}
```

### Registering components based on interface
The first method has found all ApplicationServices, since the category of these components are the same, you can use the same interface to represent, define an empty interface to represent the ApplicationService:

```
public interface IApplicationService {}
public interface IAApplicationService : IApplicationService { //.. }
public interface IBApplicationService : IApplicationService { //.. }
```

Once these components have common category, try creating the following extensions:
```
container.Register(Classes.FromAssembly().BaseOn<IApplicationService>().WithDefaultInterface());
```
The meaning of this code is obvious. Scan an assembly, find all the classes that implement IApplicationService and register the component's relationship with the container.

### When a component has multiple interfaces
A class can have multiple interfaces. In actual development, such a design is also very common:

```
public interface IOptions { //... }
public interface IAlipayOptions : IOptions { //... }
public class AlipayOptions: IAlipayOptions { //... }
Register all Options with the extensions described above:

container.Register(Classes.FromAssembly().BaseOn<IOptions>().WithDefaultInterface());
```

Try to inject through the constructor below:

```
public AlipayPayment(IAlipayOptions alipayOptions) { //... }
```

Work very well, no problem. But when we try to get all the IOptions types from the container:

```
container.ResolveAll<IOptions>();
```

You don't get any instances of the IOptions type, because the process of registering the relationship with the container is one-to-one. Our previous extension: WithDefaultInterface() only registered the relationship between `AlipayOptions` and `IAlipayOptions`, if you want to get All instances that inherit IOptions you need to use another extension:

```
container.Register(Classes.FromAssembly().BaseOn<IOptions>().WithAllInterfaces());
```

### Put the registration in the correct location
We isolate the assemblies of different responsibilities in a hierarchical way, and eventually the API project will reference these low-level assemblies. To get the API up and running, you need to register all the assembly-defined components in the container of the API project. We call the startup assembly of the API a  "Client".
So a typical client needs to register all components to DI container with the following code.

```
container.Register(Classes.FromAssembly().BaseOn<IApplicationService>().WithDefaultInterface());
container.Register(Classes.FromAssembly().BaseOn<IDomainService>().WithDefaultInterface());
//…
// There are other components that cannot be represented by a public interface, which may come from low-level assemblies
container.Register<IDateTimeSource, DateTimeSource>();container.Register<IUserFetcher, UserFetcher>();
//...
```

This code describes a phenomenon in which the API client is clear about the low-level component relationships, in violation of Tell, Don't Ask Principle. The correct approach is:
The API client tells the low-level assemblies to help me install all the components in your assembly.

```
services.Install(FromAssembly.Contains<IApplicationService>());
services.Install(FromAssembly.Contains<IDomainService>());
services.Install(FromAssembly.Contains<IOtherService>());
```

The specific component relationship should be defined in the corresponding assembly.
The ideas in this section are all from Windsor Castle. The reason for not using Castle directly is that Castle is very powerful, we don't use all the features. It is also designed to reduce the complexity of the entire project.

### DI in testing
Another feature of DI is to make it easy to write valuable and effective unit tests.
When you choose to test a component, it actually takes a lot of time to prepare the dependent data, which is obvious because the component does not exist independently. Imagine if you can get this component from the container, the container will create all the dependencies for you.
But the problem is, for example, your component being tested relies on a component that can send requests to third parties. This is obviously not what you expect. You only need to register a fake pre-prepared component to replace the component that actually sent the request.
Register the faked components of ApplicationServiceTests as follows:

```
container.Install(FromAssembly.Contains<FakedComponentsInstaller>());
//..Register other components that ApplicationService depend on
```

A test for ApplicationServiceA is as follows:

```
[Fact]
public async void ApplicationServiceATest)
{    
    //Arrange
    var service = ServiceProvider.GetService<IApplicationServiceA>();
    
    //Act
    var result = await service.DoThing();
    
    //Assert
    result.Should.NotNull();
}
```

