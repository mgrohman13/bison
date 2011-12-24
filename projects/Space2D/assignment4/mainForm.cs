using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using D3D = Microsoft.DirectX.Direct3D;
using DInput = Microsoft.DirectX.DirectInput;
using dSound = Microsoft.DirectX.DirectSound;

namespace assignment4
{
    public partial class mainForm : Form
    {
        public static D3D.Device device;
        public static DInput.Device MyKeyboard;

        D3D.Font f21, f13, f30;

        D3D.Sprite sprite;

        bool quit = false, pPressed = false;

        public static bool paused = false;

        float num = 0, total = 0;

        public const long FrameRate = 20;

        public static KeyboardState state;

        public mainForm(ProgressBar pb)
        {
            InitializeComponent();

            // Set the application to full screen mode
            this.FormBorderStyle = FormBorderStyle.None;
            Bounds = Screen.PrimaryScreen.Bounds;
            //WindowState = FormWindowState.Maximized;

            Data.setWidthHeight(Width, Height);

            InitializeGraphics();
            Data.loadTextures(pb);

            f30 = new D3D.Font(device, new System.Drawing.Font("Arial", 21));
            f21 = new D3D.Font(device, new System.Drawing.Font("Arial", 21));
            f13 = new D3D.Font(device, new System.Drawing.Font("Arial", 13));
        }

        private void InitializeGraphics()
        {
            MyKeyboard = new DInput.Device(SystemGuid.Keyboard);
            MyKeyboard.SetCooperativeLevel(this, CooperativeLevelFlags.Background | CooperativeLevelFlags.NonExclusive);

            PresentParameters presentParams = new PresentParameters();
            presentParams.Windowed = true;
            presentParams.SwapEffect = SwapEffect.Discard;

            // Create our device
            device = new D3D.Device(0, D3D.DeviceType.Hardware, this, CreateFlags.SoftwareVertexProcessing, presentParams);

            sprite = new Sprite(device);

            MyKeyboard.Acquire();
        }

        public void RunGame()
        {
            Cursor.Hide();

            Data.StartNewGame();

            //used to time framerate
            System.Diagnostics.Stopwatch MyWatch = new System.Diagnostics.Stopwatch();
            while (!quit)
            {
                MyWatch.Reset();
                MyWatch.Start();

                state = MyKeyboard.GetCurrentKeyboardState();
                if (Data.player.Lives < 0 || paused) //if the game is over
                    if (state[Key.Escape])
                        quit = true; //end the program
                    else if (state[Key.Return])
                        Data.StartNewGame(); //start a new game

                if (Data.player.Lives > -1 && !paused)
                    Data.Inc();
                Render();

                //press p to pause
                if (pPressed != state[Key.P])
                {
                    if (pPressed)
                        paused = !paused;
                    pPressed = state[Key.P];
                }

                //caulculates framerate
                if (num > 1300)
                {
                    float meh = total / num;
                    num = 0;
                    total = 0;
                }
                ++num;
                total += MyWatch.ElapsedMilliseconds;

                //wait for framerate
                long timeDiff = FrameRate - MyWatch.ElapsedMilliseconds;
                if (timeDiff > 0)
                {
                    System.Threading.Thread.Sleep((int)timeDiff);
                }

                MyWatch.Stop();
            }

            //clear unmanaged resources
            Data.disposeTextures();
            MyKeyboard.Unacquire();
            MyKeyboard.Dispose();
            f21.Dispose();
            f13.Dispose();
            f30.Dispose();
            sprite.Dispose();
            device.Dispose();

            this.Close();
            this.Dispose();
        }

        private void Render()
        {
            device.Clear(ClearFlags.Target, Color.Black, 1f, 0);

            device.BeginScene();
            sprite.Begin(SpriteFlags.AlphaBlend);

            Data.Render(sprite);

            //get color based on health percentage
            int color = Data.Random.Round(512f * Data.player.Hits / player.MaxHits);
            int green = color;
            if (green > 255)
                green = 255;
            if (green < 0)
                green = 0;
            int red = 512 - color;
            if (red > 255)
                red = 255;
            if (red < 0)
                red = 0;
            f21.DrawText(sprite, string.Format("{0}", Data.player.Hits),
                new Point(13, Height - 39), Color.FromArgb(red, green, 0));

            //draw number of lives
            sprite.Draw(powerUp.life.texture, new Rectangle(0, 0, 0, 0), new Vector3(0,
                0, 1), new Vector3(13, Height - 100, 1), Color.White);
            f21.DrawText(sprite, string.Format("{0}", Data.player.Lives),
                new Point(70, Height - 93), Color.Yellow);

            //draw counts
            f13.DrawText(sprite, string.Format("Ships: {0}", enemies.enemy.all.Count),
                new Point(13, 13), Color.White);
            f13.DrawText(sprite, string.Format("Bullets: {0}", weapons.weapon.all.Count),
                new Point(13, 29), Color.White);
            f13.DrawText(sprite, string.Format("Power Ups: {0}", powerUp.powerUp.all.Count),
               new Point(13, 45), Color.White);

            //draw score
            f21.DrawText(sprite, string.Format("{0}", Data.score),
                new Point(Width / 2 - 13, 13), Color.Pink);

            if (Data.player.Lives < 0)
            {
                //game over stuff
                f30.DrawText(sprite, "Game over!", new Point(Width / 2 - 60, Height / 2 - 130), Color.Cyan);
                f30.DrawText(sprite, "Score: " + Data.score.ToString(), new Point(Width / 2 - 60, Height / 2 - 60), Color.Cyan);
            }
            else if (paused)
                f30.DrawText(sprite, "Paused", new Point(Width / 2 - 39, Height / 2 - 13), Color.Yellow);

            //draw collected power ups in bottom right
            int yMax = int.MinValue;
            int y = Height - 13, x = Width - 13;
            foreach (powerUp.powerUp pUp in powerUp.powerUp.all)
                if (pUp.Collected)
                {
                    pUp.Draw(sprite, x, y);

                    yMax = Math.Max(yMax, pUp.Size.Height);
                    x -= 13 + pUp.Size.Width;

                    if (x < Width - 210)
                    {
                        x = Width - 13;
                        y -= 13 + yMax;
                        yMax = int.MinValue;
                    }
                }


            sprite.End();
            device.EndScene();
            try
            {
                device.Present();
            }
            catch
            {
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            RunGame();
        }
    }
}