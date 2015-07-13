using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using chippy8;

namespace chippy8gui
{
	public class MainForm : Form
	{
		FileStream log;
		StreamWriter logWriter;

		public MainForm () {
			InitializeComponents ();
			InitializeLog ();
		}

		protected override void OnHandleCreated (EventArgs e)
		{
			InitializeEmulator ();
			base.OnHandleCreated (e);
		}

		protected override void OnClosing (System.ComponentModel.CancelEventArgs e)
		{
			logWriter.Close ();
			log.Close ();
			base.OnClosing (e);
		}

		void InitializeLog () {
			log = new FileStream (Path.Combine (Application.StartupPath, "log.txt"), FileMode.OpenOrCreate);
			logWriter = new StreamWriter (log);
			logWriter.AutoFlush = true;
			Console.SetOut (logWriter);
		}

		void InitializeEmulator () {
			Emulator.Instance
				.Connect<ManagedMemory> ()
				.Connect<ManagedCPU> ()
				.Connect<WinFormsDisplay> ()
				.Connect<WinFormsKeyboard> ();
			(Emulator.Instance.Keypad as WinFormsKeyboard).AttachTo (this);
			var disp = (Emulator.Instance.Screen as WinFormsDisplay);
			disp.AttachTo (this);
			disp.Width = 640;
			disp.Height = 320;
			disp.Location = new Point (0, 0);
		}

		void InitializeComponents () {
			this.StartPosition = FormStartPosition.CenterScreen;
			this.FormBorderStyle = FormBorderStyle.FixedSingle;
			this.BackColor = Color.FromArgb (230, 230, 230);
			this.BackgroundImageLayout = ImageLayout.Zoom;
			this.MaximizeBox = false;

			#region Menu
			var menu = new MainMenu ();

			// Program menu
			var menu_program = new MenuItem ("Program");
			menu_program.MenuItems.Add (new MenuItem ("Load program", LoadProgram, Shortcut.CtrlO));
			menu_program.MenuItems.Add (new MenuItem ("Exit", (sender, e) => Application.Exit (), Shortcut.CtrlX));

			// Emulator menu
			var menu_emulator = new MenuItem ("Emulator");
			menu_emulator.MenuItems.Add (new MenuItem ("Start", (sender, e) => Debugger.Instance.Continue (), Shortcut.AltUpArrow));
			menu_emulator.MenuItems.Add (new MenuItem ("Pause", (sender, e) => Debugger.Instance.Pause (), Shortcut.AltDownArrow));
			menu_emulator.MenuItems.Add (new MenuItem ("Stop", (sender, e) => Debugger.Instance.Stop (), Shortcut.AltLeftArrow));
			menu_emulator.MenuItems.Add ("Restart", (sender, e) => Debugger.Instance.Restart ());
			var menu_emulator_speed = new MenuItem ("Speed");
			menu_emulator_speed.MenuItems.Add ("25%", (sender, e) => Debugger.Instance.Frequency = 1000);
			menu_emulator_speed.MenuItems.Add ("50%", (sender, e) => Debugger.Instance.Frequency = 2000);
			menu_emulator_speed.MenuItems.Add ("100%", (sender, e) => Debugger.Instance.Frequency = 4000);
			menu_emulator_speed.MenuItems.Add ("200%", (sender, e) => Debugger.Instance.Frequency = 8000);
			menu_emulator_speed.MenuItems.Add ("500%", (sender, e) => Debugger.Instance.Frequency = 20000);
			menu_emulator_speed.MenuItems.Add ("Unlimited", (sender, e) => Debugger.Instance.Frequency = 0);
			menu_emulator.MenuItems.Add (menu_emulator_speed);

			// Debug menu
			var menu_debug = new MenuItem ("Debugger");
			menu_debug.MenuItems.Add (new MenuItem ("Step", (sender, e) => Debugger.Instance.Step (), Shortcut.AltRightArrow));
			menu_debug.MenuItems.Add (new MenuItem ("Dump RAM", DumpRam, Shortcut.CtrlD));
			menu_debug.MenuItems.Add (new MenuItem ("Dump ROM", DumpRom, Shortcut.CtrlShiftD));

			// Views menu
			var menu_views = new MenuItem ("Views");
			menu_views.MenuItems.Add (new MenuItem ("Registers", ShowRegisters, Shortcut.CtrlShiftR));

			menu.MenuItems.AddRange (new [] { menu_program, menu_emulator, menu_debug, menu_views });
			this.Menu = menu;
			#endregion

			lblregs = new Label {
				AutoSize = false,
				Dock = DockStyle.Right,
				Width = 298,
				Font = new Font (FontFamily.GenericMonospace, 11.25f)
			};

			this.ClientSize = new Size (640, 320);
		}

