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
        public MainWindow()
        {
            InitializeComponent();
        }

        public void click_gen()
        {
            for(int i=1;i<=5;++i)
            generate_noise(Convert.ToString(i)+"_class");
            MessageBox.Show("Семплы сгенерированы");
        }
        
        public void generate_noise(string name)
        {
            string name_file = name + "_frame_";
            Bitmap img;
            for (int i = 0; i < 3; ++i)
            {
                img = iteration(name);
                img.Save(name_file+"_it_"+Convert.ToString(i)+ext, System.Drawing.Imaging.ImageFormat.Png);
            }
           
        }

        public Bitmap iteration(string name)
        {
            System.Drawing.Color new_color;
            string path = PWD + name + ext;
            Bitmap new_image = new Bitmap(path);
            Random rnd = new Random();
            int r = -1;
            double prob;
            int w = new_image.Width;
            int h = new_image.Height;
            for (int i = 0; i <w ; ++i)
                for (int j = 0; j < h; ++j)
                {
                    //prob = (1/(2* Math.Sqrt(2*Math.PI))*Math.Exp(-(Math.Pow(j-3,2)/2*Math.Pow(i+j,2))));
                    r = rnd.Next(0, 256);
                   // new_color = prob>0.05&&(i+j)%2==0?Color.FromArgb(255, 0, 0, 0) : Color.FromArgb(0, 255, 255, 255);
                    new_color =Color.FromArgb(255, r, r, r);
                    new_image.SetPixel(i, j, new_color);
                }
            return new_image;
        }

        private void gen_img_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                click_gen();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }
    }
}
