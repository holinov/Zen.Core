using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace Zen.RenameProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private ObservableCollection<ActionBase> _toDo = new ObservableCollection<ActionBase>();
        private readonly ObservableCollection<ActionBase> _done = new ObservableCollection<ActionBase>();
        private RenameController _rename;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Solution.Text != null)
            {
                _rename = new RenameController(Solution.Text, RenameFrom.Text, RenameTo.Text, true,
                                               renameDirs.IsChecked == true);
                _rename.DetectWork();
                _done.Clear();
                _toDo = new ObservableCollection<ActionBase>(_rename.Actions);
                ToRename.ItemsSource = _toDo;
                Renamed.ItemsSource = _done;
            }
        }

        private void ProcessAction(ActionBase a)
        {
            try
            {
                a.Action();
                Dispatcher.BeginInvoke(new Action(() =>
                    {
                        _toDo.Remove(a);
                        _done.Add(a);
                    }));
            }
            catch (Exception e)
            {
                Debug.WriteLine(a + " " + e.Message);
                Dispatcher.BeginInvoke(new Action(() => { logBox.Text += a + " " + e.Message + "\r\n"; }));
            }
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog()
                {
                    Filter = "*.sln|*.sln|*.*|*.*"
                };
            if (dlg.ShowDialog() == true)
            {
                Solution.Text = dlg.FileName;
                RenameFrom.Text = System.IO.Path.GetFileName(dlg.FileName).Split('.')[0];
            }
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(() =>
                {
                    foreach (var action in _rename.Actions.OfType<FindAndReplaceAction>())
                    {
                        ProcessAction(action);
                    }
                    foreach (var action in _rename.Actions.OfType<RenameFileAction>())
                    {
                        ProcessAction(action);
                    }
                    foreach (var action in _rename.Actions.OfType<RenameDirAction>())
                    {
                        ProcessAction(action);
                    }
                });
        }
    }
}
