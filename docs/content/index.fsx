(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use 
// it to define helpers that you do not want to show in the documentation.

(**

F# MVC for WPF
===================

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

Overview
-------

This library largely based on [my series](https://github.com/dmitry-a-morozov/fsharp-wpf-mvc-series/wiki). 
An upcoming (as of July 2014) book "[F# deep dives](http://www.manning.com/petricek2/)" has dedicated chapter #7 on slightly simplified version of the library. 
It includes real-world sample application and walk-through on the library implementation. 
I encourage everyone to [buy the book](http://www.amazon.com/F-Deep-Dives-Tomas-Petricek/dp/1617291323/). 
It is great source of collective knowledge from different F# experts.  
If you want to go deeper read [the series](https://github.com/dmitry-a-morozov/fsharp-wpf-mvc-series/wiki). 
The packaged library has minor differences from the series but overall content is still completely relevant. 

Primary focus of this documentation is to provide hands-on tutorial on building WPF application using the library. 
For implementation details read [source code](https://github.com/fsprojects/FSharp.Desktop.UI/tree/master/src) and walk through the [learning series](https://github.com/dmitry-a-morozov/fsharp-wpf-mvc-series).

Features
-------
    
* First-class event driven [architecture](ref_mvc.html) 
* Decoupled testable [controllers](ref_controller.html)
* WPF platform details abstracted away behind [generic view interface](ref_view.html)
* Simple library-idiomatic way to define [view models](ref_model.html)
* Compiler checked event to handler mapping
* Statically typed [data-binding](ref_databinding.html) 
* Declarative [derived/calculated properties](ref_databinding.html)
* Async support
* Full (UI elements and presentation logic) composition 
* External (non-visual) event sources
* Modal windows as sync, non-modal as sync computations 
* [Reentrancy problem](https://github.com/dmitry-a-morozov/fsharp-wpf-mvc-series/wiki/Reentrancy-problem) solved

Note on tutorials
-------

All tutorials try to strike a right balance between simplicity and proper coverage of real-world scenarios. 
Provide feedback if something important is missing.

*)


