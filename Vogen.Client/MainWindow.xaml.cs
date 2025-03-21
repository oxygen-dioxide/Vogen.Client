﻿using Microsoft.FSharp.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Vogen.Client.Controls;
using Vogen.Client.ViewModel;
using Vogen.Client.Views;

namespace Vogen.Client
{
    public partial class MainWindow : MainWindowBase
    {
        public MainWindow()
        {
            InitializeComponent();
            noteChartEditPanel.Focus();

            Loaded += (sender, e) => OnWindowLoaded();
            Closed += (sender, e) => OnWindowClosed();
        }

        public override TextBoxPopupBase TempoPopup => tempoPopup;
        public override TextBoxPopupBase TimeSigPopup => timeSigPopup;

        private void CanExecuteCmdUndo(object sender, CanExecuteRoutedEventArgs e) =>
            e.CanExecute = ProgramModel.UndoRedoStack.CanUndo.Value;
        private void CanExecuteCmdRedo(object sender, CanExecuteRoutedEventArgs e) =>
            e.CanExecute = ProgramModel.UndoRedoStack.CanRedo.Value;
        private void CanExecuteCmdSelectionHasLyric(object sender, CanExecuteRoutedEventArgs e) =>
            e.CanExecute = ProgramModel.ActiveChart.Value.SelectedNotes.Any(note => !note.IsHyphen);
        private void CanExecuteCmdHasSelection(object sender, CanExecuteRoutedEventArgs e) =>
            e.CanExecute = ProgramModel.ActiveChart.Value.SelectedNotes.Count > 0;
        private void CanExecuteCmdHasActiveUtt(object sender, CanExecuteRoutedEventArgs e) =>
            e.CanExecute = OptionModule.IsSome(ProgramModel.ActiveChart.Value.ActiveUtt);
        private void CanExecuteCmdIsPlaying(object sender, CanExecuteRoutedEventArgs e) =>
            e.CanExecute = ProgramModel.IsPlaying.Value;
        private void CanExecuteCmdIsNotPlaying(object sender, CanExecuteRoutedEventArgs e) =>
            e.CanExecute = !ProgramModel.IsPlaying.Value;

        private void OnExecuteCmdNew(object sender, ExecutedRoutedEventArgs e) => New();
        private void OnExecuteCmdOpen(object sender, ExecutedRoutedEventArgs e) => Open();
        private void OnExecuteCmdSave(object sender, ExecutedRoutedEventArgs e) => Save();
        private void OnExecuteCmdSaveAs(object sender, ExecutedRoutedEventArgs e) => SaveAs();
        private void OnExecuteCmdImport(object sender, ExecutedRoutedEventArgs e) => Import();
        private void OnExecuteCmdExportWav(object sender, ExecutedRoutedEventArgs e) => ExportWav();
        private void OnExecuteCmdExportM4a(object sender, ExecutedRoutedEventArgs e) => ExportM4a();
        private void OnExecuteCmdExit(object sender, ExecutedRoutedEventArgs e) => Close();

        private void OnExecuteCmdUndo(object sender, ExecutedRoutedEventArgs e) => ProgramModel.Undo();
        private void OnExecuteCmdRedo(object sender, ExecutedRoutedEventArgs e) => ProgramModel.Redo();
        private void OnExecuteCmdCut(object sender, ExecutedRoutedEventArgs e) => noteChartEditPanel.CutSelectedNotes();
        private void OnExecuteCmdCopy(object sender, ExecutedRoutedEventArgs e) => noteChartEditPanel.CopySelectedNotes();
        private void OnExecuteCmdPaste(object sender, ExecutedRoutedEventArgs e) => noteChartEditPanel.Paste();
        private void OnExecuteCmdDelete(object sender, ExecutedRoutedEventArgs e) => noteChartEditPanel.DeleteSelectedNotes();
        private void OnExecuteCmdSelectAll(object sender, ExecutedRoutedEventArgs e) => noteChartEditPanel.SelectAll();
        private void OnExecuteCmdBlurUtt(object sender, ExecutedRoutedEventArgs e) => noteChartEditPanel.BlurUtt();

        private void OnExecuteCmdSetGrid(object sender, ExecutedRoutedEventArgs e) => noteChartEditPanel.Quantization = (long)e.Parameter;

        private void OnExecuteCmdEditTempo(object sender, ExecutedRoutedEventArgs e) => EditTempo();
        private void OnExecuteCmdEditTimeSig(object sender, ExecutedRoutedEventArgs e) => EditTimeSig();
        private void OnExecuteCmdEditLyrics(object sender, ExecutedRoutedEventArgs e) => noteChartEditPanel.EditSelectedNoteLyrics();

        private void OnExecuteCmdLoadAccom(object sender, ExecutedRoutedEventArgs e) => LoadAccom();
        private void OnExecuteCmdClearAccom(object sender, ExecutedRoutedEventArgs e) => ProgramModel.ClearAccom();
        private void OnExecuteCmdSynth(object sender, ExecutedRoutedEventArgs e) => ProgramModel.Synth(Dispatcher);
        private void OnExecuteCmdResynth(object sender, ExecutedRoutedEventArgs e) => ProgramModel.Resynth(Dispatcher);
        private void OnExecuteCmdClearSynth(object sender, ExecutedRoutedEventArgs e) => ProgramModel.ClearAllSynth();
        private void OnExecuteCmdPlayStop(object sender, ExecutedRoutedEventArgs e) => ProgramModel.PlayOrStop();
        private void OnExecuteCmdPlay(object sender, ExecutedRoutedEventArgs e) => ProgramModel.Play();
        private void OnExecuteCmdStop(object sender, ExecutedRoutedEventArgs e) => ProgramModel.Stop();
    }
}
