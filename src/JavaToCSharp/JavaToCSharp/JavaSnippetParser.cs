using System;
using System.Text.RegularExpressions;
using japa.parser;

namespace JavaToCSharp
{
    public static class JavaSnippetParser
    {
        private const string method_regex = "Was expecting one of:.*\\s+\"class\".*\\s+\"enum\".*\\s+\"interface\"";
        
        public static string Parse(string code, JavaConversionOptions options = null)
        {
            try
            {
                string result = JavaToCSharpConverter.ConvertText(code, options);

                return result;
            }
            catch (ParseException e)
            {
                if (Regex.IsMatch(e.Message, method_regex))
                {
                    string w = "public class foo {\n" + code + "\n}";
                    return Parse(w,
                        new JavaConversionOptions
                        {
                            IncludeNamespace = false,
                            IncludeUsings = false,
                            IncludeClass = false
                        });
                }
            }

            return null;

        }
    }
}