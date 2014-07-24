open System
open System.Windows
open System.Windows.Controls
open System.Windows.Data
open System.Windows.Input

open FSharp.Desktop.UI

type NumericUpDownEvents = Up | Down

[<AbstractClass>]
type NumericUpDownEventsModel() = 
    inherit Model()

    abstract Value: int with get, set

type NumericUpDownEventsView() as this = 
    inherit View<NumericUpDownEvents, NumericUpDownEventsModel, Window>(
        Window( Width = 250., Height = 80., WindowStartupLocation = WindowStartupLocation.CenterScreen, Title = "Up/Down"))

    let mainPanel = 
        let grid = Grid()
        [ RowDefinition(); RowDefinition() ] |> List.iter grid.RowDefinitions.Add
        [ ColumnDefinition(); ColumnDefinition(Width = GridLength.Auto) ] |> List.iter grid.ColumnDefinitions.Add
        grid

    //controls
    let upButton = Button(Content = "^", Width = 50.)
    let downButton = Button(Content = "v", Width = 50.)
    let input = TextBox(TextAlignment = TextAlignment.Center, FontSize = 20., VerticalContentAlignment = VerticalAlignment.Center)
    
    do  //layout
        upButton.SetValue(Grid.ColumnProperty, 1)
        downButton.SetValue(Grid.ColumnProperty, 1)
        downButton.SetValue(Grid.RowProperty, 1)

        input.SetValue(Grid.RowSpanProperty, 2)

        mainPanel.Children.Add upButton |> ignore
        mainPanel.Children.Add downButton |> ignore
        mainPanel.Children.Add input |> ignore

        this.Root.Content <- mainPanel

    override this.EventStreams = [
        upButton.Click |> Observable.map (fun _ -> Up)
        downButton.Click |> Observable.map (fun _ -> Down)

        input.KeyUp |> Observable.choose (fun args -> if args.Key = Key.Up then Some Up else None)
        input.KeyUp |> Observable.choose (fun args -> if args.Key = Key.Down then Some Down else None)

        input.MouseWheel |> Observable.map (fun args -> if args.Delta > 0 then Up else Down)
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
    let contoroller = Controller.Create( fun event (model: NumericUpDownEventsModel) ->
        match event with
        | Up -> model.Value <- model.Value + 1
        | Down -> model.Value <- model.Value - 1
    )
    let mvc = Mvc(model, view, contoroller)
    use eventLoop = mvc.Start()

    Application().Run( window = view.Root)
