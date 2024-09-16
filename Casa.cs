using System;
using SFML.Graphics;
using SFML.System;

namespace LatoClient
{
    internal class Casa
    {
        IntRect rect;
        Texture texture;
        public Sprite sprite;

        int origine;
        public Casa(int origine)
        {
            this.origine = origine;
            rect = new IntRect(new Vector2i(origine / 10 * 81, origine % 10 * 81), new(81, 81));
            texture = new Texture(@"..\..\..\Tileset1.png", rect);
            sprite = new Sprite(texture);
        }

        public static implicit operator Casa(int origine) => new Casa(origine);
        public static explicit operator int(Casa c) => c.origine;
    }
}
