
open System
open System.Windows
open System.Windows.Controls
open System.Windows.Data

open FSharp.Desktop.UI

[<AbstractClass>]
type CalculatorModel() = 
    inherit Model()

    abstract X : int with get, set
    abstract Y : int with get, set
    abstract Result : int with get, set

type CalculatorEvents = 
    | Add
    | Subtract
    | Multiply
    | Divide
    | Clear

type CalculatorView() as this =
    inherit XamlView<CalculatorEvents, CalculatorModel>(resourceLocator = Uri("/Window.xaml", UriKind.Relative))

    let add : Button = this ? Add |> Option.get
    let subtract : Button = this ? Subtract |> Option.get
    let multiply : Button = this ? Multiply |> Option.get
    let divide : Button = this ? Divide |> Option.get
    let clear : Button = this ? Clear |> Option.get
    let x : TextBox = this ? X |> Option.get
    let y : TextBox = this ? Y |> Option.get
    let result : TextBlock = this ? Result |> Option.get

    override this.EventStreams = 
        [
            add.Click |> Observable.mapTo Add
            subtract.Click |> Observable.mapTo Subtract
            multiply.Click |> Observable.mapTo Multiply
            divide.Click |> Observable.mapTo Divide
            clear.Click |> Observable.mapTo Clear
        ]

    override this.SetBindings model = 
        Binding.FromExpression 
            <@ 
                x.Text <- coerce model.X
                y.Text <- coerce model.Y 
                result.Text <- coerce model.Result 
            @>

type CalculatorController() = 

    interface IController<CalculatorEvents, CalculatorModel> with

        member this.InitModel _ = () 

        member this.Dispatcher = function
            | Add -> Sync this.Add
            | Subtract -> Sync this.Subtract
            | Multiply -> Sync this.Multiply
            | Divide -> Sync this.Divide
            | Clear -> Sync this.Clear

    member this.Add(model: CalculatorModel) = model.Result <- model.X + model.Y

    member this.Subtract(model: CalculatorModel) = model.Result <- model.X - model.Y

    member this.Multiply(model: CalculatorModel) = model.Result <- model.X * model.Y

    member this.Divide(model: CalculatorModel) = model.Result <- model.X / model.Y

    member this.Clear(model: CalculatorModel) = 
        model.X <- 0
        model.Y <- 0
        model.Result <- 0

[<EntryPoint; STAThread>]
let main _ = 
    let model, view, controller = CalculatorModel.Create(), CalculatorView(), CalculatorController()
    let mvc = Mvc(model, view, controller)
    Application().Run(mvc, view.Control)

//Other controller implementation options 
//let controllerAsFunction = Controller.Create(fun event (model: CalculatorModel) ->
//    match event with
//    | Add -> model.Result <- model.X + model.Y
//    | Subtract -> model.Result <- model.X - model.Y
//    | Multiply -> model.Result <- model.X * model.Y
//    | Divide -> model.Result <- model.X / model.Y
//    | Clear -> 
//        model.X <- 0
//        model.Y <- 0
//        model.Result <- 0
//)
//
//type CalculatorController2() =
//    inherit Controller<CalculatorEvents, CalculatorModel>()  
//
//    override this.InitModel _ = () 
//
//    override this.Dispatcher = Sync << function
//        | Add -> this.Add
//        | Subtract -> this.Subtract
//        | Multiply -> this.Multiply
//        | Divide -> this.Divide
//        | Clear -> this.Clear
//
//    member this.Add model = model.Result <- model.X + model.Y
//    member this.Subtract model = model.Result <- model.X - model.Y
//    member this.Multiply model = model.Result <- model.X * model.Y
//    member this.Divide model = model.Result <- model.X / model.Y
//    member this.Clear model = 
//        model.X <- 0
//        model.Y <- 0
//        model.Result <- 0
