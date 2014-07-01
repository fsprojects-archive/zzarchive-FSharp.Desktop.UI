
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

    [<DerivedProperty>]
    member this.NonZeroArgs = this.X <> 0 && this.Y <> 0

type CalculatorEvents = 
    | Add
    | Subtract
    | Multiply
    | Divide
    | Clear
    | ArgChanging of text: string * cancel: (unit -> unit)

type CalculatorView() as this =
    inherit XamlView<CalculatorEvents, CalculatorModel>(resourceLocator = Uri("/Window.xaml", UriKind.Relative))

    let add : Button = this ? Add
    let subtract : Button = this ? Subtract 
    let multiply : Button = this ? Multiply
    let divide : Button = this ? Divide 
    let clear : Button = this ? Clear
    let x : TextBox = this ? X 
    let y : TextBox = this ? Y
    let result : TextBlock = this ? Result 

    override this.EventStreams = 
        [
            let buttonClicks = 
                [
                    add, Add
                    subtract, Subtract
                    multiply, Multiply
                    divide, Divide
                    clear, Clear
                ] 
                |> List.map (fun (button, event) -> button.Click |> Observable.mapTo event)

            yield! buttonClicks

            yield (x.PreviewTextInput, y.PreviewTextInput) 
                ||> Observable.merge
                |> Observable.map(fun eventArgs -> ArgChanging(eventArgs.Text, fun() -> eventArgs.Handled <- true))
        ]

    override this.SetBindings model = 
        Binding.FromExpression 
            <@ 
                x.Text <- coerce model.X
                result.Text <- coerce model.Result 

                add.IsEnabled <- model.NonZeroArgs
                subtract.IsEnabled <- model.NonZeroArgs
                divide.IsEnabled <- model.Y <> 0
            @>

        Binding.FromExpression(
            <@ 
                y.Text <- coerce model.Y 
            @>, 
            updateSourceTrigger = UpdateSourceTrigger.PropertyChanged)

type CalculatorController() = 

    interface IController<CalculatorEvents, CalculatorModel> with

        member this.InitModel _ = () 

        member this.Dispatcher = function
            | Add -> Sync this.Add
            | Subtract -> Sync this.Subtract
            | Multiply -> Sync this.Multiply
            | Divide -> Sync this.Divide
            | Clear -> Sync this.Clear
            | ArgChanging(text, cancel) -> Sync(this.DiscardInvalidInput text cancel)

    member this.Add(model: CalculatorModel) = 
        if model.Y < 0 
        then model |> Validation.setError <@ fun  m -> m.Y @> "Must be positive number."
        else model.Result <- model.X + model.Y

    member this.Subtract(model: CalculatorModel) = 
        if model.Y < 0 
        then model |> Validation.setError <@ fun  m -> m.Y @> "Must be positive number."
        else model.Result <- model.X - model.Y

    member this.Multiply(model: CalculatorModel) = 
        model.Result <- model.X * model.Y

    member this.Divide(model: CalculatorModel) = 
        model.Result <- model.X / model.Y

    member this.Clear(model: CalculatorModel) = 
        model.X <- 0
        model.Y <- 0
        model.Result <- 0

    member this.DiscardInvalidInput newValue cancel (model: CalculatorModel) = 
        match Int32.TryParse newValue with 
        | false, _  ->  cancel()
        | _ -> ()
        
        

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
