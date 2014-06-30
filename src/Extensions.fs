namespace FSharp.Desktop.UI
 
[<AutoOpen>]
module internal Extensions = 

    open System
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
module Observable =
    let mapTo value = Observable.map(fun _ -> value)

