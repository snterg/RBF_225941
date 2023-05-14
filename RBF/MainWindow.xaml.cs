using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
using Color = System.Drawing.Color;


namespace RBF
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public string PWD = Directory.GetCurrentDirectory() + "\\";

        public string ext = ".png";

        public Dictionary<string, List<int[]>> images;

        public bool istrain = false;

        public string recogn_img_url;

        Network RBF_obj;
        public MainWindow()
        {
            InitializeComponent();
        }

        public void click_gen()
        {
            images = new Dictionary<string, List<int[]>>();
            for (int i = 1; i <= 5; ++i)
                generate_noise(Convert.ToString(i), i);
            MessageBox.Show("Семплы сгенерированы");
        }

        public void generate_noise(string name, int cl)
        {
            string name_file = name + "_frame_";
            Bitmap img;
            List<int[]> iter = new List<int[]>();
            for (int i = 1; i < 9; ++i)
            {
                img = iteration(name,i);
                img.Save("img\\" + name_file + "_it_" + Convert.ToString(i) + ext, System.Drawing.Imaging.ImageFormat.Png);

                if (i < 6)
                iter.Add(inputimg(img));

            }
            images.Add(name, iter);
        }

     
        public Bitmap iteration(string name,int n)
        {
            System.Drawing.Color new_color;
            string path = PWD + name + ext;
            Bitmap new_image = new Bitmap(path);
            Random rnd = new Random();
            int a = -1,b=-1;
            double prob;
            int w = new_image.Width;
            int h = new_image.Height;

            for (int i = 0; i < n*20; ++i)
            { 
                    a = rnd.Next(0, 30);
                    b = rnd.Next(0, 30);

                    if (b <a &&a!=b)
                    a ^= b ^= a ^= b;

                new_color = Color.FromArgb(255, 255, 255, 255);
                    new_image.SetPixel(a, b, new_color);
                }

            for (int i = 0; i < n*20; ++i)
            {
                a = rnd.Next(0, 30);
                b = rnd.Next(0, 30);
                if (b < a && a != b)
                    a ^= b ^= a ^= b;

                new_color = Color.FromArgb(255, 0, 0, 0);
                new_image.SetPixel(a, b, new_color);
            }

            return new_image;
        }

        private void gen_img_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                click_gen();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        public int[] inputimg(Bitmap img)
        {

            int w = img.Width;
            int h = img.Height;

            int[] image = new int[w * h];

            Color clr;

            for (int i = 0; i < h; ++i)
            {
                for (int j = 0; j < w; ++j)
                {

                    clr = img.GetPixel(i, j);
                    if (clr.R == 255)     // Чёрный цвет
                        image[i * w + j] = 1;
                    else  // Белый цвет
                        image[i * w + j] = -1;
                }
            }

            return image;
        }


        private void recogn(object sender, RoutedEventArgs e)
        {
            if(recogn_img_url==null)
            {
                MessageBox.Show("Изображение для распознания еще не загружено!");
                return;
            }

            if (!istrain)
            {
                MessageBox.Show("Сеть еще не натренирована!");
                return;
            }
            else
            {
                Bitmap new_image = new Bitmap(recogn_img_url);
                int[] image = inputimg(new_image);

                Dictionary<string, double> results = RBF_obj.ClassifyImage(image);

                foreach (KeyValuePair<string, double> resultItem in results)
                {
                    if (resultItem.Key.Contains("1"))
                        similarity_1.Content = "Процент подобия:\n" + Convert.ToDouble(resultItem.Value * 100) + "%";
                    if (resultItem.Key.Contains("2"))
                        similarity_2.Content = "Процент подобия:\n" + Convert.ToDouble(resultItem.Value * 100) + "%";
                    if (resultItem.Key.Contains("3"))
                        similarity_3.Content = "Процент подобия:\n" + Convert.ToDouble(resultItem.Value * 100) + "%";
                    if (resultItem.Key.Contains("4"))
                        similarity_4.Content = "Процент подобия:\n" + Convert.ToDouble(resultItem.Value * 100) + "%";
                    if (resultItem.Key.Contains("5"))
                        similarity_5.Content = "Процент подобия:\n" + Convert.ToDouble(resultItem.Value * 100) + "%";
                }
            }
        }

        private void train_click(object sender, RoutedEventArgs e)
        {
            if (images == null)
            {
                MessageBox.Show("Семплы еще не сгенерированы!");
                return;
            }

            RBF_obj = new Network(images);

            if (RBF_obj.iterationCount > 0)
            {
                MessageBox.Show("Сеть натренирована! Всего было задейстовано " + RBF_obj.iterationCount + " итераций");
                istrain = true;
            }
            else
            {
                MessageBox.Show("Сеть еще не натренирована!");
                return;

            }

        }

        private void addition(object sender, RoutedEventArgs e)
        {
            try
            {
                exp.Source = null;
                OpenFileDialog saveFileDialog = new OpenFileDialog();
                saveFileDialog.InitialDirectory = Directory.GetCurrentDirectory();
                saveFileDialog.Filter = "Images(*.jpg,*.png)|*.jpg;*.png|All files(*.*)|*.*";

                if (saveFileDialog.ShowDialog() == false)
                {
                    MessageBox.Show("Изображение не загружено!");
                    return;
                }
                string filename = saveFileDialog.FileName;
                FileInfo f = new FileInfo(filename);

                recogn_img_url = filename;

                Uri u = new Uri(filename, UriKind.RelativeOrAbsolute);
                exp.Source = new BitmapImage(u);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
