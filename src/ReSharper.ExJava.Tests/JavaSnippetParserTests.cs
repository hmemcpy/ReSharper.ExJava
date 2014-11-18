using System;
using JavaToCSharp;
using NUnit.Framework;
using Shouldly;

namespace ReSharper.ExJava.Tests
{
    public class JavaSnippetParserTests
    {
        [Test]
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

        [Test]
        public void MethodsAreNotVirtualByDefault()
        {
            string java = "public void foo() {}";
            string result = JavaSnippetParser.Parse(java);

            result.ShouldNotContain("virtual");
        }

        [Test]
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

        [Test]
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

        [TestCase("int i = 2", "int i = 2")]
        [TestCase("short s = 2", "short s = 2")]
        [TestCase("byte b = 100", "byte b = 100")]
        public void PrimitivesAreCorrectlyConverted(string javaPrimitive, string csharpPrimitive)
        {
            string java = string.Format("public void foo() {{ {0}; }}", javaPrimitive);

            string result = JavaSnippetParser.Parse(java);

            result.ShouldContain(csharpPrimitive);
        }


        [TestCase("Byte b = new Byte(2)", "byte b = 2")]
        [TestCase("Short s = new Short(2)", "short s = 2")]
        [TestCase("Integer i = new Integer(2)", "int i = 2")]
        [TestCase("Long l = new Long(2)", "long l = 2")]
        [TestCase("Long ll = new Long(2000000000000)", "long ll = 2000000000000L")]
        [TestCase("Double d = new Double(2.2)", "double d = 2.2")]
        [TestCase("Character c = new Character('a')", "char c = 'a'")]
        [TestCase("String s = new String(\"hello\")", "string s = @\"hello\"")]
        [TestCase("Boolean b = new Boolean(true)", "bool b = true")]
        [TestCase("Boolean b = new Boolean(false)", "bool b = false")]
        public void PrimitiveWrappersCorrectlyConverted(string javaPrimitive, string csharpPrimitive)
        {
            string java = string.Format("public void foo() {{ {0}; }}", javaPrimitive);

            string result = JavaSnippetParser.Parse(java);

            result.ShouldContain(csharpPrimitive);
        }
    }
}