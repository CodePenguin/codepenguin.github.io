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
        calculatorKernel.UseValueSharing();
        compositeKernel.Add(calculatorKernel);

        KernelInvocationContextExtensions.Display(KernelInvocationContext.Current, "Calculator has been loaded.", "text/plain");
    }
}
