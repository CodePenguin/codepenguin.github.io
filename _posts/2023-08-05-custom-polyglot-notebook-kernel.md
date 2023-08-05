---
layout: post
title: 'Custom Polyglot Notebook Kernel'
date: 2023-08-05 17:00:00.000000000 -05:00
tags:
- dotnet
- programming
permalink: "/2023/08/05/custom-polyglot-notebook-kernel/"
---

[Polyglot Notebooks](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.dotnet-interactive-vscode) are a great Visual Studio Code extension that allows you to create interactive Notebooks using multiple languages.

Additional language support, extra commands, formatters, etc. can be added using Dotnet Nuget packages.

For this article, we will be creating a custom kernel that will execute basic calculator commands with variable support.

## Calculator Kernel and Infrastructure

We will start by creating a new CSharp Project that will contain all of our extension classes:

```shell
dotnet new classlib -n PolyglotCalculatorKernel
```

The kernel we create will be a subkernel to the CSharp Kernel so that it can share variables with other kernels.

We need to add the `Microsoft.DotNet.Interactive` package that will handle all of the kernel interactions that we require and add a new class for our kernel:

```shell
dotnet add package Microsoft.DotNet.Interactive --prerelease
```

Edit `PolyglotCalculatorKernel.csproj` and add the following section:

```xml
<ItemGroup>
    <None Include="extension.dib" Pack="true" PackagePath="interactive-extensions/dotnet" />
</ItemGroup>
```

Rename `Class1.cs` to `CalculatorKernel.cs` and rename the class to `CalculatorKernel` as well.
CalculatorKernel needs to inherit from `Microsoft.DotNet.Interactive.Kernel` and needs a constructor that calls the base constructor as well.

```csharp
using Microsoft.DotNet.Interactive;

namespace PolyglotCalculatorKernel;

public class CalculatorKernel : Kernel
{
    public CalculatorKernel(string name) : base(name)
    {
    }
}
```

Polyglot Notebook extensions are usually distributed as a Nuget package that contains an `extension.dib` file that provides the entry point for the Notebook to load the extension.

To make things easy to setup, we will create a `NotebookExtensions` class with a static Load method that will be called by our `extensions.dib` file.
This keeps all of the code in C# so it is fully compiled.

```shell
dotnet new class --name NotebookExtensions
```

```csharp
using Microsoft.DotNet.Interactive;

namespace PolyglotCalculatorKernel;

public class NotebookExtensions
{
    public static void Load(Kernel kernel)
    {
        const string kernelName = "calculator";
        if (kernel.RootKernel is not CompositeKernel compositeKernel) return;
        if (compositeKernel.FindKernelByName(kernelName) != null) return;

        var calculatorKernel = new CalculatorKernel(kernelName);
        compositeKernel.Add(calculatorKernel);

        KernelInvocationContextExtensions.Display(KernelInvocationContext.Current, "Calculator has been loaded.", "text/plain");
    }
}
```

The NotebookExtensions.Load method checks to see if the Calculator Kernel has already been loaded.
If not loaded a new instance of the `CalculatorKernel` class which is then added to the `CSharpKernel` as a sub kernel.
Finally, we display a message so the user knows everything loaded successfully.

Create an `extensions.dib` file in the project directory with the following contents:

```text
#!csharp

PolyglotCalculatorKernel.NotebookExtensions.Load(Microsoft.DotNet.Interactive.KernelInvocationContext.Current.HandlingKernel.RootKernel);
```

This just calls our `NotebookExtensions.Load` method passing in the current root kernel from the Notebook.  
If we want to add other kernels or extensions in the future, we can just update the Load method.

## Calculator Class

We need a class that will handle of the actual calculator functionality.
Since our calculator class is not the main focus of this article, it is going to be extremely basic and will not handle things the way you would in production such as error handling, operator precedence, etc.
We just need it to work enough that we can wrap it in a Polyglot Notebook Kernel.

