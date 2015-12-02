using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tao.OpenGl;
using Tao.Platform.Windows;
using Tao.FreeGlut;
using Tao.DevIl;
using Tao.OpenAl;

namespace KG_Lab5
{
    public partial class Form1 : Form
    {
        // ряд вспомогательных переменных // поворот
        private int rot = 0;
        // флаг - загружена ли текстура
        private bool textureIsLoad = false;
        // имя текстуры
        public string texture_name = ""; // идентификатор текстуры
        public int imageId = 0;
        // текстурный объект
        public uint mGlTextureObject = 0;
        float McoordX = 0, McoordY = 0; float MlastX = 0, MlastY = 0; bool mouseButton = false; float rotateX;
        float rotateY;

        public Form1()
        {
            InitializeComponent();
            AnT.InitializeContexts();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            const float ZNEAR = 0.5f;
            const float ZFAR = 10;
            const float FIELD_OF_VIEW = 45;
            float aspect = (float)AnT.Width / (float)AnT.Height;

            Glut.glutInit();
            Glut.glutInitDisplayMode(Glut.GLUT_RGB | Glut.GLUT_DOUBLE);

            Il.ilInit();
            Il.ilEnable(Il.IL_ORIGIN_SET);

            Gl.glClearColor(255, 255, 255, 1);
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);

            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();
            Glu.gluPerspective(FIELD_OF_VIEW, aspect, ZNEAR, ZFAR);//настройка области отображения

            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glLoadIdentity();

            Glu.gluLookAt(
                            0, 0, 3, //Spectator view
                            0, 0, 0, //Point, where camera is directed
                            0, 1, 0); // Vector, for identifing vector up
            DrawCube();
            RenderTimer.Start();
        }

        private void RenderTimer_Tick(object sender, EventArgs e)
        {
            //DrawCube();
        }
        private void DrawCube()
        {
            int faceCount = 6;
            float ScaleKof = 0.5f;

            float[][] vertices;
            vertices = new float[8][]
                {
                    new float [] { -1,-1,-1},
                    new float [] { 1,-1,-1},
                    new float [] { 1,1,-1},
                    new float [] { -1,1,-1},
                    new float [] { -1,-1,1},
                    new float [] { 1,-1,1},
                    new float [] { 1,1,1},
                    new float [] { -1,1,1},
                };

            int[,] faces;
            faces = new int[,]
                {
                    { 4,7,3,0},
                    { 5,1,2,6},
                    { 4,0,1,5},
                    { 7,6,2,3},
                    { 0,3,2,1},
                    { 4,5,6,7}
                };

            //Array of sides colours
            byte[][] faceColors;
            faceColors = new byte[6][]
                {
                    new byte [] { 127,0,0,0},
                    new byte [] {0,127,0,0 },
                    new byte [] {0,0,127,0 },
                    new byte [] {127,127,0,0 },
                    new byte [] {0,127,127,0 },
                    new byte [] {127,0,127,0 }
                };
            Gl.glClearColor(255, 255, 255, 0);
            

            if (!textureIsLoad)
            {
                Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
                // включение режима отбраковки невидимых граней
                Gl.glEnable(Gl.GL_CULL_FACE); Gl.glCullFace(Gl.GL_BACK); Gl.glFrontFace(Gl.GL_CCW); Gl.glEnable(Gl.GL_DEPTH_TEST);
                Gl.glPushMatrix();
                // поворот объекта
                Gl.glRotatef(rotateX, 1, 0, 0); Gl.glRotatef(rotateY, 0, 1, 0);
                // масштабирование
                Gl.glScalef(ScaleKof, ScaleKof, ScaleKof); // цвет очиcтки окна
                Gl.glClearColor(255, 255, 255, 0);
                Gl.glBegin(Gl.GL_QUADS);
                for (int face = 0; face < faceCount; ++face)
                {
                    Gl.glColor4bv(faceColors[face]); for (int i = 0; i < 4; ++i)
                    {
                        int vertexIndex = faces[face, i];
                        Gl.glVertex3fv(vertices[vertexIndex]);
                    }
                }
                Gl.glEnd(); Gl.glPopMatrix();
            }
            // завершаем рисование
            Gl.glFlush();
            AnT.Invalidate();
        }


        private void AnT_MouseMove(object sender, MouseEventArgs e)
        {
            float dx = e.X - McoordX;
            float dy = e.Y - McoordY;

            if (mouseButton)
            {
                rotateX = (dy * 180) / AnT.Height + MlastX;
                rotateY = (dx * 180) / AnT.Width + MlastY;

                DrawCube();
               AnT.Invalidate();
            }
        }

        private void AnT_MouseDown(object sender, MouseEventArgs e)
        {
            mouseButton = true;
            McoordX = e.X;
            McoordY = e.Y;
        }

