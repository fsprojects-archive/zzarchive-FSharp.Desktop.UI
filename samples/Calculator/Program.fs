
open System
open System.Windows
open System.Windows.Controls
open System.Windows.Data
open System.Threading
open System.Configuration

open FSharp.Desktop.UI

type WolframalphaResponse = 
    | Success of string
    | Failure of string
    | Empty
    | Running
    | Cancelled

[<AbstractClass>]
type CalculatorModel() = 
    inherit Model()
    
    abstract X : float with get, set
    abstract Y : float with get, set
    abstract Result : float with get, set

    [<DerivedProperty>]
    member this.OneNonEmptyArg = this.X <> 0.0 || this.Y <> 0. || this.Result <> 0.0
    
    abstract WolframalphaQuery : string with get, set
    abstract WolframalphaAppId : string with get, set
    abstract WolframalphaResponse : WolframalphaResponse with get, set

type CalculatorEvents = 
    | Add
    | Subtract
    | Multiply
    | Divide
    | Clear
    | ArgChanging of text: string * cancel: (unit -> unit)
    | AskWolframalpha
    | CancelAskWolframalphaRequest 

[<AutoOpenAttribute>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module CalculatorModel =

    let wolframalphaResponseConverter = function
        | Success result -> result
        | Failure why -> why
        | Empty -> ""
        | Running -> "running..."
        | Cancelled -> "cancelled."

    type CalculatorModel with
        [<DerivedProperty>]
        member this.AskWolframalphaEnabled = 
            string this.WolframalphaQuery <> "" && this.WolframalphaResponse <> Running

type CalculatorView() as this =
    inherit XamlView<CalculatorEvents, CalculatorModel>(resourceLocator = Uri("/Window.xaml", UriKind.Relative))

    static let passwordProperty = 
        let metadata = 
            FrameworkPropertyMetadata(propertyChangedCallback = fun sender args -> 
                let x: PasswordBox = unbox sender
                x.Password <- unbox args.NewValue
            )
        DependencyProperty.Register("PasswordProperty", typeof<string>, typeof<PasswordBox>, metadata);

    let add: Button = this ? Add
    let subtract: Button = this ? Subtract 
    let multiply: Button = this ? Multiply
    let divide: Button = this ? Divide 
    let clear: Button = this ? Clear
    let x: TextBox = this ? X 
    let y: TextBox = this ? Y
    let result: TextBlock = this ? Result 

    let wolframalphaQuery: TextBox = this ? WolframalphaQuery
    let wolframalphaAppId: PasswordBox = this ? AppId
    let askWolframalpha: Button = this ? AskWolframalpha 
    let cancelWolframalphaRequest: Button = this ? CancelWolframalphaRequest 
    let wolframalphaResponse: TextBox = this ? WolframalphaResponse

    do
        wolframalphaResponse.IsReadOnly <- true
        
        wolframalphaAppId.PasswordChanged.Add <| fun _ -> 
            wolframalphaAppId.SetValue(passwordProperty, wolframalphaAppId.Password)

    override this.EventStreams = 
        [
            let buttonClicks = 
                [
                    add, Add
                    subtract, Subtract
                    multiply, Multiply
                    divide, Divide
                    clear, Clear
                    cancelWolframalphaRequest, CancelAskWolframalphaRequest
                    askWolframalpha, AskWolframalpha
                ] 
                |> List.map (fun (button, event) -> button.Click |> Observable.mapTo event)

            yield! buttonClicks

            yield 
                (x.PreviewTextInput, y.PreviewTextInput)
                ||> Observable.merge 
                |> Observable.map(fun args -> 
                    let tb: TextBox = unbox args.Source
                    let finalText = if tb.CaretIndex = 0 then args.Text + tb.Text else tb.Text + args.Text
                    ArgChanging(finalText, fun() -> args.Handled <- true))
        ]

    override this.SetBindings model = 
        
        wolframalphaAppId.SetBinding(passwordProperty, Binding("WolframalphaAppId", Mode = BindingMode.TwoWay)) |> ignore

        Binding.OfExpression 
            <@ 
                result.Text <- String.Format("Result : {0}", model.Result) 

                divide.IsEnabled <- model.Y <> 0.
                clear.IsEnabled <- model.OneNonEmptyArg

                wolframalphaResponse.Text <- wolframalphaResponseConverter model.WolframalphaResponse
                askWolframalpha.IsEnabled <- model.AskWolframalphaEnabled
                cancelWolframalphaRequest.IsEnabled <- model.WolframalphaResponse = Running
            @>

        Binding.OfExpression(
            <@ 
                x.Text <- coerce model.X
                y.Text <- coerce model.Y 

                wolframalphaQuery.Text <- model.WolframalphaQuery
            @>, 
            updateSourceTrigger = UpdateSourceTrigger.PropertyChanged)

type CalculatorController(?wolframAlphaService: string -> string -> Async<WolframAlpha.Response>) = 

    let wolframAlphaService = defaultArg wolframAlphaService WolframAlpha.instance

    interface IController<CalculatorEvents, CalculatorModel> with

        member this.InitModel model = 
            model.WolframalphaResponse <- Empty
            let appKey = ConfigurationManager.AppSettings.["WolframalphaAppId"]
            if appKey <> null 
            then model.WolframalphaAppId <- appKey

        member this.Dispatcher = function
            | Add -> Sync this.Add
            | Subtract -> Sync this.Subtract
            | Multiply -> Sync this.Multiply
            | Divide -> Sync this.Divide
            | Clear -> Sync this.Clear
            | ArgChanging(text, cancel) -> Sync(this.DiscardInvalidInput text cancel)
            | AskWolframalpha -> Async this.AskWolframalpha 
            | CancelAskWolframalphaRequest -> Sync <| fun _ -> Async.CancelDefaultToken()

    member this.Add(model: CalculatorModel) = 
        if model.Y < 0. 
        then model |> Validation.setError <@ fun m -> m.Y @> "Must be positive number."
        else model.Result <- model.X + model.Y

    member this.Subtract(model: CalculatorModel) = 
        if model.Y < 0. 
        then model |> Validation.setError <@ fun  m -> m.Y @> "Must be positive number."
        else model.Result <- model.X - model.Y

    member this.Multiply(model: CalculatorModel) = 
        model.Result <- model.X * model.Y

    member this.Divide(model: CalculatorModel) = 
        model.Result <- model.X / model.Y

    member this.Clear(model: CalculatorModel) = 
        model.X <- 0.
        model.Y <- 0.
        model.Result <- 0.

    member this.DiscardInvalidInput (newValue: string) cancel (model: CalculatorModel) = 
        let textToCheck = if newValue.EndsWith(".") then newValue + "0" else newValue
        match Double.TryParse textToCheck with 
        | false, _  ->  cancel()
        | _ -> ()
        
    member this.AskWolframalpha (model: CalculatorModel) = 
        let uiCtx = SynchronizationContext.Current
        async {
            
            if String.IsNullOrEmpty model.WolframalphaAppId
            then 
                model |> Validation.setError <@ fun m -> m.WolframalphaAppId @> "AppId is missing"  
            else
                use! cancelHandler = Async.OnCancel(fun() -> 
                    uiCtx.Post((fun _ -> model.WolframalphaResponse <- Cancelled), null)) 

                model.WolframalphaResponse <- Running
                let! response = wolframAlphaService model.WolframalphaQuery model.WolframalphaAppId
                do! Async.SwitchToContext uiCtx
                if response.Error
                then
                    model.WolframalphaResponse <- Failure (response.Error2.Value.Msg)                    
                else
                    match response.Pods |> Array.tryFind (fun x -> x.Id = "Result" || x.Id = "DecimalApproximation") with
                    | Some x -> 
                        model.WolframalphaResponse <- Success x.Subpod.Plaintext.Value
                    | None -> 
                        model.WolframalphaResponse <- Empty
        }  

[<EntryPoint; STAThread>]
let main args =
    let wolframAlphaService = args |> Array.tryFind (fun x -> x = "/airplaneMode") |> Option.map (fun _ -> WolframAlpha.airplaneMode) 
    let model, view, controller = CalculatorModel.Create(), CalculatorView(), CalculatorController(?wolframAlphaService = wolframAlphaService)
    let mvc = Mvc(model, view, controller)
    use eventLoop = mvc.Start()
    let app = Application()
    app.DispatcherUnhandledException.Add <| fun args ->
        if MessageBox.Show(args.Exception.ToString(), "Error! Ignore?", MessageBoxButton.YesNo, MessageBoxImage.Error, MessageBoxResult.Yes) = MessageBoxResult.Yes
        then args.Handled <- true
    app.Run(window = view.Root)
