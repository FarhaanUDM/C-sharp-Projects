using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
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


namespace LogicielNettoyagePC
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string version = "1.0.0";
        //window temp folder
        public DirectoryInfo winTemp;
        //apps temp folder
        public DirectoryInfo appTemp;

        public MainWindow()
        {
            InitializeComponent();

            winTemp = new DirectoryInfo(@"C:\Windows\Temp");
            appTemp = new DirectoryInfo(System.IO.Path.GetTempPath());
            CheckActu();
            //GetDate();
        }

        public void CheckActu()
        {
            string url = "http://localhost:8080/siteweb/";
            //recuperation via un url
            using (WebClient client = new WebClient())
            {
                string actu = client.DownloadString(url);
                if (actu != String.Empty)
                {
                    actuTxt.Content = actu;
                    actuTxt.Visibility = Visibility.Visible;
                    banner.Visibility = Visibility.Visible;
                }
            }
        }

        public void CheckVersion()
        {
            string url = "http://localhost:8080/siteweb/version.txt";
            using(WebClient client = new WebClient())
            {
                string v = client.DownloadString(url);
                if (version != v)
                {
                    MessageBox.Show("Une mise a jour est dispo !","Mise a jour",MessageBoxButton.OK,MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Votre logiciel est a jour !", "Mise a jour", MessageBoxButton.OK, MessageBoxImage.Information);
                     
                }
            }
        }

        public void SaveDate()
        {
            string date = DateTime.Today.ToString();
            File.WriteAllText("date.txt", date);
        }

        public void GetDate()
        {
            string dateFichier = File.ReadAllText("data.txt");
            if (dateFichier != String.Empty)
            {
                date.Content = dateFichier;
            }
        }

        //Calcul la taille d'un dossier
        public long DirSize(DirectoryInfo dir)
        {
            return dir.GetFiles().Sum(fi => fi.Length) + dir.GetDirectories().Sum(di => DirSize(di)); ;
        }

        //vider un dossier (supprimer)
        public void ClearTempData(DirectoryInfo di)
        {
            foreach(FileInfo file in di.GetFiles())
            {
                try
                {
                    file.Delete();
                    Console.WriteLine(file.FullName);
                }
                catch (Exception e)
                {
                    continue;
                }
            }

            foreach(DirectoryInfo dir in di.GetDirectories())
            {
                try
                {
                    dir.Delete(true);
                    Console.WriteLine(dir.FullName);
                }
                catch (Exception ex)
                {
                    continue;
                }
            }
        }

        //Analyser des dossiers
        public void AnalyseFolders()
        {
            Console.WriteLine("Debut de l'Analyse ! ");
            long totalSize = 0;

            // Diviser par 1M pu avoir la taille en MB
            try
            {
                totalSize += DirSize(winTemp) / 1000000;
                totalSize += DirSize(appTemp) / 1000000;

            }
            catch (Exception ex)
            {
                Console.WriteLine("Impossible d'analyser les dossiers :" + ex.Message);
            }
           
            espace.Content = totalSize + " Mb";
            titre.Content = "Analyse effectue";
            date.Content = DateTime.Today;
            SaveDate();
        }

        // Button Clicks
        private void Button_MAJ_Click(object sender, RoutedEventArgs e)
        {
            CheckVersion();
        }

        private void Button_Histo_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("ToDO: Creer page historique", "Historique", MessageBoxButton.OK);
        }

        private void Button_Web_Click(object sender, RoutedEventArgs e)
        {
            try 
            {
                //Naviguer vers un lien URL
                Process.Start(new ProcessStartInfo("https://www.youtube.com/watch?v=Rzbz7mXa0qQ&list=RDRzbz7mXa0qQ&start_radio=1")
                {
                    UseShellExecute = true
                });
            }
            catch (Exception ex) 
            {
                Console.WriteLine("Error ! " + ex.Message);
            }
            
           
        }

        private void Button_Netto_Click(object sender, RoutedEventArgs e)
        {
            //Analyser : Trouver des fichiers a supprimer dans le temp folder
            Console.WriteLine("Debut de Nettoyage ... ! ");
            btnClean.Content = "Nettoyage en cours..";
            
            Clipboard.Clear();

            try
            {
                ClearTempData(winTemp);
            }
            catch (Exception ex )
            {
                Console.WriteLine("Erreur :" + ex.Message);
            }

            try
            {
                ClearTempData(appTemp);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur :" + ex.Message);
            }

            btnClean.Content = "NETTOYAGE TERMINER !";
            titre.Content = "Analyse effectue";
            espace.Content = "0  Mb";
        }

        private void Button_Ana_Click(object sender, RoutedEventArgs e)
        {
            AnalyseFolders();
        }



    }
}
