# Motion Spec

The Motion specification contains the usage syntax, grammar, and code semantics. This specification may change over time.

Version: May 28th, 2024

## Concepts

Motion is a LISP dialect to run in the .NET environment, but it is not designed to replace languages ​​like C# or Visual Basic. Motion was built to run in their own sandbox, where the user can define functions to transform information into information, without exposing the CLR environment to insecure execution.

Most functions in Motion can be enabled or disabled by the user, and the user can define custom functions and create their own library in the Motion environment that shares CLR code. These libraries can have methods, variables and constants.

Furthermore, there are quite a few differences in syntax between Motion and other well-known dialects like Common Lisp or Clojure. Motion was adapted to work with a syntax that is more friendly to the .NET environment.

Some language syntax characteristics:

- it has `true` and `false` boolean constants.
- `nil`, the null value, doens't represents the boolean `false`.
- symbols are case-insensitive, so `hello` is evaluated to `HELLO` and vice-versa.
- the code is compiled interpreted by the Motion parser during CLR runtime.

## The program

```
program = expression* | parenthesesless-expression
```

A Motion program is the file containing Motion execution instructions. This "token" expects a list of expressions or just an expression of the Parenthesesless level, when `CompilerFeature.AllowParenthesislessCode` is enabled. A Motion program can have comments. All comments must start with `;` and are ignored until the text reader hits the character `\n`, `\r` or reaching the end of the current file reader.

## The expression

```
expression                 = '(' atom* | keyword* | expression* ')'
parenthesesless-expression = atom* | keyword* | expression*
```

An expression is a group of atoms and expressions contained between a `(` and another `)`. Expressions can evaluate to values ​​and can return data. The layout of your content is abstract, although some features are pre-defined by default, such as method invocation, for example.

The following expression has an method-invocation layout and should invoke `concat` on the next arguments:

```lisp
(concat "hello, " "world")
```

However, some methods can handle expressions with custom forms through their own invoker:

```lisp
(cond
    ((> w 10)  "w is bigger than 10")
    ((<= w 10) "w is smaller or equals than 10"))
```

In the example above, the `cond` method has its own way of dealing with information.

It is important to highlight that information that is not literal in the code is not always evaluated when sent to a method. Some methods evaluate atoms as needed, and even if present, some methods may not be evaluated. As is the case with the `and` method:

```lisp
(and true (do-foo))
```

In the code above, that doens't matter that `do-foo` is present in the list of `and`, it will never be evaluated, as `and` deals with its atoms using its own atomic invoker.

To understand more about invokers and methods, [read this](./CLR-Methods.md).

An parenthesesless expression is the same thing as an expression, but present in the first level of code, which executes code without needing the `(` and `)`. However, it requires common expressions at level-1 or above.

Both expressions can have sub-groups of expressions in their list items. In this case, all values ​​are evaluated, but only the last one is the result of the sub-group. Example:

```lisp
-> (println (10 20 30))
30
```

In the example above, all values ​​in the atomic group are interpreted, but 30, as the last atom, is the result of the expression.

## Atom

An atom is everything else. Atoms represent values. Expressions evaluate values ​​and transform them. Atoms are divided into different groups by types.

### Strings

Strings represent a string of characters. Currently, there are three different syntaxes for strings in Motion.

```
string          = '"' '\'? [^ '"']+ '"'
verbatin-string = '^"' [^ '"']+ '"'
raw-string      = '#"'* ... '"'*
```

A regular string, which has escape characters, can be expressed as:

```lisp
-> "Hello, world!"
Hello, world!
```

Strings in Motion allow line breaks within them and allow the characters below to be escaped:

- `\\` to `\` character
- `\n` to new line character
- `\r` to return character
- `\t` to tab character
- `\"` to `"` character

Verbatim strings have the same characteristics, however, they are not character escaped, except for `\"`. To signal that a string is verbatim, use the character `^` before the expression.

```lisp
-> ^"Hello, \nworld!"
Hello, \nworld!
```

Additionally, we have raw-literal-strings, which have a more customizable delimiter to define strings. Raw-strings-literals have variable-length delimiters. The size of the starting delimiter defines the size of the ending delimiter.

```lisp
-> #"hello"
hello
-> #""hello, "world"!""
hello, "world"!
-> #"""this literal contains two "" inside it!"""
this literal contains two "" inside it!
```

If you set the beginning of the expression to `#"""`, the compiler will end the string when it finds the next `"""`.

### Numbers

Numbers in Motion can be represented as integers or decimals. Numbers can also be written with `_` to help code reading. These characters are ignored by the reader.

```
number = ['+' | '-']? ['0-9' '_'] ['.']? ['0-9']
```

```lisp
-> (type-of 200)
System.Int32
-> (type-of 200.0)
System.Double
-> (type-of 200.0f)
System.Single
-> (type-of 200.0m)
System.Decimal
-> (type-of 200l)
System.Int64
-> (type-of -0.42f)
System.Single
-> (type-of 1_000_000)
System.Int32
```

### Booleans

Motion has booleans. A boolean `false` does not equate to `NIL`.

```
boolean = true | false
```

```lisp
-> (type-of true)
System.Boolean
-> (type-of false)
System.Boolean
```

### Characters

Character expressions are used to represent single characters in Motion. You can represent the character by its known name, its literal letter, or a UTF-8 hexadecimal or octal representation.

```
character = '\'[*]
```

Currently, the long form of a character is available for:

- `\newline`, evals to `"\n"`
- `\space`, evals to `" "`
- `\tab`, evals to `"\t"`
- `\formfeed`, evals to `"\f"`
- `\backspace`, evals to `"\b"`
- `\return`, evals to `"\r"`
- `\semicolon`, evals to `";"`
- `\nil`, evals to `"\0"`

To represent a single character, use as:

- `\x`, evals to `"x"`
- `\?`, evals to `"?"`
- `\$`, evals to `"$"`
- and so on...

And for characters by numeric representations:

- `\u6B`, evals to `k`
- `\o116`, evals to `N`

### Symbol

A symbol represents an identifier in Motion code. It can point to the name of a function, variable, constant or alias. A symbol represents a name for a resource.

```
symbol = ['%' | '$' | 'a-z' | 'A-Z'] ':'? ['a-z' | 'A-Z' | '0-9' | '.' | '-' | '_' ]
```

A symbol can start with `$`, `%` or any alphabetic character. The use of these characters is at the user's discretion.