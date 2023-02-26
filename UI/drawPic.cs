using Esgis_Paint.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Esgis_Paint
{
    public partial class drawPic : Form
    {
        #region Variables

        Journal log;
        List<Point> allPoints = new List<Point>();
        public Point current = new Point();
        public Point old = new Point();
        public Graphics g;
        public Pen pen;
        public Pen eraser;

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
        #endregion
        
        public drawPic()
        {
            InitializeComponent();
            Bitmap bm = new Bitmap(pic.Width, pic.Height);
            g = Graphics.FromImage(bm);
            g.Clear(Color.White);
            pic.Image = bm;

            //Initialisations
            state = DrawingState.Pen;
            drawing = false;

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

            // Changing the location of Right Control boxes
            int rightControlBoxes_X = groupBox_Outils.Width + pic.Width + 70;            
           
            this.Width = this.Width - 50;

            btn_ZoomIn.Enabled = false;
            btn_ZoomOut.Enabled = false;
            groupBox1.Enabled = false;
            groupBox1.Visible = false;
        }

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
            g.Clear(Color.White);
            pic.Invalidate();
        }

        #endregion

        #region GROUPBOX : Formes

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            //pictureBox1.Focus 
            RefreshSpecialForm_With(pictureBox1.Image);
        }

        private void pictureBox6_Click(object sender, EventArgs e)
        {
            RefreshSpecialForm_With(pictureBox6.Image);
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            RefreshSpecialForm_With(pictureBox2.Image);
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            RefreshSpecialForm_With(pictureBox3.Image);
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            RefreshSpecialForm_With(pictureBox5.Image);
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            RefreshSpecialForm_With(pictureBox4.Image);
        }

        private void pictureBox12_Click(object sender, EventArgs e)
        {
            RefreshSpecialForm_With(pictureBox12.Image);
        }

        private void pictureBox7_Click(object sender, EventArgs e)
        {
            RefreshSpecialForm_With(pictureBox7.Image);
        }

        private void pictureBox11_Click(object sender, EventArgs e)
        {
            RefreshSpecialForm_With(pictureBox11.Image);
        }

        private void pictureBox10_Click(object sender, EventArgs e)
        {
            RefreshSpecialForm_With(pictureBox10.Image);
        }

        private void pictureBox8_Click(object sender, EventArgs e)
        {
            RefreshSpecialForm_With(pictureBox8.Image);
        }

        private void pictureBox9_Click(object sender, EventArgs e)
        {
            RefreshSpecialForm_With(pictureBox9.Image);
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
            if (((allPoints.Count == 0) & (allSpecialForms.Count == 0)) || (drawing = false))
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

        #endregion

        #region PIC

        private void pic_MouseHover(object sender, EventArgs e)
        {           
        }        

        private void pic_MouseDown(object sender, MouseEventArgs e)
        {
            old = e.Location;
            drawing = true;
            if (state == DrawingState.SpecialForm)
            {
                g.DrawImage(Specialform_IMG, e.Location);
                Specialform = new SpecialForm(Specialform_IMG, e.Location);                
            }            
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
                    // Haven't done anything yet
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

                // SpecialForm enable
                else if (state == DrawingState.SpecialForm)
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        g.DrawImage(Specialform_IMG, current);

                        //Add the draw point to the list of SpecialForm
                        Specialform = new SpecialForm(Specialform_IMG, e.Location);
                        allSpecialForms.Add(Specialform);
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
                    // Haven't done anything yet
                }
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
            Bitmap bitm = new Bitmap(pic.Width, pic.Height);
            Graphics bitGraphics = Graphics.FromImage(bitm);

            saveDialog.Filter = "Image (*.PNG)|*.PNG";

            try
            {
                //Draw all point to Graphics
                bitGraphics.DrawLines(pen, allPoints.ToArray());

                //Drawing Images to Graphics
                foreach (var item in allSpecialForms)
                {
                    bitGraphics.DrawImage(item._Image, item._Point);
                }

                //Here we let the user enter the name for the file
                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    bitm.Save(saveDialog.FileName);
                    log.WriteToLogFile("pic_save", saveDialog.FileName);
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

                //Calling modifyPic and sending to it the picture info
                editPic modifPage = new editPic();
                modifPage.getImage(choice_info);
                modifPage.Show();
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
