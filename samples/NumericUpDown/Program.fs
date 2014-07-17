open System
open System.Windows
open System.Windows.Controls
open System.Windows.Data

open FSharp.Desktop.UI

type NumericUpDownEvents = Up | Down

[<AbstractClass>]
type NumericUpDownEventsModel() = 
    inherit Model()

    abstract Value: float with get, set

type NumericUpDownEventsView() as this = 
    inherit View<NumericUpDownEvents, NumericUpDownEventsModel, Window>(Window(Width = 250., Height = 80., WindowStartupLocation = WindowStartupLocation.CenterScreen, Title = "Up/Down"))

    let upButton = Button(Content = "^", Width = 50.)
    let downButton = Button(Content = "v", Width = 50.)
    let input = TextBox(TextAlignment = TextAlignment.Center, FontSize = 20., VerticalContentAlignment = VerticalAlignment.Center, Text = "0")
    let mainPanel = Grid()

    do
        [ RowDefinition(); RowDefinition() ] |> List.iter mainPanel.RowDefinitions.Add
        [ ColumnDefinition(); ColumnDefinition(Width = GridLength.Auto) ] |> List.iter mainPanel.ColumnDefinitions.Add

        upButton.SetValue(Grid.ColumnProperty, 1)
        downButton.SetValue(Grid.ColumnProperty, 1)
        downButton.SetValue(Grid.RowProperty, 1)

        input.SetValue(Grid.RowSpanProperty, 2)

        mainPanel.Children.Add upButton |> ignore
        mainPanel.Children.Add downButton |> ignore
        mainPanel.Children.Add input |> ignore

        this.Control.Content <- mainPanel

    override this.EventStreams = [
        upButton.Click |> Observable.map (fun _ -> Up)
        downButton.Click |> Observable.map (fun _ -> Down)
    ]

    override this.SetBindings model =   
        Binding.FromExpression 
            <@
                input.Text <- coerce model.Value
            @>


[<EntryPoint; STAThread>]
let main _ = 

    let model = NumericUpDownEventsModel.Create()
    let view = NumericUpDownEventsView()
    let contoroller = Controller.Create(fun event (model: NumericUpDownEventsModel) ->
        match event with
        | Up -> model.Value <- model.Value + 1.0
        | Down -> model.Value <- model.Value - 1.0
    )
    let mvc = Mvc(model, view, contoroller)
    use eventLoop = mvc.Start()

    Application().Run view.Control
