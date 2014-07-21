module FSharp.Desktop.UI.ModelTests

open System.ComponentModel
open Xunit

[<AbstractClass>]
type MyModel() =
    inherit Model()

    abstract Value: int with get, set

let model: MyModel = MyModel.Create()
let inpc: INotifyPropertyChanged = upcast model
do 
    model.Value <- -1

[<Fact>]
let basicPropertyChanged() = 
    let whichPropertyChanged = ref ""
    use __ = inpc.PropertyChanged.Subscribe(fun args -> whichPropertyChanged := args.PropertyName)
    model.Value <- 42
    Assert.Equal<string>("Value", !whichPropertyChanged)
    Assert.Equal(42, model.Value)

[<Fact>]
let noNotificationOnSameValue() = 
    let called = ref false
    use __ = inpc.PropertyChanged.Subscribe(fun args -> called := true)
    model.Value <- model.Value
    Assert.False(!called)

