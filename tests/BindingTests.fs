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


module BindingOptionTests =
    open BindingOptions
    [<Fact>]
    let mode() = 
        let textBox = TextBox()
        let model: MyModel = MyModel.Create()
        textBox.DataContext <- model
        Binding.OfExpression <@ textBox.Text <- model.Value |> Mode BindingMode.OneTime @>
        let binding = textBox.GetBindingExpression(TextBox.TextProperty).ParentBinding
        Assert.Equal(BindingMode.OneTime, binding.Mode)

    [<Fact>]
    let oneWay() = 
        let textBox = TextBox()
        let model: MyModel = MyModel.Create()
        textBox.DataContext <- model
        Binding.OfExpression <@ textBox.Text <- model.Value |> OneWay @>
        let binding = textBox.GetBindingExpression(TextBox.TextProperty).ParentBinding
        Assert.Equal(BindingMode.OneWay, binding.Mode)

    [<Fact>]
    let updateSource() = 
        let textBox = TextBox()
        let model: MyModel = MyModel.Create()
        textBox.DataContext <- model
        Binding.OfExpression <@ textBox.Text <- model.Value |> UpdateSource UpdateSourceTrigger.Explicit @>
        let binding = textBox.GetBindingExpression(TextBox.TextProperty).ParentBinding
        Assert.Equal(UpdateSourceTrigger.Explicit, binding.UpdateSourceTrigger)

    [<Fact>]
    let updateSourceOnChange() = 
        let textBox = TextBox()
        let model: MyModel = MyModel.Create()
        textBox.DataContext <- model
        Binding.OfExpression <@ textBox.Text <- model.Value |> UpdateSourceOnChange @>
        let binding = textBox.GetBindingExpression(TextBox.TextProperty).ParentBinding
        Assert.Equal(UpdateSourceTrigger.PropertyChanged, binding.UpdateSourceTrigger)

    [<Fact>]
    let fallbackValue() = 
        let textBox = TextBox()
        let model: MyModel = MyModel.Create()
        textBox.DataContext <- model
        let fallback = box "foo"
        Binding.OfExpression <@ textBox.Text <- model.Value |> FallbackValue fallback @>
        let binding = textBox.GetBindingExpression(TextBox.TextProperty).ParentBinding
        Assert.Equal(fallback, binding.FallbackValue)

    [<Fact>]
    let targetNullValue() = 
        let textBox = TextBox()
        let model: MyModel = MyModel.Create()
        textBox.DataContext <- model
        let nullValue = box "nothing"
        Binding.OfExpression <@ textBox.Text <- model.Value |> TargetNullValue nullValue @>
        let binding = textBox.GetBindingExpression(TextBox.TextProperty).ParentBinding
        Assert.Equal(nullValue, binding.TargetNullValue)

    // Note: all ..._True and ..._False tests can be replaced with Xunit Theory+true/false parameters
    // when upgraded to Xunit 2.x.
    [<Fact>]
    let validatesOnDataErrors_True() = 
        let textBox = TextBox()
        let model: MyModel = MyModel.Create()
        textBox.DataContext <- model
        Binding.OfExpression <@ textBox.Text <- model.Value |> ValidatesOnDataErrors true @>
        let binding = textBox.GetBindingExpression(TextBox.TextProperty).ParentBinding
        Assert.Equal(true, binding.ValidatesOnDataErrors)

    [<Fact>]
    let validatesOnDataErrors_False() = 
        let textBox = TextBox()
        let model: MyModel = MyModel.Create()
        textBox.DataContext <- model
        Binding.OfExpression <@ textBox.Text <- model.Value |> ValidatesOnDataErrors false @>
        let binding = textBox.GetBindingExpression(TextBox.TextProperty).ParentBinding
        Assert.Equal(false, binding.ValidatesOnDataErrors)

    [<Fact>]
    let validatesOnExceptions_True() = 
        let textBox = TextBox()
        let model: MyModel = MyModel.Create()
        textBox.DataContext <- model
        Binding.OfExpression <@ textBox.Text <- model.Value |> ValidatesOnExceptions true @>
        let binding = textBox.GetBindingExpression(TextBox.TextProperty).ParentBinding
        Assert.Equal(true, binding.ValidatesOnExceptions)

    [<Fact>]
    let validatesOnExceptions_False() = 
        let textBox = TextBox()
        let model: MyModel = MyModel.Create()
        textBox.DataContext <- model
        Binding.OfExpression <@ textBox.Text <- model.Value |> ValidatesOnExceptions false @>
        let binding = textBox.GetBindingExpression(TextBox.TextProperty).ParentBinding
        Assert.Equal(false, binding.ValidatesOnExceptions)

    [<Fact>]
    let multipleOptions() = 
        let textBox = TextBox()
        let model: MyModel = MyModel.Create()
        textBox.DataContext <- model
        Binding.OfExpression 
            <@ 
                textBox.Text <- model.Value 
                                |> Mode BindingMode.OneTime
                                |> UpdateSourceOnChange
                                |> ValidatesOnExceptions false 
            @>
        let binding = textBox.GetBindingExpression(TextBox.TextProperty).ParentBinding
        Assert.Equal(BindingMode.OneTime, binding.Mode)
        Assert.Equal(UpdateSourceTrigger.PropertyChanged, binding.UpdateSourceTrigger)
        Assert.Equal(false, binding.ValidatesOnExceptions)
