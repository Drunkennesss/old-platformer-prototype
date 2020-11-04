using System.Threading;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Environment;


namespace platformer
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        private List<ICollidable> walls;
        private List<IEnemy> enemies;
        private List<ICollidable> all_collidables;
        private List<IBullet> bullets;
        private MainCharacter Mcharacter;
        private SpriteFont endgame;
        private Texture2D mainchar;
        private Texture2D walls_img;
        private Texture2D bullets_img;
        private IController<ICollidable> left;
        private IController<ICollidable> right;
        private IController<ICollidable> jump;
        private IController<ICollidable> fall;
        private IController<IBullet> shoot;
        private IController<IControlable> switch_ammo;

        bool end_game_condition = false;
        float _time_between_frames = 0;


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            var a = LevelLoader.GetLevelLoader().LoadTestLevel();
            walls = a.Item1;
            enemies = a.Item2;
            Mcharacter = a.Item3;
            graphics.PreferredBackBufferHeight = a.Item5 * (int)Wallconst.wallsize;
            graphics.PreferredBackBufferWidth = a.Item4 * (int)Wallconst.wallsize;
            Calculation.SetWorldBounds(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
            bullets = new List<IBullet>();
            Content.RootDirectory = "Content";
            all_collidables = new List<ICollidable>(enemies.ConvertAll<ICollidable>(x => (ICollidable)x));
            all_collidables.AddRange(walls);            
            left = new MLeft(Mcharacter,Keys.A);
            right = new MRight(Mcharacter,Keys.D);
            jump = new Mjump(Mcharacter,Keys.W);
            fall = new Mmidair(Mcharacter);
            shoot = new Mshoot(Mcharacter,Keys.R);
            switch_ammo = new Mswitch(Mcharacter, Keys.NumPad1, Keys.NumPad2);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);            
            bullets_img = Content.Load<Texture2D>("bullet");            
            mainchar = Content.Load<Texture2D>("character");
            walls_img = Content.Load<Texture2D>("walls");
            endgame = Content.Load<SpriteFont>("File");
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            Content.Unload();
            // TODO: Unload any non ContentManager content here
        }
       
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();           
            left.Execute((float)gameTime.TotalGameTime.TotalSeconds - _time_between_frames,all_collidables);            
            right.Execute((float)gameTime.TotalGameTime.TotalSeconds - _time_between_frames,all_collidables);            
            jump.Execute((float)gameTime.TotalGameTime.TotalSeconds - _time_between_frames,all_collidables);
            switch_ammo.Execute((float)gameTime.TotalGameTime.TotalSeconds - _time_between_frames);
            shoot.Execute((float)gameTime.TotalGameTime.TotalSeconds - _time_between_frames, bullets);            
            fall.Execute((float)gameTime.TotalGameTime.TotalSeconds - _time_between_frames,all_collidables);
            EnemyBehaviour.Behaviour(bullets, enemies, (float)gameTime.TotalGameTime.TotalSeconds - _time_between_frames);            
            BulletTime.Execute((float)gameTime.TotalGameTime.TotalSeconds - _time_between_frames, all_collidables, bullets);
            Calculation.DeleteDestroyed(enemies);
            Calculation.DeleteDestroyed(bullets);           
            try
            {
                BulletTime.HeroHit(bullets, Mcharacter);
                if (enemies.Count == 0)
                    throw new EndGameExeption();
            }
            catch (EndGameExeption e)
            {
                end_game_condition = true;
            }
            _time_between_frames = (float)gameTime.TotalGameTime.TotalSeconds;
            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {            
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();
            if (end_game_condition)
            {
                if(enemies.Count != 0)
                spriteBatch.DrawString(endgame, "You've died", new Vector2(graphics.PreferredBackBufferWidth/2.5f, graphics.PreferredBackBufferHeight/2.5f), Color.Green);
                else spriteBatch.DrawString(endgame, "You've won", new Vector2(graphics.PreferredBackBufferWidth / 2.5f, graphics.PreferredBackBufferHeight / 2.5f), Color.Green);
            }
            else
            {
                spriteBatch.Draw(mainchar, new Rectangle((int)Mcharacter.Position.X, (int)Mcharacter.Position.Y, (int)Mcharacter.Size.X, (int)Mcharacter.Size.Y), Color.Green);
                foreach (var item in walls)
                {
                    spriteBatch.Draw(walls_img, new Rectangle((int)item.Position.X, (int)item.Position.Y, (int)item.Size.X, (int)item.Size.Y), Color.Black);
                }
                foreach (var item in enemies)
                {
                    spriteBatch.Draw(mainchar, new Rectangle((int)item.Position.X, (int)item.Position.Y, (int)item.Size.X, (int)item.Size.Y), Color.Red);
                }
                foreach (var item in bullets)
                {
                    if (item is FastBullet)
                    {
                        var origin = new Vector2(item.Size.X / 2, item.Size.Y / 2);
                        SpriteEffects BulletDirection;
                        if (item.Right_direction)
                            BulletDirection = SpriteEffects.None;
                        else BulletDirection = SpriteEffects.FlipHorizontally;

                        spriteBatch.Draw(texture: bullets_img, destinationRectangle: new Rectangle((int)item.Position.X, (int)item.Position.Y, (int)item.Size.X, (int)item.Size.Y), effects: BulletDirection);
                    }
                    else if (item is SlowBullet)
                    {
                        var origin = new Vector2(item.Size.X / 2, item.Size.Y / 2);
                        SpriteEffects BulletDirection;
                        if (item.Right_direction)
                            BulletDirection = SpriteEffects.None;
                        else BulletDirection = SpriteEffects.FlipHorizontally;
                        spriteBatch.Draw(texture: bullets_img, destinationRectangle: new Rectangle((int)item.Position.X, (int)item.Position.Y, (int)item.Size.X, (int)item.Size.Y), effects: BulletDirection);

                    }
                }
            }
            spriteBatch.End();

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }

    internal interface IController<T>
    {
        void Execute(float time, List<T> objs = null);
        void SetKey(Keys a);
    }
    internal class MLeft : IController<ICollidable>
    {
        IControlable _char;
        Keys _key;
        public MLeft(IControlable a, Keys key)
        {
            _char = a;
            SetKey(key);
        }
        public void Execute(float time, List<ICollidable> objs)
        {
            if (Keyboard.GetState().IsKeyDown(_key))
                _char.MoveLeft(time,objs);
        }
        public void SetKey(Keys a)
        {
            _key = a;
        }
        
    }
    internal class MRight : IController<ICollidable>
    {
        IControlable _char;
        Keys _key;
        public MRight(IControlable a,Keys key)
        {
            _char = a;
            SetKey(key);
        }
        public void Execute(float time, List<ICollidable> objs)
        {
            if (Keyboard.GetState().IsKeyDown(_key))
                _char.MoveRight(time,objs);
        }
        public void SetKey(Keys a)
        {
            _key = a;
        }
    }
    internal class Mjump : IController<ICollidable>
    {
        IControlable _char;
        Keys _key;
        public Mjump(IControlable a,Keys key)
        {
            _char = a;
            SetKey(key);
        }
        public void Execute(float time, List<ICollidable> objs)
        {
            if (Keyboard.GetState().IsKeyDown(_key))
               // if (_char._falling == null)
                _char.MakeJump();           
        }
        public void SetKey(Keys a)
        {
            _key = a;
        }
    }
    internal class Mmidair : IController<ICollidable>
    {
        IControlable _char;        
        public Mmidair(IControlable a)
        {
            _char = a;            
        }
        public void Execute(float time, List<ICollidable> objs)
        {
           /* foreach (var item in objs)
            {
                if (item is Wall)
                    if (Calculation.HasNoProp((MainCharacter)_char, item))
                        _char.MakeFall();
            }*/
           // if (_char._falling == true)
                _char.Fall(time, objs);
            //else if (_char._falling == false) 
                _char.Jump(time, objs);
        }
        [System.Obsolete("This mehod should be called on other ICollidable",true)]
        public void SetKey(Keys a)
        {
            
        }
    }
    internal class Mshoot : IController<IBullet>
    {
        IControlable _char;
        Keys _key;
        public Mshoot(IControlable a,Keys key)
        {
            _char = a;
            SetKey(key);
        }
        public void Execute(float time,List<IBullet> objs)
        {
            _char.Actualizetime(time);
            if (Keyboard.GetState().IsKeyDown(_key))
                objs.Add(_char.Shoot(time));
        }
        public void SetKey(Keys a)
        {
            _key = a;
        }
    }
    internal class Mswitch : IController<IControlable>
    {
        IControlable _char;
        Keys _key1;
        Keys _key2;
        public Mswitch(IControlable a,Keys key1,Keys key2)
        {
            _char = a;
            _key1 = key1;
            _key2 = key2;
        }
        public void Execute(float time, List<IControlable> objs = null)
        {
            if (Keyboard.GetState().IsKeyDown(_key1))
                _char.SetAmmo(1);
            else if (Keyboard.GetState().IsKeyDown(_key2))
                _char.SetAmmo(0);
        }
        public void SetKey(Keys a)
        {
            _key1 = a == _key2?_key2:_key1;
            _key2 = a == _key1 ? _key1 : _key2;
        }
    }
    internal static class BulletTime
    {        
        public static void Execute(float time, List<ICollidable> objs,List<IBullet> b)
        {
            foreach (var item in b)
            {
                item?.Update(time);
                item?.Collision(objs);
            }
        }
        public static void HeroHit(List<IBullet> bullets, MainCharacter main)
        {
            foreach (var item in bullets)
            {
                item.Collision(main);
            }
        }
    }
    internal static class EnemyBehaviour
    {      
        public static void Behaviour(List<IBullet> bullets,List<IEnemy> enemies,float time)
        {
            foreach (var item in enemies)
            {
                bullets.Add(item.Shoot(time));
            } 
           
           
        }
    }
}
