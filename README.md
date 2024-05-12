# Motion Language

Motion é uma linguagem experimental, fortemente inspirada em LISP, com uma sintaxe simples, que é interpretada em cima do runtime do .NET. Seu objetivo é funcionar como uma linguagem funcional e transformar informação. É totalmente integrada ao ambiente .NET e deve ser chamada apenas por código gerenciado.

Motion não é independente, portanto, você não deve compilar um código em Motion para um programa totalmente complexo ou autossuficiente. Por padrão, algumas funções básicas são definidas em Motion. Essas funções não expõe o código Motion ao runtime sem a permissão do usuário. Toda operação dentro do interpretador do Motion é executada em um sandbox definido pelo usuário com funções limitadas ao que o usuário define.

Uma breve apresentação de um código-fonte Motion:

```lisp
(let ((name1 "Alice")
      (name2 "Bob"))
    (str:concat name1 " and " name2
        " are friends!"))
```

O que pode ser invocado no código com:

```cs
string code = """
    (let ((name1 "Alice")
          (name2 "Bob"))
        (str:concat name1 " and " name2
            " are friends!"))
    """;

var compilationResult = Motion.Compiler.Compile(code);
if (!compilationResult.Success)
{
    // failed to compile
    Console.WriteLine(compilationResult.Error?.ToString());
}

// create an execution context on the above code
var context = compilationResult.CreateContext();
var result = context.Evaluate().LastOrDefault();

Console.WriteLine(result);
```

E os objetos de resultado podem ser interpolados diretamente no código que está invocando o mesmo. É possível compartilhar variáveis, acessar constantes, definir funções customizadas e mais. Motion não deve ser confundido com LISP. Suas funções nativas são inspiradas em LISP, mas não há garantia que será executado exatamente como ocorre em LISP. 