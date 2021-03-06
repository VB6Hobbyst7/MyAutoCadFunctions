﻿using LayerConfigWindow.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace LayerConfigWindow.ViewModels
{
    public class LayerViewModel : ViewModelBase
    {
        public LayerViewModel()
        {
            testCommand = new DelegateCommand(o => testButton());
        }

        private void testButton()
        {
            MessageBox.Show("YEAH");
        }

        public ICommand testCommand { get; private set; }
    }
}
