module internal FSharp.Desktop.UI.DerivedProperties

open System
open System.Reflection
open System.Windows 
open System.Windows.Data 
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.Patterns
open Microsoft.FSharp.Quotations.DerivedPatterns
open Microsoft.FSharp.Quotations.ExprShape

type PropertyInfo with

    member internal this.DeclaredOnNullable =  
        this.DeclaringType.IsGenericType && this.DeclaringType.GetGenericTypeDefinition() = typedefof<_ Nullable> 

    member internal this.DeclaredOnOption =  
        this.DeclaringType.IsGenericType && this.DeclaringType.GetGenericTypeDefinition() = typedefof<_ option> 

let internal (|PropertyPathOfDependency|_|) self expr = 
    let rec loop e acc = 
        match e with
        | PropertyGet( Some tail, property, []) -> 
            if property.DeclaredOnNullable || (List.isEmpty acc && property.DeclaredOnOption)
            then loop tail acc 
            else loop tail (property.Name :: acc)
        | Var x when x = self -> acc
        | _ -> []

    match loop expr [] with
    | [] -> None
    | xs -> xs |> String.concat "." |> Some

let rec expand vars expr = 

    let expanded = 
        match expr with
            | ExprShape.ShapeVar v when Map.containsKey v vars -> vars.[v]
            | ExprShape.ShapeVar v -> Expr.Var v
            | Patterns.Call(body, MethodWithReflectedDefinition meth, args) ->
                let this = match body with Some b -> Expr.Application(meth, b) | _ -> meth
                let res = Expr.Applications(this, [ for a in args -> [a]])
                expand vars res
            | ExprShape.ShapeLambda(v, expr) -> 
                Expr.Lambda(v, expand vars expr)
            | ExprShape.ShapeCombination(o, exprs) ->
                ExprShape.RebuildShapeCombination(o, List.map (expand vars) exprs)

    match expanded with
    | Patterns.Application(ExprShape.ShapeLambda(v, body), assign)
    | Patterns.Let(v, assign, body) ->
        expand (Map.add v (expand vars assign) vars) body
    | _ -> expanded

let rec extractDependencies self propertyBody = 
    seq {
        match propertyBody with 
        | PropertyPathOfDependency self path -> yield path
        | ShapeVar _ -> ()
        | ShapeLambda(_, body) -> yield! extractDependencies self body   
        | ShapeCombination(_, exprs) -> for subExpr in exprs do yield! extractDependencies self subExpr
    }

let getPropertyDependencies(model, propertyBody) = 
    propertyBody
        |> expand Map.empty
        |> extractDependencies model
        |> Seq.distinct 
        |> Seq.toList

let getMultiBindingForDerivedProperty(root, model: Var, body: Expr, getter: obj -> obj) = 
    let binding = MultiBinding(ValidatesOnNotifyDataErrors = false)
    let self = Binding(path = root, Mode = BindingMode.OneWay) 
    binding.Bindings.Add self

    for path in getPropertyDependencies(model, body) do
        assert (path <> null)
        binding.Bindings.Add <| Binding(root + "." + path, Mode = BindingMode.OneWay, FallbackValue = null)

    binding.Converter <- {
        new IMultiValueConverter with

            member this.Convert(values, _, _, _) = 
                if Array.exists (fun x -> x = DependencyProperty.UnsetValue) values
                then 
                    DependencyProperty.UnsetValue
                else
                    try getter values.[0] 
                    with _ -> DependencyProperty.UnsetValue

            member this.ConvertBack(_, _, _, _) = undefined
    }

    binding :> BindingBase