		void LoadProgram (object sender, EventArgs e) {
			var dialog = new OpenFileDialog {
				Multiselect = false,
				ShowReadOnly = true,
				DefaultExt = "ch8",
				AutoUpgradeEnabled = true,
				CheckFileExists = true,
				CheckPathExists = true,
				DereferenceLinks = true,
				Filter = "CHIP8 Program|*.ch8;*.bin|All file types|*.*"
			};
			var result = dialog.ShowDialog ();
			if (result == DialogResult.OK) {
				Debugger.Instance.Stop ();
				Emulator.Instance.LoadFile (dialog.FileName);
				Debugger.Instance.Run ();
			}
		}

		void DumpRam (object sender, EventArgs e) {
			var path = Path.Combine (Application.StartupPath, "ramdump.bin");
			var ram = Emulator.Instance.DumpRam ();
			using (var fs = new FileStream (path, FileMode.Create, FileAccess.Write, FileShare.None))
			using (var writer = new BinaryWriter (fs)) {
				writer.Write (ram);
			}
		}

		void DumpRom (object sender, EventArgs e) {
			var path = Path.Combine (Application.StartupPath, "romdump.bin");
			var rom = Emulator.Instance.DumpRom ();
			using (var fs = new FileStream (path, FileMode.Create, FileAccess.Write, FileShare.None))
			using (var writer = new BinaryWriter (fs)) {
				writer.Write (rom);
			}
		}

		bool registers_shown;
		Label lblregs;
		void ShowRegisters (object sender, EventArgs e) {
			if (!registers_shown) {
				this.Controls.Add (lblregs);
				UpdateRegisters (update: true);
				this.Width += 300;
				lblregs.Refresh ();
				registers_shown = true;
			} else {
				this.Width -= 300;
				UpdateRegisters (update: false);
				this.Controls.Remove (lblregs);
				registers_shown = false;
			}
		}

		bool update_registers;
		void UpdateRegisters (bool update = true) {
			if (update == false) {
				update_registers = false;
				return;
			}
			Task.Factory.StartNew (() => {
				update_registers = true;
				while (update_registers) {
					Thread.Sleep (50);
					if (lblregs == null)
						continue;
					var snap = Emulator.Instance.Cpu.Snapshot ();
					this.BeginInvoke (new MethodInvoker (() => {
						lblregs.Text = string.Format (
							"V0:    0x{0:X4} V8:    0x{8:X4}\n" +
							"V1:    0x{1:X4} V9:    0x{9:X4}\n" +
							"V2:    0x{2:X4} VA:    0x{10:X4}\n" +
							"V3:    0x{3:X4} VB:    0x{11:X4}\n" +
							"V4:    0x{4:X4} VC:    0x{12:X4}\n" +
							"V5:    0x{5:X4} VD:    0x{13:X4}\n" +
							"V6:    0x{6:X4} VE:    0x{14:X4}\n" +
							"V7:    0x{7:X4} Flag:  0x{15:X4}\n" +
							"Index: 0x{16:X4}\n" +
							"PC:    0x{17:X4}\n" +
							"SP:    0x{18:X4}",
							snap.V0, snap.V1, snap.V2, snap.V3, snap.V4,
							snap.V5, snap.V6, snap.V7, snap.V8, snap.V9,
							snap.VA, snap.VB, snap.VC, snap.VD, snap.VE,
							snap.Carry, snap.I, snap.PC, snap.SP
						);
						lblregs.Refresh ();
					}));
				}
			});
		}
	}
}

