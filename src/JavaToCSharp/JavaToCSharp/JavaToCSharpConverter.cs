﻿using japa.parser;
using japa.parser.ast;
using japa.parser.ast.body;
using japa.parser.ast.type;
using java.lang.reflect;
using JavaToCSharp.Declarations;
using Roslyn.Compilers.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace JavaToCSharp
{
    public static class JavaToCSharpConverter
    {
        public static string ConvertText(string javaText, JavaConversionOptions options = null)
        {
            if (options == null)
                options = new JavaConversionOptions();

            options.ConversionStateChanged(ConversionState.Starting);

            var context = new ConversionContext(options);

            var textBytes = Encoding.UTF8.GetBytes(javaText ?? string.Empty);

            using (var stringreader = new MemoryStream(textBytes))
            using (var wrapper = new ikvm.io.InputStreamWrapper(stringreader))
            {
                options.ConversionStateChanged(ConversionState.ParsingJavaAST);

                var parsed = JavaParser.parse(wrapper);

                options.ConversionStateChanged(ConversionState.BuildingCSharpAST);

                var types = parsed.getTypes().ToList<TypeDeclaration>();
                var imports = parsed.getImports();
                if (imports == null || imports.isEmpty())
                    options.IncludeUsings = false;

                var package = parsed.getPackage();
                if (package == null)
                    options.IncludeNamespace = false;

                var usings = new List<UsingDirectiveSyntax>();

                //foreach (var import in imports)
                //{
                //    var usingSyntax = Syntax.UsingDirective(Syntax.ParseName(import.getName().toString()));
                //    usings.Add(usingSyntax);
                //}

                if (options.IncludeUsings)
                {
                    foreach (var ns in options.Usings)
                    {
                        var usingSyntax = Syntax.UsingDirective(Syntax.ParseName(ns));
                        usings.Add(usingSyntax);
                    }
                }

                var rootMembers = new List<MemberDeclarationSyntax>();
                NamespaceDeclarationSyntax namespaceSyntax = null;

                if (options.IncludeNamespace)
                {
                    string packageName = package.getName().toString();

                    foreach (var packageReplacement in options.PackageReplacements)
                    {
                        packageName = packageReplacement.Replace(packageName);
                    }

                    packageName = TypeHelper.Capitalize(packageName);

                    namespaceSyntax = Syntax.NamespaceDeclaration(Syntax.ParseName(packageName));
                }

                foreach (var type in types)
                {
                    if (type is ClassOrInterfaceDeclaration)
                    {
                        var classOrIntType = type as ClassOrInterfaceDeclaration;

                        if (classOrIntType.isInterface())
                        {
                            var interfaceSyntax = VisitInterfaceDeclaration(context, classOrIntType, false);

                            if (options.IncludeNamespace)
                                namespaceSyntax = namespaceSyntax.AddMembers(interfaceSyntax);
                            else if (options.IncludeClass)
                                rootMembers.Add(interfaceSyntax);
                        }
                        else
                        {
                            var classSyntax = VisitClassDeclaration(context, classOrIntType, false);

                            if (options.IncludeNamespace)
                                namespaceSyntax = namespaceSyntax.AddMembers(classSyntax);
                            else if (options.IncludeClass)
                                rootMembers.Add(classSyntax);
                            else
                                rootMembers.AddRange(classSyntax.Members.OfType<MethodDeclarationSyntax>());
                        }
                    }
                }

                if (options.IncludeNamespace)
                    rootMembers.Add(namespaceSyntax);

                var root = Syntax.CompilationUnit(
                    externs: null,
                    usings: Syntax.List(usings.ToArray()),
                    attributeLists: null,
                    members: Syntax.List<MemberDeclarationSyntax>(rootMembers))
                    .NormalizeWhitespace();

                var tree = SyntaxTree.Create(root);

                options.ConversionStateChanged(ConversionState.Done);

                return tree.GetText().ToString();
            }
        }

        private static InterfaceDeclarationSyntax VisitInterfaceDeclaration(ConversionContext context, ClassOrInterfaceDeclaration javai, bool isNested = false)
        {
            string name = "I" + javai.getName();

            if (!isNested)
                context.RootTypeName = name;

            context.LastTypeName = name;

            var classSyntax = Syntax.InterfaceDeclaration(name);

            var typeParams = javai.getTypeParameters().ToList<TypeParameter>();

            if (typeParams != null && typeParams.Count > 0)
            {
                classSyntax = classSyntax.AddTypeParameterListParameters(typeParams.Select(i => Syntax.TypeParameter(i.getName())).ToArray());
            }
            
            var mods = javai.getModifiers();

            if (mods.HasFlag(Modifier.PRIVATE))
                classSyntax = classSyntax.AddModifiers(Syntax.Token(SyntaxKind.PrivateKeyword));
            if (mods.HasFlag(Modifier.PROTECTED))
                classSyntax = classSyntax.AddModifiers(Syntax.Token(SyntaxKind.ProtectedKeyword));
            if (mods.HasFlag(Modifier.PUBLIC))
                classSyntax = classSyntax.AddModifiers(Syntax.Token(SyntaxKind.PublicKeyword));
            if (mods.HasFlag(Modifier.FINAL))
                classSyntax = classSyntax.AddModifiers(Syntax.Token(SyntaxKind.SealedKeyword));

            var implements = javai.getImplements().ToList<ClassOrInterfaceType>();

            if (implements != null)
            {
                foreach (var implement in implements)
                {
                    classSyntax = classSyntax.AddBaseListTypes(TypeHelper.GetSyntaxFromType(implement));
                }
            }

            var members = javai.getMembers().ToList<BodyDeclaration>();

            foreach (var member in members)
            {
                var syntax = BodyDeclarationVisitor.VisitBodyDeclarationForInterface(context, classSyntax, member);

                classSyntax = classSyntax.AddMembers(syntax);
            }

            return classSyntax;
        }

        private static ClassDeclarationSyntax VisitClassDeclaration(ConversionContext context, ClassOrInterfaceDeclaration javac, bool isNested = false)
        {
            string name = javac.getName();
            
            if (!isNested)
                context.RootTypeName = name;
            
            context.LastTypeName = name;

            var classSyntax = Syntax.ClassDeclaration(name);

            var typeParams = javac.getTypeParameters().ToList<TypeParameter>();

            if (typeParams != null && typeParams.Count > 0)
            {
                classSyntax = classSyntax.AddTypeParameterListParameters(typeParams.Select(i => Syntax.TypeParameter(i.getName())).ToArray());
            }

            var mods = javac.getModifiers();

            if (mods.HasFlag(Modifier.PRIVATE))
                classSyntax = classSyntax.AddModifiers(Syntax.Token(SyntaxKind.PrivateKeyword));
            if (mods.HasFlag(Modifier.PROTECTED))
                classSyntax = classSyntax.AddModifiers(Syntax.Token(SyntaxKind.ProtectedKeyword));
            if (mods.HasFlag(Modifier.PUBLIC))
                classSyntax = classSyntax.AddModifiers(Syntax.Token(SyntaxKind.PublicKeyword));
            if (mods.HasFlag(Modifier.ABSTRACT))
                classSyntax = classSyntax.AddModifiers(Syntax.Token(SyntaxKind.AbstractKeyword));
            if (mods.HasFlag(Modifier.FINAL))
                classSyntax = classSyntax.AddModifiers(Syntax.Token(SyntaxKind.SealedKeyword));

            var extends = javac.getExtends().ToList<ClassOrInterfaceType>();

            if (extends != null)
            {
                foreach (var extend in extends)
                {
                    classSyntax = classSyntax.AddBaseListTypes(TypeHelper.GetSyntaxFromType(extend));
                }
            }

            var implements = javac.getImplements().ToList<ClassOrInterfaceType>();

            if (implements != null)
            {
                foreach (var implement in implements)
                {
                    classSyntax = classSyntax.AddBaseListTypes(TypeHelper.GetSyntaxFromType(implement, true));
                }
            }

            var members = javac.getMembers().ToList<BodyDeclaration>();

            foreach (var member in members)
            {
                if (member is ClassOrInterfaceDeclaration)
                {
                    var childc = (ClassOrInterfaceDeclaration)member;

                    if (childc.isInterface())
                    {
                        var childInt = VisitInterfaceDeclaration(context, childc, true);

                        classSyntax = classSyntax.AddMembers(childInt);
                    }
                    else
                    {
                        var childClass = VisitClassDeclaration(context, childc, true);

                        classSyntax = classSyntax.AddMembers(childClass);
                    }
                }
                else
                {
                    var syntax = BodyDeclarationVisitor.VisitBodyDeclarationForClass(context, classSyntax, member);
                    classSyntax = classSyntax.AddMembers(syntax);
                }

                while (context.PendingAnonymousTypes.Count > 0)
                {
                    var anon = context.PendingAnonymousTypes.Dequeue();
                    classSyntax = classSyntax.AddMembers(anon);
                }
            }

            return classSyntax;
        }
   
        
    }
}
