using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using ConsoleLoader;

string[] possiblePaths =
{
    Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..",
        "CalculatorLib", "CalculatorLib", "bin", "Debug", "net9.0", "CalculatorLib.dll"),
    Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..",
        "CalculatorLib", "CalculatorLib", "bin", "Release", "net9.0", "CalculatorLib.dll"),
};

var dllPath = possiblePaths.Select(Path.GetFullPath).FirstOrDefault(File.Exists);

if (dllPath == null)
{
    Console.WriteLine("CalculatorLib.dll не найден. Укажите путь вручную:");
    dllPath = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(dllPath) || !File.Exists(dllPath))
    {
        Console.WriteLine("Файл не найден, выход...");
        return;
    }
}

var alcWeak = LoadAndRun(dllPath);

for (var i = 0; alcWeak.IsAlive && i < 50; i++)
{
    GC.Collect();
    GC.WaitForPendingFinalizers();
    Thread.Sleep(100);
}

Console.WriteLine(alcWeak.IsAlive
    ? "Unload failed (still alive)"
    : "Unload succeeded (collected)");
return;


static WeakReference LoadAndRun(string dllPath)
{
    var alc = new PluginLoadContext();
    var alcWeak = new WeakReference(alc);

    Assembly asm = alc.LoadFromAssemblyPath(dllPath);
    Console.WriteLine($"Loaded assembly: {asm.FullName}");

    var type = asm.GetTypes().FirstOrDefault(t =>
        t.Name == "CalculatorService" || t.GetInterface("ICalculator") != null);

    if (type == null)
    {
        Console.WriteLine("Calculator type not found");
        alc.Unload();
        return alcWeak;
    }

    Console.WriteLine($"Found type: {type.FullName}");

    var instance = Activator.CreateInstance(type);
    var evaluateMethod = type.GetMethod("Evaluate", new[] { typeof(string) });

    if (instance != null && evaluateMethod != null)
    {
        var expressions = new[] { "1+2*3", "(10-2)/4", "3.5*2+1" };
        foreach (var expr in expressions)
        {
            try
            {
                var result = evaluateMethod.Invoke(instance, new object?[] { expr });
                Console.WriteLine($"{expr} = {result}");
            }
            catch (TargetInvocationException tie)
            {
                Console.WriteLine($"Error evaluating '{expr}': {tie.InnerException?.Message}");
            }
        }
    }
    
    instance = null;
    evaluateMethod = null;
    type = null;
    asm = null;
    
    alc.Unload();

    return alcWeak;
}
