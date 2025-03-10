﻿namespace Vogen.Client.Views

open Doaz.Reactive
open Doaz.Reactive.Controls
open Doaz.Reactive.Math
open FSharp.Linq
open Microsoft.Win32
open System
open System.Collections.Generic
open System.Collections.Immutable
open System.IO
open System.Reflection
open System.Windows
open System.Windows.Controls
open System.Windows.Controls.Primitives
open System.Windows.Input
open System.Windows.Threading
open Vogen.Client.Controls
open Vogen.Client.Model
open Vogen.Client.ViewModel

#nowarn "40"


type MainWindowBase() =
    inherit Window()

    static member AppName = "未来虚拟唱鸽人训练营"

    static member WindowTitleConverter = ValueConverter.CreateMulti(fun vs ->
        match vs with
        | [| fileName; isSaved |] ->
            let fileName = fileName.ToString()
            let isSaved = Convert.ToBoolean isSaved
            let notSavedStar = if isSaved then "" else "*"
            let version = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion
            $"{notSavedStar}{fileName} - {MainWindowBase.AppName} v{version}"
        | _ ->
            raise(ArgumentException()))

    member x.ProgramModel = x.DataContext :?> ProgramModel

    abstract TempoPopup : TextBoxPopupBase
    default x.TempoPopup = Unchecked.defaultof<_>
    abstract TimeSigPopup : TextBoxPopupBase
    default x.TimeSigPopup = Unchecked.defaultof<_>

    override x.OnClosing e =
        match x.AskSaveChanges() with
        | Ok() -> ()
        | Error() -> e.Cancel <- true
        base.OnClosing e

    member x.ShowError(ex : exn) =
        MessageBox.Show(
            x, $"{ex.Message}\r\n\r\n{ex.StackTrace}", MainWindowBase.AppName,
            MessageBoxButton.OK, MessageBoxImage.Error)
        |> ignore

    member x.TryOrPrompt f =
        try f()
        with ex ->
            x.ShowError ex

    static member AutoSavePath = Path.Join(appDir, "__temp_autosaved.vog")
    member val private AutoSaveTimer = LateInit<_>()

    member x.DoAutoSave() =
        x.TryOrPrompt <| fun () ->
            if not !!x.ProgramModel.CompIsSaved then
                x.ProgramModel.SaveACopy MainWindowBase.AutoSavePath

    member x.OnWindowLoaded() =
        // Ask import previous autosave file
        if File.Exists MainWindowBase.AutoSavePath then
            let msgboxResult =
                MessageBox.Show("检测到自动保存文件，是否加载？", MainWindowBase.AppName, MessageBoxButton.YesNo, MessageBoxImage.Question)
            match msgboxResult with
            | MessageBoxResult.Yes ->
                x.TryOrPrompt <| fun () ->
                    x.ProgramModel.Import MainWindowBase.AutoSavePath
            | _ -> ()

        // setup timer
        let autoSaveTimer =
            DispatcherTimer(
                TimeSpan.FromMinutes 5.0,
                DispatcherPriority.Background,
                (fun sender e -> x.DoAutoSave()),
                x.Dispatcher)
        autoSaveTimer.Start()
        x.AutoSaveTimer.Set autoSaveTimer

    member x.OnWindowClosed() =
        let autoSaveTimer = x.AutoSaveTimer.Value
        autoSaveTimer.Stop()

        File.Delete MainWindowBase.AutoSavePath

    member x.PromptSaveLocation() = result {
        let saveFileDialog =
            SaveFileDialog(
                FileName = (!!x.ProgramModel.CompFilePathOp |> Option.defaultValue !!x.ProgramModel.CompFileName),
                DefaultExt = ".vog",
                Filter = "Vogen Package|*.vog")
        let dialogResult = saveFileDialog.ShowDialog x
        if dialogResult ?= true then
            let filePath = saveFileDialog.FileName
            return filePath
        else
            return! Error() }

    member x.SaveACopy() = result {
        try let! filePath = x.PromptSaveLocation()
            x.ProgramModel.SaveACopy filePath
        with ex ->
            x.ShowError ex
            return! Error() }

    member x.SaveAs() = result {
        try let! filePath = x.PromptSaveLocation()
            x.ProgramModel.Save filePath
        with ex ->
            x.ShowError ex
            return! Error() }

    member x.Save() = result {
        match !!x.ProgramModel.CompFilePathOp with
        | None ->
            return! x.SaveAs()
        | Some filePath ->
            try x.ProgramModel.Save filePath
            with ex ->
                x.ShowError ex
                return! x.SaveAs() }

    member x.AskSaveChanges() = result {
        if not !!x.ProgramModel.CompIsSaved then
            let messageBoxResult =
                MessageBox.Show(
                    x, $"Save changes to {!!x.ProgramModel.CompFileName}?", MainWindowBase.AppName,
                    MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Yes)
            match messageBoxResult with
            | MessageBoxResult.Yes -> return! x.Save()
            | MessageBoxResult.No -> ()
            | _ -> return! Error() }

    member x.New() = result {
        do! x.AskSaveChanges()
        x.ProgramModel.New() }

    member x.Open() = result {
        do! x.AskSaveChanges()
        try let openFileDialog =
                OpenFileDialog(
                    DefaultExt = ".vog",
                    Filter = "Vogen Package|*.vog")
            let dialogResult = openFileDialog.ShowDialog x
            if dialogResult ?= true then
                let filePath = openFileDialog.FileName
                x.ProgramModel.Open filePath
            else
                return! Error()
        with ex ->
            x.ShowError ex
            return! Error() }

    member x.Import() = result {
        do! x.AskSaveChanges()
        try let openFileDialog =
                OpenFileDialog(
                    Filter = "Supported file formats|*.vog;*.vpr")
            let dialogResult = openFileDialog.ShowDialog x
            if dialogResult ?= true then
                let filePath = openFileDialog.FileName
                x.ProgramModel.Import filePath
            else
                return! Error()
        with ex ->
            x.ShowError ex
            return! Error() }

    member x.ExportWav() : Result<unit, unit> = result {
        let saveFileDialog =
            let defaultFileName = !!x.ProgramModel.CompFilePathOp |> Option.defaultValue !!x.ProgramModel.CompFileName
            let defaultFileName = Path.GetFileNameWithoutExtension defaultFileName + ".wav"
            SaveFileDialog(
                FileName = defaultFileName,
                DefaultExt = ".wav",
                Filter = "Wave PCM Audio|*.wav")
        let dialogResult = saveFileDialog.ShowDialog x
        if dialogResult ?= true then
            let filePath = saveFileDialog.FileName
            x.ProgramModel.ExportAudio filePath }

    member x.ExportM4a() : Result<unit, unit> = result {
        let saveFileDialog =
            let defaultFileName = !!x.ProgramModel.CompFilePathOp |> Option.defaultValue !!x.ProgramModel.CompFileName
            let defaultFileName = Path.GetFileNameWithoutExtension defaultFileName + ".m4a"
            SaveFileDialog(
                FileName = defaultFileName,
                DefaultExt = ".m4a",
                Filter = "MP4 Audio|*.m4a")
        let dialogResult = saveFileDialog.ShowDialog x
        if dialogResult ?= true then
            let filePath = saveFileDialog.FileName
            x.ProgramModel.ExportAudio filePath }

    member x.EditTempo() =
        let chart = !!x.ProgramModel.ActiveChart
        let initTempo = chart.Comp.Bpm0

        let undoWriter = x.ProgramModel.UndoRedoStack.BeginPushUndo(EditCompPanelValue, chart)

        let revertChanges() =
            x.ProgramModel.ActiveChart |> Rp.set chart
            undoWriter.UnpushUndo()

        x.TempoPopup.Open(initTempo.ToString()) revertChanges <| fun tempoText ->
            let newTempoOp = tempoText.Trim() |> Double.TryParse |> Option.ofByRef

            match newTempoOp with
            | Some newTempo when newTempo |> betweenInc 40.0 300.0 ->
                if newTempo = initTempo then
                    revertChanges()
                else
                    let uttDiffDict = chart.Comp.Utts.ToImmutableDictionary(id, fun (utt : Utterance) -> utt.SetBpm0 newTempo)
                    let newComp = chart.Comp.SetBpm(newTempo).SetUtts(ImmutableArray.CreateRange(chart.Comp.Utts, fun utt -> uttDiffDict.[utt]))
                    let newChart = chart.SetActiveUtt(newComp, chart.ActiveUtt |> Option.map(fun utt -> uttDiffDict.[utt]))

                    x.ProgramModel.ActiveChart |> Rp.set newChart
                    undoWriter.PutRedo(!!x.ProgramModel.ActiveChart)
                    x.ProgramModel.CompIsSaved |> Rp.set false

                Ok()

            | _ ->
                Error()

    member x.EditTimeSig() =
        let chart = !!x.ProgramModel.ActiveChart
        let initTimeSig = chart.Comp.TimeSig0

        let undoWriter = x.ProgramModel.UndoRedoStack.BeginPushUndo(EditCompPanelValue, chart)

        let revertChanges() =
            x.ProgramModel.ActiveChart |> Rp.set chart
            undoWriter.UnpushUndo()

        x.TimeSigPopup.Open(initTimeSig.ToString()) revertChanges <| fun timeSigText ->
            let newTimeSigOp = timeSigText.Trim() |> TimeSignature.TryParse

            match newTimeSigOp with
            | Some newTimeSig ->
                if newTimeSig = initTimeSig then
                    revertChanges()
                else
                    x.ProgramModel.ActiveChart |> Rp.set(chart.SetComp(chart.Comp.SetTimeSig newTimeSig))
                    undoWriter.PutRedo(!!x.ProgramModel.ActiveChart)
                    x.ProgramModel.CompIsSaved |> Rp.set false

                Ok()

            | _ ->
                Error()

    member x.LoadAccom() =
        x.TryOrPrompt <| fun () ->
            let openFileDialog =
                OpenFileDialog(
                    Filter = "Audio Files|*.*")
            let dialogResult = openFileDialog.ShowDialog x
            if dialogResult ?= true then
                let filePath = openFileDialog.FileName
                x.ProgramModel.LoadAccom filePath

