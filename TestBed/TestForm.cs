using System;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using EmguCVExtensions;

namespace TestBed
{
    public partial class TestForm : Form
    {
        public TestForm() {
            InitializeComponent();
        }

        private void imageSelect_FileSelected(object sender, EventArgs e) {
            
            var image = new Image<Gray, byte>(imageSelect.SelectedFiles[0].FullName);
            //List<Contour> contours = ContourProcessing.FindContours(image);

            var copiedImage = new Image<Gray, byte>(image.Width, image.Height);
            Contour c = Contour.Circle;
            copiedImage.Draw(Contour.Circle, new Gray(255), 1);
            //copiedImage.Draw(contours, new Gray(255), 1);

            var pictureBox = new PictureBox();
            pictureBox.Image = copiedImage.Bitmap;
            pictureBox.Size = image.Size;

            flowPanel.Controls.Clear();
            flowPanel.Controls.Add(pictureBox);

            //foreach (Contour contour in contours)
            //    foreach (Point p in contour.Points)
            //        Console.Write(p);
        }


    }
}
