using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace LatoClient
{
    internal class Gioco
    {
        public static NetworkStream stream;

        public static RenderWindow finestra;

        static Player player = new Player();
        static Keyboard.Key k = Keyboard.Key.Numpad0;
        static System.Timers.Timer move = new(20);

        static Npc[] npc;

        static Casa[,] mappa = null;
        public static void gioco()
        {
            TcpClient client = new TcpClient();
            client.Connect("127.0.0.1", 8888);

            stream = client.GetStream();

            new Thread(Ricezione).Start();

            while (mappa == null) { }
            Thread.Sleep(500);

            move.Start();
            move.Elapsed += Move_Elapsed;

            finestra.KeyPressed += Finestra_KeyPressed;
            finestra.KeyReleased += Finestra_KeyReleased;
            while (finestra.IsOpen && Program.fase == 1)
            {
                finestra.Clear();
                Disegna();
                finestra.DispatchEvents();
                finestra.Display();
            }
        }

        private static void Move_Elapsed(object sender, ElapsedEventArgs e)
        {
            player.Move(k == Keyboard.Key.W ? 0 : 
                        k == Keyboard.Key.A ? 1 : 
                        k == Keyboard.Key.S ? 2 : 
                        k == Keyboard.Key.D ? 3 : 4);
        }

        private static void Finestra_KeyReleased(object sender, KeyEventArgs e)
        {
            if (e.Code == k)
                k = Keyboard.Key.Numpad0;
        }

        private static void Finestra_KeyPressed(object sender, KeyEventArgs e)
        {
            if (e.Code == Keyboard.Key.W) k = Keyboard.Key.W;
            else if (e.Code == Keyboard.Key.S) k = Keyboard.Key.S;
            else if (e.Code == Keyboard.Key.D) k = Keyboard.Key.D;
            else if (e.Code == Keyboard.Key.A) k = Keyboard.Key.A;
        }

        static void Disegna()
        {
            finestra.Clear();
            for (int y = 0; y < mappa.GetLength(1); y++)
            {
                for (int x = 0; x < mappa.GetLength(0); x++)
                {
                    mappa[x, y].sprite.Position = new Vector2f(x, y) * 81 * mappa[x, y].sprite.Scale.X;
                    finestra.Draw(mappa[x, y].sprite);
                }
            }

            if (player.dir == 0)
                player.sprite.TextureRect = new IntRect(player.stato * 12, 54, 12, 18);
            if (player.dir == 1)
                player.sprite.TextureRect = new IntRect(player.stato * 12, 18, 12, 18);
            if (player.dir == 2)
                player.sprite.TextureRect = new IntRect(player.stato * 12, 0, 12, 18);
            if (player.dir == 3)
                player.sprite.TextureRect = new IntRect(player.stato * 12, 36, 12, 18);

            player.sprite.Position = new Vector2f(player.cube / 10 * 81 + player.chunk / 10 * 9 + player.pixel / 10,
                                                  player.cube % 10 * 81 + player.chunk % 10 * 9 + player.pixel % 10);
            finestra.Draw(player.sprite);

            if (npc != null)
                if (npc.Length > 0)
                    foreach (Npc n in npc)
                    {
                        if (n.dir == 0)
                            n.sprite.TextureRect = new IntRect(n.stato * 12, 54, 12, 18);
                        if (n.dir == 1)
                            n.sprite.TextureRect = new IntRect(n.stato * 12, 18, 12, 18);
                        if (n.dir == 2)
                            n.sprite.TextureRect = new IntRect(n.stato * 12, 0, 12, 18);
                        if (n.dir == 3)
                            n.sprite.TextureRect = new IntRect(n.stato * 12, 36, 12, 18);

                        n.sprite.Position = new Vector2f(n.cube / 10 * 81 + n.chunk / 10 * 9 + n.pixel / 10,
                                                         n.cube % 10 * 81 + n.chunk % 10 * 9 + n.pixel % 10);
                        finestra.Draw(n.sprite);
                    }
        }

        static void Ricezione()
        {
            Dictionary<string, Func<Thread>> commands = new Dictionary<string, Func<Thread>>()
            {
                { "plr", () => new Thread(new ParameterizedThreadStart(plr)) },
                { "map", () => new Thread(new ParameterizedThreadStart(map)) },
                { "num", () => new Thread(new ParameterizedThreadStart(num)) },
                { "ngt", () => new Thread(new ParameterizedThreadStart(ngt)) },
            };

            while (true)
            {
                byte[] lengthPrefix = new byte[4];
                stream.Read(lengthPrefix, 0, 4);
                int messageLength = BitConverter.ToInt32(lengthPrefix, 0);

                byte[] data = new byte[messageLength];
                int n = stream.Read(data, 0, messageLength);
                string message = Encoding.UTF8.GetString(data);

                if (n > 0)
                {
                    string code = message[..3];
                    message = message[3..];

                    if (commands.ContainsKey(code))
                    {
                        Thread thread = commands[code]();
                        thread.Start(message);
                    }
                }
            }
        }

        public static void Write(string message)
        {
            byte[] str = Encoding.UTF8.GetBytes(message);
            byte[] lengthPrefix = BitConverter.GetBytes(str.Length);
            stream.Write(lengthPrefix, 0, lengthPrefix.Length);
            stream.Write(str, 0, str.Length);
            stream.Flush();
        }//invia il messaggio al server
        
        static void plr(object message)
        {
            string msg = message.ToString();
            int i = Convert.ToInt16(msg[0].ToString());

            if (i == player.i)
            {
                player.pixel = Convert.ToInt16(msg[1..3]);
                player.chunk = Convert.ToInt16(msg[3..5]);
                player.cube = Convert.ToInt16(msg[5..7]);
                player.stato = Convert.ToInt16(msg[7]);
                player.dir = Convert.ToInt16(msg[8]);
            }
            else
            {
                throw new Exception("numero errato");
            }
        }
        static void map(object message)
        {
            string msg = message.ToString();
            mappa = new Casa[7, 7];
            for (int y = 0; y < 7; y++)
                for (int x = 0; x < 7; x++)
                {
                    int i = (y * 7 + x) * 2;
                    mappa[x, y] = Convert.ToInt16(msg[i..(i + 2)]);
                }

            Write("confmap");
        }
        static void num(object message)
        {
            string msg = message.ToString();
            player.i = Convert.ToInt32(msg[0].ToString());

            Write("confnum");
        }
        static void ngt(object message)
        {
            string msg = message.ToString();
            npc = new Npc[Convert.ToInt16(msg[0].ToString()) - 1];
            for (int i = 0; i < npc.Length; i++)
                npc[i] = new Npc();

            Write("confngt");
        }
    }
}
