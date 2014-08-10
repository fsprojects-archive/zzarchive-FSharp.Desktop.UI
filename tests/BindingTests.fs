module FSharp.Desktop.UI.BindingTests

open Xunit
open System
open System.Windows.Controls
open System.Windows.Data

[<AbstractClass>]
type MyModel() =
    inherit Model()

    abstract Value: string with get, set

let model: MyModel = MyModel.Create()

[<Fact>]
let converter() = 
    let actual = Patterns.(|Converter|_|) <@ not( String.IsNullOrEmpty model.Value) @>
    Assert.Equal(None, actual)

[<Fact>]
let uniqueSinglePropertyExpression() = 
    let textBox = TextBox()
    textBox.DataContext <- model
    Binding.OfExpression <@ textBox.IsEnabled <- model.Value <> null && model.Value <> "" @>
    Assert.False( textBox.IsEnabled)
    model.Value <- "text"
    Assert.True( textBox.IsEnabled)
