namespace GUI.Views

open Avalonia.Controls
open Avalonia.Interactivity
open Avalonia.Markup.Xaml
open System.Diagnostics

type MainView() as this =
    inherit UserControl()

    do this.InitializeComponent()

    member private this.InitializeComponent() = AvaloniaXamlLoader.Load(this)

    member t.DoRunClicked(_: obj, _: RoutedEventArgs) =
        Debug.WriteLine $"Click {t.celcius.Text}"