The calculator commands must follow the following syntax rules:

* Each number, operator, variable must be separated by a single space.
* Numbers must be integer whole numbers.
* Anything that can be parsed into a number is considered a number.
* The following operators are supported: `+` (Add), `-` (Subtract), `*` (Multiply), `=` (Assignment).
* Variables are created when a value is assigned.

Add a new `Calculator` class with the following contents:

```csharp
public class Calculator
{
    public Dictionary<string, int> Variables { get; } = new();
    private readonly List<string> _data = new();

    public int Execute(string code)
    {
        _data.Clear();
        _data.AddRange(code.Split(' '));
        int result;
        if (_data.Count == 0) return 0;
        if (_data.Count > 2 && _data[1] == "=")
        {
            var name = _data[0];
            result = Evaluate(2);
            Variables[name] = result;
        }
        else
        {
            result = Evaluate(0);
        }
        return result;
    }

    private int Evaluate(int startIndex)
    {
        var index = startIndex;
        var result = GetInt(ref index);
        while (index < _data.Count - 1)
        {
            var op = _data[index++];
            result = op switch
            {
                "+" => result + GetInt(ref index),
                "-" => result - GetInt(ref index),
                "*" => result * GetInt(ref index),
                _ => throw new InvalidOperationException($"Unknown operator: {op}")
            };
        }
        return result;
    }

    private int GetInt(ref int index)
    {
        var item = _data[index++];
        return int.TryParse(item, out var value)
            ? value
            : GetVariable(item);
    }

    private int GetVariable(string name)
    {
        return Variables.TryGetValue(name, out var value)
            ? value
            : throw new InvalidOperationException($"Unknown Variable: {name}");
    }
}
```

If we actually used the class right now in a CSharp Polyglot Notebook cell, it would be able to handle our basic operations: 

```csharp
var calc = new Calculator();
calc.Execute("1 + 2 * 3 - 4").Display(); // Returns 5
calc.Execute("Value = 1 + 2 * 3").Display(); // Returns 9
calc.Execute("Value + 1").Display(); // Returns 10
calc.Execute("Bad").Display(); // Throws an error
```

### Handling the SubmitCode command

Kernels can inherit from various interfaces to indicate that they support commands such as executing code, gathering variables, etc.

We will focus on executing code first so we will inherit from `IKernelCommandHandler<SubmitCode>` and implement the interface:

```csharp
public class CalculatorKernel :
    Kernel,
    IKernelCommandHandler<SubmitCode>
{
    private readonly Calculator _calculator = new();

    public CalculatorKernel() : base("Calculator")
    {
    }

    Task IKernelCommandHandler<SubmitCode>.HandleAsync(SubmitCode command, KernelInvocationContext context)
    {
        context.Publish(new CodeSubmissionReceived(command));
        context.Publish(new CompleteCodeSubmissionReceived(command));

        try
        {
            int result = _calculator.Execute(command.Code);

            var formattedValues = FormattedValue.CreateManyFromObject(result);
            context.Publish(new ReturnValueProduced(result, command, formattedValues));
        }
        catch (InvalidOperationException ex)
        {
            context.DisplayStandardError(ex.Message);
        }
        catch (Exception ex)
        {
            context.Fail(command, ex);
        }

        return Task.CompletedTask;
    }
}
```

We added a private field containing a new instance of our `Calculator` class that will represent the state of our kernel.

Whenever code is submitted, the `HandleAsync` method will be called with a `SubmitCode` instance.
SubmitCode.Code contains the actual text of the code.

We publish `CodeSubmissionReceived` and `CompleteCodeSubmissionReceived` messages so that the Notebook knows we received the message and will start processing it.

We then send the code we received to our calculator and get the result.
The result is then formatted and published back to the Notebook.

Any InvalidOperationExceptions are displayed as Standard Error and any other exceptions are sent back as failures.

## Testing the Calculator Kernel

