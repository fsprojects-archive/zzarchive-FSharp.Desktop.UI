module FSharp.Desktop.UI.BindingTests

open Xunit
open System
open System.Windows.Controls
open System.Windows.Data
open System.ComponentModel

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

[<AbstractClass>]
type ModelWithOptionFields() =
    inherit Model()

    abstract X: int option with get, set

    [<DerivedProperty>]
    member this.XStr = 
        if this.X.IsSome 
        then this.X.Value.ToString()
        else "empty"

[<Fact>]
let modelWithOptionFields() = 
    let textBox = TextBox()
    let model : ModelWithOptionFields = ModelWithOptionFields.Create()
    model.X <- None
    textBox.DataContext <- model
    Binding.OfExpression <@ textBox.Text <- model.XStr @>
    Assert.Equal<string>( "empty", textBox.Text)
    model.X <- Some 42
    Assert.Equal<string>( "42", textBox.Text)

