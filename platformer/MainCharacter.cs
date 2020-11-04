using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace Environment
{

    //TODO: implement observer -> main = subject; observers : UIobs, Controller
    //color = green;
    public interface IControlable
    {
        void MoveLeft(float time, List<ICollidable> objs);
        void MoveRight(float time, List<ICollidable> objs);
        void Jump(float time, List<ICollidable> objs);
        void Fall(float time, List<ICollidable> objs);
        void SetAmmo(byte b);
        bool? _falling { get; }
        void MakeJump();
        void MakeFall();
        IBullet Shoot(float time);
        void Actualizetime(float time);
    }

    public class MainCharacter : IShooter, IControlable, ICollidable 
    {
        private Vector2 _pos;
        public Vector2 Position
        {
            private set => _pos = value;
            get => _pos;
        }
        public Vector2 Size
        {
            get;
            private set;
        } = new Vector2(70, 100);
        public byte Ammo
        {
            get;
            private set;
        } = 1;
        public void SetAmmo(byte b)
        {
            if (b >= 0 && b < 2)
                Ammo = b;
        }
        public const float speed = 100;
        private float _air_speed = speed + float.Epsilon;
        private bool right_direction;
        public bool? _falling { get; private set; } = true;
        private byte _health = 3;
        private const float shooting_delay = 0.5f;
        private float actual_time;
        public void Actualizetime(float time)
        {
            actual_time += time;
        }
        public MainCharacter(float x, float y, bool dir)
        {
            _pos = new Vector2(x,y);
            right_direction = dir;
        }
        public IBullet Shoot(float time)
        {
            if (actual_time >= shooting_delay)
            {
                actual_time = 0;
                switch (Ammo)
                {
                    case (byte)0:                        
                        if (right_direction)
                            return new SlowBullet(_pos.X + Size.X + Size.X / 17f, _pos.Y + Size.Y * 1 / 2, right_direction);
                        else
                            return new SlowBullet(_pos.X - Size.X / 1.5f, _pos.Y + Size.Y * 1 / 2, right_direction);
                    case (byte)1:
                        if (right_direction)
                            return new FastBullet(_pos.X + Size.X + Size.X / 17f, _pos.Y + Size.Y * 1 / 2, right_direction);
                        else
                            return new FastBullet(_pos.X - Size.X / 2f, _pos.Y + Size.Y * 1 / 2, right_direction);

                    default:
                        throw new Exception("Wrong inner state");
                }
            }return null;
        }        
        public void MoveLeft(float time, List<ICollidable> objs)
        {
            _pos.X -= speed * time;            
            right_direction = false;
            if (Collision(objs))
            {
                _pos.X += speed * time;
               // right_direction = true;
            }
        }
        public void MoveRight(float time, List<ICollidable> objs)
        {
            _pos.X += speed * time;
            right_direction = true;
            if (Collision(objs))
            {
                _pos.X -= speed * time;
                //right_direction = false;
            }
        }
        public void MakeJump()
        {
            if(_falling == null)
            _falling = false;
        }
        public void MakeFall()
        {
            _falling = true;
        }
        public void Jump(float time, List<ICollidable> objs)
        {            
            if (_falling == false)
            {
                _pos.Y -= _air_speed * time;
                _air_speed -= SimplePhysics.jump_deselleratin * time;   //check this
                if (Collision(objs))
                {
                    _pos.Y += _air_speed * time;
                    _air_speed = speed + float.Epsilon;
                    MakeFall();
                }
                if (_air_speed <= 0)
                {
                    MakeFall();
                    _air_speed = speed + float.Epsilon;
                }
            }
           /* else
            {
                this.Fall(time,objs);  
            }*/           
        }
        public void Fall(float time, List<ICollidable> objs)
        {
            if (_falling == true || _falling == null)
            {
                _pos.Y += SimplePhysics.jump_deselleratin * time * _falling_increment;
                _falling_increment+=0.1f;
                if (Collision(objs))
                {
                    _pos.Y -= SimplePhysics.jump_deselleratin * time * _falling_increment;
                    _falling = null;
                    _falling_increment = 1;
                }               
            }
        }
        private float _falling_increment = 1;
        public void Destroy()
        {
            if (_health > 0)
                _health--;
            else
            {
                throw new EndGameExeption();
               // _pos.X = float.NaN;
               // _pos.Y = float.NaN;
            }
        }
        private bool Collision(List<ICollidable> objs)
        {            
            foreach (var item in objs)
            {
                if (Calculation.ForColl(this, item))
                    return true;
            }
            if (Calculation.OutOfBounds(this))
                return true;
            return false;
        }        
        

        
        /*public void Update(float x, float y, bool dir)
{
_pos.X += x;
_pos.Y += y;
right_direction = dir;
}*/
    }
    
    public class EndGameExeption : Exception
    {
        public string text { get; private set; }
        public EndGameExeption(string s = "Ha! You died")
        {
            text = s;
        }
    }
}