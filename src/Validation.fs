namespace FSharp.Desktop.UI

open System.ComponentModel

[<RequireQualifiedAccess>]
module Validation = 

    open System
    open Microsoft.FSharp.Quotations
    open Microsoft.FSharp.Quotations.Patterns

    let inline setErrors( PropertySelector property : Expr<('Model -> _)>) messages (model : #INotifyDataErrorInfo)= 
        (^Model : (member SetErrors : string * string[] -> unit) (model, property.Name, messages))

    let inline setError property message = setErrors property [| message |]

    let inline setErrorf model property = 
        Printf.ksprintf (fun message -> setError property message model) 

    let inline clearErrors property = setErrors property Array.empty 

    let inline invalidIf (PropertySelector property as expr : Expr<_ -> 'a>) predicate message model = 
        if model |> property.GetValue |> unbox<'a> |> predicate then setError expr message model

    let inline assertThat expr predicate = invalidIf expr (not << predicate)

    let inline objectRequired expr = invalidIf expr ((=) null) 

    let inline textRequired expr = invalidIf expr String.IsNullOrWhiteSpace 

    let inline valueRequired expr = assertThat expr (fun(x : Nullable<_>) -> x.HasValue) 


