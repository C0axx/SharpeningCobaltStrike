// Author: Ryan Cobb (@cobbr_io)
// Project: Covenant (https://github.com/cobbr/Covenant)
// License: GNU GPLv3

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Compiler
{
    public static class Common
    {

        public static Encoding CompilerEncoding = Encoding.UTF8;
        public static string CompilerDirectory = SplitFirst(SplitFirst(Assembly.GetExecutingAssembly().Location, "bin"), "Compiler.dll");
        public static string CompilerReferencesDirectory = CompilerDirectory + "References" + Path.DirectorySeparatorChar;
        public static string CompilerReferencesConfig = CompilerReferencesDirectory + "references.yml";
        public static string Net35Directory = CompilerReferencesDirectory + "net35" + Path.DirectorySeparatorChar;
        public static string Net40Directory = CompilerReferencesDirectory + "net40" + Path.DirectorySeparatorChar;
        public static string CompilerResourcesDirectory = CompilerDirectory + "Resources" + Path.DirectorySeparatorChar;
        public static string CompilerOutputDirectory = CompilerDirectory + "Output" + Path.DirectorySeparatorChar;
        public static string CompilerRefsDirectory = CompilerDirectory + "refs" + Path.DirectorySeparatorChar;
        private static string SplitFirst(string FullString, string SubString)
        {
            return FullString.Contains(SubString) ? FullString.Substring(0, FullString.IndexOf(SubString)) : FullString;
        }

        public static List<Compiler.Reference> DefaultNet35References = new List<Compiler.Reference>
        {
            new Compiler.Reference { File = Net35Directory + "mscorlib.dll", Framework = DotNetVersion.Net35, Enabled = true },
            new Compiler.Reference { File = Net35Directory + "System.dll", Framework = DotNetVersion.Net35, Enabled = true },
            new Compiler.Reference { File = Net35Directory + "System.Core.dll", Framework = DotNetVersion.Net35, Enabled = true },
        };

        public static List<Compiler.Reference> DefaultNet40References = new List<Compiler.Reference>
        {
            new Compiler.Reference { File = Net40Directory + "mscorlib.dll", Framework = DotNetVersion.Net40, Enabled = true },
            new Compiler.Reference { File = Net40Directory + "System.dll", Framework = DotNetVersion.Net40, Enabled = true },
            new Compiler.Reference { File = Net40Directory + "System.Core.dll", Framework = DotNetVersion.Net40, Enabled = true }
        };

        public static List<Compiler.Reference> DefaultNetFrameworkReferences = new List<Compiler.Reference>
        {
            new Compiler.Reference { File = Net35Directory + "mscorlib.dll", Framework = DotNetVersion.Net35, Enabled = true },
            new Compiler.Reference { File = Net40Directory + "mscorlib.dll", Framework = DotNetVersion.Net40, Enabled = true },
            new Compiler.Reference { File = Net35Directory + "System.dll", Framework = DotNetVersion.Net35, Enabled = true },
            new Compiler.Reference { File = Net40Directory + "System.dll", Framework = DotNetVersion.Net40, Enabled = true },
            new Compiler.Reference { File = Net35Directory + "System.Core.dll", Framework = DotNetVersion.Net35, Enabled = true },
            new Compiler.Reference { File = Net40Directory + "System.Core.dll", Framework = DotNetVersion.Net40, Enabled = true }
        };

        public static List<Compiler.Reference> Net40References = new List<Compiler.Reference>{};

        public static List<Compiler.Reference> Net35References = new List<Compiler.Reference>{};

        public enum DotNetVersion
        {
            Net40,
            Net35,
            NetCore21
        }
    }
}