We can now build and test out our calculator in a Notebook by adding a cell that loads our assembly and calls `NotebookExtensions.Load`.  Create a new Polyglot Notebook in the project directory with the following CSharp cell contents:

```csharp
#r "./PolyglotCalculatorKernel/bin/Debug/net7.0/PolyglotCalculatorKernel.dll"
PolyglotCalculatorKernel.NotebookExtensions.Load(Microsoft.DotNet.Interactive.KernelInvocationContext.Current.HandlingKernel.RootKernel);
```

We are loading the assembly directly for rapid testing but in the future we will use a Nuget package instead.
If you want to make changes, you must click "Restart" on the Polyglot Notebook so that the kernel is unloaded before you try to build it again.

Execute the cell and then create and execute a new cell that uses the new "calculator" kernel with the following contents:

```text
1 + 2 * 3 - 4
```

You should see it output "5".

## Variables

Our calculator already supports internal variables but with just a couple changes we can support external variables and expose our internal variables to other kernels.

In the Notebook, open the Visual Studio Code Command Pallete (Usually Ctrl + Shift + P on Windows or Command + Shift + P on Mac) and select "Polyglot Notebooks: Focus on Variables view".
This will open a view that displays all of the variables that are available in the Notebook.

### Accessing External Variables

Lets create a new calculator cell with the following contents:

```csharp
#!set --name x --value 5
x + 1
```

This exposes a new variable called `x` with a value of 5 that our kernel can use.
If you execute this cell now, you'll get the following errors in the Notebook:

```text
Error: Unrecognized command or argument '#!set'.
Unrecognized command or argument '--name'.
Unrecognized command or argument 'x'.
Unrecognized command or argument '--value'.
Unrecognized command or argument '5'.
```

