using MobileMap;
using MobileMapToDXF.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MobileMapToDXF
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Object.Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public MainWindowViewModel()
        {
            SelectMapCommand = new RelayCommand(SelectMapCallback, c => true);
            SelectDXFCommand = new RelayCommand(SelectDXFCallback, c => true);
            CreateCommand = new RelayCommand(CreateCallback, c => true);
        }

        private MapFile MapFile;

        public string Status { get => Status_; set { SetProperty(ref Status_, value); } }
        private string Status_ = string.Empty;

        public string Source { get => Source_; set { SetProperty(ref Source_, value); } }
        private string Source_ = string.Empty;

        public string Destination { get => Destination_; set { SetProperty(ref Destination_, value); } }
        private string Destination_ = string.Empty;

        public int Progress { get => Progress_; set { SetProperty(ref Progress_, value); } }
        private int Progress_ = 0;

        public ICommand SelectMapCommand { get; }
        public ICommand SelectDXFCommand { get; }
        public ICommand CreateCommand { get; }

        private void SelectMapCallback(object parameter)
        {
            Microsoft.Win32.OpenFileDialog file = new Microsoft.Win32.OpenFileDialog
            {
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = "Map file (*.map)|*.map",
                FilterIndex = 1
            };

            file.ShowDialog();

            if (string.IsNullOrEmpty(file.FileName))
                return;

            Source = file.FileName;
        }

        private void SelectDXFCallback(object parameter)
        {
            Microsoft.Win32.OpenFileDialog file = new Microsoft.Win32.OpenFileDialog
            {
                CheckFileExists = false,
                CheckPathExists = false,
                Filter = "DXF (*.dxf)|*.dxf",
                FilterIndex = 1
            };
            file.ShowDialog();
            
            if(string.IsNullOrEmpty(file.FileName))
                return;

            Destination = file.FileName;
        }

        private void CreateCallback(object parameter)
        {
            if (string.IsNullOrEmpty(Source)) return;
            if (string.IsNullOrEmpty(Destination)) return;

            MapFile = new MapFile(Source, true);
            MapFile.LoadMapFromContent(true);

            MapUtils.SaveDXF(MapFile.Map, Destination);


        }

    }
}
