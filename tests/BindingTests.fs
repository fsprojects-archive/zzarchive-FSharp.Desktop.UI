module FSharp.Desktop.UI.BindingTests

open Xunit
open System

[<AbstractClass>]
type MyModel() =
    inherit Model()

    abstract Value: string with get, set

let model: MyModel = MyModel.Create()

[<Fact>]
let converter() = 
    let actual = Patterns.(|Converter|_|) <@ not( String.IsNullOrEmpty model.Value) @>
    Assert.Equal(None, actual)
