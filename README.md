The **Motion Language** is an experimental language strongly inspired by LISP. It has a simple syntax and is interpreted on the .NET runtime. Its purpose is to function as a functional language for transforming information. Motion is fully integrated with the .NET environment and should only be called from managed code.

However, it's important to note that Motion is not independent. You shouldn't compile Motion code into a fully complex or self-sufficient program. By default, some basic functions are defined in Motion. These functions do not expose the Motion code to the runtime without the user's permission. All operations within the Motion interpreter run in a sandbox defined by the user, with functions limited to what the user defines.

Here's a brief example of Motion source code:

```lisp
(let ((name1 "Alice")
      (name2 "Bob"))
    (str:concat name1 " and " name2
        " are friends!"))
```

This code can be invoked in C# as follows:

```csharp
string code = @"
    (let ((name1 ""Alice"")
          (name2 ""Bob""))
        (str:concat name1 "" and "" name2
            "" are friends!""))
    ";

var compilationResult = Motion.Compiler.Compile(code);
if (!compilationResult.Success)
{
    // Failed to compile
    Console.WriteLine(compilationResult.Error?.ToString());
}

// Create an execution context for the above code
var context = compilationResult.CreateContext();
var result = context.Evaluate().LastOrDefault();

Console.WriteLine(result);
```

In this example, the Motion code concatenates the names "Alice" and "Bob" to create the string "Alice and Bob are friends!"¹. The C# code demonstrates how to compile and execute this Motion code snippet.
