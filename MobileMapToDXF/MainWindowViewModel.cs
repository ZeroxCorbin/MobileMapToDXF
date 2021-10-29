using MobileMap;
using MobileMapToDXF.Commands;
using netDxf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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

        public double Progress { get => Progress_; set { SetProperty(ref Progress_, value); } }
        private double Progress_ = 0;

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

        private async void CreateCallback(object parameter)
        {
            if (string.IsNullOrEmpty(Source))
            {
                Status = "Map file path is empty.";
                return;
            }
                
            if (string.IsNullOrEmpty(Destination))
            {
                Status = "Destination file path is empty.";
                return;
            }

            if(!Path.GetExtension(Destination).Equals(".dxf", StringComparison.CurrentCultureIgnoreCase))
            {
                Status = "The destination file name must have a '.dxf' extention.";
                return;
            }


            MapFile = new MapFile(Source, true);

            if (!MapFile.LoadMapFromContent(true))
            {
                Status = "Could not parse the map contents.";
                return;
            }


            if (await Task.Run(() => Convert()))
                Status = "Complete...";
            else
                Status = "DXF could not be saved.";
        }

        public bool Convert()
        {
            // create a new document, by default it will create an AutoCad2000 DXF version
            DxfDocument doc = new DxfDocument();

            double total = MapFile.Map.Geometry.Lines.Count + MapFile.Map.Geometry.Points.Count;
            double count = 0;

            Status = $"Processing {total} entities. {(total > 50000 ? "This will take a long time." : "This will not take long.")}";

            foreach (MapGeometry.Line l in MapFile.Map.Geometry.Lines)
            {
                // an entity
                netDxf.Entities.Line entity = new netDxf.Entities.Line(new Vector2(l.Start.X, l.Start.Y), new Vector2(l.End.X, l.End.Y));
                // add your entities here
                doc.AddEntity(entity);

                count++;
                Progress = count / total * 100;
            }

            foreach (System.Drawing.Point p in MapFile.Map.Geometry.Points)
            {
                netDxf.Entities.Point entity = new netDxf.Entities.Point(new Vector2(p.X, p.Y));
                // add your entities here
                doc.AddEntity(entity);

                count++;
                Progress = count / total * 100;
            }

            // save to file
            return doc.Save(Destination);
        }
    }
}