        private void AnT_MouseUp(object sender, MouseEventArgs e)
        {
            mouseButton = false;
            MlastX = rotateX;
            MlastY = rotateY;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // открываем окно выбора файла
            DialogResult res = openFileDialog1.ShowDialog(); // если файл выбран - и возвращен результат OK
            if (res == DialogResult.OK)
            {
              
                // создаем изображение с идентификатором imageId
                Il.ilGenImages(1, out imageId); // делаем изображение текущим 
                Il.ilBindImage(imageId);
                                                // адрес изображения полученный с помощью окна выбора файла
                string url = openFileDialog1.FileName; // пробуем загрузить изображение
                if (Il.ilLoadImage(url))
                {
                    // если загрузка прошла успешно
                    // сохраняем размеры изображения
                    int width = Il.ilGetInteger(Il.IL_IMAGE_WIDTH);
                    int height = Il.ilGetInteger(Il.IL_IMAGE_HEIGHT);
                    // определяем число бит на пиксель
                    int bitspp = Il.ilGetInteger(Il.IL_IMAGE_BITS_PER_PIXEL);
                    switch (bitspp) // в зависимости от полученного результата 
                    {
// создаем текстуру используя режим GL_RGB или GL_RGBA
                        case 24:
                        mGlTextureObject = MakeGlTexture(Gl.GL_RGB, Il.ilGetData(), width, height);
                         break;
                    case 32:
                        mGlTextureObject = MakeGlTexture(Gl.GL_RGBA, Il.ilGetData(), width, height);
                         break;
                    }
                    // активируем флаг, сигнализирующий загрузку текстуры
                    textureIsLoad = true;
                    // очищаем память 
                    Il.ilDeleteImages(1, ref imageId);
                }
            }
        }

        private static uint MakeGlTexture(int Format, IntPtr pixels, int w, int h)
        {
            // идентификатор текстурного объекта
            uint texObject;
            // генерируем текстурный объект
            Gl.glGenTextures(1, out texObject);
            // устанавливаем режим упаковки пикселей
            Gl.glPixelStorei(Gl.GL_UNPACK_ALIGNMENT, 1);
            // создаем привязку к только что созданной текстуре
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, texObject);
            // устанавливаем режим фильтрации и повторения текстуры
            Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_S, Gl.GL_REPEAT); Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_T, Gl.GL_REPEAT); Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR); Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR); Gl.glTexEnvf(Gl.GL_TEXTURE_ENV, Gl.GL_TEXTURE_ENV_MODE, Gl.GL_REPLACE);
            // создаем RGB или RGBA текстуру
            switch (Format)
            {
                case Gl.GL_RGB:
                    Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, Gl.GL_RGB, w, h, 0, Gl.GL_RGB,
                    Gl.GL_UNSIGNED_BYTE, pixels); break;
                case Gl.GL_RGBA:
                    Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, Gl.GL_RGBA, w, h, 0, Gl.GL_RGBA,
                    Gl.GL_UNSIGNED_BYTE, pixels); break;
            }
            // возвращаем идентификатор текстурного объекта
            return texObject;
    }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult rsl = MessageBox.Show("Are you shure?", "Exit", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (rsl == DialogResult.Yes)
                Application.Exit();
        }


        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void DrawTexture()
        {
            if (textureIsLoad)
            {
                Gl.glDisable(Gl.GL_CULL_FACE);

                rot++;

                if (rot > 360)
                    rot = 0;

                Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
                Gl.glClearColor(255, 255, 255, 1);

                Gl.glLoadIdentity();

                Gl.glEnable(Gl.GL_TEXTURE_2D);
                Gl.glBindTexture(Gl.GL_TEXTURE_2D, mGlTextureObject);

                Gl.glPushMatrix();

                Gl.glTranslated(0, -1, -5);

                Gl.glRotated(rot, 0, 1, 0);
                Gl.glBegin(Gl.GL_QUADS);

                Gl.glTexCoord2f(0, 0);
                Gl.glVertex3d(0, 0, 0);
                Gl.glTexCoord2f(0, 1);
                Gl.glVertex3d(0, 1, 0);
                Gl.glTexCoord2f(1, 1);
                Gl.glVertex3d(1, 1, 0);
                Gl.glTexCoord2f(1, 0);
                Gl.glVertex3d(1, 0, 0);

                Gl.glEnd();

                Gl.glBegin(Gl.GL_TRIANGLES);
                Gl.glTexCoord2f(1, 0);
                Gl.glVertex3d(1, 1, 0);
                Gl.glTexCoord2f(0.5f, 1);
                Gl.glVertex3d(0.5, 2, 0);
                Gl.glTexCoord2f(0, 0);
                Gl.glVertex3d(0, 1, 0);
                Gl.glEnd();

                Gl.glPopMatrix();
                Gl.glDisable(Gl.GL_TEXTURE_2D);

                AnT.Invalidate();
                    
            }
        }

    }
}
