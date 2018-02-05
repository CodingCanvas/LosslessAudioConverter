using LosslessAudioConverter.Commands;
using LosslessAudioConverter.Helpers;
using LosslessAudioConverter.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WinForms = System.Windows.Forms;

namespace LosslessAudioConverter.ViewModel
{
    public class DirectorySelectionViewModel : ViewModelBase
    {
        #region Private Members
        private static readonly object _cancelLock = new object();
        private static readonly object _progressLock = new object();

        private static readonly ProgressUpdate NoProgress = ProgressUpdate.Create(0, int.MaxValue);

        private MusicLibraryConverter _musicConverter;

        private CancellationTokenSource _cancelConversionTokenSource = null;
        #endregion

        #region Public Properties

        public ICommand StartConversionCommand { get; internal set; }
        public ICommand CancelConversionCommand { get; internal set; }

        public ICommand ChooseSourceCommand { get; internal set; }
        public ICommand ChooseTargetCommand { get; internal set; }

        public ICommand CloseApplicationCommand { get; internal set; }
        public ICommand StartOverCommand { get; internal set; }

        public bool DirectoriesAreValid { get { return _musicConverter.IsReadyForConversion(); } }

        public string SourceDirectory
        {
            get
            {
                return _musicConverter.SourceDirectory ?? "";
            }
            set
            {
                if (!string.Equals((_musicConverter.SourceDirectory ?? ""), value,
                    StringComparison.OrdinalIgnoreCase))
                {
                    _musicConverter.SourceDirectory = value;
                    RaisePropertyChanged("SourceDirectory");
                    RaisePropertyChanged("DirectoriesAreValid");
                }
            }
        }

        public string TargetDirectory
        {
            get
            {
                return _musicConverter.TargetDirectory ?? "";
            }
            set
            {
                if (!string.Equals((_musicConverter.TargetDirectory ?? ""), value,
                    StringComparison.OrdinalIgnoreCase))
                {
                    _musicConverter.TargetDirectory = value;
                    RaisePropertyChanged("TargetDirectory");
                    RaisePropertyChanged("DirectoriesAreValid");
                }
            }
        }

        public int CurrentProgress { get { return CurrentProgressUpdate.Progress; } }
        public int CurrentProgressTotal { get { return CurrentProgressUpdate.Total; } }

        private ProgressUpdate _currentProgressUpdate;
        public ProgressUpdate CurrentProgressUpdate
        {
            get
            {
                return _currentProgressUpdate;
            }
            set
            {
                if (_currentProgressUpdate.Equals(value))
                {
                    return;
                }

                _currentProgressUpdate = value;
                RaisePropertyChanged("CurrentProgressUpdate");
                RaisePropertyChanged("ProgressText");
            }
        }

        private ApplicationState _currentApplicationState;
        public ApplicationState CurrentApplicationState
        {
            get { return _currentApplicationState; }
            set
            {
                if (_currentApplicationState == value) { return; }
                if (_currentApplicationState == ApplicationState.Cancelling)
                {
                    if (value == ApplicationState.Copying
                        || value == ApplicationState.Starting
                        || value == ApplicationState.Converting)
                    {
                        return; //cannot switch from cancelling to these states.
                    }
                }

                _currentApplicationState = value;
                RaisePropertyChanged("CurrentApplicationState");
                RaisePropertyChanged("ProgressText");
            }
        }

        private const string Operation_Inactive = "Nothing's Happening, yo";
        private const string Operation_Converting = "Converting Lossless Files";
        private const string Operation_Copying = "Copying Lossy Files";
        private const string Operation_Cancelling = "Cancelling...";

        private string GetActiveOperationText()
        {
            switch (CurrentApplicationState)
            {
                case ApplicationState.Starting: return "Choose your directories";
                case ApplicationState.Converting: return "Converting Lossless Files";
                case ApplicationState.Copying: return "Copying Lossy Files";
                case ApplicationState.Cancelling: return "Cancelling...";
                case ApplicationState.CompleteSuccess: return "Complete";
                case ApplicationState.CompleteCancelled: return "Cancelled";
                default: return null;
            }
        }

        public string ProgressText
        {
            get
            {
                if (CurrentApplicationState == ApplicationState.Cancelling
                    || CurrentApplicationState == ApplicationState.CompleteCancelled
                    || CurrentApplicationState == ApplicationState.CompleteSuccess
                    || CurrentApplicationState == ApplicationState.Starting)
                {
                    return GetActiveOperationText();
                }

                return String.Format("{0}: {1}", GetActiveOperationText(), CurrentProgressUpdate);
            }
        }

        #endregion

        #region Constructors

        public DirectorySelectionViewModel()
        {
            _musicConverter = new MusicLibraryConverter();

            //TODO: MUST include directory validation in all these "CanExecute" commands.
            //For now, we enable/disable buttons based on whether the conversion command is executing or not.

            StartConversionCommand = new AwaitableDelegateCommand(StartConversion_Async);
            CancelConversionCommand = new DelegateCommand(CancelConversion, () => !StartConversionCommand.CanExecute(null));

            ChooseSourceCommand = new DelegateCommand(ChooseSource, () => StartConversionCommand.CanExecute(null));
            ChooseTargetCommand = new DelegateCommand(ChooseTarget, () => StartConversionCommand.CanExecute(null));

            CloseApplicationCommand = new DelegateCommand(() => Application.Current.MainWindow.Close());
            StartOverCommand = new DelegateCommand(StartApplicationOver);

            CurrentApplicationState = ApplicationState.Starting;
        }

