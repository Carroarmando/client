using System;
using System.Net.Sockets;
using System.Text;
using SFML.Window;
using SFML.System;
using SFML.Graphics;


namespace LatoClient
{
    internal class Program
    {
        static RenderWindow finestra = new RenderWindow(new VideoMode(567, 567), "");

        public static int fase = 1;
        static void Main(string[] args)
        {

            Gioco.finestra = finestra;
            finestra.Closed += (object sender, EventArgs e) => finestra.Close();
            finestra.SetVerticalSyncEnabled(false);
            while (finestra.IsOpen)
            {
                switch (fase)
                {
                    case 0:
                        break;
                    case 1:
                        Gioco.gioco();
                        break;
                }
                finestra.DispatchEvents();
                finestra.Display();
            }
        }
    }
}
