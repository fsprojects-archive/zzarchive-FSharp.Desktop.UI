(*** hide ***)

#r "PresentationCore"
#r "PresentationFramework" 
#r "System.Xaml"
#r "WindowsBase"

open System
open System.Windows
open System.Windows.Controls
open System.Windows.Data
open System.Windows.Input

#r "../../bin/FSharp.Desktop.UI.dll"

open FSharp.Desktop.UI

(**
Data binding options
====================

You can specify data binding options when calling `Binding.OfExpression` with quoted 
assignment statements. All binding options reside in the `BindingOptions` namespace. 

In the following examples, we'll use this model:
*)
open BindingOptions

[<AbstractClass>]
type PersonModel() = 
    inherit Model()

    abstract FirstName: string with get, set
    abstract LastName: string with get, set
    abstract Age: int with get, set

(**
The textboxes bound to the model properties in the associated view will use the same names:
*)
type DummyEvents = Dummy

type PersonView() as this = 
    inherit View<DummyEvents, PersonModel, Window>(Window())
    
    let FirstName = TextBox()
    let LastName = TextBox()
    let Age = TextBox()
(*** hide ***)
    let mainPanel = StackPanel()
    
    do  
        mainPanel.Children.Add FirstName |> ignore
        mainPanel.Children.Add LastName |> ignore
        mainPanel.Children.Add Age |> ignore
        this.Root.Content <- mainPanel

    override this.EventStreams = []

(**
Binding options are set by piping the binding source (the right hand side of a 
binding assignment statement in the quotation sent to Binding.OfExpression) to a 
binding option modifier function, as shown in the example below:
*)
    override this.SetBindings model =   
        Binding.OfExpression 
            <@
                FirstName.Text <- model.FirstName |> OneWay
                LastName.Text <- model.LastName |> TargetNullValue "" |> Mode BindingMode.OneTime
                Age.Text <- coerce model.Age |> UpdateSourceOnChange |> ValidatesOnExceptions true
            @>
(*** hide ***)
    member __.Quotation (model: PersonModel) = 
     <@

(**
Available data binding options
------------------------------

#### FallbackValue value
Sets [`Binding.FallbackValue`](https://msdn.microsoft.com/en-us/library/system.windows.data.bindingbase.fallbackvalue%28v=vs.110%29.aspx) 
to `value`. Example:
*)
      FirstName.Text <- model.FirstName |> FallbackValue "Couldn't get first name"
(**
-----------------------------------------------------------
#### Mode bindingMode
Sets [`Binding.Mode`](https://msdn.microsoft.com/en-us/library/system.windows.data.binding.mode%28v=vs.110%29.aspx) 
to `bindingMode`. Example:
*)
      FirstName.Text <- model.FirstName |> Mode BindingMode.OneWayToSource
(**
-----------------------------------------------------------
#### OneWay
Shortcut for setting [`Binding.Mode`](https://msdn.microsoft.com/en-us/library/system.windows.data.binding.mode%28v=vs.110%29.aspx) 
to [`BindingMode.OneWay`](https://msdn.microsoft.com/en-us/library/system.windows.data.bindingmode%28v=vs.110%29.aspx). 
Example:
*)
      FirstName.Text <- model.FirstName |> OneWay
(**
-----------------------------------------------------------
#### TargetNullValue value
Sets [`Binding.TargetNullValue`](https://msdn.microsoft.com/en-us/library/system.windows.data.bindingbase.targetnullvalue%28v=vs.110%29.aspx) 
to `value`. Example:
*)
      FirstName.Text <- model.FirstName |> TargetNullValue ""
(**
-----------------------------------------------------------
#### UpdateSource trigger
Sets [`Binding.UpdateSourceTrigger`](https://msdn.microsoft.com/en-us/library/system.windows.data.binding.updatesourcetrigger%28v=vs.110%29.aspx) 
to `trigger`. Example:
*)
      FirstName.Text <- model.FirstName |> UpdateSource UpdateSourceTrigger.Explicit
(**
-----------------------------------------------------------
#### UpdateSourceOnChange
Shortcut for setting [`Binding.UpdateSourceTrigger`](https://msdn.microsoft.com/en-us/library/system.windows.data.binding.updatesourcetrigger%28v=vs.110%29.aspx) 
to `UpdateSourceTrigger.PropertyChanged`. Example:
*)
      FirstName.Text <- model.FirstName |> UpdateSourceOnChange
(**
-----------------------------------------------------------
#### ValidatesOnDataErrors value
Sets [`Binding.ValidatesOnDataErrors`](https://msdn.microsoft.com/en-us/library/system.windows.data.binding.validatesondataerrors%28v=vs.110%29.aspx) 
to `value`. Example:
*)
      FirstName.Text <- model.FirstName |> ValidatesOnDataErrors true
(**
-----------------------------------------------------------
#### ValidatesOnExceptions value
Sets [`Binding.ValidatesOnExceptions`](https://msdn.microsoft.com/en-us/library/system.windows.data.binding.validatesonexceptions%28v=vs.110%29.aspx) 
to `value`. Example:
*)
      FirstName.Text <- model.FirstName |> ValidatesOnExceptions true
(**
-----------------------------------------------------------
*)
(*** hide ***)
        // end the quotation used for the above examples
     @>
