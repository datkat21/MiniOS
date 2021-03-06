using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using System.Collections.Specialized;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using System.Reflection;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.Http;
using Newtonsoft.Json;
using System.IO;
using Xceed.Wpf.Toolkit;
namespace TestingWpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public class CSharpScriptEngine
    {
        private static ScriptState<object> scriptState = null;
        public static object Execute(string code)
        {
            scriptState = scriptState == null ? CSharpScript.RunAsync(code).Result : scriptState.ContinueWithAsync(code).Result;
            if (scriptState.ReturnValue != null && !string.IsNullOrEmpty(scriptState.ReturnValue.ToString()))
                return scriptState.ReturnValue;
            return null;
        }
    }

    public class StoreApps
    {
        [JsonProperty("version")]
        public string Version { get; set; }
        [JsonProperty("date")]
        public string Date { get; set; }
        [JsonProperty("apps")]
        // public List<StoreApps_Apps> Apps { get; set; }
        public List<StoreApps_Apps> Apps { get; set; }
    }
    public class StoreApps_Apps
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("icon")]
        public string Icon { get; set; }
        [JsonProperty("version")]
        public string Version { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("downloadUri")]
        public string DownloadUri { get; set; }
    }

    //    public class StoreApps_Apps
    //    {
    //        [JsonProperty("name")]
    //        public string Name;
    //        [JsonProperty("description")]
    //        public string Description;
    //        [JsonProperty("created")]
    //        public string Created;
    //        [JsonProperty("badge")]
    //        public string BadgeUrl;
    //    }

    public partial class MainWindow : Window
    {
        TreeViewItem dummyNode = new();
        TreeView foldersItem = new();
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            foreach (string s in Directory.GetLogicalDrives())
            {
                TreeViewItem item = new TreeViewItem();
                item.Header = s;
                item.Tag = s;
                item.FontWeight = FontWeights.Normal;
                item.Items.Add(dummyNode);
                item.Expanded += new RoutedEventHandler(folder_Expanded);
                foldersItem.Items.Add(item);
            }
        }

        void folder_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)sender;
            if (item.Items.Count == 1 && item.Items[0] == dummyNode)
            {
                item.Items.Clear();
                try
                {
                    foreach (string s in Directory.GetDirectories(item.Tag.ToString()))
                    {
                        TreeViewItem subitem = new TreeViewItem();
                        subitem.Header = s.Substring(s.LastIndexOf("\\") + 1);
                        subitem.Tag = s;
                        subitem.FontWeight = FontWeights.Normal;
                        subitem.Items.Add(dummyNode);
                        subitem.Expanded += new RoutedEventHandler(folder_Expanded);
                        item.Items.Add(subitem);
                    }
                }
                catch (Exception) { }
            }
        }


        // HttpClient is intended to be instantiated once per application, rather than per-use. See Remarks.
        static readonly HttpClient client = new HttpClient();
        public List<StoreApps> apps;

        public MainWindow()
        {
            InitializeComponent();

        }

        private void ClickPreview_BG(Border bdr, bool LetGo = false, string color = null, string thick = null)
        {

            // create listbox item example

            // ListBoxItem itm = new()
            // {
            //     Tag = "hidden",
            //     Content = "4"
            // };
            // DebugConsoleView.Items.Add(itm);
            // DebugConsoleView.SelectedIndex = DebugConsoleView.Items.Count - 1;
            // DebugConsoleView.ScrollIntoView(DebugConsoleView.SelectedItem);



            ThicknessConverter tc = new();
            Thickness newthick = bdr.BorderThickness;
            if (thick != null)
            {
                newthick = (Thickness)tc.ConvertFromString(thick);
            }
            switch (LetGo)
            {
                case true:
                    if (!string.IsNullOrEmpty(color))
                    {
                        bdr.Background = (SolidColorBrush)new BrushConverter().ConvertFromString(color);
                        bdr.BorderThickness = newthick;
                    }
                    else
                    {
                        bdr.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#252525");
                        bdr.BorderThickness = newthick;
                    }
                    break;

                case false:
                    if (!string.IsNullOrEmpty(color))
                    {
                        bdr.Background = (SolidColorBrush)new BrushConverter().ConvertFromString(color);
                        bdr.BorderThickness = newthick;
                    }
                    else
                    {
                        bdr.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#353535");
                        bdr.BorderThickness = newthick;
                    }
                    break;
            }
        }

        // PreviewMouseDown

        private void GeneralPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            ClickPreview_BG(sender as Border, false, "#404040");
        }

        // PreviewMouseUp

        private void LeaveButtonBG_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            ClickPreview_BG(LeaveButtonBG, true, null, "0, 0, 2, 0");
            LogOffConfirm.Show();
            LogOffConfirm.Focus();
            LogOffBG.Visibility = Visibility.Visible;
            LogOffBG.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#66000000");
            // #66000000
        }

        private void SettingsApp_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            ClickPreview_BG(SettingsAppBG, true, null, "0, 0, 2, 0");
            SettingsWindow.Show();
            SettingsWindow.Focus();
        }

        private void StoreApp_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            ClickPreview_BG(StoreAppBG, true, null, "0, 0, 2, 0");
            StoreWindow.Show();
            StoreWindow.Focus();
            AttemptConnection();
        }

        int filesWindowCount = 0;

        private void FilesAppBG_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            ClickPreview_BG(sender as Border, true, null, "0, 0, 2, 0");
            ChildWindow newWindow = new()
            {
                Name = "FilesWindow" + filesWindowCount,
                Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#3f3f3f"),
                Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFFFFF"),
                Caption = "Files",
                CaptionForeground = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFFFFF"),
                CaptionIcon = new BitmapImage(new Uri(@"pack://application:,,,/Assets/openLight.png")),
                WindowBackground = (SolidColorBrush)new BrushConverter().ConvertFromString("#353535"),
                Height = 300,
                Width = 1000
            };
            DockPanel dp = new();
            DockPanel sp = new()
            {
                Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#4A4A4A"),
                Width = 500,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            TextBlock tb = new()
            {
                Text = newWindow.Name,
                Padding = new Thickness(10)
            };
            TreeView tv = new()
            {
                Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#4A4A4A"),
                Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFFFFF"),

            };
            DockPanel.SetDock(sp, Dock.Left);
            sp.Children.Add(tv);
            dp.Children.Add(sp);
            dp.Children.Add(tb);
            newWindow.Content = dp;
            // Get list of files in the specific directory.
            // ... Please change the first argument.

            // var files = CustomSearcher.GetDirectories(@"C:\Users\austi\Documents\", "*", SearchOption.AllDirectories);

            // Display all the files.
            //foreach (string file in files)
            //{
            //    TreeViewItem itm = new TreeViewItem()
            //    {
            //        Header = file 
            //    };
            //    tv.Items.Add(itm);
            //}
            newWindow.Tag = "FilesAppBG";
            newWindow.CloseButtonClicked += new RoutedEventHandler(GeneralCloseButtonClicked);
            filesWindowCount++;
            TheWindowContainer.Children.Add(newWindow);
            newWindow.Show();
        }

        // MouseEnter


        private void GeneralMouseEnter(object sender, MouseEventArgs e)
        {
            ClickPreview_BG(sender as Border, false);
        }

        // MouseLeave

        private void GeneralMouseLeave(object sender, MouseEventArgs e)
        {
            ClickPreview_BG(sender as Border, true);
        }

        // CloseButtonClicked

        private void GeneralCloseButtonClicked(object sender, RoutedEventArgs e)
        {
            var btn = sender as ChildWindow;
            Border bdr = this.FindName(btn.Tag.ToString()) as Border;
            if (bdr.Name == "FilesAppBG")
            {
                if (filesWindowCount == 1)
                {
                    bdr.BorderThickness = new Thickness(0, 0, 0, 0);
                    filesWindowCount--;
                }
                else
                {
                    filesWindowCount--;
                }
            }
            else
            {
                bdr.BorderThickness = new Thickness(0, 0, 0, 0);
            }
        }

        // Other Buttons Clicked

        private void LogOffNo_Click(object sender, RoutedEventArgs e)
        {
            LeaveButtonBG.BorderThickness = new Thickness(0, 0, 0, 0);
            LogOffConfirm.Close();
        }

        private void LogOffYes_Click(object sender, RoutedEventArgs e)
        {
            LeaveButtonBG.BorderThickness = new Thickness(0, 0, 0, 0);
            LogOffConfirm.Close();
            Application.Current.Shutdown();
        }

        private void WelcomeModalClose_Clicked(object sender, RoutedEventArgs e)
        {
            WelcomeWindow.Close();
        }

        private void WelcomeModalStart_Clicked(object sender, RoutedEventArgs e)
        {
            WelcomeWindow.Close();
            Xceed.Wpf.Toolkit.MessageBox.Show("This is currently not finished.");
        }

        // Other related to debug console

        private void ShowDebugConsole_Click(object sender, RoutedEventArgs e)
        {
            DebugConsoleWindow.Show();
        }

        private void ClearDebugConsole(object sender, RoutedEventArgs e)
        {
            DebugConsoleView.Items.Clear();
        }

        private void ChangeBackground_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files (*.png, *.jpeg, *.jpg)|*.png;*.jpeg;*.jpg";
            if (openFileDialog.ShowDialog() == true)
            {
                ListBoxItem itm = new()
                {
                    Content = "Successfully opened dialog!"
                };
                DebugConsoleView.Items.Add(itm);
                ImageBrush myBrush = new ImageBrush();
                Image image = new Image();
                image.Source = new BitmapImage(new Uri(openFileDialog.FileName));
                myBrush.ImageSource = image.Source;
                gridObj.Background = myBrush;
                // Xceed.Wpf.Toolkit.MessageBox.Show("Changed background to " + openFileDialog.FileName);
                // Xceed.Wpf.Toolkit.MessageBox.Show("Grid background: " + gridObj.Background);
            }

        }

        // Other functions down here.
        StoreApps deserialized = null;
        private async void AttemptConnection()
        {
            // Call asynchronous network methods in a try/catch block to handle exceptions.
            try
            {
                HttpResponseMessage response = await client.GetAsync("https://cnmn.io/minios/store/json/");
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                // Above three lines can be replaced with new helper method below
                // string responseBody = await client.GetStringAsync(uri);

                // storeOutputBox.Text += responseBody;
                deserialized = JsonConvert.DeserializeObject<StoreApps>(responseBody);
                int index = 0;
                foreach (StoreApps_Apps App in deserialized.Apps)
                {
                    DockPanel mp = new()
                    {
                        Margin = new Thickness(5),
                        LastChildFill = false,
                        VerticalAlignment = VerticalAlignment.Top,
                        Width = 200
                    };
                    DebugConsoleView.Items.Add("Got " + deserialized.Apps[index].Name + " v" + deserialized.Apps[index].Version);
                    Image appIcon = new()
                    {
                        Source = new BitmapImage(new Uri(deserialized.Apps[index].Icon)),
                        Width = 30,
                        Height = 30
                    };
                    TextBlock appName = new()
                    {
                        FontSize = 20,
                        Text = deserialized.Apps[index].Name,
                        TextAlignment = TextAlignment.Center
                    };
                    TextBlock appDesc = new()
                    {
                        Text = deserialized.Apps[index].Description,
                        TextAlignment = TextAlignment.Center
                    };
                    TextBlock appDownload_tb = new()
                    {
                        Text = "Download",
                        TextAlignment = TextAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    Button appDownload = new()
                    {
                        Content = appDownload_tb,
                        Style = this.FindResource("FlatButtonStyle") as Style,
                        Margin = new Thickness(0, 5, 0, 0),
                        Width = 100,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };
                    DockPanel.SetDock(appName, Dock.Top);
                    DockPanel.SetDock(appIcon, Dock.Top);
                    DockPanel.SetDock(appDesc, Dock.Bottom);
                    DockPanel.SetDock(appDownload, Dock.Bottom);
                    mp.Children.Add(appIcon);
                    mp.Children.Add(appName);
                    mp.Children.Add(appDownload);
                    mp.Children.Add(appDesc);
                    StorePanel.Children.Add(mp);
                    Xceed.Wpf.Toolkit.MessageBox.Show("Adding " + appName);
                    index++;
                }

            }
            catch (HttpRequestException err)
            {
                DebugConsoleView.Items.Add("Exception Caught!");
                DebugConsoleView.Items.Add("Message: " + err.Message);
            }
        }

        private void SaveSettings(string settings)
        {
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        }

        private void GeneralRightMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            DebugConsoleView.Items.Add("Right Mouse Button Up");
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await CSharpScript.EvaluateAsync(@"string[] lines =
        {
            ""First line"", ""Second line"", ""Third line"" 
        };
        await File.WriteAllLinesAsync(""WriteLines.txt"", lines);", ScriptOptions.Default.AddImports("System", "System.IO", "System.Threading.Tasks"));
        }
    }
    public class CustomSearcher
    {
        public static List<string> GetDirectories(string path, string searchPattern = "*",
            SearchOption searchOption = SearchOption.AllDirectories)
        {
            if (searchOption == SearchOption.TopDirectoryOnly)
                return Directory.GetDirectories(path, searchPattern).ToList();

            var directories = new List<string>(GetDirectories(path, searchPattern));

            for (var i = 0; i < directories.Count; i++)
                directories.AddRange(GetDirectories(directories[i], searchPattern));

            return directories;
        }

        private static List<string> GetDirectories(string path, string searchPattern)
        {
            try
            {
                return Directory.GetDirectories(path, searchPattern).ToList();
            }
            catch (UnauthorizedAccessException)
            {
                return new List<string>();
            }
        }
    }
}
