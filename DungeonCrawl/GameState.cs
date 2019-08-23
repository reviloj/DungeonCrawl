
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using System.Collections.Generic;

abstract public class GameState
{
    protected Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
    public ContentManager Content { get; private set; }

    public GameState()
    {

    }

    abstract public GameState update(float TimeDisplacement);

    abstract public void draw(SpriteBatch sb);

    abstract public void loadContent(Dictionary<string, Texture2D> textures);

    public Sprite getSprite(string name)
    {
        return (sprites.ContainsKey(name)) ? sprites[name] : null;
    }
}
