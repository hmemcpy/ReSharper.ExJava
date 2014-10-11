using System;
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
        public void JavaMethodsAreNotVirtualByDefault()
        {
            
        }
    }
}