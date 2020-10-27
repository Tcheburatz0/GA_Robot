using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace GA_Robot
{
    public partial class Form1 : Form
    {
        class Board
        {
            const int cell_size = 20;
            static int size_x, size_y;
            int mouse_moving;
            static int W, H;
            Bitmap BMP;
            static PictureBox pic;
            static Color[] colors = { Color.LightGray, Color.DarkGray, Color.LightGray, Color.LightGray };
            static int[,] Matrix;
            static RobotPoint X0, XP, MovingPoint; static RobotPoint[] X;
            Population population;
            static ProgressBar PB;
            static Board board;
            public Board(int size_X, int size_Y, PictureBox pic_, ProgressBar pb)
            {
                board = this;
                size_x = size_X; size_y = size_Y; mouse_moving = 0;PB = pb;
                 W = cell_size * size_x + 1;
                H = cell_size * size_y + 1;
                Matrix = new int[size_x, size_y];
                for (int i = 0; i < size_x; i++)
                    for (int j = 0; j < size_y; j++)
                        Matrix[i, j] = 0;
                X0 = new RobotPoint(0, 0, Color.Black, 2);
                XP = new RobotPoint(size_x - 1, size_y - 1, Color.Red, 3);
                pic = pic_;
                pic.Width = W;
                pic.Height = H;
                X = new RobotPoint[2];
                X[0] = X0; X[1] = XP; MovingPoint = null;
                Draw();
                pic.MouseDown += MouseDown;
                pic.MouseMove += MouseMove;
                pic.MouseUp += MouseUp;
                BMP = new Bitmap(W, H);
            }
            void Draw()
            {
                Bitmap bmp = new Bitmap(W, H);
                Graphics G = Graphics.FromImage(bmp);
                for (int i = 0; i < size_x; i++)
                    for (int j = 0; j < size_y; j++)
                    {
                        DrawCell(i, j, G, colors[Matrix[i, j]]);
                    }
                pic.BackgroundImage = bmp;
                X0.Draw(); XP.Draw();
            }
            static void DrawCell(int i, int j, Graphics G, Color clr)
            {
                int x = i * cell_size, y = j * cell_size;
                G.FillRectangle(new SolidBrush(clr), x, y, cell_size, cell_size);
                G.DrawRectangle(new Pen(Brushes.Black), x, y, cell_size, cell_size);
            }
            int x0, y0, xp, yp;
            void DrawCells(Color clr)
            {
                //if(xp>=size_x||x0>=size_x|| yp >= size_y || y0 >= size_y||xp<0||x0<0||y0<0||yp<0) return;
                Graphics G = Graphics.FromImage(BMP);
                G.Clear(Color.Transparent);
                int ip = Math.Sign(xp - x0), jp = Math.Sign(yp - y0);
                int nx = Math.Abs(xp - x0) + 1, ny = Math.Abs(yp - y0) + 1;
                for (int i = 0; i < nx; i++)
                {
                    int x = x0 + ip * i;
                    for (int j = 0; j < ny; j++)
                    {
                        int y = y0 + jp * j;
                        if (Matrix[x, y] <= 1) DrawCell(x, y, G, clr);
                    }
                }
                pic.Image = BMP;
            }
            void DrawWalls(int value)
            {
                //if (xp >= size_x || x0 >= size_x || yp >= size_y || y0 >= size_y || xp < 0 || x0 < 0 || y0 < 0 || yp < 0) return;
                Graphics G = Graphics.FromImage(pic.BackgroundImage);
                G.DrawImage(BMP, 0, 0);
                int ip = Math.Sign(xp - x0), jp = Math.Sign(yp - y0);
                int nx = Math.Abs(xp - x0) + 1, ny = Math.Abs(yp - y0) + 1;
                for (int i = 0; i < nx; i++)
                {
                    int x = x0 + ip * i;
                    for (int j = 0; j < ny; j++)
                    {
                        int y = y0 + jp * j;
                        if (Matrix[x, y] <= 1) Matrix[x, y] = value;
                    }
                }
                pic.Invalidate();
                if (pic.Image != null)
                {
                    G = Graphics.FromImage(pic.Image);
                    G.Clear(Color.Transparent);
                }
            }
            public void FindOptPath(int n, int GenCount, double alpha, double beta, double gamma,int mut)
            {
                population = new Population(n, GenCount, alpha, beta, gamma,mut);
                PB.Value = 0;PB.Maximum = GenCount;
                if (!population.blocked)
                {
                    population.FindOptInd();
                }
                else MessageBox.Show("Путь блокирован");
            }
            public void ShowPopulation(RichTextBox rtb)
            {
                population.show(rtb);
            }
            private void MouseDown(object sender, MouseEventArgs e)
            {
                int i = e.X / cell_size, j = e.Y / cell_size, cell = Matrix[i, j];

                switch (cell)
                {
                    case 2:
                    case 3: mouse_moving = 1; MovingPoint = X[cell - 2]; break;
                    case 0:
                    case 1:
                        {
                            int clr = 1;
                            if (e.Button == MouseButtons.Right) clr = 0;
                            Graphics G = Graphics.FromImage(pic.BackgroundImage);
                            mouse_moving = 2; DrawCell(i, j, G, colors[clr]);
                            //Matrix[i, j] = clr;
                            x0 = i; y0 = j; xp = i; yp = j;
                            pic.Invalidate();
                        }
                        break;
                }
            }
            private void MouseMove(object sender, MouseEventArgs e)
            {
                int i = e.X / cell_size, j = e.Y / cell_size;
                if (i >= size_x || j >= size_y || i < 0 || j < 0) return;
                switch (mouse_moving)
                {
                    case 1: MovingPoint.Moving(i, j); break;
                    case 2:
                        if (i != xp || j != yp)
                        {
                            Color clr = colors[1];
                            xp = i; yp = j;
                            if (e.Button == MouseButtons.Right) clr = colors[0];
                            DrawCells(clr);
                        }
                        break;
                }
            }
            private void MouseUp(object sender, MouseEventArgs e)
            {
                switch (mouse_moving)
                {
                    case 1:
                        if (Matrix[MovingPoint.move_x, MovingPoint.move_y] != 5 - MovingPoint.value) MovingPoint.Move();
                        else
                        {
                            MovingPoint.CancelMove();
                            X[0].Draw(); X[1].Draw();
                        }
                        MovingPoint = null; break;
                    case 2:
                        {
                            int value = 1;
                            if (e.Button == MouseButtons.Right) value = 0;
                            DrawWalls(value);
                        }
                        break;
                }
                mouse_moving = 0;
            }
            public void SaveFile()
            {
                SaveFileDialog SFD = new SaveFileDialog();
                SFD.Filter = "Файл лабиринта|*.dat";
                if (SFD.ShowDialog() == DialogResult.Cancel) return;
                StreamWriter SW = new StreamWriter(SFD.FileName);
                SW.WriteLine(size_x + "\t" + size_y + "\t"+X0.x + "\t" + X0.y + "\t" + XP.x + "\t" + XP.y);
                for (int i = 0; i < size_x; i++)
                {
                    string str = "";
                    for (int j = 0; j < size_y; j++)
                    {
                        str = str + Matrix[i, j] + "\t";
                    }
                    str = str + "0";
                    SW.WriteLine(str);
                }
                SW.Close();
            }
            public void OpenFile()
            {
                OpenFileDialog OFD = new OpenFileDialog();
                OFD.Filter = "Файл лабиринта|*.dat";
                if (OFD.ShowDialog() == DialogResult.Cancel) return;
                StreamReader SR = new StreamReader(OFD.FileName);
                string str=SR.ReadLine();
                string[] S = str.Split((char)9);
                size_x = int.Parse(S[0]); size_y = int.Parse(S[1]);
                Matrix = new int[size_x, size_y];
                X0 = new RobotPoint(int.Parse(S[2]), int.Parse(S[3]), Color.Black, 2);
                XP = new RobotPoint(int.Parse(S[4]), int.Parse(S[5]), Color.Red, 3);
                mouse_moving = 0;
                W = cell_size * size_x + 1;
                H = cell_size * size_y + 1;
                for (int i = 0; i < size_x; i++)
                {
                    str = SR.ReadLine();
                    S = str.Split((char)9);
                    for (int j = 0; j < size_y; j++)
                    {
                        Matrix[i, j] = int.Parse(S[j]);
                    }
                }
                
                pic.Width = W;
                pic.Height = H;
                Draw();
                MovingPoint = null;
                SR.Close();
            }
            class RobotPoint
            {
                public int x, y, move_x, move_y;
                Color color;
                public int value;
                public RobotPoint(int x_, int y_, Color clr, int value_)
                {
                    color = clr; value = value_;
                    x = x_; y = y_;
                    move_x = x; move_y = y;
                    Matrix[x, y] = value;
                }
                public void Draw()
                {
                    Bitmap bmp = new Bitmap(cell_size, cell_size);
                    Graphics G = Graphics.FromImage(bmp);
                    G.FillRectangle(new SolidBrush(colors[0]), 0, 0, cell_size, cell_size);
                    G.DrawRectangle(new Pen(Brushes.Black), 0, 0, cell_size, cell_size);
                    G.FillEllipse(new SolidBrush(color), cell_size / 2 - 5, cell_size / 2 - 5, 10, 10);
                    Graphics G1 = Graphics.FromImage(pic.BackgroundImage);
                    G1.DrawImage(bmp, new Point(move_x * cell_size, move_y * cell_size));
                    pic.Invalidate();
                }
                public void Moving(int x_, int y_)
                {
                    //move_x = x_; move_y = y_;
                    if ((x_ < size_x && y_ < size_y && x_ >= 0 && y_ >= 0) && (x_ != move_x || y_ != move_y))
                    {
                        Graphics G = Graphics.FromImage(pic.BackgroundImage);
                        DrawCell(move_x, move_y, G, colors[Matrix[move_x, move_y]]);
                        if (Matrix[move_x, move_y] == 5 - value) X[3 - value].Draw();
                        move_x = x_; move_y = y_;
                        Draw();
                    }
                    //pic.BackgroundImage
                }
                public void Move()
                {
                    Matrix[x, y] = 0;
                    x = move_x; y = move_y;
                    Matrix[x, y] = value;
                }
                public void CancelMove()
                {
                    move_x = x; y = move_y = y;
                }
            }
            class Population//класс популяции
            {
                int n; static double alpha, beta, gamma;
                Individ[] individ;internal bool blocked=false;
                static Random rnd = new Random();
                int GenCount;
                Mutation[] mutation=new Mutation[2];Mutation ActiveMutation;
                public Population(int n_, int GenCount_, double alpha_, double beta_, double gamma_,int mut)//конструктор популяции
                {
                    n = n_; GenCount = GenCount_; mutation[0] = new NullMutation();mutation[1] = new TrueMutation();
                    ActiveMutation = mutation[mut];
                    alpha = alpha_; beta = beta_; gamma = gamma_;
                    individ = new Individ[n * 2];
                    individ[0] = new Individ(ref blocked);
                    for (int i = 1; i < n; i++)
                    {
                        individ[i] = new Individ();
                    }
                }
                public void show(RichTextBox rtb)
                {
                    rtb.Text = "";int m = size_x*size_y;
                    for (int i = 0; i < n; i++)
                    {                        
                        for (int j = 0; j < m; j++)
                        {
                            int chr = individ[i].chromo[j];
                            int x = chr >> 8,y=chr-((chr>>8)<<8);
                            rtb.Text = rtb.Text +"("+x+";"+y+")";
                        }
                        rtb.Text = rtb.Text + "\t" + individ[i].Info() + "\n";
                    }
                }
                void Sort()
                {
                    int N = 2 * n;
                    bool stop = false;
                    while (!stop)
                    {
                        stop = true;
                        for (int i = 0; i < N - 1; i++)
                        {
                            if (individ[i].fitness > individ[i + 1].fitness)
                            {
                                Individ ind = individ[i];
                                individ[i] = individ[i + 1];
                                individ[i + 1] = ind;
                                stop = false;
                            }
                        }
                    }
                }

                public void FindOptInd()
                {
                    for (int i = 0; i < GenCount; i++)
                    {
                        for (int j = 0; j < n; j++)
                        {
                            individ[n + j] = new Individ(individ[j], individ[(j + 1) % n]);
                        }
                        Sort();
                        double top = individ[0].fitness;
                        for (int j = 1; j < n; j++)
                        {
                            if ( individ[(n + j + 1) % n].fitness == individ[j].fitness) ActiveMutation.MutationExcecute(individ[j]);
                        }
                        Sort();
                        PB.Value = i + 1;
                        //individ[0].Draw();
                        //MessageBox.Show(individ[0].Info());
                        //MessageBox.Show(i+"");
                    }
                    individ[0].Draw();
                    //MessageBox.Show( individ[0].Info());
                }
                abstract class Mutation
                {
                    public Mutation()
                    {

                    }
                    public abstract void MutationExcecute(Individ ind);
                }
                class NullMutation:Mutation
                {
                    public NullMutation()
                    {

                    }
                    public override void MutationExcecute(Individ ind)
                    {

                    }
                }
                class TrueMutation : Mutation
                {
                    public TrueMutation()
                    {

                    }
                    public override void MutationExcecute(Individ ind)
                    {
                        ind.mutation();
                    }
                }

                public class Individ//класс особи
                {
                    int n;//Число хромосом
                    internal  int[] chromo;
                    double wall_cross = 0, dir_changes = 0, length = 0;
                    internal double fitness;
                    public Individ()//Конструктор особи из начальной популяции
                    {
                        n = size_y * size_x;
                        chromo = new int[n];
                        Generate();
                        fitness = Fitness();
                        //Draw();
                        //
                    }
                    public Individ(ref bool blocked)//Конструктор особи из начальной популяции
                    {
                        n = size_y * size_x;
                        chromo = new int[n];
                        GenerateRight(ref blocked);
                        fitness = Fitness();
                        //Draw();
                        //MessageBox.Show("gh");
                        //
                    }
                    public void mutation()
                    {
                        int k = rnd.Next(0, n);
                        int x = rnd.Next(0, size_x), y = rnd.Next(0, size_y);
                        chromo[k] = (x << 8) | y;
                        fitness = Fitness();
                    }
                    void Generate()
                    {
                        int p =n;
                        int chr = 0,N=n/p;
                        for (int i=0;i<N;i++)
                        {
                            int x =rnd.Next(0, size_x), y= rnd.Next(0, size_y);
                            chr = (x << 8) | y;
                            for (int j = 0; j < p; j++)
                                chromo[i*p+j] = (x << 8) | y;
                            //int k = chromo[i];
                            //MessageBox.Show((k >> 8)+" "+ (k - ((k >> 8) << 8)));
                        }
                        for (int i = 0; i < n %p; i++) chromo[p*N + i] = chr;
                    }
                    void GenerateRight(ref bool blocked)
                    {
                        blocked = false;
                        bool[,] Marked = new bool[size_x, size_y];
                        //int[,] PrevPath = new int[size_x, size_y];
                        for (int i=0;i<size_x;i++)
                            for (int j = 0; j < size_y; j++)
                            {
                                Marked[i, j] = false;
                                //PrevPath[i,j] = 0;
                            }
                        Marked[X0.x, X0.y] = true;
                        GenerateRight(0, 0, 0, Marked,ref blocked);
                    }
                    void GenerateRight(int k,int i, int j,bool[,] Marked,ref bool blocked)
                    {
                        if (i==XP.x && j==XP.y)
                        {
                            for (int p = k; p < n; p++)
                                chromo[p] = chromo[k - 1];
                            //MessageBox.Show((k >> 8) + " " + (k - ((k >> 8) << 8)));
                            return;
                        }
                        chromo[k] = (i << 8) | j;
                        Marked[i, j] = true;
                        if (i < size_x - 1 && !Marked[i + 1, j] && Matrix[i + 1, j] != 1)
                        {
                            GenerateRight(k + 1, i + 1, j, Marked, ref blocked);
                            return;
                        }
                        if (i > 0 && !Marked[i - 1, j] && Matrix[i - 1, j] != 1)
                        {
                            GenerateRight(k + 1, i - 1, j, Marked, ref blocked);
                            return;
                        }
                        if (j > 0 && !Marked[i, j - 1] && Matrix[i, j - 1] != 1)
                        {
                            GenerateRight(k + 1, i, j - 1, Marked, ref blocked);
                            return;
                        }
                        if (j < size_y - 1 && !Marked[i, j + 1] && Matrix[i, j + 1] != 1)
                        {
                            GenerateRight(k + 1, i, j + 1, Marked, ref blocked);
                            return;
                        }
                        //Draw();
                        if(k==0)
                        {
                            blocked = true;
                            return;
                        }
                        int kk = chromo[k - 1];
                        //MessageBox.Show((k >> 8) + " " + (k - ((k >> 8) << 8)));
                        GenerateRight(k - 1, kk >> 8, kk - ((kk >> 8) << 8), Marked, ref blocked);
                    }
                    public void Draw()
                    {
                        Bitmap bmp = new Bitmap(W, H);
                        Graphics G = Graphics.FromImage(bmp);
                        int x = X0.x * cell_size + cell_size / 2, y = X0.y * cell_size + cell_size / 2;
                        for (int i = 0; i < n; i++)
                        {
                            int k = chromo[i];
                            int xx = k>>8,yy= k-((k>>8)<<8);
                            int x1 = cell_size * xx + cell_size /2 , y1 = cell_size * yy + cell_size /2 ;
                            if (!(x == x1 && y == y1))
                            {
                                G.DrawLine(new Pen(Brushes.Black), x, y, x1, y1);
                                x = x1; y = y1;
                            }
                        }
                        G.DrawLine(new Pen(Brushes.Black), x, y, XP.x * cell_size + cell_size / 2, XP.y* cell_size + cell_size / 2);
                        pic.Image = bmp;
                    }
                    public Individ(Individ male, Individ female)//Конструктор скрещивания
                    {
                        n = size_y * size_x;
                        int k = rnd.Next(0, n);
                        chromo = new int[n];
                        for (int i = 0; i < k; i++)//гены самца
                            chromo[i] = male.chromo[i];
                        for (int i = k; i < n; i++)//гены самки
                            chromo[i] = female.chromo[i];
                        fitness = Fitness();
                    }
                    double WallCross(int x0,int y0,int x1,int y1)
                    {
                        double w = 0;
                        x0 = cell_size * x0+cell_size/2;y0 = cell_size * y0 + cell_size / 2; x1 = cell_size * x1 + cell_size / 2; y1 = cell_size * y1 + cell_size / 2;
                        double XX = x1 - x0, YY = y1 - y0;
                        double len=Math.Sqrt(XX * XX + YY * YY);
                        int T = (int)len;
                        for (int t = 0; t < T; t++)
                        {
                            int xt = (int)((x0 + XX * t/len)/cell_size), yt = (int)((y0 + YY * t/len) / cell_size);
                            if (Matrix[xt, yt] == 1) w++;
                        }
                        return w;
                    }
                    public double Fitness()
                    {
                        length = 0; dir_changes = 0; wall_cross = 0;
                        int x = X0.x, y = X0.y, xx, yy, XX, YY;
                        for (int i = 0; i < n; i++)
                        {
                            int k = chromo[i];
                            xx = k >> 8; yy = k - ((k >> 8) << 8);
                            XX = xx - x; YY = yy - y;
                            length = length + Math.Sqrt(XX * XX + YY * YY);
                            if (!((XX== 0) || (YY== 0))) dir_changes++;
                            if (length > 0) wall_cross = wall_cross+ WallCross(x, y, xx, yy);
                                x = xx; y = yy;
                        }
                        //MessageBox.Show(wall_cross+"");
                        xx = XP.x; yy = XP.y;
                        XX = xx - x; YY = yy - y;
                        length = length + Math.Sqrt(XX *XX + YY * YY);
                        wall_cross = wall_cross + WallCross(x, y, xx, yy);

                        return alpha * length + beta * wall_cross + gamma * dir_changes;
                    }
                    public string Info()
                    {
                        return length + "\t" + wall_cross + "\t" + dir_changes + "\t" + fitness;
                    }
                }
            }
        }
        public Form1()
        {
            InitializeComponent();
        }
        Board board;
        private void Form1_Load(object sender, EventArgs e)
        {
            board = new Board(20, 10, pictureBox1,progressBar1);
        }

        private void button1_Click(object sender, EventArgs e)
        {


        }

        private void найтиОптимальныйПутьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            double alpha=0, beta=0, gamma=0;
            int GenCount=0, n=0;
            bool correct = double.TryParse(textBox3.Text,out alpha) && double.TryParse(textBox4.Text, out beta) && double.TryParse(textBox5.Text, out gamma);
            correct = correct && int.TryParse(textBox1.Text, out n) && int.TryParse(textBox2.Text, out GenCount) && (n > 10 && n < 200) && (GenCount > 10) && GenCount < 5000;
            if (!correct) return;
            board.FindOptPath(n, GenCount, alpha, beta, gamma,checkBox1.Checked.GetHashCode());
        }

        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            board.SaveFile();
        }

        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            board.OpenFile();
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void направленийToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }
    }
}
