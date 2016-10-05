module FSharp.Desktop.UI.ModelTests

open System.ComponentModel
open Xunit

[<AbstractClass>]
type MyModel() =
    inherit Model()

    abstract Value: int with get, set
    abstract FirstName: string with get, set
    abstract LastName: string with get, set

    [<DerivedFrom([| "FirstName"; "LastName" |])>]
    member x.FullName () =
        sprintf "%s %s" x.FirstName x.LastName
        
    [<DerivedFrom([| "FullName" |])>]
    member x.FullNameWithAge () =
        sprintf "Name: %s Age: %d" (x.FullName()) 42

let getModel () =
    let model: MyModel = MyModel.Create()
    let inpc: INotifyPropertyChanged = upcast model
    model.Value <- -1
    model.FirstName <- "Freddie"
    model.LastName <- "Mercury"
    (model, inpc)

[<Fact>]
let basicPropertyChanged() = 
    let (model, inpc) = getModel()
    let whichPropertyChanged = ref ""
    use __ = inpc.PropertyChanged.Subscribe(fun args -> whichPropertyChanged := args.PropertyName)
    model.Value <- 42
    Assert.Equal<string>("Value", !whichPropertyChanged)
    Assert.Equal(42, model.Value)

[<Fact>]
let noNotificationOnSameValue() = 
    let (model, inpc) = getModel()
    let called = ref false
    use __ = inpc.PropertyChanged.Subscribe(fun args -> called := true)
    model.Value <- model.Value
    Assert.False(!called)

[<Fact>]
let oneParentPropertyChanged() =
    let (model, inpc) = getModel()
    let whichPropertiesChanged = new System.Collections.Generic.List<string>()
    use __ = inpc.PropertyChanged.Subscribe(fun args -> whichPropertiesChanged.Add(args.PropertyName))
    model.FirstName <- "Daniela"
    Assert.Contains("FullName", whichPropertiesChanged)
    Assert.Contains("FirstName", whichPropertiesChanged)

[<Fact>]
let noParentsChange() =
    let (model, inpc) = getModel()
    let whichPropertiesChanged = new System.Collections.Generic.List<string>()
    use __ = inpc.PropertyChanged.Subscribe(fun args -> whichPropertiesChanged.Add(args.PropertyName))
    Assert.Empty(whichPropertiesChanged)

[<Fact>]
let transitivePropertiesPropagate() =
    let (model, inpc) = getModel()
    let whichPropertiesChanged = new System.Collections.Generic.List<string>()
    use __ = inpc.PropertyChanged.Subscribe(fun args -> whichPropertiesChanged.Add(args.PropertyName))
    model.FirstName <- "Daniela"
    Assert.Contains("FirstName", whichPropertiesChanged)
    Assert.Contains("FullName", whichPropertiesChanged)
    Assert.Contains("FullNameWithAge", whichPropertiesChanged)
    Assert.Equal(3, whichPropertiesChanged.Count);
