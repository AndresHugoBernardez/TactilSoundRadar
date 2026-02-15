
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SoundRadar
{
    public partial class Form1 : Form
    {

        private SerialPort serialPort;

        public Form1()
        {
            InitializeComponent();

            this.Load += (sender, e) =>
            {
                PopulateTableWithPanels();
                InitializeSerialAndLog(); // inicializa el log y el puerto serie
            };
        }

        private void InitializeSerialAndLog()
        {
            // Elimina y dispone los items dinámicos (mantén noPortToolStripMenuItem)
            for (int i = puertoToolStripMenuItem.DropDownItems.Count - 1; i >= 0; i--)
            {
                var item = puertoToolStripMenuItem.DropDownItems[i];
                if (item == noPortToolStripMenuItem)
                    continue;
                puertoToolStripMenuItem.DropDownItems.RemoveAt(i);
                item.Dispose();
            }

            var ports = SerialPort.GetPortNames();

            if (ports.Length == 0)
            {
                this.noPortToolStripMenuItem.Visible = true;
                return;
            }

            this.noPortToolStripMenuItem.Visible = false;

            // Añade un ToolStripMenuItem por cada puerto disponible
            foreach (var p in ports)
            {
                var portName = p; // copia local para evitar captura en el lambda
                var mi = new ToolStripMenuItem(portName)
                {
                    Tag = portName,
                    Checked = false
                };
                mi.Click += Checking;
                puertoToolStripMenuItem.DropDownItems.Add(mi);
            }
        }

        private void Checking(object sender, EventArgs e)
        {
            if (!(sender is ToolStripMenuItem clicked))
                return;

            // Si estaba marcado -> desmarcar todo y cerrar puerto
            if (clicked.Checked)
            {
                foreach (ToolStripItem item in puertoToolStripMenuItem.DropDownItems)
                {
                    if (item is ToolStripMenuItem mi) mi.Checked = false;
                }
                CloseSerial();
                return;
            }

            // Marcar solo el pulsado y abrir
            foreach (ToolStripItem item in puertoToolStripMenuItem.DropDownItems)
            {
                if (item is ToolStripMenuItem mi) mi.Checked = false;
            }

            clicked.Checked = true;
            var portName = clicked.Tag as string ?? clicked.Text;
            StartSerial(portName, 9600);
        }

        private void StartSerial(string portName, int baudRate = 9600)
        {
            if (string.IsNullOrWhiteSpace(portName))
                return;

            try
            {
                if (serialPort != null)
                {
                    CloseSerial();
                }

                serialPort = new SerialPort(portName, baudRate)
                {
                    Encoding = Encoding.ASCII,
                    NewLine = "\n",
                    ReadTimeout = 500
                };

                serialPort.DataReceived += SerialPort_DataReceived;
                serialPort.Open();
            }
            catch (Exception)
            {
                if (serialPort != null)
                {
                    try { serialPort.Dispose(); } catch { }
                    serialPort = null;
                }
            }
        }

        private void CloseSerial()
        {
            try
            {
                if (serialPort != null)
                {
                    try
                    {
                        serialPort.DataReceived -= SerialPort_DataReceived;
                        if (serialPort.IsOpen)
                            serialPort.Close();
                    }
                    catch { }
                    try
                    {
                        serialPort.Dispose();
                    }
                    catch { }
                    serialPort = null;
                }
            }
            catch (Exception)
            {
                // opcional: log
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string line;
                try
                {
                    line = serialPort.ReadLine();
                }
                catch (TimeoutException)
                {
                    line = serialPort.ReadExisting();
                }

                if (!string.IsNullOrEmpty(line))
                {
                    // Marshal seguro a UI
                    if (this.IsHandleCreated && !this.IsDisposed)
                    {
                        this.BeginInvoke(new Action<string>(ProcesarComandoSerie), line);
                    }
                }
            }
            catch (Exception)
            {
                // opcional: log
            }
        }

        // Posiciones
        private int positionL = 0;
        private int positionR = 0;
        private int positionF = 0;
        private int positionB = 0;

        private void ProcesarComandoSerie(string input)
        {
            if (string.IsNullOrWhiteSpace(input) || tableLayoutPanel1 == null)
                return;

            // Procesa cada línea completa que venga (evita loops infinitos)
            var lines = input.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                // Mantén la actualización de UI agrupada para evitar parpadeo
                tableLayoutPanel1.SuspendLayout();
                this.SuspendLayout();

                // Pinta rastro anterior (lo mantenías en azul)
                SafeColorearAzul(positionL, positionF);
                SafeColorearAzul(positionR, positionF);
                SafeColorearAzul(positionL, positionB);
                SafeColorearAzul(positionR, positionB);

                // Parse seguro: cada comando es una letra L/R/F/B seguida de un dígito opcional 0..7
                for (int i = 0; i < line.Length; i++)
                {
                    char letra = line[i];
                    if ((letra == 'L' || letra == 'R' || letra == 'F' || letra == 'B') && i + 1 < line.Length)
                    {
                        char d = line[++i];
                        int val = -1;
                        if (d >= '0' && d <= '7')
                            val = d - '0';

                        switch (letra)
                        {
                            case 'L':
                                positionL = (val >= 0) ? 7 - val : 7;
                                break;
                            case 'R':
                                positionR = (val >= 0) ? 8 + val : 8;
                                break;
                            case 'F':
                                positionF = (val >= 0) ? 7 - val : 7;
                                break;
                            case 'B':
                                positionB = (val >= 0) ? 8 + val : 8;
                                break;
                        }
                    }
                    else
                    {
                        // Ignorar caracteres no reconocidos
                    }
                }

                // Colorea nueva posición en rojo
                SafeColorearRojo(positionL, positionF);
                SafeColorearRojo(positionR, positionF);
                SafeColorearRojo(positionL, positionB);
                SafeColorearRojo(positionR, positionB);

                tableLayoutPanel1.ResumeLayout(false);
                this.ResumeLayout(false);
                this.PerformLayout();
            }
        }

        // Métodos seguros para colorear (comprueban índices y nulls)
        private bool IsValidCell(int col, int row)
        {
            return tableLayoutPanel1 != null
                && col >= 0 && col < tableLayoutPanel1.ColumnCount
                && row >= 0 && row < tableLayoutPanel1.RowCount;
        }

        private void SafeColorearAzul(int a, int b)
        {
            if (!IsValidCell(a, b)) return;
            var ctl = tableLayoutPanel1.GetControlFromPosition(a, b);
            if (ctl != null) ctl.BackColor = Color.DarkSlateBlue;
        }

        private void SafeColorearRojo(int a, int b)
        {
            if (!IsValidCell(a, b)) return;
            var ctl = tableLayoutPanel1.GetControlFromPosition(a, b);
            if (ctl != null) ctl.BackColor = Color.DarkRed;
        }

        private void SafeDecolorar(int a, int b)
        {
            if (!IsValidCell(a, b)) return;
            var ctl = tableLayoutPanel1.GetControlFromPosition(a, b);
            if (ctl != null) ctl.BackColor = SystemColors.Control;
        }

        private void PopulateTableWithPanels()
        {
            if (tableLayoutPanel1 == null)
                return;

            tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();

            tableLayoutPanel1.Controls.Clear();

            int cols = tableLayoutPanel1.ColumnCount;
            int rows = tableLayoutPanel1.RowCount;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    var panel = new Panel
                    {
                        Dock = DockStyle.Fill,
                        Margin = new Padding(1),
                        BackColor = SystemColors.ControlDark,
                        Tag = new Point(c, r)
                    };

                    panel.Click += Panel_Click;

                    tableLayoutPanel1.Controls.Add(panel, c, r);
                }
            }

            tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private void Panel_Click(object sender, EventArgs e)
        {
            // Puedes reactivar el toggling si quieres
        }

        private void toolStripDropDownButton1_Click(object sender, EventArgs e)
        {

        }

        private void pToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void puertoToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox17_Click(object sender, EventArgs e)
        {

        }

        private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}