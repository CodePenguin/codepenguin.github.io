using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.Events;
using Microsoft.DotNet.Interactive.ValueSharing;

namespace PolyglotCalculatorKernel;

public class CalculatorKernel :
    Kernel,
    IKernelCommandHandler<RequestCompletions>,
    IKernelCommandHandler<RequestHoverText>,
    IKernelCommandHandler<RequestValue>,
    IKernelCommandHandler<RequestValueInfos>,
    IKernelCommandHandler<SendValue>,
    IKernelCommandHandler<SubmitCode>
{
    private readonly Calculator _calculator = new();

    public CalculatorKernel(string name) : base(name)
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
}
