
using Chip8;
using System.Collections.Generic;

namespace chip8
{
    class Machine
    {
        bool[,] frameBuffer;
        byte[] VReg = new byte[16];
        UInt16 I = 0;                           // Index register
        ushort[] stack = new ushort[16];
        byte SP;                                // stack pointer
        byte DT;                                // Delay Timer
        byte ST;                                // Sound Timer
        UInt16 PC;                              // Program Counter
        public byte[] mem = new byte[4096];     // 4K Memory
        bool run = false;                       // CPU Run Flag
        bool refreshScreen = false;             // Screen refresh thread flag
        PictureBox canvas;                      // Framebuffer screen object
        readonly ushort SCRWidth = 64;
        readonly ushort SCRHeight = 32;
        public keyboard keys = new keyboard();

        private double waitTime = 0;
        private int CalibrationInstructionCount = 700;   // how many instructions per second to calibrate to
        private bool calibrateMode = true;

        byte[] CalibrateROM = { 0x60, 0x00, 0x70, 0x01, 0x61, 0x0F, 0x80, 0x12, 0x62, 0x00, 0xF0, 0x29, 0xD2, 0x25, 0x6A, 0xFF, 0xFA, 0x15, 0x00, 0xE0, 0x12, 0x02 };

        public Machine(PictureBox can)
        {
            Random rnd = new();

            // Initialize display framebuffer
            frameBuffer = new bool[SCRWidth, SCRHeight];

            // Initialize V Registers
            for (int i = 0; i < 16; i++)
            {
                VReg[i] = 0;
            }


            // Initialize memory with fake random garbage
            for (int i = 0; i < 4096; i++)
            {
                mem[i] = (byte)rnd.Next(0, 256);
            }

            // Initialize the program memory with the self-calibrating program.
            for (int i = 0; i < CalibrateROM.Length; i++)
            {
                mem[0x200 + i] = CalibrateROM[i];
            }

            this.canvas = can;

            //Calibrate();

        }

        /// <summary>
        /// Run specified number of instructions and return how many milliseconds it took to do so
        /// </summary>
        /// <param name="instructions"></param>
        public async void Calibrate()
        {
            calibrateMode = true;
            int InitialInstructionCount = CalibrationInstructionCount;

            Task? calibrateTask = Run(); // ? as it may return null.

            if (calibrateTask != null)
            {
                DateTime start = DateTime.Now;
                await calibrateTask;

                double duration = (DateTime.Now - start).TotalMilliseconds;
                MessageBox.Show(duration.ToString());
                calibrateMode = false;

                //double msecPerInstructions = (double)duration / InitialInstructionCount; // One instruction takes this time
                waitTime = (1000-duration)/InitialInstructionCount;
            }
        }

        private void RenderDisplayPort()//Canvas can)
        {
            SolidBrush blackBrush = new(Color.Black);
            SolidBrush whiteBrush = new(Color.White);

            Rectangle[,] pixels = new Rectangle[SCRWidth, SCRHeight];
            Graphics display = canvas.CreateGraphics();




            // Initialize the pixel matrix
            for (int i = 0; i < SCRWidth; i++)
            {
                for (int j = 0; j < SCRHeight; j++)
                {
                    pixels[i, j] = new Rectangle(i * 10, j * 10, 10, 10);
                }
            }

            while (refreshScreen)
            {

                Thread.Sleep(5);


                for (int i = 0; i < SCRWidth; i++)
                {
                    for (int j = 0; j < SCRHeight; j++)
                    {
                        if (frameBuffer[i, j])
                        {
                            display.FillRectangle(whiteBrush, pixels[i, j]);
                        }
                        else
                        {
                            display.FillRectangle(blackBrush, pixels[i, j]);
                        }
                    }
                }
            }



            // Destroy the resources
            display.Dispose();
            whiteBrush.Dispose();
            blackBrush.Dispose();
            Console.Beep(500, 100);
            Thread.Sleep(100);
        }