To get this to work we need to enable [Variable Sharing](https://github.com/dotnet/interactive/blob/main/docs/variable-sharing.md) by updating `NotebookExtensions.Load` to call the `UseValueSharing` extension method on our kernel instance:

```csharp
var calculatorKernel = new CalculatorKernel(kernelName);
calculatorKernel.UseValueSharing();
compositeKernel.Add(calculatorKernel);
```

Execute the cell again and you get the following error:

```text
Error: Microsoft.DotNet.Interactive.CommandNotSupportedException: Kernel PolyglotCalculatorKernel.CalculatorKernel: calculator (kernel://pid-31897/Calculator) does not support command type SendValue.
```

This tells us that CalculatorKernel needs to support the `SendValue` command.

Update `CalculatorKernel` so that it also inherits from `IKernelCommandHandler<SendValue>` and implement the interface with the following method:

```csharp
async Task IKernelCommandHandler<SendValue>.HandleAsync(SendValue command, KernelInvocationContext context)
{
    await SetValueAsync(command, context, (name, value, declaredType) =>
    {
        if (value is int intValue || int.TryParse(value.ToString(), out intValue))
        {
            _calculator.Variables[name] = intValue;
        }
        return Task.CompletedTask;
    });
}
```

The `SetValueAsync` method comes from `Microsoft.DotNet.Interactive.Kernel` and handles some basic conversions for us.
It calls a delegate that we provide to actually set the variables.
If the variable is an int or can be parsed to an int from a string value then it will be added to our calculator.

If we click "Restart" in the Notebook, build the project and then re-run all of the cells, the last cell should now process as intended and outputs 6.

We can even take variables from other kernels such as CSharp.
Create a new cell with the following contents:

```csharp
var csharpInt = 11;
```

Execute this cell and the variable view will have a new value displayed.
We can use this `csharpInt` variable in our calculator by using the `#!set` command.
Create a new calculator cell with the following contents and the output should be 20:

```csharp
#!set --name csharpInt --value @csharp:csharpInt
csharpInt + 9
```

### Exposing Internal Variables

Now that we can get variables into our calculator, we now need to get variables out of our calculator.

Create a new calculator cell with the following contents which will create a new variable `calcInt` and set its value to 23:

```text
calcInt = 23
```

Notice how the variables view still only contains the `csharpInt` value.

Update `CalculatorKernel` so that it also inherits from `IKernelCommandHandler<RequestValueInfos>` and implement the interface with the following method:

```csharp
Task IKernelCommandHandler<RequestValueInfos>.HandleAsync(RequestValueInfos command, KernelInvocationContext context)
{
    var variables = _calculator.Variables
        .Select(v =>
        {
            var formattedValues = FormattedValue.CreateSingleFromObject(v.Value, command.MimeType);
            return new KernelValueInfo(v.Key, formattedValues, typeof(int));
        }).ToArray();
    context.Publish(new ValueInfosProduced(variables, command));
    return Task.CompletedTask;
}
```

This HandleAsync is given a `RequestValueInfos` command which contains a MimeType indicating what format the variables are being requested in.
We loop through each variable in our calculator and pass the value into `FormattedValue.CreateSingleFromObject` which will convert the value to the specified MimeType.
Finally, the list of variables is then published as a `ValueInfosProduced` message back to the Notebook.

Click "Restart" in the Notebook, rebuild the project and then click "Run All" in the Notebook.
The variable view should now have a few more items such as `calcInt`.

Let's use the `calcInt` in a CSharp kernel.
Create a new CSharp cell with the following contents:

```csharp
#!set --value @calculator:calcInt --name calcInt
Console.WriteLine(calcInt);
```

This exposes the `calcInt` variable from our calculator kernel and then writes out the value.
If you execute this cell now you will get the following error:

```text
Error: Microsoft.DotNet.Interactive.NoSuitableKernelException: No kernel found for Microsoft.DotNet.Interactive.Commands.RequestValue with target kernel 'calculator'.
```

This is because we handled the `RequestValueInfos` command to expose a list of variables but we haven't handled the `RequestValue` command to retrieve an individual value.

Update `CalculatorKernel` so that it also inherits from `IKernelCommandHandler<RequestValue>` and implement the interface with the following method:

```csharp
Task IKernelCommandHandler<RequestValue>.HandleAsync(RequestValue command, KernelInvocationContext context)
{
    if (_calculator.Variables.TryGetValue(command.Name, out var value))
    {
        context.PublishValueProduced(command, value);
    }
    else
    {
        context.Fail(command, message: $"Value '{command.Name}' not found in kernel {Name}");
    }
    return Task.CompletedTask;
}
```

This HandleAsync is given a `RequestValue` instance that has a Name property indicating what variable is being requested.
We attempt to retrieve the variable out of the calculator's list of variables and then publish the value back to the Notebook.

Click "Restart" in the Notebook, rebuild the project and then click "Run All" in the Notebook.
The last cell should now execute correctly and output the value 23.

## Auto Completion

Since the calculator kernel keeps track of all of the variables that are defined, we should be able to get a list of them from our Notebook using Auto Complete (Ctrl + Space).
We just need to handle the `RequestCompletions` kernel command.

The `RequestCompletions` command contains the `Code` that is being typed and the `LinePosition` indicating where in the code the completion is being requested for.
You can change the list of completions based on the position of the cursor within the code.

First we need to add the following method to our `Calculator` class so we can get the token at the cursor position:

```csharp
public static string GetTokenAtPosition(string code, int position, bool partialToken = false)
{
    if (string.IsNullOrEmpty(code)) return string.Empty;
    var endPosition = partialToken ? position : code.IndexOf(' ', position + 1);
    if (endPosition == -1) endPosition = code.Length;
    var codeBeforeCursor = code[..endPosition];
    var tokensBeforeCursor = codeBeforeCursor.Split(' ');
    return tokensBeforeCursor[^1];
}
```

Update `CalculatorKernel` so that it also inherits from `IKernelCommandHandler<RequestCompletions>` and implement the interface with the following method:

```csharp
Task IKernelCommandHandler<RequestCompletions>.HandleAsync(RequestCompletions command, KernelInvocationContext context)
{
    var token = Calculator.GetTokenAtPosition(command.Code, command.LinePosition.Character, partialToken: true);
    var completionList  = _calculator.Variables
        .Where(v => string.IsNullOrEmpty(token) || v.Key.StartsWith(token))
        .Select(v =>
        {
            return new CompletionItem(v.Key, typeof(int).FullName,
                insertText: string.IsNullOrEmpty(token) ? v.Key : v.Key[token.Length..],
                documentation: $"Current Value = {v.Value}");
        });
    context.Publish(new CompletionsProduced(completionList, command));
    return Task.CompletedTask;
}
```

We first get the token at the current cursor position.
If we find it, we will get a list of all variables that start with that token.
If there isn't a token at the cursor position then we get a list of all variables.
We then convert our list of variables into a list of `CompletionItem` instances and then return a `CompletionsProduced` instance that includes our converted items.

The `CodeCompletionItem.Documentation` can be set to give extra information about the item.
Since we are just returning a list of integer variables, we will show the current value of the variable which will be displayed in a hover-over window.

We also set the `CodeCompletionItem.InsertText` based on the token that we are replacing.
This lets us only insert the missing part of the selected token instead of putting the entire completed value at the cursor position which would repeat what was already typed.

## Hover Text

When you mouse over the text in a cell, we can provide some hover text that can be displayed.
For our calculator, it would be great if hovering over a variable would show the current value.
We just need to handle the `RequestHoverText` kernel command.

The `RequestHoverText` command contains the `Code` and `LinePosition` indicating where in the code the the mouse is currently hovering over.

Update `CalculatorKernel` so that it also inherits from `IKernelCommandHandler<RequestHoverText>` and implement the interface with the following method:

```csharp
Task IKernelCommandHandler<RequestHoverText>.HandleAsync(RequestHoverText command, KernelInvocationContext context)
{
    var token = Calculator.GetTokenAtPosition(command.Code, command.LinePosition.Character, partialToken: false);
    if (!string.IsNullOrWhiteSpace(token) && _calculator.Variables.TryGetValue(token, out var value))
    {
        context.Publish(
            new HoverTextProduced(
                command,
                new[] { new FormattedValue("text/markdown", $"**{token}** (Current Value = {value})") },
                new LinePositionSpan(command.LinePosition, command.LinePosition)));
    }
    return Task.CompletedTask;
}
```

We reuse our `Calculator.GetTokenAtPosition` but pass in false for `partialToken` so we get the full token at the specified position.

If the token is one of our variables we then publish a `HoverTextProduced` instance with a bit of Markdown formatted text that will be displayed in the hover text.

## Nuget Packages

So far we have been manually compiling and including our custom kernel as an assembly using the `#r "./PolyglotCalculatorKernel/bin/Debug/net7.0/PolyglotCalculatorKernel.dll"` magic command.
For final distribution, custom kernels should be packaged as Nuget Packages and loaded using something like `#r "nuget:ClockExtension,1.0.0"`.
The [ClockExtension Sample](https://github.com/dotnet/interactive/blob/main/samples/extensions/ClockExtension.ipynb) walks through the steps to build things as a Nuget package.

## Conclusion

Through a feature rich API, custom Polyglot Notebook Kernels can be created to bring other languages into Polyglot Notebooks.
You don't have to use all of the different IKernelCommandHandler<T> request types that were discussed here for every kernel.
You can mix and match based on your specific needs.

Full source code for this project: <https://github.com/CodePenguin/codepenguin.github.io/tree/main/assets/2023/08/custom-polyglot-notebook-kernel/PolyglotCalculatorKernel/>

If you have questions or comments, feel free to post in the discussion: <https://github.com/CodePenguin/codepenguin.github.io/discussions/6>