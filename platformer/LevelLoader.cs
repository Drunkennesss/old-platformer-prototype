using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using env = Environment;
using dir = System.AppDomain;

namespace platformer
{
    class LevelLoader
    {
        static LevelLoader inst = null;
        private byte[,] array;
        private enum GameObjects : byte
        {
            space = 0, invis_wall,wall,enemy,mainchar,visited
        
        }
        [System.Obsolete("Refactor this", true)]
        public (List<env.ICollidable>,List<env.IEnemy>,env.MainCharacter,int,int) Loadlevel(string path = null)
        {
            var lines = ReadFile(path);
            array = ConvertToByte(lines);
            return ProcessRows();
        }
        public (List<env.ICollidable>, List<env.IEnemy>, env.MainCharacter, int, int) LoadTestLevel()
        {
            var result = (new List<env.ICollidable>(), new List<env.IEnemy>(), new env.MainCharacter(1*env.Wallconst.wallsize, 2 * env.Wallconst.wallsize, false), 8, 6);
           result.Item1.Add(new env.Wall(env.Wallconst.wallsize * 1, env.Wallconst.wallsize * 3, new Vector2(env.Wallconst.wallsize * 3, env.Wallconst.wallsize)));
            result.Item1.Add(new env.Wall(env.Wallconst.wallsize * 3, env.Wallconst.wallsize * 2, new Vector2(env.Wallconst.wallsize * 2, env.Wallconst.wallsize)));
            result.Item1.Add(new env.Wall(env.Wallconst.wallsize * 4, env.Wallconst.wallsize * 5.5f, new Vector2(env.Wallconst.wallsize * 3, env.Wallconst.wallsize/2)));
            result.Item2.Add(new env.NormalEnemy(env.Wallconst.wallsize * 4, env.Wallconst.wallsize * 1, true));
            result.Item2.Add(new env.WeakEnemy(env.Wallconst.wallsize * 1, env.Wallconst.wallsize * 5, true));

            return result;
        }
        private string[] ReadFile(string path)
        {
            if (path == null)
            {
                path = Regex.Replace(dir.CurrentDomain.BaseDirectory, @"bin.*", @"levels\temp.txt");
                //path = dir.CurrentDomain.BaseDirectory + @"\levels\temp.txt";
            }
            return System.IO.File.ReadAllLines(path);
        }

        private byte[,] ConvertToByte(string[] lines)
        {            
            var temp_array = new byte[lines.Length, lines[0].Length];
            for (int i = 0; i < lines.Length; i++)
            {
                for (int j = 0; j < lines[0].Length; j++)
                {
                    switch (lines[i][j])
                    {
                        case '#':
                            temp_array[i, j] = (byte)GameObjects.invis_wall;
                            break;
                        case '-':
                            temp_array[i, j] = (byte)GameObjects.wall;
                            break;
                        case '*':
                            temp_array[i, j] = (byte)GameObjects.enemy;
                            break;
                        case '@':
                            temp_array[i, j] = (byte)GameObjects.mainchar;
                            break;
                        case ' ':
                            temp_array[i, j] = (byte)GameObjects.space;
                            break;
                    }
                }
            }
            return temp_array;
        }
        
        private (List<env.ICollidable>, List<env.IEnemy>, env.MainCharacter,int,int) ProcessRows()
        {
            var result = (new List<env.ICollidable>(), new List<env.IEnemy>(), new env.MainCharacter(0,0,false),array.GetLength(0),array.GetLength(1));
            for (int i = 0; i < array.GetLength(0); i++)
            {
                byte temp = 255;
                int start_inx = 0;
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    if(array[i,j] != (byte)GameObjects.visited || array[i, j] != (byte)GameObjects.space)
                    {
                        temp = array[i, j];
                        start_inx = j;
                        break;
                    }
                }
                if (temp == 255) continue;
                var counter = 0;
                for (int j = start_inx; j < array.GetLength(1); j++)
                {
                    if(array[i,j] == temp)
                    {
                        counter++;
                        array[i, j] = (byte)GameObjects.visited;
                    }
                }                
                switch (temp)
                {
                    case (byte)GameObjects.wall:
                        result.Item1.Add(new env.Wall(start_inx * env.Wallconst.wallsize, i * env.Wallconst.wallsize));
                        break;
                    
                    case (byte)GameObjects.enemy:
                        var temp2 = new Random().Next(0, 2);
                        if(temp2 == 1)
                        {
                            result.Item2.Add(new env.WeakEnemy(start_inx * env.Wallconst.wallsize, i * env.Wallconst.wallsize,true));
                        }
                        else
                        {
                            result.Item2.Add(new env.NormalEnemy(start_inx * env.Wallconst.wallsize, i * env.Wallconst.wallsize, false));
                        }
                        break;
                    case (byte)GameObjects.mainchar:
                        result.Item3 = new env.MainCharacter(start_inx * env.Wallconst.wallsize, i * env.Wallconst.wallsize, true);
                        break;
                }
                if (_for_process(i))
                {
                    i--;
                }
            }
            return result;
        }

        private bool _for_process(int i)
        {          
            var result111 = true;
            for (int jj = 0; jj < array.GetLength(1); jj++)
            {
                if (array[i, jj] != (byte)GameObjects.visited)
                    result111 = false;
            }
            return result111;
          
        }
        private LevelLoader() { }
        public static LevelLoader GetLevelLoader()
        {
            if(inst == null)
                inst = new LevelLoader();            
            return inst;
        }
    }
}
