using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Environment
{//TODO: add bool direction to enemies 
    static class SimplePhysics
    {
        public const float fall_speed = 100;
        public const float jump_deselleratin = 30;    
       
    }

    static class Calculation
    {
        private static Vector2 _worldbounds;
        public static void SetWorldBounds(int x, int y)
        {
            if(_worldbounds == default(Vector2))
                _worldbounds = new Vector2(x, y);
        }
        public static bool ForColl(ICollidable main, ICollidable second)
        {            
            if (main.Position.Y < second.Position.Y + second.Size.Y && main.Position.Y + main.Size.Y > second.Position.Y &&
            main.Position.X + main.Size.X > second.Position.X && main.Position.X < second.Position.X + second.Size.X)
            {
                return true;
            }
            return false;
        }
        public static bool OutOfBounds(ICollidable main)
        {
            if (main.Position.X + main.Size.X >= _worldbounds.X || main.Position.Y + main.Size.Y >= _worldbounds.Y ||
                main.Position.X < 0 || main.Position.Y < 0)
                return true;
            return false;
        }
        public static bool HasNoProp(MainCharacter main, ICollidable second)
        {
            if (main._falling == null && main.Position.X + main.Size.X < second.Position.X && main.Position.Y + main.Size.Y >= second.Position.Y)
                return true;
            return false;
        }        
        public static void DeleteDestroyed<T>(List<T> objs) where T: ICollidable
        {
            objs.RemoveAll(x => x == null || float.IsNaN(x.Position.X) || float.IsNaN(x.Position.Y));
        }
        
    }
    public interface ICollidable
    {
        Vector2 Position
        {
            get;
        }
        Vector2 Size
        {
            get;
        }
        void Destroy();
    }
    public abstract class IBullet : ICollidable //color = grey
    {
        public float Speed
        {
            get;
            protected set;
        }
        protected Vector2 _pos;

        public Vector2 Position
        {
            get => _pos;
            protected set => _pos = value;
        }

        public Vector2 Size
        {
            get;
            protected set;
        }
        protected bool right_direction;
        public bool Right_direction { get => right_direction; protected set => right_direction = value; }

        public IBullet(float x, float y, bool dir)
        {
            Position = new Vector2((int)x, (int)y);
            right_direction = dir;     
        }

        public void Update(float time)
        {
            if (_pos.X != float.NaN && _pos.Y != float.NaN)
            {
                if (right_direction)
                    _pos.X += Speed * time;
                else _pos.X -= Speed * time;
            }
        }

        public void Collision(List<ICollidable> objs)
        {
            foreach (var item in objs)
            {
                if (Calculation.ForColl(this, item))
                {                    
                    if (item is IEnemy || item is MainCharacter)
                    {                        
                        item.Destroy();
                    }
                    this.Destroy();
                }
            }
            if (Calculation.OutOfBounds(this))
                this.Destroy();
        }
        public void Collision(MainCharacter main)
        {
            if (Calculation.ForColl(this, main))
            {
                main.Destroy();
                this.Destroy();
            }
        }

        public void Destroy()
        {
            this._pos.X = float.NaN;
            this._pos.Y = float.NaN;
        }
        

    }
    public class FastBullet : IBullet
    {
        public FastBullet(float x, float y, bool dir) : base(x, y, dir)
        {           
            Speed = 200;
            Size = new Vector2(20);
        }
    }

    public class SlowBullet : IBullet
    {
        public SlowBullet(float x, float y, bool dir) : base(x,y,dir)
        {            
            Speed = 100;
            Size = new Vector2(40);
        }
    }

    public interface IShooter
    {
        IBullet Shoot(float time);  
        
    }

    public abstract class IEnemy : ICollidable, IShooter // color = red || orange
    {
        protected Vector2 _pos;

        public Vector2 Position
        {
            get => _pos;
            protected set => _pos = value;
        }
        public bool Right_direction
        {
            get;
            protected set;
        }
        public abstract IBullet Shoot(float time);
        protected float _shooting_delay;
        protected float actual_time;
        public Vector2 Size
        {
            get;
            protected set;
        }

        public IEnemy(float x, float y, bool dir)
        {
            _pos = new Vector2((int)x, (int)y);
            Right_direction = dir;
        }
        
        public virtual void Destroy()
        {
            _pos.X = float.NaN;
            _pos.Y = float.NaN;
        }
    }

    public class WeakEnemy : IEnemy
    {
        public WeakEnemy(float x, float y, bool dir) : base(x, y, dir) { Size = new Vector2(100); }
        public override IBullet Shoot(float time)
        {
            actual_time += time;
            var rand = new Random();
            
            if (rand.Next(0, 2) != 1)
                Right_direction = !Right_direction;
            if (actual_time >= _shooting_delay)
            {
                if (Right_direction)
                {
                    _shooting_delay = rand.Next(0, 4) + (float)rand.NextDouble();
                    actual_time = 0;
                    return new SlowBullet(_pos.X + Size.X + float.Epsilon, _pos.Y + Size.Y * 1 / 2, Right_direction);
                }
                else
                {
                    _shooting_delay = rand.Next(0, 4) + (float)rand.NextDouble();
                    actual_time = 0;
                    return new SlowBullet(_pos.X - Size.X/2.5f, _pos.Y + Size.Y * 1 / 2, Right_direction);
                }
            }
            return null;
        }
    }

    public class NormalEnemy : IEnemy
    {
        private byte _health = 2;

        public NormalEnemy(float x, float y, bool dir) : base(x, y, dir)
        {
            Size = new Vector2(100);
        }

        public override IBullet Shoot(float time)
        {
            actual_time += time;
            var rand = new Random();
            if (rand.Next(0, 2) != 1)
                Right_direction = !Right_direction;
            if (actual_time >= _shooting_delay)
            {
                if (Right_direction)
                {
                    _shooting_delay = rand.Next(0, 4) + (float)rand.NextDouble();
                    actual_time = 0;
                    return new FastBullet(_pos.X + Size.X + float.Epsilon, _pos.Y + Size.Y * 1 / 2, Right_direction);
                }
                else
                {
                    _shooting_delay = rand.Next(0, 4) + (float)rand.NextDouble();
                    actual_time = 0;
                    return new FastBullet(_pos.X - Size.X / 2.5f, _pos.Y + Size.Y * 1 / 2, Right_direction);
                }
            }
            return null;
        }

        public override void Destroy()
        {
            if (_health > 0)
                _health--;
            else base.Destroy();
            
        }
    }

    public class Wall : ICollidable // color : black
    {
        public Vector2 Position
        {
            get;
            private set;
        }
        public Vector2 Size
        {
            get;
            private set;
        } = new Vector2(Wallconst.wallsize);

        public Wall(float x, float y)
        {
           Position = new Vector2((int)x, (int)y);           
        }
        public Wall(float x, float y, Vector2 size)
        {
            Position = new Vector2((int)x, (int)y);
            this.Size = size;
        }

        [System.ObsoleteAttribute("Destroy metod should be called from other ICollidable",true)]
        public void Destroy() { }
    }
   
    public static class Wallconst { public const float wallsize = 100; }


}