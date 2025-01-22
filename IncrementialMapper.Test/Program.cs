// See https://aka.ms/new-console-template for more information

using IncrementialMapper.Test.Examples;

Console.WriteLine("Hello, World!");

PartialExample1 example = new PartialExample1();

PartialExample1Target _ = example.MapToPartialExample1Target(example);
// PartialExample1Target _ = example.MapToPartialExample1Target();