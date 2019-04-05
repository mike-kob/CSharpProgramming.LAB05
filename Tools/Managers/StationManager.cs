﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace LAB05.Tools.Managers
{
    internal static class StationManager
    {
        public static event Action StopThreads;

        internal static DataGrid PersonTable { get; set; }

        

        public static Random r = new Random();

        internal static List<Process> PrList
        {
            get { return _prList;}
        }

        private static List<Process> _prList = new List<Process>(Process.GetProcesses());

        internal static void UpdateList()
        {
            _prList = new List<Process>(Process.GetProcesses());
        }

        internal static void Initialize()
        {
        }

        internal static void CloseApp()
        {
            MessageBox.Show("ShutDown");
            StopThreads?.Invoke();
            Environment.Exit(1);
        }
    }
}
