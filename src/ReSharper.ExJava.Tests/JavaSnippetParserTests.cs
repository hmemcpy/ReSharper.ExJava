using System;
using System.Collections.Generic;
using System.Globalization;
using JavaToCSharp;
using Shouldly;
using Xunit;

namespace ReSharper.ExJava.Tests
{
    public class JavaSnippetParserTests
    {
        [Fact]
        public void CanParseMethods()
        {
            string java = "public void foo() {}";
            string csharp = 
@"public void Foo()
{
}";
            string result = JavaSnippetParser.Parse(java);
            
            result.ShouldBe(csharp);
        }

        [Fact]
        public void MethodsAreNotVirtualByDefault()
        {
            string java = "public void foo() {}";
            string result = JavaSnippetParser.Parse(java);

            result.ShouldNotContain("virtual");
        }

        [Fact]
        public void CanParseClassesWithoutPackage()
        {
            string java = "public class Example {}";
            string csharp = 
@"public class Example
{
}";
            string result = JavaSnippetParser.Parse(java);

            result.ShouldBe(csharp);
        }

        [Fact]
        public void SysoConverterToConsoleWriteline()
        {
            string java = "public void foo() { System.out.println(\"hello\"); }";
            string csharp = 
@"public void Foo()
{
    System.Console.WriteLine(@""hello"");
}";

            string result = JavaSnippetParser.Parse(java);

            result.ShouldBe(csharp);
        }
    }
}