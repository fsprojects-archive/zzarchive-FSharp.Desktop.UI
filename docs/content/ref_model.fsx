(*** hide ***)
#r "../../bin/FSharp.Desktop.UI.dll"

open FSharp.Desktop.UI

(**

Model
========================

General requirements
-------
 - Implement [INotifyPropertyChanged](http://msdn.microsoft.com/en-us/library/ms229614.aspx) (this restriction enforced by type constrain on Mvc type) 
 - Optionally implement [INotifyDataErrorInfo](http://msdn.microsoft.com/en-us/library/system.componentmodel.inotifydataerrorinfo.aspx) to support user-input validation

Library-idiomatic way to define model types
-------
 - Inherit from FSharp.Desktop.UI.Model 
 - Mark type with `[<AbstractClassAttribute>]`
 - Define as abstract read/write properties participating in data-binding
   * Virtual properties INotifyPropertyChanged auto-wiring is not supported
 - Use Model.Create<'Model> static factory method to create model instances.
  
*)

[<AbstractClass>]
type PersonModel() = 
    inherit Model()

    abstract FirstName: string with get, set
    abstract LastName: string with get, set
    [<DerivedProperty>]
    member this.FullName = sprintf "%s %s" this.FirstName this.LastName
    abstract Age: int with get, set

let model: PersonModel = PersonModel.Create()

(**
Hand-written property definitions
-------
If for some reason (like performance) you prefer to roll-out you own properties definitions 
you still can leverage base class helper methods to send PropertyChanged notifications. 
It can be mixed with abstract auto-wired properties.
*)

[<AbstractClass>]
type PersonModel2() = 
    inherit Model()

    let mutable firstName: string = null
    let mutable lastName: string = null

    member this.FirstName 
        with get() = firstName 
        and set value = 
            firstName <- value
            //raise notification manually
            this.NotifyPropertyChanged "FirstName"
            //Update of property value clears up errors for auto-wired properties.
            //For had-written properties it is left up you 
            //this.SetErrors("FirstName")

    member this.LastName
        with get() = lastName 
        and set value = 
            lastName <- value
            //A safer alternative is to use quotation-base overload
            this.NotifyPropertyChanged <@ this.LastName @>
            //Same as for FirstName property
            //this.SetErrors("LastName")

    //other properties can be kept as abstract therefore auto-wired
    abstract Age: int with get, set

(**
Alternative ways to implement models     
-------
  - Fully hand-written implementation
  - [Fody/PropertyChanged](https://github.com/Fody/PropertyChanged) is valuable alternative.
  - Other ... (maybe one day Roslyn-based solution built into Visual Studio which is essentially similar to Fody.PropertyChanged)    

Don not forget about INotifyDataErrorInfo if you need to support data-input errors notifications

The library tries to be both powerful and composable (flexible). 
For that reason it accepts many default components can be swapped for custom ones as long as they conform a same interface. 
This is applicable to model implementation too. 

Note on conceptual design
-------
  - Model is holder of _visual state_
  - Another way to think about model is that it's a projection (via data binding) of some GUI elements properties
  - Ideally model has parameter-less constructor (although Model.Create factory method supports parameter-full constructors)

<img src="img/ModelConcept.png"/>

*)
