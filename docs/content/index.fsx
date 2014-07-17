(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use 
// it to define helpers that you do not want to show in the documentation.
#I "../../bin"
#I @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5\"

(**
F# MVC for WPF Framework
===================

Documentation

<div class="row">
  <div class="span1"></div>
  <div class="span6">
    <div class="well well-small" id="nuget">
      The F# MVC for WPF library can be <a href="https://nuget.org/packages/FSharp.Desktop.UI">installed from NuGet</a>:
      <pre>PM> Install-Package FSharp.Desktop.UI -Pre</pre>
    </div>
  </div>
  <div class="span1"></div>
</div>

Example
-------

This example demonstrates using a function defined in this sample library.

*)

#r "PresentationCore.dll"
#r "PresentationFramework.dll"
#r "WindowsBase.dll"
#r "System.Xaml.dll"

#r "FSharp.Desktop.UI.dll"

open System.Windows

let form = Window(Height= 250, Width=650, Title = "Numeric up/down")
