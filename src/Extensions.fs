namespace FSharp.Desktop.UI

open System.Runtime.CompilerServices
[<assembly:InternalsVisibleTo("SqlClient.Tests")>]
do() 

open System

[<AutoOpen>]
module internal Extensions = 

    open Microsoft.FSharp.Quotations
    open Microsoft.FSharp.Quotations.Patterns

    let inline undefined<'T> = raise<'T> <| NotImplementedException()

    let inline (|PropertySelector|) (expr : Expr<('T -> 'a)>) = 
        match expr with 
        | Lambda(arg, PropertyGet( Some (Var selectOn), property, [])) -> 
            assert(arg.Name = selectOn.Name)
            property
        | _ -> 
            invalidArg "Expecting property getter expression" (string expr)

[<RequireQualifiedAccess>]
module internal Observer =

    open System.Reactive
    open System.Windows.Threading

    let notifyOnDispatcher(observer : IObserver<_>) = 
        let dispatcher = Dispatcher.CurrentDispatcher 
        let invokeOnDispatcher f = if dispatcher.CheckAccess() then f() else dispatcher.InvokeAsync f |> ignore 
        { 
            new IObserver<_> with 
                member __.OnNext value = invokeOnDispatcher(fun() -> observer.OnNext value)
                member __.OnError error = invokeOnDispatcher(fun() -> observer.OnError error)
                member __.OnCompleted() = invokeOnDispatcher observer.OnCompleted
        }    

    let preventReentrancy observer = Observer.Synchronize(observer, preventReentrancy = true)

[<RequireQualifiedAccess>]
module Observable =
    let mapTo value = Observable.map(fun _ -> value)


