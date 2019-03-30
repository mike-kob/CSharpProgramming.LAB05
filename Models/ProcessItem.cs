using System;
using System.Management;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.VisualBasic.Devices;

namespace LAB05.Models
{
    class ProcessItem : INotifyPropertyChanged
    {
        private double _proceesFraction = -1;
        private Process _process;
        private bool _isSelected;

        private DateTime _lastTime = new DateTime();
        private TimeSpan _lastTotalProcessorTime;
        private DateTime _curTime = new DateTime();
        private TimeSpan _curTotalProcessorTime;

        #region Properties

        public bool IsSelected
        {
            get { return _isSelected;}
            set
            {
                _isSelected = value;
                OnPropertyChanged();
            } }

        private double ProcessFraction
        {
            get
            {
                return (_proceesFraction < 0)
                    ? (_proceesFraction = 100.0 / (new ComputerInfo()).TotalPhysicalMemory)
                    : _proceesFraction;
            }
        }

        public string Name { get; set; }
        public string Title { get; set; }
        public long Id { get; set; }

        public string FileLocation
        {
            get
            {
                try
                {
                    return _process.MainModule.FileName;
                }
                catch (Exception e)
                {
                    return "";
                }
            }
        }


        public long MemoryWorkingSet { get; set; }

        public Process MyProcess
        {
            set { _process = value; }
        }

        public string MemoryUsagePercent
        {
            get { return (MemoryWorkingSet * ProcessFraction).ToString("0.00"); }
        }

        public string MemoryUsageMB
        {
            get { return (MemoryWorkingSet / (1024.0 * 1024.0)).ToString("0.00"); }
        }

        public string CPU
        {
            get
            {
                return "0";
                /*try
                {
                    if (_lastTime == new DateTime())
                    {
                        _lastTime = DateTime.Now;
                        _lastTotalProcessorTime = _process.TotalProcessorTime;
                        return "0";
                    }
                    else
                    {
                        _curTime = DateTime.Now;
                        _curTotalProcessorTime = _process.TotalProcessorTime;

                        double CPUUsage =
                            (_curTotalProcessorTime.TotalMilliseconds - _lastTotalProcessorTime.TotalMilliseconds) /
                            _curTime.Subtract(_lastTime).TotalMilliseconds / Convert.ToDouble(Environment.ProcessorCount);

                        _lastTime = _curTime;
                        _lastTotalProcessorTime = _curTotalProcessorTime;
                        return (CPUUsage * 100).ToString("0.00");

                    }
                }
                catch (Exception e)
                {
                    return "No access";
                }*/
                
            }
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
                catch (Exception e)
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
                catch (Exception e)
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
    }
}