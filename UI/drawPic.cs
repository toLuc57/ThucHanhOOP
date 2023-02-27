using Esgis_Paint.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Esgis_Paint
{
    public partial class drawPic : Form
    {
        #region Variables
        
        FileStream picture_stream;
        FileInfo img;
        Journal log;
        List<Point> allPoints = new List<Point>();
        public Point current = new Point();
        public Point old = new Point();
        Bitmap bm;
        public Graphics g;
        public Pen pen;
        public Pen eraser;
        bool change = false;

        //Parameters that user can change
        Color pen_color;
        decimal pen_width;
        decimal eraser_width;
        SpecialForm Specialform;
        List<SpecialForm> allSpecialForms = new List<SpecialForm>();
        Image Specialform_IMG;

        //Useful to control pen state
        private DrawingState state;
        private bool drawing;

        // Save bitmaps 
        private readonly Stack<Bitmap> undoStack = new Stack<Bitmap>();
        private readonly Stack<Bitmap> redoStack = new Stack<Bitmap>();

        #endregion

        public drawPic()
        {
            InitializeComponent();
            pic.Width = panel1.Width - 10;
            pic.Height = panel1.Height - 10;
            bm = new Bitmap(pic.Width, pic.Height);
            g = Graphics.FromImage(bm);
            g.Clear(Color.White);
            pic.Image = bm;

            //Initialisations
            state = DrawingState.Pen;
            drawing = false;
            change = false;

            log = new Journal();
            eraser_width = 6;
            pen_width = 6;
            pen_color = Color.Black;
            pen = new Pen(Color.Black, (int) pen_width);
            eraser = new Pen(Color.White, (int) eraser_width);
            pen.SetLineCap(System.Drawing.Drawing2D.LineCap.Round, System.Drawing.Drawing2D.LineCap.Round, System.Drawing.Drawing2D.DashCap.Round);
            eraser.SetLineCap(System.Drawing.Drawing2D.LineCap.Round, System.Drawing.Drawing2D.LineCap.Round, System.Drawing.Drawing2D.DashCap.Round);
            numericUpDown_Epaisseur.Value = pen_width;
            numericUpDown_Epaisseur.Increment = 3;


            #region COLORS OF COLOR GROUPBOX
            pictureBox_ColorActual.BackColor = pen.Color;
            pictureBox_Color1.BackColor = Color.White;
            pictureBox_Color2.BackColor = Color.Black;
            pictureBox_Color3.BackColor = Color.Blue;
            pictureBox_Color4.BackColor = Color.Green;
            pictureBox_Color5.BackColor = Color.Yellow;
            pictureBox_Color6.BackColor = Color.Red; 
            #endregion
        }

        private void drawPic_Load(object sender, EventArgs e)
        {
            // Changing the location of Left Control boxes
            groupBox_Outils.Location = new Point(groupBox_Outils.Location.X + 10, groupBox_Outils.Location.Y);
            groupBox_Formes.Location = new Point(groupBox_Formes.Location.X + 2, groupBox_Formes.Location.Y);
            groupBox_Toolbar.Location = new Point(groupBox_Toolbar.Location.X + 2, groupBox_Toolbar.Location.Y);
            // Changing the location of Right Control boxes
            int rightControlBoxes_X = groupBox_Outils.Width + pic.Width + 70;            

            this.Width = this.Width - 50;
            
            groupBox1.Enabled = false;
            groupBox1.Visible = false;
        }

        #region GROUPBOX: TOOLBAR

        private void btnUndo_Click(object sender, EventArgs e)
        {
            if (undoStack.Count > 0)
            {
                redoStack.Push((Bitmap)bm.Clone());
                bm = undoStack.Pop();
                g = Graphics.FromImage(bm);
                pic.Image = bm;
                pic.Invalidate();
            }
            else
            {
                MessageBox.Show("Nothing to Undo");
            }
        }

        private void btnRedo_Click(object sender, EventArgs e)
        {
            if (redoStack.Count > 0)
            {
                undoStack.Push((Bitmap)bm.Clone());
                bm = redoStack.Pop();
                g = Graphics.FromImage(bm);
                pic.Image = bm;
                pic.Invalidate();
            }
            else
            {
                MessageBox.Show("Nothing to Redo");
            }
        }
        #endregion        

        #region GROUPBOX : Outils

        private void btn_pencil_Click(object sender, EventArgs e)
        {
            state = DrawingState.Pen;
            numericUpDown_Epaisseur.Value = (decimal)pen.Width;
        }

        private void btn_eraser_Click(object sender, EventArgs e)
        {
            state = DrawingState.Erase;
            numericUpDown_Epaisseur.Value = (decimal)eraser.Width;
        }

        private void btn_clear_Click(object sender, EventArgs e)
        {
            allPoints = new List<Point>();
            allSpecialForms = new List<SpecialForm>();
            undoStack.Clear();
            redoStack.Clear();
            g.Clear(Color.White);
            pic.Invalidate();
        }
        private void btnFill_Click(object sender, EventArgs e)
        {
            state = DrawingState.Fill;
        }

        private void btn_RotateLeft_Click(object sender, EventArgs e)
        {            
            bm.RotateFlip(RotateFlipType.Rotate270FlipNone);
            pic.Image = bm;
        }

        private void btn_RotateRight_Click(object sender, EventArgs e)
        {
            bm.RotateFlip(RotateFlipType.Rotate90FlipNone);
            pic.Image = bm;
        }

        #endregion

        #region GROUPBOX : Formes

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            //pictureBox1.Focus 
            RefreshSpecialForm_With(pictureBox1.Image);
        }

        private void picLine_Click(object sender, EventArgs e)
        {
            state = DrawingState.Line;
            numericUpDown_Epaisseur.Value = (decimal)pen.Width;
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            RefreshSpecialForm_With(picTriangle.Image);
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            RefreshSpecialForm_With(pictureBox3.Image);
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            RefreshSpecialForm_With(pictureBox5.Image);
        }

        private void pictureBox12_Click(object sender, EventArgs e)
        {
            RefreshSpecialForm_With(pictureBox12.Image);
        }

        private void picEllipse_Click(object sender, EventArgs e)
        {
            state = DrawingState.Ellipse;
            numericUpDown_Epaisseur.Value = (decimal)pen.Width;
        }

        private void picRect_Click(object sender, EventArgs e)
        {
            state = DrawingState.Rect;
            numericUpDown_Epaisseur.Value = (decimal)pen.Width;
        }

        private void picTriangle_Click(object sender, EventArgs e)
        {
            state = DrawingState.Triangle;
            numericUpDown_Epaisseur.Value = (decimal)pen.Width;
        }
        #endregion

        #region GROUPBOX : Epaisseur

        private void numericUpDown_Epaisseur_ValueChanged(object sender, EventArgs e)
        {
            //Update pen width
            if (state == DrawingState.Pen)
            {
                pen_width = numericUpDown_Epaisseur.Value;
                pen.Width = (int)pen_width;
            }
            else //Update eraser width
            {
                eraser_width = numericUpDown_Epaisseur.Value;
                eraser.Width = (int)eraser_width;
            }
        }

        #endregion

        #region GROUPBOX : Couleurs

        private void button3_Click(object sender, EventArgs e)
        {
            DialogResult result = colorDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                pen.Color = colorDialog1.Color;
                ChangePenColor();
            }
        }

        private void pictureBox_Color1_Click(object sender, EventArgs e)
        {
            pen.Color = pictureBox_Color1.BackColor;
            ChangePenColor();
        }

        private void pictureBox_Color2_Click(object sender, EventArgs e)
        {
            pen.Color = pictureBox_Color2.BackColor;
            ChangePenColor();
        }

        private void pictureBox_Color3_Click(object sender, EventArgs e)
        {
            pen.Color = pictureBox_Color3.BackColor;
            ChangePenColor();
        }

        private void pictureBox_Color4_Click(object sender, EventArgs e)
        {
            pen.Color = pictureBox_Color4.BackColor;
            ChangePenColor();
        }

        private void pictureBox_Color5_Click(object sender, EventArgs e)
        {
            pen.Color = pictureBox_Color5.BackColor;
            ChangePenColor();
        }

        private void pictureBox_Color6_Click(object sender, EventArgs e)
        {
            pen.Color = pictureBox_Color6.BackColor;
            ChangePenColor();
        }

        #endregion

        #region GROUPBOX : Fichier

        private void btn_print_Click(object sender, EventArgs e)
        {            
            PrintSketch();
        }

        private void btn_save_Click(object sender, EventArgs e)
        {
            SaveSketch();
        }

        private void btn_exit_Click(object sender, EventArgs e)
        {
            
        }

        private void btn_close_Click(object sender, EventArgs e)
        {
            //TODO: Put the code inside a method and call it here
            if (!change)
            {                
                log.WriteToLogFile("disconnect");
                Dispose();
            }
            else
            {
                DialogResult exitresult = MessageBox.Show("Một bản vẽ đang được tiến hành! Bạn có muốn lưu nó không?", "Lưu? ", MessageBoxButtons.YesNo);

                if (exitresult == DialogResult.Yes) //Saving the skecth is user is ok
                {
                    SaveSketch();
                }

                Dispose();
            }
        }

        private void btnReplay_Click(object sender, EventArgs e)
        {
            int count = undoStack.Count;
            while (undoStack.Count > 0)
            {
                redoStack.Push((Bitmap)bm.Clone());
                bm = undoStack.Pop();
            }
            g = Graphics.FromImage(bm);
            pic.Image = bm;
            pic.Invalidate();

            for (int i = 0; i < count; ++i)
            {
                Application.DoEvents();
                Thread.Sleep(1000);
                undoStack.Push((Bitmap)bm.Clone());
                bm = redoStack.Pop(); ;
                g = Graphics.FromImage(bm);
                pic.Image = bm;
                pic.Invalidate();
            }
            MessageBox.Show("Done");
        }
        #endregion

        #region PIC

        private void pic_MouseHover(object sender, EventArgs e)
        {           
        }        

        private void pic_MouseDown(object sender, MouseEventArgs e)
        {
            if (state == DrawingState.SpecialForm)
            {
                g.DrawImage(Specialform_IMG, e.Location);

                //Add the draw point to the list of SpecialForm
                Specialform = new SpecialForm(Specialform_IMG, e.Location);
                allSpecialForms.Add(Specialform);

                pic.Refresh();
            }

            old = e.Location;
            drawing = true;
            change = true;

            undoStack.Push((Bitmap)bm.Clone());
            redoStack.Clear();
            
        }

        private void pic_MouseUp(object sender, MouseEventArgs e)
        {
            if (drawing)
            {
                if (state == DrawingState.Rect)
                {
                    g.DrawRectangle(pen, GetRect());
                }
                else if (state == DrawingState.Ellipse)
                {
                    g.DrawEllipse(pen, GetRect());
                }
                else if (state == DrawingState.Line)
                {
                    g.DrawLine(pen, old, current);
                }
                else if (state == DrawingState.Triangle)
                {
                    Rectangle shape = GetRect();
                    Point pointA = new Point(shape.X + shape.Width / 2, shape.Y);
                    Point pointB = new Point(shape.X, shape.Y + shape.Width);
                    Point pointC = new Point(shape.X + shape.Width, shape.Y + shape.Width);

                    // Draw triange
                    g.DrawLine(pen, pointA, pointB);
                    g.DrawLine(pen, pointC, pointB);
                    g.DrawLine(pen, pointA, pointC);
                }
                drawing = false;
            }            
        }

        private void pic_MouseMove(object sender, MouseEventArgs e)
        {
            if (drawing) {
                current = e.Location;
                if (state == DrawingState.Pen) //Pen enable
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        //Cursor.Current = Cursors.Cross;                        
                        g.DrawLine(pen, old, current);
                        old = current;

                        //Add the draw point to the list of Points
                        allPoints.Add(current);
                    }
                }

                //Eraser enable
                else if (state == DrawingState.Erase)
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        g.DrawLine(eraser, old, current);
                        old = current;

                        //Remove the eraser point from the list of points
                        allPoints.Remove(current);
                        //drawing = true;
                    }
                }

                pic.Refresh();
            }
            RefreshInformations();
        }

        private void pic_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            if (drawing)
            {
                if (state == DrawingState.Rect)
                {
                    g.DrawRectangle(pen, GetRect());
                }
                else if (state == DrawingState.Ellipse)
                {
                    g.DrawEllipse(pen, GetRect());
                }
                else if (state == DrawingState.Line)
                {
                    g.DrawLine(pen, old, current);
                }
                else if (state == DrawingState.Triangle)
                {
                    Rectangle shape = GetRect();
                    Point pointA = new Point(shape.X + shape.Width / 2, shape.Y);
                    Point pointB = new Point(shape.X, shape.Y + shape.Width);
                    Point pointC = new Point(shape.X + shape.Width, shape.Y + shape.Width);

                    // Draw triange
                    g.DrawLine(pen, pointA, pointB);
                    g.DrawLine(pen, pointC, pointB);
                    g.DrawLine(pen, pointA, pointC);
                }
            }
        }

        private void pic_MouseClick(object sender, MouseEventArgs e)
        {
            if (state == DrawingState.Fill)
            {
                Point p = SetPoint(pic, e.Location);
                Fill(bm, p.X, p.Y, pictureBox_ColorActual.BackColor);
                pic.Refresh();
            }
        }
        #endregion

        #region MENU

        private void fichierToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void enregistrerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveSketch();
        }

        private void annulerToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void quitterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (((allPoints.Count == 0) & (allSpecialForms.Count == 0)) || (drawing = false))
            {
                log.WriteToLogFile("disconnect");
                Dispose();
            }
            else
            {
                DialogResult exitresult = MessageBox.Show("Un dessin est en cours ! Voulez-vous l'enregistrer ?", "Enregistrer ? ", MessageBoxButtons.YesNo);

                if (exitresult == DialogResult.Yes) //Saving the skecth is user is ok
                {
                    SaveSketch();
                }

                log.WriteToLogFile("disconnect");;
                Dispose();
            }
        }

        private void dessinToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void nouveauToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void effacerToutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            allPoints = new List<Point>();
            allSpecialForms = new List<SpecialForm>();
            pic.Invalidate();
        }

        private void nouveauDessinToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        #endregion

        #region OTHERS CONTROLS

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
        
        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        #endregion

        #region METHODS

        /// <summary>
        /// Refresh the label witcch provide info about user action
        /// </summary>
        private void RefreshInformations()
        {
            if ((allPoints.Count == 0) & (allSpecialForms.Count == 0))
            {                
                label_Info.Text = "Papier vide !";
            }
            else
            {
                label_Info.Text = "Dessin en cours... ";
            }
        }

        private void RefreshSpecialForm_With(Image img)
        {
            state = DrawingState.SpecialForm;
            Specialform_IMG = img;
        }

        private void SaveSketch()
        {
            SaveFileDialog saveDialog = new SaveFileDialog();

            saveDialog.Filter = "Image (*.PNG)|*.PNG";

            try
            {
                //Here we let the user enter the name for the file
                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    bm.Save(saveDialog.FileName);
                    log.WriteToLogFile("pic_save", saveDialog.FileName);
                    change = false;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Không thể lưu bản vẽ trống!!");
            }
            
        }

        /// <summary>
        /// Open the windows used to draw a sketch
        /// </summary>
        private void OpenDrawWindows()
        {
            drawPic draw_Windows = new drawPic();
            draw_Windows.Show();
        }

        /// <summary>
        /// Make the user choose a picture and open ModifPic Windows
        /// </summary>
        private void OpenPicture()
        {
            OpenFileDialog OpenFile_dialog = new OpenFileDialog();
            OpenFile_dialog.Title = "Chọn hình";
            OpenFile_dialog.Multiselect = false;

            // Set filter options and filter index.
            OpenFile_dialog.Filter = "Images (*.BMP;*.JPG;*.PNG)|*.BMP;*.JPG;*.PNG";

            //Open the file dialog
            if (OpenFile_dialog.ShowDialog() == DialogResult.OK)
            {
                FileInfo choice_info = new FileInfo(OpenFile_dialog.FileName);
                                
                img = choice_info;
                picture_stream = img.OpenRead();
                Image pictureObj = Image.FromStream(picture_stream);
                bm = new Bitmap(pictureObj, pic.Width, pic.Height);
                g = Graphics.FromImage(bm);
                pic.Image = bm;
                this.Text = img.FullName;
            }
        }

        /// <summary>
        /// Open the log file
        /// </summary>
        private void OpenJournal()
        {
            log.openLogFile();
        }

        /// <summary>
        /// Close correctly the app
        /// </summary>
        private void DisconnectApp()
        {
            if (drawing)
            {
                DialogResult exitresult = MessageBox.Show("Một bản vẽ đang được tiến hành! Bạn có muốn lưu nó không?", "Lưu? ", MessageBoxButtons.YesNo);

                if (exitresult == DialogResult.Yes) //Saving the skecth is user is ok
                {
                    SaveSketch();                    
                }
            }

            log.WriteToLogFile("disconnect");;
            Dispose();
        }

        /// <summary>
        /// Print the sketch/drawing on the panel
        /// </summary>
        private void PrintSketch()
        {
            printPreviewDialog1 = new PrintPreviewDialog();

            printDialog1.Document = printDocument1;

            if (printDialog1.ShowDialog() == DialogResult.OK)
            {
                printDocument1.Print();
                log.WriteToLogFile("In", "Phác thato tạm thời chưa lưu");
            }
        }

        /// <summary>
        /// Change mouse icon to cross
        /// </summary>
        private void ChangeMouseIcon()
        {
            pic.Cursor = Cursors.Cross;
        }

        /// <summary>
        /// Change pen color 
        /// </summary>
        private void ChangePenColor()
        {
            pictureBox_ColorActual.BackColor = pen.Color;
        }

        /// <summary>
        /// Get Rectange
        /// </summary>
        private Rectangle GetRect()
        {
            Rectangle shape = new Rectangle();
            shape.X = Math.Min(old.X, current.X);
            shape.Y = Math.Min(old.Y, current.Y);
            shape.Width = Math.Abs(old.X - current.X);
            shape.Height = Math.Abs(old.Y - current.Y);
            return shape;
        }

        /// <summary>
        /// Set the point
        /// </summary>
        /// <param name="pb"></param>
        /// <param name="pt"></param>
        /// <returns></returns>
        private Point SetPoint(PictureBox pb, Point pt)
        {
            float pX = 1f * pb.Image.Width / pb.Width;
            float pY = 1f * pb.Image.Height / pb.Height;
            return new Point((int)(pt.X * pX), (int)(pt.Y * pY));
        }

        /// <summary>
        /// Check position that is can change color
        /// </summary>
        /// <param name="bm"></param>
        /// <param name="sp"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="oldColor"></param>
        /// <param name="newColor"></param>
        private void validate(Bitmap bm, Stack<Point> sp, int x, int y, Color oldColor, Color newColor)
        {
            Color cx = bm.GetPixel(x, y);
            if (cx == oldColor)
            {
                Point p = new Point(x, y);
                sp.Push(p);
                bm.SetPixel(x, y, newColor);
            }
        }
        /// <summary>
        /// Fill color with the where location is
        /// </summary>
        /// <param name="bm"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="newColor"></param>
        private void Fill(Bitmap bm, int x, int y, Color newColor)
        {
            Color oldColor = bm.GetPixel(x, y);
            Stack<Point> pixel = new Stack<Point>();
            pixel.Push(new Point(x, y));
            bm.SetPixel(x, y, newColor);

            if (oldColor == newColor)
                return;

            while (pixel.Count > 0)
            {
                Point pt = (Point)pixel.Pop();
                if (pt.X > 0 && pt.Y > 0
                    && pt.X < bm.Width - 1 && pt.Y < bm.Height - 1)
                {
                    validate(bm, pixel, pt.X - 1, pt.Y, oldColor, newColor);
                    validate(bm, pixel, pt.X + 1, pt.Y, oldColor, newColor);
                    validate(bm, pixel, pt.X, pt.Y - 1, oldColor, newColor);
                    validate(bm, pixel, pt.X, pt.Y + 1, oldColor, newColor);
                }
            }
        }

        #endregion

        #region MENU

        private void ouvrirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenPicture();
        }

        private void imprimerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PrintSketch();
        }

        private void printDocument1_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            Bitmap bitm = new Bitmap(pic.Width, pic.Height);
            try
            {
                //Draw all point to Graphics
                e.Graphics.DrawLines(pen, allPoints.ToArray());

                //Drawing Images to Graphics
                foreach (var item in allSpecialForms)
                {
                    e.Graphics.DrawImage(item._Image, item._Point);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể in một bản vẽ trống");
            }

            e.Graphics.DrawImage(bitm, 0, 0);
            bitm.Dispose();
        }

        #endregion
    }
}