        #endregion

        #region Public Methods

        public void OnWindowClosing(object sender, CancelEventArgs args)
        {
            //Check that 
            bool changeMadeToSettings = false;

            if (Directory.Exists(SourceDirectory) &&
                !Properties.Settings.Default.previousSourceFolder.Equals(SourceDirectory, StringComparison.OrdinalIgnoreCase))
            {
                Properties.Settings.Default.previousSourceFolder = SourceDirectory;
                changeMadeToSettings = true;
            }

            if (Directory.Exists(TargetDirectory) &&
                !Properties.Settings.Default.previousTargetFolder.Equals(TargetDirectory, StringComparison.OrdinalIgnoreCase))
            {
                Properties.Settings.Default.previousTargetFolder = TargetDirectory;
                changeMadeToSettings = true;
            }

            if (changeMadeToSettings)
                Properties.Settings.Default.Save();
        }

        #endregion

        #region Private Methods

        private void StartApplicationOver()
        {
            throw new NotImplementedException();
        }

        private async Task StartConversion_Async()
        {
            lock (_cancelLock)
            {
                if (_cancelConversionTokenSource != null) return; //Don't run this multiple times, yo.

                _cancelConversionTokenSource = new CancellationTokenSource();
            }

            CurrentApplicationState = ApplicationState.Converting;

            //Reset progress
            CurrentProgressUpdate = NoProgress;

            //Define the progress updates here, not in the task.  This is because the 'Progress' class captures the current thread context to make updates work as expected.
            IProgress<ProgressUpdate> conversionProgress = new Progress<ProgressUpdate>(ConversionProgressCallback);
            IProgress<ProgressUpdate> copyingProgress = new Progress<ProgressUpdate>(CopyingProgressCallback);

            await Task.Run(() => { _musicConverter.ConvertLosslessFiles_Parallel(_cancelConversionTokenSource.Token, conversionProgress, copyingProgress); }, _cancelConversionTokenSource.Token);

            bool canceledEarly = false;
            lock (_cancelLock)
            {
                canceledEarly = _cancelConversionTokenSource.IsCancellationRequested;

                _cancelConversionTokenSource.Dispose();
                _cancelConversionTokenSource = null; //The operation has finished, so set the cancellation source to null to signify it as being such that it is.
            }

            //TODO: Finalize everything!  We're done here!
            if (canceledEarly)
            {
                CurrentApplicationState = ApplicationState.CompleteCancelled;
            }
            else
            {
                CurrentApplicationState = ApplicationState.CompleteSuccess;
            }
        }

        private void CancelConversion()
        {
            lock (_cancelLock)
            {
                if (_cancelConversionTokenSource == null) return; //Task isn't running, nothing to do.

                CurrentApplicationState = ApplicationState.Cancelling;
                _cancelConversionTokenSource.Cancel();
            }
        }

        private void ConversionProgressCallback(ProgressUpdate update)
        {
            lock (_progressLock)
            {
                CurrentApplicationState = ApplicationState.Converting;

                if (CurrentProgressUpdate.Equals(NoProgress))
                {
                    CurrentProgressUpdate = update;
                    RaisePropertyChanged("CurrentProgress");
                    RaisePropertyChanged("CurrentProgressTotal");
                    RaisePropertyChanged("OperationCanBeCancelled");
                }
                else if (update.Progress > CurrentProgressUpdate.Progress)
                {
                    CurrentProgressUpdate = update;
                    RaisePropertyChanged("CurrentProgress");
                }

                //Check if this is the last update (so we can change over to copy progress)
                if (CurrentProgressUpdate.Progress == CurrentProgressUpdate.Total)
                {
                    //time to switch over to copying, yo.  Reset progress to be nice.
                    CurrentProgressUpdate = NoProgress;
                }
            }
        }

        private void CopyingProgressCallback(ProgressUpdate update)
        {
            lock (_progressLock)
            {
                CurrentApplicationState = ApplicationState.Copying;

                if (CurrentProgressUpdate.Equals(NoProgress))
                {
                    CurrentProgressUpdate = update;
                    RaisePropertyChanged("CurrentProgress");
                    RaisePropertyChanged("CurrentProgressTotal");
                    RaisePropertyChanged("OperationCanBeCancelled");
                }
                else if (update.Progress > CurrentProgressUpdate.Progress)
                {
                    RaisePropertyChanged("CurrentProgress");
                    CurrentProgressUpdate = update;
                }
            }
        }

        private void ChooseTarget()
        {
            using (WinForms.FolderBrowserDialog folderBrowser = new WinForms.FolderBrowserDialog())
            {
                folderBrowser.ShowNewFolderButton = true;
                folderBrowser.Description = "Select Output Music Directory";
                folderBrowser.RootFolder = Environment.SpecialFolder.MyComputer;

                if (folderBrowser.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    return;

                TargetDirectory = folderBrowser.SelectedPath;
            }
        }

        private void ChooseSource()
        {
            using (WinForms.FolderBrowserDialog folderBrowser = new WinForms.FolderBrowserDialog())
            {
                folderBrowser.ShowNewFolderButton = false;
                folderBrowser.Description = "Select Lossless Music Directory";
                folderBrowser.RootFolder = Environment.SpecialFolder.MyComputer;

                if (folderBrowser.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    return;

                SourceDirectory = folderBrowser.SelectedPath;
            }
        }

        #endregion
    }
}
