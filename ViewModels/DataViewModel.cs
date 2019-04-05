using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Forms;
using LAB05.Models;
using LAB05.Tools;
using LAB05.Tools.Managers;

namespace LAB05.ViewModels
{
    internal class DataViewModel : BaseViewModel
    {
        #region Constructors

        public DataViewModel()
        {
            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;
            Load();
            StartWorkingThread();
            StationManager.StopThreads += StopWorkingThread;
            ViewSource.Source = _processMap;
            ViewSource.View.Filter = ShowOnlyBargainsFilter;
        
        }

        #endregion


        #region Fields

        private static ConcurrentDictionary<int, ProcessItem>
            _processMap = new ConcurrentDictionary<int, ProcessItem>();

        private KeyValuePair<int, ProcessItem> _selectedProcess;
        private CollectionViewSource _viewSource = new CollectionViewSource();

        private string[] _filterBy = {"Id", "Name", "Window Title"};
        private string[] _sortBy = {"Id", "Name", "Window Title", "Memory Usage"};

        private RelayCommand<object> _openCommand;
        private RelayCommand<object> _filterCommand;
        private RelayCommand<object> _terminateCommand;


        private Thread _workingThread;
        private Thread _workingThread2;

        private CancellationToken _token;
        private CancellationTokenSource _tokenSource;

        #endregion

        #region Properties

        public CollectionViewSource ViewSource
        {
            get
            {
                KeyValuePair<int, ProcessItem> t = _selectedProcess;
                _viewSource?.View?.Refresh();
                SelectedProcess = t;

                return _viewSource;
            }
        }

        public string FilterText { get; set; }

        public string[] FilterBy
        {
            get { return _filterBy; }
        }

        public string[] SortBy
        {
            get { return _sortBy; }
        }


        public int FilterByIndex { get; set; } = 1;

        public KeyValuePair<int, ProcessItem> SelectedProcess
        {
            get { return _selectedProcess; }
            set
            {
                _selectedProcess = value;

                OnPropertyChanged();
                OnPropertyChanged("ProcessModules");
                OnPropertyChanged("ProcessThreads");
                OnPropertyChanged("ThreadsNumber");
            }
        }


        public RelayCommand<object> OpenCommand
        {
            get
            {
                return _openCommand ?? (_openCommand = new RelayCommand<object>(OpenImplementation, CanDoWithProcess));
            }
        }

        public RelayCommand<object> TerminateCommand
        {
            get
            {
                return _terminateCommand ?? (_terminateCommand = new RelayCommand<object>(
                           TerminateImplementation, CanDoWithProcess));
            }
        }

        public RelayCommand<object> FilterCommand
        {
            get
            {
                return _filterCommand ?? (_filterCommand = new RelayCommand<object>(
                           (o =>
                           {
                               _viewSource.View.Refresh();
                               OnPropertyChanged("ViewSource");
                           })));
            }
        }

        #endregion

        #region ProcessProps

        public ProcessModuleCollection ProcessModules
        {
            get { return SelectedProcess.Value?.Modules; }
        }

        public ProcessThreadCollection ProcessThreads
        {
            get { return SelectedProcess.Value?.Threads; }
        }

        public int ThreadsNumber
        {
            get
            {
                if (SelectedProcess.Value != null)
                    return SelectedProcess.Value.Threads.Count;
                else

                    return 0;
            }
        }

        #endregion


        private async void OpenImplementation(object obj)
        {
            LoaderManeger.Instance.ShowLoader();
            await Task.Run((() =>
            {
                if (String.IsNullOrWhiteSpace(SelectedProcess.Value.FileLocation))
                {
                    MessageBox.Show("Access denied", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string argument = "/select, \"" + SelectedProcess.Value.FileLocation + "\"";
                Process.Start("explorer.exe", argument);
            }));
            LoaderManeger.Instance.HideLoader();
        }

        private async void TerminateImplementation(object obj)
        {
            LoaderManeger.Instance.ShowLoader();
            await Task.Run(() =>
            {
                try
                {
                    SelectedProcess.Value.TerminateProcess();
                    OnPropertyChanged("ViewSource");
                }
                catch (Exception e)
                {
                    MessageBox.Show("Access denied", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            });
            LoaderManeger.Instance.HideLoader();
        }

        private bool CanDoWithProcess(object obj)
        {
            return SelectedProcess.Value != null;
        }

        private void StartWorkingThread()
        {
            _workingThread = new Thread(WorkingThreadProcess);
            _workingThread2 = new Thread(WorkingThreadProcess2);
            _workingThread.Start();
            _workingThread2.Start();
        }


        private void WorkingThreadProcess()
        {
            int i = 0;
            while (!_token.IsCancellationRequested)
            {
         
                Process[] processes = Process.GetProcesses();

                var old = new HashSet<int>(_processMap.Keys);

                foreach (var process in processes)
                {
                    _processMap.GetOrAdd(process.Id, new ProcessItem(process));
                    old.Remove(process.Id);
                    if (_token.IsCancellationRequested)
                        return;
                }

                foreach (var o in old)
                {
                    ProcessItem s;
                    _processMap.TryRemove(o, out s);
                    if (_token.IsCancellationRequested)
                        return;
                }

                OnPropertyChanged("ViewSource");

      
                for (int j = 0; j < 10; j++)
                {
                    Thread.Sleep(500);

                    if (_token.IsCancellationRequested)
                        break;
                }

                i++;
            }
        }

        private void WorkingThreadProcess2()
        {
            int i = 0;
            while (!_token.IsCancellationRequested)
            {
                foreach (var process in _processMap.Values)
                {
                    process.Update();
                    if (_token.IsCancellationRequested)
                        return;
                }

                OnPropertyChanged("ViewSource");

                for (int j = 0; j < 4; j++)
                {
                    Thread.Sleep(500);

                    if (_token.IsCancellationRequested)
                        break;
                }

                i++;
            }
        }


        private bool ShowOnlyBargainsFilter(object item)
        {
            KeyValuePair<int, ProcessItem> process = (KeyValuePair<int, ProcessItem>) item;
            if (process.Value != null && !String.IsNullOrWhiteSpace(FilterText))
            {
                ProcessItem l = process.Value;

                switch (FilterByIndex)
                {
                    case 0:
                        return l.Id.ToString().Contains(FilterText);
                    case 1:
                        return l.Name.Contains(FilterText);
                    case 2:
                        return l.Title.Contains(FilterText);
                    default:
                        return true;
                }
            }

            return true;
        }

        private async void Load()
        {
            LoaderManeger.Instance.ShowLoader();
            await Task.Run(() =>
            {
                Process[] processes = Process.GetProcesses();

                var old = new HashSet<int>(_processMap.Keys);

                foreach (var process in processes)
                {
                    _processMap.GetOrAdd(process.Id, new ProcessItem(process));
                    old.Remove(process.Id);
                    if (_token.IsCancellationRequested)
                        return;
                }

                foreach (var o in old)
                {
                    ProcessItem s;
                    _processMap.TryRemove(o, out s);
                    if (_token.IsCancellationRequested)
                        return;
                }

                OnPropertyChanged("ViewSource");
            });
            

            LoaderManeger.Instance.HideLoader();
        }

        internal void StopWorkingThread()
        {
            _tokenSource.Cancel();
            _workingThread.Join(2000);
            _workingThread.Abort();
            _workingThread = null;

            _workingThread2.Join(2000);
            _workingThread2.Abort();
            _workingThread2 = null;
        }
    }
}