open System
open System.Windows
open System.Windows.Controls
open System.Windows.Data
open System.Windows.Input

open FSharp.Desktop.UI

[<AbstractClass>]
type NumericUpDownModel() = 
    inherit Model()

    abstract Value: int with get, set

type NumericUpDownEvents = Up | Down

type NumericUpDownView() as this = 
    inherit View<NumericUpDownEvents, NumericUpDownModel, Window>( Window())
    
    //Assembling WPF window in code. 
    do 
        this.Root.Width <- 250.
        this.Root.Height <- 80.
        this.Root.WindowStartupLocation <- WindowStartupLocation.CenterScreen
        this.Root.Title <- "Up/Down"

    let mainPanel = 
        let grid = Grid()
        [ RowDefinition(); RowDefinition() ] |> List.iter grid.RowDefinitions.Add
        [ ColumnDefinition(); ColumnDefinition(Width = GridLength.Auto) ] |> List.iter grid.ColumnDefinitions.Add
        grid

    let upButton = Button(Content = "^", Width = 50.)
    let downButton = Button(Content = "v", Width = 50.)
    let input = TextBox(TextAlignment = TextAlignment.Center, FontSize = 20., VerticalContentAlignment = VerticalAlignment.Center)
    
    do  
        upButton.SetValue(Grid.ColumnProperty, 1)
        downButton.SetValue(Grid.ColumnProperty, 1)
        downButton.SetValue(Grid.RowProperty, 1)

        input.SetValue(Grid.RowSpanProperty, 2)

        mainPanel.Children.Add upButton |> ignore
        mainPanel.Children.Add downButton |> ignore
        mainPanel.Children.Add input |> ignore

        this.Root.Content <- mainPanel

    //View implementation 
    override this.EventStreams = [
        upButton.Click |> Observable.map (fun _ -> Up)
        downButton.Click |> Observable.map (fun _ -> Down)

        input.KeyUp |> Observable.choose (fun args -> 
            match args.Key with 
            | Key.Up -> Some Up  
            | Key.Down -> Some Down
            | _ ->  None
        )

        input.MouseWheel |> Observable.map (fun args -> if args.Delta > 0 then Up else Down)
    ]

    override this.SetBindings model =   
        Binding.OfExpression 
            <@
                input.Text <- coerce model.Value
                //'coerce' means "use WPF default conversions"
            @> 

let eventHandler event (model: NumericUpDownModel) =
    match event with
    | Up -> model.Value <- model.Value + 1
    | Down -> model.Value <- model.Value - 1

let controller = Controller.Create eventHandler

[<STAThread>]
do
    let model = NumericUpDownModel.Create()
    let view = NumericUpDownView()
    let mvc = Mvc(model, view, controller)
    use eventLoop = mvc.Start()
    Application().Run( window = view.Root) |> ignore
