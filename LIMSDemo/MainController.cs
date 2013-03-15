﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace LIMSDemo
{
    public class MainController : IMainController
    {
        private string _exportFileName = string.Empty;
        private readonly LIMSConnection _lims = new LIMSConnection();
        private WindowMain _view;

        public void AbortRun()
        {
            if (_lims.AbortExperiment())
                Display("Experiment aborted.");
            else
                DisplayLastStatus();

            UpdateControls();
        }

        public void AcquireLibrary()
        {
            if (_lims.LoadLibrary())
                Display("COM Object Loaded.");
            else
                DisplayLastStatus();

            UpdateControls();
        }

        public void ClearQueryParameters()
        {
            if (_view == null) return;

            _view.txtQueryName.Text = "";
            _view.txtQueryOwner.Text = "";
            _view.txtQueryType.Text = "";
            _view.rbQueryDateAll.IsChecked = true;
        }

        public void CloseDoor()
        {
            if (_lims.CloseDoor())
                Display("Door closed.");
            else
                DisplayLastStatus();

            UpdateControls();
        }

        private void Display(string aMessage)
        {
            Display(aMessage, true);
        }

        private void Display(string aMessage, bool aClearDisplay)
        {
            if (_view == null) return;

            if (aClearDisplay)
                _view.txtMessages.Text = aMessage + "\r\n";
            else
                _view.txtMessages.Text += aMessage + "\r\n";
        }

        private void DisplayLastStatus()
        {
            if (_lims.HasLastResult)
                Display(_lims.LastResult.Message + "\r\n" + _lims.LastResult.UserMessage);
        }

        public void ExperimentStatus()
        {
            string lStatus;
            if (_lims.GetExperimentStatus(ExperimentName, out lStatus))
            {
                Display("Experiment Status:");
                Display(lStatus, false);
            }
            else
                DisplayLastStatus();
        }

        public void ExperimentSummary()
        {
            string lSummary;
            if (_lims.GetExperimentSummary(ExperimentName, out lSummary))
            {
                Display("Experiment Summary:");
                Display(lSummary, false);
            }
            else
                DisplayLastStatus();
        }

        public void ExportExperiment()
        {
            string lInitialDirectory;
            try
            {
                lInitialDirectory = System.IO.Path.GetDirectoryName(_exportFileName);
            }
            catch
            {
                lInitialDirectory = Environment.GetEnvironmentVariable("CSIDL_DEFAULT_MYDOCUMENTS");
            }

            var lDialog = new OpenFileDialog
            {
                Title = "Chose Export File",
                Filter = "Object Export (*.ixo)|*.ixo|All files (*.*)|*.*",
                DefaultExt = "ixo",
                CheckPathExists = true,
                CheckFileExists = false,
                InitialDirectory = lInitialDirectory
            };

            if (lDialog.ShowDialog() == DialogResult.OK)
            {
                _exportFileName = lDialog.FileName;
                if (_lims.ExportExperiment(ExperimentName, _exportFileName))
                    Display("Experiment exported to " + _exportFileName);
                else
                    DisplayLastStatus();                    
            }
            else
                Display("Export cancelled.");
        }

        public void GetContainerBarcode()
        {
            string lBarcode;

            if (_lims.GetContainerBarcode(out lBarcode))
                Display("Container Barcode: " + lBarcode);
            else
            {
                Display("Failed to obtain container barcode.", false);
                DisplayLastStatus();
            }

            UpdateControls();
        }

        public void GetSensor()
        {
            bool lSensor;

            if (_lims.GetContainerSensor(out lSensor))
                Display("Container sensor: " + (lSensor ? "ON" : "OFF"));
            else
            {
                Display("Failed to obtain sensor value.", false);
                DisplayLastStatus();
            }

            UpdateControls();
        }

        public void GetStatus()
        {
            string lStatus;

            if (_lims.GetStatus(out lStatus))
                Display("Status message: " + lStatus);
            else
            {
                Display("Failed to obtain status message.", false);
                DisplayLastStatus();
            }

            UpdateControls();
        }

        public void Login()
        {
            if (_view == null) return;

            if (_lims.Connect(_view.txtHostname.Text, _view.txtUsername.Text, _view.txtPassword.Password))
                Display("Logged in.");
            else
                DisplayLastStatus();

            UpdateControls();
        }

        public void Logout()
        {
            if (_lims.Disconnect()) Display("Logged out.");
            UpdateControls();
        }

        public void OpenDoor()
        {
            if (_lims.OpenDoor())
                Display("Door opened.");
            else
                DisplayLastStatus();

            UpdateControls();
        }

        public void Query()
        {
            if (_view == null) return;

            LIMSClientLib.LIMSQueryDateType lDateType;
            var lFromDate = DateTime.MinValue;
            var lToDate = DateTime.MaxValue;

            if (_view.rbQueryDateAll.IsChecked ?? false)
                lDateType = LIMSClientLib.LIMSQueryDateType.qdtAllDateQuery;
            else if (_view.rbQueryDateCreated.IsChecked ?? false)
            {
                lDateType = LIMSClientLib.LIMSQueryDateType.qdtCreationDateQuery;
                // lFromDate
                // lToDate
            }
            else
            {
                lDateType = LIMSClientLib.LIMSQueryDateType.qdtModificationDateQuery;
                // lFromDate
                // lToDate
            }

            List<LIMSConnection.QueryResult> lResults;

            if (_lims.ExecuteQuery(
                    new LIMSConnection.QueryParameters
                    {
                        Name = _view.txtQueryName.Text,
                        ObjectType = _view.txtQueryType.Text,
                        Owner = _view.txtQueryOwner.Text,
                        DateType = lDateType,
                        DateFrom = lFromDate,
                        DateTo = lToDate
                    }, out lResults))
            {
                Display("Query Successful.");

                foreach (var i in lResults)
                    Display(string.Format("  Name: '{0}' Path: '{1}' Type: '{2}' Created: {3} Modified: {4}",
                        i.Name, i.Path, i.ObjectType, i.Created, i.Modified), false);
            }
            else
                DisplayLastStatus();
        }

        public void ReleaseLibrary()
        {
            _lims.UnloadLibrary();
            Display("COM object released.");
            UpdateControls();
        }

        public void ReserveInstrument()
        {            
            if (_lims.ReserveInstrument())
                Display("Instrument reserved.");
            else
                DisplayLastStatus();

            UpdateControls();
        }

        public void ToggleSensor()
        {
            bool lSensor;

            if (_lims.GetContainerSensor(out lSensor))
            {
                if (_lims.SetContainerSensor(!lSensor))
                    Display("Container sensor value toggled.");
                else
                {
                    Display("Failed to set new container sensor value.", false);
                    DisplayLastStatus();
                }
            }
            else
            {
                Display("Failed to read container sensor value.", false);
                DisplayLastStatus();
            }

            UpdateControls();
        }

        public void UnreserveInstrument()
        {
            if (_lims.UnreserveInstrument())
                Display("Instrument released.");
            else
                DisplayLastStatus();

            UpdateControls();
        }

        private void UpdateControls()
        {
            if (_view == null) return;
                    
            bool lIsLoaded = _lims.IsLibraryLoaded;
            bool lIsConnected = _lims.IsConnected;
            bool lIsResserved = _lims.IsReserved;

            _view.btnAcquire.IsEnabled = !lIsLoaded;
            _view.btnRelease.IsEnabled = lIsLoaded && !lIsConnected;
            _view.txtHostname.IsEnabled = lIsLoaded && !lIsConnected;
            _view.txtUsername.IsEnabled = lIsLoaded && !lIsConnected;
            _view.txtPassword.IsEnabled = lIsLoaded && !lIsConnected;
            _view.btnConnect.IsEnabled = lIsLoaded && !lIsConnected;
            _view.btnDisconnect.IsEnabled = lIsConnected;

            _view.btnReserve.IsEnabled = lIsConnected;
            _view.btnUnreserve.IsEnabled = lIsConnected;

            _view.btnOpenDoor.IsEnabled = lIsResserved;
            _view.btnCloseDoor.IsEnabled = lIsResserved;
            _view.btnStatus.IsEnabled = lIsResserved;
            _view.btnContainerBarcode.IsEnabled = lIsResserved;
            _view.btnGetSensor.IsEnabled = lIsResserved;
            _view.btnToggleSensor.IsEnabled = lIsResserved;
            _view.btnStartRun.IsEnabled = lIsResserved;
            _view.btnAbortRun.IsEnabled = lIsResserved;

            _view.txtQueryName.IsEnabled = lIsConnected;
            _view.txtQueryType.IsEnabled = lIsConnected;
            _view.txtQueryOwner.IsEnabled = lIsConnected;
            _view.rbQueryDateAll.IsEnabled = lIsConnected;
            _view.rbQueryDateModified.IsEnabled = lIsConnected;
            _view.rbQueryDateCreated.IsEnabled = lIsConnected;
            _view.btnQueryClear.IsEnabled = lIsConnected;
            _view.btnExecuteQuery.IsEnabled = lIsConnected;

            _view.txtExperimentName.IsEnabled = lIsConnected;
            _view.btnExperimentStatus.IsEnabled = lIsConnected;
            _view.btnExperimentSummary.IsEnabled = lIsConnected;
            _view.btnExperimentExport.IsEnabled = lIsConnected;
        }

        public string ExperimentName {
            get
            {
                if (_view == null) return string.Empty;
                return _view.txtExperimentName.Text;
            }
        }

        public WindowMain View
        {
            get { return _view; }
            set
            {
                _view = value;
                UpdateControls();
            }
        }
    }
}