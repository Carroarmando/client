using SFML.Graphics;

namespace LatoClient
{
    internal class Npc
    {
        static IntRect rect = new IntRect(0, 0, 48, 72);
        static Texture texture = new Texture(@"..\..\..\Player.png", rect);
        public Sprite sprite = new Sprite(texture);

        public int stato = 0, dir;
        public int cube = 0, chunk = 0, pixel = 0;

        public bool ally;
        public Npc()
        {
            sprite.TextureRect = new IntRect(0, 0, 12, 18);
            sprite.Scale *= 2;
        }
    }
}
