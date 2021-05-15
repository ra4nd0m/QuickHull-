using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace QuickHull_
{
    public partial class Form1 : Form
    {
        List<Point> points;
        List<Point> hull;
        public Form1()
        {
            InitializeComponent();
            pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics graphics = Graphics.FromImage(pictureBox1.Image);
            graphics.Clear(Color.White);
            points = new List<Point>();
            hull = new List<Point>();
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            Graphics graphics = Graphics.FromImage(pictureBox1.Image);
            Pen pen = new Pen(Color.Black);
            if(e.Button == MouseButtons.Left)
            {
                points.Add(new Point(e.X, e.Y));
                graphics.DrawRectangle(pen, e.X, e.Y, 1, 1);
                pictureBox1.Invalidate();
            }
        }
        private int Side(Point p1, Point p2, Point p)
        {
            int rez = (p.Y - p1.Y) * (p2.X - p1.X) - (p2.Y - p1.Y) * (p.X - p1.X);
            if (rez > 0)
                return 1;
            if (rez < 0)
                return -1;
            return 0;
        }
        private int Distance(Point p1, Point p2, Point p)
        {
            return Math.Abs((p.Y - p1.Y) * (p2.X - p1.X) - (p2.Y - p1.Y) * (p.X - p1.X));
        }
        private void QuickHull()
        {
            if(points.Count <= 3)
            {
                foreach(var p in points)
                {
                    hull.Add(p);
                }
                return;
            }
            Point pmin = points
                .Select(p => new { point = p, x = p.X })
                .Aggregate((p1, p2) => p1.x < p2.x ? p1 : p2).point;
            Point pmax = points
                .Select(p => new { point = p, x = p.X })
                .Aggregate((p1, p2) => p1.x > p2.x ? p1 : p2).point;
            hull.Add(pmin);
            hull.Add(pmax);
            List<Point> left = new List<Point>();
            List<Point> right = new List<Point>();
            for(int i = 0; i<points.Count; i++)
            {
                Point p = points[i];
                if (Side(pmin, pmax, p) == 1)            
                    left.Add(p);
                if (Side(pmin, pmax, p) == -1)
                    right.Add(p);

            }
            CreateHull(pmin, pmax, left);
            CreateHull(pmax, pmin, right);
        }

        private void CreateHull(Point a, Point b, List<Point> points)
        {
            int pos = hull.IndexOf(b);
            if (points.Count == 0)
                return;
            if(points.Count == 1)
            {
                Point pp = points[0];
                hull.Insert(pos, pp);
                return;
            }
            int dist = int.MinValue;
            int point = 0;
            for(int i = 0; i < points.Count; i++)
            {
                Point pp = points[i];
                int distance = Distance(a, b, pp);
                if (distance > dist)
                {
                    dist = distance;
                    point = i;
                }
            }
            Point p = points[point];
            hull.Insert(pos, p);
            List<Point> ap = new List<Point>();
            List<Point> pb = new List<Point>();
            for(int i=0;i<points.Count; i++)
            {
                Point pp = points[i];
                if (Side(a, p, pp) == 1)
                    ap.Add(pp);
                if (Side(p, b, pp) == 1)
                    pb.Add(pp);
            }
            CreateHull(a, p, ap);
            CreateHull(p, b, pb);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            hull.Clear();
            if (points.Count != 0)
            {
                pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                Graphics graphics = Graphics.FromImage(pictureBox1.Image);
                graphics.Clear(Color.White);
                Pen pen = new Pen(Color.Black);
                foreach(var p in points)
                {
                    graphics.DrawRectangle(pen, p.X, p.Y, 1, 1);
                    pictureBox1.Invalidate();
                }
            }
            Graphics graphics1 = Graphics.FromImage(pictureBox1.Image);
            Pen pen1 = new Pen(Color.Black);
            var watch = Stopwatch.StartNew();
            QuickHull();
            watch.Stop();
            label1.Text = watch.ElapsedMilliseconds.ToString();
            graphics1.DrawPolygon(pen1, hull.ToArray());
            pictureBox1.Invalidate();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics graphics = Graphics.FromImage(pictureBox1.Image);
            graphics.Clear(Color.White);
            points.Clear();
            hull.Clear();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
                return;
            string filename = openFileDialog1.FileName;
            try
            {
                using (StreamReader sr = new StreamReader(filename,System.Text.Encoding.Default))
                {
                    string line;                   
                    int filex;
                    int filey;
                    while((line = sr.ReadLine())!= null)
                    {
                        bool space_passed = false;
                        string x = "";
                        string y = "";
                        for (int i = 0; i < line.Length; i++)
                        {
                            if (line[i] != ' ' && space_passed == false)
                                x += line[i];
                            if (line[i] == ' ')
                                space_passed = true;
                            if (line[i] != ' ' && space_passed == true)
                                y += line[i];
                        }
                        filex = int.Parse(x);
                        filey = int.Parse(y);
                        Graphics graphics = Graphics.FromImage(pictureBox1.Image);
                        Pen pen = new Pen(Color.Black);
                        graphics.DrawRectangle(pen, filex, filey, 1, 1);
                        pictureBox1.Invalidate();
                        points.Add(new Point { X = filex, Y = filey });
                    }
                }
            }
            catch
            {
                MessageBox.Show("Ошибка открытия файла", "Ошибка!", MessageBoxButtons.OK);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.Cancel)
                return;
            string filename = saveFileDialog1.FileName;
            try
            {
                using (StreamWriter sw = new StreamWriter(filename))
                {
                    for (int i = 0; i < hull.Count; i++)
                    {
                        sw.WriteLine("{0} {1}", hull[i].X, hull[i].Y);
                    }
                }
            }
            catch
            {
                MessageBox.Show("Ошибка открытия файла", "Ошибка!", MessageBoxButtons.OK);
            }
        }
    }
}
