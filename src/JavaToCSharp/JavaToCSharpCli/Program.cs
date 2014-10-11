﻿using JavaToCSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JavaToCSharpCli
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args == null || args.Length < 2)
            {
                Console.WriteLine("Usage: JavaToCSharpCli.exe [pathToJavaFile] [pathToCsOutputFile]");
                return;
            }
            
            if (!File.Exists(args[0]))
            {
                Console.WriteLine("Java input file doesn't exist!");
                return;
            }

            var javaText = File.ReadAllText(args[0]);

            // HACK for testing
            var options = new JavaConversionOptions()
                .AddPackageReplacement("org\\.apache\\.lucene", "Lucene.Net")
                .AddUsing("Lucene.Net")
                .AddUsing("Lucene.Net.Support")
                .AddUsing("Lucene.Net.Util");

            var parsed = JavaToCSharpConverter.ConvertText(javaText, options);

            File.WriteAllText(args[1], parsed);
            Console.WriteLine("Done!");
        }
    }
}
