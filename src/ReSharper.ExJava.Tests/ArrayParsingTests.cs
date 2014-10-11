using System;
using Xunit;

namespace ReSharper.ExJava.Tests
{
    public class ArrayParsingTests
    {
        [Fact]
        public void ArrayWithInitializerParsedCorrectly()
        {
            string javaArray = @"
public void foo() {
  byte bytes[] = new byte[] { (byte)'a', (byte)'b', (byte)'c', (byte)'d' };
}";
            string result = JavaToCSharp.JavaToCSharpConverter.ConvertText(javaArray);

            Console.WriteLine(result);
        }
    }
}
