﻿namespace FSharp.Desktop.UI

open System
open System.Windows
open System.Windows.Controls

[<AbstractClass>]
type PartialView<'Event, 'Model, 'Control when 'Control :> FrameworkElement>(control : 'Control) =

    member this.Control = control
    
    interface IPartialView<'Event, 'Model> with
        member this.Events = 
            this.EventStreams |> List.reduce Observable.merge 
        member this.SetBindings model =
            control.DataContext <- model
            this.SetBindings model

    abstract EventStreams : IObservable<'Event> list
    abstract SetBindings : 'Model -> unit

[<AbstractClass>]
type View<'Event, 'Model, 'Window when 'Window :> Window and 'Window : (new : unit -> 'Window)>(?window) = 
    inherit PartialView<'Event, 'Model, 'Window>(control = defaultArg window (new 'Window()))

    let mutable isOK = false

    interface IView<'Event, 'Model> with
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
type XamlView<'Event, 'Model>(resourceLocator) = 
    inherit View<'Event, 'Model, Window>(resourceLocator |> Application.LoadComponent |> unbox)

    static member (?) (view : PartialView<'Event, 'Model, 'Control>, name) = 
        match view.Control.FindName name with
        | null -> invalidArg "Name" ("Cannot find child control or resource named: " + name)
        | control -> control |> unbox
