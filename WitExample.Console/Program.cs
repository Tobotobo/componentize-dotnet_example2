using HostappWorld.wit.imports.example.calculator;

var left = 123;
var right = 456;
var result = OperationsInterop.Add(left, right);
Console.WriteLine($"{left} + {right} = {result}");