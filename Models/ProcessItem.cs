using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.VisualBasic.Devices;

namespace LAB05.Models
{
    class ProcessItem : INotifyPropertyChanged
    {
        public ProcessItem(Process pr)
        {
            _process = pr;
            Name = pr.ProcessName;
            Title = pr.MainWindowTitle;
            Id = pr.Id;
            _cpuUsage = new PerformanceCounter("Process", "% Processor Time", pr.ProcessName, pr.MachineName);
            _proceesFraction = 100.0 / (new ComputerInfo()).TotalPhysicalMemory;

            try
            {
                _startTime = _process.StartTime;
            }
            catch (Exception)
            {
            }
            
            try
            {
                FileLocation = _process.MainModule.FileName;
            }
            catch (Exception)
            {
            }

        }

        #region Fields

        private double _proceesFraction = -1;
        private Process _process;
        private PerformanceCounter _cpuUsage;
        private long _lastTime = -1;
        private long _workingSet = 0;
        private float _cpuUsageFloat;
        private DateTime _startTime = new DateTime();

        #endregion


        #region Properties

        public string Name { get; set; }
        public string Title { get; set; }
        public int Id { get; set; }
        public string FileLocation { get; set; }

        public float CPUUsageFloat
        {
            get { return _cpuUsageFloat; }
        }

        public long MemoryWorkingSet
        {
            get { return _workingSet; }
        }

        public string MemoryUsagePercent
        {
            get { return (_workingSet * _proceesFraction).ToString("0.00"); }
        }

        public string MemoryUsageMB
        {
            get { return (_workingSet / (1024.0 * 1024.0)).ToString("0.00"); }
        }

        public bool IsActive
        {
            get { return _process.Responding; }
        }

        public string CPU
        {
            get
            {
                return _cpuUsageFloat.ToString("0.00");
                
            }
        }

        public DateTime StartTime
        {
            get { return _startTime; }
        }

        public ProcessModuleCollection Modules
        {
            get
            {
                try
                {
                    ProcessModuleCollection a = _process.Modules;
                    return _process.Modules;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public ProcessThreadCollection Threads
        {
            get
            {
                try
                {
                    return _process.Threads;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        #endregion

        public void TerminateProcess()
        {
            _process.Kill();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Update()
        {
            _workingSet = _process.WorkingSet64;
            try
            {
                if (_lastTime == -1)
                {
                    _lastTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    _cpuUsageFloat = 0;
                    _cpuUsage.NextValue();
                }
                else if (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - _lastTime > 1000)
                {
                    _lastTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    _cpuUsageFloat = _cpuUsage.NextValue() / Environment.ProcessorCount;
                }

            }
            catch (Exception)
            {
            }

            OnPropertyChanged("MemoryWorkingSet");
            OnPropertyChanged("MemoryUsageMB");
            OnPropertyChanged("MemoryUsagePercent");
            OnPropertyChanged("CPU");

        }
    }
}