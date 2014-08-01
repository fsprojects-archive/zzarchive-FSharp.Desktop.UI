(*** hide ***)

#r "PresentationCore"
#r "PresentationFramework" 
#r "System.Xaml"
#r "WindowsBase"

open System
open System.Windows
open System.Windows.Controls
open System.Windows.Data
open System.Windows.Input

#r "../../bin/FSharp.Desktop.UI.dll"

open FSharp.Desktop.UI

(**

Basics
===================

[Numeric Up/Down control](https://github.com/fsprojects/FSharp.Desktop.UI/tree/master/samples/NumericUpDown) is simplified version of IntegerUpDown control from [Extended WPF Toolkit](https://wpftoolkit.codeplex.com/wikipage?title=IntegerUpDown&referringTitle=NumericUpDown). 
It provides a TextBox with button spinners that allow incrementing and decrementing int values by using the spinner buttons, 
keyboard up/down arrows, or mouse wheel.

To keep it simple we will build application rather than reusable control.

<img src="img/NumericUpDown.png"/>

Let's go step-by-step through development process.

Model
-------------------------------------
Our model has single property `Value` bounded to input text box.
The library provides quick canonical way to define custom models. 
Make sure to inherit model type from FSharp.Desktop.UI.Model and declare as abstract read/write properties you want to data-bind. 
Once that done INotifyPropertyChanged and INotifyDataErrorInfo will be auto-wired.

*)

[<AbstractClass>]
type NumericUpDownModel() = 
    inherit Model()

    abstract Value: int with get, set

(**
There are [alternative methods](ref_model.html) to define custom models.

Events + View
-------------------------------------

Because view is essentially single event stream the best way to define variety of event is to use discriminated unions. It seems obvious that we need two events: Up and Down. Low level events like `MouseMove` or `KeyUp` are mapped into high-level ones. 
Although it is not too hard to provide implementation of `IView<'Event, 'Model>` interface, the library provides base helper classes like View and XamlView.

Traditional way to design actual WPF window is to use XAML designer. 
For simplicity in this particular application we will build it in a code. 
It also proves the point that WPF details abstracted so well that the library is agnostic to actual way WPF primitives assembled.   

In addition to event sourcing View also responsible for setting up proper data bindings. 
The library introduce [type-safe data-binding](ref_databinding.html). 
The idea is map F# assignment statement quotation to data binding setup.  
Other examples will expand on topic on type safe data binding.
*)

type NumericUpDownEvents = Up | Down

type NumericUpDownView() as this = 
    inherit View<NumericUpDownEvents, NumericUpDownModel, Window>()
    
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
        Binding.FromExpression 
            <@
                input.Text <- coerce model.Value 
                //'coerce' means "use WPF default conversions"
            @>

(**

Controller
-------------------------------------
For this introductory example think of controller as callback that takes event and model. 
It processes event and apply changes to the model. 
Model state changes propagate back to view via data binding.

*)

let eventHandler event (model: NumericUpDownModel) =
    match event with
    | Up -> model.Value <- model.Value + 1
    | Down -> model.Value <- model.Value - 1

let controller = Controller.Create eventHandler

(**
Controllers in real-world application are more complex. 
But `Controller.Create` method exists in the library and can be used as a shortcut to build simple controllers. 

Pattern matching in event inside controller callback is very interesting because it represents compiler checked event handlers map. 
To prove the point comment out one of the case, for example Down. 
You will immediately see a warning from compiler "Incomplete pattern matches on this expression. 
For example, the value 'Down' may indicate a case not covered by the pattern(s)."

Application
-------------------------------------
Application boot-strap code is trivial. Worth noting that model has be created via `Create` factory method.

*)

[<STAThread>]
do
    let model = NumericUpDownModel.Create()
    let view = NumericUpDownView()
    let mvc = Mvc(model, view, controller)
    use eventLoop = mvc.Start()
    Application().Run( window = view.Root) |> ignore

(**

Where Are We?
-------------------------------------

It is quite remarkable what we were able to archive with such small amount of code. 
Try to implement exactly same functionality using classic MVVM or one of the mvvm-style frameworks. 
Compare and make you own judgment. Next example is [Calculator application](tutorial_calculator.html).

*)