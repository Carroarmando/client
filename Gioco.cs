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

		static Casa[,] mappa;
		public static void gioco()
		{
			TcpClient client = new TcpClient();
			client.Connect("127.0.0.1", 8888);

			stream = client.GetStream();

            {
                byte[] lengthPrefix = new byte[4];
                stream.Read(lengthPrefix, 0, 4);
                int messageLength = BitConverter.ToInt32(lengthPrefix, 0);

                byte[] data = new byte[messageLength];
                stream.Read(data, 0, messageLength);

                string message = Encoding.UTF8.GetString(data);
                if (message.StartsWith("map"))
                {
                    message = message.Substring(3);
                    mappa = new Casa[7, 7];
                    for (int y = 0; y < 7; y++)
                        for (int x = 0; x < 7; x++)
                        {
                            int i = y * 7 + x;
                            mappa[x, y] = Convert.ToInt16(message[i..(i + 2)]);
                        }

                    byte[] str = Encoding.UTF8.GetBytes("confmap");
                    lengthPrefix = BitConverter.GetBytes(str.Length);
                    stream.Write(lengthPrefix, 0, lengthPrefix.Length);
                    stream.Write(str, 0, str.Length);
                    stream.Flush();
                }
                else
                    throw new Exception(message);
            }//attesa e ricezione della mappa
            {
                byte[] lengthPrefix = new byte[4];
                stream.Read(lengthPrefix, 0, 4);
                int messageLength = BitConverter.ToInt32(lengthPrefix, 0);

                byte[] data = new byte[messageLength];
                stream.Read(data, 0, messageLength);

                string message = Encoding.UTF8.GetString(data);
                if (message.StartsWith("num"))
                {
                    player.i = Convert.ToInt32(message[3].ToString());

                    byte[] str = Encoding.UTF8.GetBytes("confnum");
                    lengthPrefix = BitConverter.GetBytes(str.Length);
                    stream.Write(lengthPrefix, 0, lengthPrefix.Length);
                    stream.Write(str, 0, str.Length);
                    stream.Flush();
                }
                else
                    throw new Exception(message);
            }//attesa e ricezione del numero del giocatore
            {
                byte[] lengthPrefix = new byte[4];
                stream.Read(lengthPrefix, 0, 4);
                int messageLength = BitConverter.ToInt32(lengthPrefix, 0);

                byte[] data = new byte[messageLength];
                stream.Read(data, 0, messageLength);

                string message = Encoding.UTF8.GetString(data);
                if (message.StartsWith("ngt"))
                {
                    npc = new Npc[Convert.ToInt16(message[3].ToString()) - 1];
                    for (int i = 0; i < npc.Length; i++)
                        npc[i] = new Npc();
                    if (player.i < 2)
                    {
                        for (int i = 0; i < npc.Length; i++)
                            if (i == 0)
                                npc[i].ally = true;
                            else
                                npc[i].ally = false;
                    }
                    else
                    {
                        for (int i = 0; i < npc.Length; i++)
                            if (i == npc.Length - 1)
                                npc[i].ally = true;
                            else
                                npc[i].ally = false;
                    }

                    byte[] str = Encoding.UTF8.GetBytes("confngt");
                    lengthPrefix = BitConverter.GetBytes(str.Length);
                    stream.Write(lengthPrefix, 0, lengthPrefix.Length);
                    stream.Write(str, 0, str.Length);
                    stream.Flush();
                }
                else
                    throw new Exception(message);
            }//attesa e ricezione del numero di giocatori e assegnazione degli alleati/nemici

            Thread ricezione = new Thread(Ricezione);
			ricezione.Start();

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
			player.Move(k);
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
                player.sprite.TextureRect = new IntRect(player.stato * 12, 36, 12, 18);
            if (player.dir == 3)
                player.sprite.TextureRect = new IntRect(player.stato * 12, 0, 12, 18);

            player.sprite.Position = new Vector2f(player.cube / 10 * 81 + player.chunk / 10 * 9 + player.pixel / 10,
                                                  player.cube % 10 * 81 + player.chunk % 10 * 9 + player.pixel % 10);
            finestra.Draw(player.sprite);

            foreach (Npc n in npc)
            {
                if (n.dir == 0)
                    n.sprite.TextureRect = new IntRect(n.stato * 12, 54, 12, 18);
                if (n.dir == 1)
                    n.sprite.TextureRect = new IntRect(n.stato * 12, 18, 12, 18);
                if (n.dir == 2)
                    n.sprite.TextureRect = new IntRect(n.stato * 12, 36, 12, 18);
                if (n.dir == 3)
                    n.sprite.TextureRect = new IntRect(n.stato * 12, 0, 12, 18);

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
            };

            while (true)
            {
                List<byte> data = new List<byte>();
                byte[] buffer = new byte[1024];
                int bytesRead;
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    data.AddRange(buffer.Take(bytesRead));
                    if (bytesRead < 1024)
                        break;
                }
                string message = Encoding.UTF8.GetString(data.ToArray());

				string code = message[..3];
				message = message.Substring(3);

                if (commands.ContainsKey(code))
                {
                    Thread thread = commands[code]();
                    thread.Start(message);
                }
            }
		}

		static void plr(object message)
		{
			string msg = message.ToString();
            int i = Convert.ToInt16(msg[0].ToString());

            if (i == player.i)
            {
                player.pixel = Convert.ToInt16(msg[1..3]);
                player.chunk = Convert.ToInt16(msg[3..5]);
                player.cube  = Convert.ToInt16(msg[5..7]);
				player.stato = Convert.ToInt16(msg[7]);
                player.dir   = Convert.ToInt16(msg[8]);
            }
            else
            {
                if(i < player.i)
                {
                    npc[i].pixel = Convert.ToInt16(msg[1..3]);
                    npc[i].chunk = Convert.ToInt16(msg[3..5]);
                    npc[i].cube  = Convert.ToInt16(msg[5..7]);
                    npc[i].stato = Convert.ToInt16(msg[7]);
                    npc[i].dir   = Convert.ToInt16(msg[8]);
                }
                else
                {
                    npc[i - 1].pixel = Convert.ToInt16(msg[1..3]);
                    npc[i - 1].chunk = Convert.ToInt16(msg[3..5]);
                    npc[i - 1].cube  = Convert.ToInt16(msg[5..7]);
                    npc[i - 1].stato = Convert.ToInt16(msg[7]);
                    npc[i - 1].dir   = Convert.ToInt16(msg[8]);
                }
            }
		}
	}
}
