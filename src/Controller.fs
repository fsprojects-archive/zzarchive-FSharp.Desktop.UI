namespace FSharp.Desktop.UI

[<AbstractClass>]
type Controller<'Events, 'Model>() =

    interface IController<'Events, 'Model> with
        member this.InitModel model = this.InitModel model
        member this.Dispatcher = this.Dispatcher

    abstract InitModel : 'Model -> unit
    abstract Dispatcher : ('Events -> EventHandler<'Model>)

    static member Create callback = {
        new IController<'Events, 'Model> with
            member this.InitModel _ = ()
            member this.Dispatcher = callback
    } 

    static member Create callback = {
        new IController<'Events, 'Model> with
            member this.InitModel _ = ()
            member this.Dispatcher = fun event -> Sync(callback event)
    } 
