# Chip8

Chip8 emulator targeting .Net 6 with the following features:

- Automatic self-calibration of execution speed, roughly set at 700 instructions per second.
- Asynchronous Display Refresh, CPU and Timer threads.
- Esc key to halt execution.
- Extra opcode FFFF for code halt.
- Audible beeps to indicate starting and stopping of CPU thread, and framebuffer stoppage.

Works with "some" games, and most demos. There may be some debugging involved in the keyboard input, although most Chip8 test programs say it's ok.

Enjoy, and learn as I did! 