        public Task? Run()    // nullable
        {
            if (!run)
            {
                run = true;

                CharROM charrom = new CharROM(mem, 0);

                if (!refreshScreen && !calibrateMode)
                {
                    refreshScreen = true;
                    Task.Run(RenderDisplayPort);
                    Thread.Sleep(100);
                    Console.Beep(1000, 100);
                    ClearScreen();
                }

                // Start the 60Hz timer ticker
                if (!calibrateMode)
                {
                    Task.Run(SixtyHertzTick);
                }

                PC = 0x200;
                SP = 0;
                DT = 0;
                ST = 0;
                keys.clear();

                return Task.Run(RunCPU);
            }

            return null;
        }

        private void SixtyHertzTick()
        {
            while (run)
            {
                if (DT > 0)
                {
                    DT--;
                }

                if (ST > 0)
                {
                    Console.Beep(1000, 1000 / 60);
                    ST--;
                }
                else
                {
                    Thread.Sleep(1000 / 60); // ~ 60Hz
                }


            }
        }

        public void Stop()
        {
            refreshScreen = false;
            run = false;
            //sixtyHertz.IsEnabled = false;
            Thread.Sleep(100);
        }

        private void RunCPU()
        {
            UInt16 inst;
            UInt16 nnn;
            byte kk;
            byte op;
            byte Vx;
            byte Vy;
            byte n;
            int i;
            int j;
            bool HALT = false;
            DateTime startWait;

            // Reinitialize registers
            for (i = 0; i < 0x10; i++)
            {
                VReg[i] = 0;
            }

            while (run)
            {
                 startWait = DateTime.Now;
                while ((DateTime.Now - startWait).TotalMilliseconds < waitTime) { };

                // skip executing the CPU for a set number of times based on the speed calibration
                
                {                
                    if (calibrateMode)
                    {
                        if (CalibrationInstructionCount > 0)
                        {
                            CalibrationInstructionCount--;
                        }
                        else
                        {
                            run = false;
                            break;
                        }
                    }



                    if (PC >= mem.Length)
                    {
                        this.Stop();
                        MessageBox.Show("Program Counter Overrun.");
                        break;
                    }

                    inst = (ushort)(((ushort)mem[PC] << 8) | ((ushort)mem[PC + 1]));
                    nnn = (ushort)(inst & 0xFFF);
                    kk = (byte)(inst & 0xFF);
                    op = (byte)((inst & 0xf000) >> 12);
                    Vx = (byte)((inst & 0x0f00) >> 8);
                    Vy = (byte)((inst & 0x00f0) >> 4);
                    n = (byte)(inst & 0x000F);

                    switch (op)
                    {
                        case 0x0:
                            {
                                switch (kk)
                                {
                                    // CLS
                                    case 0xe0:
                                        {
                                            ClearScreen();
                                            break;
                                        }

                                    // RET
                                    case 0xee:
                                        {
                                            PC = stack[SP];
                                            SP--;
                                            break;
                                        }
                                }
                            }
                            break;

                        // JP nnn
                        case 0x1:
                            {
                                PC = (ushort)(nnn - 2);  // PC will increment by 2 at end of loop
                                break;
                            }

                        // CALL nnn
                        case 0x2:
                            {
                                SP++;

                                if (SP >= stack.Length)
                                {
                                    this.Stop();
                                    MessageBox.Show("Stack overflow.");
                                    break;
                                }


                                stack[SP] = (ushort)(PC);
                                PC = (ushort)(nnn - 2);   // PC will increment by 2 focibly at end of loop

                                break;
                            }

                        // SE Vx, kk
                        case 0x3:
                            {
                                if (VReg[Vx] == kk)
                                { PC += 2; }
                                break;
                            }

                        // SNE Vx, kk
                        case 0x4:
                            {
                                if (VReg[Vx] != kk)
                                { PC += 2; }
                                break;
                            }

                        // SE Vx, Vy
                        case 0x5:
                            {
                                if (VReg[Vx] == VReg[Vy])
                                { PC += 2; }
                                break;
                            }

                        // LD Vx, kk
                        case 0x6:
                            {
                                VReg[Vx] = kk;
                                break;
                            }

                        // ADD Vx, kk
                        case 0x7:
                            {
                                VReg[Vx] += kk;
                                break;
                            }

                        // LD Vx, Vy
                        case 0x8:
                            {
                                switch (n)
                                {

                                    // LD Vx, Vy
                                    case 0x0:
                                        {

                                            VReg[Vx] = VReg[Vy];
                                            break;
                                        }

                                    // OR Vx, Vy
                                    case 0x1:
                                        {
                                            VReg[Vx] |= VReg[Vy];
                                            break;
                                        }

                                    // AND Vx, Vy
                                    case 0x2:
                                        {
                                            VReg[Vx] &= VReg[Vy];
                                            break;
                                        }

                                    // XOR Vx, Vy
                                    case 0x3:
                                        {
                                            VReg[Vx] ^= VReg[Vy];
                                            break;
                                        }

                                    // ADD Vx, Vy
                                    case 0x4:
                                        {
                                            int interim = VReg[Vx] + VReg[Vy];
                                            VReg[Vx] = (byte)(interim);
                                            VReg[0xf] = (Byte)(interim > 255 ? 1 : 0);

                                            break;
                                        }

                                    // SUB Vx, Vy
                                    case 0x5:
                                        {
                                            byte interim = (byte)(VReg[Vx] > VReg[Vy] ? 1 : 0);
                                            VReg[Vx] -= VReg[Vy];
                                            VReg[0xf] = interim;
                                            break;
                                        }

                                    // SHR Vx,Vy
                                    // Store the value of register VY shifted right one bit in register VX
                                    // Set register VF to the least significant bit prior to the shift
                                    // VY is unchanged
                                    case 0x6:
                                        {
                                            byte interim = (byte)(VReg[Vy] & 0x1);
                                            VReg[Vx] = (byte)(VReg[Vy] >> 1);
                                            VReg[0xf] = interim;
                                            break;
                                        }

                                    // SUBN Vx, Vy
                                    case 0x7:
                                        {
                                            byte interim = (byte)((VReg[Vy] > VReg[Vx]) ? 1 : 0);
                                            VReg[Vx] = (byte)(VReg[Vy] - VReg[Vx]);
                                            VReg[0xf] = interim;
                                            break;
                                        }

                                    // SHL Vx, Vy
                                    // Store the value of register VY shifted left one bit in register VX
                                    // Set register VF to the most significant bit prior to the shift
                                    // VY is unchanged
                                    case 0xe:
                                        {
                                            byte interim = (byte)(((VReg[Vy] & 0x80) != 0) ? 1 : 0);
                                            VReg[Vx] = (byte)(VReg[Vy] << 1);
                                            VReg[0xf] = interim;
                                            break;
                                        }
                                }
                                break;
                            }

                        // SNE Vx, Vy
                        case 0x9:
                            {
                                if (VReg[Vx] != VReg[Vy]) { PC += 2; }
                                break;
                            }

                        // LD I , nnn
                        case 0xa:
                            {
                                I = nnn;
                                break;
                            }

                        // JP V0, nnn
                        case 0xb:
                            {
                                PC = (ushort)(VReg[0] + nnn - 2); // PC is auto-incremented by 2 after loop
                                break;
                            }

                        // RND Vx, kk
                        case 0xc:
                            {
                                VReg[Vx] = (byte)(new Random().Next(0, 256) & kk);
                                break;
                            }

                        // DRW Vx, Vy,n
                        case 0xd:
                            {
                                VReg[0xf] = 0;
                                ushort EffectiveX;
                                ushort EffectiveY;

                                for (j = 0; j < n; j++)
                                {
                                    for (i = 0; i < 8; i++)
                                    // (int)VReg[Vx]; i < (int)(VReg[Vx] + 8); i++)
                                    {
                                        if (((mem[I + j] >> (7 - i)) & 0x1) != 0)
                                        {
                                            EffectiveX = (ushort)((VReg[Vx] + i) % SCRWidth);
                                            EffectiveY = (ushort)((VReg[Vy] + j) % SCRHeight);

                                            if (frameBuffer[EffectiveX, EffectiveY]) { VReg[0xf] = 1; }
                                            frameBuffer[EffectiveX, EffectiveY] ^= true;
                                        }
                                    }
                                }
                                //Thread.Sleep(1000 / 60);// allow display refresh to catch up
                                break;
                            }

                        case 0xe:
                            {
                                switch (kk)
                                {
                                    // SKP Vx
                                    case 0x9e:
                                        {
                                            if (keys.isKeyDown(keys.KeysCodes[VReg[Vx]]))
                                            {
                                                PC += 2;
                                            }
                                            break;
                                        }

                                    // SKNP Vx
                                    case 0xa1:
                                        {
                                            if (!keys.isKeyDown(keys.KeysCodes[VReg[Vx]]))
                                            {
                                            }
                                            PC += 2;
                                            break;
                                        }
                                }
                                break;
                            }

                        case 0xf:
                            {
                                switch (kk)
                                {
                                    // HALT (custom op code)
                                    case 0xFF:
                                        {
                                            if (!HALT)
                                            {
                                                HALT = true;
                                                MessageBox.Show("HALT instruction encountered at address " + PC.ToString("X4")); ;
                                            }

                                            PC -= 2;
                                            break;
                                        }

                                    // LD Vx, DT
                                    case 0x07:
                                        {
                                            VReg[Vx] = DT;
                                            break;
                                        }

                                    // LD Vx, K
                                    case 0x0a:
                                        {
                                            ushort inkey;

                                            inkey = keys.GetAnyKey();
                                            if (inkey == 0xfff)   // meaning no key pressed
                                            {
                                                PC -= 2;
                                            }
                                            else
                                            {
                                                VReg[Vx] = (byte)inkey;
                                            }
                                            break;
                                        }

                                    // LD DT, Vx
                                    case 0x15:
                                        {
                                            DT = VReg[Vx];
                                            break;
                                        }

                                    // LD ST, Vx
                                    case 0x18:
                                        {
                                            ST = VReg[Vx];
                                            break;
                                        }

                                    // ADD I, Vx
                                    case 0x1e:
                                        {
                                            I = (ushort)(I + VReg[Vx]);
                                            break;
                                        }

                                    // Fx29 - LD I, Vx
                                    case 0x29:
                                        {
                                            I = (ushort)(VReg[Vx] * 5);
                                            break;
                                        }

                                    // LD [I], Vx (BCD)
                                    case 0x33:
                                        {
                                            mem[I] = (byte)(VReg[Vx] / 100);
                                            mem[I + 1] = (byte)((VReg[Vx] / 10) % 10);
                                            mem[I + 2] = (byte)(VReg[Vx] % 10);
                                            break;
                                        }

                                    // LD [I], Vx
                                    case 0x55:
                                        {
                                            for (j = 0; j <= Vx; j++)
                                            {
                                                mem[I + j] = VReg[j];
                                            }
                                            break;
                                        }

                                    // LD Vx, [I]
                                    case 0x65:
                                        {
                                            for (j = 0; j <= Vx; j++)
                                            {
                                                VReg[j] = mem[I + j];
                                            }
                                            break;
                                        }
                                }
                                break;
                            }
                    }

                    PC += 2;
                }
                //Thread.Sleep(1000 / 60);// allow display time to breathe and refresh
            }

            Console.Beep(1000, 100);
        }

        void ClearScreen()
        {
            int i;
            for (i = 0; i < SCRWidth; i++)
            {
                for (int j = 0; j < SCRHeight; j++)
                {
                    frameBuffer[i, j] = false;
                }
            }

        }
    }
}
