module FSharp.Desktop.UI.ViewTests

open System
open System.ComponentModel
open Xunit
open System.Windows.Controls
open System.Reactive.Linq

[<Fact>]
let emptyEventStreams() = 
    let silentView: IPartialView<_, _> = upcast {
        new PartialView<unit, unit, Control>(Control()) with
            member __.EventStreams = []
            member __.SetBindings _ = raise <| NotImplementedException()
    }

    Assert.Empty(silentView.Events.ToEnumerable())

