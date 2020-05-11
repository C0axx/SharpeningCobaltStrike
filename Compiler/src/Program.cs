using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Validation;
using System.ComponentModel.DataAnnotations;

using Confuser.Core;
using Confuser.Core.Project;
using YamlDotNet.Serialization;

namespace Compiler
{
    public class Program
    {
        static void Main(string[] args)
        {
            CommandLineApplication app = new CommandLineApplication();
            app.HelpOption("-? | -h | --help");
            app.ThrowOnUnexpectedArgument = false;

            // Required arguments
            CommandOption OutputFileOption = app.Option(
                "-f | --file <OUTPUT_FILE>",
                "The output file to write to.",
                CommandOptionType.SingleValue
            ).IsRequired();

            // Compilation-related arguments
            CommandOption<Common.DotNetVersion> DotNetVersionOption = app.Option<Common.DotNetVersion>(
                "-d | --dotnet | --dotnet-framework <DOTNET_VERSION>",
                "The Dotnet Framework version to target (net35 or net40).",
                CommandOptionType.SingleValue
            );
            DotNetVersionOption.Validators.Add(new MustBeDotNetVersionValidator());
            CommandOption OutputKindOption = app.Option(
                "-o | --output-kind <OUTPUT_KIND>",
                "The OutputKind to use (dll or exe).",
                CommandOptionType.SingleValue
            );
            OutputKindOption.Validators.Add(new MustBeOutputKindValidator());
            CommandOption PlatformOption = app.Option(
                "-p | --platform <PLATFORM>",
                "The Platform to use (AnyCpu, x86, or x64).",
                CommandOptionType.SingleValue
            );
            PlatformOption.Validators.Add(new MustBePlatformValidator());
            CommandOption OptimizationOption = app.Option(
                "-O | --optimization",
                "Use source code optimization.",
                CommandOptionType.NoValue
            );
            PlatformOption.Validators.Add(new MustBePlatformValidator());
            CommandOption UnsafeCompilingOption = app.Option(
                "-u | --unsafecompiling",
                "Use unsafe compiling.",
                CommandOptionType.NoValue
            );
            PlatformOption.Validators.Add(new MustBePlatformValidator());
            CommandOption ConfuseOption = app.Option(
                "-c | --confuse",
                "Obfuscate with ConfuserEx",
                CommandOptionType.NoValue
            );
            // Source-related arguments
            CommandOption SourceFileOption = app.Option(
                "-s | --source-file <SOURCE_FILE>",
                "The Main source code file to compile.",
                CommandOptionType.SingleValue
            ).Accepts(v => v.ExistingFile()).IsRequired();
            //CommandOption ReferenceOption = app.Option(
            //    "-r | --references <ref1.dll,ref2.dll>",
            //    "References used by the target project",
            //    CommandOptionType.MultipleValue
            //);

            app.OnExecute(() =>
            {
                // Check dotnet version
                Common.DotNetVersion TargetDotNetVersion = DotNetVersionOption.HasValue() ? DotNetVersionOption.ParsedValue : Common.DotNetVersion.Net40;
                bool net35 = (TargetDotNetVersion.ToString().Contains("35")) ? true : false;

                // Check output format
                OutputKind TargetOutputKind = OutputKind.ConsoleApplication;
                if (OutputKindOption.HasValue())
                {
                    if (OutputKindOption.Value().Contains("console", StringComparison.OrdinalIgnoreCase) || OutputKindOption.Value().Contains("exe", StringComparison.OrdinalIgnoreCase))
                    {
                        TargetOutputKind = OutputKind.ConsoleApplication;
                    }
                    else if (OutputKindOption.Value().Contains("dll", StringComparison.OrdinalIgnoreCase) || OutputKindOption.Value().Contains("dynamicallylinkedlibrary", StringComparison.OrdinalIgnoreCase))
                    {
                        TargetOutputKind = OutputKind.DynamicallyLinkedLibrary;
                    }
                }
                else if (OutputFileOption.HasValue())
                {
                    if (OutputFileOption.Value().EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                    {
                        TargetOutputKind = OutputKind.ConsoleApplication;
                    }
                    else if (OutputFileOption.Value().EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                    {
                        TargetOutputKind = OutputKind.DynamicallyLinkedLibrary;
                    }
                }

                // Check platform
                Platform TargetPlatform = Platform.AnyCpu;
                if (PlatformOption.HasValue())
                {
                    if (PlatformOption.Value().Equals("x86", StringComparison.OrdinalIgnoreCase))
                    {
                        TargetPlatform = Platform.X86;
                    }
                    else if (PlatformOption.Value().Equals("x64", StringComparison.OrdinalIgnoreCase))
                    {
                        TargetPlatform = Platform.X64;
                    }
                    else if (PlatformOption.Value().Equals("AnyCpu", StringComparison.OrdinalIgnoreCase))
                    {
                        TargetPlatform = Platform.AnyCpu;
                    }
                }

                List<Compiler.Reference> references = net35 ? Common.Net35References : Common.Net40References;
                string TargetRefs = Path.GetDirectoryName(SourceFileOption.Value()) + Path.DirectorySeparatorChar + "Refs";
                if(File.Exists(TargetRefs)){
                    foreach(string Reference in File.ReadAllLines(TargetRefs)){
                        if(!Reference.StartsWith("#")){
                            references.Add(new Compiler.Reference
                            {
                                File = net35 ? Common.Net35Directory + Reference : Common.Net40Directory + Reference,
                                Framework = net35 ? Common.DotNetVersion.Net35 : Common.DotNetVersion.Net40,
                                Enabled = true
                            });
                        }
                    }
                }
                else
                {
                    // Import all references
                    foreach(string Reference in Directory.GetFiles(net35 ? Common.Net35Directory : Common.Net40Directory , "*.dll", SearchOption.AllDirectories)){
                        references.Add(new Compiler.Reference
                        {
                            File = Reference,
                            Framework = net35 ? Common.DotNetVersion.Net35 : Common.DotNetVersion.Net40,
                            Enabled = true
                        });
                    }
                }

                // Leaving it here for opsec pros
                // Load default references and add optional references
                //List<Compiler.Reference> references = net35 ? Common.DefaultNet35References : Common.DefaultNet40References;
                //if(ReferenceOption.HasValue()){
                //    foreach(string Reference in ReferenceOption.Value().Split(',')){
                //        references.Add(new Compiler.Reference
                //        {
                //            File = net35 ? Common.Net35Directory + Reference : Common.Net40Directory + Reference,
                //            Framework = net35 ? Common.DotNetVersion.Net35 : Common.DotNetVersion.Net40,
                //            Enabled = true
                //        });
                //    }
                //}
                // Check for Refs file in source folder
                //string TargetRefs = Path.GetDirectoryName(SourceFileOption.Value()) + Path.DirectorySeparatorChar + "Refs";
                //Console.WriteLine(TargetRefs);
                //if(File.Exists(TargetRefs)){
                //    foreach(string Reference in File.ReadAllLines(TargetRefs)){
                //        references.Add(new Compiler.Reference
                //        {
                //            File = net35 ? Common.Net35Directory + Reference : Common.Net40Directory + Reference,
                //            Framework = net35 ? Common.DotNetVersion.Net35 : Common.DotNetVersion.Net40,
                //            Enabled = true
                //        });
                //    }
                //}

                //Compile
                File.WriteAllBytes(OutputFileOption.Value(),
                Compiler.Compile(new Compiler.CompilationRequest
                {
                    Source = File.ReadAllText(SourceFileOption.Value()),
                    SourceFile = SourceFileOption.Value(),
                    SourceDirectories = new[] { Path.GetDirectoryName(SourceFileOption.Value()) },
                    TargetDotNetVersion = net35 ? Common.DotNetVersion.Net35 : Common.DotNetVersion.Net40,
                    OutputKind = TargetOutputKind,
                    Platform = TargetPlatform,
                    References = references,
                    UnsafeCompile = UnsafeCompilingOption.HasValue(),
                    Confuse = ConfuseOption.HasValue(),
                    // TODO: Fix optimization to work with GhostPack
                    Optimize = OptimizationOption.HasValue()
                }));
            });
            //
            app.Execute(args);
        }

        private class MustBeOutputKindValidator : IOptionValidator
        {
            public ValidationResult GetValidationResult(CommandOption option, ValidationContext context)
            {
                if (!option.HasValue()) { return ValidationResult.Success; }
                string val = option.Value().ToLower();

                if (val != "console" && val != "consoleapp" && val != "consoleapplication" &&
                   val != "dll" && val != "dynamicallylinkedlibrary" && val != "exe")
                {
                    return new ValidationResult($"Invalid --{option.LongName} specified.");
                }

                return ValidationResult.Success;
            }
        }

        private class MustBeDotNetVersionValidator : IOptionValidator
        {
            public ValidationResult GetValidationResult(CommandOption option, ValidationContext context)
            {
                if (!option.HasValue()) { return ValidationResult.Success; }
                string val = option.Value();

                if (!val.Contains("35") && !val.Contains("40"))
                {
                    return new ValidationResult($"Invalid --{option.LongName} specified.");
                }

                return ValidationResult.Success;
            }
        }

        private class MustBeIdentifierValidator : IOptionValidator
        {
            private static Regex identifierRegex = new Regex("^[a-zA-Z_][a-zA-Z0-9]*$");
            public ValidationResult GetValidationResult(CommandOption option, ValidationContext context)
            {
                if (!option.HasValue()) { return ValidationResult.Success; }
                string val = option.Value();
                if (!identifierRegex.IsMatch(val))
                {
                    return new ValidationResult($"Invalid --{option.LongName} specified.");
                }

                return ValidationResult.Success;
            }
        }

        private class MustBePlatformValidator : IOptionValidator
        {
            public ValidationResult GetValidationResult(CommandOption option, ValidationContext context)
            {
                if (!option.HasValue()) { return ValidationResult.Success; }
                string val = option.Value().ToLower();

                if (val != "x86" && val != "x64" && val != "anycpu")
                {
                    return new ValidationResult($"Invalid --{option.LongName} specified.");
                }

                return ValidationResult.Success;
            }
        }
    }
}