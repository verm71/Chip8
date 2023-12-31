﻿namespace Chip8
{
    public class keypad
    {
        bool[] KeyState;   // timestamp of when the key was pressed down
        public readonly Keys[] KeysCodes = { Keys.X, Keys.D1, Keys.D2, Keys.D3, Keys.Q, Keys.W, Keys.E, Keys.A, Keys.S, Keys.D, Keys.Z, Keys.C, Keys.D4, Keys.R, Keys.F, Keys.V, Keys.D0 };

        public keypad()
        {
            KeyState = new bool[KeysCodes.Length + 1];
        }

        public byte register(Keys keyhit)
        {
            for (int i = 0; i < KeysCodes.Length; i++)
            {
                if (KeysCodes[i] == keyhit)
                {
                    KeyState[i] = true;
                    return (byte)i;                    
                }
            }

            return 0xff; // key not found
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
            for (int i = 0; i < KeysCodes.Length; i++)
            {
                if (KeyState[i] && (KeysCodes[i] == key))
                    return true;
            }

            return false;
        }

        public bool isKeyDown(byte key)
        {
            return (KeyState[key]);
        }

        public void clear()
        {
            for (int i = 0; i < KeysCodes.Length; i++)
            {
                KeyState[i] = false;
            }
        }

        public byte GetAnyKey()
        {
            for (byte i = 0; i < KeysCodes.Length; i++)
            {
                if (KeyState[i])
                {
                    return i;
                }
            }

            return 0xff;   // meaning no key is pressed
        }
    }
}
