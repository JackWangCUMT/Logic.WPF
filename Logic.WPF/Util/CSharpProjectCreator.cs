using Logic.Core;
using Logic.WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.WPF.Util
{
    public class CSharpProjectCreator
    {
        public void Create(XBlock block, CodeViewModel vm)
        {
            string dir = System.IO.Path.GetDirectoryName(vm.ProjectPath);
            string projectPath = vm.ProjectPath;
            string propertiesDir = System.IO.Path.Combine(dir, "Properties");
            string propertiesPath = System.IO.Path.Combine(dir, "Properties", "AssemblyInfo.cs");
            string codePath = System.IO.Path.Combine(dir, vm.ClassName + ".cs");

            // C# project
            string project = GenerateCSharpProject(vm.NamespaceName, vm.ClassName);
            using (var fs = System.IO.File.CreateText(projectPath))
            {
                fs.Write(project);
            };

            // C# properties
            string properties = GenerateAssemblyInfo(vm.NamespaceName, vm.BlockName);
            System.IO.Directory.CreateDirectory(propertiesDir);
            using (var fs = System.IO.File.CreateText(propertiesPath))
            {
                fs.Write(properties);
            };

            // C# code
            string code = GenerateClas(
                block,
                vm.NamespaceName,
                vm.ClassName,
                vm.BlockName);
            using (var fs = System.IO.File.CreateText(codePath))
            {
                fs.Write(code);
            };
        }

        private string GenerateCSharpProject(string namespaceName, string className)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sb.AppendLine("<Project ToolsVersion=\"12.0\" DefaultTargets=\"Build\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">");
            sb.AppendLine("  <Import Project=\"$(MSBuildExtensionsPath)\\$(MSBuildToolsVersion)\\Microsoft.Common.props\" Condition=\"Exists('$(MSBuildExtensionsPath)\\$(MSBuildToolsVersion)\\Microsoft.Common.props')\" />");
            sb.AppendLine("  <PropertyGroup>");
            sb.AppendLine("  <MinimumVisualStudioVersion>10.0</MinimumVisualStudioVersion>");
            sb.AppendLine("    <Configuration Condition=\" '$(Configuration)' == '' \">Debug</Configuration>");
            sb.AppendLine("    <Platform Condition=\" '$(Platform)' == '' \">AnyCPU</Platform>");
            sb.AppendLine("    <ProjectGuid>{" + Guid.NewGuid().ToString().ToUpper() + "}</ProjectGuid>");
            sb.AppendLine("    <OutputType>Library</OutputType>");
            sb.AppendLine("    <AppDesignerFolder>Properties</AppDesignerFolder>");
            sb.AppendLine("    <RootNamespace>" + namespaceName + "</RootNamespace>");
            sb.AppendLine("    <AssemblyName>" + namespaceName + "</AssemblyName>");
            sb.AppendLine("    <DefaultLanguage>en-US</DefaultLanguage>");
            sb.AppendLine("    <FileAlignment>512</FileAlignment>");
            sb.AppendLine("    <ProjectTypeGuids>{786C830F-07A1-408B-BD7F-6EE04809D6DB};{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}</ProjectTypeGuids>");
            sb.AppendLine("    <TargetFrameworkProfile>Profile344</TargetFrameworkProfile>");
            sb.AppendLine("    <TargetFrameworkVersion>v4.0</TargetFrameworkVersion>");
            sb.AppendLine("  </PropertyGroup>");
            sb.AppendLine("  <PropertyGroup Condition=\" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' \">");
            sb.AppendLine("    <DebugSymbols>true</DebugSymbols>");
            sb.AppendLine("    <DebugType>full</DebugType>");
            sb.AppendLine("    <Optimize>false</Optimize>");
            sb.AppendLine("    <OutputPath>bin\\Debug\\</OutputPath>");
            sb.AppendLine("    <DefineConstants>DEBUG;TRACE</DefineConstants>");
            sb.AppendLine("    <ErrorReport>prompt</ErrorReport>");
            sb.AppendLine("    <WarningLevel>4</WarningLevel>");
            sb.AppendLine("  </PropertyGroup>");
            sb.AppendLine("  <PropertyGroup Condition=\" '$(Configuration)|$(Platform)' == 'Release|AnyCPU' \">");
            sb.AppendLine("    <DebugType>pdbonly</DebugType>");
            sb.AppendLine("    <Optimize>true</Optimize>");
            sb.AppendLine("    <OutputPath>bin\\Release\\</OutputPath>");
            sb.AppendLine("    <DefineConstants>TRACE</DefineConstants>");
            sb.AppendLine("    <ErrorReport>prompt</ErrorReport>");
            sb.AppendLine("    <WarningLevel>4</WarningLevel>");
            sb.AppendLine("  </PropertyGroup>");
            sb.AppendLine("  <ItemGroup>");
            sb.AppendLine("    <!-- A reference to the entire .NET Framework is automatically included -->");
            sb.AppendLine("    <ProjectReference Include=\"..\\..\\Logic.Core\\Logic.Core.csproj\">");
            sb.AppendLine("      <Project>{d7169bea-6ba7-4e6e-8487-b922dc1a8822}</Project>");
            sb.AppendLine("      <Name>Logic.Core</Name>");
            sb.AppendLine("    </ProjectReference>");
            sb.AppendLine("  </ItemGroup>");
            sb.AppendLine("  <ItemGroup>");
            sb.AppendLine("    <Compile Include=\"" + className + ".cs\" />");
            sb.AppendLine("    <Compile Include=\"Properties\\AssemblyInfo.cs\" />");
            sb.AppendLine("  </ItemGroup>");
            sb.AppendLine("  <Import Project=\"$(MSBuildExtensionsPath32)\\Microsoft\\Portable\\$(TargetFrameworkVersion)\\Microsoft.Portable.CSharp.targets\" />");
            sb.AppendLine("  <PropertyGroup>");
            sb.AppendLine("    <PostBuildEvent>if not exist \"$(SolutionDir)Logic.WPF\\$(OutDir)Blocks\\\" mkdir \"$(SolutionDir)Logic.WPF\\$(OutDir)Blocks\\\"");
            sb.AppendLine("copy \"$(TargetPath)\" \"$(SolutionDir)Logic.WPF\\$(OutDir)Blocks\\$(TargetFileName)\"");
            sb.AppendLine("copy \"$(TargetDir)\\$(TargetName).pdb\" \"$(SolutionDir)Logic.WPF\\$(OutDir)Blocks\\\"</PostBuildEvent>");
            sb.AppendLine("  </PropertyGroup>");
            sb.AppendLine("  <!-- To modify your build process, add your task inside one of the targets below and uncomment it. ");
            sb.AppendLine("       Other similar extension points exist, see Microsoft.Common.targets.");
            sb.AppendLine("  <Target Name=\"BeforeBuild\">");
            sb.AppendLine("  </Target>");
            sb.AppendLine("  <Target Name=\"AfterBuild\">");
            sb.AppendLine("  </Target>");
            sb.AppendLine("  -->");
            sb.AppendLine("</Project>");
            return sb.ToString();
        }

        private string GenerateAssemblyInfo(string namespaceName, string blockName)
        {
            var sb = new StringBuilder();
            sb.AppendLine("using System.Resources;");
            sb.AppendLine("using System.Reflection;");
            sb.AppendLine("using System.Runtime.CompilerServices;");
            sb.AppendLine("using System.Runtime.InteropServices;");
            sb.AppendLine("");
            sb.AppendLine("// General Information about an assembly is controlled through the following ");
            sb.AppendLine("// set of attributes. Change these attribute values to modify the information");
            sb.AppendLine("// associated with an assembly.");
            sb.AppendLine("[assembly: AssemblyTitle(\"" + namespaceName + "\")]");
            sb.AppendLine("[assembly: AssemblyDescription(\"Logic Diagram Editor " + blockName + " Block\")]");
            sb.AppendLine("[assembly: AssemblyConfiguration(\"\")]");
            sb.AppendLine("[assembly: AssemblyCompany(\"Wiesław Šoltés\")]");
            sb.AppendLine("[assembly: AssemblyProduct(\"" + namespaceName + "\")]");
            sb.AppendLine("[assembly: AssemblyCopyright(\"Copyright © Wiesław Šoltés 2014\")]");
            sb.AppendLine("[assembly: AssemblyTrademark(\"\")]");
            sb.AppendLine("[assembly: AssemblyCulture(\"\")]");
            sb.AppendLine("[assembly: NeutralResourcesLanguage(\"en\")]");
            sb.AppendLine("");
            sb.AppendLine("// Version information for an assembly consists of the following four values:");
            sb.AppendLine("//");
            sb.AppendLine("//      Major Version");
            sb.AppendLine("//      Minor Version ");
            sb.AppendLine("//      Build Number");
            sb.AppendLine("//      Revision");
            sb.AppendLine("//");
            sb.AppendLine("// You can specify all the values or you can default the Build and Revision Numbers ");
            sb.AppendLine("// by using the '*' as shown below:");
            sb.AppendLine("// [assembly: AssemblyVersion(\"1.0.*\")]");
            sb.AppendLine("[assembly: AssemblyVersion(\"1.0.0.0\")]");
            sb.AppendLine("[assembly: AssemblyFileVersion(\"1.0.0.0\")]");
            return sb.ToString();
        }

        private string GenerateClas(XBlock block, string namespaceName, string className, string blockName)
        {
            var sb = new StringBuilder();

            sb.AppendLine("using Logic.Core;");
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine("using System.Text;");
            sb.AppendLine("");
            sb.AppendLine("namespace " + namespaceName);
            sb.AppendLine("{");
            sb.AppendLine("    public class " + className + " : XBlock");
            sb.AppendLine("    {");
            sb.AppendLine("        public " + className + "()");
            sb.AppendLine("        {");
            sb.AppendLine("            base.Shapes = new List<IShape>();");
            sb.AppendLine("            base.Pins = new List<XPin>();");
            sb.AppendLine("");
            sb.AppendLine("            base.Name = \"" + blockName + "\";");
            sb.AppendLine("");

            string indent = "            ";
            foreach (var shape in block.Shapes)
            {
                if (shape is XLine)
                {
                    var line = shape as XLine;
                    var value = string.Format(
                        "{0}base.Shapes.Add(new XLine() {{ X1 = {1}, Y1 = {2}, X2 = {3}, Y2 = {4} }});",
                        indent,
                        line.X1,
                        line.Y1,
                        line.X2,
                        line.Y2);
                    sb.AppendLine(value);
                }
                else if (shape is XEllipse)
                {
                    var ellipse = shape as XEllipse;
                    var value = string.Format(
                        "{0}base.Shapes.Add(new XEllipse() {{ X = {1}, Y = {2}, RadiusX = {3}, RadiusY = {4}, IsFilled = {5} }});",
                        indent,
                        ellipse.X,
                        ellipse.Y,
                        ellipse.RadiusX,
                        ellipse.RadiusY,
                        ellipse.IsFilled.ToString().ToLower());
                    sb.AppendLine(value);
                }
                else if (shape is XRectangle)
                {
                    var rectangle = shape as XRectangle;
                    var value = string.Format(
                        "{0}base.Shapes.Add(new XRectangle() {{ X = {1}, Y = {2}, Width = {3}, Height = {4}, IsFilled = {5} }});",
                        indent,
                        rectangle.X,
                        rectangle.Y,
                        rectangle.Width,
                        rectangle.Height,
                        rectangle.IsFilled.ToString().ToLower());
                    sb.AppendLine(value);
                }
                else if (shape is XText)
                {
                    var text = shape as XText;
                    sb.AppendLine(indent + "base.Shapes.Add(");
                    sb.AppendLine(indent + "    new XText()");
                    sb.AppendLine(indent + "    {");
                    sb.AppendLine(indent + "        X = " + text.X + ",");
                    sb.AppendLine(indent + "        Y = " + text.Y + ",");
                    sb.AppendLine(indent + "        Width = " + text.Width + ",");
                    sb.AppendLine(indent + "        Height = " + text.Height + ",");
                    sb.AppendLine(indent + "        HAlignment = HAlignment." + text.HAlignment + ",");
                    sb.AppendLine(indent + "        VAlignment = VAlignment." + text.VAlignment + ",");
                    sb.AppendLine(indent + "        FontName = \"" + text.FontName + "\",");
                    sb.AppendLine(indent + "        FontSize = " + text.FontSize + ",");
                    sb.AppendLine(indent + "        Text = \"" + text.Text + "\"");
                    sb.AppendLine(indent + "    });");
                }
                else if (shape is XWire)
                {
                    // Not supported.
                }
                else if (shape is XPin)
                {
                    // Not supported.
                }
                else if (shape is XBlock)
                {
                    // Not supported.
                }
            }

            foreach (var pin in block.Pins)
            {
                var value = string.Format(
                    "{0}base.Pins.Add(new XPin() {{ Name = \"{1}\", X = {2}, Y = {3}, PinType = PinType.{4}, Owner = null }});",
                    indent,
                    pin.Name,
                    pin.X,
                    pin.Y,
                    pin.PinType);
                sb.AppendLine(value);
            }

            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }
    }
}
