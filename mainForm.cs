using chip8;

namespace Chip8
{
    public partial class mainForm : Form
    {

        Machine mac;

        public mainForm()
        {
            InitializeComponent();

            mac = new Machine(display);

            KeyPreview = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {

            mac.Run();



        }

        private void mainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                mac.Stop();
            }
            else
            {
                mac.keys.register(e.KeyCode);
            }
        }

        private void load_Click(object sender, EventArgs e)
        {

            DialogResult res = openFileDialog1.ShowDialog(this);

            if (res == DialogResult.OK)
            {
                mac.Stop();

                // MessageBox.Show(openFileDialog1.FileName);
                byte[] InFile = File.ReadAllBytes(openFileDialog1.FileName);

                // Copy file into 0x200 RAM
                int i = 0x200;
                foreach (byte b in InFile)
                {
                    mac.mem[i++] = b;
                }

                mac.Run();
            }
        }

        private void mainForm_KeyUp(object sender, KeyEventArgs e)
        {
            mac.keys.unregister(e.KeyCode);
        }

        private void mainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            mac.Stop();

        }

        private void mainForm_Load(object sender, EventArgs e)
        {
            mac.Calibrate();
            fastCheckBox.Checked = mac.fast;
        }

        private void fastCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            mac.fast = (fastCheckBox.Checked);
        }
    }
}