using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chip8
{
     public class keyboard
    {
        bool[] KeyState;   // timestamp of when the key was pressed down
        public readonly Keys[] KeysCodes = { Keys.D0, Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6, Keys.D7, Keys.D8, Keys.D9, Keys.A, Keys.B, Keys.C, Keys.D, Keys.E, Keys.F ,Keys.D0};
        

        public keyboard()
        {
            KeyState = new bool[KeysCodes.Length+1];            
        }

        public void register(Keys keyhit)
        {
            for (int i=0;i<KeysCodes.Length;i++)
            {
                if (KeysCodes[i] == keyhit)
                {
                    KeyState[i] = true;
                    break;
                }
            }
        }

        public void unregister(Keys keyhit)
        {
            for (int i = 0; i < KeysCodes.Length; i++)
            {
                if (KeysCodes[i] == keyhit)
                {
                    KeyState[i] = false;
                    break;
                }
            }
        }

        public bool isKeyDown(Keys key)
        {
            for (int i=0;i<KeysCodes.Length;i++)
            {
                if (KeyState[i] && (KeysCodes[i]==key))
                    return true;
            }

            return false;
        }

        public void clear()
        {
            for (int i=0; i<KeysCodes.Length; i++)
            {
                KeyState[i]=false;
            }
        }

        public ushort GetAnyKey()
        {
            for (int i=0;i<KeysCodes.Length;i++)
            {
                if (KeyState[i])
                {
                    return (ushort)i;
                }
            }

            return 0xfff;   // meaning no key is pressed
        }
    }
}
