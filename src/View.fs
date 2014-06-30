namespace FSharp.Desktop.UI

open System
open System.Windows
open System.Windows.Controls

[<AbstractClass>]
type PartialView<'Events, 'Model, 'Control when 'Control :> FrameworkElement>(control : 'Control) =

    member this.Control = control
    
    interface IPartialView<'Events, 'Model> with
        member this.Events = 
            this.EventStreams |> List.reduce Observable.merge 
        member this.SetBindings model = 
            this.SetBindings model
            control.DataContext <- model

    abstract EventStreams : IObservable<'Events> list
    abstract SetBindings : 'Model -> unit

[<AbstractClass>]
type View<'Events, 'Model, 'Window when 'Window :> Window and 'Window : (new : unit -> 'Window)>(?window) = 
    inherit PartialView<'Events, 'Model, 'Window>(control = defaultArg window (new 'Window()))

    let mutable isOK = false

    interface IView<'Events, 'Model> with
        member this.ShowDialog() = 
            this.Control.ShowDialog() |> ignore
            isOK
        member this.Show() = 
            this.Control.Show()
            this.Control.Closed |> Event.map (fun _ -> isOK) |> Async.AwaitEvent 

    member this.Close isOK' = 
        isOK <- isOK'
        this.Control.Close()

    member this.OK() = this.Close true
    member this.Cancel() = this.Close false

    member this.CancelButton with set(value : Button) = value.Click.Add(ignore >> this.Cancel)
    member this.DefaultOKButton 
        with set(value : Button) = 
            value.IsDefault <- true
            value.Click.Add(ignore >> this.OK)
    
[<AbstractClass>]
type XamlView<'Events, 'Model>(resourceLocator) = 
    inherit View<'Events, 'Model, Window>(resourceLocator |> Application.LoadComponent |> unbox)

    static member (?) (view : PartialView<'Events, 'Model, 'Control>, name) = 
        match view.Control.FindName name with
        | null -> None 
        | control -> control |> unbox |> Some
