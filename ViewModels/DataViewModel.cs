using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using LAB05.Models;
using LAB05.Tools;
using LAB05.Tools.Managers;
using LAB05.Tools.Navigation;
using Microsoft.VisualBasic.Devices;

namespace LAB05.ViewModels
{
    internal class DataViewModel : BaseViewModel
    {
        public DataViewModel()
        {
            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;
            StartWorkingThread();
            StationManager.StopThreads += StopWorkingThread;

            IEnumerator<ProcessItem> a = ProcessList.GetEnumerator();
           
            MessageBox.Show(a.MoveNext()+"");
            _curProcess = a.Current;
            MessageBox.Show(_curProcess+"");
            _curProcess.IsSelected = true;
            SelectedProcess = _curProcess;
            OnPropertyChanged("ProcessList");

        }

        #region Fields

        private List<Process> _processList = StationManager.PrList;
        private ProcessItem _selectedProcess;
        private string[] _filterBy = {"Id", "Name", "Window Title"};
        private string[] _sortBy = {"Id", "Name", "Window Title", "Memory Usage"};
        private int _sortByIndex = 1;
        private int _filterByIndex = 1;
        private bool _isAsc = true;
        private int _selectedIndex = -1;

        private RelayCommand<object> _openCommand;
        private RelayCommand<object> _filterCommand;
        private RelayCommand<object> _terminateCommand;


        private Thread _workingThread;
        private CancellationToken _token;
        private CancellationTokenSource _tokenSource;
        private BackgroundWorker _backgroundWorker;
        private Task _backgroundTask;


        private ProcessItem _curProcess;

        #endregion

        #region Properties

        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                if (value != -1)
                {
                    _selectedIndex = value;
                }
            }
        }

        public bool IsAscending
        {
            get { return _isAsc; }
            set
            {
                _isAsc = value;
                OnPropertyChanged("ProcessList");
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

        public int SortByIndex
        {
            get { return _sortByIndex; }
            set
            {
                _sortByIndex = value;
                OnPropertyChanged("ProcessList");
            }
        }

        public int FilterByIndex
        {
            get { return _filterByIndex; }
            set { _filterByIndex = value; }
        }

        public ProcessItem SelectedProcess
        {
            get { return _selectedProcess; }
            set
            {
              
                    _selectedProcess = value;

                    OnPropertyChanged();
                    OnPropertyChanged("ProcessModules");
                    OnPropertyChanged("ProcessThreads");
                
            }
        }

        public int SelectedSortDescriptionByIndex { get; set; }

        public IEnumerable<ProcessItem> ProcessList
        {
            get
            {
           
                _processList = new List<Process>(Process.GetProcesses());

                IEnumerable<ProcessItem> l = (from row in _processList
                    where (String.IsNullOrWhiteSpace(FilterText)
                           || (FilterByIndex == 0 && row.Id.ToString().Contains(FilterText))
                           || (FilterByIndex == 1 && row.ProcessName.Contains(FilterText))
                           || (FilterByIndex == 2 && row.MainWindowTitle.Contains(FilterText))
                        )
                    select new ProcessItem
                    {
                        MemoryWorkingSet = row.WorkingSet64,
                        Id = row.Id,
                        Name = row.ProcessName,
                        Title = row.MainWindowTitle,
                        MyProcess = row
                    });

                switch (SortByIndex)
                {
                    case 0:
                        l = IsAscending ? l.OrderBy(o => o.Id) : l.OrderByDescending(o => o.Id);
                        break;
                    case 1:
                        l = IsAscending ? l.OrderBy(o => o.Name) : l.OrderByDescending(o => o.Name);
                        break;
                    case 2:
                        l = IsAscending ? l.OrderBy(o => o.Title) : l.OrderByDescending(o => o.Title);
                        break;
                    case 3:
                        l = IsAscending
                            ? l.OrderBy(o => o.MemoryWorkingSet)
                            : l.OrderByDescending(o => o.MemoryWorkingSet);
                        break;
                }
  
                return l;
            }
        }

        public RelayCommand<object> OpenCommand
        {
            get { return _openCommand ?? (_openCommand = new RelayCommand<object>(OpenImplementation, CanDoWithProcess)); }
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
                           (o => { OnPropertyChanged("ProcessList"); })));
            }
        }

        #endregion

        #region Process Props

        public ProcessModuleCollection ProcessModules
        {
            get { return SelectedProcess?.Modules; }
        }

        public ProcessThreadCollection ProcessThreads
        {
            get { return SelectedProcess?.Threads; }
        }

        #endregion

        private async void OpenImplementation(object obj)
        {
            LoaderManeger.Instance.ShowLoader();
            await Task.Run((() =>
            {
                if (String.IsNullOrWhiteSpace(SelectedProcess.FileLocation))
                {
                    MessageBox.Show("Access denied", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }


                OnPropertyChanged("ProcessList");
                string argument = "/select, \"" + SelectedProcess.FileLocation + "\"";
                Process.Start("explorer.exe", argument);
            }));
            LoaderManeger.Instance.HideLoader();
        }

        private async void TerminateImplementation(object obj)
        {
            LoaderManeger.Instance.ShowLoader();
            await Task.Run(() => {
                try
                {
                    SelectedProcess?.TerminateProcess();
                    OnPropertyChanged("ProcessList");
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
            return SelectedProcess != null;
        }

        private void StartWorkingThread()
        {
            _workingThread = new Thread(WorkingThreadProcess);
            _workingThread.Start();
        }


        private void WorkingThreadProcess()
        {
            int i = 0;
            while (!_token.IsCancellationRequested)
            {
                ProcessItem tmp = SelectedProcess;

                OnPropertyChanged("ProcessList");
                SelectedProcess = tmp;
                for (int j = 0; j < 10; j++)
                {
                    Thread.Sleep(500);

                    if (_token.IsCancellationRequested)
                        break;
                }
                i++;
            }
        }

        internal void StopWorkingThread()
        {
            _tokenSource.Cancel();
            _workingThread.Join(2000);
            _workingThread.Abort();
            _workingThread = null;
        }
    }
}