namespace FSharp.Desktop.UI

[<AbstractClass>]
type Controller<'Event, 'Model>() =

    interface IController<'Event, 'Model> with
        member this.InitModel model = this.InitModel model
        member this.Dispatcher = this.Dispatcher

    abstract InitModel : 'Model -> unit
    abstract Dispatcher : ('Event -> EventHandler<'Model>)

    static member Create callback = {
        new IController<'Event, 'Model> with
            member this.InitModel _ = ()
            member this.Dispatcher = callback
    } 

    static member Create callback = {
        new IController<'Event, 'Model> with
            member this.InitModel _ = ()
            member this.Dispatcher = fun event -> Sync(callback event)
    } 
