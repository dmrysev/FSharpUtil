namespace Util.UI.View.Avalonia.CommandPrompt

open Util.UI.Core.CommandPrompt
open Avalonia
open Avalonia.Controls
open Avalonia.Input
open Avalonia.Markup.Xaml
open Avalonia.Interactivity

type MainView () as this = 
    inherit UserControl ()
    do this.InitializeComponent()

    member private this.InitializeComponent() =
        AvaloniaXamlLoader.Load(this)
        this.DataContextChanged.Add (fun _ ->
            let viewModel = this.DataContext :?> MainViewModel
            viewModel.FocusInputEvent.Publish.Add this.FocusCommandPromptInput )

    member this.FocusCommandPromptInput() =
        let commandPromptInput = this.FindControl<TextBox>("CommandPromptInput")
        commandPromptInput.Focus() |> ignore
