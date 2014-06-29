namespace FSharp.Desktop.UI

open System
open System.ComponentModel
open System.Runtime.ExceptionServices
open System.Threading
open System.Reactive
open System.Reactive.Concurrency

type IPartialView<'Events, 'Model> = 

    abstract Events : IObservable<'Events> with get
    abstract SetBindings : 'Model -> unit

type IView<'Events, 'Model> =
    inherit IPartialView<'Events, 'Model>

    abstract ShowDialog : unit -> bool
    abstract Show : unit -> Async<bool>

type EventHandler<'Model> = 
    | Sync of ('Model -> unit)
    | Async of ('Model -> Async<unit>)

type IController<'Events, 'Model> =

    abstract InitModel : 'Model -> unit
    abstract Dispatcher : ('Events -> EventHandler<'Model>)

type Mvc<'Events, 'Model when 'Model :> INotifyPropertyChanged>(model : 'Model, view : IView<'Events, 'Model>, controller : IController<'Events, 'Model>, ?onError: 'Events -> exn -> unit) =

    let onError = defaultArg onError (fun _ exn -> ExceptionDispatchInfo.Capture(exn).Throw())
    
    member this.Start() =
        controller.InitModel model
        view.SetBindings model

        let observer = Observer.Create(fun event -> 
            match controller.Dispatcher event with
            | Sync eventHandler ->
                try eventHandler model 
                with exn -> onError event exn
            | Async eventHandler -> 
                Async.StartWithContinuations(
                    computation = eventHandler model, 
                    continuation = ignore, 
                    exceptionContinuation = onError event,
                    cancellationContinuation = ignore))        
#if DEBUG
        let observer = observer.Checked()
#endif
        let observer = Observer.Synchronize(observer, preventReentrancy = true)
        assert(SynchronizationContext.Current <> null)
        let observer = Observer.NotifyOn(observer, SynchronizationContextScheduler(SynchronizationContext.Current, alwaysPost = false))
        view.Events.Subscribe observer

    member this.StartDialog() =
        use subscription = this.Start()
        view.ShowDialog()

    member this.StartWindow() =
        async {
            use subscription = this.Start()
            return! view.Show()
        }

    member this.Compose(childController : IController<'EX, 'MX>, childView : IPartialView<'EX, 'MX>, childModelSelector : _ -> 'MX) = 
        let compositeView = {
                new IView<_, _> with
                    member __.Events = Observable.unify view.Events childView.Events
                    member __.SetBindings model =
                        view.SetBindings model  
                        model |> childModelSelector |> childView.SetBindings
                    member __.Show() = view.Show()
                    member __.ShowDialog() = view.ShowDialog()
        }

        let compositeController = { 
            new IController<_, _> with
                member __.InitModel model = 
                    controller.InitModel model
                    model |> childModelSelector |> childController.InitModel
                member __.Dispatcher = function 
                    | Choice1Of2 e -> controller.Dispatcher e
                    | Choice2Of2 e -> 
                        match childController.Dispatcher e with
                        | Sync handler -> Sync(childModelSelector >> handler)  
                        | Async handler -> Async(childModelSelector >> handler) 
        }

        Mvc(model, compositeView, compositeController)

    static member (<+>) (mvc : Mvc<_, _>,  (childController, childView, childModelSelector)) = 
        mvc.Compose(childController, childView, childModelSelector)

    member this.Compose(childController : IController<_, _>, events : IObservable<_>) = 
        let childView = {
            new IPartialView<_, _> with
                member __.Events = events
                member __.SetBindings _ = () 
        }
        this.Compose(childController, childView, id)

        

    