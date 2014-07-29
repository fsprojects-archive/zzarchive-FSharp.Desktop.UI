namespace FSharp.Desktop.UI

open System
open System.Windows
open System.Windows.Controls
open System.Reactive.Linq

[<AbstractClass>]
type PartialView<'Event, 'Model, 'Element when 'Element :> FrameworkElement>(root : 'Element) =

    member this.Root = root
    
    interface IPartialView<'Event, 'Model> with
        member this.Events = 
            this.EventStreams.Merge()
        member this.SetBindings model = 
            this.SetBindings model
            root.DataContext <- model

    abstract EventStreams : IObservable<'Event> list
    abstract SetBindings : 'Model -> unit

[<AbstractClass>]
type View<'Event, 'Model, 'Window when 'Window :> Window and 'Window : (new : unit -> 'Window)>(?window) = 
    inherit PartialView<'Event, 'Model, 'Window>(root = defaultArg window (new 'Window()))

    interface IView<'Event, 'Model> with
        member this.ShowDialog() = 
            let result = this.Root.ShowDialog()
            if result.HasValue then result.Value else false
        member this.Show() = 
            this.Root.Show()
            this.Root.Closed |> Event.map ignore |> Async.AwaitEvent 

[<AbstractClass>]
type XamlView<'Event, 'Model>(resourceLocator) = 
    inherit View<'Event, 'Model, Window>(resourceLocator |> Application.LoadComponent |> unbox)

    static member (?) (view : PartialView<'Event, 'Model, 'Control>, name) = 
        match view.Root.FindName name with
        | null -> invalidArg "Name" ("Cannot find child control or resource named: " + name)
        | control -> control |> unbox
