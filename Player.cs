using SFML.Graphics;
using SFML.Window;
using System;
using System.Net.Sockets;
using System.Text;
using System.Timers;

namespace LatoClient
{
    internal class Player
    {
        public int i;

        static IntRect rect = new IntRect(0, 0, 48, 72);
        static Texture texture = new Texture(@"..\..\..\Player.png", rect);
        public Sprite sprite = new Sprite(texture);

        int s = 0;
        public int stato = 0, dir;
        Timer change = new Timer(250);
        public int cube = 0, chunk = 0, pixel = 0;
        int cu = 0, ch = 0, pi = 0;
        public Keyboard.Key k;
        public Player()
        {
            sprite.TextureRect = new IntRect(0, 0, 12, 18);
            sprite.Scale *= 2;

            change.Start();
            change.Elapsed += Change_Elapsed;
        }

        private void Change_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (k == Keyboard.Key.W || k == Keyboard.Key.A || k == Keyboard.Key.S || k == Keyboard.Key.D)
                if (s == 3)
                    s = 0;
                else
                    s++;
        }

        public void Move(Keyboard.Key k)
        {
            this.k = k;

            switch (k)
            {
                case Keyboard.Key.W:
                    changepixel(-2);
                    break;
                case Keyboard.Key.A:
                    changepixel(-20);
                    break;
                case Keyboard.Key.S:
                    changepixel(2);
                    break;
                case Keyboard.Key.D:
                    changepixel(20);
                    break;
            }

            string str = "plr" + i + (pi < 10 ? "0" + pi : pi) + (ch < 10 ? "0" + ch : ch) + (cu < 10 ? "0" + cu : cu) + s + (k == Keyboard.Key.W ? 0 :
                                                                                                                              k == Keyboard.Key.A ? 1 :
                                                                                                                              k == Keyboard.Key.S ? 2 :
                                                                                                                              k == Keyboard.Key.D ? 3 : 4);
            Console.WriteLine(str);
            byte[] data = Encoding.UTF8.GetBytes(str);

            int offset = 0;
            while (offset < data.Length)
            {
                Gioco.stream.Write(data, offset, Math.Min(data.Length - offset, 1024));
                offset += 1024;
            }

            void changepixel(int value)
            {
                if (value == -2)
                {
                    if (pixel % 10 > 1)
                        pi = pixel - 2;
                    else
                    {
                        pi = pixel + 7;
                        changechunk(-1);
                    }
                }
                else if (value == -20)
                {
                    if (pixel / 10 > 1)
                        pi = pixel - 20;
                    else
                    {
                        pi = pixel + 70;
                        changechunk(-10);
                    }
                }
                else if (value == 2)
                {
                    if (pixel % 10 < 7)
                        pi = pixel + 2;
                    else
                    {
                        pi = pixel - 7;
                        changechunk(+1);
                    }
                }
                else if (value == 20)
                {
                    if (pixel / 10 < 7)
                        pi = pixel + 20;
                    else
                    {
                        pi = pixel - 70;
                        changechunk(10);
                    }
                }
            }
            void changechunk(int value)
            {
                if (value == -1)
                {
                    if (chunk % 10 != 0)
                        ch = chunk - 1;
                    else
                    {
                        ch = chunk + 8;
                        changecube(-1);
                    }
                }
                else if (value == -10)
                {
                    if (chunk / 10 != 0)
                        ch = chunk - 10;
                    else
                    {
                        ch = chunk + 80;
                        changecube(-10);
                    }
                }
                else if (value == 1)
                {
                    if (chunk % 10 != 8)
                        ch = chunk + 1;
                    else
                    {
                        ch = chunk - 8;
                        changecube(1);
                    }
                }
                else if (value == 10)
                {
                    if (chunk / 10 != 8)
                        ch = chunk + 10;
                    else
                    {
                        ch = chunk - 80;
                        changecube(10);
                    }
                }
            }
            void changecube(int value)
            {
                if (value == -1)
                {
                    if (cube % 10 != 0)
                        cu = cube - 1;
                }
                else if (value == -10)
                {
                    if (cube / 10 != 0)
                        cu = cube - 10;
                }
                else if (value == 1)
                {
                    if (cube % 10 != 6)
                        cu = cube + 1;
                }
                else if (value == 10)
                {
                    if (cube / 10 != 6)
                        cu = cube + 10;
                }
            }
        }
    }
}
